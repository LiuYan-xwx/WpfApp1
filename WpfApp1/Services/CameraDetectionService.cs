using WpfApp1.Models;

namespace WpfApp1.Services;

public sealed class CameraDetectionService : ICameraDetectionService
{
    private CameraUsageMonitor? _monitor;

    public event Action<bool>? UsageChanged;

    public string? CurrentCameraId { get; private set; }
    public bool IsMonitoring => _monitor is not null;
    public bool IsInUse { get; private set; }

    public async Task<IReadOnlyList<CameraDeviceInfo>> GetAllCamerasAsync()
    {
        var devices = await CameraDeviceHelper.GetAllCamerasAsync();
        return devices
            .Select(d => new CameraDeviceInfo
            {
                Id = d.Id,
                Name = d.Name
            })
            .ToList();
    }

    public void StartMonitoring(string cameraId)
    {
        if (CurrentCameraId == cameraId && _monitor is not null)
            return;

        StopMonitoring();

        var monitor = new CameraUsageMonitor(cameraId);
        monitor.UsageChanged += OnUsageChanged;
        monitor.Start();

        _monitor = monitor;
        CurrentCameraId = cameraId;
        IsInUse = monitor.IsInUse;
    }

    public void StopMonitoring()
    {
        if (_monitor is not null)
        {
            _monitor.UsageChanged -= OnUsageChanged;
            _monitor.Dispose();
            _monitor = null;
        }

        CurrentCameraId = null;
        IsInUse = false;
    }

    private void OnUsageChanged(bool inUse)
    {
        IsInUse = inUse;
        UsageChanged?.Invoke(inUse);
    }

    public void Dispose()
    {
        StopMonitoring();
    }
}