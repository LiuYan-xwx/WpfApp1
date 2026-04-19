using Windows.Devices.Enumeration;

namespace WpfApp1;

public static class CameraDeviceHelper
{
    public static async Task<IReadOnlyList<DeviceInformation>> GetAllCamerasAsync()
    {
        var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
        return devices;
    }

    public static async Task<string?> GetFirstCameraIdAsync()
    {
        var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
        return devices.Count > 0 ? devices[0].Id : null;
    }
}