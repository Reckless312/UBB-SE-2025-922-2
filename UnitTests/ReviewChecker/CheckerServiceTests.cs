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
    using DataAccess.Model.AdminDashboard;
    using DrinkDb_Auth.AutoChecker;
    using DrinkDb_Auth.Service.AdminDashboard;
    using DrinkDb_Auth.Service.AdminDashboard.Components;
    using DrinkDb_Auth.Service.AdminDashboard.Interfaces;
    using Moq;
    using UnitTests.ReviewChecker.AuxiliaryTestsClasses;
    using Xunit;

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
            List<Review> reviews = new List<Review> { new Review(1, new Guid(), 4, "Offensive Content", DateTime.Now), new Review(2, new Guid(), 3, "Normal Content", DateTime.Now) };

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
    }
}
