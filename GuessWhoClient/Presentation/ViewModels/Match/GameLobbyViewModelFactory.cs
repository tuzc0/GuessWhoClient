using GuessWhoClient.Assets;
using GuessWhoClient.Infraestructure.ErrorHandling;
using GuessWhoClient.Infraestructure.Match;
using GuessWhoClient.Session;
using GuessWhoCore.Contracts.Requests;
using GuessWhoCore.Contracts.Response;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace GuessWhoClient.Presentation.ViewModels.Match
{
    public sealed class GameLobbyViewModelFactory : IGameLobbyViewModelFactory
    {
        private const string EMPTY = "";
        private const long INVALID_ID = 0;

        private const byte DEFAULT_VISIBILITY_PRIVATE = 2;
        private const byte DEFAULT_MODE_CLASSIC = 1;

        private const byte HOST_SLOT_NUMBER = 1;

        private const string SESSION_PROP_DISPLAY_NAME = "DisplayName";
        private const string SESSION_PROP_AVATAR_ID = "AvatarId";
        private const string SESSION_PROP_AVATAR = "Avatar";
        private const string SESSION_PROP_AVATAR_KEY = "AvatarKey";

        private readonly MatchHub matchHub;
        private readonly IAvatarPathResolver avatarPathResolver;
        private readonly IUiFaultMapper uiFaultMapper;
        private readonly Func<string, string> localize;
        private readonly SessionContext sessionContext;

        public GameLobbyViewModelFactory(
            MatchHub matchHub,
            IAvatarPathResolver avatarPathResolver,
            IUiFaultMapper uiFaultMapper,
            Func<string, string> localize,
            SessionContext sessionContext)
        {
            this.matchHub = matchHub ?? throw new ArgumentNullException(nameof(matchHub));
            this.avatarPathResolver = avatarPathResolver ?? throw new ArgumentNullException(nameof(avatarPathResolver));
            this.uiFaultMapper = uiFaultMapper ?? throw new ArgumentNullException(nameof(uiFaultMapper));
            this.localize = localize ?? throw new ArgumentNullException(nameof(localize));
            this.sessionContext = sessionContext ?? throw new ArgumentNullException(nameof(sessionContext));
        }

        public GameLobbyViewModel CreateFromJoin(JoinMatchResponse response, long currentUserId)
        {
            if (response == null || response.MatchId <= INVALID_ID || currentUserId <= INVALID_ID)
            {
                return CreateInvalidLobbyVm();
            }

            return new GameLobbyViewModel(
                matchHub,
                avatarPathResolver,
                uiFaultMapper,
                localize,
                matchId: response.MatchId,
                matchCode: response.Code ?? EMPTY,
                currentUserId: currentUserId,
                hostUserId: response.HostUserId,
                initialVisibility: response.Visibility,
                initialMode: response.Mode,
                initialPlayers: response.Players ?? new List<LobbyPlayerDto>());
        }

        public GameLobbyViewModel CreateFromCreate(CreateMatchResponse created, string createdMatchCode, long currentUserId)
        {
            if (created == null || created.MatchId <= INVALID_ID || currentUserId <= INVALID_ID)
            {
                return CreateInvalidLobbyVm();
            }

            LobbyPlayerDto hostPlayer = BuildHostPlayerDto(created.MatchId, currentUserId);

            var initialPlayers = new List<LobbyPlayerDto> { hostPlayer };

            byte visibility = created.VisibilityId != 0 ? created.VisibilityId : DEFAULT_VISIBILITY_PRIVATE;
            byte mode = created.ModeId != 0 ? created.ModeId : DEFAULT_MODE_CLASSIC;

            return new GameLobbyViewModel(
                matchHub,
                avatarPathResolver,
                uiFaultMapper,
                localize,
                matchId: created.MatchId,
                matchCode: (createdMatchCode ?? EMPTY).Trim(),
                currentUserId: currentUserId,
                hostUserId: currentUserId,
                initialVisibility: visibility,
                initialMode: mode,
                initialPlayers: initialPlayers);
        }

        private LobbyPlayerDto BuildHostPlayerDto(long matchId, long userId)
        {
            string displayName = ReadSessionString(sessionContext, SESSION_PROP_DISPLAY_NAME);

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = userId.ToString();
            }

            string avatarId =
                ReadSessionString(sessionContext, SESSION_PROP_AVATAR_ID) ??
                ReadSessionString(sessionContext, SESSION_PROP_AVATAR_KEY) ??
                ReadSessionString(sessionContext, SESSION_PROP_AVATAR) ??
                EMPTY;

            return new LobbyPlayerDto
            {
                MatchId = matchId,
                UserId = userId,
                DisplayName = displayName,
                AvatarId = avatarId,
                SlotNumber = HOST_SLOT_NUMBER,
                IsReady = true,
                IsHost = true
            };
        }

        private static string ReadSessionString(SessionContext session, string propertyName)
        {
            if (session == null || string.IsNullOrWhiteSpace(propertyName))
            {
                return EMPTY;
            }

            PropertyInfo prop = session.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);

            if (prop == null)
            {
                return EMPTY;
            }

            object value = prop.GetValue(session, null);

            return value is string s ? (s ?? EMPTY) : EMPTY;
        }

        private GameLobbyViewModel CreateInvalidLobbyVm()
        {
            return new GameLobbyViewModel(
                matchHub,
                avatarPathResolver,
                uiFaultMapper,
                localize,
                matchId: 0,
                matchCode: EMPTY,
                currentUserId: 0,
                hostUserId: 0,
                initialVisibility: DEFAULT_VISIBILITY_PRIVATE,
                initialMode: DEFAULT_MODE_CLASSIC,
                initialPlayers: new List<LobbyPlayerDto>());
        }
    }
}
