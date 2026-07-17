using System.Text;

namespace DS.IHF.Services;

public static class LogService
{
    private static readonly string LogDirectory =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DS-Versand", "logs");

    private static string LogFilePath =>
        Path.Combine(LogDirectory, $"ds-ihf-{DateTime.Now:yyyy-MM-dd}.log");

    private static readonly object _lock = new();

    public static void Write(string message)
    {
        try
        {
            if (!Directory.Exists(LogDirectory))
                Directory.CreateDirectory(LogDirectory);

            var line = $"[{DateTime.Now:HH:mm:ss}] {message}";
            lock (_lock)
                File.AppendAllText(LogFilePath, line + Environment.NewLine, Encoding.UTF8);
        }
        catch
        {
            // Logging darf nie die App zum Absturz bringen
        }
    }
}
