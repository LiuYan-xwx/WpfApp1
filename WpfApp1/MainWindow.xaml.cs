using System.ComponentModel;
using System.Windows;

namespace WpfApp1;

public partial class MainWindow : Window
{
    private CameraUsageMonitor? _monitor;

    public MainWindow()
    {
        InitializeComponent();

        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var cameras = await CameraDeviceHelper.GetAllCamerasAsync();
            var cameraId = await CameraDeviceHelper.GetFirstCameraIdAsync();

            if (string.IsNullOrWhiteSpace(cameraId))
            {
                StatusText.Text = "没找到摄像头";
                return;
            }

            if (cameras.Count > 0)
            {
                CameraText.Text = $"当前摄像头：{cameras[0].Name}";
            }

            _monitor = new CameraUsageMonitor(cameraId);
            _monitor.UsageChanged += OnUsageChanged;
            _monitor.Start();

            StatusText.Text = _monitor.IsInUse ? "占用中" : "空闲";
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "启动失败");
        }
    }

    private void OnUsageChanged(bool inUse)
    {
        Dispatcher.Invoke(() =>
        {
            StatusText.Text = inUse ? "占用中" : "空闲";
        });
    }

    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        if (_monitor is not null)
        {
            _monitor.UsageChanged -= OnUsageChanged;
            _monitor.Dispose();
            _monitor = null;
        }
    }
}