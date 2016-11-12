namespace ReamQuery.Core.Helpers
{
    using System.Runtime.InteropServices;

    public static class PlatformHelper
    {
        public static bool IsWindows;
        public static bool IsLinux;

        static PlatformHelper()
        {
            IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        }
    }
}