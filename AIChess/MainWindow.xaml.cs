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
using Microsoft.Win32; // Added for loading colors from settings

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

        // Player color settings
        private Color _player1Color = Colors.Blue;
        private Color _player2Color = Colors.Red;
        private Color _aiColor = Colors.Green;

        public MainWindow()
        {
            InitializeComponent();
            _boardSquares = new Border[8, 8];
            _highlightedMoves = new List<Border>();
            _aiDifficulty = AIPlayer.Difficulty.Easy;
            LoadPlayerColorsFromSettings();
            InitializeBoard();
            UpdateAIDifficultyMenuAvailability();
            SetupNewGame();
        }

        private void InitializeBoard()
        {
            // Load square colors from settings (fall back to current defaults)
            Color lightColor = LoadColorFromSettings("LightSquareColor", Colors.Beige);
            Color darkColor = LoadColorFromSettings("DarkSquareColor", Colors.SaddleBrown);
            var lightBrush = new SolidColorBrush(lightColor);
            var darkBrush = new SolidColorBrush(darkColor);

            // Create the checkerboard pattern
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Border square = new Border();
                    square.Background = (row + col) % 2 == 0 ? lightBrush : darkBrush;
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

        private void ApplyBoardColors()
        {
            Color lightColor = LoadColorFromSettings("LightSquareColor", Colors.Beige);
            Color darkColor = LoadColorFromSettings("DarkSquareColor", Colors.SaddleBrown);
            var lightBrush = new SolidColorBrush(lightColor);
            var darkBrush = new SolidColorBrush(darkColor);

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var square = _boardSquares[row, col];
                    if (square == null) continue;
                    square.Background = (row + col) % 2 == 0 ? lightBrush : darkBrush;
                }
            }
        }

        private void LoadPlayerColorsFromSettings()
        {
            _player1Color = LoadColorFromSettings("Player1Color", Colors.Blue);
            _player2Color = LoadColorFromSettings("Player2Color", Colors.Red);
            _aiColor = LoadColorFromSettings("AIColor", Colors.Green);
        }

        private Color LoadColorFromSettings(string key, Color fallback)
        {
            try
            {
                using (var keyReg = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\\AIChess\\Colors"))
                {
                    string colorString = keyReg?.GetValue(key) as string;
                    if (!string.IsNullOrEmpty(colorString))
                    {
                        return (Color)ColorConverter.ConvertFromString(colorString);
                    }
                }
            }
            catch
            {
                // ignore and use fallback
            }
            return fallback;
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
                _blackPlayer = new AIPlayer(PieceColor.Black, _aiDifficulty);

            UpdateBoardDisplay();
            UpdateGameInfo();

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
            // Reload player colors in case they changed
            LoadPlayerColorsFromSettings();

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
                        // Always use the white asset as a base so tint colors are vivid
                        var baseSource = GetPieceImageBaseWhite(piece);
                        var tintColor = GetTintForPiece(piece.Color);
                        var tinted = TintImage(baseSource, tintColor);
                        pieceImage.Source = tinted;
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

        private ImageSource GetPieceImageBaseWhite(ChessPiece piece)
        {
            string pieceName = piece.GetType().Name.ToLower();
            string fileName;

            if (piece is Knight k && k.IsLeftKnight)
                fileName = "whiteknightl.png";
            else if (piece is Knight)
                fileName = "whiteknightr.png";
            else if (piece is Bishop b && b.IsLeftBishop)
                fileName = "whitebishopl.png";
            else if (piece is Bishop)
                fileName = "whitebishopr.png";
            else
                fileName = $"white{pieceName}.png";

            // Same-assembly pack URI (no ;component needed when not crossing assemblies)
            var uri = new Uri($"pack://application:,,,/Resources/{fileName}", UriKind.Absolute);

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = uri;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;   // ensure stream is closed for tinting
            bitmapImage.CreateOptions = BitmapCreateOptions.None;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }

        private Color GetTintForPiece(PieceColor pieceColor)
        {
            if (pieceColor == PieceColor.White)
            {
                // White pieces: if controlled by AI, use AI color; else Player 1
                if (_whitePlayer is AIPlayer) return _aiColor;
                return _player1Color;
            }
            else
            {
                // Black pieces: if controlled by AI, use AI color; else Player 2
                if (_blackPlayer is AIPlayer) return _aiColor;
                return _player2Color;
            }
        }

        private ImageSource TintImage(ImageSource source, Color tint)
        {
            var bitmapSource = source as BitmapSource;
            if (bitmapSource == null)
                return source;

            // Ensure format is BGRA32 for easy manipulation
            var converted = new FormatConvertedBitmap(bitmapSource, PixelFormats.Bgra32, null, 0);

            int width = converted.PixelWidth;
            int height = converted.PixelHeight;
            int stride = width * 4;
            byte[] pixels = new byte[height * stride];
            converted.CopyPixels(pixels, stride, 0);

            for (int i = 0; i < pixels.Length; i += 4)
            {
                byte b = pixels[i + 0];
                byte g = pixels[i + 1];
                byte r = pixels[i + 2];
                byte a = pixels[i + 3];
                if (a == 0) continue; // transparent pixel

                // Compute brightness from the base (white) asset
                double brightness = Math.Max(r, Math.Max(g, b)) / 255.0; // 0..1

                pixels[i + 0] = (byte)(tint.B * brightness);
                pixels[i + 1] = (byte)(tint.G * brightness);
                pixels[i + 2] = (byte)(tint.R * brightness);
                // preserve alpha
            }

            var writable = new WriteableBitmap(width, height, converted.DpiX, converted.DpiY, PixelFormats.Bgra32, null);
            writable.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
            writable.Freeze();
            return writable;
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
                            else if (_isGameActive && _gameState.CurrentPlayer == PieceColor.White && _whitePlayer is AIPlayer whiteAIPlayer)
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
                        else if (_isGameActive && _gameState.CurrentPlayer == PieceColor.White && _whitePlayer is AIPlayer whiteAIPlayer)
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

        private void MakeAIMove(AIPlayer aiPlayer)
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
                UpdateBoardDisplay();
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
                _blackPlayer = new AIPlayer(PieceColor.Black, _aiDifficulty);
                UpdateBoardDisplay();

                if (_gameState.CurrentPlayer == PieceColor.Black)
                {
                    MakeAIMove((AIPlayer)_blackPlayer);
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

            if (_isGameActive && _blackPlayer is AIPlayer)
            {
                ((AIPlayer)_blackPlayer).SetDifficulty(_aiDifficulty);
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

                // If colors were updated, refresh board and piece colors
                if (settingsDialog.ColorsUpdated)
                {
                    LoadPlayerColorsFromSettings();
                    ApplyBoardColors();
                    UpdateBoardDisplay();
                }
            }
        }
    }
}

