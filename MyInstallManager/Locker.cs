namespace MyInstallManager;

public class Locker
{
    private Locker()
    {
        // Don't instantiate me.
    }

    public static void WithLock(string lockname, Action action)
    {
        NamedWaitHandleOptions waitHandleOptions = new();
        waitHandleOptions.CurrentSessionOnly = false;
        waitHandleOptions.CurrentUserOnly = false;

        bool isLocked = false;
        using (Mutex mutex = new Mutex(lockname, waitHandleOptions))
        {
            try
            {
                mutex.WaitOne();
                isLocked = true;
                action();
            }
            finally
            {
                if (isLocked)
                {
                    mutex.ReleaseMutex();

                }
            }
        }

    }

}