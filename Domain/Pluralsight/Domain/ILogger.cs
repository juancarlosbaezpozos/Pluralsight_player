using System;

namespace Pluralsight.Domain;

public interface ILogger
{
    void Log(string message, LogLevel level);

    void LogException(Exception e);
}
