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

        public Difficulty AIDifficulty => _difficulty;

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
            switch (_difficulty)
            {
                case Difficulty.Easy:
                    return GetEasyMove(board, gameState);
                case Difficulty.Medium:
                    return GetMediumMove(board, gameState);
                case Difficulty.Hard:
                    return GetHardMove(board, gameState);
                case Difficulty.Reactive:
                    return GetReactiveMove(board, gameState);
                case Difficulty.Average:
                    return GetAverageMove(board, gameState);
                case Difficulty.WorldChampion:
                    return GetWorldChampionMove(board, gameState);
                default:
                    return GetMediumMove(board, gameState);
            }
        }

        private ChessMove GetEasyMove(ChessBoard board, GameState gameState)
        {
            int searchDepth = 2;
            float randomChance = 0.25f;

            if (_random.NextDouble() < randomChance)
            {
                return MakeRandomMove(gameState);
            }

            return _engine.FindBestMove(board, gameState, searchDepth);
        }

        private ChessMove GetMediumMove(ChessBoard board, GameState gameState)
        {
            int searchDepth = 3;
            float randomChance = 0.1f;

            if (_random.NextDouble() < randomChance)
            {
                return MakeRandomMove(gameState);
            }

            return _engine.FindBestMove(board, gameState, searchDepth);
        }

        private ChessMove GetHardMove(ChessBoard board, GameState gameState)
        {
            int searchDepth = 4;
            float randomChance = 0.0f;

            return _engine.FindBestMove(board, gameState, searchDepth);
        }

        private ChessMove GetReactiveMove(ChessBoard board, GameState gameState)
        {
            // TODO: Implement advanced AI logic using GitHub token
            return GetHardMove(board, gameState);
        }

        private ChessMove GetAverageMove(ChessBoard board, GameState gameState)
        {
            // TODO: Implement advanced AI logic using GitHub token
            return GetHardMove(board, gameState);
        }

        private ChessMove GetWorldChampionMove(ChessBoard board, GameState gameState)
        {
            // TODO: Implement advanced AI logic using GitHub token
            return GetHardMove(board, gameState);
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
