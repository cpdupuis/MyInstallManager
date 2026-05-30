namespace MyInstallManager;

using System.CommandLine;



class Program
{
    private string[] args;

    static int Main(string[] args)
    {

        Console.WriteLine("Hello, World!");
        Program prog = new Program(args);
        return prog.run();
    }

    public Program(string[] args)
    {
        this.args = args;
    }

    public int run()
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
        Option<string> installDir = new("--dir")
        {
            Description = "Directory for installing the application"
        };
        Command installCommand = new("install", "Install the application")
        {
            downloadUrlOption,
            installDir
        };
        rootCommand.Subcommands.Add(installCommand);

        statusCommand.SetAction(parsedArgs => GetStatus(parsedArgs));
        updateCommand.SetAction(parsedArgs => DoUpdate(parsedArgs));
        installCommand.SetAction(parsedArgs => DoInstall(parsedArgs));
        return rootCommand.Parse(args).Invoke();

    }

    private void GetStatus(ParseResult pr)
    {
       Console.WriteLine(UpdaterStatus.GetStatus());
    }

    private void DoUpdate(ParseResult pr)
    {
        Updater updater = new ();
        updater.Update();
    }

    private async Task DoInstall(ParseResult pr)
    {
        Installer installer = new("http://localhost:3000/apps", ".");
        await installer.Install();
    }

}
