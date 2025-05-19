using System.Windows;

namespace TrubChess.Dialogs
{
    public partial class PawnPromotionDialog : Window
    {
        public string SelectedPiece { get; private set; }

        public PawnPromotionDialog()
        {
            InitializeComponent();
        }

        private void Promotion_Click(object sender, RoutedEventArgs e)
        {
            if (sender == QueenButton)
                SelectedPiece = "Queen";
            else if (sender == RookButton)
                SelectedPiece = "Rook";
            else if (sender == BishopButton)
                SelectedPiece = "Bishop";
            else if (sender == KnightButton)
                SelectedPiece = "Knight";
            DialogResult = true;
            Close();
        }
    }
}