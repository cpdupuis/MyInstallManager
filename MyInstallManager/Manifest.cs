namespace MyInstallManager;
using System.Text.Json.Nodes;

// Class to represent the contents of a manifest.json file
public class Manifest
{
    private JsonNode node;

    public string InstallURL => node["installURL"]!.GetValue<string>();
    public string? SelfUpdaterURL => node["selfUpdaterURL"]?.GetValue<string>();
    public IReadOnlyList<string> UpdateURLs { get; private set; }

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

        JsonArray? updateURLs = node["updateURLs"]?.AsArray();
        // Allow updateURLs to be null in case updates aren't wanted
        List<string> validURLs = new();
        if (updateURLs is not null) {
            foreach (JsonNode child in updateURLs!) {
                string url = child.GetValue<string>();
                if (url != null) {
                    validURLs.Add(url);
                } else {
                    // TODO complain
                }
            }
        }
        this.UpdateURLs = validURLs;

        this.node = node;
    } 
}
