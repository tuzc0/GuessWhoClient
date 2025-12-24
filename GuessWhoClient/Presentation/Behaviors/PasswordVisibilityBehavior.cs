using System;
using System.Windows;
using System.Windows.Controls;

namespace GuessWhoClient.Presentation.Behaviors
{
    public static class PasswordVisibilityBehavior
    {
        public static readonly DependencyProperty IsPasswordVisibleProperty =
            DependencyProperty.RegisterAttached(
                "IsPasswordVisible",
                typeof(bool),
                typeof(PasswordVisibilityBehavior),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnPasswordBoxIsPasswordVisibleChanged));

        public static readonly DependencyProperty VisibleTextBoxProperty =
            DependencyProperty.RegisterAttached(
                "VisibleTextBox",
                typeof(TextBox),
                typeof(PasswordVisibilityBehavior),
                new PropertyMetadata(null, OnPasswordBoxVisibleTextBoxChanged));

        private static readonly DependencyProperty IsPasswordBoxEventHandlersAttachedProperty =
            DependencyProperty.RegisterAttached(
                "IsPasswordBoxEventHandlersAttached",
                typeof(bool),
                typeof(PasswordVisibilityBehavior),
                new PropertyMetadata(false));

        public static readonly DependencyProperty PasswordBoxProperty =
            DependencyProperty.RegisterAttached(
                "PasswordBox",
                typeof(PasswordBox),
                typeof(PasswordVisibilityBehavior),
                new PropertyMetadata(null, OnTextBoxPasswordBoxChanged));

        public static readonly DependencyProperty IsTextVisibleProperty =
            DependencyProperty.RegisterAttached(
                "IsTextVisible",
                typeof(bool),
                typeof(PasswordVisibilityBehavior),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnTextBoxIsTextVisibleChanged));

        private static readonly DependencyProperty IsTextBoxEventHandlersAttachedProperty =
            DependencyProperty.RegisterAttached(
                "IsTextBoxEventHandlersAttached",
                typeof(bool),
                typeof(PasswordVisibilityBehavior),
                new PropertyMetadata(false));

        public static bool GetIsPasswordVisible(DependencyObject dependencyObject)
        {
            return (bool)dependencyObject.GetValue(IsPasswordVisibleProperty);
        }

        public static void SetIsPasswordVisible(DependencyObject dependencyObject, 
            bool isPasswordVisible)
        {
            dependencyObject.SetValue(IsPasswordVisibleProperty, isPasswordVisible);
        }

        public static TextBox GetVisibleTextBox(DependencyObject dependencyObject)
        {
            return (TextBox)dependencyObject.GetValue(VisibleTextBoxProperty);
        }

        public static void SetVisibleTextBox(DependencyObject dependencyObject, 
            TextBox visibleTextBox)
        {
            dependencyObject.SetValue(VisibleTextBoxProperty, visibleTextBox);
        }

        private static bool GetIsPasswordBoxEventHandlersAttached(DependencyObject dependencyObject)
        {
            return (bool)dependencyObject.GetValue(IsPasswordBoxEventHandlersAttachedProperty);
        }

        private static void SetIsPasswordBoxEventHandlersAttached(DependencyObject dependencyObject,
            bool areEventHandlersAttached)
        {
            dependencyObject.SetValue(IsPasswordBoxEventHandlersAttachedProperty, areEventHandlersAttached);
        }

        public static PasswordBox GetPasswordBox(DependencyObject dependencyObject)
        {
            return (PasswordBox)dependencyObject.GetValue(PasswordBoxProperty);
        }

        public static void SetPasswordBox(DependencyObject dependencyObject, PasswordBox passwordBox)
        {
            dependencyObject.SetValue(PasswordBoxProperty, passwordBox);
        }

        public static bool GetIsTextVisible(DependencyObject dependencyObject)
        {
            return (bool)dependencyObject.GetValue(IsTextVisibleProperty);
        }

        public static void SetIsTextVisible(DependencyObject dependencyObject, bool isTextVisible)
        {
            dependencyObject.SetValue(IsTextVisibleProperty, isTextVisible);
        }

        private static bool GetIsTextBoxEventHandlersAttached(DependencyObject dependencyObject)
        {
            return (bool)dependencyObject.GetValue(IsTextBoxEventHandlersAttachedProperty);
        }

        private static void SetIsTextBoxEventHandlersAttached(DependencyObject dependencyObject,
            bool areEventHandlersAttached)
        {
            dependencyObject.SetValue(IsTextBoxEventHandlersAttachedProperty, areEventHandlersAttached);
        }

        private static void OnPasswordBoxVisibleTextBoxChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs args)
        {
            var passwordBox = dependencyObject as PasswordBox;

            if (passwordBox == null)
            {
                return;
            }

            EnsurePasswordBoxEventsAttached(passwordBox);
            ApplyVisibilityFromPasswordBox(passwordBox);
        }

        private static void OnPasswordBoxIsPasswordVisibleChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs args)
        {
            var passwordBox = dependencyObject as PasswordBox;

            if (passwordBox == null)
            {
                return;
            }

            EnsurePasswordBoxEventsAttached(passwordBox);
            ApplyVisibilityFromPasswordBox(passwordBox);
        }

        private static void EnsurePasswordBoxEventsAttached(PasswordBox passwordBox)
        {
            if (GetIsPasswordBoxEventHandlersAttached(passwordBox))
            {
                return;
            }

            passwordBox.PasswordChanged += OnPasswordBoxPasswordChanged;
            passwordBox.Unloaded += OnPasswordBoxUnloaded;

            SetIsPasswordBoxEventHandlersAttached(passwordBox, true);
        }

        private static void ApplyVisibilityFromPasswordBox(PasswordBox passwordBox)
        {
            TextBox visibleTextBox = GetVisibleTextBox(passwordBox);

            if (visibleTextBox == null)
            {
                return;
            }

            bool shouldShowPlainText = GetIsPasswordVisible(passwordBox);

            if (shouldShowPlainText)
            {
                if (!string.Equals(visibleTextBox.Text, passwordBox.Password, StringComparison.Ordinal))
                {
                    visibleTextBox.Text = passwordBox.Password;
                }

                visibleTextBox.Visibility = Visibility.Visible;
                passwordBox.Visibility = Visibility.Collapsed;
                return;
            }

            if (!string.Equals(passwordBox.Password, visibleTextBox.Text, StringComparison.Ordinal))
            {
                passwordBox.Password = visibleTextBox.Text;
            }

            visibleTextBox.Visibility = Visibility.Collapsed;
            passwordBox.Visibility = Visibility.Visible;
        }

        private static void OnPasswordBoxPasswordChanged(object sender, RoutedEventArgs args)
        {
            var passwordBox = sender as PasswordBox;

            if (passwordBox == null)
            {
                return;
            }

            if (!GetIsPasswordVisible(passwordBox))
            {
                return;
            }

            TextBox visibleTextBox = GetVisibleTextBox(passwordBox);

            if (visibleTextBox == null)
            {
                return;
            }

            if (!string.Equals(visibleTextBox.Text, passwordBox.Password, StringComparison.Ordinal))
            {
                visibleTextBox.Text = passwordBox.Password;
            }
        }

        private static void OnPasswordBoxUnloaded(object sender, RoutedEventArgs args)
        {
            var passwordBox = sender as PasswordBox;

            if (passwordBox == null)
            {
                return;
            }

            passwordBox.PasswordChanged -= OnPasswordBoxPasswordChanged;
            passwordBox.Unloaded -= OnPasswordBoxUnloaded;

            SetIsPasswordBoxEventHandlersAttached(passwordBox, false);
        }

        private static void OnTextBoxPasswordBoxChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs args)
        {
            var visibleTextBox = dependencyObject as TextBox;

            if (visibleTextBox == null)
            {
                return;
            }

            EnsureTextBoxEventsAttached(visibleTextBox);
            ApplyVisibilityFromTextBox(visibleTextBox);
        }

        private static void OnTextBoxIsTextVisibleChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs args)
        {
            var visibleTextBox = dependencyObject as TextBox;

            if (visibleTextBox == null)
            {
                return;
            }

            EnsureTextBoxEventsAttached(visibleTextBox);
            ApplyVisibilityFromTextBox(visibleTextBox);
        }

        private static void EnsureTextBoxEventsAttached(TextBox visibleTextBox)
        {
            if (GetIsTextBoxEventHandlersAttached(visibleTextBox))
            {
                return;
            }

            visibleTextBox.TextChanged += OnVisibleTextBoxTextChanged;
            visibleTextBox.Unloaded += OnVisibleTextBoxUnloaded;

            SetIsTextBoxEventHandlersAttached(visibleTextBox, true);
        }

        private static void ApplyVisibilityFromTextBox(TextBox visibleTextBox)
        {
            PasswordBox passwordBox = GetPasswordBox(visibleTextBox);

            if (passwordBox == null)
            {
                return;
            }

            bool shouldShowPlainText = GetIsTextVisible(visibleTextBox);

            SetIsPasswordVisible(passwordBox, shouldShowPlainText);

            if (shouldShowPlainText)
            {
                if (!string.Equals(visibleTextBox.Text, passwordBox.Password, StringComparison.Ordinal))
                {
                    visibleTextBox.Text = passwordBox.Password;
                }

                visibleTextBox.Visibility = Visibility.Visible;
                passwordBox.Visibility = Visibility.Collapsed;
                return;
            }

            if (!string.Equals(passwordBox.Password, visibleTextBox.Text, StringComparison.Ordinal))
            {
                passwordBox.Password = visibleTextBox.Text;
            }

            visibleTextBox.Visibility = Visibility.Collapsed;
            passwordBox.Visibility = Visibility.Visible;
        }

        private static void OnVisibleTextBoxTextChanged(object sender, TextChangedEventArgs args)
        {
            var visibleTextBox = sender as TextBox;

            if (visibleTextBox == null)
            {
                return;
            }

            if (!GetIsTextVisible(visibleTextBox))
            {
                return;
            }

            PasswordBox passwordBox = GetPasswordBox(visibleTextBox);

            if (passwordBox == null)
            {
                return;
            }

            if (!string.Equals(passwordBox.Password, visibleTextBox.Text, StringComparison.Ordinal))
            {
                passwordBox.Password = visibleTextBox.Text;
            }
        }

        private static void OnVisibleTextBoxUnloaded(object sender, RoutedEventArgs args)
        {
            var visibleTextBox = sender as TextBox;

            if (visibleTextBox == null)
            {
                return;
            }

            visibleTextBox.TextChanged -= OnVisibleTextBoxTextChanged;
            visibleTextBox.Unloaded -= OnVisibleTextBoxUnloaded;

            SetIsTextBoxEventHandlersAttached(visibleTextBox, false);
        }
    }
}
