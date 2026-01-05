using GuessWhoClient.Globalization;
using GuessWhoClient.Infraestructure.ErrorHandling;
using GuessWhoClient.Infraestructure.Wcf;
using GuessWhoClient.Presentation.ViewsModels.Base;
using GuessWhoClient.Services.Alerts;
using log4net;
using System;

namespace GuessWhoClient.Presentation.ViewModels.Auth
{
    public abstract class AuthViewModelBase : ViewModelBase
    {
        protected const string EMPTY = "";

        protected const string KEY_UI_TITLE_ERROR = "UiTitleError";
        protected const string KEY_UI_TITLE_INFO = "UiTitleInfo";
        protected const string KEY_UI_TITLE_WARNING = "UiTitleWarning";

        protected const string KEY_UI_GENERIC_ERROR = "UiGenericError";

        private const string LOCALIZATION_MISSING_PREFIX = "!";
        private const string LOCALIZATION_MISSING_SUFFIX = "!";

        protected readonly IAlertService alertService;
        protected readonly ILocalizationService localizationService;
        protected readonly IUiFaultMapper faultMapper;

        protected AuthViewModelBase(
            IAlertService alertService,
            ILocalizationService localizationService,
            IUiFaultMapper faultMapper)
        {
            this.alertService = alertService ?? throw new ArgumentNullException(nameof(alertService));
            this.localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            this.faultMapper = faultMapper ?? throw new ArgumentNullException(nameof(faultMapper));
        }

        protected abstract ILog LoggerInstance { get; }

        protected bool TryBeginOperation()
        {
            if (IsBusy)
            {
                return false;
            }

            IsBusy = true;
            return true;
        }

        protected void EndOperation()
        {
            IsBusy = false;
        }

        protected void ShowCallError<T>(WcfCallResult<T> result, string fallbackUiKey, string logContext)
        {
            string message = ResolveUiMessage(result, fallbackUiKey);

            alertService.Error(message, localizationService.Get(KEY_UI_TITLE_ERROR));

            string code = result != null ? result.FaultCode ?? EMPTY : EMPTY;
            string serverMessageKey = result != null ? result.ServerMessage ?? EMPTY : EMPTY;

            LoggerInstance.WarnFormat(
                "{0}. FaultCode='{1}', ServerMessage='{2}'.",
                logContext ?? EMPTY,
                code,
                serverMessageKey);
        }

        protected void ShowCallWarning<T>(WcfCallResult<T> result, string fallbackUiKey, string logContext)
        {
            string message = ResolveUiMessage(result, fallbackUiKey);

            alertService.Warn(message, localizationService.Get(KEY_UI_TITLE_WARNING));

            string code = result != null ? result.FaultCode ?? EMPTY : EMPTY;
            string serverMessageKey = result != null ? result.ServerMessage ?? EMPTY : EMPTY;

            LoggerInstance.WarnFormat(
                "{0}. FaultCode='{1}', ServerMessage='{2}'.",
                logContext ?? EMPTY,
                code,
                serverMessageKey);
        }

        protected string ResolveUiMessage<T>(WcfCallResult<T> result, string fallbackUiKey)
        {
            if (result == null)
            {
                return localizationService.Get(fallbackUiKey);
            }

            UiKeyMapping mapping = faultMapper.Map(result.FaultCode);

            if (mapping.IsMapped)
            {
                return localizationService.Get(mapping.UiKey);
            }

            string serverKeyLocalized = TryLocalizeKeyOrEmpty(result.ServerMessage);

            if (!string.IsNullOrWhiteSpace(serverKeyLocalized))
            {
                return serverKeyLocalized;
            }

            return localizationService.Get(fallbackUiKey);
        }

        protected string TryLocalizeKeyOrEmpty(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return EMPTY;
            }

            string localized = localizationService.Get(key) ?? EMPTY;
            string missingMarker = string.Concat(LOCALIZATION_MISSING_PREFIX, key, LOCALIZATION_MISSING_SUFFIX);

            return string.Equals(localized, missingMarker, StringComparison.Ordinal)
                ? EMPTY
                : localized;
        }

        protected string FormatFromResource(string templateKey, params object[] args)
        {
            string template = localizationService.Get(templateKey);

            try
            {
                return string.Format(template, args ?? Array.Empty<object>());
            }
            catch (FormatException)
            {
                return template ?? EMPTY;
            }
        }
    }
}
