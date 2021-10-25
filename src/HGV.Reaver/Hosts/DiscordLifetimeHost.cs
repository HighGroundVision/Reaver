using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using HGV.Reaver.Commands;
using HGV.Reaver.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HGV.Reaver.Hosts
{
    internal class DiscordLifetimeHost : BackgroundService
    {
        private DiscordClient _client;
        private SlashCommandsExtension _slash;
        private IServiceProvider _services;

        public DiscordLifetimeHost(IServiceProvider sp, DiscordClient client)
        {
            _services = sp;
            _client = client;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            //var clientConfiguration = new DiscordConfiguration
            //{
            //    Token = _token,
            //    TokenType = TokenType.Bot,
            //    AutoReconnect = true,
            //    MinimumLogLevel = LogLevel.Debug,
            //};
            //_client = new DiscordClient(clientConfiguration);
            _client.Ready += this.OnReady;
            _client.GuildAvailable += this.OnGuildAvailable;
            _client.ClientErrored += this.OnClientError;

            var slashConfiguration = new SlashCommandsConfiguration
            {
                Services = _services
            };
            _slash = _client.UseSlashCommands(slashConfiguration);
            _slash.SlashCommandExecuted += this.OnSlashCommandExecuted;
            _slash.SlashCommandErrored += this.OnSlashCommandErrored;
            _slash.ContextMenuExecuted += OnContextMenuExecuted;
            _slash.ContextMenuErrored += OnContextMenuErrored;

            // 319171565818478605
            _slash.RegisterCommands<AdminCommands>();
            _slash.RegisterCommands<SelfService>();
            _slash.RegisterCommands<TeamCommands>();
            _slash.RegisterCommands<LeagueCommands>();
            _slash.RegisterCommands<AccountCommands>();
            _slash.RegisterCommands<ProfileCommands>();
            _slash.RegisterCommands<ImageCommands>();
            _slash.RegisterCommands<AbilityCommands>();
            _slash.RegisterCommands<ProfileContextMenu>();

            var interactivityConfiguration = new InteractivityConfiguration()
            {
                PollBehaviour = PollBehaviour.KeepEmojis,
                Timeout = TimeSpan.FromSeconds(60),
            };
            _client.UseInteractivity(interactivityConfiguration);

            await _client.ConnectAsync(status: UserStatus.Online);

            await Task.Delay(Timeout.Infinite, cancellationToken);

            _client.Logger.LogInformation("Disconnecting Client");

            await _client.UpdateStatusAsync(userStatus: UserStatus.Offline);
            await _client.DisconnectAsync();
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

        private async Task OnSlashCommandErrored(SlashCommandsExtension sender, DSharpPlus.SlashCommands.EventArgs.SlashCommandErrorEventArgs e)
        {
            try
            {
                // let's log the error details
                e.Context.Client.Logger.LogError($"{e.Context.User.Username} tried executing '{e?.Context?.CommandName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);

                if (e.Exception is AccountNotLinkedException)
                {
                    var emoji = DiscordEmoji.FromName(e.Context.Client, ":stop_sign:");
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = "Accounts Not Linked",
                        Description = $"{emoji} You do not have an account linked. Plese run the '/account link' command.",
                        Color = new DiscordColor(0xFF0000) // red
                    };

                    await e.Context.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                }
                else if (e.Exception is SlashExecutionChecksFailedException)
                {
                    var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = "Access denied",
                        Description = $"{emoji} You do not have the permissions required to execute this command.",
                        Color = new DiscordColor(0xFF0000) // red
                    };

                    await e.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
                }
                else if (e.Exception is UserFriendlyException)
                {
                    var msg = e.Exception.Message;
                    var emoji = DiscordEmoji.FromName(e.Context.Client, ":warning:");
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = "Error",
                        Description = $"{emoji} {msg}",
                        Color = new DiscordColor(0xFF0000) // red
                    };

                    await e.Context.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                }
                else
                {
                    var emoji = DiscordEmoji.FromName(e.Context.Client, ":warning:");
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = "Error",
                        Description = $"{emoji} Uh-Oh something happened we did not count for. We have logged the error but you probly let a admin know too.",
                        Color = new DiscordColor(0xFF0000) // red
                    };

                    await e.Context.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                }
            }
            catch (Exception ex)
            {
                e.Context.Client.Logger.LogError(ex, $"Failed to handle Error for SlashCommand: '{e?.Context?.CommandName ?? "<unknown command>"}'");
            }
        }

 
        private async Task OnContextMenuErrored(SlashCommandsExtension sender, DSharpPlus.SlashCommands.EventArgs.ContextMenuErrorEventArgs e)
        {
            try
            {
                // let's log the error details
                e.Context.Client.Logger.LogError($"{e.Context.User.Username} tried executing '{e?.Context?.CommandName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);

                if (e.Exception is AccountNotLinkedException)
                {
                    var emoji = DiscordEmoji.FromName(e.Context.Client, ":stop_sign:");
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = "Accounts Not Linked",
                        Description = $"{emoji} the selected user dose not have a steam account linked.",
                        Color = new DiscordColor(0xFF0000) // red
                    };

                    await e.Context.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                }
                else if (e.Exception is ContextMenuExecutionChecksFailedException)
                {
                    var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = "Access denied",
                        Description = $"{emoji} You do not have the permissions required to execute this command.",
                        Color = new DiscordColor(0xFF0000) // red
                    };

                    await e.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
                }
                else if (e.Exception is UserFriendlyException)
                {
                    var msg = e.Exception.Message;
                    var emoji = DiscordEmoji.FromName(e.Context.Client, ":warning:");
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = "Error",
                        Description = $"{emoji} {msg}",
                        Color = new DiscordColor(0xFF0000) // red
                    };

                    await e.Context.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                }
                else
                {
                    var emoji = DiscordEmoji.FromName(e.Context.Client, ":warning:");
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = "Error",
                        Description = $"{emoji} Uh-Oh something happened we did not count for. We have logged the error but you probly let a admin know too.",
                        Color = new DiscordColor(0xFF0000) // red
                    };

                    await e.Context.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                }
            }
            catch (Exception ex)
            {
                e.Context.Client.Logger.LogError(ex, $"Failed to handle Error for SlashCommand: '{e?.Context?.CommandName ?? "<unknown command>"}'");
            }
        }
    }
}
