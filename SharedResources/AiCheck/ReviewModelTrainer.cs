namespace DataAccess.AiCheck
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Microsoft.ML;
    using static Microsoft.ML.DataOperationsCatalog;

    public class ReviewModelTrainer
    {
        private const int NumberOfTrees = 100;
        private const int NumberOfLeaves = 50;
        private const int MinimumExampleCountPerLeaf = 10;
        private const float LearningRate = 0.1f;
        private const float TestFraction = 0.2f;
        private const char CsvSeparator = '}';

        private static readonly string ProjectRoot = GetProjectRoot();
        private static readonly string DefaultDataPath = Path.Combine(ProjectRoot, "AiCheck", "review_data.csv");
        private static readonly string DefaultModelPath = Path.Combine(ProjectRoot, "Models", "curseword_model.zip");
        private static readonly string DefaultLogPath = Path.Combine(ProjectRoot, "Logs", "training_log.txt");

        public static bool TrainModel()
        {
            return TrainModel(ReviewModelTrainer.DefaultDataPath, ReviewModelTrainer.DefaultModelPath, ReviewModelTrainer.DefaultLogPath);
        }

        public static bool TrainModel(string customDataPath)
        {
            string modelPath = Path.Combine(Path.GetDirectoryName(customDataPath), "test_model.zip");
            string logPath = Path.Combine(Path.GetDirectoryName(customDataPath), "test_training_log.txt");
            return TrainModel(customDataPath, modelPath, logPath);
        }

        public static bool TrainModel(string dataPath, string modelPath, string logPath)
        {
            LogToFile($"Starting model training process. Project root: {ReviewModelTrainer.ProjectRoot}", logPath);
            LogToFile($"Looking for training data at: {dataPath}", logPath);

            if (!File.Exists(dataPath))
            {
                LogToFile($"ERROR: Missing training data file at {dataPath}", logPath);
                return false;
            }

            try
            {
                EnsureDirectoriesExist(modelPath, logPath);

                MLContext machineLearningContext = new MLContext(seed: 0);

                IDataView trainingData = LoadTrainingData(machineLearningContext, dataPath);

                IEstimator<ITransformer> modelPipeline = CreateModelPipeline(machineLearningContext);

                TrainTestData trainTestSplit = machineLearningContext.Data.TrainTestSplit(trainingData, testFraction: ReviewModelTrainer.TestFraction);

                ITransformer trainedModel = modelPipeline.Fit(trainTestSplit.TrainSet);

                EvaluateModel(machineLearningContext, trainedModel, trainTestSplit.TestSet, logPath);

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

        private static string GetProjectRoot([CallerFilePath] string filePath = "")
        {
            DirectoryInfo? directory = new FileInfo(filePath).Directory;
            while (directory != null && !directory.GetFiles("*.csproj").Any())
            {
                directory = directory.Parent;
            }

            return directory?.FullName ?? throw new Exception("Project root not found!");
        }

        private static void EnsureDirectoriesExist(string modelPath, string logPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(modelPath));
            Directory.CreateDirectory(Path.GetDirectoryName(logPath));
        }

        private static IDataView LoadTrainingData(MLContext machineLearningContext, string dataPath)
        {
            if (!ValidateDataFormat(dataPath))
            {
                throw new InvalidOperationException("Invalid data format. Expected separator character is '" + ReviewModelTrainer.CsvSeparator + "'.");
            }

            return machineLearningContext.Data.LoadFromTextFile<ReviewData>(
                path: dataPath,
                separatorChar: ReviewModelTrainer.CsvSeparator,
                hasHeader: true);
        }

        private static bool ValidateDataFormat(string dataPath)
        {
            try
            {
                string? firstLine = File.ReadLines(dataPath).FirstOrDefault();
                if (string.IsNullOrEmpty(firstLine))
                {
                    return false;
                }

                if (!firstLine.Contains(ReviewModelTrainer.CsvSeparator))
                {
                    return false;
                }

                string[] headerColumns = firstLine.Split(CsvSeparator);
                if (headerColumns.Length < 2 ||
                    !headerColumns[0].Trim().Equals("ReviewContent", StringComparison.OrdinalIgnoreCase) ||
                    !headerColumns[1].Trim().Equals("IsOffensiveContent", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                bool hasDataRow = false;
                foreach (string line in File.ReadLines(dataPath).Skip(1))
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    if (!line.Contains(ReviewModelTrainer.CsvSeparator))
                    {
                        return false;
                    }

                    string[] columns = line.Split(ReviewModelTrainer.CsvSeparator);
                    if (columns.Length < 2)
                    {
                        return false;
                    }

                    bool isBoolean = bool.TryParse(columns[1].Trim(), out _);
                    bool isInteger = int.TryParse(columns[1].Trim(), out int value);

                    if (!isBoolean && (!isInteger || (value != 0 && value != 1)))
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

        private static IEstimator<ITransformer> CreateModelPipeline(MLContext machineLearningContext)
        {
            return machineLearningContext.Transforms.Text.FeaturizeText(
                outputColumnName: "Features",
                inputColumnName: nameof(ReviewData.ReviewContent))
            .Append(machineLearningContext.BinaryClassification.Trainers.FastTree(
                labelColumnName: nameof(ReviewData.IsOffensiveContent),
                featureColumnName: "Features",
                numberOfTrees: ReviewModelTrainer.NumberOfTrees,
                numberOfLeaves: ReviewModelTrainer.NumberOfLeaves,
                minimumExampleCountPerLeaf: MinimumExampleCountPerLeaf,
                learningRate: ReviewModelTrainer.LearningRate));
        }

        private static void EvaluateModel(MLContext machineLearningContext, ITransformer trainedModel, IDataView testData, string logPath)
        {
            IDataView predictions = trainedModel.Transform(testData);

            List<ReviewPrediction> predictedResults = machineLearningContext.Data.CreateEnumerable<ReviewPrediction>(predictions, reuseRowObject: false).ToList();
            List<ReviewData> actualResults = machineLearningContext.Data.CreateEnumerable<ReviewData>(testData, reuseRowObject: false).ToList();

            int correctPredictions = 0;
            int totalPredictions = predictedResults.Count;

            for (int index = 0; index < predictedResults.Count; index++)
            {
                ReviewPrediction prediction = predictedResults[index];
                ReviewData actual = actualResults[index];

                if (prediction.IsPredictedOffensive != actual.IsOffensiveContent)
                {
                    LogToFile($"Mistake in Review {index + 1}: Predicted {prediction.IsPredictedOffensive}, Actual {actual.IsOffensiveContent}. Text: {actual.ReviewContent}", logPath);
                }
                else
                {
                    correctPredictions++;
                }
            }

            float accuracy = (float)correctPredictions / totalPredictions * 100;
            LogToFile($"Model evaluation complete. Accuracy: {accuracy:F2}% ({correctPredictions}/{totalPredictions} correct predictions)", logPath);
        }

        private static void LogToFile(string message, string logPath)
        {
            try
            {
                string timestampedMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
                File.AppendAllText(logPath, timestampedMessage + Environment.NewLine);
            }
            catch
            {
            }
        }
    }
}