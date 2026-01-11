using GuessWhoClient.Infraestructure.ErrorHandling;
using System;

namespace GuessWhoClient.Presentation.ViewModels.Match
{
    public sealed class MatchUiKeyResolver : IMatchUiKeyResolver
    {
        private const string EMPTY = "";

        private readonly IUiFaultMapper uiFaultMapper;

        public MatchUiKeyResolver(IUiFaultMapper uiFaultMapper)
        {
            this.uiFaultMapper = uiFaultMapper ?? throw new ArgumentNullException(nameof(uiFaultMapper));
        }

        public string ResolveFaultOrServerKey(string faultCode, string serverMessageKey, string fallbackKey)
        {
            UiKeyMapping mapping = uiFaultMapper.Map(faultCode);

            if (mapping.IsMapped)
            {
                return mapping.UiKey;
            }

            if (!string.IsNullOrWhiteSpace(serverMessageKey))
            {
                return serverMessageKey;
            }

            return fallbackKey ?? EMPTY;
        }

        public string ResolveCodeOrFallback(string code, string fallbackKey)
        {
            UiKeyMapping mapping = uiFaultMapper.Map(code);

            return mapping.IsMapped
                ? mapping.UiKey
                : (fallbackKey ?? EMPTY);
        }
    }
}
