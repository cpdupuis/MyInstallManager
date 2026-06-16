namespace MyInstallManager;
using System.Text.Json.Nodes;

// Class to represent the contents of a manifest.json file
public class Manifest
{
    private JsonNode node;

    public string InstallURL => node["installURL"]!.GetValue<string>();

    public static Manifest ParseManifest(Stream input)
    {
        JsonNode? node = JsonNode.Parse(input);
        if (node == null)
        {
            throw new Exception("Can't parse manifest");
        }
        return new Manifest(node);
    }

    public string Serialize()
    {
        return node.ToJsonString();
    }

    private Manifest(JsonNode node)
    {
        if (node["installURL"] == null)
        {
            throw new Exception("No install URL");
        }

        this.node = node;
    } 
}
