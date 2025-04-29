// <copyright file="ReviewModelTrainer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DrinkDb_Auth.AiCheck
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Microsoft.ML;
    using Microsoft.ML.Data;
    using Microsoft.ML.FastTree;
    using static Microsoft.ML.DataOperationsCatalog;

    /// <summary>
    /// Handles the training and evaluation of the review content classification model.
    /// </summary>
    public class ReviewModelTrainer
    {
        // Model training parameters
        private const int NumberOfTrees = 100;
        private const int NumberOfLeaves = 50;
        private const int MinimumExampleCountPerLeaf = 10;
        private const float LearningRate = 0.1f;
        private const float TestFraction = 0.2f;
        private const char CsvSeparator = '}';

        // File paths
        private static readonly string ProjectRoot = GetProjectRoot();
        private static readonly string DefaultDataPath = Path.Combine(ProjectRoot, "AiCheck", "review_data.csv");
        private static readonly string DefaultModelPath = Path.Combine(ProjectRoot, "Models", "curseword_model.zip");
        private static readonly string DefaultLogPath = Path.Combine(ProjectRoot, "Logs", "training_log.txt");

        /// <summary>
        /// Trains a binary classification model to detect offensive content in reviews.
        /// </summary>
        /// <returns>True if training was successful, false otherwise.</returns>
        public static bool TrainModel()
        {
            return TrainModel(DefaultDataPath, DefaultModelPath, DefaultLogPath);
        }

        /// <summary>
        /// Trains a binary classification model to detect offensive content in reviews using a custom data path.
        /// </summary>
        /// <param name="customDataPath">Optional custom path to the training data file.</param>
        /// <returns>True if training was successful, false otherwise.</returns>
        public static bool TrainModel(string customDataPath)
        {
            string modelPath = Path.Combine(Path.GetDirectoryName(customDataPath), "test_model.zip");
            string logPath = Path.Combine(Path.GetDirectoryName(customDataPath), "test_training_log.txt");
            return TrainModel(customDataPath, modelPath, logPath);
        }

        /// <summary>
        /// Trains a binary classification model to detect offensive content in reviews using custom paths.
        /// </summary>
        /// <param name="dataPath">Path to the training data file.</param>
        /// <param name="modelPath">Path where the trained model will be saved.</param>
        /// <param name="logPath">Path where the training log will be saved.</param>
        /// <returns>True if training was successful, false otherwise.</returns>
        public static bool TrainModel(string dataPath, string modelPath, string logPath)
        {
            LogToFile($"Starting model training process. Project root: {ProjectRoot}", logPath);
            LogToFile($"Looking for training data at: {dataPath}", logPath);

            if (!File.Exists(dataPath))
            {
                LogToFile($"ERROR: Missing training data file at {dataPath}", logPath);
                return false;
            }

            try
            {
                // Ensure directories exist
                EnsureDirectoriesExist(modelPath, logPath);

                // Initialize MLContext with a fixed seed for reproducibility
                MLContext machineLearningContext = new MLContext(seed: 0);

                // Load and prepare the training data
                IDataView trainingData = LoadTrainingData(machineLearningContext, dataPath);

                // Create and configure the model pipeline
                IEstimator<ITransformer> modelPipeline = CreateModelPipeline(machineLearningContext);

                // Split data into training and testing sets
                TrainTestData trainTestSplit = machineLearningContext.Data.TrainTestSplit(trainingData, testFraction: TestFraction);

                // Train the model
                ITransformer trainedModel = modelPipeline.Fit(trainTestSplit.TrainSet);

                // Evaluate the model on the test set
                EvaluateModel(machineLearningContext, trainedModel, trainTestSplit.TestSet, logPath);

                // Save the trained model
                machineLearningContext.Model.Save(trainedModel, trainingData.Schema, modelPath);

                LogToFile($"Model trained and saved successfully at {modelPath}", logPath);
                return true;
            }
            catch (FileNotFoundException fileNotFoundException)
            {
                LogToFile($"File not found error: {fileNotFoundException.Message}", logPath);
                return false;
            }
            catch (InvalidOperationException invalidOperationException)
            {
                LogToFile($"Invalid operation error: {invalidOperationException.Message}", logPath);
                return false;
            }
            catch (Exception exception)
            {
                LogToFile($"Unexpected error during model training: {exception.Message}", logPath);
                return false;
            }
        }

        /// <summary>
        /// Gets the project root directory by traversing up from the current file.
        /// </summary>
        /// <param name="filePath">The path of the current file (automatically provided by the compiler).</param>
        /// <returns>The full path to the project root directory.</returns>
        /// <exception cref="Exception">Thrown when the project root cannot be found.</exception>
        private static string GetProjectRoot([CallerFilePath] string filePath = "")
        {
            DirectoryInfo? directory = new FileInfo(filePath).Directory;
            while (directory != null && !directory.GetFiles("*.csproj").Any())
            {
                directory = directory.Parent;
            }

            return directory?.FullName ?? throw new Exception("Project root not found!");
        }

        /// <summary>
        /// Ensures that all required directories exist.
        /// </summary>
        private static void EnsureDirectoriesExist(string modelPath, string logPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(modelPath));
            Directory.CreateDirectory(Path.GetDirectoryName(logPath));
        }

        /// <summary>
        /// Loads the training data from the CSV file.
        /// </summary>
        /// <param name="machineLearningContext">The ML.NET context.</param>
        /// <param name="dataPath">The path to the training data file.</param>
        /// <returns>The loaded training data.</returns>
        private static IDataView LoadTrainingData(MLContext machineLearningContext, string dataPath)
        {
            // Validate the data format before attempting to load it
            if (!ValidateDataFormat(dataPath))
            {
                throw new InvalidOperationException("Invalid data format. Expected separator character is '" + CsvSeparator + "'.");
            }

            return machineLearningContext.Data.LoadFromTextFile<ReviewData>(
                path: dataPath,
                separatorChar: CsvSeparator,
                hasHeader: true);
        }

        /// <summary>
        /// Validates the format of the training data file.
        /// </summary>
        /// <param name="dataPath">The path to the training data file.</param>
        /// <returns>True if the data format is valid, false otherwise.</returns>
        private static bool ValidateDataFormat(string dataPath)
        {
            try
            {
                // Read the first line to check the header format
                string? firstLine = File.ReadLines(dataPath).FirstOrDefault();
                if (string.IsNullOrEmpty(firstLine))
                {
                    return false;
                }

                // Check if the header contains the expected separator
                if (!firstLine.Contains(CsvSeparator))
                {
                    return false;
                }

                // Check if the header has the expected columns
                string[] headerColumns = firstLine.Split(CsvSeparator);
                if (headerColumns.Length < 2 ||
                    !headerColumns[0].Trim().Equals("ReviewContent", StringComparison.OrdinalIgnoreCase) ||
                    !headerColumns[1].Trim().Equals("IsOffensiveContent", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                // Check at least one data row
                bool hasDataRow = false;
                foreach (string line in File.ReadLines(dataPath).Skip(1))
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    if (!line.Contains(CsvSeparator))
                    {
                        return false;
                    }

                    string[] columns = line.Split(CsvSeparator);
                    if (columns.Length < 2)
                    {
                        return false;
                    }

                    // Check if the second column is a valid boolean value (0 or 1)
                    bool isBoolean = bool.TryParse(columns[1].Trim(), out _);
                    bool isInteger = int.TryParse(columns[1].Trim(), out int value);

                    if (!isBoolean && (!isInteger || value != 0 && value != 1))
                    {
                        return false;
                    }

                    hasDataRow = true;
                    break;
                }

                return hasDataRow;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Creates the model pipeline for training.
        /// </summary>
        /// <param name="machineLearningContext">The ML.NET context.</param>
        /// <returns>The configured model pipeline.</returns>
        private static IEstimator<ITransformer> CreateModelPipeline(MLContext machineLearningContext)
        {
            return machineLearningContext.Transforms.Text.FeaturizeText(
                outputColumnName: "Features",
                inputColumnName: nameof(ReviewData.ReviewContent))
            .Append(machineLearningContext.BinaryClassification.Trainers.FastTree(
                labelColumnName: nameof(ReviewData.IsOffensiveContent),
                featureColumnName: "Features",
                numberOfTrees: NumberOfTrees,
                numberOfLeaves: NumberOfLeaves,
                minimumExampleCountPerLeaf: MinimumExampleCountPerLeaf,
                learningRate: LearningRate));
        }

        /// <summary>
        /// Evaluates the trained model on the test dataset.
        /// </summary>
        /// <param name="machineLearningContext">The ML.NET context.</param>
        /// <param name="trainedModel">The trained model.</param>
        /// <param name="testData">The test dataset.</param>
        /// <param name="logPath">Path to the log file.</param>
        private static void EvaluateModel(MLContext machineLearningContext, ITransformer trainedModel, IDataView testData, string logPath)
        {
            // Transform the test data using the trained model
            IDataView predictions = trainedModel.Transform(testData);

            // Convert predictions to a list for easy comparison
            List<ReviewPrediction> predictedResults = machineLearningContext.Data.CreateEnumerable<ReviewPrediction>(predictions, reuseRowObject: false).ToList();
            List<ReviewData> actualResults = machineLearningContext.Data.CreateEnumerable<ReviewData>(testData, reuseRowObject: false).ToList();

            // Compare predictions with actual values and log mistakes
            int correctPredictions = 0;
            int totalPredictions = predictedResults.Count;

            for (int index = 0; index < predictedResults.Count; index++)
            {
                ReviewPrediction prediction = predictedResults[index];
                ReviewData actual = actualResults[index];

                // If the prediction is incorrect, log it
                if (prediction.IsPredictedOffensive != actual.IsOffensiveContent)
                {
                    LogToFile($"Mistake in Review {index + 1}: Predicted {prediction.IsPredictedOffensive}, Actual {actual.IsOffensiveContent}. Text: {actual.ReviewContent}", logPath);
                }
                else
                {
                    correctPredictions++;
                }
            }

            // Log overall accuracy
            float accuracy = (float)correctPredictions / totalPredictions * 100;
            LogToFile($"Model evaluation complete. Accuracy: {accuracy:F2}% ({correctPredictions}/{totalPredictions} correct predictions)", logPath);
        }

        /// <summary>
        /// Logs a message to the log file with a timestamp.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="logPath">Path to the log file.</param>
        private static void LogToFile(string message, string logPath)
        {
            try
            {
                string timestampedMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
                File.AppendAllText(logPath, timestampedMessage + Environment.NewLine);
                Console.WriteLine(timestampedMessage);
            }
            catch (Exception exception)
            {
                // Fallback to console if logging fails
                Console.WriteLine($"LOG FAILED: {message}. Error: {exception.Message}");
            }
        }
    }
}