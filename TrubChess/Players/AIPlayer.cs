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
            Hard
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

            // Sometimes play a random move based on difficulty level
            if (_random.NextDouble() < randomChance)
            {
                return MakeRandomMove(gameState);
            }

            // Otherwise use the chess engine to calculate the best move
            return _engine.FindBestMove(board, gameState, searchDepth);
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
