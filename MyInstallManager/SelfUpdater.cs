class SelfUpdater
{
    public SelfUpdater()
    {
        
    }

    public void UpdatePrepare()
    {
        // In the Prepare process ("official path")
        // Fetch an update (if available)

        // Create an updated executable in a temporary file

        // Exec the new executable

        // Wait for new process to exit
    }

    public void UpdateCommit() {

        // In the Commit process ("temp path")

        // Swap official executable for for the temporary one

        // Exec the new official executable with next phase args

        // Exit.

        // 
    }
}