using System;
using System.Collections.Generic;
using System.Linq;
using TrubChess.Models;
using TrubChess.AI;

namespace TrubChess.Players
{
    public class AIPlayer : Player
    {
        public enum Difficulty
        {
            Easy,
            Medium,
            Hard,
            Reactive,
            Average,
            WorldChampion
        }

        private Difficulty _difficulty;
        private readonly ChessEngine _engine;
        private readonly Random _random;

        public AIPlayer(PieceColor color, Difficulty difficulty) : base(color)
        {
            _difficulty = difficulty;
            _engine = new ChessEngine();
            _random = new Random();
        }

        public void SetDifficulty(Difficulty difficulty)
        {
            _difficulty = difficulty;
        }

        public override ChessMove GetNextMove(ChessBoard board, GameState gameState)
        {
            int searchDepth = GetSearchDepth();
            float randomChance = GetRandomMoveChance();

            // For advanced AI difficulties, check if we have GitHub token
            if (IsAdvancedAIDifficulty() && !HasGitHubToken())
            {
                // Fall back to hard difficulty behavior if no token available
                searchDepth = 4;
                randomChance = 0.0f;
            }

            // Sometimes play a random move based on difficulty level
            if (_random.NextDouble() < randomChance)
            {
                return MakeRandomMove(gameState);
            }

            // For advanced difficulties, use enhanced move selection
            if (IsAdvancedAIDifficulty() && HasGitHubToken())
            {
                return FindAdvancedMove(board, gameState, searchDepth);
            }

            // Otherwise use the chess engine to calculate the best move
            return _engine.FindBestMove(board, gameState, searchDepth);
        }

        private bool IsAdvancedAIDifficulty()
        {
            return _difficulty == Difficulty.Reactive || 
                   _difficulty == Difficulty.Average || 
                   _difficulty == Difficulty.WorldChampion;
        }

        private bool HasGitHubToken()
        {
            return !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GITHUB_ACCESS_TOKEN"));
        }

        private ChessMove FindAdvancedMove(ChessBoard board, GameState gameState, int searchDepth)
        {
            // Enhanced move selection that considers move history for pattern recognition
            ChessMove bestMove = _engine.FindBestMove(board, gameState, searchDepth);

            // For reactive difficulty, analyze opponent's recent moves
            if (_difficulty == Difficulty.Reactive && gameState.MoveHistory.Count >= 2)
            {
                return FindReactiveMove(board, gameState, bestMove);
            }

            // For average and world champion, use deeper analysis
            return bestMove;
        }

        private ChessMove FindReactiveMove(ChessBoard board, GameState gameState, ChessMove defaultMove)
        {
            // Analyze the last few moves to detect patterns
            var recentMoves = gameState.MoveHistory.TakeLast(4).ToList();
            
            // Simple pattern detection: if opponent is advancing pawns, counter with piece development
            // This is a basic implementation - with GitHub token, this could use more sophisticated AI
            
            return defaultMove; // For now, return the default move
        }

        private int GetSearchDepth()
        {
            switch (_difficulty)
            {
                case Difficulty.Easy:
                    return 2;
                case Difficulty.Medium:
                    return 3;
                case Difficulty.Hard:
                    return 4;
                case Difficulty.Reactive:
                    return 3; // Similar to medium but with different behavior
                case Difficulty.Average:
                    return 5; // Deeper search than hard
                case Difficulty.WorldChampion:
                    return 6; // Maximum search depth
                default:
                    return 3;
            }
        }

        private float GetRandomMoveChance()
        {
            switch (_difficulty)
            {
                case Difficulty.Easy:
                    return 0.25f;  // 25% chance of random move
                case Difficulty.Medium:
                    return 0.1f;   // 10% chance of random move
                case Difficulty.Hard:
                    return 0.0f;   // Never play a random move
                case Difficulty.Reactive:
                    return 0.05f;  // 5% chance of random move for unpredictability
                case Difficulty.Average:
                    return 0.02f;  // 2% chance of random move
                case Difficulty.WorldChampion:
                    return 0.0f;   // Never play a random move
                default:
                    return 0.1f;
            }
        }

        private ChessMove MakeRandomMove(GameState gameState)
        {
            List<ChessPiece> pieces = gameState.Board.GetPieces(Color);
            List<ChessMove> allMoves = new List<ChessMove>();

            // Get all valid moves for all pieces
            foreach (var piece in pieces)
            {
                allMoves.AddRange(gameState.GetValidMovesFor(piece.Row, piece.Col));
            }

            // Return a random move, or null if no valid moves are available
            if (allMoves.Count > 0)
            {
                return allMoves[_random.Next(allMoves.Count)];
            }

            return null;
        }
    }
}
