using System;

namespace GuessWhoClient.Presentation.Services.Errors
{
    public interface IUiErrorMapper
    {
        string ToUserMessage(Exception ex);
    }
}
