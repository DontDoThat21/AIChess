using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Chat;

namespace AIChess.Services
{
    public class OpenAIChessService
    {
        private readonly OpenAIClient _openAIClient;
        private readonly string _apiKey;

        public OpenAIChessService()
        {
            _apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new InvalidOperationException("OPENAI_API_KEY environment variable is not set. Please set your OpenAI API key.");
            }
            
            _openAIClient = new OpenAIClient(_apiKey);
        }

        public async Task<string> GetBestMoveAsync(string boardState, string gameHistory, string playerColor)
        {
            try
            {
                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage(@"You are a world-class chess grandmaster AI. You will be given a chess position and must respond with only the best move in standard algebraic notation (e.g., 'e4', 'Nf3', 'Qh5+', 'O-O', 'O-O-O').

Rules:
1. Respond with ONLY the move in standard algebraic notation
2. Do not include explanations, analysis, or additional text
3. Ensure the move is legal for the given position
4. Play the strongest, most principled move
5. Consider tactics, strategy, endgame principles, and positional factors
6. If checkmate is possible, play it
7. If check is necessary for the best move, include '+' 
8. For castling, use 'O-O' (kingside) or 'O-O-O' (queenside)
9. For pawn promotion, include the piece (e.g., 'e8=Q')

Example responses: 'e4', 'Nf3', 'Bxf7+', 'O-O', 'Rxe8#'"),
                    
                    new UserChatMessage($@"Current chess position (FEN-like format):
{boardState}

Game history (moves played so far):
{gameHistory}

You are playing as: {playerColor}

What is the best move?")
                };

                var chatRequest = new ChatRequest(messages, "gpt-4");
                var response = await _openAIClient.ChatEndpoint.GetCompletionAsync(chatRequest);
                
                var moveText = response.FirstChoice.Message.Content.Trim();
                
                // Validate that the response looks like a chess move
                if (IsValidChessMoveFormat(moveText))
                {
                    return moveText;
                }
                else
                {
                    throw new InvalidOperationException($"OpenAI returned invalid move format: {moveText}");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error calling OpenAI API: {ex.Message}", ex);
            }
        }

        private bool IsValidChessMoveFormat(string move)
        {
            if (string.IsNullOrWhiteSpace(move))
                return false;

            // Remove common suffixes
            var cleanMove = move.Replace("+", "").Replace("#", "").Replace("!", "").Replace("?", "");
            
            // Check for castling
            if (cleanMove == "O-O" || cleanMove == "O-O-O")
                return true;
            
            // Check for basic move patterns (simplified validation)
            // This is a basic check - the actual game logic will validate if the move is legal
            return cleanMove.Length >= 2 && cleanMove.Length <= 7 && 
                   !cleanMove.Any(c => char.IsDigit(c) && c > '8') && // No invalid ranks
                   !cleanMove.Any(c => char.IsLetter(c) && char.ToLower(c) > 'h'); // No invalid files
        }

        public bool IsAvailable()
        {
            return !string.IsNullOrEmpty(_apiKey);
        }
    }
}