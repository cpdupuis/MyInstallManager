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

        Option<string> noSelfUpdateOption = new("--no-self-update")
        {
            Description = "Attempts to update with this instance instead of doing a self-update first",
            Hidden = true,
        };
        Command updateCommand = new("update", "Update the installation")
        {
            installDirOption,
            noSelfUpdateOption
        };
        rootCommand.Subcommands.Add(updateCommand);

        statusCommand.SetAction(parsedArgs => GetStatus(parsedArgs));
        updateCommand.SetAction(async parsedArgs =>
        {
            if (parsedArgs.Errors.Count > 0 || parsedArgs.GetValue(installDirOption) is null)
            {
                Console.WriteLine("Required arg: --dir");
                return;
            }
            Updater updater = new(parsedArgs.GetValue(installDirOption)!);
            updater.AllowSelfUpdate = parsedArgs.GetValue(noSelfUpdateOption) == null;
            await updater.Update();
        });
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

    private async Task DoInstall(string installDir)
    {
        const string zipfileResourceName = "sample_installation.zip";
        const string manifestResourceName = "sample_installation_manifest.json";

        await using Stream? zipfileStream =
            Assembly.GetExecutingAssembly().GetManifestResourceStream(zipfileResourceName);
        if (zipfileStream is null)
        {
            Console.WriteLine($"Embedded resource not found: {zipfileResourceName}");
            return;
        }

        Manifest manifest;
        await using (Stream? manifestStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(manifestResourceName)) {
            if (manifestStream is null)
            {
                Console.WriteLine($"Embedded resource not found: {manifestResourceName}");
                return;
            }
            manifest = Manifest.ParseManifest(manifestStream);
        }

        Installer installer = new();
        await installer.InstallFromZipStream(zipfileStream, installDir, manifest);
        Console.WriteLine("Done");
    }

}
