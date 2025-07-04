using System;
using System.Collections.Generic;
using TrubChess.Models.Pieces;

namespace TrubChess.Models
{
    public class ChessBoard
    {
        private ChessPiece[,] _board;

        public ChessBoard()
        {
            _board = new ChessPiece[8, 8];
            InitializeBoard();
        }

        private ChessBoard(ChessPiece[,] board)
        {
            _board = new ChessPiece[8, 8];
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    _board[row, col] = board[row, col]?.Clone();
                }
            }
        }

        private void InitializeBoard()
        {
            // Place pawns
            for (int col = 0; col < 8; col++)
            {
                _board[1, col] = new Pawn(PieceColor.Black, 1, col);
                _board[6, col] = new Pawn(PieceColor.White, 6, col);
            }

            // Place rooks
            _board[0, 0] = new Rook(PieceColor.Black, 0, 0);
            _board[0, 7] = new Rook(PieceColor.Black, 0, 7);
            _board[7, 0] = new Rook(PieceColor.White, 7, 0);
            _board[7, 7] = new Rook(PieceColor.White, 7, 7);

            // Place knights
            _board[0, 1] = new Knight(PieceColor.Black, 0, 1, true);  // Left knight
            _board[0, 6] = new Knight(PieceColor.Black, 0, 6, false); // Right knight
            _board[7, 1] = new Knight(PieceColor.White, 7, 1, true);  // Left knight
            _board[7, 6] = new Knight(PieceColor.White, 7, 6, false); // Right knight

            // Place bishops
            _board[0, 2] = new Bishop(PieceColor.Black, 0, 2, true);  // Left bishop
            _board[0, 5] = new Bishop(PieceColor.Black, 0, 5, false); // Right bishop
            _board[7, 2] = new Bishop(PieceColor.White, 7, 2, true);  // Left bishop
            _board[7, 5] = new Bishop(PieceColor.White, 7, 5, false); // Right bishop

            // Place queens
            _board[0, 3] = new Queen(PieceColor.Black, 0, 3);
            _board[7, 3] = new Queen(PieceColor.White, 7, 3);

            // Place kings
            _board[0, 4] = new King(PieceColor.Black, 0, 4);
            _board[7, 4] = new King(PieceColor.White, 7, 4);
        }

        public ChessPiece GetPieceAt(int row, int col)
        {
            if (row < 0 || row > 7 || col < 0 || col > 7)
                return null;

            return _board[row, col];
        }

        public void MovePiece(int fromRow, int fromCol, int toRow, int toCol)
        {
            ChessPiece piece = _board[fromRow, fromCol];
            if (piece == null)
                return;

            // Update piece position
            piece.Row = toRow;
            piece.Col = toCol;

            // Move the piece
            _board[toRow, toCol] = piece;
            _board[fromRow, fromCol] = null;

            // Handle pawn promotion
            if (piece is Pawn && ((toRow == 0 && piece.Color == PieceColor.White) ||
                                 (toRow == 7 && piece.Color == PieceColor.Black)))
            {
                // Default promotion to Queen
                _board[toRow, toCol] = new Queen(piece.Color, toRow, toCol);
            }

            // Handle castling - move the rook as well
            if (piece is King && Math.Abs(fromCol - toCol) == 2)
            {
                // Kingside castling
                if (toCol > fromCol)
                {
                    ChessPiece rook = _board[toRow, 7];
                    rook.Row = toRow;
                    rook.Col = toCol - 1;
                    _board[toRow, toCol - 1] = rook;
                    _board[toRow, 7] = null;
                }
                // Queenside castling
                else
                {
                    ChessPiece rook = _board[toRow, 0];
                    rook.Row = toRow;
                    rook.Col = toCol + 1;
                    _board[toRow, toCol + 1] = rook;
                    _board[toRow, 0] = null;
                }
            }
        }

        public void PromotePawn(int row, int col, ChessPiece newPiece)
        {
            if (_board[row, col] is Pawn)
            {
                _board[row, col] = newPiece;
            }
        }

        public void PromotePawn(int row, int col, string newPiece, PieceColor color)
        {
            // Replace pawn with new piece (Queen, Rook, Bishop, Knight)
            if (_board[row, col] is Pawn)
            {
                switch (newPiece)
                {
                    case "Queen":
                        _board[row, col] = new Queen(color, row, col);
                        break;
                    case "Rook":
                        _board[row, col] = new Rook(color, row, col);
                        break;
                    case "Bishop":
                        _board[row, col] = new Bishop(color, row, col, col == 2 || col == 5);
                        break;
                    case "Knight":
                        _board[row, col] = new Knight(color, row, col, col == 1 || col == 6);
                        break;
                    default:
                        _board[row, col] = new Queen(color, row, col);
                        break;
                }
            }
        }

        public King GetKing(PieceColor color)
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (_board[row, col] is King king && king.Color == color)
                    {
                        return king;
                    }
                }
            }
            return null;
        }

        public List<ChessPiece> GetPieces(PieceColor color)
        {
            List<ChessPiece> pieces = new List<ChessPiece>();
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (_board[row, col] != null && _board[row, col].Color == color)
                    {
                        pieces.Add(_board[row, col]);
                    }
                }
            }
            return pieces;
        }

        public ChessBoard Clone()
        {
            return new ChessBoard(_board);
        }

        public bool CanCastleKingSide(PieceColor color)
        {
            // TODO: Implement castling rights check
            return false;
        }

        public bool CanCastleQueenSide(PieceColor color)
        {
            // TODO: Implement castling rights check
            return false;
        }

        public bool IsEnPassantPossible(int fromRow, int fromCol, int toRow, int toCol)
        {
            // TODO: Implement en passant rule check
            return false;
        }
    }
}
