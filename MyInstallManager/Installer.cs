namespace MyInstallManager;
// Class for installing a program
using System.IO.Compression;


class Installer
{
    private string manifestURL;
    private HttpClient httpClient;
    
    private string downloadFilename = "package.whatever";
    private string installDirectory = @"D:\scratch\InstallerTest123\";
    public Installer(string manifestURL)
    {
        this.manifestURL = manifestURL;
        httpClient = new();
    }
    public async Task Install()
    {
        // Fetch and parse the manifest
        string manifestStr = await httpClient.GetStringAsync(manifestURL);
        Manifest manifest = Manifest.ParseManifest(manifestStr);
        
        // Download the package archive
        using (FileStream fileStream = File.Open(downloadFilename, FileMode.CreateNew, FileAccess.Read, FileShare.None))
        {
            using (Stream httpStream = await httpClient.GetStreamAsync(manifest.PackageURL())) 
            {
                await httpStream.CopyToAsync(fileStream);
            }
        }

        // Extract the package files from the package archive.
        // TODO: make this do validation. See https://learn.microsoft.com/en-us/dotnet/standard/io/how-to-compress-and-extract-files
        await ZipFile.ExtractToDirectoryAsync(downloadFilename, installDirectory);

        // All the files are in place. Now run the on-install script
        string onInstallScript = Path.Combine(installDirectory, "OnInstall.ps1");
        if (Path.Exists(onInstallScript))
        {
            List<string> results = ScriptRunner.RunPowershellScript(onInstallScript);
            foreach (var res in results)
            {
                Console.WriteLine("Install script result: " + res);
            }
        }
    }
}
