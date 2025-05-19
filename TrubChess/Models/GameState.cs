using System;
using System.Collections.Generic;
using System.Linq;
using TrubChess.Models.Pieces;

namespace TrubChess.Models
{
    public class ChessMove
    {
        public int FromRow { get; }
        public int FromCol { get; }
        public int ToRow { get; }
        public int ToCol { get; }
        public string Notation { get; set; }

        public ChessMove(int fromRow, int fromCol, int toRow, int toCol)
        {
            FromRow = fromRow;
            FromCol = fromCol;
            ToRow = toRow;
            ToCol = toCol;
        }
    }

    public class GameState
    {
        public ChessBoard Board { get; private set; }
        public PieceColor CurrentPlayer { get; private set; }
        public bool IsCheck { get; private set; }
        public bool IsCheckmate { get; private set; }
        public bool IsStalemate { get; private set; }
        public List<ChessMove> MoveHistory { get; private set; }
        
        // For en passant
        private ChessMove _lastMove;

        public GameState()
        {
            Board = new ChessBoard();
            CurrentPlayer = PieceColor.White;
            IsCheck = false;
            IsCheckmate = false;
            IsStalemate = false;
            MoveHistory = new List<ChessMove>();
        }

        public bool TryMakeMove(ChessMove move)
        {
            // Check if the game is already over
            if (IsCheckmate || IsStalemate)
                return false;

            // Get the piece to move
            ChessPiece piece = Board.GetPieceAt(move.FromRow, move.FromCol);
            if (piece == null || piece.Color != CurrentPlayer)
                return false;

            // Check if this move is in the list of valid moves
            List<ChessMove> validMoves = GetValidMovesFor(move.FromRow, move.FromCol);
            if (!validMoves.Any(m => m.ToRow == move.ToRow && m.ToCol == move.ToCol))
                return false;

            // Create a temporary board to test the move
            ChessBoard tempBoard = Board.Clone();
            tempBoard.MovePiece(move.FromRow, move.FromCol, move.ToRow, move.ToCol);

            // Check if the move would put the current player's king in check
            King king = tempBoard.GetKing(CurrentPlayer);
            if (IsKingInCheck(king, tempBoard))
                return false;

            // If all checks pass, make the move on the real board
            Board.MovePiece(move.FromRow, move.FromCol, move.ToRow, move.ToCol);
            _lastMove = move;

            // Generate algebraic notation for the move
            move.Notation = GenerateAlgebraicNotation(piece, move);
            MoveHistory.Add(move);

            // Switch turn
            CurrentPlayer = CurrentPlayer == PieceColor.White ? PieceColor.Black : PieceColor.White;

            // Check game state for the new current player
            UpdateGameState();

            return true;
        }

        public List<ChessMove> GetValidMovesFor(int row, int col)
        {
            ChessPiece piece = Board.GetPieceAt(row, col);
            if (piece == null || piece.Color != CurrentPlayer)
                return new List<ChessMove>();

            // Get all possible moves for the piece
            List<ChessMove> possibleMoves = piece.GetPossibleMoves(Board);
            List<ChessMove> validMoves = new List<ChessMove>();

            // Check which moves don't put/leave the king in check
            foreach (ChessMove move in possibleMoves)
            {
                ChessBoard tempBoard = Board.Clone();
                tempBoard.MovePiece(move.FromRow, move.FromCol, move.ToRow, move.ToCol);
                King king = tempBoard.GetKing(CurrentPlayer);
                
                if (!IsKingInCheck(king, tempBoard))
                {
                    validMoves.Add(move);
                }
            }

            return validMoves;
        }

        private void UpdateGameState()
        {
            // Check if the current player is in check
            King king = Board.GetKing(CurrentPlayer);
            IsCheck = IsKingInCheck(king, Board);

            // Check for checkmate or stalemate
            bool hasValidMove = false;
            List<ChessPiece> pieces = Board.GetPieces(CurrentPlayer);

            foreach (ChessPiece piece in pieces)
            {
                if (GetValidMovesFor(piece.Row, piece.Col).Count > 0)
                {
                    hasValidMove = true;
                    break;
                }
            }

            if (!hasValidMove)
            {
                if (IsCheck)
                {
                    IsCheckmate = true;
                }
                else
                {
                    IsStalemate = true;
                }
            }
        }

        private bool IsKingInCheck(King king, ChessBoard board)
        {
            if (king == null)
                return false;

            // Get all pieces of the opposite color
            PieceColor oppositeColor = king.Color == PieceColor.White ? PieceColor.Black : PieceColor.White;
            List<ChessPiece> opponentPieces = board.GetPieces(oppositeColor);

            // Check if any of them can capture the king
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

        private string GenerateAlgebraicNotation(ChessPiece piece, ChessMove move)
        {
            string notation = "";
            string[] columns = { "a", "b", "c", "d", "e", "f", "g", "h" };
            string[] rows = { "8", "7", "6", "5", "4", "3", "2", "1" };

            // Add piece symbol (empty for pawn)
            if (!(piece is Pieces.Pawn))
            {
                notation += piece.GetNotationSymbol();
            }

            // Check if capture
            if (Board.GetPieceAt(move.ToRow, move.ToCol) != null)
            {
                // For pawns, prepend the origin file
                if (piece is Pieces.Pawn)
                {
                    notation += columns[move.FromCol];
                }
                notation += "x";
            }
            
            // Add destination square
            notation += columns[move.ToCol] + rows[move.ToRow];

            // Check for check/checkmate
            ChessBoard tempBoard = Board.Clone();
            tempBoard.MovePiece(move.FromRow, move.FromCol, move.ToRow, move.ToCol);
            
            PieceColor nextPlayer = CurrentPlayer == PieceColor.White ? PieceColor.Black : PieceColor.White;
            King opponentKing = tempBoard.GetKing(nextPlayer);
            bool inCheck = IsKingInCheck(opponentKing, tempBoard);
            
            if (inCheck)
            {
                // Check if it's checkmate
                bool hasEscape = false;
                List<ChessPiece> opponentPieces = tempBoard.GetPieces(nextPlayer);
                
                foreach (ChessPiece opponentPiece in opponentPieces)
                {
                    List<ChessMove> possibleMoves = opponentPiece.GetPossibleMoves(tempBoard);
                    
                    foreach (ChessMove possibleMove in possibleMoves)
                    {
                        ChessBoard testBoard = tempBoard.Clone();
                        testBoard.MovePiece(possibleMove.FromRow, possibleMove.FromCol, possibleMove.ToRow, possibleMove.ToCol);
                        
                        if (!IsKingInCheck(testBoard.GetKing(nextPlayer), testBoard))
                        {
                            hasEscape = true;
                            break;
                        }
                    }
                    
                    if (hasEscape)
                        break;
                }
                
                notation += hasEscape ? "+" : "#";
            }

            return notation;
        }

        public bool IsDrawByRepetition
        {
            get
            {
                // Implement threefold repetition detection
                // ...
                return false;
            }
        }

        public bool IsDrawByInsufficientMaterial
        {
            get
            {
                // Implement insufficient material detection
                // ...
                return false;
            }
        }

        public bool IsDrawByFiftyMoveRule
        {
            get
            {
                // Implement 50-move rule detection
                // ...
                return false;
            }
        }
    }
}
