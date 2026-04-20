using System.ComponentModel;
using System.Windows;
using WpfApp1.ViewModels;

namespace WpfApp1;

public partial class MainWindow : Window
{
    private MainViewModel ViewModel => (MainViewModel)DataContext;

    public MainWindow()
    {
        InitializeComponent();

        DataContext = new MainViewModel();

        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.InitializeAsync();
    }

    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        ViewModel.Dispose();
    }
}