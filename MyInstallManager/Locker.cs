namespace MyInstallManager;

using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

public class Locker : IDisposable
{
    [DllImport("kernel32.dll")]
    static extern bool LockFileEx(SafeFileHandle hFile, uint dwFlags, uint dwReserved,
                                  uint nNumberOfBytesToLockLow, uint nNumberOfBytesToLockHigh,
                                  [In] ref System.Threading.NativeOverlapped lpOverlapped);
    [DllImport("kernel32.dll")]
    static extern bool UnlockFileEx(SafeFileHandle hFile, uint dwReserved,
                                    uint nNumberOfBytesToUnlockLow, uint nNumberOfBytesToUnlockHigh,
                                    [In] ref System.Threading.NativeOverlapped lpOverlapped);

    private FileStream fs;
    private SafeFileHandle handle;

    private Locker(string lockname)
    {
        this.fs = File.Open(lockname, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read | FileShare.Write | FileShare.Delete);
        this.handle = fs.SafeFileHandle;
    }

    public void Dispose() {
        handle.Dispose();
        fs.Dispose();
    }

    public static async Task WithLock(string lockname, Func<Locker, Task> action)
    {
        using (Locker locker = new Locker(lockname)) {
            await locker.WithLock(action);
        }
    }

    public async Task WithLock(Func<Locker, Task> action) {
        Acquire();
        try {
            await action(this);
        } finally {
            Release();
        }
    }

    public async Task WithoutLock(Func<Locker, Task> action) {
        Release();
        try {
            await action(this);
        } finally {
            Acquire();
        }
    }

    private void Acquire() {
        System.Threading.NativeOverlapped overlapped = new();
        overlapped.EventHandle = 0;

        LockFileEx(
            handle,
            2, // LOCKFILE_EXCLUSIVE_LOCK
            0,
            1,
            0,
            ref overlapped
        );
    }

    private void Release() {
        System.Threading.NativeOverlapped overlapped = new();
        overlapped.EventHandle = 0;

        UnlockFileEx(
            handle,
            0,
            1,
            0,
            ref overlapped
        );
    }
}
