using System;

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
            if (value is null && typeof(T) == typeof(string))
            {
                value = (T)(object)EMPTY;
            }

            return new WcfCallResult<T>(true, true, value, EMPTY, EMPTY);
        }

        public static WcfCallResult<T> Fail(string faultCode, string serverMessage)
        {
            return new WcfCallResult<T>(false, false, CreateDefaultValue(), faultCode, serverMessage);
        }


        private static T CreateDefaultValue()
        {
            Type type = typeof(T);

            if (type == typeof(string))
            {
                return (T)(object)EMPTY;
            }

            if (type.IsValueType)
            {
                return default;
            }

            try
            {
                object instance = Activator.CreateInstance(type);
                return (T)instance;
            }
            catch (MissingMethodException)
            {
                return default;
            }
            catch (MemberAccessException)
            {
                return default;
            }
            catch (System.Reflection.TargetInvocationException)
            {
                return default;
            }
            catch (TypeLoadException)
            {
                return default;
            }
        }

    }
}
