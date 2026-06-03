namespace MyInstallManager;

using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Resources;

class Program
{
    private string[] args;

    static async Task<int> Main(string[] args)
    {

        Console.WriteLine("Hello there, World!");
        foreach (var name in Assembly.GetExecutingAssembly().GetManifestResourceNames())
        {
            Console.WriteLine(name);
        }
        Program prog = new Program(args);
        await prog.run();
        return 0;
    }

    public Program(string[] args)
    {
        this.args = args;
    }

    public async Task run()
    {
        Option<string> helpOption = new("--help")
        {
            Description = "Show this usage message."
        };
        RootCommand rootCommand = new("Yet another install manager")
        {
            helpOption
        };

        Command statusCommand = new("status", "Check the status");
        rootCommand.Subcommands.Add(statusCommand);

        Command updateCommand = new("update", "Update the installation");
        rootCommand.Subcommands.Add(updateCommand);

        Option<string> downloadUrlOption = new("--url")
        {
            Description = "URL of the manifest.json to install"
        };
        Option<string> installDirOption = new("--dir")
        {
            Description = "Directory for installing the application"
        };
        Command installCommand = new("install", "Install the application")
        {
            downloadUrlOption,
            installDirOption
        };
        rootCommand.Subcommands.Add(installCommand);

        statusCommand.SetAction(parsedArgs => GetStatus(parsedArgs));
        updateCommand.SetAction(parsedArgs => DoUpdate(parsedArgs));
        installCommand.SetAction(async parsedArgs =>
        {
            if (parsedArgs.Errors.Count == 0 && parsedArgs.GetValue(installDirOption) is string installDir)
            {
                await DoInstall(installDir);
            }
            else
            {
                Console.WriteLine("Required arg: --dir");
            }
        });
        rootCommand.Parse(args).Invoke();
    }

    private void GetStatus(ParseResult pr)
    {
        Console.WriteLine(UpdaterStatus.GetStatus());
    }

    private void DoUpdate(ParseResult pr)
    {
        Updater updater = new();
        updater.Update();
    }

    [RequiresAssemblyFiles("Calls System.Reflection.Assembly.GetFile(String)")]
    private async Task DoInstall(string installDir)
    {
        string? resourceName = null;
        foreach (var name in Assembly.GetExecutingAssembly().GetManifestResourceNames())
        {
            Console.WriteLine(name);
            if (name.Contains("firefox"))
            {
                resourceName = name;
            }
        }
        if (resourceName != null)
        {
        string resourceDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
        Console.WriteLine($"Resource dir: {resourceDir}");
        //ResourceManager rm = ResourceManager.CreateFileBasedResourceManager("FullInstaller", resourceDir, null);
        Console.WriteLine("Yup");

        // "MyApp.Properties.Resources" corresponds to your RootName (Namespace.ResourceFile)
        //ResourceManager rm = new ResourceManager("FullInstaller.firefox-152.0a1.en-US.win64.zip", Assembly.GetExecutingAssembly());
        //ResourceManager rm = new ResourceManager();
        //CultureInfo culture = new CultureInfo("en");
        //    var zipfileStream = rm.GetStream(resourceName, culture);
        var zipfileStream = Assembly.GetExecutingAssembly().GetFile("FullInstaller.firefox-152.0a1.en-US.win64.zip");
            if (zipfileStream != null)
            {
                Installer installer = new();
                await installer.InstallFromEmbeddedResource(zipfileStream, installDir);
                Console.WriteLine("Done");
            }
        }

    }

}
