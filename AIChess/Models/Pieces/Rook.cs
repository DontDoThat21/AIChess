using System;
using System.Collections.Generic;

namespace TrubChess.Models.Pieces
{
    public class Rook : ChessPiece
    {
        public Rook(PieceColor color, int row, int col) 
            : base(color, row, col)
        {
        }

        public override List<ChessMove> GetPossibleMoves(ChessBoard board)
        {
            List<ChessMove> moves = new List<ChessMove>();
            
            // Directions: up, right, down, left
            int[] rowDelta = { -1, 0, 1, 0 };
            int[] colDelta = { 0, 1, 0, -1 };
            
            for (int direction = 0; direction < 4; direction++)
            {
                int newRow = Row;
                int newCol = Col;
                
                while (true)
                {
                    newRow += rowDelta[direction];
                    newCol += colDelta[direction];
                    
                    if (newRow < 0 || newRow > 7 || newCol < 0 || newCol > 7)
                        break;
                    
                    ChessPiece targetPiece = board.GetPieceAt(newRow, newCol);
                    if (targetPiece == null)
                    {
                        moves.Add(new ChessMove(Row, Col, newRow, newCol));
                    }
                    else
                    {
                        if (targetPiece.Color != Color)
                        {
                            moves.Add(new ChessMove(Row, Col, newRow, newCol));
                        }
                        break;  // Stop at any piece
                    }
                }
            }
            
            return moves;
        }

        public override ChessPiece Clone()
        {
            return new Rook(Color, Row, Col) { HasMoved = HasMoved };
        }

        public override int GetValue()
        {
            return 5;
        }

        public override string GetNotationSymbol()
        {
            return "R";
        }
    }
}
