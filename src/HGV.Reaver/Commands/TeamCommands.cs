using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using HGV.Reaver.Data;
using HGV.Reaver.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace HGV.Reaver.Commands
{

    [SlashCommandGroup("Teams", "Teams Commands", false)]
    public class TeamCommands : ApplicationCommandModule
    {
        private const string DEFAULT_IMAGE_URL = "https://steamuserimages-a.akamaihd.net/ugc/868480752636433334/1D2881C5C9B3AD28A1D8852903A8F9E1FF45C2C8/";
        private const string DEFAULT_IMAGE_WARD = "https://hyperstone.highgroundvision.com/images/wards/observer.png";
        
        private readonly IAccountService accountService;
        private readonly IProfileService profileService;

        public TeamCommands(IAccountService accountService, IProfileService profileService)
        {
            this.accountService = accountService;
            this.profileService = profileService;
        }


        [SlashCommand("Promote", "Promote your team")]
        public async Task Promote(InteractionContext ctx,
            [Option("Name", "The name of the team")] string name,
            [Choice("Americas", "Americas"), Choice("Europe", "Europe"), Option("Region", "The region team will be playing in.")] string region,
            [Choice("One", 1), Choice("Two", 2), Choice("Three", 3), Option("Players", "The number of players your looking for")] long players,
            [Option("Image", "Url of the team logo")] string? thumbnail = null
        )
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());

            var link = await this.accountService.Get(ctx.Guild.Id, ctx.Member.Id);
            var profile = await this.profileService.GetDotaProfile(link.SteamId);
            if (profile is null)
                throw new NullReferenceException("TeamCommands::Promote::GetDotaProfile");

            var rating = profile.Rating is null ? "N/A" : profile.Rating.Value.ToString("F0");

            var embed = new DiscordEmbedBuilder();
            embed.WithColor(DiscordColor.Purple);
            embed.WithThumbnail(thumbnail ?? DEFAULT_IMAGE_URL);
            embed.WithTitle($"{name} is Recuiting");
            embed.WithDescription($"This team is looking for a few players");

            embed.AddField("TEAM", $"{name}", true);
            embed.AddField("CAPTAIN", $"{ctx.Member.DisplayName}", true);
            embed.AddField("RATING", $"{rating}", true);

            embed.AddField("PLAYERS", $"{players}", true);
            embed.AddField("REGION", region, true);
            embed.WithFooter($"{ctx.Member.Id}");

            var button1 = new DiscordButtonComponent(ButtonStyle.Primary, "3b8a4245-a436-4a69-9b5d-5ff4d198fe20", "Request to Join");
            var button2 = new DiscordButtonComponent(ButtonStyle.Danger, "51290b36-5292-4751-8b82-bbe111c15df8", string.Empty, false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":heavy_multiplication_x:")));

            var builder = new DiscordWebhookBuilder();
            builder.AddEmbed(embed);
            builder.AddComponents(button1, button2);

            await ctx.EditResponseAsync(builder);
        }


        //[SlashCommand("Create", "Answer some questions to create a team", false)]
        //public async Task Create(InteractionContext ctx)
        //{
        //    await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());

        //    // Send dm to memeber that requested the command
        //    // Ask questions about the team:
        //    // 1. Name
        //    // 2. Description
        //    // 3. Logo

        //    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Not Implemented."));
        //}

        //[SlashCommand("Import", "Import a team from an existing sytem", false)]
        //public async Task Import(InteractionContext ctx,
        //    [Choice("Dota", 1), Choice("FaceIt", 2), Option("System", "The system to import the team details from.")] long type,
        //    [Option("Team", "Enter the team Id")] string id
        //)
        //{
        //    await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());

        //    var link = await this.accountService.Get(ctx.Guild.Id, ctx.Member.Id);

        //    switch (type)
        //    {
        //        case 0:
        //            {
        //                // Import from API
        //                // https://api.steampowered.com/IDOTA2Match_570/GetTeamInfoByTeamID/v0001/?key={key}&start_at_team_id={id}
        //                // Check that admin_account_id equals the members linked account
        //                // Get Teams Logo
        //                // https://api.steampowered.com/ISteamRemoteStorage/GetUGCFileDetails/v1/?key={key}&appid=570&ugcid={logo}
        //                // data.url => https://steamusercontent-a.akamaihd.net/ugc/1796368211082425833/B81B4ED44CBB957E81100CAEF41ED75E26652ACA/
        //                break;
        //            }
        //        case 1:
        //            {
        //                var team = await championshipsService.GetTeam(id);
        //                if (team is null)
        //                    throw new UserFriendlyException("No FaceIt team found with that Id");

        //                var leaderId = team.Leader ?? string.Empty;
        //                var leader = await championshipsService.GetPlayer(leaderId);
        //                if (leader?.GetSteamId() != link?.GetSteamId())
        //                    throw new UserFriendlyException($"Only the team leader can input the team '{team.Name}' from FaceIt.");

        //                // team.Avatar
        //                // team.Name
        //                // team.Description    

        //                break;
        //            }
        //        default:
        //            break;
        //    }

        //    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Not Implemented."));
        //}

        //[SlashCommand("Add", "Add a member to a team")]
        //public async Task Add(InteractionContext ctx)
        //{
        //    await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());

        //    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Error Updating Team Card"));
        //}

        //[SlashCommand("Remove", "Remove a member from a team")]
        //public async Task Remove(InteractionContext ctx)
        //{
        //    await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());

        //    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Error Updating Team Card"));
        //}

        //[SlashCommand("Delete", "Removes the team")]
        //public async Task Delete(InteractionContext ctx)
        //{
        //    await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());

        //    // Find team by Guild/Admin 
        //    // 

        //    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Error Deleting the Team Card"));
        //}
    }
}
