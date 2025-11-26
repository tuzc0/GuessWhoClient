using System;
using System.Windows;
using System.Windows.Controls;

namespace GuessWhoClient.Utilities
{
    public sealed class ChangePasswordVisibility
    {
        public bool IsPasswordVisible { get; }
        public PasswordBox PasswordBox { get; }
        public TextBox VisibleTextBox { get; }

        public ChangePasswordVisibility(bool isPasswordVisible, PasswordBox passwordBox, TextBox visibleTextBox)
        {
            PasswordBox = passwordBox ?? throw new ArgumentNullException(nameof(passwordBox));
            VisibleTextBox = visibleTextBox ?? throw new ArgumentNullException(nameof(visibleTextBox));
            IsPasswordVisible = isPasswordVisible;
        }
    }

    public static class PasswordVisibilityHelper
    {
        public static void TogglePasswordPair(ChangePasswordVisibility visibilityContext)
        {
            if (visibilityContext.IsPasswordVisible)
            {
                visibilityContext.VisibleTextBox.Text = visibilityContext.PasswordBox.Password;
                visibilityContext.VisibleTextBox.Visibility = Visibility.Visible;
                visibilityContext.PasswordBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                visibilityContext.PasswordBox.Password = visibilityContext.VisibleTextBox.Text;
                visibilityContext.VisibleTextBox.Visibility = Visibility.Collapsed;
                visibilityContext.PasswordBox.Visibility = Visibility.Visible;
            }
        }

        public static void SyncPasswordToText(ChangePasswordVisibility visibilityContext)
        {
            if (!visibilityContext.IsPasswordVisible)
            {
                return;
            }

            visibilityContext.VisibleTextBox.Text = visibilityContext.PasswordBox.Password;
        }

        public static void SyncTextToPassword(ChangePasswordVisibility visibilityContext)
        {
            if (!visibilityContext.IsPasswordVisible)
            {
                return;
            }

            visibilityContext.PasswordBox.Password = visibilityContext.VisibleTextBox.Text;
        }
    }
}
