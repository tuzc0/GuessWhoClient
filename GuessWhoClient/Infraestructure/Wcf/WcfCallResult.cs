namespace GuessWhoClient.Infraestructure.Wcf
{
    public sealed class WcfCallResult<T>
    {
        private const string EMPTY = "";

        private WcfCallResult(bool isSuccess, T value, string faultCode, string serverMessage)
        {
            IsSuccess = isSuccess;
            Value = value;
            FaultCode = faultCode ?? EMPTY;
            ServerMessage = serverMessage ?? EMPTY;
        }

        public bool IsSuccess { get; }
        public T Value { get; }
        public string FaultCode { get; }
        public string ServerMessage { get; }

        public static WcfCallResult<T> Ok(T value)
        {
            return new WcfCallResult<T>(true, value, EMPTY, EMPTY);
        }

        public static WcfCallResult<T> Fail(string faultCode, string serverMessage)
        {
            return new WcfCallResult<T>(false, default, faultCode, serverMessage);
        }
    }
}
