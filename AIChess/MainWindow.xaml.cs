using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AIChess.Models;
using AIChess.Players;
using AIChess.AI;
using AIChess.Models.Pieces;
using System.Windows.Media.Animation;
using System.Windows.Documents;

namespace AIChess
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ChessBoard _chessBoard;
        private GameState _gameState;
        private Player _whitePlayer;
        private Player _blackPlayer;
        private Border[,] _boardSquares;
        private Border _selectedSquare;
        private List<Border> _highlightedMoves;
        private bool _isGameActive;
        private AIPlayer.Difficulty _aiDifficulty;

        public MainWindow()
        {
            InitializeComponent();
            _boardSquares = new Border[8, 8];
            _highlightedMoves = new List<Border>();
            _aiDifficulty = AIPlayer.Difficulty.Easy;
            InitializeBoard();
            LoadAndApplyStartupColors();
            UpdateAIDifficultyMenuAvailability();
            SetupNewGame();
        }

        private void InitializeBoard()
        {
            // Create the checkerboard pattern
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Border square = new Border();
                    square.Background = (row + col) % 2 == 0 ? Brushes.Beige : Brushes.SaddleBrown;
                    square.BorderBrush = Brushes.Black;
                    square.BorderThickness = new Thickness(0.5);

                    // Store row and column as Tag for easy position lookup
                    square.Tag = new Tuple<int, int>(row, col);
                    square.MouseDown += Square_MouseDown;

                    Grid.SetRow(square, row + 1);  // +1 because the grid has headers in row/col 0
                    Grid.SetColumn(square, col + 1);

                    ChessBoard.Children.Add(square);
                    _boardSquares[row, col] = square;
                }
            }
        }

        private void SetupNewGame()
        {
            _chessBoard = new ChessBoard();
            _gameState = new GameState();
            _isGameActive = true;

            _whitePlayer = new HumanPlayer(PieceColor.White);

            if (PlayerVsPlayerMenuItem.IsChecked)
                _blackPlayer = new HumanPlayer(PieceColor.Black);
            else
                _blackPlayer = new AIPlayerExtended(PieceColor.Black, (AIPlayerExtended.Difficulty)_aiDifficulty);

            UpdateBoardDisplay();
            UpdateGameInfo();
            UpdateScoreAndCaptured();

            _selectedSquare = null;
            ClearHighlightedMoves();

            MoveHistoryText.Text = "";

            if (_gameState.CurrentPlayer == PieceColor.Black && _blackPlayer is AIPlayer aiPlayer)
            {
                MakeAIMove(aiPlayer);
            }
        }

        private void UpdateBoardDisplay()
        {
            // Clear all highlights first
            ClearHighlightedMoves();
            
            foreach (var square in _boardSquares)
            {
                // Properly clear any child elements
                ClearSquareContent(square);
            }

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    ChessPiece piece = _chessBoard.GetPieceAt(row, col);
                    if (piece != null)
                    {
                        Image pieceImage = new Image();
                        pieceImage.Source = GetPieceImage(piece);
                        pieceImage.Stretch = Stretch.Uniform;

                        _boardSquares[row, col].Child = pieceImage;
                    }
                }
            }
        }

        private void ClearSquareContent(Border square)
        {
            if (square.Child is Grid grid)
            {
                // Clear all children from the grid first
                grid.Children.Clear();
            }
            square.Child = null;
        }

        private ImageSource GetPieceImage(ChessPiece piece)
        {
            string color = piece.Color == PieceColor.White ? "white" : "black";
            string pieceName = piece.GetType().Name.ToLower();

            if (piece is Knight && ((Knight)piece).IsLeftKnight)
                return new BitmapImage(new Uri($"/AIChess;component/Resources/{color}knightl.png", UriKind.Relative));
            else if (piece is Knight)
                return new BitmapImage(new Uri($"/AIChess;component/Resources/{color}knightr.png", UriKind.Relative));
            else if (piece is Bishop && ((Bishop)piece).IsLeftBishop)
                return new BitmapImage(new Uri($"/AIChess;component/Resources/{color}bishopl.png", UriKind.Relative));
            else if (piece is Bishop)
                return new BitmapImage(new Uri($"/AIChess;component/Resources/{color}bishopr.png", UriKind.Relative));
            else
                return new BitmapImage(new Uri($"/AIChess;component/Resources/{color}{pieceName}.png", UriKind.Relative));
        }

        private void UpdateGameInfo()
        {
            CurrentPlayerText.Text = _gameState.CurrentPlayer == PieceColor.White ? "White" : "Black";

            if (_gameState.IsCheckmate)
            {
                GameStatusText.Text = "Checkmate";
                StatusText.Text = $"Game over. {(_gameState.CurrentPlayer == PieceColor.White ? "Black" : "White")} wins by checkmate!";
                _isGameActive = false;
            }
            else if (_gameState.IsStalemate)
            {
                GameStatusText.Text = "Stalemate";
                StatusText.Text = "Game over. Draw by stalemate.";
                _isGameActive = false;
            }
            else if (_gameState.IsCheck)
            {
                GameStatusText.Text = "Check";
                StatusText.Text = $"{(_gameState.CurrentPlayer == PieceColor.White ? "White" : "Black")} is in check!";
            }
            else
            {
                GameStatusText.Text = "In Progress";
                StatusText.Text = $"{(_gameState.CurrentPlayer == PieceColor.White ? "White" : "Black")} to move.";
            }

            UpdateScoreAndCaptured();
        }

        private void AnimatePieceMove(Border fromSquare, Border toSquare, Image pieceImage, Action onComplete)
        {
            var fromPoint = fromSquare.PointToScreen(new Point(0, 0));
            var toPoint = toSquare.PointToScreen(new Point(0, 0));
            var offset = new Point(toPoint.X - fromPoint.X, toPoint.Y - fromPoint.Y);

            var windowPoint = this.PointToScreen(new Point(0, 0));
            var adornerLayer = AdornerLayer.GetAdornerLayer(this.Content as Visual);
            if (adornerLayer == null)
            {
                onComplete?.Invoke();
                return;
            }

            var animImage = new Image
            {
                Source = pieceImage.Source,
                Width = fromSquare.ActualWidth,
                Height = fromSquare.ActualHeight
            };

            var overlay = new Canvas
            {
                IsHitTestVisible = false
            };
            overlay.Children.Add(animImage);
            Canvas.SetLeft(animImage, fromPoint.X - windowPoint.X);
            Canvas.SetTop(animImage, fromPoint.Y - windowPoint.Y);

            (this.Content as Grid).Children.Add(overlay);

            var animX = new DoubleAnimation(Canvas.GetLeft(animImage), Canvas.GetLeft(animImage) + offset.X, TimeSpan.FromMilliseconds(200));
            var animY = new DoubleAnimation(Canvas.GetTop(animImage), Canvas.GetTop(animImage) + offset.Y, TimeSpan.FromMilliseconds(200));
            animX.FillBehavior = FillBehavior.Stop;
            animY.FillBehavior = FillBehavior.Stop;

            animY.Completed += (s, e) =>
            {
                (this.Content as Grid).Children.Remove(overlay);
                onComplete?.Invoke();
            };

            animImage.BeginAnimation(Canvas.LeftProperty, animX);
            animImage.BeginAnimation(Canvas.TopProperty, animY);
        }

        private void Square_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isGameActive || !(_gameState.CurrentPlayer == PieceColor.White ?
                _whitePlayer is HumanPlayer : _blackPlayer is HumanPlayer))
                return;

            Border clickedSquare = sender as Border;
            if (clickedSquare == null) return;

            var position = clickedSquare.Tag as Tuple<int, int>;
            if (position == null) return;

            int row = position.Item1;
            int col = position.Item2;

            if (_selectedSquare == null)
            {
                ChessPiece piece = _chessBoard.GetPieceAt(row, col);
                if (piece != null && piece.Color == _gameState.CurrentPlayer)
                {
                    _selectedSquare = clickedSquare;
                    _selectedSquare.BorderBrush = Brushes.Yellow;
                    _selectedSquare.BorderThickness = new Thickness(3);

                    List<ChessMove> validMoves = _gameState.GetValidMovesFor(row, col);
                    foreach (var validMove in validMoves)
                    {
                        Border targetSquare = _boardSquares[validMove.ToRow, validMove.ToCol];
                        ChessPiece targetPiece = _chessBoard.GetPieceAt(validMove.ToRow, validMove.ToCol);
                        
                        // Don't highlight squares with own pieces - this shouldn't happen if GetValidMovesFor is working correctly
                        // but let's add this as a safety check
                        if (targetPiece != null && targetPiece.Color == _gameState.CurrentPlayer)
                            continue;
                        
                        Ellipse highlight = new Ellipse
                        {
                            Width = 20,
                            Height = 20,
                            Fill = targetPiece != null
                                ? new SolidColorBrush(Color.FromArgb(150, 255, 0, 0))  // Red for capture
                                : new SolidColorBrush(Color.FromArgb(150, 0, 255, 0))  // Green for empty square
                        };

                        AddHighlightToSquare(targetSquare, highlight);
                        _highlightedMoves.Add(targetSquare);
                    }
                }
            }
            else
            {
                var selectedPosition = _selectedSquare.Tag as Tuple<int, int>;
                int fromRow = selectedPosition.Item1;
                int fromCol = selectedPosition.Item2;

                if (fromRow == row && fromCol == col)
                {
                    _selectedSquare.BorderBrush = Brushes.Black;
                    _selectedSquare.BorderThickness = new Thickness(0.5);
                    _selectedSquare = null;
                    ClearHighlightedMoves();
                    return;
                }

                // Check if clicking on another piece of the same color - if so, select that piece instead
                ChessPiece clickedPiece = _chessBoard.GetPieceAt(row, col);
                if (clickedPiece != null && clickedPiece.Color == _gameState.CurrentPlayer)
                {
                    // Clear previous selection
                    _selectedSquare.BorderBrush = Brushes.Black;
                    _selectedSquare.BorderThickness = new Thickness(0.5);
                    ClearHighlightedMoves();
                    
                    // Select the new piece
                    _selectedSquare = clickedSquare;
                    _selectedSquare.BorderBrush = Brushes.Yellow;
                    _selectedSquare.BorderThickness = new Thickness(3);

                    List<ChessMove> validMoves = _gameState.GetValidMovesFor(row, col);
                    foreach (var validMove in validMoves)
                    {
                        Border targetSquare = _boardSquares[validMove.ToRow, validMove.ToCol];
                        ChessPiece targetPiece = _chessBoard.GetPieceAt(validMove.ToRow, validMove.ToCol);
                        
                        // Safety check to ensure we don't highlight own pieces
                        if (targetPiece != null && targetPiece.Color == _gameState.CurrentPlayer)
                            continue;
                        
                        Ellipse highlight = new Ellipse
                        {
                            Width = 20,
                            Height = 20,
                            Fill = targetPiece != null
                                ? new SolidColorBrush(Color.FromArgb(150, 255, 0, 0))  // Red for capture
                                : new SolidColorBrush(Color.FromArgb(150, 0, 255, 0))  // Green for empty square
                        };

                        AddHighlightToSquare(targetSquare, highlight);
                        _highlightedMoves.Add(targetSquare);
                    }
                    return;
                }

                ChessMove move = new ChessMove(fromRow, fromCol, row, col);
                if (_gameState.TryMakeMove(move))
                {
                    var fromSquare = _boardSquares[fromRow, fromCol];
                    var toSquare = _boardSquares[row, col];
                    Image pieceImage = GetSquarePieceImage(fromSquare);
                    
                    if (pieceImage != null)
                    {
                        AnimatePieceMove(fromSquare, toSquare, pieceImage, () =>
                        {
                            _chessBoard.MovePiece(fromRow, fromCol, row, col);
                            UpdateBoardDisplay();
                            UpdateGameInfo();
                            HandlePawnPromotionIfNeeded(row, col);

                            string playerColor = move.Notation.StartsWith("#") ? "Black" : "White";
                            int moveNumber = _gameState.MoveHistory.Count / 2 + (_gameState.MoveHistory.Count % 2);

                            if (_gameState.MoveHistory.Count % 2 == 1)
                            {
                                MoveHistoryText.Text += $"{moveNumber}. {move.Notation} ";
                            }
                            else
                            {
                                MoveHistoryText.Text += $"{move.Notation}\n";
                            }

                            if (_isGameActive && _gameState.CurrentPlayer == PieceColor.Black && _blackPlayer is AIPlayer aiPlayer)
                            {
                                MakeAIMove(aiPlayer);
                            }
                            else if (_isGameActive && _gameState.CurrentPlayer == PieceColor.White && _whitePlayer is AIPlayerExtended whiteAIPlayer)
                            {
                                MakeAIMove(whiteAIPlayer);
                            }
                        });
                    }
                    else
                    {
                        _chessBoard.MovePiece(fromRow, fromCol, row, col);
                        UpdateBoardDisplay();
                        UpdateGameInfo();
                        HandlePawnPromotionIfNeeded(row, col);

                        string playerColor = move.Notation.StartsWith("#") ? "Black" : "White";
                        int moveNumber = _gameState.MoveHistory.Count / 2 + (_gameState.MoveHistory.Count % 2);

                        if (_gameState.MoveHistory.Count % 2 == 1)
                        {
                            MoveHistoryText.Text += $"{moveNumber}. {move.Notation} ";
                        }
                        else
                        {
                            MoveHistoryText.Text += $"{move.Notation}\n";
                        }

                        if (_isGameActive && _gameState.CurrentPlayer == PieceColor.Black && _blackPlayer is AIPlayer aiPlayer)
                        {
                            MakeAIMove(aiPlayer);
                        }
                        else if (_isGameActive && _gameState.CurrentPlayer == PieceColor.White && _whitePlayer is AIPlayerExtended whiteAIPlayer)
                        {
                            MakeAIMove(whiteAIPlayer);
                        }
                    }
                }

                _selectedSquare.BorderBrush = Brushes.Black;
                _selectedSquare.BorderThickness = new Thickness(0.5);
                _selectedSquare = null;
                ClearHighlightedMoves();
            }
        }

        private void AddHighlightToSquare(Border targetSquare, Ellipse highlight)
        {
            if (targetSquare.Child is Image existingImage)
            {
                // Create a new grid to hold both the image and highlight
                Grid grid = new Grid();
                
                // Remove the image from its current parent first
                targetSquare.Child = null;
                
                // Add both elements to the grid
                grid.Children.Add(existingImage);
                grid.Children.Add(highlight);
                
                // Set the grid as the new child
                targetSquare.Child = grid;
            }
            else if (targetSquare.Child is Grid existingGrid)
            {
                // If it's already a grid, just add the highlight
                existingGrid.Children.Add(highlight);
            }
            else
            {
                // If no existing content, just add the highlight directly
                targetSquare.Child = highlight;
            }
        }

        private Image GetSquarePieceImage(Border square)
        {
            if (square.Child is Image image)
            {
                return image;
            }
            else if (square.Child is Grid grid)
            {
                // Look for an Image in the grid's children
                foreach (UIElement child in grid.Children)
                {
                    if (child is Image gridImage)
                    {
                        return gridImage;
                    }
                }
            }
            return null;
        }

        private void HandlePawnPromotionIfNeeded(int row, int col)
        {
            ChessPiece piece = _chessBoard.GetPieceAt(row, col);
            if (piece is Pawn pawn)
            {
                if ((pawn.Color == PieceColor.White && row == 0) ||
                    (pawn.Color == PieceColor.Black && row == 7))
                {
                    var dialog = new Dialogs.PawnPromotionDialog();
                    dialog.Owner = this;
                    if (dialog.ShowDialog() == true)
                    {
                        var newPiece = dialog.SelectedPiece;
                        _chessBoard.PromotePawn(row, col, newPiece, pawn.Color);
                        UpdateBoardDisplay();
                        UpdateScoreAndCaptured();
                    }
                }
            }
        }

        private void ClearHighlightedMoves()
        {
            foreach (Border square in _highlightedMoves)
            {
                if (square.Child is Grid grid)
                {
                    Image preservedImage = null;
                    
                    // Find and preserve any Image element
                    foreach (UIElement element in grid.Children)
                    {
                        if (element is Image image)
                        {
                            preservedImage = image;
                            break;
                        }
                    }

                    // Clear all children from the grid
                    grid.Children.Clear();

                    // If we found an image, set it as the direct child of the square
                    if (preservedImage != null)
                    {
                        square.Child = preservedImage;
                    }
                    else
                    {
                        square.Child = null;
                    }
                }
                else if (square.Child is Ellipse)
                {
                    // Remove standalone highlights
                    square.Child = null;
                }
            }

            _highlightedMoves.Clear();
        }

        private void MakeAIMove(AIPlayerExtended aiPlayer)
        {
            StatusText.Text = "AI is thinking...";

            Dispatcher.BeginInvoke(new Action(() =>
            {
                ChessMove move = aiPlayer.GetNextMove(_chessBoard, _gameState);

                // Execute the move
                if (move != null && _gameState.TryMakeMove(move))
                {
                    // Keep UI board model in sync with GameState board
                    _chessBoard.MovePiece(move.FromRow, move.FromCol, move.ToRow, move.ToCol);

                    // Update UI
                    UpdateBoardDisplay();
                    UpdateGameInfo();

                    // Add move to history display
                    int moveNumber = _gameState.MoveHistory.Count / 2 + (_gameState.MoveHistory.Count % 2);

                    if (_gameState.MoveHistory.Count % 2 == 1)
                    {
                        MoveHistoryText.Text += $"{moveNumber}. {move.Notation} ";
                    }
                    else
                    {
                        MoveHistoryText.Text += $"{move.Notation}\n";
                    }
                }
            }));
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedSquare != null)
            {
                _selectedSquare.BorderBrush = Brushes.Black;
                _selectedSquare.BorderThickness = new Thickness(0.5);
                _selectedSquare = null;
            }
            ClearHighlightedMoves();

            // New game
            SetupNewGame();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void PlayerVsPlayer_Click(object sender, RoutedEventArgs e)
        {
            PlayerVsPlayerMenuItem.IsChecked = true;
            PlayerVsComputerMenuItem.IsChecked = false;
            AIDifficultyMenu.IsEnabled = false;

            if (_isGameActive)
            {
                _whitePlayer = new HumanPlayer(PieceColor.White);
                _blackPlayer = new HumanPlayer(PieceColor.Black);
            }
        }

        private void PlayerVsComputer_Click(object sender, RoutedEventArgs e)
        {
            PlayerVsPlayerMenuItem.IsChecked = false;
            PlayerVsComputerMenuItem.IsChecked = true;
            AIDifficultyMenu.IsEnabled = true;

            if (_isGameActive)
            {
                _whitePlayer = new HumanPlayer(PieceColor.White);
                _blackPlayer = new AIPlayerExtended(PieceColor.Black, (AIPlayerExtended.Difficulty)_aiDifficulty);

                if (_gameState.CurrentPlayer == PieceColor.Black)
                {
                    MakeAIMove((AIPlayerExtended)_blackPlayer);
                }
            }
        }

        private void AIEasy_Click(object sender, RoutedEventArgs e)
        {
            SetAIDifficulty(AIPlayer.Difficulty.Easy);
        }

        private void AIMedium_Click(object sender, RoutedEventArgs e)
        {
            SetAIDifficulty(AIPlayer.Difficulty.Medium);
        }

        private void AIHard_Click(object sender, RoutedEventArgs e)
        {
            SetAIDifficulty(AIPlayer.Difficulty.Hard);
        }

        private void AIReactive_Click(object sender, RoutedEventArgs e)
        {
            if (CheckGitHubTokenForAdvancedAI())
            {
                SetAIDifficulty(AIPlayer.Difficulty.Reactive);
            }
        }

        private void AIAverage_Click(object sender, RoutedEventArgs e)
        {
            if (CheckGitHubTokenForAdvancedAI())
            {
                SetAIDifficulty(AIPlayer.Difficulty.Average);
            }
        }

        private void AIWorldChampion_Click(object sender, RoutedEventArgs e)
        {
            if (CheckGitHubTokenForAdvancedAI())
            {
                SetAIDifficulty(AIPlayer.Difficulty.WorldChampion);
            }
        }

        private void SetAIDifficulty(AIPlayer.Difficulty difficulty)
        {
            _aiDifficulty = difficulty;
            
            // Uncheck all difficulty menu items
            AIEasyMenuItem.IsChecked = false;
            AIMediumMenuItem.IsChecked = false;
            AIHardMenuItem.IsChecked = false;
            AIReactiveMenuItem.IsChecked = false;
            AIAverageMenuItem.IsChecked = false;
            AIWorldChampionMenuItem.IsChecked = false;

            // Check the selected difficulty
            switch (difficulty)
            {
                case AIPlayer.Difficulty.Easy:
                    AIEasyMenuItem.IsChecked = true;
                    break;
                case AIPlayer.Difficulty.Medium:
                    AIMediumMenuItem.IsChecked = true;
                    break;
                case AIPlayer.Difficulty.Hard:
                    AIHardMenuItem.IsChecked = true;
                    break;
                case AIPlayer.Difficulty.Reactive:
                    AIReactiveMenuItem.IsChecked = true;
                    break;
                case AIPlayer.Difficulty.Average:
                    AIAverageMenuItem.IsChecked = true;
                    break;
                case AIPlayer.Difficulty.WorldChampion:
                    AIWorldChampionMenuItem.IsChecked = true;
                    break;
            }

            if (_isGameActive && _blackPlayer is AIPlayerExtended)
            {
                ((AIPlayerExtended)_blackPlayer).SetDifficulty((AIPlayerExtended.Difficulty)_aiDifficulty);
            }
        }

        private bool CheckGitHubTokenForAdvancedAI()
        {
            if (!Services.GitHubTokenManager.HasGitHubToken())
            {
                Services.GitHubTokenManager.PromptForTokenSetup(
                    (msg, caption) => MessageBox.Show(msg, caption, MessageBoxButton.YesNo, MessageBoxImage.Information),
                    errMsg => MessageBox.Show(errMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                );
                UpdateAIDifficultyMenuAvailability();
                return false;
            }
            UpdateAIDifficultyMenuAvailability();
            return true;
        }

        private void UpdateAIDifficultyMenuAvailability()
        {
            bool hasToken = AIChess.Services.GitHubTokenManager.HasGitHubToken();
            if (AIReactiveMenuItem != null) AIReactiveMenuItem.IsEnabled = hasToken;
            if (AIAverageMenuItem != null) AIAverageMenuItem.IsEnabled = hasToken;
            if (AIWorldChampionMenuItem != null) AIWorldChampionMenuItem.IsEnabled = hasToken;
        }

        private void LoadAndApplyStartupColors()
        {
            // Load colors using the same method as SettingsDialog
            var lightSquareColor = LoadColorFromSettings("LightSquareColor", Colors.Beige);
            var darkSquareColor = LoadColorFromSettings("DarkSquareColor", Colors.SaddleBrown);
            
            // Apply the loaded colors to the board
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Border square = _boardSquares[row, col];
                    bool isLightSquare = (row + col) % 2 == 0;
                    
                    square.Background = isLightSquare 
                        ? new SolidColorBrush(lightSquareColor)
                        : new SolidColorBrush(darkSquareColor);
                }
            }
        }

        private Color LoadColorFromSettings(string key, Color defaultColor)
        {
            try
            {
                using (var key_reg = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"SOFTWARE\AIChess\Colors"))
                {
                    string colorString = key_reg?.GetValue(key) as string;
                    if (!string.IsNullOrEmpty(colorString))
                    {
                        return (Color)ColorConverter.ConvertFromString(colorString);
                    }
                }
            }
            catch
            {
                // If loading fails, use default
            }
            return defaultColor;
        }

        private void ApplyColorSettings(Dialogs.SettingsDialog settingsDialog)
        {
            // Update chess board square colors
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Border square = _boardSquares[row, col];
                    bool isLightSquare = (row + col) % 2 == 0;
                    
                    square.Background = isLightSquare 
                        ? new SolidColorBrush(settingsDialog.LightSquareColor)
                        : new SolidColorBrush(settingsDialog.DarkSquareColor);
                }
            }

            // TODO: Apply player colors when displaying player names or highlighting pieces
            // For now, the player colors are stored and can be used for future UI enhancements
            // such as coloring player names, move highlights, or status indicators
        }

        private void Resign_Click(object sender, RoutedEventArgs e)
        {
            if (!_isGameActive) return;

            _isGameActive = false;

            PieceColor winner = _gameState.CurrentPlayer == PieceColor.White ? PieceColor.Black : PieceColor.White;
            string winnerName = winner == PieceColor.White ? "White" : "Black";

            GameStatusText.Text = "Game Over - Resigned";
            StatusText.Text = $"Game over. {winnerName} wins by resignation!";

            string playerColor = _gameState.CurrentPlayer == PieceColor.White ? "White" : "Black";
            int moveNumber = _gameState.MoveHistory.Count / 2 + 1;

            if (_gameState.MoveHistory.Count % 2 == 0)
            {
                MoveHistoryText.Text += $"{moveNumber}. {playerColor} resigns ";
            }
            else
            {
                MoveHistoryText.Text += $"{playerColor} resigns\n";
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var settingsDialog = new Dialogs.SettingsDialog();
            settingsDialog.Owner = this;
            
            if (settingsDialog.ShowDialog() == true)
            {
                // If token was updated, refresh the AI difficulty menu availability
                if (settingsDialog.TokenUpdated)
                {
                    UpdateAIDifficultyMenuAvailability();
                }
                
                // If colors were updated, apply them to the board
                if (settingsDialog.ColorsUpdated)
                {
                    ApplyColorSettings(settingsDialog);
                }
            }
        }

        private void UpdateScoreAndCaptured()
        {
            if (_gameState == null) return;

            // Use the authoritative GameState board to compute material
            var whitePieces = _gameState.Board.GetPieces(PieceColor.White);
            var blackPieces = _gameState.Board.GetPieces(PieceColor.Black);

            int whiteMaterial = whitePieces.Sum(p => p.GetValue());
            int blackMaterial = blackPieces.Sum(p => p.GetValue());
            int diff = whiteMaterial - blackMaterial;

            string advantage = diff == 0 ? "Even" : (diff > 0 ? $"White +{diff}" : $"Black +{-diff}");
            ScoreText.Text = $"White {whiteMaterial} - Black {blackMaterial} ({advantage})";

            // Update captured pieces panels
            CapturedByWhitePanel.Children.Clear();
            CapturedByBlackPanel.Children.Clear();

            if (_gameState.CapturedByWhite != null)
            {
                foreach (var cap in _gameState.CapturedByWhite)
                {
                    CapturedByWhitePanel.Children.Add(CreateSmallPieceImage(cap));
                }
            }
            if (_gameState.CapturedByBlack != null)
            {
                foreach (var cap in _gameState.CapturedByBlack)
                {
                    CapturedByBlackPanel.Children.Add(CreateSmallPieceImage(cap));
                }
            }
        }

        private Image CreateSmallPieceImage(ChessPiece piece)
        {
            var img = new Image
            {
                Source = GetPieceImage(piece),
                Width = 24,
                Height = 24,
                Margin = new Thickness(2)
            };
            return img;
        }
    }
}

