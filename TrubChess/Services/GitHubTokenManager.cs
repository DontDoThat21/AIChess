using System;
using System.Diagnostics;
using System.Windows;

namespace TrubChess.Services
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

        public static bool PromptForTokenSetup()
        {
            var result = MessageBox.Show(
                "No GitHub access token found in environment variables.\n\n" +
                "To enable advanced AI difficulty options (Reactive, Average, World Champion), " +
                "you need to set a GitHub access token in your environment variables.\n\n" +
                "Would you like to open System Environment Variables to set the token?\n\n" +
                "Environment variable name: " + GITHUB_TOKEN_ENV_VAR,
                "GitHub Access Token Required",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    OpenEnvironmentVariables();
                    return true;
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    MessageBox.Show(
                        "Failed to open System Environment Variables due to a system error: " + ex.Message + "\n\n" +
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