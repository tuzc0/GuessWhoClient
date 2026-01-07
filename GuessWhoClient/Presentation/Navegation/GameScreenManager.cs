using System;
using System.Collections.Generic;
namespace GuessWhoClient.Presentation.Navegation
{
    public sealed class GameScreenManager : IGameScreenManager
    {
        private readonly IGameScreenHost screenHost;
        private readonly IReadOnlyDictionary<GameScreenType, Func<object>> screenFactories; 

        public GameScreenManager(
            IGameScreenHost screenHost,
            IReadOnlyDictionary<GameScreenType, Func<object>> screenFactories)
        {
            this.screenHost = screenHost ?? throw new ArgumentNullException(nameof(screenHost));
            this.screenFactories = screenFactories ?? throw new ArgumentNullException(nameof(screenFactories));
        }

        public NavigationResult ShowScreen(GameScreenType screenType)
        {
            NavigationResult result = Create(screenType);

            if (result.IsSuccess)
            {
                screenHost.SetMainContent(result.View);
            }

            return result;
        }

        public NavigationResult ShowOverlay(GameScreenType screenType)
        {
            NavigationResult result = Create(screenType);

            if (result.IsSuccess)
            {
                screenHost.SetOverlayContent(result.View);
            }

            return result;
        }

        public void HideOverlay()
        {
            screenHost.SetOverlayContent(null);
        }


        private NavigationResult Create(GameScreenType screenType)
        {
            if (!screenFactories.TryGetValue(screenType, out Func<object> factory) || factory == null)
            {
                return NavigationResult.Fail(NavigationCodes.CODE_SCREEN_FACTORY_MISSING, screenType);
            }

            object view = factory.Invoke();

            if (view == null)
            {
                return NavigationResult.Fail(NavigationCodes.CODE_SCREEN_FACTORY_RETURNED_NULL, screenType);
            }

            return NavigationResult.Ok(view, screenType);
        }
    }
}
