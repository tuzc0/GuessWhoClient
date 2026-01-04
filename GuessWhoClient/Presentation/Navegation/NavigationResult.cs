using System;

namespace GuessWhoClient.Presentation.Navegation
{
    public sealed class NavigationResult
    {
        private const string EMPTY = "";

        private NavigationResult(bool isSuccess, bool hasView, object view, string code, GameScreenType screenType) 
        {
            IsSuccess = isSuccess;
            HasView = hasView;
            View = view;
            Code = code ?? EMPTY;
            ScreenType = screenType;
        }

        public bool IsSuccess { get; }
        public bool HasView { get; }
        public object View { get; }
        public string Code { get; }
        public GameScreenType ScreenType { get; }

        public static NavigationResult Ok(object view, GameScreenType screenType)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            return new NavigationResult(true, true, view, EMPTY, screenType);
        }

        public static NavigationResult Fail(string code, GameScreenType screenType)
        {
            return new NavigationResult(false, false, null, code, screenType);
        }
    }
}
