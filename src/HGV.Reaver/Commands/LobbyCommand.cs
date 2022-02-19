using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using HGV.Basilius;
using HGV.Basilius.Client;
using HGV.Reaver.Models;
using HGV.Reaver.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HGV.Reaver.Commands
{
    using Team = SteamKit2.GC.Dota.Internal.DOTA_GC_TEAM;

    public class LobbyTypeChoiceProvider : ChoiceProvider
    {
        public LobbyTypeChoiceProvider()
        {
        }

        public override Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
        {
            var collection = new List<DiscordApplicationCommandOptionChoice>();
            collection.Add(new DiscordApplicationCommandOptionChoice("Default", 0));
            collection.Add(new DiscordApplicationCommandOptionChoice("Custom", 1));
            collection.Add(new DiscordApplicationCommandOptionChoice("Single Draft", 2));
            collection.Add(new DiscordApplicationCommandOptionChoice("All Pick", 3));

            return Task.FromResult(collection.AsEnumerable());
        }
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
        private readonly IHyperstoneService hyperstoneService;
        private readonly IDotaService dota;
        private readonly IMetaClient meta;
        

        public LobbyCommand(IOptions<ReaverSettings> settings, IAccountService accountService, IProfileService profileService, IHyperstoneService hyperstoneService, IDotaService dota, IMetaClient meta)
        {
            this.username = settings.Value?.SteamUsername ?? throw new ArgumentNullException(nameof(ReaverSettings.SteamUsername));
            this.password = settings.Value?.SteamPassword ?? throw new ArgumentNullException(nameof(ReaverSettings.SteamPassword));
            this.accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            this.profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
            this.hyperstoneService = hyperstoneService ?? throw new ArgumentNullException(nameof(hyperstoneService));
            this.dota = dota ?? throw new ArgumentNullException(nameof(dota));
            this.meta = meta ?? throw new ArgumentNullException(nameof(meta));
        }

        [SlashCommand("Create", "Create A Custom Lobby")]
        public async Task Create(InteractionContext ctx,
            [ChoiceProvider(typeof(LobbyTypeChoiceProvider)), Option("Type", "The type of lobby")] long type,
            [ChoiceProvider(typeof(RegionChoiceProvider)), Option("Region", "The region to host to lobby")] long regionId,
            [Option("ShuffleTeams", "Limit players to below this rating.")] bool shuffle_teams = false,
            [Option("ShufflePlayers", "Limit players to below this rating.")] bool shuffle_players = false,
            [Option("RatingCap", "Limit players to below this rating.")] long limit = long.MaxValue
        )
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());

            var generator = Guid.NewGuid().ToString();
            var key = generator.Substring(0, 8);
            var name = $"HGV {key}";
            var password = generator.Substring(9, 4);

            switch (type)
            {
                case 1:
                    await CreateCustomLobby(ctx, name, password, regionId, shuffle_teams, shuffle_players, limit);
                    break;
                case 0:
                    await CreateDefautLobby(ctx, name, password, regionId, shuffle_teams, shuffle_players, limit);
                    break;
                default:
                    throw new UserFriendlyException("Invaild Lobby Type");
            }
        }

        private async Task CreateDefautLobby(InteractionContext ctx, string name, string password, long regionId, bool shuffle_teams, bool shuffle_players, long limit)
        {
            try
            {
                var cap = (limit == long.MaxValue) ? "None" : limit.ToString();
                var region = this.meta.GetRegion((int)regionId);

                var emoji = DiscordEmoji.FromName(ctx.Client, ":dota:", true);

                var content = new StringBuilder();
                content.AppendLine($"Join the HGV bot as it host an inhouse lobby.");
                content.AppendLine($"The Bot is waiting 5 minutes to collect players before trying to creating a lobby.");
                content.AppendLine($"It will invite the players that reacted with {emoji} directly to the lobby via steam.");
                content.AppendLine($"When all 10 slots in the lobby are full the Bot will start the count down.");

                var embedLobby = new DiscordEmbedBuilder()
                    .WithTitle("HGV Blitz Match")
                    .WithDescription(content.ToString())
                    .WithColor(DiscordColor.Purple);

                embedLobby.AddField("Lobby Name", $"{name}", false);
                embedLobby.AddField("Region", $"{region.Name}", false);
                embedLobby.AddField("Rating Cap", cap, false);
                embedLobby.AddField("Shuffle Teams", $"{(shuffle_teams ? "TRUE" : "FALSE")}", false);
                embedLobby.AddField("Shuffle Players", $"{(shuffle_players ? "TRUE" : "FALSE")}", false);
                embedLobby.AddField("Roster", "Random", false);

                var builder = new DiscordWebhookBuilder()
                    .AddEmbed(embedLobby);

                var msg = await ctx.EditResponseAsync(builder);

                await msg.CreateReactionAsync(emoji);

                await Task.Delay(TimeSpan.FromMinutes(5));

                var reactions = await msg.GetReactionsAsync(emoji);

                var users = reactions.DistinctBy(_ => _.Id).Where(_ => _.IsBot == false).ToList();
                if (users.Count() < 10)
                {
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"Not enough users reacted so no lobby will be created."));
                    return;
                }

                var reasons = new StringBuilder();
                var players = new List<ulong>();

                foreach (var user in users)
                {
                    // Find linked Account
                    var link = await accountService.Get(ctx.Guild.Id, user.Id);
                    if (link is null)
                    {
                        reasons.AppendLine($"{user.Mention} your account is not linked. Plese run the '/account link' command.");
                        continue;
                    }

                    // Get Dota profile
                    var profile = await profileService.GetDotaProfile(link.SteamId);
                    if (profile is null)
                    {
                        reasons.AppendLine($"{user.Mention} your account is linked but we can not find your profile.");
                        continue;
                    }

                    // Check rating
                    if (profile.Rating > limit)
                    {
                        reasons.AppendLine($"{user.Mention} your rating of {(int)profile.Rating} is above the limit {limit}");
                        continue;
                    }

                    players.Add(link.SteamId);
                }

                reasons.Insert(0, $"Not enough players ({players.Count()}) meet the requirements so no lobby will be created. {Environment.NewLine}");

                if (players.Count() < 10)
                {
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent(reasons.ToString()));
                    return;
                }

                var heroes = new List<uint>();

                // Create Session.
                var id = await this.dota.CreateSessionAsync(this.username, this.password);

                // Create Lobby.
                await this.dota.CreateLobbyAsync(name, password, (uint)regionId, shuffle_players, heroes);

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
      
        private async Task CreateCustomLobby(InteractionContext ctx, string name, string password, long regionId, bool shuffle_teams, bool shuffle_players, long limit)
        {
            try
            {
                var cap = (limit == long.MaxValue) ? "None" : limit.ToString();
                var region = this.meta.GetRegion((int)regionId);

                var roster = this.meta.GetHeroes().Where(_ => _.AbilityDraftEnabled == true).Shuffle().Take(12).ToList();
                var radiant = roster.Skip(0).Take(5).Select(_ => _.Name).ToList();
                var dire = roster.Skip(5).Take(5).Select(_ => _.Name).ToList();
                var extra = roster.Skip(10).Take(2).Select(_ => _.Name).ToList();

                var thumbnail = await hyperstoneService.StorageRosterImage(roster);

                var emoji = DiscordEmoji.FromName(ctx.Client, ":dota:", true);

                var content = new StringBuilder();
                content.AppendLine($"Join the HGV bot as it host an inhouse lobby.");
                content.AppendLine($"The Bot is waiting 5 minutes to collect players before trying to creating a lobby.");
                content.AppendLine($"It will invite the players that reacted with {emoji} directly to the lobby via steam.");
                content.AppendLine($"When all 10 slots in the lobby are full the Bot will start the count down.");
                
                var embedLobby = new DiscordEmbedBuilder()
                    .WithTitle("HGV Blitz Match")
                    .WithDescription(content.ToString())
                    .WithColor(DiscordColor.SpringGreen);

                embedLobby.AddField("Lobby Name", $"{name}", false);
                embedLobby.AddField("Region", $"{region.Name}", false);
                embedLobby.AddField("Rating Cap", cap, false);
                embedLobby.AddField("Shuffle Teams", $"{(shuffle_teams ? "TRUE" : "FALSE")}", false);
                embedLobby.AddField("Shuffle Players", $"{(shuffle_players ? "TRUE" : "FALSE")}", false);
                embedLobby.AddField("Roster", "Custom", false);
                embedLobby.AddField("Radiant", String.Join(", ", radiant), false);
                embedLobby.AddField("Dire", String.Join(", ", dire), false);
                embedLobby.AddField("Extra", String.Join(", ", extra), false);

                embedLobby.WithImageUrl(thumbnail);

                var builder = new DiscordWebhookBuilder()
                    .AddEmbed(embedLobby);

                var msg = await ctx.EditResponseAsync(builder);

                await msg.CreateReactionAsync(emoji);

                await Task.Delay(TimeSpan.FromMinutes(5));
                
                var reactions = await msg.GetReactionsAsync(emoji);

                var users = reactions.DistinctBy(_ => _.Id).Where(_ => _.IsBot == false).ToList();
                if (users.Count() < 10)
                {
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"Not enough users reacted so no lobby will be created."));
                    return;
                }

                var reasons = new StringBuilder();
                var players = new List<ulong>();

                foreach (var user in users)
                {
                    // Find linked Account
                    var link = await accountService.Get(ctx.Guild.Id, user.Id);
                    if (link is null)
                    {
                        reasons.AppendLine($"{user.Mention} your account is not linked. Plese run the '/account link' command.");
                        continue;
                    }
                        
                    // Get Dota profile
                    var profile = await profileService.GetDotaProfile(link.SteamId);
                    if (profile is null)
                    {
                        reasons.AppendLine($"{user.Mention} your account is linked but we can not find your profile.");
                        continue;
                    }

                    // Check rating
                    if (profile.Rating > limit)
                    {
                        reasons.AppendLine($"{user.Mention} your rating of {(int)profile.Rating} is above the limit {limit}");
                        continue;
                    }
                    
                    players.Add(link.SteamId);
                }

                reasons.Insert(0, $"Not enough players ({players.Count()}) meet the requirements so no lobby will be created. {Environment.NewLine}");

                if (players.Count() < 10)
                {
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent(reasons.ToString()));
                    return;
                }

                var heroes = roster.Select(_ => (uint)_.Id).ToList();

                // Create Session.
                var id = await this.dota.CreateSessionAsync(this.username, this.password);

                // Create Lobby.
                await this.dota.CreateLobbyAsync(name, password, (uint)regionId, shuffle_players, heroes);

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
