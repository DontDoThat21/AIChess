using System;
using System.Collections.Generic;

namespace TrubChess.Models.Pieces
{
    public class King : ChessPiece
    {
        public King(PieceColor color, int row, int col) 
            : base(color, row, col)
        {
        }

        public override List<ChessMove> GetPossibleMoves(ChessBoard board)
        {
            List<ChessMove> moves = new List<ChessMove>();
            
            // All eight directions
            int[] rowDelta = { -1, -1, -1, 0, 0, 1, 1, 1 };
            int[] colDelta = { -1, 0, 1, -1, 1, -1, 0, 1 };
            
            // Normal king moves
            for (int i = 0; i < 8; i++)
            {
                int newRow = Row + rowDelta[i];
                int newCol = Col + colDelta[i];
                
                if (CanMoveTo(newRow, newCol, board))
                {
                    moves.Add(new ChessMove(Row, Col, newRow, newCol));
                }
            }
            
            // Castling
            if (!HasMoved)
            {
                // Kingside castling
                Rook kingsideRook = board.GetPieceAt(Row, 7) as Rook;
                if (kingsideRook != null && !kingsideRook.HasMoved && 
                    board.GetPieceAt(Row, Col + 1) == null && 
                    board.GetPieceAt(Row, Col + 2) == null)
                {
                    moves.Add(new ChessMove(Row, Col, Row, Col + 2));
                }
                
                // Queenside castling
                Rook queensideRook = board.GetPieceAt(Row, 0) as Rook;
                if (queensideRook != null && !queensideRook.HasMoved && 
                    board.GetPieceAt(Row, Col - 1) == null && 
                    board.GetPieceAt(Row, Col - 2) == null && 
                    board.GetPieceAt(Row, Col - 3) == null)
                {
                    moves.Add(new ChessMove(Row, Col, Row, Col - 2));
                }
            }
            
            return moves;
        }

        public override ChessPiece Clone()
        {
            return new King(Color, Row, Col) { HasMoved = HasMoved };
        }

        public override int GetValue()
        {
            return 0; // King's value is effectively infinite
        }

        public override string GetNotationSymbol()
        {
            return "K";
        }
    }
}
