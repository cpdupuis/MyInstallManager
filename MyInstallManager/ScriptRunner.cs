using System.Management.Automation;

class ScriptRunner
{
    public static List<string> RunPowershellScript(string filename)
    {
        using (PowerShell ps = PowerShell.Create())
        {
            // Use AddScript to run a block of code or a file path
            ps.AddScript(filename);

            // Use AddParameter if your script expects inputs
            // ps.AddParameter("ParamName", "Value");

            var results = ps.Invoke();
            List<string> resultsList = new();
            foreach (var result in results)
            {
                resultsList.Add(result.ToString());
            }
            return resultsList;
        }
    }
}