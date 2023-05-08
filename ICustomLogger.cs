namespace Db2.Middleware.Logging
{
    public interface ICustomLogger
    {
        void LogRequest(LogDetail detail);
        void LogDebug(string message);
        void LogInformation(string message);
        void LogError(string message);
    }
}
