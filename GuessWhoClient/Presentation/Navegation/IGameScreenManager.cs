namespace GuessWhoClient.Presentation.Navegation
{
    public interface IGameScreenManager
    {
        NavigationResult ShowScreen(GameScreenType screenType);
        NavigationResult ShowOverlay(GameScreenType screenType);
        void HideOverlay();
    }
}
