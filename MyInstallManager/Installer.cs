namespace MyInstallManager;
// Class for installing a program
using System.IO.Compression;


public class Installer
{
    private string manifestURL;
    private HttpClient httpClient;

    private string installDirectory;
    public Installer(string manifestURL, string installDirectory)
    {
        this.manifestURL = manifestURL;
        this.installDirectory = installDirectory;
        httpClient = new();
    }
    public async Task Install()
    {
        Console.WriteLine("INSTALLING!!!!");
        // Fetch and parse the manifest
        string manifestStr;
        if (manifestURL.StartsWith("http"))
        {
            manifestStr = await httpClient.GetStringAsync(manifestURL);
        }
        else
        {
            manifestStr = await File.ReadAllTextAsync(manifestURL);
        }
        Manifest manifest = Manifest.ParseManifest(manifestStr);
        string? filename = Path.GetFileName(manifest.InstallURL);
        if (filename == null)
        {
            throw new Exception("Can't find filename in install URL");
        }
        if (!Directory.Exists(installDirectory))
        {
            Directory.CreateDirectory(installDirectory);
        }
        string packageDownloadPath = Path.Combine(installDirectory, filename);

        // Download the package archive
        using (FileStream fileStream = File.Open(packageDownloadPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
        {
            using (Stream httpStream = await httpClient.GetStreamAsync(manifest.InstallURL))
            {
                await httpStream.CopyToAsync(fileStream);
            }
        }
        Console.WriteLine("Finished downloading");

        // Extract the package files from the package archive.
        // TODO: make this do validation. See https://learn.microsoft.com/en-us/dotnet/standard/io/how-to-compress-and-extract-files
       //await ZipFile.ExtractToDirectoryAsync(packageDownloadPath, installDirectory);
        ExtractFromZipfileToDir(packageDownloadPath, installDirectory);
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

    public void ExtractFromZipfileToDir(string zipPath, string extractPath)
    {
        char[] delimiterChars = ['/', '\\'];

        using (ZipArchive archive = ZipFile.OpenRead(zipPath))
        {
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                List<string> components = new(entry.FullName.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
                foreach (string s in components) {
                    Console.WriteLine($"INITCOMP: {s}");
                }
                if (components.Count < 2)
                {
                    continue;
                }
                foreach (string s in components) {
                    Console.WriteLine($"COMP: {s}");
                }
                components.RemoveAt(0);
                components.Insert(0, extractPath);
                string destinationPath = Path.Combine(components.ToArray());
                // Gets the full path to ensure that relative segments are removed.
                //string destinationPath = Path.GetFullPath(Path.Combine(extractPath, entry.Name));
                Console.WriteLine($"This is destination for the file {entry.FullName}: {destinationPath}");
                // Ordinal match is safest, case-sensitive volumes can be mounted within volumes that
                // are case-insensitive.
                if (destinationPath.StartsWith(extractPath, StringComparison.Ordinal))
                    entry.ExtractToFile(destinationPath);
            }
        }
    }
}

