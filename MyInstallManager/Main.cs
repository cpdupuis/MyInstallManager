namespace MyInstallManager;

using System.CommandLine;
using System.Reflection;

public class InstallManager
{
    private string[] args;

    private Assembly executable;
    private bool canInstall => executable.GetManifestResourceStream("installation_manifest.json") != null;

    public static async Task<int> Main(string[] args, Assembly exe)
    {
        Console.WriteLine("MyInstallManager v. 1.1");
        Console.WriteLine("  Bundled resources:");
        foreach (var name in exe.GetManifestResourceNames())
        {
            Console.WriteLine($"  - {name}");
        }

        InstallManager prog = new InstallManager(args, exe);
        await prog.run();
        return 0;
    }

    private InstallManager(string[] args, Assembly executable)
    {
        this.args = args;
        this.executable = executable;
    }

    private async Task run()
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
        if (canInstall) {
            rootCommand.Subcommands.Add(installCommand);
        }

        Option<bool> noSelfUpdateOption = new("--no-self-update")
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
            updater.AllowSelfUpdate = !parsedArgs.GetValue(noSelfUpdateOption);
            Console.WriteLine(updater.AllowSelfUpdate);
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
        const string zipfileResourceName = "installation.zip";
        const string manifestResourceName = "installation_manifest.json";

        Manifest manifest;
        await using (Stream? manifestStream = executable.GetManifestResourceStream(manifestResourceName)) {
            if (manifestStream is null)
            {
                // No manifest was bundled, so this is actually an updater.
                Console.WriteLine($"Cannot install from an updater.");
                return;
            }
            manifest = Manifest.ParseManifest(manifestStream);
        }

        await using Stream? zipfileStream =
        executable.GetManifestResourceStream(zipfileResourceName);
        Installer installer = new();
        if (zipfileStream is null)
        {
            // hack
            Console.WriteLine("Doing stub installation");
            await installer.InstallFromUrl(manifest.UpdateURLs[0], installDir);
        } else {
            Console.WriteLine("Doing full installation");
            await installer.InstallFromZipStream(zipfileStream, installDir, manifest);
        }

        Console.WriteLine("Done");
    }

}
