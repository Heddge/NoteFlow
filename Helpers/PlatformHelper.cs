using System;
using System.Runtime.InteropServices;

namespace NoteFlow.Helpers
{
    public static class PlatformHelper
    {
        public static bool IsMacOS 
        { 
            get 
            {
                try
                {
                    bool isOSX = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
                    
                    // Альтернативные проверки
                    var osDesc = RuntimeInformation.OSDescription ?? "";
                    var isDarwin = osDesc.ToLower().Contains("darwin");
                    var isMacInDescription = osDesc.ToLower().Contains("mac");
                    
                    return isOSX || isDarwin || isMacInDescription;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }
        
        public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        
        // Для отладки
        public static string OSInfo => $"OS: {RuntimeInformation.OSDescription}, Architecture: {RuntimeInformation.OSArchitecture}, Framework: {RuntimeInformation.FrameworkDescription}";
    }
}