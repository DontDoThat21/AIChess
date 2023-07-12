using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TrubChess
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ColorBg();
            SetChessPiecesUp();
        }

        private void SetChessPiecesUp()
        {
            throw new NotImplementedException();
        }

        private void ColorBg()
        {
            for (int c = 1; c < ChessBoard.ColumnDefinitions.Count; c++)
            {
                for (int r = 1; r < ChessBoard.RowDefinitions.Count; r++)
                {
                    var cell = new Border
                    { 
                        Background = Brushes.AliceBlue
                    };
                    var textTest = new TextBlock
                    {
                        Text = $"c:{c} r: {r}"
                    };
                    cell.Child = textTest;
                    Grid.SetRow(cell, r);
                    Grid.SetColumn(cell, c);
                    ChessBoard.Children.Add(cell);
                }
            }
        }
    }
}
