using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HGV.Reaver.Bot.Commands
{
    public class BotCommands : ApplicationCommandModule
    {
        [SlashCommand("quiz", "Interactive Quiz")]
        public async Task TestCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            await ctx.Member.CreateDmChannelAsync();

            var answers = new Dictionary<string, string>();
            await AskQuestion(ctx, "Question #1", answers);
            await AskQuestion(ctx, "Question #2", answers);
            await AskQuestion(ctx, "Question #3", answers);

            var embed = new DiscordEmbedBuilder()
            {
                Title = "Answers",
                Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
            };

            foreach (var item in answers)
            {
                embed.AddField(item.Key, item.Value);
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
        }

        private static async Task AskQuestion(InteractionContext ctx, string question, Dictionary<string,string> answers)
        {
            var msg = await ctx.Member.SendMessageAsync(question);

            var interactivity = ctx.Client.GetInteractivity();
            var reponse = await interactivity.WaitForMessageAsync(i => i.ChannelId == msg.ChannelId, TimeSpan.FromSeconds(10));
            if (reponse.TimedOut)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Timed out gets an answer for '{question}'"));
            }
            else
            {
                answers.Add(question, reponse.Result.Content);
            }
        }
    }
}
