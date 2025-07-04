using System;
using System.Collections.Generic;

namespace TrubChess.Models
{
    public enum PieceColor
    {
        White,
        Black
    }

    public abstract class ChessPiece
    {
        public PieceColor Color { get; }
        public int Row { get; set; }
        public int Col { get; set; }
        public bool HasMoved { get; set; }

        protected ChessPiece(PieceColor color, int row, int col)
        {
            Color = color;
            Row = row;
            Col = col;
            HasMoved = false;
        }

        /// <summary>
        /// Gets all possible moves for this piece without considering check.
        /// </summary>
        /// <param name="board">The current chess board.</param>
        /// <returns>List of valid moves.</returns>
        public abstract List<ChessMove> GetPossibleMoves(ChessBoard board);

        /// <summary>
        /// Checks if a move is possible for this piece.
        /// </summary>
        /// <param name="toRow">Destination row.</param>
        /// <param name="toCol">Destination column.</param>
        /// <param name="board">The current chess board.</param>
        /// <returns>True if the move is possible.</returns>
        public bool CanMoveTo(int toRow, int toCol, ChessBoard board)
        {
            // Check if destination is within board bounds
            if (toRow < 0 || toRow > 7 || toCol < 0 || toCol > 7)
                return false;

            // Check if destination has a piece of the same color
            ChessPiece pieceAtDestination = board.GetPieceAt(toRow, toCol);
            if (pieceAtDestination != null && pieceAtDestination.Color == Color)
                return false;

            return true;
        }

        /// <summary>
        /// Creates a deep copy of this chess piece.
        /// </summary>
        /// <returns>A new chess piece with the same properties.</returns>
        public abstract ChessPiece Clone();

        /// <summary>
        /// Gets the relative value of the piece for AI evaluation.
        /// </summary>
        /// <returns>Piece value.</returns>
        public abstract int GetValue();

        /// <summary>
        /// Gets the notation symbol for this piece.
        /// </summary>
        /// <returns>The piece symbol (K for King, Q for Queen, etc.).</returns>
        public abstract string GetNotationSymbol();
    }
}
