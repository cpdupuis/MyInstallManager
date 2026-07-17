namespace MyInstallManager;
// Class for installing a program
using System.IO.Compression;
using System.Text;

public class Installer
{
    private  char[] delimiterChars = ['/', '\\'];

    public async Task InstallFromUrl(string manifestURL, string installDirectory)
    {
        HttpClient httpClient = new();
        Console.WriteLine("INSTALLING!!!!");
        // Fetch and parse the manifest
        Manifest manifest;
        await using (Stream manifestStream = await httpClient.GetStreamAsync(manifestURL)) {
            manifest = Manifest.ParseManifest(manifestStream);
        }

        // Download the package archive
        using (Stream httpStream = await httpClient.GetStreamAsync(manifest.InstallURL))
        {
            await InstallFromZipStream(httpStream, installDirectory, manifest);
        }
    }

    public async Task InstallFromZipStream(Stream zipfileStream, string installDirectory, Manifest manifest, bool isUpdate = false)
    {
        // If it already exists, this is a no-op.
        // (TODO: we should probably reject installing if it does exist)
        Directory.CreateDirectory(installDirectory);

        string manifestPath = Path.Combine(installDirectory, "update-manifest.json");
        using (Stream fileStream = File.Open(manifestPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None)) {
            byte[] encoded = new UTF8Encoding(true).GetBytes(manifest.Serialize());
            fileStream.Write(encoded, 0, encoded.Length);
        }

        ExtractFromZipStreamToDir(zipfileStream, installDirectory);
        Console.WriteLine("Finished extracting");

        // Install the updater. (HACKY: this only really works for the stub
        // installer. The full installer will need some other way to bundle it,
        // I'd have to talk to Chris about that.)
        if (!isUpdate) {
            string updaterPath = Path.Combine(installDirectory, "updater.exe");
            using (Stream fileStream = File.Open(updaterPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None)) {
                await using (Stream updaterStream = await new HttpClient().GetStreamAsync(manifest.SelfUpdaterURL!)) {
                    await updaterStream.CopyToAsync(fileStream);
                }
            }
        }

        // All the files are in place. Now run the on-install script
        string onInstallScript = Path.Combine(installDirectory, "OnInstall.ps1");
        Console.WriteLine($"Looking for install script {onInstallScript}");
        if (Path.Exists(onInstallScript))
        {
            Console.WriteLine("Script exxists");
            List<string> results = ScriptRunner.RunPowershellScript(onInstallScript);
            foreach (var res in results)
            {
                Console.WriteLine("Install script result: " + res);
            }
        }
        else
        {
            Console.WriteLine("No such script");
        }
    }

    public void ExtractFromZipStreamToDir(Stream zipStream, string extractPath)
    {
        using (ZipArchive archive = new ZipArchive(zipStream))
        {
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                Console.WriteLine($"Extracting an entry to {extractPath}: {entry.FullName}");
                ExtractOneEntry(entry, extractPath);
            }
        }
    }

    private void ExtractOneEntry(ZipArchiveEntry entry, string extractPath)
    {
                List<string> components = new(entry.FullName.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
                foreach (string s in components) {
                    Console.WriteLine($"INITCOMP: {s}");
                }
                if (components.Count < 2)
                {
                    return;
                }
                foreach (string s in components) {
                    Console.WriteLine($"COMP: {s}");
                }
                components.RemoveAt(0);
                components.Insert(0, extractPath);
                string destinationPath = Path.Combine(components.ToArray());
                string? destinationFolder = Path.GetDirectoryName(destinationPath);
                if (destinationFolder != null && !Path.Exists(destinationFolder)) {
                    Directory.CreateDirectory(destinationFolder);
                }
                // Gets the full path to ensure that relative segments are removed.
                //string destinationPath = Path.GetFullPath(Path.Combine(extractPath, entry.Name));
                Console.WriteLine($"This is destination for the file {entry.FullName}: {destinationPath}");
                // Ordinal match is safest, case-sensitive volumes can be mounted within volumes that
                // are case-insensitive.
                File.Delete(destinationPath);
                if (destinationPath.StartsWith(extractPath, StringComparison.Ordinal))
                    entry.ExtractToFile(destinationPath);

    }
}

