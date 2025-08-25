using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrubChess.Players;
using TrubChess.Services;
using AIChess.Services;
using System;
using System.Windows.Media;

namespace TrubChess.Tests
{
    [TestClass]
    public class AIPlayerTests
    {
        [TestMethod]
        public void TestNewDifficultyLevels()
        {
            // Test that new difficulty levels can be created
            var reactiveAI = new AIPlayer(Models.PieceColor.Black, AIPlayer.Difficulty.Reactive);
            var averageAI = new AIPlayer(Models.PieceColor.Black, AIPlayer.Difficulty.Average);
            var worldChampionAI = new AIPlayer(Models.PieceColor.Black, AIPlayer.Difficulty.WorldChampion);

            Assert.IsNotNull(reactiveAI);
            Assert.IsNotNull(averageAI);
            Assert.IsNotNull(worldChampionAI);
        }

        [TestMethod]
        public void TestDifficultyCanBeSet()
        {
            var aiPlayer = new AIPlayer(Models.PieceColor.Black, AIPlayer.Difficulty.Easy);
            
            // Test setting each new difficulty level
            aiPlayer.SetDifficulty(AIPlayer.Difficulty.Reactive);
            aiPlayer.SetDifficulty(AIPlayer.Difficulty.Average);
            aiPlayer.SetDifficulty(AIPlayer.Difficulty.WorldChampion);
            
            // If we get here without exceptions, the test passes
            Assert.IsTrue(true);
        }
    }

    [TestClass]
    public class GitHubTokenManagerTests
    {
        [TestMethod]
        public void TestGetGitHubToken()
        {
            // Test that the method doesn't throw an exception
            string token = GitHubTokenManager.GetGitHubToken();
            
            // Token can be null or empty, that's expected behavior
            Assert.IsTrue(token == null || token is string);
        }

        [TestMethod]
        public void TestHasGitHubToken()
        {
            // Test that the method returns a boolean
            bool hasToken = GitHubTokenManager.HasGitHubToken();
            
            Assert.IsTrue(hasToken == true || hasToken == false);
        }
    }

    [TestClass]
    public class RookColorCustomizationTests
    {
        private DatabaseService _databaseService;

        [TestInitialize]
        public void Setup()
        {
            _databaseService = new DatabaseService();
            _databaseService.InitializeDatabase();
        }

        [TestMethod]
        public void TestSaveAndLoadRookColor()
        {
            // Arrange
            string colorKey = "TestRookColor";
            string expectedColor = "#FFFF0000"; // Red color

            // Act
            _databaseService.SaveColorSetting(colorKey, expectedColor);
            string actualColor = _databaseService.LoadColorSetting(colorKey);

            // Assert
            Assert.AreEqual(expectedColor, actualColor);
        }

        [TestMethod]
        public void TestLoadNonexistentRookColor()
        {
            // Arrange
            string nonexistentKey = "NonexistentRookColor";

            // Act
            string result = _databaseService.LoadColorSetting(nonexistentKey);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void TestDatabaseConnection()
        {
            // Act
            bool connectionTest = _databaseService.TestConnection();

            // Assert
            Assert.IsTrue(connectionTest);
        }

        [TestMethod]
        public void TestRookColorSettingsCanBeOverwritten()
        {
            // Arrange
            string colorKey = "TestRookColorOverwrite";
            string firstColor = "#FF0000FF"; // Blue
            string secondColor = "#FF00FF00"; // Green

            // Act
            _databaseService.SaveColorSetting(colorKey, firstColor);
            string firstResult = _databaseService.LoadColorSetting(colorKey);
            
            _databaseService.SaveColorSetting(colorKey, secondColor);
            string secondResult = _databaseService.LoadColorSetting(colorKey);

            // Assert
            Assert.AreEqual(firstColor, firstResult);
            Assert.AreEqual(secondColor, secondResult);
            Assert.AreNotEqual(firstResult, secondResult);
        }
    }
}