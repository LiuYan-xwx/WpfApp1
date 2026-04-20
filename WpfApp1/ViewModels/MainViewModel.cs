using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WpfApp1.Models;

namespace WpfApp1.ViewModels;

public partial class MainViewModel : ObservableObject, IDisposable
{
    private CameraUsageMonitor? _monitor;
    private bool _suppressSelectionChanged;
    private string? _currentCameraId;

    public ObservableCollection<CameraItem> Cameras { get; } = new();

    [ObservableProperty]
    private string statusText = "初始化中...";

    [ObservableProperty]
    private string cameraText = "";

    [ObservableProperty]
    private bool isLoading;

    private CameraItem? _selectedCamera;
    public CameraItem? SelectedCamera
    {
        get => _selectedCamera;
        set
        {
            if (SetProperty(ref _selectedCamera, value))
            {
                CameraText = value is null ? "" : $"当前摄像头：{value.Name}";

                if (!_suppressSelectionChanged && value is not null)
                {
                    _ = SwitchCameraAsync(value);
                }
            }
        }
    }

    public async Task InitializeAsync()
    {
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        try
        {
            IsLoading = true;

            var devices = await CameraDeviceHelper.GetAllCamerasAsync();
            var previousId = SelectedCamera?.Id;

            _suppressSelectionChanged = true;
            Cameras.Clear();

            foreach (var device in devices)
            {
                Cameras.Add(new CameraItem
                {
                    Id = device.Id,
                    Name = device.Name
                });
            }

            if (Cameras.Count == 0)
            {
                SelectedCamera = null;
                StopMonitor();
                StatusText = "没找到摄像头";
                CameraText = "";
                return;
            }

            SelectedCamera = Cameras.FirstOrDefault(x => x.Id == previousId) ?? Cameras[0];
        }
        catch (Exception ex)
        {
            StopMonitor();
            StatusText = "加载失败";
            CameraText = ex.Message;
        }
        finally
        {
            _suppressSelectionChanged = false;
            IsLoading = false;
        }

        if (SelectedCamera is not null)
        {
            await SwitchCameraAsync(SelectedCamera);
        }
    }

    private async Task SwitchCameraAsync(CameraItem camera)
    {
        if (_currentCameraId == camera.Id && _monitor is not null)
        {
            CameraText = $"当前摄像头：{camera.Name}";
            StatusText = _monitor.IsInUse ? "占用中" : "空闲";
            return;
        }

        try
        {
            StopMonitor();

            _monitor = new CameraUsageMonitor(camera.Id);
            _monitor.UsageChanged += OnUsageChanged;
            _monitor.Start();

            _currentCameraId = camera.Id;
            CameraText = $"当前摄像头：{camera.Name}";
            StatusText = _monitor.IsInUse ? "占用中" : "空闲";
        }
        catch (Exception ex)
        {
            StopMonitor();
            StatusText = "启动失败";
            CameraText = ex.Message;
        }

        await Task.CompletedTask;
    }

    private void OnUsageChanged(bool inUse)
    {
        var text = inUse ? "占用中" : "空闲";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            StatusText = text;
        }
        else
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                StatusText = text;
            });
        }
    }

    private void StopMonitor()
    {
        if (_monitor is not null)
        {
            _monitor.UsageChanged -= OnUsageChanged;
            _monitor.Dispose();
            _monitor = null;
        }

        _currentCameraId = null;
    }

    public void Dispose()
    {
        StopMonitor();
    }
}