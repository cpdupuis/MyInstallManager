namespace MyInstallManager;
using System.Text.Json;

// Class to represent the contents of a manifest.json file
class Manifest
{
    private string baseURL = "http://localhost:3000/apps/whatever";
    private string os = "win";
    private string arch = "x64";

    private string version = "0.0.2";

    private string name = "hum";

    private ISet<string> availableUpgradeVersions = new HashSet<string>(["0.0.0", "0.0.1"]);
    public Manifest()
    {
        
    }

    public string PackageURL()
    {
        string filename = $"{name}-{os}-{arch}.hum";
        return Path.Combine(baseURL, version, filename);
    }

    public string? UpdateURL(string currentVersion)
    {
        if (availableUpgradeVersions.Contains(currentVersion)) {
            string filename = $"{name}-{os}-{arch}-patch-from-{currentVersion}.hum";
            return Path.Combine(baseURL, version, filename);
        }
        else
        {
            return null;
        }

    }

    public static Manifest ParseManifest(string str)
    {
        JsonDocument doc = JsonDocument.Parse(str);
        Manifest man =  new ();

        return man;
    }
}