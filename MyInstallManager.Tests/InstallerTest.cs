[assembly: CaptureConsole]
namespace MyInstallManager.Tests;

using MyInstallManager;

public class InstallerTest
{
    [Fact]
    public async Task TestInstall()
    {
         string installDirectory = @"D:\scratch\InstallerTest123";
         string manifestURL = @"D:\source\MyInstallManager\SampleServer\public\sample_installation_manifest.json";
        Console.WriteLine("HELLO THERE");
         Installer installer = new Installer(manifestURL, installDirectory);
         await installer.Install();
        Assert.Equal("Hello there", installDirectory);
    }
}
