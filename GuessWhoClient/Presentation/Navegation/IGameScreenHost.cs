namespace GuessWhoClient.Presentation.Navegation
{
    public interface IGameScreenHost
    {
        void SetMainContent(object view);
        void SetOverlayContent(object view);
    }
}
