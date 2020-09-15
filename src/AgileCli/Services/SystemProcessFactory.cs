using System.Runtime.InteropServices;

namespace AgileCli.Services
{
    internal class SystemProcessFactory
    {
        public static ISystemProcess Create()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return new WindowsSystemProcess();

            return new SystemProcess();
        }
    }
}