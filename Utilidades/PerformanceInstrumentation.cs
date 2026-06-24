using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web;

namespace Utilidades
{
    public static class PerformanceInstrumentation
    {
        private const string RequestItemKey = "__CarniSysPerfRequest";
        private static readonly object FileLock = new object();

        public static bool IsEnabled()
        {
            return GetSettings().Enabled;
        }

        public static void BeginWebRequest(HttpContext context)
        {
            if (!IsEnabled() || context == null || context.Items[RequestItemKey] != null)
                return;

            context.Items[RequestItemKey] = new RequestTiming
            {
                RequestId = Guid.NewGuid().ToString("N"),
                StartedAtUtc = DateTime.UtcNow,
                Stopwatch = Stopwatch.StartNew()
            };
        }

        public static void EndWebRequest(HttpContext context)
        {
            if (!IsEnabled() || context == null)
                return;

            var request = context.Items[RequestItemKey] as RequestTiming;
            if (request == null)
                return;

            request.Stopwatch.Stop();

            try
            {
                context.Response.Headers["X-CarniSys-Request-Id"] = request.RequestId;
                context.Response.Headers["X-CarniSys-Elapsed-Ms"] = request.Stopwatch.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture);
                context.Response.Headers["X-CarniSys-Db-Ms"] = request.TotalDbMilliseconds.ToString(CultureInfo.InvariantCulture);
                context.Response.Headers["X-CarniSys-Db-Calls"] = request.DbOperations.Count.ToString(CultureInfo.InvariantCulture);
            }
            catch
            {
                // No bloquear la respuesta si no se pueden escribir headers.
            }

            var settings = GetSettings();
            bool logByRequest = request.Stopwatch.ElapsedMilliseconds >= settings.MinRequestMilliseconds;
            bool logByDb = request.DbOperations.Any(x => x.ElapsedMilliseconds >= settings.MinDbMilliseconds);
            if (!logByRequest && !logByDb)
                return;

            WriteLine(BuildRequestSummary(context, request, settings));
        }

        public static T MeasureDb<T>(
            string sqlOrSp,
            System.Data.CommandType commandType,
            Func<T> action,
            Func<T, string> resultSummary = null,
            string connectionTarget = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerFilePath] string callerFilePath = null)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (!IsEnabled())
                return action();

            var sw = Stopwatch.StartNew();
            Exception error = null;
            T result = default(T);

            try
            {
                result = action();
                return result;
            }
            catch (Exception ex)
            {
                error = ex;
                throw;
            }
            finally
            {
                sw.Stop();
                RegisterDbOperation(
                    BuildCallerName(callerFilePath, callerMemberName),
                    commandType,
                    sqlOrSp,
                    sw.ElapsedMilliseconds,
                    connectionTarget,
                    SafeResultSummary(resultSummary, result),
                    error);
            }
        }

        private static string SafeResultSummary<T>(Func<T, string> resultSummary, T result)
        {
            if (resultSummary == null)
                return null;

            try
            {
                return resultSummary(result);
            }
            catch
            {
                return null;
            }
        }

        private static void RegisterDbOperation(
            string caller,
            System.Data.CommandType commandType,
            string sqlOrSp,
            long elapsedMilliseconds,
            string connectionTarget,
            string resultSummary,
            Exception error)
        {
            var context = HttpContext.Current;
            var request = context != null ? context.Items[RequestItemKey] as RequestTiming : null;

            var item = new DbOperationTiming
            {
                Caller = caller,
                CommandType = commandType.ToString(),
                Operation = SummarizeCommand(sqlOrSp, commandType),
                ElapsedMilliseconds = elapsedMilliseconds,
                ConnectionTarget = string.IsNullOrWhiteSpace(connectionTarget) ? "principal" : connectionTarget,
                ResultSummary = resultSummary,
                Error = error != null ? error.GetType().Name + ": " + error.Message : null
            };

            if (request != null)
            {
                request.DbOperations.Add(item);
                request.TotalDbMilliseconds += elapsedMilliseconds;
                return;
            }

            if (elapsedMilliseconds >= GetSettings().MinDbMilliseconds)
                WriteLine(BuildStandaloneDbSummary(item));
        }

        private static string BuildStandaloneDbSummary(DbOperationTiming item)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0:yyyy-MM-dd HH:mm:ss.fff} | DB | {1} | {2} | {3} ms | {4} | {5}{6}",
                DateTime.Now,
                item.Caller,
                item.CommandType,
                item.ElapsedMilliseconds,
                item.ConnectionTarget,
                item.Operation,
                string.IsNullOrWhiteSpace(item.ResultSummary) ? string.Empty : " | " + item.ResultSummary);
        }

        private static string BuildRequestSummary(HttpContext context, RequestTiming request, PerfSettings settings)
        {
            var routeValues = context.Request != null && context.Request.RequestContext != null
                ? context.Request.RequestContext.RouteData.Values
                : null;

            string controller = routeValues != null && routeValues["controller"] != null ? routeValues["controller"].ToString() : "-";
            string action = routeValues != null && routeValues["action"] != null ? routeValues["action"].ToString() : "-";
            string url = context.Request != null ? context.Request.RawUrl ?? "" : "";
            string method = context.Request != null ? context.Request.HttpMethod ?? "" : "";
            int statusCode = context.Response != null ? context.Response.StatusCode : 0;

            var topDb = request.DbOperations
                .OrderByDescending(x => x.ElapsedMilliseconds)
                .ThenBy(x => x.Caller)
                .Take(settings.TopDbOperationsToLog)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendFormat(
                CultureInfo.InvariantCulture,
                "{0:yyyy-MM-dd HH:mm:ss.fff} | WEB | {1}/{2} | {3} | {4} | status={5} | total={6} ms | db={7} ms/{8} calls | reqId={9}",
                DateTime.Now,
                controller,
                action,
                method,
                url,
                statusCode,
                request.Stopwatch.ElapsedMilliseconds,
                request.TotalDbMilliseconds,
                request.DbOperations.Count,
                request.RequestId);

            foreach (var item in topDb)
            {
                sb.AppendLine();
                sb.Append("    DB -> ");
                sb.Append(item.Caller);
                sb.Append(" | ");
                sb.Append(item.CommandType);
                sb.Append(" | ");
                sb.Append(item.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture));
                sb.Append(" ms | ");
                sb.Append(item.Operation);

                if (!string.IsNullOrWhiteSpace(item.ResultSummary))
                {
                    sb.Append(" | ");
                    sb.Append(item.ResultSummary);
                }

                if (!string.IsNullOrWhiteSpace(item.Error))
                {
                    sb.Append(" | ERROR: ");
                    sb.Append(item.Error);
                }
            }

            return sb.ToString();
        }

        private static string BuildCallerName(string callerFilePath, string callerMemberName)
        {
            string fileName = string.IsNullOrWhiteSpace(callerFilePath)
                ? "Unknown"
                : Path.GetFileNameWithoutExtension(callerFilePath);

            return fileName + "." + (callerMemberName ?? "Unknown");
        }

        private static string SummarizeCommand(string sqlOrSp, System.Data.CommandType commandType)
        {
            if (string.IsNullOrWhiteSpace(sqlOrSp))
                return commandType.ToString();

            string text = sqlOrSp.Trim().Replace(Environment.NewLine, " ").Replace("\n", " ").Replace("\r", " ");
            while (text.Contains("  "))
                text = text.Replace("  ", " ");

            if (commandType == System.Data.CommandType.StoredProcedure)
                return text;

            return text.Length <= 120 ? text : text.Substring(0, 120) + "...";
        }

        private static void WriteLine(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            string path = ResolveLogPath();
            if (string.IsNullOrWhiteSpace(path))
                return;

            try
            {
                string directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                lock (FileLock)
                {
                    File.AppendAllText(path, message + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch
            {
                // No interrumpir la app por problemas de logging.
            }
        }

        private static string ResolveLogPath()
        {
            var settings = GetSettings();
            string configured = settings.LogRelativePath;
            if (string.IsNullOrWhiteSpace(configured))
                configured = "~/App_Data/perf-web.log";

            var context = HttpContext.Current;
            if (context != null)
                return context.Server.MapPath(configured);

            if (configured.StartsWith("~/", StringComparison.Ordinal))
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory ?? "";
                return Path.Combine(baseDir, configured.Substring(2).Replace('/', Path.DirectorySeparatorChar));
            }

            return configured;
        }

        private static PerfSettings GetSettings()
        {
            return new PerfSettings
            {
                Enabled = ReadBool("PerfInstrumentationEnabled", false),
                MinRequestMilliseconds = ReadInt("PerfInstrumentationMinRequestMs", 0),
                MinDbMilliseconds = ReadInt("PerfInstrumentationMinDbMs", 0),
                TopDbOperationsToLog = Math.Max(1, ReadInt("PerfInstrumentationTopDbOps", 10)),
                LogRelativePath = ReadString("PerfInstrumentationLogPath", "~/App_Data/perf-web.log")
            };
        }

        private static bool ReadBool(string key, bool defaultValue)
        {
            bool parsed;
            return bool.TryParse(ReadString(key, defaultValue.ToString()), out parsed) ? parsed : defaultValue;
        }

        private static int ReadInt(string key, int defaultValue)
        {
            int parsed;
            return int.TryParse(ReadString(key, defaultValue.ToString(CultureInfo.InvariantCulture)), NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed)
                ? parsed
                : defaultValue;
        }

        private static string ReadString(string key, string defaultValue)
        {
            try
            {
                return ConfigurationManager.AppSettings[key] ?? defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        private sealed class PerfSettings
        {
            public bool Enabled { get; set; }
            public int MinRequestMilliseconds { get; set; }
            public int MinDbMilliseconds { get; set; }
            public int TopDbOperationsToLog { get; set; }
            public string LogRelativePath { get; set; }
        }

        private sealed class RequestTiming
        {
            public string RequestId { get; set; }
            public DateTime StartedAtUtc { get; set; }
            public Stopwatch Stopwatch { get; set; }
            public long TotalDbMilliseconds { get; set; }
            public List<DbOperationTiming> DbOperations { get; } = new List<DbOperationTiming>();
        }

        private sealed class DbOperationTiming
        {
            public string Caller { get; set; }
            public string CommandType { get; set; }
            public string Operation { get; set; }
            public long ElapsedMilliseconds { get; set; }
            public string ConnectionTarget { get; set; }
            public string ResultSummary { get; set; }
            public string Error { get; set; }
        }
    }
}
