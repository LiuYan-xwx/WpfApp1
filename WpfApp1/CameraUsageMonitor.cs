using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Media.MediaFoundation;
using WpfApp1.Interop;

namespace WpfApp1;

public sealed class CameraUsageMonitor : IDisposable
{
    private const int MF_VERSION = 0x00020070;

    private readonly string _deviceId;
    private readonly SensorActivityCallback _callback;

    private IMFSensorActivityMonitor? _monitor;
    private bool _started;

    public bool IsInUse => _callback.IsInUse;

    public event Action<bool>? UsageChanged
    {
        add => _callback.UsageChanged += value;
        remove => _callback.UsageChanged -= value;
    }

    public CameraUsageMonitor(string deviceId)
    {
        _deviceId = deviceId;
        _callback = new SensorActivityCallback(_deviceId);
    }

    public void Start()
    {
        if (_started)
            return;

        PInvoke.MFStartup(MF_VERSION, 0);

        IntPtr callbackPtr = IntPtr.Zero;

        try
        {
            callbackPtr = Marshal.GetComInterfaceForObject(
                _callback,
                typeof(IManagedSensorActivitiesReportCallback));

            var hr = NativeMethodsRaw.MFCreateSensorActivityMonitor(callbackPtr, out _monitor);
            hr.ThrowOnFailure();

            _monitor.Start();
            _started = true;
        }
        finally
        {
            if (callbackPtr != IntPtr.Zero)
            {
                Marshal.Release(callbackPtr);
            }
        }
    }

    public void Stop()
    {
        if (!_started)
            return;

        try
        {
            _monitor?.Stop();
        }
        catch
        {
        }

        try
        {
            if (_monitor is IMFShutdown shutdown)
            {
                shutdown.Shutdown();
            }
        }
        catch
        {
        }

        if (_monitor is not null)
        {
            Marshal.ReleaseComObject(_monitor);
            _monitor = null;
        }

        PInvoke.MFShutdown();
        _started = false;
    }

    public void Dispose()
    {
        Stop();
    }
}