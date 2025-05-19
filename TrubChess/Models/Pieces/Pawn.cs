using System;
using System.Collections.Generic;

namespace TrubChess.Models.Pieces
{
    public class Pawn : ChessPiece
    {
        public Pawn(PieceColor color, int row, int col) 
            : base(color, row, col)
        {
        }

        public override List<ChessMove> GetPossibleMoves(ChessBoard board)
        {
            List<ChessMove> moves = new List<ChessMove>();
            
            // Determine direction (up for white, down for black)
            int direction = Color == PieceColor.White ? -1 : 1;
            
            // Forward move
            int newRow = Row + direction;
            if (newRow >= 0 && newRow <= 7 && board.GetPieceAt(newRow, Col) == null)
            {
                moves.Add(new ChessMove(Row, Col, newRow, Col));
                
                // Two square forward move from starting position
                if (!HasMoved)
                {
                    int twoSquaresForward = Row + 2 * direction;
                    if (twoSquaresForward >= 0 && twoSquaresForward <= 7 && 
                        board.GetPieceAt(twoSquaresForward, Col) == null)
                    {
                        moves.Add(new ChessMove(Row, Col, twoSquaresForward, Col));
                    }
                }
            }
            
            // Diagonal captures
            for (int deltaCol = -1; deltaCol <= 1; deltaCol += 2)
            {
                int newCol = Col + deltaCol;
                if (newCol >= 0 && newCol <= 7)
                {
                    // Regular capture
                    ChessPiece targetPiece = board.GetPieceAt(newRow, newCol);
                    if (targetPiece != null && targetPiece.Color != Color)
                    {
                        moves.Add(new ChessMove(Row, Col, newRow, newCol));
                    }
                    
                    // En passant capture
                    // Implement later if needed
                }
            }
            
            return moves;
        }

        public override ChessPiece Clone()
        {
            return new Pawn(Color, Row, Col) { HasMoved = HasMoved };
        }

        public override int GetValue()
        {
            return 1;
        }

        public override string GetNotationSymbol()
        {
            return "";  // Pawns don't have a notation symbol
        }
    }
}
