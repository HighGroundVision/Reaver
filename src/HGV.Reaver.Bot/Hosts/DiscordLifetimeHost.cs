using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using HGV.Reaver.Bot.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HGV.Reaver.Bot.Hosts
{
    internal class DiscordLifetimeHost : BackgroundService
    {
        private readonly string _token;
        private DiscordClient _client;
        private SlashCommandsExtension _slash { get; set; }
        private IServiceProvider _services { get; set; }

        public DiscordLifetimeHost(IConfiguration configuration, IServiceProvider sp)
        {
            _token = configuration["DiscordToken"].ToString();
            _services = sp;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var clientConfiguration = new DiscordConfiguration
            {
                Token = _token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug,
            };
            _client = new DiscordClient(clientConfiguration);
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

#if DEBUG
            // To register them for a single server, recommended for testing; Remove this later...
            _slash.RegisterCommands<BotCommands>(319171565818478605);
            _slash.RegisterCommands<ProfileCommand>(319171565818478605);
#else
            // To register them globally, once you're confident that they're ready to be used by everyone
            //_slash.RegisterCommands<BotCommands>();
            //_slash.RegisterCommands<ProfileCommand>();
#endif

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

        private async Task OnGuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
        {
            // let's log the name of the guild that was just sent to our client
            sender.Logger.LogInformation($"Guild available: {e.Guild.Name}");

            
            // TODO: If needed can update the permissions of slash commands to control who can run them...
            //var commands = await e.Guild.GetApplicationCommandsAsync();
            //var cmd = commands.FirstOrDefault(_ => _.Name == "AD Profile");
            //var permissions = new List<DiscordApplicationCommandPermission>();
            //await e.Guild.EditApplicationCommandPermissionsAsync(cmd, permissions);

            // var permissions = new List<DiscordApplicationCommandPermission>();
            // permissions.Add(new DiscordApplicationCommandPermission(e.Guild.Owner, true));
            // await e.Guild.EditApplicationCommandPermissionsAsync(cmd, permissions);


            // return Task.CompletedTask;
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

        private async Task OnSlashCommandErrored(SlashCommandsExtension sender, DSharpPlus.SlashCommands.EventArgs.SlashCommandErrorEventArgs e)
        {
            // let's log the error details
            e.Context.Client.Logger.LogError($"{e.Context.User.Username} tried executing '{e?.Context?.CommandName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);

            if (e.Exception is SlashExecutionChecksFailedException)
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
        }

        private Task OnContextMenuExecuted(SlashCommandsExtension sender, DSharpPlus.SlashCommands.EventArgs.ContextMenuExecutedEventArgs e)
        {
            // let's log the name of the command and user
            e.Context.Client.Logger.LogInformation($"{e.Context.User.Username} executed '{e.Context.CommandName}'");

            return Task.CompletedTask;
        }

        private async Task OnContextMenuErrored(SlashCommandsExtension sender, DSharpPlus.SlashCommands.EventArgs.ContextMenuErrorEventArgs e)
        {
            // let's log the error details
            e.Context.Client.Logger.LogError($"{e.Context.User.Username} tried executing '{e?.Context?.CommandName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);

            if (e.Exception is ContextMenuExecutionChecksFailedException)
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
        }

        
    }
}
