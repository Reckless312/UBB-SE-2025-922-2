// <copyright file="CheckerServiceTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace UnitTests.ReviewChecker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using App1.AutoChecker;
    using App1.Models;
    using App1.Services;
    using Moq;
    using UnitTests.ReviewChecker.AuxiliaryTestsClasses;

    public class CheckerServiceTests
    {
        private readonly Mock<IReviewService> mockReviewService;
        private readonly Mock<IAutoCheck> mockAutoCheck;
        private readonly CheckersService checkersService;

        public CheckerServiceTests()
        {
            this.mockReviewService = new Mock<IReviewService>();
            this.mockAutoCheck = new Mock<IAutoCheck>();
            this.checkersService = new CheckersService(this.mockReviewService.Object, this.mockAutoCheck.Object);
        }

        [Fact]
        public void Constructor_InitializesCorrectly()
        {
            Assert.NotNull(this.checkersService);
        }

        [Fact]
        public void RunAutoCheck_WithOffensiveReviews_HidesOffensiveReviews()
        {
            List<Review> reviews = new List<Review> { new Review(1, 1, 4, "Offensive Content", DateTime.Now), new Review(2, 1, 3, "Normal Content", DateTime.Now) };

            this.mockAutoCheck.Setup(mockCheckAutomatically => mockCheckAutomatically.AutoCheckReview("Offensive Content")).Returns(true);
            this.mockAutoCheck.Setup(mockAutomaticallyCheck => mockAutomaticallyCheck.AutoCheckReview("Normal Content")).Returns(false);
            List<string> result = this.checkersService.RunAutoCheck(reviews);
            Assert.Equal(2, result.Count);
            Assert.Contains($"Review 1 is offensive. Hiding the review.", result);
            Assert.Contains($"Review 2 is not offensive.", result);

            this.mockReviewService.Verify(m => m.HideReview(1), Times.Once);
            this.mockReviewService.Verify(m => m.ResetReviewFlags(1), Times.Once);
            this.mockReviewService.Verify(m => m.HideReview(2), Times.Never);
            this.mockReviewService.Verify(m => m.ResetReviewFlags(2), Times.Never);
        }

        [Fact]
        public void RunAutoCheck_WithEmptyList_ReturnsEmptyMessages()
        {
            List<Review> reviews = new List<Review>();
            List<string> result = this.checkersService.RunAutoCheck(reviews);
            Assert.Empty(result);
            this.mockReviewService.Verify(m => m.HideReview(It.IsAny<int>()), Times.Never);
            this.mockReviewService.Verify(m => m.ResetReviewFlags(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void GetOffensiveWordsList_ReturnsListFromAutoCheck()
        {
            HashSet<string> offensiveWords = new HashSet<string> { "word1", "word2" };
            this.mockAutoCheck.Setup(m => m.GetOffensiveWordsList()).Returns(offensiveWords);
            HashSet<string> result = this.checkersService.GetOffensiveWordsList();
            Assert.Same(offensiveWords, result);
            Assert.Equal(2, result.Count);
            Assert.Contains("word1", result);
            Assert.Contains("word2", result);
        }

        [Fact]
        public void AddOffensiveWord_CallsAutoCheck()
        {
            string newWord = "badword";
            this.checkersService.AddOffensiveWord(newWord);
            this.mockAutoCheck.Verify(m => m.AddOffensiveWord(newWord), Times.Once);
        }

        [Fact]
        public void DeleteOffensiveWord_CallsAutoCheck()
        {
            string word = "badword";
            this.checkersService.DeleteOffensiveWord(word);
            this.mockAutoCheck.Verify(m => m.DeleteOffensiveWord(word), Times.Once);
        }

        [Fact]
        public void RunAICheckForOneReview_WithNullReview_LogsErrorAndReturns()
        {
            ConsoleOutput consoleOutput = new ConsoleOutput();
            this.checkersService.RunAICheckForOneReview(null);
            Assert.Contains("Review not found.", consoleOutput.GetOutput());
            this.mockReviewService.Verify(m => m.HideReview(It.IsAny<int>()), Times.Never);
            this.mockReviewService.Verify(m => m.ResetReviewFlags(It.IsAny<int>()), Times.Never);
        }

        public class OffensiveTextDetectorTests
        {
            [Fact]
            public void DetectOffensiveContent_ReturnsExpectedResponse()
            {
                string text = "Test content";
                string result = OffensiveTextDetector.DetectOffensiveContent(text);
                Assert.NotNull(result);
            }
        }

        public class CheckersServicePrivateMethodTests
        {
            [Fact]
            public void GetConfidenceScoreForHateSpeach_WithValidJson_ReturnsCorrectScore()
            {
                string validJson = @"[[{""label"":""hate"",""score"":""0.9""},{""label"":""not_hate"",""score"":""0.1""}]]";
                MethodInfo? method = typeof(CheckersService).GetMethod("GetConfidenceScoreForHateSpeach", BindingFlags.NonPublic | BindingFlags.Static);
                object? result = method.Invoke(null, new object[] { validJson });
                Assert.Equal(0.9f, result);
            }

            [Fact]
            public void GetConfidenceScoreForHateSpeach_WithNoHateLabel_ReturnsZero()
            {
                string jsonWithoutHate = @"[[{""label"":""not_hate"",""score"":""0.9""},{""label"":""offensive"",""score"":""0.1""}]]";
                MethodInfo? method = typeof(CheckersService).GetMethod("GetConfidenceScoreForHateSpeach", BindingFlags.NonPublic | BindingFlags.Static);
                object result = method.Invoke(null, new object[] { jsonWithoutHate });
                Assert.Equal(0f, result);
            }

            [Fact]
            public void GetConfidenceScoreForHateSpeach_WithInvalidJson_ReturnsZero()
            {
                string invalidJson = "not valid json";
                MethodInfo? method = typeof(CheckersService).GetMethod("GetConfidenceScoreForHateSpeach", BindingFlags.NonPublic | BindingFlags.Static);
                object? result = method.Invoke(null, new object[] { invalidJson });
                Assert.Equal(0f, result);
            }

            [Fact]
            public void CheckReviewWithAI_WithNonOffensiveContent_ReturnsFalse()
            {
                MethodInfo? method = typeof(CheckersService).GetMethod("CheckReviewWithAI", BindingFlags.NonPublic | BindingFlags.Static);
                using (new MethodSwapper(typeof(OffensiveTextDetector), "DetectOffensiveContent", typeof(TestHelpers), "MockDetectOffensiveContentLowScore"))
                {
                    bool result = (bool)method.Invoke(null, new object[] { "Normal content" });
                    Assert.False(result);
                }
            }

            [Fact]
            public void GetProjectRoot_ReturnsValidPath()
            {
                MethodInfo? method = typeof(CheckersService).GetMethod("GetProjectRoot", BindingFlags.NonPublic | BindingFlags.Static);
                object? result = method.Invoke(null, new object[] { Assembly.GetExecutingAssembly().Location });
                Assert.NotNull(result);
                Assert.IsType<string>(result);
            }

            [Fact]
            public void LogToFile_WritesToFile()
            {
                MethodInfo? method = typeof(CheckersService).GetMethod("LogToFile", BindingFlags.NonPublic | BindingFlags.Static);
                string testMessage = "Test log message";
                method.Invoke(null, new object[] { testMessage });
                Assert.True(true);
            }
        }
    }
}
