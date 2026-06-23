namespace MyInstallManager;

public class Locker
{
    private Locker()
    {
        // Don't instantiate me.
    }

    public static async Task WithLock(string lockname, Func<Task> action)
    {
        using (FileStream fs = File.Open(lockname, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read | FileShare.Write | FileShare.Delete)) {
            // C# implicitly takes an exclusive lock, which is conveniently
            // exactly what we want.
            fs.Lock(0, 1);
            await action();
        }
    }

}
