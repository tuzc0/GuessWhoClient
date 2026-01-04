using System;

namespace GuessWhoClient.Infraestructure.ErrorHandling
{
    public sealed class CompositeUiFaultMapper : IUiFaultMapper
    {
        private readonly IUiFaultMapper[] mappers;

        public CompositeUiFaultMapper(params IUiFaultMapper[] mappers)
        {
            this.mappers = mappers ?? Array.Empty<IUiFaultMapper>();
        }

        public bool TryMap(string faultCode, out string uiKey)
        {
            uiKey = null;

            for (int index = 0; index < mappers.Length; index++)
            {
                IUiFaultMapper mapper = mappers[index];

                if (mapper != null && mapper.TryMap(faultCode, out uiKey))
                {
                    return true;
                }
            }

            uiKey = null;
            return false;
        }
    }
}
