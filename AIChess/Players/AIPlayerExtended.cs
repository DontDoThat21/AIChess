using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AIChess.Models;
using AIChess.Models.Pieces;
using AIChess.AI;
using AIChess.Services;
using AIChess.Extensions;

namespace AIChess.Players
{
    public class AIPlayerExtended : Player
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
        private readonly OpenAIChessService _openAIService;
        private readonly GitHubTokenManager _githubTokenManager;

        public Difficulty AIDifficulty => _difficulty;

        public AIPlayerExtended(PieceColor color, Difficulty difficulty) : base(color)
        {
            _difficulty = difficulty;
            _engine = new ChessEngine();
            _random = new Random();
            _githubTokenManager = new GitHubTokenManager();
            
            // Initialize OpenAI service for World Champion difficulty
            if (_difficulty == Difficulty.WorldChampion)
            {
                try
                {
                    _openAIService = new OpenAIChessService();
                }
                catch (Exception)
                {
                    // If OpenAI service fails to initialize, fall back to traditional AI
                    _openAIService = null;
                }
            }
        }

        public void SetDifficulty(Difficulty difficulty)
        {
            _difficulty = difficulty;
            
            // Initialize OpenAI service if switching to World Champion
            if (_difficulty == Difficulty.WorldChampion && _openAIService == null)
            {
                try
                {
                    _openAIService = new OpenAIChessService();
                }
                catch (Exception)
                {
                    _openAIService = null;
                }
            }
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

        private ChessMove GetWorldChampionMove(ChessBoard board, GameState gameState)
        {
            // Try OpenAI first if available
            if (_openAIService != null && _openAIService.IsAvailable())
            {
                try
                {
                    var openAIMove = GetOpenAIMoveAsync(board, gameState).Result;
                    if (openAIMove != null)
                    {
                        return openAIMove;
                    }
                }
                catch (Exception)
                {
                    // Fall back to traditional AI if OpenAI fails
                }
            }
            
            // Fallback to maximum depth traditional AI
            return _engine.FindBestMove(board, gameState, 6);
        }

        private async Task<ChessMove> GetOpenAIMoveAsync(ChessBoard board, GameState gameState)
        {
            try
            {
                // Convert board to text representation
                string boardState = BoardConverter.ConvertToTextRepresentation(board);
                
                // Convert move history to algebraic notation
                string gameHistory = BoardConverter.ConvertMoveHistoryToAlgebraic(gameState.MoveHistory);
                
                // Determine player color
                string playerColor = Color == PieceColor.White ? "White" : "Black";
                
                // Get move from OpenAI
                string algebraicMove = await _openAIService.GetBestMoveAsync(boardState, gameHistory, playerColor);
                
                // Parse the algebraic move back to ChessMove
                ChessMove move = BoardConverter.ParseAlgebraicMove(algebraicMove, board, Color);
                
                // Validate the move is legal
                if (move != null && IsValidMove(move, gameState))
                {
                    return move;
                }
                
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private bool IsValidMove(ChessMove move, GameState gameState)
        {
            // Get the piece at the from position
            var piece = gameState.Board.GetPieceAt(move.FromRow, move.FromCol);
            if (piece == null || piece.Color != Color)
                return false;
            
            // Check if this move is in the piece's valid moves
            var validMoves = gameState.GetValidMovesFor(move.FromRow, move.FromCol);
            return validMoves.Any(m => m.FromRow == move.FromRow && m.FromCol == move.FromCol && 
                                      m.ToRow == move.ToRow && m.ToCol == move.ToCol);
        }

        private ChessMove GetReactiveMove(ChessBoard board, GameState gameState)
        {
            // Check if we have GitHub token for advanced features
            if (!_githubTokenManager.HasValidToken())
            {
                // Fallback to Hard difficulty
                return GetHardMove(board, gameState);
            }
            
            // Reactive AI - focuses on responding to opponent threats
            int searchDepth = 3;
            
            // Look for immediate threats first
            var threatenedPieces = GetThreatenedPieces(gameState);
            if (threatenedPieces.Count > 0)
            {
                // Prioritize defending high-value pieces
                var defendMoves = FindDefensiveMoves(gameState, threatenedPieces);
                if (defendMoves.Count > 0)
                {
                    return defendMoves.OrderByDescending(m => EvaluateDefensiveMove(m, gameState)).First();
                }
            }
            
            // Look for tactical opportunities
            var tacticalMoves = FindTacticalMoves(gameState);
            if (tacticalMoves.Count > 0)
            {
                return tacticalMoves.OrderByDescending(m => EvaluateTacticalMove(m, gameState)).First();
            }
            
            return _engine.FindBestMove(board, gameState, searchDepth);
        }

        private ChessMove GetAverageMove(ChessBoard board, GameState gameState)
        {
            // Check if we have GitHub token for advanced features
            if (!_githubTokenManager.HasValidToken())
            {
                // Fallback to Hard difficulty
                return GetHardMove(board, gameState);
            }
            
            // Average AI - balanced play with enhanced evaluation
            int searchDepth = 5;
            return _engine.FindBestMove(board, gameState, searchDepth);
        }

        private List<ChessMove> FindDefensiveMoves(GameState gameState, List<ChessPiece> threatenedPieces)
        {
            var defensiveMoves = new List<ChessMove>();
            
            foreach (var piece in threatenedPieces)
            {
                // Try to move the threatened piece to safety
                var moves = gameState.GetValidMovesFor(piece.Row, piece.Col);
                foreach (var move in moves)
                {
                    if (!IsSquareUnderAttack(gameState, move.ToRow, move.ToCol))
                    {
                        defensiveMoves.Add(move);
                    }
                }
                
                // Try to defend the piece with another piece
                var defenders = FindDefendingMoves(gameState, piece.Row, piece.Col);
                defensiveMoves.AddRange(defenders);
            }
            
            return defensiveMoves;
        }

        private List<ChessMove> FindDefendingMoves(GameState gameState, int targetRow, int targetCol)
        {
            var defendingMoves = new List<ChessMove>();
            var myPieces = gameState.Board.GetPieces(Color);
            
            foreach (var piece in myPieces)
            {
                var moves = piece.GetPossibleMoves(gameState.Board);
                foreach (var move in moves)
                {
                    // Check if this move would defend the target square
                    if (WouldDefendSquare(gameState, move, targetRow, targetCol))
                    {
                        defendingMoves.Add(move);
                    }
                }
            }
            
            return defendingMoves;
        }

        private bool WouldDefendSquare(GameState gameState, ChessMove move, int targetRow, int targetCol)
        {
            // This is a simplified check - a full implementation would simulate the move
            // and check if the target square would be defended
            var piece = gameState.Board.GetPieceAt(move.FromRow, move.FromCol);
            if (piece == null) return false;
            
            // Simple heuristic: if the piece can attack the target square after the move
            var afterMoves = piece.GetPossibleMovesFrom(gameState.Board, move.ToRow, move.ToCol);
            return afterMoves.Any(m => m.ToRow == targetRow && m.ToCol == targetCol);
        }

        private List<ChessMove> FindTacticalMoves(GameState gameState)
        {
            var tacticalMoves = new List<ChessMove>();
            var myPieces = gameState.Board.GetPieces(Color);
            
            foreach (var piece in myPieces)
            {
                var moves = gameState.GetValidMovesFor(piece.Row, piece.Col);
                foreach (var move in moves)
                {
                    // Look for captures
                    var targetPiece = gameState.Board.GetPieceAt(move.ToRow, move.ToCol);
                    if (targetPiece != null && targetPiece.Color != Color)
                    {
                        tacticalMoves.Add(move);
                    }
                    
                    // Look for checks
                    if (WouldGiveCheck(gameState, move))
                    {
                        tacticalMoves.Add(move);
                    }
                }
            }
            
            return tacticalMoves;
        }

        private bool WouldGiveCheck(GameState gameState, ChessMove move)
        {
            // Simulate the move and check if opponent king is in check
            var tempBoard = gameState.Board.Clone();
            tempBoard.MovePiece(move.FromRow, move.FromCol, move.ToRow, move.ToCol);
            
            var opponentColor = Color == PieceColor.White ? PieceColor.Black : PieceColor.White;
            var opponentKing = tempBoard.GetKing(opponentColor);
            
            return opponentKing != null && IsKingInCheck(opponentKing, tempBoard);
        }

        private int EvaluateDefensiveMove(ChessMove move, GameState gameState)
        {
            // Simple evaluation for defensive moves
            var piece = gameState.Board.GetPieceAt(move.FromRow, move.FromCol);
            if (piece == null) return 0;
            
            int value = GetPieceValue(piece);
            
            // Bonus for moving to a safe square
            if (!IsSquareUnderAttack(gameState, move.ToRow, move.ToCol))
            {
                value += 50;
            }
            
            return value;
        }

        private int EvaluateTacticalMove(ChessMove move, GameState gameState)
        {
            int value = 0;
            
            // Value for captures
            var targetPiece = gameState.Board.GetPieceAt(move.ToRow, move.ToCol);
            if (targetPiece != null && targetPiece.Color != Color)
            {
                value += GetPieceValue(targetPiece);
            }
            
            // Bonus for checks
            if (WouldGiveCheck(gameState, move))
            {
                value += 30;
            }
            
            return value;
        }

        private int GetPieceValue(ChessPiece piece)
        {
            if (piece is Pawn) return 100;
            if (piece is Knight || piece is Bishop) return 300;
            if (piece is Rook) return 500;
            if (piece is Queen) return 900;
            if (piece is King) return 10000;
            return 0;
        }

        // Keep all the original methods from AIPlayer...
        private ChessMove GetEasyMove(ChessBoard board, GameState gameState)
        {
            // Easy AI makes lots of mistakes - perfect for beginners drinking beer!
            double randomValue = _random.NextDouble();

            // 50% chance of making a completely random move
            if (randomValue < 0.5)
            {
                return MakeRandomMove(gameState);
            }
            // 25% chance of making a "bad" move (hangs pieces, ignores captures, etc.)
            else if (randomValue < 0.75)
            {
                var badMove = MakeBadMove(gameState);
                if (badMove != null) return badMove;
                return MakeRandomMove(gameState);
            }
            // 15% chance of making a mediocre move
            else if (randomValue < 0.9)
            {
                var mediocreMove = MakeMediocreMove(gameState);
                if (mediocreMove != null) return mediocreMove;
                return MakeRandomMove(gameState);
            }
            // Only 10% chance of making a decent move
            else
            {
                // Use engine but with very shallow depth and add randomness
                var engineMove = _engine.FindBestMove(board, gameState, 1);
                if (engineMove != null && _random.NextDouble() < 0.8)
                {
                    return engineMove;
                }
                return MakeRandomMove(gameState);
            }
        }

        private ChessMove GetMediumMove(ChessBoard board, GameState gameState)
        {
            int searchDepth = 3;
            float randomChance = 0.1f;  // 10% chance of random move

            // Sometimes play a random move based on difficulty level
            if (_random.NextDouble() < randomChance)
            {
                return MakeRandomMove(gameState);
            }

            // Otherwise use the chess engine to calculate the best move
            return _engine.FindBestMove(board, gameState, searchDepth);
        }

        private ChessMove GetHardMove(ChessBoard board, GameState gameState)
        {
            int searchDepth = 4;
            // Hard AI never makes random moves
            return _engine.FindBestMove(board, gameState, searchDepth);
        }

        private ChessMove MakeBadMove(GameState gameState)
        {
            List<ChessPiece> pieces = gameState.Board.GetPieces(Color);
            List<ChessMove> badMoves = new List<ChessMove>();

            // Look for moves that hang pieces (put them where they can be captured for free)
            foreach (var piece in pieces)
            {
                var moves = gameState.GetValidMovesFor(piece.Row, piece.Col);
                foreach (var move in moves)
                {
                    // Check if this move puts our piece where it can be captured
                    if (IsSquareUnderAttack(gameState, move.ToRow, move.ToCol))
                    {
                        // Don't sacrifice the king or make moves that result in immediate checkmate
                        if (!(piece is King) && !WouldResultInImmediateCheckmate(gameState, move))
                        {
                            // Higher chance to hang less valuable pieces
                            int weight = piece is Pawn ? 3 : (piece is Queen ? 1 : 2);
                            for (int i = 0; i < weight; i++)
                            {
                                badMoves.Add(move);
                            }
                        }
                    }
                }
            }

            // Look for moves that ignore opponent's threats
            var threatenedPieces = GetThreatenedPieces(gameState);
            foreach (var piece in pieces)
            {
                if (!threatenedPieces.Contains(piece)) // If this piece is not threatened
                {
                    var moves = gameState.GetValidMovesFor(piece.Row, piece.Col);
                    // Move pieces that are safe while ignoring pieces that are in danger
                    foreach (var move in moves)
                    {
                        if (!IsSquareUnderAttack(gameState, move.ToRow, move.ToCol))
                        {
                            badMoves.Add(move);
                        }
                    }
                }
            }

            if (badMoves.Count > 0)
            {
                return badMoves[_random.Next(badMoves.Count)];
            }

            return null;
        }

        private ChessMove MakeMediocreMove(GameState gameState)
        {
            List<ChessPiece> pieces = gameState.Board.GetPieces(Color);
            List<ChessMove> mediocreMove = new List<ChessMove>();

            // Look for captures, but sometimes miss them or make bad trades
            foreach (var piece in pieces)
            {
                var moves = gameState.GetValidMovesFor(piece.Row, piece.Col);
                foreach (var move in moves)
                {
                    var targetPiece = gameState.Board.GetPieceAt(move.ToRow, move.ToCol);
                    if (targetPiece != null && targetPiece.Color != Color)
                    {
                        // Found a capture, but sometimes make bad trades
                        // Example: capture pawn with queen when queen can be recaptured
                        if (IsSquareUnderAttack(gameState, move.ToRow, move.ToCol))
                        {
                            // This is a bad trade, but mediocre AI sometimes makes it
                            if (_random.NextDouble() < 0.4) // 40% chance to make bad trade
                            {
                                mediocreMove.Add(move);
                            }
                        }
                        else
                        {
                            // Good capture, add multiple times to increase chance
                            for (int i = 0; i < 3; i++)
                            {
                                mediocreMove.Add(move);
                            }
                        }
                    }
                    else
                    {
                        // Non-capture moves that don't hang pieces
                        if (!IsSquareUnderAttack(gameState, move.ToRow, move.ToCol))
                        {
                            mediocreMove.Add(move);
                        }
                    }
                }
            }

            if (mediocreMove.Count > 0)
            {
                return mediocreMove[_random.Next(mediocreMove.Count)];
            }

            return null;
        }

        private List<ChessPiece> GetThreatenedPieces(GameState gameState)
        {
            List<ChessPiece> threatenedPieces = new List<ChessPiece>();
            List<ChessPiece> myPieces = gameState.Board.GetPieces(Color);
            
            foreach (var piece in myPieces)
            {
                if (IsSquareUnderAttack(gameState, piece.Row, piece.Col))
                {
                    threatenedPieces.Add(piece);
                }
            }
            
            return threatenedPieces;
        }

        private bool IsSquareUnderAttack(GameState gameState, int row, int col)
        {
            PieceColor opponentColor = Color == PieceColor.White ? PieceColor.Black : PieceColor.White;
            List<ChessPiece> opponentPieces = gameState.Board.GetPieces(opponentColor);

            foreach (var piece in opponentPieces)
            {
                var moves = piece.GetPossibleMoves(gameState.Board);
                if (moves.Any(m => m.ToRow == row && m.ToCol == col))
                {
                    return true;
                }
            }

            return false;
        }

        private bool WouldResultInImmediateCheckmate(GameState gameState, ChessMove move)
        {
            // Create a temporary game state to test the move
            var tempBoard = gameState.Board.Clone();
            tempBoard.MovePiece(move.FromRow, move.FromCol, move.ToRow, move.ToCol);
            
            // Check if our king would be in checkmate after this move
            var king = tempBoard.GetKing(Color);
            if (king == null) return true; // If king is captured, that's bad

            // Simple check - if king is in check and has no legal moves, it's checkmate
            return IsKingInCheck(king, tempBoard);
        }

        private bool IsKingInCheck(King king, ChessBoard board)
        {
            if (king == null) return false;
            
            PieceColor opponentColor = king.Color == PieceColor.White ? PieceColor.Black : PieceColor.White;
            List<ChessPiece> opponentPieces = board.GetPieces(opponentColor);
            
            foreach (var piece in opponentPieces)
            {
                var moves = piece.GetPossibleMoves(board);
                if (moves.Any(m => m.ToRow == king.Row && m.ToCol == king.Col))
                {
                    return true;
                }
            }
            
            return false;
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

            // Favor moving pawns and minor pieces over major pieces for more "beginner-like" play
            List<ChessMove> beginnerMoves = new List<ChessMove>();
            foreach (var move in allMoves)
            {
                var piece = gameState.Board.GetPieceAt(move.FromRow, move.FromCol);
                if (piece is Pawn)
                {
                    // Add pawn moves 3 times to make them more likely
                    for (int i = 0; i < 3; i++) beginnerMoves.Add(move);
                }
                else if (piece is Knight || piece is Bishop)
                {
                    // Add minor piece moves 2 times
                    for (int i = 0; i < 2; i++) beginnerMoves.Add(move);
                }
                else
                {
                    // Add major piece moves once
                    beginnerMoves.Add(move);
                }
            }

            // Return a weighted random move, or fallback to any move if no weighted moves
            if (beginnerMoves.Count > 0)
            {
                return beginnerMoves[_random.Next(beginnerMoves.Count)];
            }
            else if (allMoves.Count > 0)
            {
                return allMoves[_random.Next(allMoves.Count)];
            }

            return null;
        }
    }
}