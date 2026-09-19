using UnityEngine;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif
// Note: System.Diagnostics is NOT imported to avoid ambiguity with UnityEngine.Debug
// We use fully qualified names (System.Diagnostics.Process) when needed for Linux

/// <summary>
/// Universal URL opener utility class
/// Provides cross-platform URL opening functionality
/// Works on all platforms: WebGL, Windows, Mac, Linux, Android, iOS
/// Can be called from any script without requiring a MonoBehaviour instance
/// </summary>
public static class UniversalURL
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void OpenNewTab(string url);
#endif

    /// <summary>
    /// Opens a URL in the default browser
    /// Platform-specific implementation:
    /// - WebGL: Uses JSlib OpenNewTab function
    /// - Linux: Uses xdg-open command (more reliable)
    /// - Other platforms: Uses Application.OpenURL
    /// </summary>
    /// <param name="url">The URL to open (e.g., "https://example.com")</param>
    public static void Open(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            UnityEngine.Debug.LogWarning("UniversalURL: Cannot open null or empty URL.");
            return;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL: Use modern JSlib API (OpenNewTab function in UniversalURLWebGL.jslib)
        // This uses UTF8ToString instead of deprecated Pointer_stringify
        OpenNewTab(url);
        UnityEngine.Debug.Log($"UniversalURL: WebGL - Opening URL using JSlib: {url}");
#elif UNITY_STANDALONE_LINUX && !UNITY_EDITOR
        // Linux: Try multiple methods to open URL
        // WSL (Windows Subsystem for Linux) requires special handling
        bool urlOpened = false;
        
        // Method 1: Try wslview (WSL-specific, opens URL in Windows default browser)
        try
        {
            System.Diagnostics.ProcessStartInfo wslviewInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "wslview",
                Arguments = url,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            
            System.Diagnostics.Process wslviewProcess = null;
            try
            {
                wslviewProcess = System.Diagnostics.Process.Start(wslviewInfo);
            }
            catch (System.Exception startEx)
            {
                // Process.Start() itself failed
                UnityEngine.Debug.Log($"UniversalURL: Linux - wslview Process.Start() failed: {startEx.Message}");
            }
            
            if (wslviewProcess != null)
            {
                try
                {
                    wslviewProcess.WaitForExit(1000); // Wait max 1 second
                    if (wslviewProcess.ExitCode == 0)
                    {
                        UnityEngine.Debug.Log($"UniversalURL: Linux (WSL) - Successfully opened URL using wslview: {url}");
                        urlOpened = true;
                    }
                    else
                    {
                        UnityEngine.Debug.Log($"UniversalURL: Linux (WSL) - wslview exited with code: {wslviewProcess.ExitCode}");
                    }
                }
                catch (System.Exception waitEx)
                {
                    // WaitForExit failed, but process may have started
                    UnityEngine.Debug.Log($"UniversalURL: Linux (WSL) - wslview WaitForExit exception (process may have started): {waitEx.Message}");
                    // If process started, consider it success
                    urlOpened = true;
                }
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.Log($"UniversalURL: Linux - wslview exception: {e.Message}");
        }
        
        // Method 2: If wslview failed, try xdg-open (works on native Linux and WSL)
        if (!urlOpened)
        {
            try
            {
                // First check if xdg-open exists
                System.Diagnostics.ProcessStartInfo checkInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "which",
                    Arguments = "xdg-open",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                
                using (System.Diagnostics.Process checkProcess = System.Diagnostics.Process.Start(checkInfo))
                {
                    if (checkProcess != null)
                    {
                        checkProcess.WaitForExit(500); // Wait max 500ms
                        if (checkProcess.ExitCode != 0)
                        {
                            UnityEngine.Debug.Log($"UniversalURL: Linux - xdg-open not found in PATH");
                        }
                    }
                }
                
                // Try to start xdg-open
                System.Diagnostics.ProcessStartInfo xdgInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "xdg-open",
                    Arguments = url,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                
                System.Diagnostics.Process xdgProcess = null;
                try
                {
                    xdgProcess = System.Diagnostics.Process.Start(xdgInfo);
                }
                catch (System.Exception startEx)
                {
                    // Process.Start() itself failed
                    UnityEngine.Debug.LogWarning($"UniversalURL: Linux - xdg-open Process.Start() failed: {startEx.Message}");
                }
                
                if (xdgProcess != null)
                {
                    // Process started successfully
                    // Don't wait for exit - xdg-open may not exit immediately
                    // Just check if process started successfully
                    UnityEngine.Debug.Log($"UniversalURL: Linux - Successfully started xdg-open for URL: {url}");
                    urlOpened = true;
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"UniversalURL: Linux - xdg-open process failed to start (returned null)");
                }
            }
            catch (System.Exception e)
            {
                // If we get here, it means process started but something else failed
                // Check if process was actually started before the exception
                UnityEngine.Debug.LogWarning($"UniversalURL: Linux - xdg-open exception (but process may have started): {e.Message}");
                UnityEngine.Debug.LogWarning($"UniversalURL: Linux - Exception type: {e.GetType().Name}");
                // Don't set urlOpened = true here, as we can't verify if process started
            }
        }
        
        // Method 3: If both failed, try PowerShell (WSL-specific, more reliable than cmd.exe)
        if (!urlOpened)
        {
            try
            {
                // Check if we're in WSL by checking for /mnt/ directory (WSL mounts Windows drives here)
                if (System.IO.Directory.Exists("/mnt/c"))
                {
                    // We're in WSL, use PowerShell to open URL (more reliable than cmd.exe)
                    System.Diagnostics.ProcessStartInfo psInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "/mnt/c/Windows/System32/WindowsPowerShell/v1.0/powershell.exe",
                        Arguments = $"-Command \"Start-Process '{url}'\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };
                    
                    System.Diagnostics.Process psProcess = null;
                    try
                    {
                        psProcess = System.Diagnostics.Process.Start(psInfo);
                    }
                    catch (System.Exception startEx)
                    {
                        // Process.Start() itself failed
                        UnityEngine.Debug.LogWarning($"UniversalURL: Linux (WSL) - PowerShell Process.Start() failed: {startEx.Message}");
                    }
                    
                    if (psProcess != null)
                    {
                        UnityEngine.Debug.Log($"UniversalURL: Linux (WSL) - Successfully started PowerShell for URL: {url}");
                        urlOpened = true;
                    }
                }
            }
            catch (System.Exception e)
            {
                // If we get here, it means something else failed
                UnityEngine.Debug.Log($"UniversalURL: Linux (WSL) - PowerShell method exception: {e.Message}");
            }
        }
        
        // Method 4: Fallback to cmd.exe if PowerShell failed
        if (!urlOpened)
        {
            try
            {
                if (System.IO.Directory.Exists("/mnt/c"))
                {
                    // We're in WSL, use Windows cmd.exe to open URL
                    System.Diagnostics.ProcessStartInfo cmdInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "/mnt/c/Windows/System32/cmd.exe",
                        Arguments = $"/c start \"\" \"{url}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };
                    
                    System.Diagnostics.Process cmdProcess = null;
                    try
                    {
                        cmdProcess = System.Diagnostics.Process.Start(cmdInfo);
                    }
                    catch (System.Exception startEx)
                    {
                        // Process.Start() itself failed
                        UnityEngine.Debug.LogWarning($"UniversalURL: Linux (WSL) - cmd.exe Process.Start() failed: {startEx.Message}");
                    }
                    
                    if (cmdProcess != null)
                    {
                        UnityEngine.Debug.Log($"UniversalURL: Linux (WSL) - Successfully started cmd.exe for URL: {url}");
                        urlOpened = true;
                    }
                }
            }
            catch (System.Exception e)
            {
                // If we get here, it means something else failed
                UnityEngine.Debug.LogWarning($"UniversalURL: Linux (WSL) - cmd.exe method exception: {e.Message}");
            }
        }
        
        // Fallback: Use Application.OpenURL as last resort
        if (!urlOpened)
        {
            UnityEngine.Debug.Log($"UniversalURL: Linux - Direct process methods unavailable, using Application.OpenURL: {url}");
            UnityEngine.Debug.Log($"UniversalURL: Linux - Note: Application.OpenURL may work even if process methods failed");
            try
            {
                Application.OpenURL(url);
                UnityEngine.Debug.Log($"UniversalURL: Linux - Application.OpenURL called successfully");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"UniversalURL: Linux - Application.OpenURL failed: {e.Message}");
            }
        }
        else
        {
            UnityEngine.Debug.Log($"UniversalURL: Linux - URL opened successfully using process method");
        }
#else
        // Windows, Mac, Android, iOS: Use Unity's Application.OpenURL
        Application.OpenURL(url);
        UnityEngine.Debug.Log($"UniversalURL: Opening URL: {url} (Platform: {Application.platform})");
#endif
    }

    /// <summary>
    /// Opens a URL in the default browser with automatic protocol detection
    /// Automatically adds "https://" if no protocol is specified
    /// </summary>
    /// <param name="url">The URL to open (e.g., "example.com" or "https://example.com")</param>
    public static void OpenURL(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            UnityEngine.Debug.LogWarning("UniversalURL: Cannot open null or empty URL.");
            return;
        }

        // Ensure URL has protocol
        if (!url.StartsWith("http://") && !url.StartsWith("https://") && !url.StartsWith("mailto:"))
        {
            url = "https://" + url;
        }

        Open(url);
    }
}
