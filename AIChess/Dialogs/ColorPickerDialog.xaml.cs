using System.Windows;
using System.Windows.Media;

namespace AIChess.Dialogs
{
    public partial class ColorPickerDialog : Window
    {
        public Color SelectedColor { get; private set; }

        public ColorPickerDialog(Color initialColor)
        {
            InitializeComponent();
            SetFromColor(initialColor);
        }

        private void SetFromColor(Color c)
        {
            ASlider.Value = c.A;
            RSlider.Value = c.R;
            GSlider.Value = c.G;
            BSlider.Value = c.B;
            UpdatePreview();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            byte a = (byte)ASlider.Value;
            byte r = (byte)RSlider.Value;
            byte g = (byte)GSlider.Value;
            byte b = (byte)BSlider.Value;

            AText.Text = a.ToString();
            RText.Text = r.ToString();
            GText.Text = g.ToString();
            BText.Text = b.ToString();

            SelectedColor = Color.FromArgb(a, r, g, b);
            PreviewRectangle.Fill = new SolidColorBrush(SelectedColor);
            HexText.Text = $"#{a:X2}{r:X2}{g:X2}{b:X2}";
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
