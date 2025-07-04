using System;
using System.Diagnostics;
using System.Windows;

namespace AIChess.Services
{
    public static class GitHubTokenManager
    {
        private const string GITHUB_TOKEN_ENV_VAR = "GITHUB_ACCESS_TOKEN";

        public static string GetGitHubToken()
        {
            return Environment.GetEnvironmentVariable(GITHUB_TOKEN_ENV_VAR);
        }

        public static bool HasGitHubToken()
        {
            return !string.IsNullOrWhiteSpace(GetGitHubToken());
        }

        public static bool PromptForTokenSetup(Func<string, string, MessageBoxResult> showMessageBox, Action<string> showErrorMessage)
        {
            var message = "No GitHub access token found in environment variables.\n\n" +
                          "To enable advanced AI difficulty options (Reactive, Average, World Champion), " +
                          "you need to set a GitHub access token in your environment variables.\n\n" +
                          "Would you like to open the Settings dialog to configure it?\n\n" +
                          "Environment variable name: " + GITHUB_TOKEN_ENV_VAR;

            var result = showMessageBox(message, "GitHub Access Token Required");
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Try to open the settings dialog if available
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    if (mainWindow != null)
                    {
                        var settingsDialog = new Dialogs.SettingsDialog();
                        settingsDialog.Owner = mainWindow;
                        settingsDialog.ShowDialog();
                        return true;
                    }
                    else
                    {
                        // Fallback to system environment variables
                        OpenEnvironmentVariables();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Failed to open settings dialog: " + ex.Message + "\n\n" +
                        "Please manually set the environment variable:\n" +
                        "Variable Name: " + GITHUB_TOKEN_ENV_VAR + "\n" +
                        "Variable Value: Your GitHub access token",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }

            return false;
        }

        private static void OpenEnvironmentVariables()
        {
            // Open System Properties -> Advanced -> Environment Variables
            Process.Start("rundll32.exe", "sysdm.cpl,EditEnvironmentVariables");
        }
    }
}