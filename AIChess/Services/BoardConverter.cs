using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AIChess.Models;
using AIChess.Models.Pieces;

namespace AIChess.Services
{
    public static class BoardConverter
    {
        public static string ConvertToTextRepresentation(ChessBoard board)
        {
            var sb = new StringBuilder();
            sb.AppendLine("  a b c d e f g h");
            
            for (int row = 0; row < 8; row++)
            {
                sb.Append($"{8 - row} ");
                for (int col = 0; col < 8; col++)
                {
                    var piece = board.GetPieceAt(row, col);
                    if (piece == null)
                    {
                        sb.Append(". ");
                    }
                    else
                    {
                        sb.Append(GetPieceSymbol(piece) + " ");
                    }
                }
                sb.AppendLine($"{8 - row}");
            }
            
            sb.AppendLine("  a b c d e f g h");
            return sb.ToString();
        }

        public static string ConvertToFEN(ChessBoard board, GameState gameState)
        {
            var fen = new StringBuilder();
            
            // Piece placement
            for (int row = 0; row < 8; row++)
            {
                int emptyCount = 0;
                for (int col = 0; col < 8; col++)
                {
                    var piece = board.GetPieceAt(row, col);
                    if (piece == null)
                    {
                        emptyCount++;
                    }
                    else
                    {
                        if (emptyCount > 0)
                        {
                            fen.Append(emptyCount);
                            emptyCount = 0;
                        }
                        fen.Append(GetFENPieceSymbol(piece));
                    }
                }
                if (emptyCount > 0)
                {
                    fen.Append(emptyCount);
                }
                if (row < 7)
                {
                    fen.Append('/');
                }
            }
            
            fen.Append(' ');
            
            // Active color
            fen.Append(gameState.CurrentPlayer == PieceColor.White ? 'w' : 'b');
            
            fen.Append(' ');
            
            // Castling availability (simplified - would need gameState enhancements)
            fen.Append("KQkq"); // Assume all castling rights for now
            
            fen.Append(' ');
            
            // En passant target square (simplified)
            fen.Append('-');
            
            fen.Append(' ');
            
            // Halfmove clock (simplified)
            fen.Append('0');
            
            fen.Append(' ');
            
            // Fullmove number (simplified)
            fen.Append('1');
            
            return fen.ToString();
        }

        public static string ConvertMoveHistoryToAlgebraic(List<ChessMove> moves)
        {
            if (moves == null || moves.Count == 0)
                return "Game start";
                
            var sb = new StringBuilder();
            for (int i = 0; i < moves.Count; i++)
            {
                if (i % 2 == 0)
                {
                    sb.Append($"{(i / 2) + 1}. ");
                }
                sb.Append(ConvertMoveToAlgebraic(moves[i]));
                if (i < moves.Count - 1)
                {
                    sb.Append(" ");
                }
            }
            return sb.ToString();
        }

        private static string ConvertMoveToAlgebraic(ChessMove move)
        {
            // This is a simplified conversion - proper algebraic notation would require
            // more context about piece types, captures, checks, etc.
            var from = $"{(char)('a' + move.FromCol)}{8 - move.FromRow}";
            var to = $"{(char)('a' + move.ToCol)}{8 - move.ToRow}";
            return $"{from}{to}";
        }

        public static ChessMove ParseAlgebraicMove(string algebraicMove, ChessBoard board, PieceColor playerColor)
        {
            // This is a simplified parser for basic moves returned by OpenAI
            // It handles moves like "e4", "Nf3", "Bxf7+", "O-O", etc.
            
            if (string.IsNullOrWhiteSpace(algebraicMove))
                return null;
                
            var move = algebraicMove.Trim().Replace("+", "").Replace("#", "").Replace("!", "").Replace("?", "");
            
            // Handle castling
            if (move == "O-O") // Kingside castling
            {
                int row = playerColor == PieceColor.White ? 7 : 0;
                return new ChessMove(row, 4, row, 6); // King moves from e to g
            }
            if (move == "O-O-O") // Queenside castling
            {
                int row = playerColor == PieceColor.White ? 7 : 0;
                return new ChessMove(row, 4, row, 2); // King moves from e to c
            }
            
            // For other moves, we'll need to implement a more sophisticated parser
            // For now, let's handle simple pawn moves and basic piece moves
            if (move.Length >= 2)
            {
                // Extract destination square
                char toFile = move[move.Length - 2];
                char toRank = move[move.Length - 1];
                
                if (toFile >= 'a' && toFile <= 'h' && toRank >= '1' && toRank <= '8')
                {
                    int toCol = toFile - 'a';
                    int toRow = 8 - (toRank - '0');
                    
                    // Find the piece that can make this move
                    return FindMoveThatMatchesDestination(board, playerColor, move, toRow, toCol);
                }
            }
            
            return null;
        }

        private static ChessMove FindMoveThatMatchesDestination(ChessBoard board, PieceColor playerColor, string algebraicMove, int toRow, int toCol)
        {
            var pieces = board.GetPieces(playerColor);
            
            foreach (var piece in pieces)
            {
                var possibleMoves = piece.GetPossibleMoves(board);
                foreach (var move in possibleMoves)
                {
                    if (move.ToRow == toRow && move.ToCol == toCol)
                    {
                        // Check if this move matches the algebraic notation pattern
                        if (DoesMoveMarchAlgebraic(piece, move, algebraicMove))
                        {
                            return move;
                        }
                    }
                }
            }
            
            return null;
        }

        private static bool DoesMoveMarchAlgebraic(ChessPiece piece, ChessMove move, string algebraicMove)
        {
            // Simple pattern matching - this would need to be more sophisticated for production
            if (algebraicMove.Length >= 2)
            {
                char pieceChar = algebraicMove[0];
                
                // If starts with a piece letter, check if it matches
                if (char.IsUpper(pieceChar))
                {
                    string expectedPiece = GetPieceSymbolForAlgebraic(piece);
                    return expectedPiece == pieceChar.ToString();
                }
                else
                {
                    // Pawn move - no piece letter
                    return piece is Pawn;
                }
            }
            
            return false;
        }

        private static string GetPieceSymbolForAlgebraic(ChessPiece piece)
        {
            if (piece is King) return "K";
            if (piece is Queen) return "Q";
            if (piece is Rook) return "R";
            if (piece is Bishop) return "B";
            if (piece is Knight) return "N";
            return ""; // Pawn has no symbol
        }

        private static string GetPieceSymbol(ChessPiece piece)
        {
            char symbol = '.';
            
            if (piece is King) symbol = 'K';
            else if (piece is Queen) symbol = 'Q';
            else if (piece is Rook) symbol = 'R';
            else if (piece is Bishop) symbol = 'B';
            else if (piece is Knight) symbol = 'N';
            else if (piece is Pawn) symbol = 'P';
            
            return piece.Color == PieceColor.White ? symbol.ToString() : symbol.ToString().ToLower();
        }

        private static char GetFENPieceSymbol(ChessPiece piece)
        {
            char symbol = '.';
            
            if (piece is King) symbol = 'K';
            else if (piece is Queen) symbol = 'Q';
            else if (piece is Rook) symbol = 'R';
            else if (piece is Bishop) symbol = 'B';
            else if (piece is Knight) symbol = 'N';
            else if (piece is Pawn) symbol = 'P';
            
            return piece.Color == PieceColor.White ? symbol : char.ToLower(symbol);
        }
    }
}