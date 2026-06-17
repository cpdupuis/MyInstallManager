namespace MyInstallManager;


// Class for updating an installation
public class Updater
{
    public bool AllowSelfUpdate = true;

    private string installationDir;
    public Updater(string installationDir)
    {
        this.installationDir = installationDir;
    }

    public async Task Update()
    {
        Console.WriteLine("UPDATING!!!!");
        string manifestPath = Path.Combine(installationDir, "update-manifest.json");
        Manifest originalManifest;
        await using (FileStream fileStream = File.OpenRead(manifestPath)) {
            originalManifest = Manifest.ParseManifest(fileStream);
        }

        Manifest? latestManifest = null;
        HttpClient client = new();
        foreach (string updateURL in originalManifest.UpdateURLs) {
            Console.WriteLine($"Trying {updateURL}");
            try {
                // Do these one-at-a-time to avoid unneeded I/O.
                await using (Stream stream = await client.GetStreamAsync(updateURL)) {
                    latestManifest = Manifest.ParseManifest(stream);
                }
                Console.WriteLine("  OK");
            } catch (Exception e) {
                Console.WriteLine($"  failed: {e.Message}");
                continue;
            }
        }

        if (latestManifest == null) {
            // Couldn't fetch the latest update.
            return;
        }


        // TODO do the rest of the update
    }

}
