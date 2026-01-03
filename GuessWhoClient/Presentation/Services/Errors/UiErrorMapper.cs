using GuessWhoClient.Globalization;
using System.ServiceModel.Security;
using log4net;
using System;
using System.ServiceModel;
using GuessWhoCore.Contracts.Faults;

namespace GuessWhoClient.Presentation.Services.Errors
{
    public class UiErrorMapper : IUiErrorMapper
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(UiErrorMapper));

        private const string UI_SECURITY_NEGOTIATION_FAILED_KEY = "UiSecurityNegotiationFailed";
        private const string UI_COMMS_GENERIC_KEY = "UiCommsGeneric";
        private const string FAULT_UNEXPECTED_KEY = "FaultUnexpected";
        private const string FAULT_DATABASE_TIMEOUT_KEY = "FaultDatabaseTimeout";

        private readonly ILocalizationService localizationService;

        public UiErrorMapper(ILocalizationService localizationService)
        {
            this.localizationService = localizationService;
        }

        public string ToUserMessage(Exception ex)
        {
            if (ex == null)
            {
                return localizationService.Get(FAULT_UNEXPECTED_KEY);
            }

            if (ex is SecurityNegotiationException)
            {
                Logger.Error("Security negotiation failed", ex);
                return localizationService.Get(UI_SECURITY_NEGOTIATION_FAILED_KEY);
            }

            if (ex is TimeoutException)
            {
                Logger.Error("Database timeout occurred", ex);
                return localizationService.Get(FAULT_DATABASE_TIMEOUT_KEY);
            }

            if (ex is CommunicationException)
            {
                Logger.Error("Communication error occurred", ex);
                return localizationService.Get(UI_COMMS_GENERIC_KEY);
            }

            if (ex is FaultException<ServiceFault> faultEx)
            {
                Logger.Warn("Service fault received", faultEx);

                string faultKey = string.Concat("Fault", faultEx.Detail?.Code);
                
                return localizationService.LocalOrFallback(
                    faultKey,
                    faultEx.Detail?.Message,
                    FAULT_UNEXPECTED_KEY);
            }

            Logger.Error("Unexpected error occurred", ex);
            return localizationService.Get(FAULT_UNEXPECTED_KEY);
        }
    }
}
