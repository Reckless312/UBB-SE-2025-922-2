using System;
using System.Collections.Generic;
using DrinkDb_Auth.AutoChecker;
using IRepository;
using Moq;
using Xunit;

namespace UnitTests.Autocheck
{
    public class AutoCheckTests
    {
        private readonly Mock<IOffensiveWordsRepository> mockRepository;
        private readonly HashSet<string> testOffensiveWords;
        private readonly AutoCheck autoCheck;

        public AutoCheckTests()
        {
            this.testOffensiveWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "bad", "offensive", "inappropriate" };

            this.mockRepository = new Mock<IOffensiveWordsRepository>();
            this.mockRepository.Setup(r => r.LoadOffensiveWords()).Returns(this.testOffensiveWords);

            this.autoCheck = new AutoCheck(this.mockRepository.Object);
        }

        [Fact]
        public void Constructor_NullRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new AutoCheck(null));
        }

        [Fact]
        public void AutoCheckReview_NullReviewText_ReturnsFalse()
        {
            bool result = this.autoCheck.AutoCheckReview(null);
            Assert.False(result);
        }

        [Fact]
        public void AutoCheckReview_EmptyReviewText_ReturnsFalse()
        {
            bool result = this.autoCheck.AutoCheckReview(string.Empty);
            Assert.False(result);
        }

        [Fact]
        public void AutoCheckReview_WhitespaceReviewText_ReturnsFalse()
        {
            bool result = this.autoCheck.AutoCheckReview("   ");
            Assert.False(result);
        }

        [Fact]
        public void AutoCheckReview_ContainsOffensiveWord_ReturnsTrue()
        {
            string reviewText = "This review contains an offensive word.";
            bool result = this.autoCheck.AutoCheckReview(reviewText);
            Assert.True(result);
        }

        [Fact]
        public void AutoCheckReview_ContainsOffensiveWordWithDifferentCase_ReturnsTrue()
        {
            string reviewText = "This review contains an OFFENSIVE word.";
            bool result = this.autoCheck.AutoCheckReview(reviewText);
            Assert.True(result);
        }

        [Fact]
        public void AutoCheckReview_NoOffensiveWords_ReturnsFalse()
        {
            string reviewText = "This review is perfectly fine and acceptable.";
            bool result = this.autoCheck.AutoCheckReview(reviewText);
            Assert.False(result);
        }

        [Fact]
        public void AutoCheckReview_OffensiveWordSurroundedByDelimiters_ReturnsTrue()
        {
            string reviewText = "This review has!bad,punctuation.";
            bool result = this.autoCheck.AutoCheckReview(reviewText);
            Assert.True(result);
        }

        [Fact]
        public void AddOffensiveWord_NewWord_AddsToRepositoryAndLocalCache()
        {
            string newWord = "terrible";
            this.mockRepository.Setup(r => r.AddWord(newWord)).Verifiable();
            this.autoCheck.AddOffensiveWord(newWord);
            HashSet<string> words = this.autoCheck.GetOffensiveWordsList();
            this.mockRepository.Verify(r => r.AddWord(newWord), Times.Once);
            Assert.Contains(newWord, words);
        }

        [Fact]
        public void AddOffensiveWord_ExistingWord_DoesNotAddAgain()
        {
            string existingWord = "offensive";
            this.autoCheck.AddOffensiveWord(existingWord);
            this.mockRepository.Verify(r => r.AddWord(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void AddOffensiveWord_ExistingWordDifferentCase_DoesNotAddAgain()
        {
            string existingWord = "OFFENSIVE";
            this.autoCheck.AddOffensiveWord(existingWord);
            this.mockRepository.Verify(r => r.AddWord(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void AddOffensiveWord_NullOrEmptyWord_DoesNothing()
        {
            this.autoCheck.AddOffensiveWord(null);
            this.autoCheck.AddOffensiveWord(string.Empty);
            this.autoCheck.AddOffensiveWord("   ");
            this.mockRepository.Verify(r => r.AddWord(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void DeleteOffensiveWord_ExistingWord_RemovesFromRepositoryAndLocalCache()
        {
            string wordToDelete = "offensive";
            this.mockRepository.Setup(r => r.DeleteWord(wordToDelete)).Verifiable();
            this.autoCheck.DeleteOffensiveWord(wordToDelete);
            HashSet<string> words = this.autoCheck.GetOffensiveWordsList();
            this.mockRepository.Verify(r => r.DeleteWord(wordToDelete), Times.Once);
            Assert.DoesNotContain(wordToDelete, words);
        }

        [Fact]
        public void DeleteOffensiveWord_NonExistingWord_DoesNothing()
        {
            string nonExistingWord = "nonexistent";
            this.autoCheck.DeleteOffensiveWord(nonExistingWord);
            this.mockRepository.Verify(r => r.DeleteWord(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void DeleteOffensiveWord_NullOrEmptyWord_DoesNothing()
        {
            this.autoCheck.DeleteOffensiveWord(null);
            this.autoCheck.DeleteOffensiveWord(string.Empty);
            this.autoCheck.DeleteOffensiveWord("   ");

            this.mockRepository.Verify(r => r.DeleteWord(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void GetOffensiveWordsList_ReturnsNewInstanceWithSameContent()
        {
            HashSet<string> result = this.autoCheck.GetOffensiveWordsList();
            Assert.Equal(this.testOffensiveWords.Count, result.Count);
            foreach (string word in this.testOffensiveWords)
            {
                Assert.Contains(word, result);
            }

            Assert.NotSame(this.testOffensiveWords, result);
        }
    }
}