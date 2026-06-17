namespace MyInstallManager;

using System.Diagnostics;

class SelfUpdater
{
    private SelfUpdater()
    {
    }

    public static async Task<bool> SelfUpdateIfNeeded(HttpClient client, string installationDir, Manifest originalManifest, Manifest latestManifest) {
        // TODO compare versions of the two, return false if we're newer (?)

        string? url = latestManifest.SelfUpdaterURL;
        if (url == null) {
            return false;
        }

        // Create an updated executable in a temporary file
        string temp = Path.GetTempFileName();
        using (FileStream file = File.Open(temp, FileMode.Open, FileAccess.Write, FileShare.Read)) {
            await using (Stream executable = await client.GetStreamAsync(url)) {
                await executable.CopyToAsync(file);
            }
        }

        // Exec the new executable
        Process process = Process.Start(temp, new string[] { "update", installationDir, "--no-self-update" });

        // Wait for new process to exit
        await process.WaitForExitAsync();

        Console.WriteLine($"Subprocess finished! Exit code: {process.ExitCode}");
        if (process.ExitCode == 0) {
            // The update was successful, replace ourselves. (This doesn't
            // follow the original plan, I just don't want to wire up another
            // command-line argument and I think it still makes sense.)
            Console.WriteLine($"Updater seems to be at {Environment.ProcessPath}");
            string toRemove = Path.GetTempFileName();
            File.Move(temp, Environment.ProcessPath, true);
        } else {
            File.Delete(temp);
        }

        return true;
    }
}
