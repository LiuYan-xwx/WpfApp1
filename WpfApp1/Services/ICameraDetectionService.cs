using WpfApp1.Models;

namespace WpfApp1.Services;

public interface ICameraDetectionService : IDisposable
{
    event Action<bool>? UsageChanged;

    string? CurrentCameraId { get; }
    bool IsMonitoring { get; }
    bool IsInUse { get; }

    Task<IReadOnlyList<CameraDeviceInfo>> GetAllCamerasAsync();
    void StartMonitoring(string cameraId);
    void StopMonitoring();
}