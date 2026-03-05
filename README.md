# Radicle for Windows

A Windows toolset for the [Radicle](https://radicle.xyz/) peer-to-peer code collaboration stack, providing both a graphical (WPF) and command-line interface, plus a VS Code extension.

## Components

### 1. Core Library (`src/RadicleWindows.Core`)

Shared .NET 8 library that wraps the Radicle CLI (`rad`) with a strongly-typed API.

**Key features:**
- `IRadicleService` – high-level interface for all Radicle operations
- Models for profiles, repositories, issues, patches, and node status
- `IProcessExecutor` – abstraction for testable process execution
- Parsing utilities for CLI output

### 2. WPF Application (`src/RadicleWindows.Wpf`)

Windows desktop GUI for managing Radicle repositories, built with .NET 8 and WPF.

**Features:**
- Dashboard with repository, issue, and patch counts
- Repository browser
- Issue and patch management
- Node start/stop/status controls
- Identity viewer
- Dark theme with Radicle-inspired colour palette

### 3. Console Application (`src/RadicleWindows.Console`)

CLI tool that wraps the `rad` commands with a Windows-friendly interface.

**Usage:**
```
radicle-windows <command> [options]

Commands:
  gui               Launch the WPF graphical interface
  self              Show your Radicle identity
  auth              Authenticate / create identity
  ls                List local repositories
  init              Initialize a new Radicle repository
  clone <RID>       Clone a repository by Radicle ID
  sync              Synchronize with the network
  issue list        List issues
  issue open        Create a new issue
  patch list        List patches
  patch open        Submit a new patch
  node status       Show node status
  node start        Start the Radicle node
  node stop         Stop the Radicle node
  -- <args>         Pass-through to the rad CLI
```

### 4. VS Code Extension (`vscode-extension/`)

Extension for Visual Studio Code that integrates Radicle commands directly into the editor.

**Features:**
- Command palette commands for all Radicle operations
- Sidebar tree views for issues and patches
- Status bar indicator showing Radicle node status
- Input prompts for creating issues, patches, and repositories

## Prerequisites

- [Radicle CLI](https://radicle.xyz/) (`rad`) installed and available on PATH
- .NET 8 SDK (for the WPF / Console apps)
- Node.js 18+ (for the VS Code extension)

## Building

### .NET Solution

```bash
dotnet build RadicleWindows.slnx
```

### VS Code Extension

```bash
cd vscode-extension
npm install
npm run compile
```

## Testing

### .NET Tests

```bash
dotnet test
```

### VS Code Extension Tests

```bash
cd vscode-extension
npx mocha
```

## Project Structure

```
├── RadicleWindows.slnx              # .NET solution file
├── src/
│   ├── RadicleWindows.Core/         # Shared library
│   │   ├── Interfaces/              # IRadicleService, IProcessExecutor
│   │   ├── Models/                  # Data models
│   │   └── Services/                # RadicleService, ProcessExecutor
│   ├── RadicleWindows.Wpf/          # WPF desktop application
│   └── RadicleWindows.Console/      # Console CLI application
├── tests/
│   └── RadicleWindows.Core.Tests/   # xUnit tests for Core library
└── vscode-extension/                # VS Code extension
    └── src/
        ├── extension.ts             # Extension entry point
        ├── radCli.ts                # Radicle CLI wrapper
        ├── treeProviders.ts         # Issue & Patch tree views
        ├── nodeStatusBar.ts         # Status bar integration
        └── test/                    # Mocha tests
```

## License

MIT
