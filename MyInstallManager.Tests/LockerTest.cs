namespace MyInstallManager.Tests;

using MyInstallManager;

public class LockerTest
{
    [Fact]
    public void TestLocking()
    {
        string lockname = @"IAmALock.lock";
        List<string> sequence = new();

        Thread blockedThread = new Thread(() =>
        {
            Locker.WithLock(lockname, () =>
            {
                sequence.Add("Child thread");
            });
        });
        Locker.WithLock(lockname, () =>
        {
            sequence.Add("First entry from main thread");
            blockedThread.Start();
            Thread.Sleep(1000);
            sequence.Add("Second entry from main thread");
        });
        blockedThread.Join();
        Assert.Equal(3, sequence.Count);
        Assert.Equal("First entry from main thread", sequence[0]);
        Assert.Equal("Second entry from main thread", sequence[1]);
        Assert.Equal("Child thread", sequence[2]);
    }
}
