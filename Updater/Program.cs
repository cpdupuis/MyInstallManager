namespace MyInstallManager;

using System.Reflection;

class Program
{
    static async Task<int> Main(string[] args)
    {
        return await InstallManager.Main(args, Assembly.GetExecutingAssembly());
    }
}
