using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using HGV.Reaver.Commands;
using HGV.Reaver.Models;
using HGV.Reaver.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using HGV.Reaver.Factories;
using DSharpPlus.SlashCommands.EventArgs;

namespace HGV.Reaver.Hosts
{
    internal class DiscordLifetimeHost : BackgroundService
    {
        private DiscordClient client;
        private SlashCommandsExtension commands;
        private readonly IServiceProvider services;

        public DiscordLifetimeHost(IServiceProvider sp)
        {
            this.services = sp;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var factory = this.services.GetService<IDiscordClientFactory>();

            this.client = factory.Client;
            this.client.Ready += this.OnReady;
            this.client.GuildAvailable += this.OnGuildAvailable;
            this.client.ClientErrored += this.OnClientError;
            this.client.MessageReactionAdded += OnMessageReactionAdded;
            this.client.MessageReactionRemoved += OnMessageReactionRemoved;

            var slashConfiguration = new SlashCommandsConfiguration
            {
                Services = services
            };
            commands = this.client.UseSlashCommands(slashConfiguration);
            commands.SlashCommandExecuted += this.OnSlashCommandExecuted;
            commands.SlashCommandErrored += this.OnSlashCommandErrored;
            commands.ContextMenuExecuted += OnContextMenuExecuted;
            commands.ContextMenuErrored += OnContextMenuErrored;

            commands.RegisterCommands<AdminCommands>();
            commands.RegisterCommands<SelfServiceCommand>();
            commands.RegisterCommands<TeamCommands>();
            //commands.RegisterCommands<LeagueCommands>();
            commands.RegisterCommands<AccountCommands>();
            commands.RegisterCommands<ProfileCommands>();
            commands.RegisterCommands<MatchCommands>();
            commands.RegisterCommands<AbilityCommands>();
            commands.RegisterCommands<ProfileContextMenu>();

            var interactivityConfiguration = new InteractivityConfiguration()
            {
                PollBehaviour = PollBehaviour.KeepEmojis,
                Timeout = TimeSpan.FromSeconds(60),
            };
            this.client.UseInteractivity(interactivityConfiguration);

            await this.client.ConnectAsync(status: UserStatus.Online);

            await Task.Delay(Timeout.Infinite, cancellationToken);

            this.client.Logger.LogInformation("Disconnecting Client");

            await this.client.UpdateStatusAsync(userStatus: UserStatus.Offline);
            await this.client.DisconnectAsync();
        }
        
        private Task OnReady(DiscordClient sender, ReadyEventArgs e)
        {
            // let's log the fact that this event occured
            sender.Logger.LogInformation("Client is ready to process events.");
            return Task.CompletedTask;
        }

        private Task OnGuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
        {
            // let's log the name of the guild that was just sent to our client
            sender.Logger.LogInformation($"Guild available: {e.Guild.Name}");
            return Task.CompletedTask;
        }

        private Task OnClientError(DiscordClient sender, ClientErrorEventArgs e)
        {
            // let's log the details of the error that just  occured in our client
            sender.Logger.LogError(e.Exception, "Exception occured");
            return Task.CompletedTask;
        }

        private Task OnSlashCommandExecuted(SlashCommandsExtension sender, DSharpPlus.SlashCommands.EventArgs.SlashCommandExecutedEventArgs e)
        {
            // let's log the name of the command and user
            e.Context.Client.Logger.LogInformation($"{e.Context.User.Username} executed '{e.Context.CommandName}'");
            return Task.CompletedTask;
        }

        private Task OnContextMenuExecuted(SlashCommandsExtension sender, DSharpPlus.SlashCommands.EventArgs.ContextMenuExecutedEventArgs e)
        {
            // let's log the name of the command and user
            e.Context.Client.Logger.LogInformation($"{e.Context.User.Username} executed '{e.Context.CommandName}'");
            return Task.CompletedTask;
        }

        private async Task OnMessageReactionAdded(DiscordClient sender, MessageReactionAddEventArgs e)
        {
            try
            {
                var service = this.services.GetService<IRoleLinkService>();
                if (service is null)
                    throw new Exception("Missing an implementation of the IRoleLinkService");

                var link = await service.Get(e.Guild.Id, e.Message.Id, e.Emoji.GetDiscordName());
                if (link is null)
                    return;

                var role = e.Guild.GetRole(link.RoleId);
                if (role is null)
                    return;

                var user = await e.Guild.GetMemberAsync(e.User.Id);
                if (user is null)
                    return;

                await user.GrantRoleAsync(role);

                sender.Logger.LogInformation($"{e.User.Username} added a role {role.Name}");
            }
            catch(Exception ex)
            {
                sender.Logger.LogError(ex, $"{e.User.Username} had and error with self service {e.Message.Id} in {e.Channel.Name} by {e.Emoji.GetDiscordName()}");
            }
            finally
            {
                e.Handled = true;
            }
        }

        private async Task OnMessageReactionRemoved(DiscordClient sender, MessageReactionRemoveEventArgs e)
        {
            try
            {
                var service = this.services.GetService<IRoleLinkService>();
                if (service is null)
                    throw new Exception("Missing an implementation of the IRoleLinkService");

                var link = await service.Get(e.Guild.Id, e.Message.Id, e.Emoji.GetDiscordName());
                if (link is null)
                    return;

                var role = e.Guild.GetRole(link.RoleId);
                if (role is null)
                    return;

                var user = await e.Guild.GetMemberAsync(e.User.Id);
                if (user is null)
                    return;

                await user.RevokeRoleAsync(role);

                sender.Logger.LogInformation($"{e.User.Username} removed a role {role.Name}");
            }
            catch (Exception ex)
            {
                sender.Logger.LogError(ex, $"{e.User.Username} had and error with self service {e.Message.Id} in {e.Channel.Name} by {e.Emoji.GetDiscordName()}");
            }
            finally
            {
                e.Handled = true;
            }
        }


        private async Task OnSlashCommandErrored(SlashCommandsExtension sender, DSharpPlus.SlashCommands.EventArgs.SlashCommandErrorEventArgs e)
        {
            e.LogError();

            var no_entry = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");
            var stop_sign = DiscordEmoji.FromName(e.Context.Client, ":stop_sign:");
            var warning = DiscordEmoji.FromName(e.Context.Client, ":warning:");
            var bug = DiscordEmoji.FromName(e.Context.Client, ":bug:");

            var buidler = new DiscordWebhookBuilder();
            var embed = new DiscordEmbedBuilder() { Color = DiscordColor.Red };

            if (e.Exception is SlashExecutionChecksFailedException ex)
            {
                embed.WithTitle($"{no_entry} Access denied").WithDescription($"You do not have the permissions required to execute this command.");
            }
            else if (e.Exception is AccountNotLinkedException)
            {
                embed.WithTitle($"{stop_sign} Accounts Not Linked").WithDescription($"You do not have an account linked. Plese run the '/account link' command.");
            }
            else if (e.Exception is UserFriendlyException)
            {
                embed.WithTitle($"Warning").WithDescription($"{warning} Issue: {e.Exception.Message}");
            }
            else
            {
                embed.WithTitle($"Error").WithDescription($"{bug} Yes that is a bug an we have logged the issue. Some errors happen and you can try again but if your repeatedly doing same action expceting a diferent result welcome to insanity.");
            }

            await e.Context.EditResponseAsync(buidler.AddEmbed(embed));

            e.Handled = true;
        }

        

        private async Task OnContextMenuErrored(SlashCommandsExtension sender, DSharpPlus.SlashCommands.EventArgs.ContextMenuErrorEventArgs e)
        {
            e.LogError();

            var no_entry = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");
            var stop_sign = DiscordEmoji.FromName(e.Context.Client, ":stop_sign:");
            var warning = DiscordEmoji.FromName(e.Context.Client, ":warning:");
            var bug = DiscordEmoji.FromName(e.Context.Client, ":bug:");

            var buidler = new DiscordWebhookBuilder();
            var embed = new DiscordEmbedBuilder() { Color = DiscordColor.Red };

            if (e.Exception is ContextMenuExecutionChecksFailedException ex)
            {
                embed.WithTitle($"{no_entry} Access denied").WithDescription($"You do not have the permissions required to execute this command.");
            }
            else if (e.Exception is AccountNotLinkedException)
            {
                embed.WithTitle($"{stop_sign} Accounts Not Linked").WithDescription($"You do not have an account linked. Plese run the '/account link' command.");
            }
            else if (e.Exception is UserFriendlyException)
            {
                embed.WithTitle($"Warning").WithDescription($"{warning} Issue: {e.Exception.Message}");
            }
            else
            {
                embed.WithTitle($"Error").WithDescription($"{bug} Yes that is a bug.");
            }

            await e.Context.EditResponseAsync(buidler.AddEmbed(embed));

            e.Handled = true;
        }
     
    }
}
