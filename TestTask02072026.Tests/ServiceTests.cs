using Xunit;
using Services;
using System.Diagnostics;

namespace TestTask02072026.Tests
{
    public sealed class CompressorServiceTests
    {
        [Fact] //Check if method returns expected result
        public void Compress_ReturnsExpectedString()
        {
            string input = "aaabbcccdde";

            string result = CompressorService.Compress(input);

            Assert.Equal("a3b2c3d2e", result);
        }
    }

    public sealed class ServerServiceServiceTests
    {
        [Fact] //Check many parallel write operations
        public void AddToCount_WhenCalledInParallel_UpdatesCountCorrectly()
        {
            ServerService.Reset();

            int tasksCount = 1000;

            Parallel.For(0, tasksCount, _ =>
            {
                ServerService.AddToCount(1);
            });

            Assert.Equal(1000, ServerService.GetCount());
        }
    }

    public sealed class LogServiceTests
    {
        [Fact]
        public void ProcessFile_ValidFormat1Line_WritesNormalizedLineToOutput()
        {
            // Arrange
            string tempDirectory = CreateTempDirectory();

            string inputPath = Path.Combine(tempDirectory, "input.txt");
            string outputPath = Path.Combine(tempDirectory, "output.txt");
            string problemsPath = Path.Combine(tempDirectory, "problems.txt");

            File.WriteAllText(
                inputPath,
                "10.03.2025 15:14:49.523 INFORMATION User logged in");

            // Act
            LogService.ProcessFile(inputPath, outputPath, problemsPath);

            // Assert
            string output = File.ReadAllText(outputPath);
            string problems = File.ReadAllText(problemsPath);

            Assert.Equal(
                "2025-03-10\t15:14:49.523\tINFO\tDEFAULT\tUser logged in" + Environment.NewLine,
                output);

            Assert.Equal(string.Empty, problems);
        }

        [Fact]
        public void ProcessFile_ValidFormat2Line_WritesNormalizedLineToOutput()
        {
            // Arrange
            string tempDirectory = CreateTempDirectory();

            string inputPath = Path.Combine(tempDirectory, "input.txt");
            string outputPath = Path.Combine(tempDirectory, "output.txt");
            string problemsPath = Path.Combine(tempDirectory, "problems.txt");

            File.WriteAllText(
                inputPath,
                "2025-03-10 15:14:49.523| WARNING|123|UserService | Something happened");

            // Act
            LogService.ProcessFile(inputPath, outputPath, problemsPath);

            // Assert
            string output = File.ReadAllText(outputPath);
            string problems = File.ReadAllText(problemsPath);

            Assert.Equal(
                "2025-03-10\t15:14:49.523\tWARN\tUserService\tSomething happened" + Environment.NewLine,
                output);

            Assert.Equal(string.Empty, problems);
        }

        [Fact]
        public void ProcessFile_InvalidLine_WritesLineToProblemsFile()
        {
            // Arrange
            string tempDirectory = CreateTempDirectory();

            string inputPath = Path.Combine(tempDirectory, "input.txt");
            string outputPath = Path.Combine(tempDirectory, "output.txt");
            string problemsPath = Path.Combine(tempDirectory, "problems.txt");

            File.WriteAllText(inputPath, "this is not a valid log line");

            // Act
            LogService.ProcessFile(inputPath, outputPath, problemsPath);

            // Assert
            string output = File.ReadAllText(outputPath);
            string problems = File.ReadAllText(problemsPath);

            Assert.Equal(string.Empty, output);
            Assert.Equal("this is not a valid log line" + Environment.NewLine, problems);
        }

        [Fact]
        public void ProcessFile_MultipleLines_SplitsValidAndInvalidLines()
        {
            // Arrange
            string tempDirectory = CreateTempDirectory();

            string inputPath = Path.Combine(tempDirectory, "input.txt");
            string outputPath = Path.Combine(tempDirectory, "output.txt");
            string problemsPath = Path.Combine(tempDirectory, "problems.txt");

            File.WriteAllLines(inputPath, new[]
            {
                "10.03.2025 15:14:49.523 INFORMATION User logged in",
                "broken line",
                "2025-03-10 16:20:00.100| ERROR|500|PaymentService| Payment failed"
            });

            // Act
            LogService.ProcessFile(inputPath, outputPath, problemsPath);

            // Assert
            string[] outputLines = File.ReadAllLines(outputPath);
            string[] problemLines = File.ReadAllLines(problemsPath);

            Assert.Equal(2, outputLines.Length);
            Assert.Single(problemLines);

            Assert.Equal(
                "2025-03-10\t15:14:49.523\tINFO\tDEFAULT\tUser logged in",
                outputLines[0]);

            Assert.Equal(
                "2025-03-10\t16:20:00.100\tERROR\tPaymentService\tPayment failed",
                outputLines[1]);

            Assert.Equal("broken line", problemLines[0]);
        }

        private static string CreateTempDirectory()
        {
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(path);
            return path;
        }
    }
}


