using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using System;

namespace YoutubeConverter.Services
{
    internal class DialogService
    {
        private readonly Window _window;
        private ContentDialog _loadingDialog;
        private TextBlock _statusTextBlock;

        public DialogService(Window window)
        {
            _window = window;
        }

        /// <summary>
        /// Shows a new custom dialog window.
        /// </summary>
        public void ShowMessage(string message, string title)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = _window.Content.XamlRoot
            };

            _ = dialog.ShowAsync();
        }

        /// <summary>
        /// Shows a successful dialog message with a richTextBlock for better formatting.
        /// </summary>
        public void ShowSuccessfulMessage(string message, string fileName)
        {
            var richTextBlock = new RichTextBlock();

            var normalText = new Paragraph
            {
                Inlines = { new Run { Text = message } }
            };

            var boldText = new Paragraph
            {
                Inlines = { new Run { Text = fileName, FontWeight = FontWeights.Bold } }
            };

            richTextBlock.Blocks.Add(normalText);
            richTextBlock.Blocks.Add(boldText);

            var dialog = new ContentDialog
            {
                Title = "Sukces",
                Content = richTextBlock,
                CloseButtonText = "OK",
                XamlRoot = _window.Content.XamlRoot
            };

            _ = dialog.ShowAsync();
        }

        /// <summary>
        /// Shows dialog with a loading ring.
        /// </summary>
        public async void ShowLoadingDialogAsync()
        {
            var progressRing = new ProgressRing
            {
                IsActive = true,
                Margin = new Thickness(10),
                Width = 60,
                Height = 60,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            _statusTextBlock = new TextBlock
            {
                Text = "Konwertowanie...",
                FontSize = 16,
                Margin = new Thickness(10),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            stackPanel.Children.Add(progressRing);
            stackPanel.Children.Add(_statusTextBlock);

            _loadingDialog = new ContentDialog
            {
                Title = "Konwertowanie...",
                Content = stackPanel,
                XamlRoot = _window.Content.XamlRoot
            };

            await _loadingDialog.ShowAsync();
        }

        /// <summary>
        /// Updates a textBlock that is in the loading dialog.
        /// </summary>
        public void UpdateStatus(string message)
        {
            _statusTextBlock.Text = message;
        }

        /// <summary>
        /// Closes loading dialog.
        /// </summary>
        public void CloseLoadingDialog()
        {
            _loadingDialog.Hide();
            _loadingDialog = null;
        }
    }
}