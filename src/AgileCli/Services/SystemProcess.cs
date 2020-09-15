using System.Diagnostics;

namespace AgileCli.Services
{
    internal class SystemProcess : ISystemProcess
    {
        public void Start(string filePath)
        {
            Process.Start(new ProcessStartInfo(filePath)
            {
                UseShellExecute = true
            });
        }
    }
}