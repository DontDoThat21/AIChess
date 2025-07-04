using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrubChess.Players;
using TrubChess.Services;
using TrubChess.Models;
using System;

namespace TrubChess.Tests
{
    [TestClass]
    public class IntegrationTests
    {
        [TestMethod]
        public void TestAdvancedAIFallbackBehavior()
        {
            // Create AI player with advanced difficulty
            var aiPlayer = new AIPlayer(PieceColor.Black, AIPlayer.Difficulty.WorldChampion);
            
            // Create a simple game state for testing
            var gameState = new GameState();
            var board = gameState.Board;
            
            // Test that AI can get a move even without GitHub token
            // (should fall back to standard behavior)
            var move = aiPlayer.GetNextMove(board, gameState);
            
            // Move might be null if no valid moves, but method should not throw exception
            Assert.IsTrue(move == null || move is ChessMove);
        }

        [TestMethod]
        public void TestAllDifficultyLevelsCanBeSet()
        {
            var aiPlayer = new AIPlayer(PieceColor.Black, AIPlayer.Difficulty.Easy);
            
            // Test setting each difficulty level
            foreach (AIPlayer.Difficulty difficulty in Enum.GetValues(typeof(AIPlayer.Difficulty)))
            {
                aiPlayer.SetDifficulty(difficulty);
                // If we get here without exception, the test passes
            }
            
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestGitHubTokenEnvironmentVariableCheck()
        {
            // Save original value
            string originalToken = Environment.GetEnvironmentVariable("GITHUB_ACCESS_TOKEN");
            
            try
            {
                // Test with no token
                Environment.SetEnvironmentVariable("GITHUB_ACCESS_TOKEN", null);
                Assert.IsFalse(GitHubTokenManager.HasGitHubToken());
                
                // Test with empty token
                Environment.SetEnvironmentVariable("GITHUB_ACCESS_TOKEN", "");
                Assert.IsFalse(GitHubTokenManager.HasGitHubToken());
                
                // Test with whitespace token
                Environment.SetEnvironmentVariable("GITHUB_ACCESS_TOKEN", "   ");
                Assert.IsFalse(GitHubTokenManager.HasGitHubToken());
                
                // Test with valid token
                Environment.SetEnvironmentVariable("GITHUB_ACCESS_TOKEN", "test-token");
                Assert.IsTrue(GitHubTokenManager.HasGitHubToken());
            }
            finally
            {
                // Restore original value
                Environment.SetEnvironmentVariable("GITHUB_ACCESS_TOKEN", originalToken);
            }
        }
    }
}