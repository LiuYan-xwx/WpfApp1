using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.Media.MediaFoundation;

namespace WpfApp1;

internal static class NativeMethodsRaw
{
    [DllImport("MFSENSORGROUP.dll", ExactSpelling = true)]
    internal static extern HRESULT MFCreateSensorActivityMonitor(
        IntPtr pCallback,
        out IMFSensorActivityMonitor ppActivityMonitor);
}