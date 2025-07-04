using System;
using System.Collections.Generic;
using TrubChess.Models;
using TrubChess.Models.Pieces;

namespace TrubChess.AI
{
    public class MoveEvaluator
    {
        // Piece value constants
        private const int PawnValue = 100;
        private const int KnightValue = 320;
        private const int BishopValue = 330;
        private const int RookValue = 500;
        private const int QueenValue = 900;
        private const int KingValue = 20000;
        
        // Positional values - center control is valuable, especially in opening and middlegame
        private static readonly int[,] PawnPositionValues = {
            { 0,  0,  0,  0,  0,  0,  0,  0 },
            { 50, 50, 50, 50, 50, 50, 50, 50 },
            { 10, 10, 20, 30, 30, 20, 10, 10 },
            { 5,  5, 10, 25, 25, 10,  5,  5 },
            { 0,  0,  0, 20, 20,  0,  0,  0 },
            { 5, -5,-10,  0,  0,-10, -5,  5 },
            { 5, 10, 10,-20,-20, 10, 10,  5 },
            { 0,  0,  0,  0,  0,  0,  0,  0 }
        };
        
        private static readonly int[,] KnightPositionValues = {
            { -50,-40,-30,-30,-30,-30,-40,-50 },
            { -40,-20,  0,  0,  0,  0,-20,-40 },
            { -30,  0, 10, 15, 15, 10,  0,-30 },
            { -30,  5, 15, 20, 20, 15,  5,-30 },
            { -30,  0, 15, 20, 20, 15,  0,-30 },
            { -30,  5, 10, 15, 15, 10,  5,-30 },
            { -40,-20,  0,  5,  5,  0,-20,-40 },
            { -50,-40,-30,-30,-30,-30,-40,-50 }
        };
        
        private static readonly int[,] BishopPositionValues = {
            { -20,-10,-10,-10,-10,-10,-10,-20 },
            { -10,  0,  0,  0,  0,  0,  0,-10 },
            { -10,  0, 10, 10, 10, 10,  0,-10 },
            { -10,  5,  5, 10, 10,  5,  5,-10 },
            { -10,  0,  5, 10, 10,  5,  0,-10 },
            { -10,  5,  5,  5,  5,  5,  5,-10 },
            { -10,  0,  5,  0,  0,  5,  0,-10 },
            { -20,-10,-10,-10,-10,-10,-10,-20 }
        };
        
        private static readonly int[,] RookPositionValues = {
            {  0,  0,  0,  0,  0,  0,  0,  0 },
            {  5, 10, 10, 10, 10, 10, 10,  5 },
            { -5,  0,  0,  0,  0,  0,  0, -5 },
            { -5,  0,  0,  0,  0,  0,  0, -5 },
            { -5,  0,  0,  0,  0,  0,  0, -5 },
            { -5,  0,  0,  0,  0,  0,  0, -5 },
            { -5,  0,  0,  0,  0,  0,  0, -5 },
            {  0,  0,  0,  5,  5,  0,  0,  0 }
        };
        
        private static readonly int[,] QueenPositionValues = {
            { -20,-10,-10, -5, -5,-10,-10,-20 },
            { -10,  0,  0,  0,  0,  0,  0,-10 },
            { -10,  0,  5,  5,  5,  5,  0,-10 },
            {  -5,  0,  5,  5,  5,  5,  0, -5 },
            {   0,  0,  5,  5,  5,  5,  0, -5 },
            { -10,  5,  5,  5,  5,  5,  0,-10 },
            { -10,  0,  5,  0,  0,  0,  0,-10 },
            { -20,-10,-10, -5, -5,-10,-10,-20 }
        };
        
        private static readonly int[,] KingMiddlegameValues = {
            { -30,-40,-40,-50,-50,-40,-40,-30 },
            { -30,-40,-40,-50,-50,-40,-40,-30 },
            { -30,-40,-40,-50,-50,-40,-40,-30 },
            { -30,-40,-40,-50,-50,-40,-40,-30 },
            { -20,-30,-30,-40,-40,-30,-30,-20 },
            { -10,-20,-20,-20,-20,-20,-20,-10 },
            {  20, 20,  0,  0,  0,  0, 20, 20 },
            {  20, 30, 10,  0,  0, 10, 30, 20 }
        };
        
        public int EvaluatePosition(ChessBoard board)
        {
            int whiteScore = EvaluateForColor(board, PieceColor.White);
            int blackScore = EvaluateForColor(board, PieceColor.Black);
            
            // Return score relative to white (positive means white is better)
            return whiteScore - blackScore;
        }
        
        private int EvaluateForColor(ChessBoard board, PieceColor color)
        {
            int score = 0;
            List<ChessPiece> pieces = board.GetPieces(color);
            
            foreach (var piece in pieces)
            {
                // Add material value
                score += GetPieceValue(piece);
                
                // Add positional value
                score += GetPositionalValue(piece);
            }
            
            // Add mobility bonus (more legal moves = better position)
            score += CalculateMobility(board, color) * 5;
            
            return score;
        }
        
        private int GetPieceValue(ChessPiece piece)
        {
            if (piece is Pawn) return PawnValue;
            if (piece is Knight) return KnightValue;
            if (piece is Bishop) return BishopValue;
            if (piece is Rook) return RookValue;
            if (piece is Queen) return QueenValue;
            if (piece is King) return KingValue;
            return 0;
        }
        
        private int GetPositionalValue(ChessPiece piece)
        {
            int row = piece.Row;
            int col = piece.Col;
            
            // For black pieces, flip the board to use the same position tables
            if (piece.Color == PieceColor.Black)
            {
                row = 7 - row;
            }
            
            if (piece is Pawn) return PawnPositionValues[row, col];
            if (piece is Knight) return KnightPositionValues[row, col];
            if (piece is Bishop) return BishopPositionValues[row, col];
            if (piece is Rook) return RookPositionValues[row, col];
            if (piece is Queen) return QueenPositionValues[row, col];
            if (piece is King) return KingMiddlegameValues[row, col];
            return 0;
        }
        
        private int CalculateMobility(ChessBoard board, PieceColor color)
        {
            int mobility = 0;
            List<ChessPiece> pieces = board.GetPieces(color);
            
            foreach (var piece in pieces)
            {
                mobility += piece.GetPossibleMoves(board).Count;
            }
            
            return mobility;
        }
    }
}
