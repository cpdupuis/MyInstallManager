namespace MyInstallManager;

class Locker<T>
{

    private static string lockfile = "lockfile.data";

    public enum LockType
    {
        Run,
        Update
    }
    private Locker()
    {
        // Don't instantiate me.
    }

    public static T WithLock(Func<T> func)
    {
        // Acquire update lock
        using (FileStream fstream = new FileStream(lockfile, FileMode.OpenOrCreate,
            FileAccess.ReadWrite, FileShare.None))
        {
            bool isLocked = false;
            try
            {
                fstream.Lock(0, 1);
                isLocked = true;
                return func();
            }
            finally
            {
                if (isLocked)
                {
                    fstream.Unlock(0, 1);
                }
            }
        }

    }

}