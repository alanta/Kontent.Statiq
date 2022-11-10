using System;
using System.Linq;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Xunit.Sdk;

namespace Kontent.Statiq.Tests.Tools;

public static class LoggerTestExtensions
{
    public static void VerifyLogged(this ILogger logger, LogLevel level, string logMessage)
    {
        var (found, actualLevel, actualMessage) = logger.VerifyLog(logMessage);
        if (!found)
        {
            throw new XunitException($"No log message found containing '{logMessage}' at any loglevel");
        }

        if (actualLevel != level)
        {
            throw new AssertActualExpectedException($"[{level}] {logMessage}", $"[{actualLevel}] {actualMessage}",
                $"Unexpected log level for log message");
        }
    }
        
    private static (bool found, LogLevel? level, string? message) VerifyLog(this ILogger logger, string message)
    {
        var call = Fake.GetCalls(logger)
            .FirstOrDefault(call => call.Arguments[2].ToString().Contains(message, StringComparison.OrdinalIgnoreCase));

        return (call != null, (LogLevel?)call?.Arguments[0], call?.Arguments[2].ToString());
    }
}