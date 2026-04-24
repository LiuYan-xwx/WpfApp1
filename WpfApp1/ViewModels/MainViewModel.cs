using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WpfApp1.Models;
using WpfApp1.Services;

namespace WpfApp1.ViewModels;

public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly ICameraDetectionService _cameraService;
    private bool _suppressSelectionChanged;

    public ObservableCollection<CameraDeviceInfo> Cameras { get; } = new();

    [ObservableProperty]
    private string statusText = "初始化中...";

    [ObservableProperty]
    private string cameraText = "";

    [ObservableProperty]
    private bool isLoading;

    private CameraDeviceInfo? _selectedCamera;
    public CameraDeviceInfo? SelectedCamera
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

    public MainViewModel() : this(new CameraDetectionService())
    {
    }

    public MainViewModel(ICameraDetectionService cameraService)
    {
        _cameraService = cameraService;
        _cameraService.UsageChanged += OnUsageChanged;
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

            var devices = await _cameraService.GetAllCamerasAsync();
            var previousId = SelectedCamera?.Id;

            _suppressSelectionChanged = true;
            Cameras.Clear();

            foreach (var device in devices)
            {
                Cameras.Add(device);
            }

            if (Cameras.Count == 0)
            {
                SelectedCamera = null;
                _cameraService.StopMonitoring();
                StatusText = "没找到摄像头";
                CameraText = "";
                return;
            }

            SelectedCamera = Cameras.FirstOrDefault(x => x.Id == previousId) ?? Cameras[0];
        }
        catch (Exception ex)
        {
            _cameraService.StopMonitoring();
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

    private async Task SwitchCameraAsync(CameraDeviceInfo camera)
    {
        if (_cameraService.CurrentCameraId == camera.Id && _cameraService.IsMonitoring)
        {
            CameraText = $"当前摄像头：{camera.Name}";
            StatusText = _cameraService.IsInUse ? "占用中" : "空闲";
            return;
        }

        try
        {
            _cameraService.StartMonitoring(camera.Id);
            CameraText = $"当前摄像头：{camera.Name}";
            StatusText = _cameraService.IsInUse ? "占用中" : "空闲";
        }
        catch (Exception ex)
        {
            _cameraService.StopMonitoring();
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

    public void Dispose()
    {
        _cameraService.UsageChanged -= OnUsageChanged;
        _cameraService.Dispose();
    }
}