using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using ScreenTranslator.Services;

namespace ScreenTranslator
{
    public partial class OverlayWindow : Window
    {
        public OverlayWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var hwnd = new WindowInteropHelper(this).Handle;
            WindowManager.MakeWindowClickThrough(hwnd);
        }

        public void UpdatePosition(WindowManager.RECT rect)
        {
            this.Left = rect.Left;
            this.Top = rect.Top;
            this.Width = rect.Width;
            this.Height = rect.Height;
        }

        public void RenderTranslations(List<(OcrRegion Region, string TranslatedText)> items)
        {
            OverlayCanvas.Children.Clear();

            foreach (var item in items)
            {
                if (string.IsNullOrWhiteSpace(item.TranslatedText)) continue;

                var border = new Border
                {
                    Background = new SolidColorBrush(Color.FromArgb(230, 15, 15, 20)),
                    CornerRadius = new CornerRadius(3),
                    Padding = new Thickness(2, 0, 2, 0)
                };

                var textBlock = new TextBlock
                {
                    Text = item.TranslatedText,
                    Foreground = Brushes.LightGreen,
                    FontSize = Math.Max(11, item.Region.Height * 0.75),
                    FontWeight = FontWeights.SemiBold,
                    TextWrapping = TextWrapping.Wrap
                };

                border.Child = textBlock;

                Canvas.SetLeft(border, item.Region.X);
                Canvas.SetTop(border, item.Region.Y);
                border.MaxWidth = Math.Max(item.Region.Width * 1.5, 150);

                OverlayCanvas.Children.Add(border);
            }
        }
    }
}
