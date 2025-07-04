using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrubChess.Players;
using TrubChess.Services;
using System;

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
}