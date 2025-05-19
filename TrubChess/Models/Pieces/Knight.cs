using System;
using System.Collections.Generic;

namespace TrubChess.Models.Pieces
{
    public class Knight : ChessPiece
    {
        public bool IsLeftKnight { get; }

        public Knight(PieceColor color, int row, int col, bool isLeftKnight) 
            : base(color, row, col)
        {
            IsLeftKnight = isLeftKnight;
        }

        public override List<ChessMove> GetPossibleMoves(ChessBoard board)
        {
            List<ChessMove> moves = new List<ChessMove>();
            
            // Knight's move patterns
            int[] rowDelta = { -2, -1, 1, 2, 2, 1, -1, -2 };
            int[] colDelta = { 1, 2, 2, 1, -1, -2, -2, -1 };
            
            for (int i = 0; i < 8; i++)
            {
                int newRow = Row + rowDelta[i];
                int newCol = Col + colDelta[i];
                
                if (CanMoveTo(newRow, newCol, board))
                {
                    moves.Add(new ChessMove(Row, Col, newRow, newCol));
                }
            }
            
            return moves;
        }

        public override ChessPiece Clone()
        {
            return new Knight(Color, Row, Col, IsLeftKnight) { HasMoved = HasMoved };
        }

        public override int GetValue()
        {
            return 3;
        }

        public override string GetNotationSymbol()
        {
            return "N";
        }
    }
}
