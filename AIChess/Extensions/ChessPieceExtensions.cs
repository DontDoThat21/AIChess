using System.Collections.Generic;
using AIChess.Models;
using AIChess.Models.Pieces;

namespace AIChess.Extensions
{
    public static class ChessPieceExtensions
    {
        /// <summary>
        /// Gets possible moves for a piece if it were at a different position.
        /// This is useful for defensive calculations.
        /// </summary>
        public static List<ChessMove> GetPossibleMovesFrom(this ChessPiece piece, ChessBoard board, int fromRow, int fromCol)
        {
            // Temporarily change the piece position
            int originalRow = piece.Row;
            int originalCol = piece.Col;
            
            piece.Row = fromRow;
            piece.Col = fromCol;
            
            // Get possible moves from new position
            var moves = piece.GetPossibleMoves(board);
            
            // Restore original position
            piece.Row = originalRow;
            piece.Col = originalCol;
            
            return moves;
        }
    }
}