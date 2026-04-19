using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.Media.MediaFoundation;
using WpfApp1.Interop;

namespace WpfApp1;

[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
public sealed class SensorActivityCallback : IManagedSensorActivitiesReportCallback
{
    private readonly string _deviceId;

    public bool IsInUse { get; private set; }

    public event Action<bool>? UsageChanged;

    public SensorActivityCallback(string deviceId)
    {
        _deviceId = deviceId;
    }

    public void OnActivitiesReport(IntPtr sensorActivitiesReportPtr)
    {
        IMFSensorActivitiesReport? report = null;

        try
        {
            report = Marshal.GetObjectForIUnknown(sensorActivitiesReportPtr) as IMFSensorActivitiesReport;
            if (report is null)
                return;

            bool inUse = false;

            IMFSensorActivityReport deviceReport;

            unsafe
            {
                fixed (char* symbolicName = _deviceId)
                {
                    report.GetActivityReportByDeviceName(symbolicName, out deviceReport);
                }
            }

            try
            {
                deviceReport.GetProcessCount(out uint count);

                for (uint i = 0; i < count; i++)
                {
                    deviceReport.GetProcessActivity(i, out var processActivity);

                    try
                    {
                        unsafe
                        {
                            BOOL streamingState = default;
                            processActivity.GetStreamingState(&streamingState);

                            if (streamingState)
                            {
                                inUse = true;
                                break;
                            }
                        }
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(processActivity);
                    }
                }
            }
            finally
            {
                Marshal.ReleaseComObject(deviceReport);
            }

            if (IsInUse != inUse)
            {
                IsInUse = inUse;
                UsageChanged?.Invoke(IsInUse);
            }
        }
        catch
        {
            // 第一版先不让异常炸掉回调线程
        }
        finally
        {
            if (report is not null)
            {
                Marshal.ReleaseComObject(report);
            }
        }
    }
}