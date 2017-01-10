namespace CodeNav.Helpers
{
    public static class LogHelper
    {
        public static void Log(string message)
        {
            #if DEBUG
                Logger.Log(message);
            #endif
        }
    }
}
