using System;
using TrubChess.Models;

namespace TrubChess.Players
{
    public class HumanPlayer : Player
    {
        public HumanPlayer(PieceColor color) : base(color)
        {
        }

        /// <summary>
        /// For a human player, the move is determined by UI interactions and not by this method.
        /// </summary>
        public override ChessMove GetNextMove(ChessBoard board, GameState gameState)
        {
            // This method isn't used for human players as they interact with the UI
            return null;
        }
    }
}
