namespace MyInstallManager;


// Class for updating an installation
public class Updater
{
    private SelfUpdater selfUpdater;
    public Updater()
    {
        selfUpdater = new SelfUpdater();
    }


    public void Update()
    {
        if (selfUpdater.isUpdateAvailable())
        {
            // Start the "prepare" phase of the self updater.
            // ()
            selfUpdater.UpdatePrepare();
            // The UpdatePrepare step is supposed to restart the updater.
            // So if we get to here, there was a problem.
            throw new Exception("UpdatePrepare didn't restart the updater");
        }
        // See if there is a new version of the manifest
        
    }

}