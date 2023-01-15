using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FWGPUE.IO;
static class Log {
    #region severity
    public enum Severity {
        Inane = -1,
        All = 0,
        Warn,
        Error,
        None
    }
    /// <summary>
    /// Log all messages with greater severity than this value.
    /// </summary>
    public static Severity LogLevel { get; set; } = Severity.All;
    #endregion severity

    #region colour state management
    static Stack<(ConsoleColor foreground, ConsoleColor background)> ColourStateStack { get; } = new();
    static void PushColourState() {
        ColourStateStack.Push((Console.ForegroundColor, Console.BackgroundColor));
    }
    static void PopColourState() {
        if (ColourStateStack.TryPop(out var result)) {
            Console.ForegroundColor = result.foreground;
            Console.BackgroundColor = result.background;
        }
    }
    static void SetColours(ConsoleColor foreground, ConsoleColor background) {
        Console.ForegroundColor = foreground;
        Console.BackgroundColor = background;
    }
    #endregion colour state management

    #region log file management 
    public static bool WriteToLogFile { get; set; } = true;
    public static EngineFileLocation LogFileLocation { get; set; } = new EngineFileLocation("log.txt");

    static Log() {
        if (File.Exists(LogFileLocation.FullPath)) {
            File.Delete(LogFileLocation.FullPath);
        }
    }
    #endregion log file management

    public static ConsoleColor NormalLogColour { get; set; } = ConsoleColor.Gray;
    public static ConsoleColor WarningLogColour { get; set; } = ConsoleColor.Yellow;
    public static ConsoleColor ErrorLogColour { get; set; } = ConsoleColor.DarkRed;

    public static ConsoleColor Background { get; set; } = ConsoleColor.Black;

    public static void Message(ConsoleColor foreground, ConsoleColor background, object message, string logFileType, bool newline = true) {
        string fullMessage = $"{message}{(newline ? Environment.NewLine : "")}";

        PushColourState();
        #region no trace

        SetColours(foreground, background);
        Console.Write(fullMessage);

        #endregion no trace
        PopColourState();

        if (WriteToLogFile) {
            LogFileLocation.EnsureExists();
            if (File.Exists(LogFileLocation.FullPath)) {
                using var writer = File.AppendText(LogFileLocation.FullPath);
                writer.Write($"{logFileType}: {fullMessage}");
            }

        }
    }

    private static string GetCallerInfo(string caller, string file, int line) {
        StackFrame frame = new StackFrame(2);
        return $"{(frame.GetMethod()?.DeclaringType?.ToString() ?? "decltype not found")} {caller} l{line}";
    }

    public static void Inane(object message, bool logCallingLocation = true,
        [CallerMemberName] string caller = "",
        [CallerFilePath] string callerFile = "",
        [CallerLineNumber] int callingLine = 0) {

        if (LogLevel <= Severity.Inane) {
            // get caller string if desired, leave blank if not desired
            string callingMethod = logCallingLocation ?
                $"{GetCallerInfo(caller, callerFile, callingLine)} |"
                : "";
            // write out
            Message(NormalLogColour, Background, $"{callingMethod} {message}", "inane");
        }
    }

    /// <summary> Log at Severity 0 (Any). </summary>
    public static void Info(object message, bool logCallingLocation = true,
        [CallerMemberName] string caller = "",
        [CallerFilePath] string callerFile = "",
        [CallerLineNumber] int callingLine = 0) {

        if (LogLevel <= Severity.All) {
            // get caller string if desired, leave blank if not desired
            string callingMethod = logCallingLocation ?
                $"{GetCallerInfo(caller, callerFile, callingLine)} |"
                : "";
            // write out
            Message(NormalLogColour, Background, $"{callingMethod} {message}", "info");
        }
    }
    /// <summary> Log at Severity 1 (Warn). </summary>
    public static void Warn(object message, bool logCallingLocation = true,
        [CallerMemberName] string caller = "",
        [CallerFilePath] string callerFile = "",
        [CallerLineNumber] int callingLine = 0) {

        if (LogLevel <= Severity.Warn) {
            // get caller string if desired, leave blank if not desired
            string callingMethod = logCallingLocation ?
                $"{GetCallerInfo(caller, callerFile, callingLine)} |"
                : "";
            // write out
            Message(WarningLogColour, Background, $"{callingMethod} {message}", "warning");
        }
    }
    /// <summary> Log at Severity 2 (Error). </summary>
    public static void Error(object message, bool logCallingLocation = true,
        [CallerMemberName] string caller = "",
        [CallerFilePath] string callerFile = "",
        [CallerLineNumber] int callingLine = 0) {

        if (LogLevel <= Severity.Error) {
            // get caller string if desired, leave blank if not desired
            string callingMethod = logCallingLocation ?
                $"{GetCallerInfo(caller, callerFile, callingLine)} |"
                : "";
            // write out
            Message(ErrorLogColour, Background, $"{callingMethod} {message}", "error");
        }
    }
}
