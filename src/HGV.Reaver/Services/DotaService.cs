using SteamKit2;
using HGV.Crystalys;
using System;
using SteamKit2.GC.Dota.Internal;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace HGV.Reaver.Services
{

    public interface IDotaService
    {
        public bool ExistingLobby { get; }

        CSODOTALobby GetActiveLobby();
        Task<SteamID> CreateSessionAsync(string username, string password, CancellationToken token = default(CancellationToken));
        void StopSession();
        Task CreateLobbyAsync(string name, string password, uint region, bool shuffle_players, List<uint> requested_heroes);
        Task<CMsgDOTAJoinChatChannelResponse> JoinLobbyChatAsync();
        Task DestroyLobbyAsync();
        Task LaunchGameAsync();
        Task ShuffleTeams();
        Task SendChatMessage(ulong id, string msg);
        Task InviteToLobby(ulong steamId);
        Task KickPlayerFromLobbyTeam(uint accountId);
    }

    public class DotaService : IDotaService
    {
        private readonly SteamClient client;
        private readonly SteamUser user;
        private readonly SteamFriends friends;
        private readonly DotaGameCoordinatorHandler dota;
        private readonly CallbackManager callbacks;
        private readonly CancellationTokenSource callbackCts;
        private readonly Task callbackWatcher;

        public bool ExistingLobby => this.dota.Lobby != null;

        public DotaService()
        {
            this.client = new SteamClient();
            this.client.AddHandler(new DotaGameCoordinatorHandler(client));
            this.user = client.GetHandler<SteamUser>() ?? throw new ArgumentNullException(nameof(SteamUser));
            this.friends = client.GetHandler<SteamFriends>() ?? throw new ArgumentNullException(nameof(SteamFriends));
            this.dota = client.GetHandler<DotaGameCoordinatorHandler>() ?? throw new ArgumentNullException(nameof(DotaGameCoordinatorHandler));

            this.callbacks = new CallbackManager(this.client);
            this.callbacks.Subscribe<SteamUser.LoggedOffCallback>((_) =>
            {
                friends.SetPersonaState(EPersonaState.Offline);
                client.Disconnect();
            });

            this.callbackCts = new CancellationTokenSource();
            this.callbackWatcher = Task.Run(() => 
            {
                while (this.callbackCts.IsCancellationRequested == false)
                    callbacks.RunWaitAllCallbacks(TimeSpan.FromSeconds(10));
            });
        }

        private Task ConnectToSteamAsync(CancellationToken token)
        {
            var tcs = new TaskCompletionSource<SteamClient.ConnectedCallback>();
            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            cts.CancelAfter(TimeSpan.FromSeconds(30));
            cts.Token.Register(() => tcs.SetCanceled());

            var subscription = this.callbacks.Subscribe<SteamClient.ConnectedCallback>((_) =>
            {
                cts.Dispose();
                tcs.SetResult(_);
            });

            this.client.Connect();

            return tcs.Task.ContinueWith(t => subscription.Dispose());
        }

        private Task LoginToSteamAsync(string username, string password, CancellationToken token)
        {
            var tcs = new TaskCompletionSource<SteamUser.LoggedOnCallback>();
            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            cts.CancelAfter(TimeSpan.FromSeconds(30));
            cts.Token.Register(() => tcs.SetCanceled());

            var subscription = this.callbacks.Subscribe<SteamUser.LoggedOnCallback>((_) =>
            {
                cts.Dispose();

                if (_.Result == EResult.OK)
                    friends.SetPersonaState(EPersonaState.Online);

                tcs.SetResult(_);
            });

            var details = new SteamUser.LogOnDetails { Username = username, Password = password };
            user.LogOn(details);

            return tcs.Task.ContinueWith(t => subscription.Dispose());
        }

        private Task StartDotaAsync(CancellationToken token)
        {
            var tcs = new TaskCompletionSource<GCWelcomeCallback>();
            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            cts.CancelAfter(TimeSpan.FromSeconds(30));
            cts.Token.Register(() => tcs.SetCanceled());

            var subscription = this.callbacks.Subscribe<GCWelcomeCallback>((_) =>
            {
                cts.Dispose();
                tcs.SetResult(_);
            });

            return dota.Start().ContinueWith(t => tcs.Task).ContinueWith(t => subscription.Dispose());
        }

        public async Task<SteamID> CreateSessionAsync(string username, string password, CancellationToken token = default(CancellationToken))
        {
            await this.ConnectToSteamAsync(token);
            await this.LoginToSteamAsync(username, password, token);
            await this.StartDotaAsync(token);

            return this.client.SteamID ?? throw new InvalidOperationException("SteamClient SteamID should be set after Login To Steam.");
        }

        public void StopSession()
        {
            this.callbackCts.Cancel();
            this.dota.Stop();
            this.user.LogOff();
        }

        public Task CreateLobbyAsync(string name, string password, uint region, bool shuffle_players, List<uint> requested_heroes)
        {
            var tcs = new TaskCompletionSource<CSODOTALobby>();
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            cts.Token.Register(() => tcs.SetCanceled());

            var subscription = this.callbacks.Subscribe<PracticeLobbySnapshot>((evt) =>
            {
                cts.Dispose();
                tcs.TrySetResult(evt.lobby);
            });

            var details = new CMsgPracticeLobbySetDetails
            {
                game_name = name,
                pass_key = password,
                server_region = region,
                game_mode = 18,
                allow_cheats = false,
                fill_with_bots = false,
                allow_spectating = true,
                dota_tv_delay = LobbyDotaTVDelay.LobbyDotaTV_120,
                pause_setting = LobbyDotaPauseSetting.LobbyDotaPauseSetting_Unlimited,
                game_version = DOTAGameVersion.GAME_VERSION_CURRENT,
                visibility = DOTALobbyVisibility.DOTALobbyVisibility_Public,
            };
            details.ability_draft_specific_details = new CMsgPracticeLobbySetDetails.AbilityDraftSpecificDetails
            {
                shuffle_draft_order = shuffle_players
            };

            if (requested_heroes.Count == 12)
                details.requested_hero_ids.AddRange(requested_heroes);

            dota.CreateLobby(details);

            return tcs.Task.ContinueWith(t => subscription.Dispose());
        }
      

        public async Task<CMsgDOTAJoinChatChannelResponse> JoinLobbyChatAsync()
        {
            var tcs = new TaskCompletionSource<CMsgDOTAJoinChatChannelResponse>();
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            cts.Token.Register(() => tcs.SetCanceled());
            
            var subscription = this.callbacks.Subscribe<JoinChatChannelResponse>((evt) =>
            {
                cts.Dispose();

                if (evt.result.result == CMsgDOTAJoinChatChannelResponse.Result.JOIN_SUCCESS)
                {
                    tcs.SetResult(evt.result);
                }
                else
                {
                    var reason = Enum.GetName(typeof(CMsgDOTAJoinChatChannelResponse.Result), evt.result.result);
                    var ex = new InvalidOperationException($"Failed to join chat: {reason}");
                    tcs.TrySetException(ex);
                }
            });

            try
            {

                var name = $"Lobby_{this.dota.Lobby.lobby_id}";
                dota.JoinChatChannel(name, DOTAChatChannelType_t.DOTAChannelType_Lobby);

                var result = await tcs.Task;
                return result;
            }
            finally
            {
                subscription.Dispose();
            }
        }

        public CSODOTALobby GetActiveLobby()
        {
            return this.dota.Lobby;
        }

        public Task DestroyLobbyAsync()
        {
            this.dota.DestroyLobby();
            return Task.CompletedTask;
        }

        public Task LaunchGameAsync()
        {
            this.dota.LaunchLobby();
            return Task.CompletedTask;
        }

        public Task InviteToLobby(ulong steamId)
        {
            this.dota.InviteToLobby(steamId);
            return Task.CompletedTask;
        }

        public Task ShuffleTeams()
        {
            this.dota.PracticeLobbyShuffleTeam();
            return Task.CompletedTask;
        }

        public Task KickPlayerFromLobbyTeam(uint accountId)
        {
            this.dota.KickPlayerFromLobbyTeam(accountId);
            return Task.CompletedTask;
        }
        public Task SendChatMessage(ulong id, string msg)
        {
            this.dota.SendChannelMessage(id, msg);
            return Task.CompletedTask;
        }
    }
}
