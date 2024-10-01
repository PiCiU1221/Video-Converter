using Microsoft.UI.Xaml;
using YoutubeConverter.Services;
using YoutubeConverter.ViewModels;

namespace YoutubeConverter
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            DialogService dialogService = new DialogService(this);
            ConverterService converterService = new ConverterService(dialogService);
            MainGrid.DataContext = new MainWindowViewModel(converterService);
        }
    }
}