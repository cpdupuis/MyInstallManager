using System.Management.Automation;

public class ScriptRunner
{
    public static List<string> RunPowershellScript(string filename)
    {
        string script = File.ReadAllText(filename);
        using (PowerShell ps = PowerShell.Create())
        {
            var results = ps.AddScript(script).Invoke();
            List<string> resultsList = new();
            foreach (var result in results)
            {
                resultsList.Add(result.ToString());
            }
            return resultsList;
        }
    }
}