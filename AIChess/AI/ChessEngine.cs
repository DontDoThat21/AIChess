using System;
using System.Collections.Generic;
using System.Linq;
using TrubChess.Models;
using TrubChess.Models.Pieces;

namespace TrubChess.AI
{
    public class ChessEngine
    {
        private readonly MoveEvaluator _evaluator;
        private readonly Random _random;

        public ChessEngine()
        {
            _evaluator = new MoveEvaluator();
            _random = new Random();
        }

        public ChessMove FindBestMove(ChessBoard board, GameState gameState, int depth)
        {
            List<ChessMove> legalMoves = GetAllLegalMoves(board, gameState);
            
            // If no legal moves, return null
            if (legalMoves.Count == 0)
                return null;
                
            // For very easy difficulty, just return a random legal move
            if (depth <= 1)
                return legalMoves[_random.Next(legalMoves.Count)];
                
            ChessMove bestMove = null;
            int bestScore = int.MinValue;
            
            foreach (ChessMove move in legalMoves)
            {
                // Make a copy of the board and apply the move
                ChessBoard newBoard = board.Clone();
                newBoard.MovePiece(move.FromRow, move.FromCol, move.ToRow, move.ToCol);
                
                // Evaluate the resulting position using minimax
                int score = -Minimax(newBoard, depth - 1, int.MinValue, int.MaxValue, false);
                
                // Keep track of the best score
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
            }
            
            return bestMove;
        }
        
        private int Minimax(ChessBoard board, int depth, int alpha, int beta, bool isMaximizingPlayer)
        {
            // Base case: reached max depth or terminal node
            if (depth == 0)
                return _evaluator.EvaluatePosition(board);
                
            PieceColor currentColor = isMaximizingPlayer ? PieceColor.White : PieceColor.Black;
            List<ChessPiece> pieces = board.GetPieces(currentColor);
            List<ChessMove> moves = new List<ChessMove>();
            
            // Get all possible moves for the current player
            foreach (ChessPiece piece in pieces)
            {
                moves.AddRange(piece.GetPossibleMoves(board));
            }
            
            // Filter out illegal moves
            moves = FilterIllegalMoves(board, moves, currentColor);
            
            if (moves.Count == 0)
            {
                // If checkmate or stalemate, evaluate accordingly
                King king = board.GetKing(currentColor);
                bool inCheck = IsKingInCheck(king, board);
                
                if (inCheck)
                    return isMaximizingPlayer ? -10000 : 10000;  // Checkmate
                else
                    return 0;  // Stalemate
            }
            
            if (isMaximizingPlayer)
            {
                int maxEval = int.MinValue;
                foreach (ChessMove move in moves)
                {
                    ChessBoard newBoard = board.Clone();
                    newBoard.MovePiece(move.FromRow, move.FromCol, move.ToRow, move.ToCol);
                    
                    int eval = Minimax(newBoard, depth - 1, alpha, beta, false);
                    maxEval = Math.Max(maxEval, eval);
                    alpha = Math.Max(alpha, eval);
                    if (beta <= alpha)
                        break;  // Beta cutoff
                }
                return maxEval;
            }
            else
            {
                int minEval = int.MaxValue;
                foreach (ChessMove move in moves)
                {
                    ChessBoard newBoard = board.Clone();
                    newBoard.MovePiece(move.FromRow, move.FromCol, move.ToRow, move.ToCol);
                    
                    int eval = Minimax(newBoard, depth - 1, alpha, beta, true);
                    minEval = Math.Min(minEval, eval);
                    beta = Math.Min(beta, eval);
                    if (beta <= alpha)
                        break;  // Alpha cutoff
                }
                return minEval;
            }
        }
        
        private List<ChessMove> GetAllLegalMoves(ChessBoard board, GameState gameState)
        {
            List<ChessPiece> pieces = board.GetPieces(gameState.CurrentPlayer);
            List<ChessMove> allMoves = new List<ChessMove>();
            
            foreach (ChessPiece piece in pieces)
            {
                allMoves.AddRange(gameState.GetValidMovesFor(piece.Row, piece.Col));
            }
            
            return allMoves;
        }
        
        private List<ChessMove> FilterIllegalMoves(ChessBoard board, List<ChessMove> moves, PieceColor color)
        {
            List<ChessMove> legalMoves = new List<ChessMove>();
            
            foreach (ChessMove move in moves)
            {
                ChessBoard tempBoard = board.Clone();
                tempBoard.MovePiece(move.FromRow, move.FromCol, move.ToRow, move.ToCol);
                
                King king = tempBoard.GetKing(color);
                if (!IsKingInCheck(king, tempBoard))
                {
                    legalMoves.Add(move);
                }
            }
            
            return legalMoves;
        }
        
        private bool IsKingInCheck(King king, ChessBoard board)
        {
            if (king == null)
                return false;
                
            PieceColor oppositeColor = king.Color == PieceColor.White ? PieceColor.Black : PieceColor.White;
            List<ChessPiece> opponentPieces = board.GetPieces(oppositeColor);
            
            foreach (ChessPiece piece in opponentPieces)
            {
                List<ChessMove> moves = piece.GetPossibleMoves(board);
                if (moves.Any(m => m.ToRow == king.Row && m.ToCol == king.Col))
                {
                    return true;
                }
            }
            
            return false;
        }
    }
}
