using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Internal;
using EventStore.ClientAPI.SystemData;
using NUnit.Framework;

namespace EventStore.Core.Tests.ClientAPI.Helpers
{
    public static class TestConnection
    {
        private static int _nextConnId = -1;

        public static IEventStoreConnection Create(IPEndPoint endPoint, TcpType tcpType = TcpType.Normal,
            UserCredentials userCredentials = null)
        {
            return EventStoreConnection.Create(Settings(tcpType, userCredentials),
                endPoint.ToESTcpUri(),
                string.Format("ESC-{0}", Interlocked.Increment(ref _nextConnId)));
        }

        public static IEventStoreConnection Create(TcpType tcpType, UserCredentials userCredentials = null)
        {
            return EventStoreConnection.Create(Settings(tcpType, userCredentials),
                tcpType == TcpType.Ssl ? TestNode.TcpSecEndPoint.ToESTcpUri() : TestNode.TcpEndPoint.ToESTcpUri(),
                string.Format("ESC-{0}", Interlocked.Increment(ref _nextConnId)));
        }

        private static ConnectionSettingsBuilder Settings(TcpType tcpType, UserCredentials userCredentials)
        {

            var settings = ConnectionSettings.Create()
                .SetDefaultUserCredentials(userCredentials)
                .UseCustomLogger(new NunitLogger(TestContext.Out))
                .EnableVerboseLogging()
                .LimitReconnectionsTo(10)
                .LimitRetriesForOperationTo(100)
                .SetTimeoutCheckPeriodTo(TimeSpan.FromMilliseconds(100))
                .SetReconnectionDelayTo(TimeSpan.Zero)
                .FailOnNoServerResponse()
                .SetOperationTimeoutTo(TimeSpan.FromDays(1));
            if (tcpType == TcpType.Ssl)
                settings.UseSslConnection("ES", false);
            return settings;
        }

        class NunitLogger : ILogger
        {
            private readonly TextWriter _writer;

            public NunitLogger(TextWriter writer)
            {
                _writer = writer ?? Console.Out;
            }
            /// <summary>
            /// Writes an error to the logger
            /// </summary>
            /// <param name="format">Format string for the log message.</param>
            /// <param name="args">Arguments to be inserted into the format string.</param>
            public void Error(string format, params object[] args)
            {
                _writer.WriteLine(Log("ERROR", format, args));
            }

            /// <summary>
            /// Writes an error to the logger
            /// </summary>
            /// <param name="ex">A thrown exception.</param>
            /// <param name="format">Format string for the log message.</param>
            /// <param name="args">Arguments to be inserted into the format string.</param>
            public void Error(Exception ex, string format, params object[] args)
            {
                _writer.WriteLine(Log("ERROR", ex, format, args));
            }

            /// <summary>
            /// Writes a debug message to the logger
            /// </summary>
            /// <param name="format">Format string for the log message.</param>
            /// <param name="args">Arguments to be inserted into the format string.</param>
            public void Debug(string format, params object[] args)
            {
                _writer.WriteLine(Log("DEBUG", format, args));
            }

            /// <summary>
            /// Writes a debug message to the logger
            /// </summary>
            /// <param name="ex">A thrown exception.</param>
            /// <param name="format">Format string for the log message.</param>
            /// <param name="args">Arguments to be inserted into the format string.</param>
            public void Debug(Exception ex, string format, params object[] args)
            {
                _writer.WriteLine(Log("DEBUG", ex, format, args));
            }

            /// <summary>
            /// Writes an information message to the logger
            /// </summary>
            /// <param name="format">Format string for the log message.</param>
            /// <param name="args">Arguments to be inserted into the format string.</param>
            public void Info(string format, params object[] args)
            {
                _writer.WriteLine(Log("INFO", format, args));
            }

            /// <summary>
            /// Writes an information message to the logger
            /// </summary>
            /// <param name="ex">A thrown exception.</param>
            /// <param name="format">Format string for the log message.</param>
            /// <param name="args">Arguments to be inserted into the format string.</param>
            public void Info(Exception ex, string format, params object[] args)
            {
                _writer.WriteLine(Log("INFO", ex, format, args));
            }

            private string Log(string level, string format, params object[] args)
            {
                return string.Format("[{0:00},{1:HH:mm:ss.fff},{2}] {3}",
                    Thread.CurrentThread.ManagedThreadId,
                    DateTime.UtcNow,
                    level,
                    args.Length == 0 ? format : string.Format(format, args));
            }

            private string Log(string level, Exception exc, string format, params object[] args)
            {
                var sb = new StringBuilder();
                while (exc != null)
                {
                    sb.AppendLine();
                    sb.AppendLine(exc.ToString());
                    exc = exc.InnerException;
                }

                return string.Format("[{0:00},{1:HH:mm:ss.fff},{2}] {3}\nEXCEPTION(S) OCCURRED:{4}",
                    Thread.CurrentThread.ManagedThreadId,
                    DateTime.UtcNow,
                    level,
                    args.Length == 0 ? format : string.Format(format, args),
                    sb);

            }
        }
    }
}
