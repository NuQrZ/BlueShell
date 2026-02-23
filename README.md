# BlueShell

BlueShell is a modern Windows desktop application built with **WinUI 3** and **.NET 8**.
It provides a configurable terminal interface with a modular command system and seamless integration with the Windows file system.

---

## Features

* Interactive terminal UI
* Modular command system
* File system and drive operations
* MVVM architecture (CommunityToolkit.Mvvm)
* Dependency Injection support
* Modern WinUI 3 interface
* Expandable command registry

### Current Commands

* `Clear`
* `ClearDisplay`
* `Exit`
* `--Version`
* `Drive`
* `Simulate`

---

## Technologies

* .NET 8
* WinUI 3
* CommunityToolkit.Mvvm
* Microsoft.Extensions.DependencyInjection
* Windows App SDK

---

## Architecture

The project follows a clean layered structure:

```
BlueShell
├── View/            # XAML UI
├── ViewModel/       # MVVM view models
├── Terminal/        # Terminal engine and commands
├── Services/        # Services (filesystem, printing…)
├── Model/           # Data models
├── Helpers/         # Utility classes
├── Converters/      # XAML converters
```

### Terminal Subsystem

The terminal is organized into several key components:

* **Abstractions** — interfaces for commands and output
* **Commands** — command implementations
* **Infrastructure** — command dispatcher and registry
* **WinUI** — terminal UI rendering

This design allows easy extension with new commands.

---

## Getting Started

### Requirements

* Windows 10/11
* Visual Studio 2022 (17.8+)
* .NET 8 SDK
* Windows App SDK workload

### Build and Run

```bash
git clone https://github.com/NuQrZ/BlueShell.git
cd BlueShell
```

Open `BlueShell.sln` in Visual Studio and run:

```
Ctrl + F5
```

---

## Usage

Type commands directly into the terminal interface, for example:

```
version
clear
drive
```

---

## Adding a New Command

To add a new command:

1. Implement `ITerminalCommand`
2. Register the command in `TerminalCommandRegistry`

### Example

```csharp
public class VersionCommand : ITerminalCommand
{
    public string CommandName => "yourcommand";

    public Task ExecuteAsync(TerminalCommandContext context, string commandLine)
    {
        // Your command logic
        return Task.CompletedTask; 
    }
}
```

---

## Key Components

* **TerminalCommandDispatcher** — parses and executes commands
* **TerminalCommandRegistry** — registers available commands
* **TerminalViewModel** — bridge between UI and terminal engine
* **FileSystemService / DriveService** — Windows file system abstraction

---

## License

This project is licensed under the MIT License.

---

## Author

**NuQrZ**
