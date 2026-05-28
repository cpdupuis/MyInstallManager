namespace MyInstallManager.Tests;

using MyInstallManager;

public class ScriptRunnerTest
{
    [Fact]
    public void RunSimpleScript()
    {
        List<string> result = ScriptRunner.RunPowershellScript(@"D:\source\MyInstallManager\MyInstallManager.Tests\test_script1.ps1");
        Assert.Equal(2, result.Count);
        Assert.Equal("Hello there", result[0]);
        Assert.Equal("Hello again", result[1]);
    }
}
