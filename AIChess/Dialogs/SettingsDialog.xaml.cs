using System;
using System.Diagnostics;
using System.Windows;
using Microsoft.Win32;
using AIChess.Services;

namespace AIChess.Dialogs
{
    public partial class SettingsDialog : Window
    {
        public bool TokenUpdated { get; private set; }

        public SettingsDialog()
        {
            InitializeComponent();
            LoadCurrentToken();
        }

        private void LoadCurrentToken()
        {
            // Load existing token if available
            string existingToken = GitHubTokenManager.GetGitHubToken();
            if (!string.IsNullOrEmpty(existingToken))
            {
                GitHubTokenPasswordBox.Password = existingToken;
            }
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