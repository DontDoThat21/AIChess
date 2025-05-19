using System.Collections.Generic;

namespace TrubChess.Models.Pieces
{
    public class Bishop : ChessPiece
    {
        public bool IsLeftBishop { get; }

        public Bishop(PieceColor color, int row, int col, bool isLeftBishop) 
            : base(color, row, col)
        {
            IsLeftBishop = isLeftBishop;
        }

        public override List<ChessMove> GetPossibleMoves(ChessBoard board)
        {
            List<ChessMove> moves = new List<ChessMove>();
            
            int[] rowDelta = { -1, 1, 1, -1 };
            int[] colDelta = { 1, 1, -1, -1 };
            
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
                        break;  
                    }
                }
            }
            
            return moves;
        }

        public override ChessPiece Clone()
        {
            return new Bishop(Color, Row, Col, IsLeftBishop) { HasMoved = HasMoved };
        }

        public override int GetValue()
        {
            return 3;
        }

        public override string GetNotationSymbol()
        {
            return "B";
        }
    }
}
