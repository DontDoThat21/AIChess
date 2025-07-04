using System;
using TrubChess.Models;

namespace TrubChess.Players
{
    public abstract class Player
    {
        public PieceColor Color { get; }

        protected Player(PieceColor color)
        {
            Color = color;
        }

        /// <summary>
        /// Gets the next move for this player.
        /// </summary>
        /// <param name="board">The current chess board.</param>
        /// <param name="gameState">The current game state.</param>
        /// <returns>The player's next move.</returns>
        public abstract ChessMove GetNextMove(ChessBoard board, GameState gameState);
    }
}
