using GuessWhoClient.Windows.ScreensType;
using System.Windows.Controls;

namespace GuessWhoClient.Interfaces
{
    public interface IGameScreenManager
    {
        void ShowScreen(GameScreenType screenType);
        void ShowOverlay(GameScreenType overlayType);
        void CloseOverlay();
        void ExitGame();
    }
}
