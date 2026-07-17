namespace MyInstallManager;

class Program
{
    static async Task<int> Main(string[] args)
    {
        return await InstallManager.Main(args, Assembly.GetExecutingAssembly());
    }
}
