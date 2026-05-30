namespace MyInstallManager;
using System.Text.Json.Nodes;

// Class to represent the contents of a manifest.json file
public class Manifest
{
    public string InstallURL { get; set;}
    public static Manifest ParseManifest(string str)
    {
        JsonNode? node = JsonNode.Parse(str);
        if (node == null)
        {
            throw new Exception("Can't parse manifest");
        }
        return new Manifest(node);
    }

    private Manifest(JsonNode node)
    {
        JsonNode? installURLNode = node["installURL"];
        if (installURLNode == null)
        {
            throw new Exception("No install URL");
        }
        this.InstallURL = installURLNode.GetValue<string>();
        
    } 
}