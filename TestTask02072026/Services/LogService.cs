//Block-scoped namespace (traditional) as it lasted till C# 9
using System.Globalization;
using System.Text.RegularExpressions;

namespace Services
{
    public static class LogService
    {
        private static readonly Regex Format1 = new(
            @"^(?<date>\d{2}\.\d{2}\.\d{4})\s+(?<time>\d{2}:\d{2}:\d{2}\.\d+)\s+(?<level>[A-Z]+)\s+(?<message>.+)$");

        private static readonly Regex Format2 = new(
            @"^(?<date>\d{4}-\d{2}-\d{2})\s+(?<time>\d{2}:\d{2}:\d{2}\.\d+)\|\s*(?<level>[A-Z]+)\|\d+\|(?<method>[^|]+)\|\s*(?<message>.+)$");

        //The method throws as input validation is not this class concern
        public static void ProcessFile(
            string inputPath,
            string outputPath,
            string problemsPath)
        {
            //Null check
            if (string.IsNullOrWhiteSpace(inputPath) ||
                string.IsNullOrWhiteSpace(outputPath) ||
                string.IsNullOrWhiteSpace(problemsPath))
            {
                throw new ArgumentException("Paths cannot be empty.");
            }
            //Input file checks
            if (!File.Exists(inputPath))
            {
                throw new FileNotFoundException($"Input file was not found.", nameof(inputPath));
            }
            if (!string.Equals(Path.GetExtension(inputPath), ".txt", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Input file must have .txt extension.", nameof(inputPath));
            }

            var outputDirectory = Path.GetDirectoryName(outputPath);
            var problemsDirectory = Path.GetDirectoryName(problemsPath);

            //Output/Problems directories check
            if (!string.IsNullOrWhiteSpace(outputDirectory) && !Directory.Exists(outputDirectory))
            {
                throw new DirectoryNotFoundException("Output directory was not found.");
            }
            if (!string.IsNullOrWhiteSpace(problemsDirectory) && !Directory.Exists(problemsDirectory))
            {
                throw new DirectoryNotFoundException("Problems directory was not found.");
            }

            using var output = new StreamWriter(outputPath);
            using var problems = new StreamWriter(problemsPath);

            foreach (string line in File.ReadLines(inputPath))
            {
                //I've used a null-forgiving operator as if parsing succeeds
                //you wont encounter a null entry in the block
                if (TryParse(line, out LogEntry? entry))
                {
                    output.WriteLine(
                        $"{entry!.Date}\t{entry.Time}\t{entry.Level}\t{entry.Method}\t{entry.Message}");
                }
                else
                {
                    //Problems fallback in case if parsing fails
                    problems.WriteLine(line);
                }
            }
        }

        private static bool TryParse(string line, out LogEntry? entry)
        {
            return TryParseFormat1(line, out entry)
                || TryParseFormat2(line, out entry);
        }

        private static bool TryParseFormat1(string line, out LogEntry? entry)
        {
            entry = null;

            Match match = Format1.Match(line);

            if (!match.Success)
                return false;

            string? level = NormalizeLevel(match.Groups["level"].Value);

            if (level is null)
                return false;

            if (!DateTime.TryParseExact(
                match.Groups["date"].Value,
                "dd.MM.yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime date))
            {
                return false;
            }

            entry = new LogEntry(
                date.ToString("yyyy-MM-dd"),
                match.Groups["time"].Value,
                level,
                "DEFAULT",
                match.Groups["message"].Value);

            return true;
        }

        private static bool TryParseFormat2(string line, out LogEntry? entry)
        {
            entry = null;

            Match match = Format2.Match(line);

            if (!match.Success)
                return false;

            string? level = NormalizeLevel(match.Groups["level"].Value);

            if (level is null)
                return false;

            if (!DateTime.TryParseExact(
                    match.Groups["date"].Value,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime date))
            {
                return false;
            }

            entry = new LogEntry(
                date.ToString("yyyy-MM-dd"),
                match.Groups["time"].Value,
                level,
                match.Groups["method"].Value.Trim(),
                match.Groups["message"].Value);

            return true;
        }

        private static string? NormalizeLevel(string level)
        {
            return level switch
            {
                "INFORMATION" or "INFO" => "INFO",
                "WARNING" or "WARN" => "WARN",
                "ERROR" => "ERROR",
                "DEBUG" => "DEBUG",
                _ => null
            };
        }
    }

    //I've explicitly decided not to use records for that task
    //they were introduced in the latest versions of C#
    public sealed class LogEntry
    {
        public string Date { get; }
        public string Time { get; }
        public string Level { get; }
        public string Method { get; }
        public string Message { get; }

        public LogEntry(string date, string time, string level, string method, string message)
        {
            Date = date;
            Time = time;
            Level = level;
            Method = method;
            Message = message;
        }
    }
}