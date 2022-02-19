using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using HGV.Reaver.Models;
using HGV.Reaver.Services;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using HGV.Basilius.Client;
using Microsoft.Extensions.DependencyInjection;
using DSharpPlus.Interactivity.Extensions;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using HGV.Basilius;

namespace HGV.Reaver.Commands
{
    using Team = SteamKit2.GC.Dota.Internal.DOTA_GC_TEAM;

    public class Option
    {
        public DiscordEmoji Emoji { get; set; }
        public IEnumerable<Hero> Roster { get; set; }
        public IReadOnlyList<DiscordUser> Users { get; set; }
        public IEnumerable<ulong> Players { get; set; }

        public Option(DiscordEmoji Emoji)
        {
            this.Emoji = Emoji;
            this.Roster = new List<Hero>();
            this.Users = new List<DiscordUser>();
            this.Players = new List<ulong>();
        }

        public List<uint> Identities => Roster.Select(_ => (uint)_.Id).ToList();
        public IEnumerable<string> Heroes => Roster.Select(_ => _.Name);
    }

    public class RegionChoiceProvider : ChoiceProvider
    {
        public RegionChoiceProvider()
        {
        }

        public override Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
        {
            var denyList = new List<string>() { "Unknown", "China" };

            var client = this.Services.GetService<IMetaClient>() ?? throw new NullReferenceException(nameof(IMetaClient));
            var regions = client.GetRegions().Where(_ => denyList.Contains(_.Name) == false).ToList();

            var collection = new List<DiscordApplicationCommandOptionChoice>();
            foreach (var region in regions)
            {
                collection.Add(new DiscordApplicationCommandOptionChoice(region.Name, region.Id));
            }

            return Task.FromResult(collection.AsEnumerable());
        }
    }

    [SlashCommandGroup("Lobby", "Match Commands", false)]
    public class LobbyCommand : ApplicationCommandModule
    {
        private readonly string username;
        private readonly string password;

        private readonly IAccountService accountService;
        private readonly IProfileService profileService;
        private readonly IDotaService dota;
        private readonly IMetaClient meta;

        public LobbyCommand(IOptions<ReaverSettings> settings, IAccountService accountService, IProfileService profileService, IDotaService dota, IMetaClient meta)
        {
            this.username = settings.Value?.SteamUsername ?? throw new ArgumentNullException(nameof(ReaverSettings.SteamUsername));
            this.password = settings.Value?.SteamPassword ?? throw new ArgumentNullException(nameof(ReaverSettings.SteamPassword));
            this.accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            this.profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
            this.dota = dota ?? throw new ArgumentNullException(nameof(dota));
            this.meta = meta ?? throw new ArgumentNullException(nameof(meta));
        }

        [SlashCommand("Create", "Create A Custom Lobby")]
        public async Task Create(InteractionContext ctx,
            [ChoiceProvider(typeof(RegionChoiceProvider)), Option("Region", "The region to host to lobby")] long region,
            [Option("ShuffleTeams", "Shuffle players to balance the teams.")] bool shuffle_teams = false,
            [Option("ShufflePlayers", "Shuffle slots randomize players pick order.")] bool shuffle_players = false,
            [Option("RatingCap", "Limit players to below this rating.")] long limit = long.MaxValue
        )
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());

            try
            {
                var generator = Guid.NewGuid().ToString();
                var key = generator.Substring(0, 8);
                var password = generator.Substring(9, 4);
                var name = $"HGV {key}";

                var options = new List<Option>()
                {
                    new Option(DiscordEmoji.FromName(ctx.Client, ":one:"))      { Roster = this.meta.GetHeroes().Shuffle().Take(12).ToList() },
                    new Option(DiscordEmoji.FromName(ctx.Client, ":two:"))      { Roster = this.meta.GetHeroes().Shuffle().Take(12).ToList() },
                    new Option(DiscordEmoji.FromName(ctx.Client, ":three:"))    { Roster = this.meta.GetHeroes().Shuffle().Take(12).ToList() },
                };

                var content = new StringBuilder();
                content.AppendLine($"Join the HGV bot as it host an inhouse lobby.");
                content.AppendLine($"The Bot is waiting 5 minutes to collect players before trying to creating a lobby.");
                content.AppendLine($"The Bot will invite the players tha reacted directly to the lobby via steamm.");
                content.AppendLine($"When all 10 slots are full the Bot will start the count down.");
                content.AppendLine($"Your accounts must be linked for this to work.");
                content.AppendLine($"Each options is a different roster the Bot can set.");

                for (int i = 0; i < 3; i++)
                {
                    content.AppendLine($"{options[i].Emoji} {string.Join(", ", options[i].Heroes)}");
                }

                var embed = new DiscordEmbedBuilder()
                       .WithTitle("HGV In House Lobby")
                       .WithDescription(content.ToString())
                       .WithColor(DiscordColor.Purple);

                embed.AddField("Lobby Name", $"{name}", false);
                //embed.AddField("Lobby Passkey", $"{password}", false);
                embed.AddField("Shuffle Teams", $"{(shuffle_teams ? "TRUE" : "FALSE")}", false);
                embed.AddField("Shuffle Players", $"{(shuffle_players ? "TRUE" : "FALSE")}", false);

                if(limit == long.MaxValue)
                    embed.AddField("Rating Cap", $"N/A", false);
                else
                    embed.AddField("Rating Cap", $"{limit}", false);

                var builder = new DiscordWebhookBuilder()
                    .WithContent(content.ToString())
                    .AddEmbed(embed);

                var msg = await ctx.EditResponseAsync(builder);

                for (int i = 0; i < 3; i++)
                {
                    await msg.CreateReactionAsync(options[i].Emoji);
                }

                await Task.Delay(TimeSpan.FromMinutes(5));

                for (int i = 0; i < 3; i++)
                {
                    options[i].Users = await msg.GetReactionsAsync(options[i].Emoji, 25, ctx.Client.CurrentUser.Id);
                }

                var users = options.SelectMany(_ => _.Users).DistinctBy(_ => _.Id).ToList();
                if (users.Count() < 10)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Not enough users reacted so no lobby will be created."));
                    return;
                }

                var players = new List<ulong>();
                foreach (var user in users)
                {
                    var link = await accountService.Get(ctx.Guild.Id, user.Id); // Find linked Account
                    if (link is null)
                        continue;

                    var profile = await profileService.GetDotaProfile(link.SteamId);
                    if (profile is null)
                        continue;

                    if (profile.Rating > limit)
                        continue;
                        
                    players.Add(link.SteamId);
                }

                if(players.Count() < 10)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Not enough players ({players.Count()}) meet the requirements so no lobby will be created."));
                    return;
                }

                var heroes = options.OrderByDescending(_ => _.Users.Count()).Select(_ => _.Identities).FirstOrDefault();

                // Create Session.
                var id = await this.dota.CreateSessionAsync(this.username, this.password);

                // Create Lobby.
                await this.dota.CreateLobbyAsync(name, password, (uint)region, shuffle_players, heroes);

                // Get Lobby.
                var _lobby = this.dota.GetActiveLobby();

                // Kick Bot From Team
                await dota.KickPlayerFromLobbyTeam(id.AccountID);

                foreach (var player in players)
                {
                    // Invite to lobby
                    await dota.InviteToLobby(player);
                }

                var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));

                var ready = 0;
                do
                {
                    cts.Token.ThrowIfCancellationRequested();

                    // Wait 10 seconds
                    await Task.Delay(TimeSpan.FromSeconds(10), cts.Token);

                    var lobby = this.dota.GetActiveLobby();

                    ready = lobby.all_members.Count(_ => _.team == Team.DOTA_GC_TEAM_GOOD_GUYS || _.team == Team.DOTA_GC_TEAM_BAD_GUYS);
                } 
                while (ready < 10);

                // Shuffle Teams if requested
                if (shuffle_teams)
                {
                    await dota.ShuffleTeams();
                }

                // Join and Get the Lobby Chat Channel.
                var channel = await dota.JoinLobbyChatAsync();

                // message the lobby with Lunch Warning.
                await dota.SendChatMessage(channel.channel_id, $"T minus 5 seconds and counting");

                // Countdown
                for (int i = 5; i >= 0; i--)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    await dota.SendChatMessage(channel.channel_id, $"{i}");
                }

                // Launch Game
                await dota.LaunchGameAsync();
            }
            catch (OperationCanceledException)
            {
                await dota.DestroyLobbyAsync();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                this.dota.StopSession();
            }

        }
    }
}
