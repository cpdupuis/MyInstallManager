namespace MyInstallManager;
// Class for installing a program
using System.IO.Compression;


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

    public async Task InstallFromZipStream(Stream zipfileStream, string installDirectory, Manifest manifest)
    {
        // If it already exists, this is a no-op.
        // (TODO: we should probably reject installing if it does exist)
        Directory.CreateDirectory(installDirectory);

        ExtractFromZipStreamToDir(zipfileStream, installDirectory);
        Console.WriteLine("Finished extracting");
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
                if (destinationPath.StartsWith(extractPath, StringComparison.Ordinal))
                    entry.ExtractToFile(destinationPath);

    }
}

