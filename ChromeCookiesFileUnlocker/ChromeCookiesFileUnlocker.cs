using System.Runtime.InteropServices;

public class FileUnlockerException(string message) : Exception(message);

public static partial class ChromeCookiesFileUnlocker
{
    #region CONSTANTS

    private const int ERROR_SUCCESS = 0;
    private const int ERROR_MORE_DATA = 234;
    private const int RM_FORCE_SHUTDOWN = 1;
    private const string RSTRTMGR_DLL = "rstrtmgr.dll";

    #endregion

    #region LIB IMPORT

    [LibraryImport(RSTRTMGR_DLL, StringMarshalling = StringMarshalling.Utf16)]
    private static partial int RmStartSession(out uint pSessionHandle, int dwSessionFlags, string strSessionKey);

    [LibraryImport(RSTRTMGR_DLL)]
    private static partial int RmEndSession(uint pSessionHandle);

    [LibraryImport(RSTRTMGR_DLL, StringMarshalling = StringMarshalling.Utf16)]
    private static partial int RmRegisterResources(uint pSessionHandle, uint nFiles, string[] rgsFilenames, uint nApplications, string[] rgApplications, uint nServices, string[] rgsServiceNames);

    [LibraryImport(RSTRTMGR_DLL)]
    private static partial int RmShutdown(uint pSessionHandle, int lAction, RM_WRITE_STATUS fnStatus);

    #endregion

    public static event Action<uint> ProgressChanged;
    private delegate void RM_WRITE_STATUS(uint nPercentComplete);
    private static readonly string SessionKey = Guid.NewGuid().ToString();

    private static void UnlockCookies(string cookiesPath)
    {
        var handle = StartSession();
        if (handle is null)
            throw new FileUnlockerException("Failed to start session.");

        try
        {
            RegisterResources(handle.Value, cookiesPath);
            ShutdownSession(handle.Value);
        }
        finally
        {
            EndSession(handle.Value);
        }
    }

    private static uint? StartSession()
    {
        var result = RmStartSession(out var handle, 0, SessionKey);
        return result is ERROR_SUCCESS ? handle : null;
    }

    private static void RegisterResources(uint handle, string cookiesPath)
    {
        string[] resources = [cookiesPath];
        var result = RmRegisterResources(handle, (uint)resources.Length, resources, 0, null, 0, null);

        switch (result)
        {
            case ERROR_SUCCESS:
                return;
            case ERROR_MORE_DATA:
                throw new FileUnlockerException("RmRegisterResources encountered ERROR_MORE_DATA. Buffer size insufficient.");
            default:
                throw new FileUnlockerException($"RmRegisterResources returned non-zero result: {result}");
        }
    }

    private static void ShutdownSession(uint handle)
    {
        var result = RmShutdown(handle, RM_FORCE_SHUTDOWN, Callback);

        if (result is not ERROR_SUCCESS)
            throw new FileUnlockerException($"RmShutdown returned non-zero result: {result}");
    }

    private static void EndSession(uint handle)
    {
        var result = RmEndSession(handle);

        if (result is not ERROR_SUCCESS)
            throw new FileUnlockerException($"RmEndSession returned non-zero result: {result}");
    }

    private static void Callback(uint percent)
    {
        ProgressChanged?.Invoke(percent);
    }

    public static void CopyFile(string cookiesPath, string outputPath)
    {
        MakeFullPath(ref cookiesPath);
        UnlockCookies(cookiesPath);

        try
        {
            File.Copy(cookiesPath, outputPath, true);
        }
        catch (Exception e)
        {
            throw new FileUnlockerException($"Failed to copy file: {e.Message}");
        }
    }

    private static void MakeFullPath(ref string sourcePath)
    {
        if (sourcePath.Contains('%'))
            sourcePath = Environment.ExpandEnvironmentVariables(sourcePath);
        else if (!Path.IsPathRooted(sourcePath))
            sourcePath = Path.Join(Environment.CurrentDirectory, sourcePath);
    }
}