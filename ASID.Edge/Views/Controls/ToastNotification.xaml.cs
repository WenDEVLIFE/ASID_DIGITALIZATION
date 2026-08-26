using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace ASID.Edge.Views.Controls
{
    public enum ToastType { Success, Error, Warning, Info }

    public partial class ToastNotification : UserControl
    {
        public ToastNotification()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Show a toast notification. Caller should pass the parent ToastPanel
        /// control name, or use the static helper.
        /// </summary>
        public void Show(string message, ToastType type = ToastType.Info, int durationMs = 3000)
        {
            var toast = CreateToastItem(message, type, durationMs);
            ToastStack.Children.Insert(0, toast);

            // Slide in
            var slideIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250));
            toast.RenderTransform = new TranslateTransform(80, 0);
            toast.RenderTransform.BeginAnimation(TranslateTransform.XProperty, slideIn);

            // Auto-remove
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(durationMs) };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                RemoveToast(toast);
            };
            timer.Start();
        }

        /// <summary>Shorthand: success toast.</summary>
        public void Success(string message, int ms = 3000)
            => Show(message, ToastType.Success, ms);

        /// <summary>Shorthand: error toast (longer duration).</summary>
        public void Error(string message, int ms = 5000)
            => Show(message, ToastType.Error, ms);

        /// <summary>Shorthand: warning toast.</summary>
        public void Warning(string message, int ms = 4000)
            => Show(message, ToastType.Warning, ms);

        /// <summary>Shorthand: info toast.</summary>
        public void Info(string message, int ms = 3000)
            => Show(message, ToastType.Info, ms);

        private Border CreateToastItem(string message, ToastType type, int durationMs)
        {
            var (bg, fg, icon) = type switch
            {
                ToastType.Success => ("#2E7D32", "White", "✓"),
                ToastType.Error   => ("#C62828", "White", "✗"),
                ToastType.Warning => ("#F57F17", "White", "⚠"),
                _                 => ("#1565C0", "White", "ℹ"),
            };

            var border = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bg)),
                Style = (Style)FindResource("ToastBorder"),
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto, MinWidth = 10 });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Icon
            var iconText = new TextBlock
            {
                Text = icon,
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fg)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0),
            };
            Grid.SetColumn(iconText, 0);

            // Message
            var msgText = new TextBlock
            {
                Text = message,
                FontSize = 14,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fg)),
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 320,
            };
            Grid.SetColumn(msgText, 1);

            // Close button
            var closeBtn = new Button
            {
                Content = "✕",
                FontSize = 14,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fg)),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0),
                Padding = new Thickness(4, 0, 4, 0),
            };
            Grid.SetColumn(closeBtn, 2);

            closeBtn.Click += (_, _) => RemoveToast(border);

            grid.Children.Add(iconText);
            grid.Children.Add(msgText);
            grid.Children.Add(closeBtn);
            border.Child = grid;

            return border;
        }

        private void RemoveToast(Border toast)
        {
            if (!ToastStack.Children.Contains(toast))
                return;

            var slideOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
            slideOut.Completed += (_, _) =>
            {
                ToastStack.Children.Remove(toast);
            };
            toast.RenderTransform?.BeginAnimation(TranslateTransform.XProperty, slideOut);
        }
    }
}
