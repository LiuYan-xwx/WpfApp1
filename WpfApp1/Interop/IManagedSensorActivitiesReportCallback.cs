using System.Runtime.InteropServices;

namespace WpfApp1.Interop;

[ComVisible(true)]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("DE5072EE-DBE3-46DC-8A87-B6F631194751")]
public interface IManagedSensorActivitiesReportCallback
{
    void OnActivitiesReport(IntPtr sensorActivitiesReport);
}