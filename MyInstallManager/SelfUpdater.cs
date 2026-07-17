namespace MyInstallManager;

using System.Diagnostics;

class SelfUpdater
{
    private SelfUpdater()
    {
    }

    public static async Task<bool> SelfUpdateIfNeeded(HttpClient client, string installationDir, Manifest originalManifest, Manifest latestManifest, Locker locker) {
        // TODO compare versions of the two, return false if we're newer (?)

        string? url = latestManifest.SelfUpdaterURL;
        if (url == null) {
            return false;
        }

        // Create an updated executable in a temporary file. This temporary
        // file needs to be on the same filesystem, or else we won't be able to
        // replace the running executable.
        string currentExe = Environment.ProcessPath;
        string downloadTo = currentExe + ".replacement";
        using (FileStream file = File.Open(downloadTo, FileMode.Create, FileAccess.Write, FileShare.Read)) {
            await using (Stream executable = await client.GetStreamAsync(url)) {
                await executable.CopyToAsync(file);
            }
        }

        // Exec the new executable
        Process process = Process.Start(downloadTo, new string[] { "update", "--dir", installationDir, "--no-self-update" });

        await locker.WithoutLock(async (_locker) => {
            // Wait for new process to exit
            await process.WaitForExitAsync();
        });

        Console.WriteLine($"Subprocess finished! Exit code: {process.ExitCode}");
        if (process.ExitCode == 0) {
            // The update was successful, replace ourselves. (This doesn't
            // follow the original plan, I just don't want to wire up another
            // command-line argument and I think it still makes sense.)
            Console.WriteLine($"Updater seems to be at {currentExe}");
            File.Replace(downloadTo, currentExe, null);
        } else {
            // Something went wrong, don't trust it.
            File.Delete(downloadTo);
        }

        return true;
    }
}
