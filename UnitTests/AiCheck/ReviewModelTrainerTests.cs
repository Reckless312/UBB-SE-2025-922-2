using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DrinkDb_Auth.AiCheck;
using Microsoft.ML;
using Moq;
using Xunit;

namespace UnitTests.AiCheck
{
    public class ReviewModelTrainerTests
    {
        private static readonly string TestDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "review_data.csv");
        private static readonly string TestModelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "test_model.zip");
        private static readonly string TestLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "test_training_log.txt");
        private static readonly string NonExistentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NonExistentDirectory", "missing_file.csv");

        [Fact]
        public void TrainModel_WithValidData_ShouldSucceed()
        {
            // Arrange
            Console.WriteLine($"Current Directory: {Directory.GetCurrentDirectory()}");
            Console.WriteLine($"Test is running from: {AppDomain.CurrentDomain.BaseDirectory}");
            Console.WriteLine($"Test data path: {TestDataPath}");
            Console.WriteLine($"Test model path: {TestModelPath}");
            Console.WriteLine($"Test log path: {TestLogPath}");

            EnsureTestDirectoriesExist();
            CreateRobustTestDataFile();

            // Make sure target files don't exist before test
            if (File.Exists(TestModelPath))
            {
                File.Delete(TestModelPath);
            }

            if (File.Exists(TestLogPath))
            {
                File.Delete(TestLogPath);
            }

            // Verify test data file exists
            if (!File.Exists(TestDataPath))
            {
                Console.WriteLine("WARNING: Test data file doesn't exist after CreateRobustTestDataFile call!");

                // Try to create it again with absolute path validation
                CreateRobustTestDataFile();
                Console.WriteLine($"Test data file exists after retry: {File.Exists(TestDataPath)}");
            }

            // Act
            Console.WriteLine("Starting model training...");
            bool result = ReviewModelTrainer.TrainModel(TestDataPath, TestModelPath, TestLogPath);
            Console.WriteLine($"Model training finished with result: {result}");

            // Assert
            if (!result)
            {
                DisplayLogIfAvailable();
                Assert.True(result, "TrainModel returned false when expected to succeed. See log output above.");
            }

            Assert.True(File.Exists(TestModelPath), "Model file was not created");
            Assert.True(File.Exists(TestLogPath), "Log file was not created");

            // Cleanup
            CleanupTestFiles();
        }

        [Fact]
        public void TrainModel_WithInvalidData_ShouldFail()
        {
            // Arrange
            EnsureTestDirectoriesExist();
            CreateInvalidTestDataFile();

            // Make sure target files don't exist before test
            if (File.Exists(TestModelPath))
            {
                File.Delete(TestModelPath);
            }

            if (File.Exists(TestLogPath))
            {
                File.Delete(TestLogPath);
            }

            // Act
            bool result = ReviewModelTrainer.TrainModel(TestDataPath, TestModelPath, TestLogPath);

            // Assert
            Assert.False(result, "TrainModel should return false for invalid data");
            Assert.False(File.Exists(TestModelPath), "Model file should not be created for invalid data");
            Assert.True(File.Exists(TestLogPath), "Log file should be created even for invalid data");

            // Note: We're not checking specific log content since the implementation may vary
            // Just checking that it logs something

            // Cleanup
            CleanupTestFiles();
        }

        [Fact]
        public void TrainModel_WithInvalidDataFormat_ShouldFail()
        {
            // Arrange
            EnsureTestDirectoriesExist();
            CreateMalformedTestDataFile();

            // Make sure target files don't exist before test
            if (File.Exists(TestModelPath))
            {
                File.Delete(TestModelPath);
            }

            if (File.Exists(TestLogPath))
            {
                File.Delete(TestLogPath);
            }

            // Act
            bool result = ReviewModelTrainer.TrainModel(TestDataPath, TestModelPath, TestLogPath);

            // Assert
            Assert.False(result, "TrainModel should return false for malformed data");
            Assert.False(File.Exists(TestModelPath), "Model file should not be created for malformed data");
            Assert.True(File.Exists(TestLogPath), "Log file should be created even for malformed data");

            // Note: Not checking specific log content as implementation may vary

            // Cleanup
            CleanupTestFiles();
        }

        [Fact]
        public void TrainModel_WithMissingFile_ShouldFail()
        {
            // Arrange
            EnsureTestDirectoriesExist();

            // Make sure directory doesn't exist to ensure file doesn't exist
            string nonExistentDir = Path.GetDirectoryName(NonExistentPath);
            if (Directory.Exists(nonExistentDir))
            {
                Directory.Delete(nonExistentDir, true);
            }

            // Make sure target files don't exist before test
            if (File.Exists(TestModelPath))
            {
                File.Delete(TestModelPath);
            }

            if (File.Exists(TestLogPath))
            {
                File.Delete(TestLogPath);
            }

            // Act
            bool result = ReviewModelTrainer.TrainModel(NonExistentPath, TestModelPath, TestLogPath);

            // Assert
            Assert.False(result, "TrainModel should return false for missing file");
            Assert.False(File.Exists(TestModelPath), "Model file should not be created when input file is missing");
            Assert.True(File.Exists(TestLogPath), "Log file should be created even when input file is missing");

            // Cleanup
            CleanupTestFiles();
        }

        [Fact]
        public void TrainModel_WithFileNotFoundException_ShouldFail()
        {
            // Arrange
            EnsureTestDirectoriesExist();

            // Instead of trying to lock a file (which can be flaky in a test environment),
            // create a file that will trigger a FileNotFoundException during processing
            // by using a path that exists but can't be read properly
            string tempDir = Path.Combine(Path.GetTempPath(), "ReviewModelTrainerTests");
            Directory.CreateDirectory(tempDir);

            // Use a path that can't be properly read using ML.NET's TextLoader
            // Creating a file but with invalid encoding can trigger file access errors
            string specialDataPath = Path.Combine(tempDir, "problematic_file.csv");

            try
            {
                // Create a binary file with invalid CSV format that will trigger errors when processed
                using (var fs = new FileStream(specialDataPath, FileMode.Create, FileAccess.Write))
                {
                    // Write some binary data that's not valid text/CSV
                    byte[] invalidData = new byte[] { 0xFF, 0xFE, 0x00, 0x00 }; // Invalid UTF encoding
                    fs.Write(invalidData, 0, invalidData.Length);
                }

                // Make sure target files don't exist before test
                if (File.Exists(TestModelPath))
                {
                    File.Delete(TestModelPath);
                }

                if (File.Exists(TestLogPath))
                {
                    File.Delete(TestLogPath);
                }

                // Act
                bool result = ReviewModelTrainer.TrainModel(specialDataPath, TestModelPath, TestLogPath);

                // Assert
                Assert.False(result, "TrainModel should fail with invalid binary file");
                Assert.False(File.Exists(TestModelPath), "Model file should not be created with invalid binary file");
                Assert.True(File.Exists(TestLogPath), "Log file should be created even with invalid binary file");
            }
            finally
            {
                // Cleanup
                try
                {
                    if (File.Exists(specialDataPath))
                    {
                        File.Delete(specialDataPath);
                    }

                    if (Directory.Exists(tempDir))
                    {
                        Directory.Delete(tempDir, true);
                    }
                }
                catch
                {
                    // Ignore cleanup errors
                }

                CleanupTestFiles();
            }
        }

        [Fact]
        public void TrainModel_WithGeneralException_ShouldFail()
        {
            // Arrange
            EnsureTestDirectoriesExist();

            // Create a file with correct header but invalid data values (invalid boolean format)
            var csvContent = new StringBuilder();
            csvContent.AppendLine("ReviewContent}IsOffensiveContent");
            csvContent.AppendLine("This is a test}2"); // Invalid boolean value (not 0 or 1)
            csvContent.AppendLine("This is another test}3"); // Invalid boolean value (not 0 or 1)

            File.WriteAllText(TestDataPath, csvContent.ToString());

            // Make sure target files don't exist before test
            if (File.Exists(TestModelPath))
            {
                File.Delete(TestModelPath);
            }

            if (File.Exists(TestLogPath))
            {
                File.Delete(TestLogPath);
            }

            // Act
            bool result = ReviewModelTrainer.TrainModel(TestDataPath, TestModelPath, TestLogPath);

            // Assert
            Assert.False(result, "TrainModel should fail with invalid boolean values");
            Assert.False(File.Exists(TestModelPath), "Model file should not be created with invalid boolean values");
            Assert.True(File.Exists(TestLogPath), "Log file should be created even with invalid boolean values");

            // Cleanup
            CleanupTestFiles();
        }

        [Fact]
        public void TrainModel_NoParameters_ShouldUseDefaultPaths()
        {
            // This test checks that the parameterless TrainModel() method works
            // We'll use reflection to access the private fields to verify the paths

            // Arrange
            Type trainerType = typeof(ReviewModelTrainer);
            FieldInfo defaultDataPathField = trainerType.GetField("DefaultDataPath", BindingFlags.NonPublic | BindingFlags.Static);
            FieldInfo defaultModelPathField = trainerType.GetField("DefaultModelPath", BindingFlags.NonPublic | BindingFlags.Static);
            FieldInfo defaultLogPathField = trainerType.GetField("DefaultLogPath", BindingFlags.NonPublic | BindingFlags.Static);

            string defaultDataPath = (string)defaultDataPathField.GetValue(null);
            string defaultModelPath = (string)defaultModelPathField.GetValue(null);
            string defaultLogPath = (string)defaultLogPathField.GetValue(null);

            // Create the necessary directories and a valid data file
            Directory.CreateDirectory(Path.GetDirectoryName(defaultDataPath));
            CreateRobustTestDataFile(defaultDataPath);

            // Make sure target files don't exist before test
            if (File.Exists(defaultModelPath))
            {
                File.Delete(defaultModelPath);
            }

            if (File.Exists(defaultLogPath))
            {
                File.Delete(defaultLogPath);
            }

            // Act
            bool result = ReviewModelTrainer.TrainModel();

            // Assert
            Assert.True(result, "TrainModel with default paths should succeed");
            Assert.True(File.Exists(defaultModelPath), "Model file should be created with default paths");
            Assert.True(File.Exists(defaultLogPath), "Log file should be created with default paths");

            // Cleanup
            if (File.Exists(defaultDataPath))
            {
                File.Delete(defaultDataPath);
            }

            if (File.Exists(defaultModelPath))
            {
                File.Delete(defaultModelPath);
            }

            if (File.Exists(defaultLogPath))
            {
                File.Delete(defaultLogPath);
            }
        }

        [Fact]
        public void TrainModel_WithCustomDataPath_ShouldDeriveOtherPaths()
        {
            // Arrange
            string customDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CustomTestData", "custom_data.csv");
            string expectedModelPath = Path.Combine(Path.GetDirectoryName(customDataPath), "test_model.zip");
            string expectedLogPath = Path.Combine(Path.GetDirectoryName(customDataPath), "test_training_log.txt");

            Directory.CreateDirectory(Path.GetDirectoryName(customDataPath));
            CreateRobustTestDataFile(customDataPath);

            // Make sure target files don't exist before test
            if (File.Exists(expectedModelPath))
            {
                File.Delete(expectedModelPath);
            }

            if (File.Exists(expectedLogPath))
            {
                File.Delete(expectedLogPath);
            }

            // Act
            bool result = ReviewModelTrainer.TrainModel(customDataPath);

            // Assert
            Assert.True(result, "TrainModel with custom data path should succeed");
            Assert.True(File.Exists(customDataPath), "Data file should exist with custom data path");
            Assert.True(File.Exists(expectedModelPath), "Model file should be created with derived path");
            Assert.True(File.Exists(expectedLogPath), "Log file should be created with derived path");

            // Cleanup
            if (File.Exists(customDataPath))
            {
                File.Delete(customDataPath);
            }

            if (File.Exists(expectedModelPath))
            {
                File.Delete(expectedModelPath);
            }

            if (File.Exists(expectedLogPath))
            {
                File.Delete(expectedLogPath);
            }

            if (Directory.Exists(Path.GetDirectoryName(customDataPath)))
            {
                Directory.Delete(Path.GetDirectoryName(customDataPath), true);
            }
        }

        [Fact]
        public void TrainModel_WithNonExistentDirectories_ShouldCreateThem()
        {
            // Arrange
            string newDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NewTestDirectory");
            string dataPath = Path.Combine(newDirectory, "data.csv");
            string modelPath = Path.Combine(newDirectory, "model.zip");
            string logPath = Path.Combine(newDirectory, "log.txt");

            // Make sure directory doesn't exist
            if (Directory.Exists(newDirectory))
            {
                Directory.Delete(newDirectory, true);
            }

            try
            {
                // Create valid test data
                Directory.CreateDirectory(newDirectory);
                CreateRobustTestDataFile(dataPath);

                // Make sure target files don't exist before test
                if (File.Exists(modelPath))
                {
                    File.Delete(modelPath);
                }

                if (File.Exists(logPath))
                {
                    File.Delete(logPath);
                }

                // Act
                bool result = ReviewModelTrainer.TrainModel(dataPath, modelPath, logPath);

                // Assert
                Assert.True(result, "TrainModel should create directories when needed");
                Assert.True(Directory.Exists(Path.GetDirectoryName(modelPath)), "Model directory should be created");
                Assert.True(Directory.Exists(Path.GetDirectoryName(logPath)), "Log directory should be created");
                Assert.True(File.Exists(modelPath), "Model file should be created in new directory");
                Assert.True(File.Exists(logPath), "Log file should be created in new directory");
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(newDirectory))
                {
                    try
                    {
                        Directory.Delete(newDirectory, true);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
            }
        }

        [Fact]
        public void TrainModel_WithEmptyTrainingData_ShouldFail()
        {
            // Arrange
            EnsureTestDirectoriesExist();
            CreateEmptyTrainingDataFile();

            // Make sure target files don't exist before test
            if (File.Exists(TestModelPath))
            {
                File.Delete(TestModelPath);
            }

            if (File.Exists(TestLogPath))
            {
                File.Delete(TestLogPath);
            }

            // Act
            bool result = ReviewModelTrainer.TrainModel(TestDataPath, TestModelPath, TestLogPath);

            // Assert
            Assert.False(result, "TrainModel should fail with empty training data");
            Assert.False(File.Exists(TestModelPath), "Model file should not be created with empty training data");
            Assert.True(File.Exists(TestLogPath), "Log file should be created even with empty training data");

            // Cleanup
            CleanupTestFiles();
        }

        [Fact]
        public void TrainModel_ValidateDataFormat_WithEmptyFile_ShouldFail()
        {
            // Arrange
            EnsureTestDirectoriesExist();

            // Create completely empty file
            File.WriteAllText(TestDataPath, string.Empty);

            // Make sure target files don't exist before test
            if (File.Exists(TestModelPath))
            {
                File.Delete(TestModelPath);
            }

            if (File.Exists(TestLogPath))
            {
                File.Delete(TestLogPath);
            }

            // Act
            bool result = ReviewModelTrainer.TrainModel(TestDataPath, TestModelPath, TestLogPath);

            // Assert
            Assert.False(result, "TrainModel should fail with empty file");
            Assert.False(File.Exists(TestModelPath), "Model file should not be created with empty file");
            Assert.True(File.Exists(TestLogPath), "Log file should be created even with empty file");

            // Cleanup
            CleanupTestFiles();
        }

        [Fact]
        public void TrainModel_ValidateDataFormat_WithoutRequiredColumns_ShouldFail()
        {
            // Arrange
            EnsureTestDirectoriesExist();
            var csvContent = new StringBuilder();
            csvContent.AppendLine("WrongColumn1}WrongColumn2");
            csvContent.AppendLine("Test content}0");
            File.WriteAllText(TestDataPath, csvContent.ToString());

            // Make sure target files don't exist before test
            if (File.Exists(TestModelPath))
            {
                File.Delete(TestModelPath);
            }

            if (File.Exists(TestLogPath))
            {
                File.Delete(TestLogPath);
            }

            // Act
            bool result = ReviewModelTrainer.TrainModel(TestDataPath, TestModelPath, TestLogPath);

            // Assert
            Assert.False(result, "TrainModel should fail with wrong column names");
            Assert.False(File.Exists(TestModelPath), "Model file should not be created with wrong column names");
            Assert.True(File.Exists(TestLogPath), "Log file should be created even with wrong column names");

            // Cleanup
            CleanupTestFiles();
        }

        [Fact]
        public void TrainModel_ValidateDataFormat_WithInvalidBooleanValues_ShouldFail()
        {
            // Arrange
            EnsureTestDirectoriesExist();
            var csvContent = new StringBuilder();
            csvContent.AppendLine("ReviewContent}IsOffensiveContent");
            csvContent.AppendLine("Test content}NotABoolean");
            File.WriteAllText(TestDataPath, csvContent.ToString());

            // Make sure target files don't exist before test
            if (File.Exists(TestModelPath))
            {
                File.Delete(TestModelPath);
            }

            if (File.Exists(TestLogPath))
            {
                File.Delete(TestLogPath);
            }

            bool result = ReviewModelTrainer.TrainModel(TestDataPath, TestModelPath, TestLogPath);
            Assert.False(result, "TrainModel should fail with non-boolean values");
            Assert.False(File.Exists(TestModelPath), "Model file should not be created with non-boolean values");
            Assert.True(File.Exists(TestLogPath), "Log file should be created even with non-boolean values");
            CleanupTestFiles();
        }

        [Fact]
        public void TrainModel_LoggingFails_ShouldContinueWithConsoleOutput()
        {
            EnsureTestDirectoriesExist();
            CreateRobustTestDataFile();
            if (File.Exists(TestModelPath))
            {
                File.Delete(TestModelPath);
            }

            string readOnlyDir = Path.Combine(Path.GetTempPath(), "ReadOnlyDir");

            try
            {
                if (Directory.Exists(readOnlyDir))
                {
                    Directory.Delete(readOnlyDir, true);
                }

                Directory.CreateDirectory(readOnlyDir);

                // Make the directory read-only
                DirectoryInfo dirInfo = new DirectoryInfo(readOnlyDir);
                dirInfo.Attributes = FileAttributes.ReadOnly;

                // Use a log path in the read-only directory
                string readOnlyLogPath = Path.Combine(readOnlyDir, "readonly_log.txt");

                // Act
                bool result = ReviewModelTrainer.TrainModel(TestDataPath, TestModelPath, readOnlyLogPath);

                // Assert - The training should still succeed with fallback to console
                Assert.True(result, "TrainModel should succeed even when logging fails");
                Assert.True(File.Exists(TestModelPath), "Model file should be created even when logging fails");
            }
            finally
            {
                // Cleanup - make directory writable again so we can delete it
                if (Directory.Exists(readOnlyDir))
                {
                    try
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(readOnlyDir);
                        dirInfo.Attributes = FileAttributes.Normal;
                        Directory.Delete(readOnlyDir, true);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }

                CleanupTestFiles();
            }
        }

        private static void EnsureTestDirectoriesExist()
        {
            string testDataDir = Path.GetDirectoryName(TestDataPath);
            string testModelDir = Path.GetDirectoryName(TestModelPath);
            string testLogDir = Path.GetDirectoryName(TestLogPath);

            Console.WriteLine($"Ensuring directory exists: {testDataDir}");
            Directory.CreateDirectory(testDataDir);

            Console.WriteLine($"Ensuring directory exists: {testModelDir}");
            Directory.CreateDirectory(testModelDir);

            Console.WriteLine($"Ensuring directory exists: {testLogDir}");
            Directory.CreateDirectory(testLogDir);

            // Verify directories were created successfully
            Console.WriteLine($"Test data directory exists: {Directory.Exists(testDataDir)}");
            Console.WriteLine($"Test model directory exists: {Directory.Exists(testModelDir)}");
            Console.WriteLine($"Test log directory exists: {Directory.Exists(testLogDir)}");
        }

        private static void CreateTestDataFile()
        {
            CreateTestDataFile(TestDataPath);
        }

        private static void CreateTestDataFile(string path)
        {
            StringBuilder csvContent = new StringBuilder();
            csvContent.AppendLine("ReviewContent}IsOffensiveContent");
            csvContent.AppendLine("This is a great drink!}0");
            csvContent.AppendLine("I love this cocktail, it's amazing!}0");
            csvContent.AppendLine("This is the worst drink ever!}0");
            csvContent.AppendLine("You're an idiot if you like this drink!}1");
            csvContent.AppendLine("This drink is f***ing terrible!}1");
            csvContent.AppendLine("I can't believe how bad this is, you moron!}1");

            File.WriteAllText(path, csvContent.ToString());
        }

        private static void CreateRobustTestDataFile(string path = null)
        {
            string filePath = path ?? TestDataPath;

            // Ensure directory exists
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Console.WriteLine($"Creating directory for test data: {directory}");
                Directory.CreateDirectory(directory);
            }

            Console.WriteLine($"Creating robust test data at: {filePath}");

            try
            {
                var csvContent = new StringBuilder();
                csvContent.AppendLine("ReviewContent}IsOffensiveContent");

                // Add more data with clearer patterns for ML to detect
                // Non-offensive reviews (positive examples) - 150 examples
                for (int i = 0; i < 50; i++)
                {
                    csvContent.AppendLine($"This drink is absolutely delicious and refreshing! {i}}}0");
                    csvContent.AppendLine($"The cocktail had perfect balance of flavors, highly recommended! {i}}}0");
                    csvContent.AppendLine($"I loved this beverage, it was smooth and well-crafted. {i}}}0");
                }

                // Offensive reviews (negative examples) - 150 examples
                for (int i = 0; i < 50; i++)
                {
                    csvContent.AppendLine($"This drink is absolutely disgusting, only an idiot would enjoy this! {i}}}1");
                    csvContent.AppendLine($"The bartender is incompetent and stupid for making this terrible drink! {i}}}1");
                    csvContent.AppendLine($"What a waste of money, this drink is garbage and the service is awful! {i}}}1");
                }

                File.WriteAllText(filePath, csvContent.ToString());
                Console.WriteLine($"Test data file created at: {filePath}");
                Console.WriteLine($"File exists: {File.Exists(filePath)}");
                Console.WriteLine($"File size: {new FileInfo(filePath).Length} bytes");
                try
                {
                    string[] firstFewLines = File.ReadLines(filePath).Take(3).ToArray();
                    Console.WriteLine($"First few lines of file: {string.Join(", ", firstFewLines)}");
                }
                catch (Exception readEx)
                {
                    Console.WriteLine($"Error reading back file content: {readEx.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR creating test data file: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private static void CreateInvalidTestDataFile()
        {
            StringBuilder csvContent = new StringBuilder();
            csvContent.AppendLine("Invalid,Data,Format");
            csvContent.AppendLine("This,is,not,correct");
            File.WriteAllText(TestDataPath, csvContent.ToString());
        }

        private static void CreateMalformedTestDataFile()
        {
            StringBuilder csvContent = new StringBuilder();
            csvContent.AppendLine("ReviewContent}WrongColumn");
            csvContent.AppendLine("This is content}not a boolean");
            File.WriteAllText(TestDataPath, csvContent.ToString());
        }

        private static void CreateEmptyTrainingDataFile()
        {
            File.WriteAllText(TestDataPath, "ReviewContent}IsOffensiveContent");
        }

        private static void DisplayLogIfAvailable()
        {
            if (File.Exists(TestLogPath))
            {
                string logContent = File.ReadAllText(TestLogPath);
                Console.WriteLine("Log contents:");
                Console.WriteLine(logContent);
                string[] logLines = File.ReadAllLines(TestLogPath);
                if (logLines.Length > 10)
                {
                    Console.WriteLine("\nLast 10 lines of log:");
                    foreach (string line in logLines.Skip(logLines.Length - 10))
                    {
                        Console.WriteLine(line);
                    }
                }
            }
            else
            {
                Console.WriteLine("No log file was created at path: " + TestLogPath);
            }
        }

        private static void CleanupTestFiles()
        {
            try
            {
                if (File.Exists(TestDataPath))
                {
                    Console.WriteLine($"Deleting test data file: {TestDataPath}");
                    File.Delete(TestDataPath);
                }
                else
                {
                    Console.WriteLine($"Test data file not found for cleanup: {TestDataPath}");
                }

                if (File.Exists(TestModelPath))
                {
                    Console.WriteLine($"Deleting test model file: {TestModelPath}");
                    File.Delete(TestModelPath);
                }
                else
                {
                    Console.WriteLine($"Test model file not found for cleanup: {TestModelPath}");
                }

                if (File.Exists(TestLogPath))
                {
                    Console.WriteLine($"Deleting test log file: {TestLogPath}");
                    File.Delete(TestLogPath);
                }
                else
                {
                    Console.WriteLine($"Test log file not found for cleanup: {TestLogPath}");
                }

                Console.WriteLine("Cleanup completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during cleanup: {ex.Message}");

                // Continue execution, don't throw from cleanup
            }
        }
    }
}