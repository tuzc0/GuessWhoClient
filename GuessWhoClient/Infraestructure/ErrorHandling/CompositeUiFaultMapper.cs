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

        public UiKeyMapping Map(string faultCode)
        {
            for (int index = 0; index < mappers.Length; index++)
            {
                IUiFaultMapper mapper = mappers[index];

                if (mapper == null)
                {
                    continue;
                }

                UiKeyMapping mapping = mapper.Map(faultCode);

                if (mapping.IsMapped)
                {
                    return mapping;
                }
            }

            return UiKeyMapping.Unmapped();
        }
    }
}
