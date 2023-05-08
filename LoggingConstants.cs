namespace Db2.Middleware.Logging
{
    public static class LoggingConstants
    {
        public enum LogLevel
        {
            Information,
            Warning,
            Error
        }

        public const string TextLogger = "txt";
        public const string SeqLogger = "seq";
    }
}
