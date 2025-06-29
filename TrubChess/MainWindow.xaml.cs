using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TrubChess.Models;
using TrubChess.Players;
using TrubChess.AI;
using TrubChess.Models.Pieces;
using System.Windows.Media.Animation;
using System.Windows.Documents;

namespace TrubChess
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
            foreach (var square in _boardSquares)
            {
                if (square.Child is Image || (square.Child is Grid grid && grid.Children.Count > 0))
                {
                    square.Child = null;
                }
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

        private ImageSource GetPieceImage(ChessPiece piece)
        {
            string color = piece.Color == PieceColor.White ? "white" : "black";
            string pieceName = piece.GetType().Name.ToLower();

            if (piece is Knight && ((Knight)piece).IsLeftKnight)
                return new BitmapImage(new Uri($"/TrubChess;component/Resources/{color}knightl.png", UriKind.Relative));
            else if (piece is Knight)
                return new BitmapImage(new Uri($"/TrubChess;component/Resources/{color}knightr.png", UriKind.Relative));
            else if (piece is Bishop && ((Bishop)piece).IsLeftBishop)
                return new BitmapImage(new Uri($"/TrubChess;component/Resources/{color}bishopl.png", UriKind.Relative));
            else if (piece is Bishop)
                return new BitmapImage(new Uri($"/TrubChess;component/Resources/{color}bishopr.png", UriKind.Relative));
            else
                return new BitmapImage(new Uri($"/TrubChess;component/Resources/{color}{pieceName}.png", UriKind.Relative));
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
                    foreach (var move in validMoves)
                    {
                        Border targetSquare = _boardSquares[move.ToRow, move.ToCol];
                        Ellipse highlight = new Ellipse
                        {
                            Width = 20,
                            Height = 20,
                            Fill = _chessBoard.GetPieceAt(move.ToRow, move.ToCol) != null
                                ? new SolidColorBrush(Color.FromArgb(150, 255, 0, 0))
                                : new SolidColorBrush(Color.FromArgb(150, 0, 255, 0))
                        };

                        if (targetSquare.Child is Image)
                        {
                            Grid grid = new Grid();
                            if (!(targetSquare.Child is Grid))
                                grid.Children.Add(targetSquare.Child);
                            grid.Children.Add(highlight);
                            targetSquare.Child = grid;
                        }
                        else
                        {
                            targetSquare.Child = highlight;
                        }

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

                ChessMove move = new ChessMove(fromRow, fromCol, row, col);
                if (_gameState.TryMakeMove(move))
                {
                    var fromSquare = _boardSquares[fromRow, fromCol];
                    var toSquare = _boardSquares[row, col];
                    Image pieceImage = fromSquare.Child as Image;
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
                    UIElement imageElement = null;
                    foreach (UIElement element in grid.Children)
                    {
                        if (element is Image)
                        {
                            imageElement = element;
                            break;
                        }
                    }

                    if (imageElement != null)
                    {
                        square.Child = imageElement;
                    }
                    else
                    {
                        square.Child = null;
                    }
                }
                else if (square.Child is Ellipse)
                {
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
                _blackPlayer = new AIPlayer(PieceColor.Black, _aiDifficulty);

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
                Services.GitHubTokenManager.PromptForTokenSetup();
                return false;
            }
            return true;
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
    }
}

