namespace GuessWhoClient.Infraestructure.Wcf
{
    internal static class WcfEndpointNames
    {
        public const string LOGIN_SERVICE = "NetTcpBinding_ILoginService";
        public const string FRIEND_SERVICE = "NetTcpBinding_IFriendService";
        public const string USER_SERVICE = "NetTcpBinding_IUserService";
        public const string UPDATE_SERVICE = "NetTcpBinding_IUpdateProfileService";
        public const string MATCH_SERVICE = "NetTcpBinding_IMatchService";
        public const string LEADERBOARD_SERVICE = "NetTcpBinding_ILeaderboardService";
    }
}
