# AI Difficulty Levels Documentation

## Overview
The TrubChess application now supports multiple AI difficulty levels, including advanced AI options that utilize GitHub access tokens for enhanced gameplay.

## Difficulty Levels

### Basic AI Levels
- **Easy**: Search depth 2, 25% random moves
- **Medium**: Search depth 3, 10% random moves  
- **Hard**: Search depth 4, no random moves

### Advanced AI Levels (Requires GitHub Token)
- **Reactive**: Search depth 3, 5% random moves, analyzes opponent move patterns
- **Average**: Search depth 5, 2% random moves, enhanced position evaluation
- **World Champion**: Search depth 6, no random moves, maximum difficulty

## GitHub Token Setup

### Environment Variable
The advanced AI levels require a GitHub access token set in the environment variable:
- **Variable Name**: `GITHUB_ACCESS_TOKEN`
- **Variable Value**: Your GitHub personal access token

### Setting Up the Token

1. **Automatic Setup**: When selecting an advanced AI difficulty without a token, the application will prompt you to set one and can open System Environment Variables.

2. **Manual Setup**: 
   - Open System Properties > Advanced > Environment Variables
   - Add a new user or system variable:
     - Name: `GITHUB_ACCESS_TOKEN`
     - Value: Your GitHub access token
   - Restart the application

### Fallback Behavior
If an advanced AI difficulty is selected but no GitHub token is available, the AI will automatically fall back to "Hard" difficulty behavior to ensure the game remains playable.

## Features

### Move History Analysis
Advanced AI difficulties have access to the complete move history and can:
- Detect opponent patterns
- Adjust strategy based on previous moves
- Make more informed tactical decisions

### Enhanced Search
Higher difficulty levels use deeper search algorithms:
- More comprehensive position evaluation
- Better long-term planning
- Improved endgame play

## Usage Notes
- Restart the application after setting the GitHub token environment variable
- Advanced AI levels may take slightly longer to compute moves due to enhanced analysis
- The token is only used for enabling advanced AI features; no external API calls are made