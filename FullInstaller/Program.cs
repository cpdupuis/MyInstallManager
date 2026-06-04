namespace MyInstallManager;

using System.CommandLine;
using System.Reflection;

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

    private async Task DoInstall(string installDir)
    {
        const string resourceName = "sample_installation.zip";

        await using Stream? zipfileStream =
            Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        if (zipfileStream is null)
        {
            Console.WriteLine($"Embedded resource not found: {resourceName}");
            return;
        }

        Installer installer = new();
        await installer.InstallFromEmbeddedResource(zipfileStream, installDir);
        Console.WriteLine("Done");
    }

}
