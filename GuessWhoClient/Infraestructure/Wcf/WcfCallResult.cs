namespace GuessWhoClient.Infraestructure.Wcf
{
    public sealed class WcfCallResult<T>
    {
        private const string EMPTY = "";

        private WcfCallResult(bool isSuccess, bool hasValue, T value, string faultCode, string serverMessage)
        {
            IsSuccess = isSuccess;
            HasValue = hasValue;
            Value = value;
            FaultCode = faultCode ?? EMPTY;
            ServerMessage = serverMessage ?? EMPTY;
        }

        public bool IsSuccess { get; }
        public bool HasValue { get; }
        public T Value { get; }
        public string FaultCode { get; }
        public string ServerMessage { get; }

        public static WcfCallResult<T> Ok(T value)
        {
            return new WcfCallResult<T>(true, true, value, EMPTY, EMPTY);
        }

        public static WcfCallResult<T> Fail(string faultCode, string serverMessage)
        {
            return new WcfCallResult<T>(false, false, default, faultCode, serverMessage);
        }
    }
}
