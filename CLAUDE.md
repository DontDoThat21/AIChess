# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

AIChess is a WPF-based chess application written in C# (.NET Framework 4.8.1) featuring AI opponents with multiple difficulty levels. The project includes both basic and advanced AI implementations, with advanced levels requiring GitHub token authentication for enhanced functionality.

## Build Commands

Due to .NET Framework compatibility issues with newer dotnet CLI versions, use Visual Studio or MSBuild:

```bash
# Build in Visual Studio (recommended)
# Open AIChess.sln in Visual Studio and build

# Alternative: Use MSBuild if available
msbuild AIChess.sln /p:Configuration=Debug
msbuild AIChess.sln /p:Configuration=Release
```

Note: `dotnet build` may fail with MSBuild task host errors on this .NET Framework 4.8.1 project.

## Testing

The project includes MSTest-based unit tests in the AIChess.Tests directory:
- ChessRulesTests.cs - Chess rule validation tests  
- IntegrationTests.cs - Integration testing
- NewAIFeaturesTests.cs - AI feature testing

Run tests through Visual Studio Test Explorer or MSBuild test runner.

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

**UI** (`AIChess/`)
- `MainWindow.xaml/.cs` - Main game interface
- `Dialogs/` - Settings and pawn promotion dialogs

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

### Key Implementation Notes

- The project references some files via absolute paths from a previous "TrubChess" project - these are linked files, not duplicates
- Chess piece images are stored in `Resources/` directory
- The application uses WPF Canvas for the chess board visualization
- Move validation and game rules are implemented in individual piece classes
- GitHub token management is handled by `GitHubTokenManager.cs` for advanced AI features

### Development Patterns

- Follow existing WPF/MVVM patterns established in MainWindow
- Maintain separation between game logic (Models) and UI (Views)
- AI difficulty configuration should maintain backward compatibility
- Use existing chess notation and coordinate systems (0-7 for both row/col)