namespace GuessWhoClient.Infraestructure.Wcf
{
    public static class WcfTechnicalFaultCodes
    {
        public const string ENDPOINT_NOT_FOUND = "WCF_ENDPOINT_NOT_FOUND";
        public const string SECURITY_ERROR = "WCF_SECURITY_ERROR";
        public const string TIMEOUT = "WCF_TIMEOUT";
        public const string COMMUNICATION_ERROR = "WCF_COMMUNICATION_ERROR";
        public const string UNEXPECTED = "WCF_UNEXPECTED";
        public const string NULL_RESPONSE = "WCF_NULL_RESPONSE";

        public const string CLIENT_NOT_CONNECTED = "WCF_CLIENT_NOT_CONNECTED";
    }
}
