using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using AIChess.Services;

namespace AIChess.Dialogs
{
    public partial class SettingsDialog : Window
    {
        private readonly DatabaseService _databaseService;
        
        public bool TokenUpdated { get; private set; }
        public bool ColorsUpdated { get; private set; }

        // Color properties
        public Color Player1Color { get; private set; } = Colors.Blue;
        public Color Player2Color { get; private set; } = Colors.Red;
        public Color AIColor { get; private set; } = Colors.Green;
        public Color LightSquareColor { get; private set; } = Colors.White;
        public Color DarkSquareColor { get; private set; } = Colors.SaddleBrown;
        
        // Rook-specific color properties
        public Color RookPlayer1Color { get; private set; } = Colors.Blue;
        public Color RookPlayer2Color { get; private set; } = Colors.Red;
        public Color RookAIColor { get; private set; } = Colors.Green;

        public SettingsDialog()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            _databaseService.InitializeDatabase();
            LoadCurrentToken();
            LoadCurrentColors();
        }

        private void LoadCurrentToken()
        {
            // Load existing GitHub token if available
            string existingToken = GitHubTokenManager.GetGitHubToken();
            if (!string.IsNullOrEmpty(existingToken))
            {
                GitHubTokenPasswordBox.Password = existingToken;
            }
            
            // Load existing OpenAI API key if available
            string existingOpenAIKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (!string.IsNullOrEmpty(existingOpenAIKey))
            {
                OpenAIKeyPasswordBox.Password = existingOpenAIKey;
            }
        }

        private void LoadCurrentColors()
        {
            // Load saved colors from settings or use defaults
            Player1Color = LoadColorFromSettings("Player1Color", Colors.Blue);
            Player2Color = LoadColorFromSettings("Player2Color", Colors.Red);
            AIColor = LoadColorFromSettings("AIColor", Colors.Green);
            LightSquareColor = LoadColorFromSettings("LightSquareColor", Colors.White);
            DarkSquareColor = LoadColorFromSettings("DarkSquareColor", Colors.SaddleBrown);

            // Load rook-specific colors from database with fallback to player colors
            RookPlayer1Color = LoadColorFromDatabase("RookPlayer1Color", Player1Color);
            RookPlayer2Color = LoadColorFromDatabase("RookPlayer2Color", Player2Color);
            RookAIColor = LoadColorFromDatabase("RookAIColor", AIColor);

            // Update preview rectangles
            UpdateColorPreviews();
        }

        private Color LoadColorFromSettings(string key, Color defaultColor)
        {
            try
            {
                // Try to load from registry or configuration
                // For now, we'll use a simple approach with registry
                using (var key_reg = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\AIChess\Colors"))
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

        private Color LoadColorFromDatabase(string key, Color defaultColor)
        {
            try
            {
                string colorString = _databaseService.LoadColorSetting(key);
                if (!string.IsNullOrEmpty(colorString))
                {
                    return (Color)ColorConverter.ConvertFromString(colorString);
                }
            }
            catch
            {
                // If loading fails, use default
            }
            return defaultColor;
        }

        private void SaveColorToSettings(string key, Color color)
        {
            try
            {
                using (var key_reg = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\AIChess\Colors"))
                {
                    key_reg?.SetValue(key, color.ToString());
                }
            }
            catch
            {
                // Ignore save errors
            }
        }

        private void SaveColorToDatabase(string key, Color color)
        {
            try
            {
                _databaseService.SaveColorSetting(key, color.ToString());
            }
            catch
            {
                // Ignore save errors
            }
        }

        private void UpdateColorPreviews()
        {
            Player1ColorPreview.Fill = new SolidColorBrush(Player1Color);
            Player2ColorPreview.Fill = new SolidColorBrush(Player2Color);
            AIColorPreview.Fill = new SolidColorBrush(AIColor);
            LightSquareColorPreview.Fill = new SolidColorBrush(LightSquareColor);
            DarkSquareColorPreview.Fill = new SolidColorBrush(DarkSquareColor);
            
            // Update rook color previews
            RookPlayer1ColorPreview.Fill = new SolidColorBrush(RookPlayer1Color);
            RookPlayer2ColorPreview.Fill = new SolidColorBrush(RookPlayer2Color);
            RookAIColorPreview.Fill = new SolidColorBrush(RookAIColor);
        }

        private void TestToken_Click(object sender, RoutedEventArgs e)
        {
            string token = GitHubTokenPasswordBox.Password;
            if (string.IsNullOrWhiteSpace(token))
            {
                MessageBox.Show("Please enter a token first.", "No Token", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Simple validation - check if it looks like a GitHub token
            if (token.Length < 20 || (!token.StartsWith("ghp_") && !token.StartsWith("github_pat_")))
            {
                MessageBox.Show("The token format doesn't appear to be a valid GitHub Personal Access Token.\n\nGitHub tokens typically start with 'ghp_' or 'github_pat_' and are at least 20 characters long.", 
                    "Invalid Token Format", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show("Token format appears valid. Note: This is a basic format check only.\n\nTo fully verify the token works, try using one of the advanced AI difficulty levels after saving.", 
                "Token Test", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SetEnvironmentVariable_Click(object sender, RoutedEventArgs e)
        {
            string token = GitHubTokenPasswordBox.Password;
            if (string.IsNullOrWhiteSpace(token))
            {
                MessageBox.Show("Please enter a token first.", "No Token", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Set the environment variable for the current process
                Environment.SetEnvironmentVariable("GITHUB_ACCESS_TOKEN", token);
                
                // Also try to set it as a user environment variable
                Environment.SetEnvironmentVariable("GITHUB_ACCESS_TOKEN", token, EnvironmentVariableTarget.User);
                
                MessageBox.Show("Environment variable set successfully!\n\nNote: You may need to restart the application for the change to take full effect.", 
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                
                TokenUpdated = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to set environment variable: {ex.Message}\n\nTry using 'Open System Environment Variables' instead.", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenEnvVars_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Open System Properties -> Advanced -> Environment Variables
                Process.Start("rundll32.exe", "sysdm.cpl,EditEnvironmentVariables");
                
                MessageBox.Show("System Environment Variables dialog opened.\n\nAdd a new variable:\nName: GITHUB_ACCESS_TOKEN\nValue: Your GitHub Personal Access Token", 
                    "Environment Variables", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open environment variables dialog: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TestOpenAIKey_Click(object sender, RoutedEventArgs e)
        {
            string apiKey = OpenAIKeyPasswordBox.Password;
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                MessageBox.Show("Please enter an API key first.", "No API Key", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Simple validation - check if it looks like an OpenAI API key
            if (apiKey.Length < 40 || !apiKey.StartsWith("sk-"))
            {
                MessageBox.Show("The API key format doesn't appear to be a valid OpenAI API key.\n\nOpenAI API keys typically start with 'sk-' and are around 50+ characters long.", 
                    "Invalid API Key Format", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show("API key format appears valid. Note: This is a basic format check only.\n\nTo fully verify the API key works, try using World Champion difficulty after saving.", 
                "API Key Test", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SetOpenAIEnvironmentVariable_Click(object sender, RoutedEventArgs e)
        {
            string apiKey = OpenAIKeyPasswordBox.Password;
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                MessageBox.Show("Please enter an API key first.", "No API Key", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Set the environment variable for the current process
                Environment.SetEnvironmentVariable("OPENAI_API_KEY", apiKey);
                
                // Also try to set it as a user environment variable
                Environment.SetEnvironmentVariable("OPENAI_API_KEY", apiKey, EnvironmentVariableTarget.User);
                
                MessageBox.Show("Environment variable set successfully!\n\nNote: You may need to restart the application for the change to take full effect.", 
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                
                TokenUpdated = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to set environment variable: {ex.Message}\n\nTry setting it manually in System Environment Variables.", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        // Color selection event handlers
        private void Player1Color_Click(object sender, RoutedEventArgs e)
        {
            Color? selectedColor = ShowColorDialog(Player1Color);
            if (selectedColor.HasValue)
            {
                Player1Color = selectedColor.Value;
                Player1ColorPreview.Fill = new SolidColorBrush(Player1Color);
                SaveColorToSettings("Player1Color", Player1Color);
                ColorsUpdated = true;
            }
        }

        private void Player2Color_Click(object sender, RoutedEventArgs e)
        {
            Color? selectedColor = ShowColorDialog(Player2Color);
            if (selectedColor.HasValue)
            {
                Player2Color = selectedColor.Value;
                Player2ColorPreview.Fill = new SolidColorBrush(Player2Color);
                SaveColorToSettings("Player2Color", Player2Color);
                ColorsUpdated = true;
            }
        }

        private void AIColor_Click(object sender, RoutedEventArgs e)
        {
            Color? selectedColor = ShowColorDialog(AIColor);
            if (selectedColor.HasValue)
            {
                AIColor = selectedColor.Value;
                AIColorPreview.Fill = new SolidColorBrush(AIColor);
                SaveColorToSettings("AIColor", AIColor);
                ColorsUpdated = true;
            }
        }

        private void LightSquareColor_Click(object sender, RoutedEventArgs e)
        {
            Color? selectedColor = ShowColorDialog(LightSquareColor);
            if (selectedColor.HasValue)
            {
                LightSquareColor = selectedColor.Value;
                LightSquareColorPreview.Fill = new SolidColorBrush(LightSquareColor);
                SaveColorToSettings("LightSquareColor", LightSquareColor);
                ColorsUpdated = true;
            }
        }

        private void DarkSquareColor_Click(object sender, RoutedEventArgs e)
        {
            Color? selectedColor = ShowColorDialog(DarkSquareColor);
            if (selectedColor.HasValue)
            {
                DarkSquareColor = selectedColor.Value;
                DarkSquareColorPreview.Fill = new SolidColorBrush(DarkSquareColor);
                SaveColorToSettings("DarkSquareColor", DarkSquareColor);
                ColorsUpdated = true;
            }
        }

        private void ResetColors_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Reset all colors to their default values?", 
                "Reset Colors", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                Player1Color = Colors.Blue;
                Player2Color = Colors.Red;
                AIColor = Colors.Green;
                LightSquareColor = Colors.White;
                DarkSquareColor = Colors.SaddleBrown;

                // Reset rook colors to match player colors
                RookPlayer1Color = Colors.Blue;
                RookPlayer2Color = Colors.Red;
                RookAIColor = Colors.Green;

                UpdateColorPreviews();

                // Save defaults
                SaveColorToSettings("Player1Color", Player1Color);
                SaveColorToSettings("Player2Color", Player2Color);
                SaveColorToSettings("AIColor", AIColor);
                SaveColorToSettings("LightSquareColor", LightSquareColor);
                SaveColorToSettings("DarkSquareColor", DarkSquareColor);

                // Save rook defaults to database
                SaveColorToDatabase("RookPlayer1Color", RookPlayer1Color);
                SaveColorToDatabase("RookPlayer2Color", RookPlayer2Color);
                SaveColorToDatabase("RookAIColor", RookAIColor);

                ColorsUpdated = true;
            }
        }

        // Rook color selection event handlers
        private void RookPlayer1Color_Click(object sender, RoutedEventArgs e)
        {
            Color? selectedColor = ShowColorDialog(RookPlayer1Color);
            if (selectedColor.HasValue)
            {
                RookPlayer1Color = selectedColor.Value;
                RookPlayer1ColorPreview.Fill = new SolidColorBrush(RookPlayer1Color);
                SaveColorToDatabase("RookPlayer1Color", RookPlayer1Color);
                ColorsUpdated = true;
            }
        }

        private void RookPlayer2Color_Click(object sender, RoutedEventArgs e)
        {
            Color? selectedColor = ShowColorDialog(RookPlayer2Color);
            if (selectedColor.HasValue)
            {
                RookPlayer2Color = selectedColor.Value;
                RookPlayer2ColorPreview.Fill = new SolidColorBrush(RookPlayer2Color);
                SaveColorToDatabase("RookPlayer2Color", RookPlayer2Color);
                ColorsUpdated = true;
            }
        }

        private void RookAIColor_Click(object sender, RoutedEventArgs e)
        {
            Color? selectedColor = ShowColorDialog(RookAIColor);
            if (selectedColor.HasValue)
            {
                RookAIColor = selectedColor.Value;
                RookAIColorPreview.Fill = new SolidColorBrush(RookAIColor);
                SaveColorToDatabase("RookAIColor", RookAIColor);
                ColorsUpdated = true;
            }
        }

        private Color? ShowColorDialog(Color currentColor)
        {
            var dlg = new ColorPickerDialog(currentColor) { Owner = this };
            var result = dlg.ShowDialog();
            if (result == true)
            {
                return dlg.SelectedColor;
            }
            return null;
        }
    }
}