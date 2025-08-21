# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

AIChess is a WPF-based chess application written in C# (.NET Framework 4.8.1) featuring AI opponents with multiple difficulty levels. The project includes both basic and advanced AI implementations, with advanced levels requiring GitHub token authentication for enhanced functionality. The application supports customizable player and board colors with persistent settings.

## Build Commands

Due to .NET Framework compatibility issues with newer dotnet CLI versions, use Visual Studio or MSBuild:

```bash
# Build in Visual Studio (recommended)
# Open AIChess.sln in Visual Studio and build

# Alternative: Use MSBuild if available
msbuild AIChess.sln /p:Configuration=Debug
msbuild AIChess.sln /p:Configuration=Release

# NuGet package restoration (if needed)
nuget restore AIChess.sln
```

Note: `dotnet build` may fail with MSBuild task host errors on this .NET Framework 4.8.1 project.

## Testing

The project includes MSTest-based unit tests in the AIChess.Tests directory:
- ChessRulesTests.cs - Chess rule validation tests  
- IntegrationTests.cs - Integration testing
- NewAIFeaturesTests.cs - AI feature testing

Run tests through Visual Studio Test Explorer or MSBuild test runner.

## Dependencies

Key NuGet packages:
- **OpenAI** (1.11.0) - OpenAI API integration for advanced AI features
- **System.Data.SQLite** (1.0.118.0) - Local database storage
- **EntityFramework** (6.5.1) - ORM for database operations
- **Newtonsoft.Json** (13.0.3) - JSON serialization
- **System.Windows.Forms** - Color picker dialog integration

## Architecture

### Core Components

**Models** (`AIChess/Models/`)
- `ChessBoard.cs` - Board state and piece positioning
- `ChessPiece.cs` - Base piece class with common functionality  
- `GameState.cs` - Game state management (turn, castling rights, en passant)
- `Pieces/` - Individual piece implementations (King, Queen, Rook, Bishop, Knight, Pawn)

**AI System** (`AIChess/AI/`)
- `ChessEngine.cs` - Core AI engine with minimax algorithm
- `MoveEvaluator.cs` - Position evaluation and move scoring

**Players** (`AIChess/Players/`)
- `Player.cs` - Abstract base player class
- `HumanPlayer.cs` - Human player implementation
- `AIPlayer.cs` - AI player with difficulty levels

**Services** (`AIChess/Services/`)
- `GitHubTokenManager.cs` - GitHub token management for advanced AI
- `OpenAIChessService.cs` - OpenAI API integration
- `DatabaseService.cs` - SQLite database management
- `BoardConverter.cs` - Board state text representation utilities

**UI** (`AIChess/`)
- `MainWindow.xaml/.cs` - Main game interface with chess board visualization
- `Dialogs/SettingsDialog` - Multi-tab settings (AI configuration, appearance customization)
- `Dialogs/PawnPromotionDialog` - Pawn promotion piece selection

### AI Difficulty System

The AI system supports 6 difficulty levels as documented in AI_DIFFICULTY_GUIDE.md:

**Basic Levels:**
- Easy (depth 2, 25% random)
- Medium (depth 3, 10% random) 
- Hard (depth 4, no random)

**Advanced Levels** (require GitHub token):
- Reactive (depth 3, pattern analysis)
- Average (depth 5, enhanced evaluation)
- World Champion (depth 6, maximum difficulty)

Advanced AI features require `GITHUB_ACCESS_TOKEN` environment variable. Without it, advanced levels fallback to Hard difficulty.

## Environment Variables

The application uses environment variables for external service configuration:

- **GITHUB_ACCESS_TOKEN**: Required for advanced AI difficulty levels (Reactive, Average, World Champion)
- **OPENAI_API_KEY**: Required for OpenAI-powered AI features

## Settings and Configuration

### User Settings Storage
- **Chess board colors**: Stored in Windows Registry at `HKEY_CURRENT_USER\SOFTWARE\AIChess\Colors`
- **Player colors**: Registry-based persistence for Player 1, Player 2, and AI colors
- **GitHub tokens**: Environment variable management through SettingsDialog

### Database Storage
- **Local SQLite database**: Located at `%APPDATA%\AIChess\AIChess.db`
- **Entity Framework 6**: Used for database operations and migrations

## Key Implementation Notes

- The project references some files via absolute paths from a previous "TrubChess" project - these are linked files, not duplicates
- Chess piece images are stored in `Resources/` directory with separate files for left/right-facing knights and bishops
- Chess board uses Border elements in a Grid (8x8) with dynamic color application
- Move validation and game rules are implemented in individual piece classes
- Color customization system uses System.Windows.Forms.ColorDialog for color selection
- Board state can be converted to text representation for AI analysis via BoardConverter

## Development Patterns

- Follow existing WPF patterns established in MainWindow (not strict MVVM)
- Maintain separation between game logic (Models), AI (AI/), Services, and UI (Views)
- AI difficulty configuration should maintain backward compatibility with fallback behavior
- Use existing chess notation and coordinate systems (0-7 for both row/col)
- Registry-based settings should include error handling for access failures
- Color system requires both preview rectangles and live board updates