# Transmittal

![Revit Version](https://img.shields.io/badge/Revit%20Version-2021_--_2024-blue.svg) ![.NET](https://img.shields.io/badge/.NET-4.8-blue.svg) <br>
![Revit Version](https://img.shields.io/badge/Revit%20Version-2025_--_2026-blue.svg) ![.NET](https://img.shields.io/badge/.NET-8-blue.svg) <br>
![Revit Version](https://img.shields.io/badge/Revit%20Version-2027-blue.svg) ![.NET](https://img.shields.io/badge/.NET-10-blue.svg)

![GitHub last commit](https://img.shields.io/github/last-commit/russgreen/transmittal) ![License](https://img.shields.io/badge/License-GPL%20v3-blue.svg)

Transmittal is a comprehensive document management solution for Autodesk® Revit® that provides a wizard interface to assist with setting revisions, managing drawing sheet status, and publishing selected sheets to PDF, DWF, and DWG formats. The solution also includes a standalone desktop application for managing transmittals outside of Revit and can record transmittal history in a SQLite database with reporting capabilities.

> **Note for Revit 2021 users**: To use Transmittal with Revit 2021 the freeware PDF24 printer must be installed.  This is not required for Revit 2022 and later. The PDF24 printer can be downloaded from https://download.pdf24.org/pdf24-creator-11.11.1-x64.msi

![Screenshot 2022-06-06 064221](https://user-images.githubusercontent.com/1886088/172102241-c7e597ad-ac73-45c0-ad63-7f65f5f0eddb.png)

<a href="https://russgreen.github.io/Transmittal/"><img src="https://img.shields.io/badge/-READ%20MORE-blue" /></a>

## Table of Contents

- [Features](#-features)
- [Requirements](#-requirements)
- [Solution Architecture](#️-solution-architecture)
- [Development Setup](#️-development-setup)
- [Building the Solution](#-building-the-solution)
- [Installation](#-installation)
- [Documentation](#-documentation)
- [Testing](#-testing)
- [Version Management](#-version-management)
- [Licensing](#️-licensing)
- [Acknowledgments](#-acknowledgments)
- [Support](#-support)

## Features

### Revit Add-in
- **Sheet Management**: Set revisions and status of drawing sheets
- **Multi-format Export**: Publish sheets to PDF, DWF, and DWG formats
- **ISO 19650 Compliance**: Support for ISO 19650 naming conventions and metadata
- **Custom Parameters**: Extensive support for custom project and sheet parameters
- **Batch Processing**: Process multiple sheets efficiently
- **File Naming**: Flexible file and folder naming with customizable templates

### Standalone Desktop Application
- **Universal Document Management**: Handle any document type, not just Revit sheets
- **Database Integration**: SQLite database for transmittal history tracking
- **Reporting**: Generate transmittal reports and historic document issue sheets
- **File Parsing**: Automatic filename parsing for standardized naming conventions
- **Drag & Drop**: Easy document addition via drag and drop interface

## Requirements

### For Revit 2021-2024
- **Operating System**: Windows 10/11 (64-bit)
- **Framework**: .NET Framework 4.8
- **Revit**: Autodesk Revit 2021, 2022, 2023, or 2024
- **PDF Support**: PDF24 printer (freeware) required for Revit 2021 only
  - Download: https://download.pdf24.org/pdf24-creator-11.11.1-x64.msi

### For Revit 2025-2026
- **Operating System**: Windows 10/11 (64-bit)
- **Framework**: .NET 8
- **Revit**: Autodesk Revit 2025 or 2026

### For Desktop Application
- **Operating System**: Windows 10/11 (64-bit)
- **Framework**: .NET 8
- **Database**: SQLite (embedded)

## Solution Architecture

The solution consists of multiple projects targeting different .NET frameworks and use cases:

```
Transmittal/
├── source/
│   ├── Transmittal/                      # Revit Add-in (.NET Framework 4.8 / .NET 8)
│   ├── Transmittal.Desktop/              # Standalone WPF Application (.NET 8)
│   ├── Transmittal.Library/              # Shared Core Library (Multi-target)
│   ├── Transmittal.Reports/              # Report Generation (.NET 8)
│   └── Transmittal.Library.Tests/        # Unit Tests (.NET 8)
├── build/                                # NUKE Build System
├── docs/                                 # Documentation (GitHub Pages)
├── Installer/                            # Advanced Installer Project
└── Directory.Build.props                 # Shared MSBuild Properties
```


### Project Dependencies

#### Transmittal (Revit Add-in)
- **UI Framework**: WPF with Syncfusion controls
- **MVVM**: CommunityToolkit.MVVM
- **Revit APIs**: Nice3point.Revit.Api packages
- **Logging**: Serilog
- **IoC**: Microsoft.Extensions.Hosting
- **Assembly Merging**: ILRepack
- **Analytics**: Transmittal.Analytics.Client

#### Transmittal.Desktop (Standalone)
- **UI Framework**: WPF with Syncfusion controls
- **MVVM**: CommunityToolkit.MVVM
- **Dialogs**: Ookii.Dialogs.Wpf
- **Logging**: Serilog
- **IoC**: Microsoft.Extensions.Hosting
- **Analytics**: Transmittal.Analytics.Client

#### Transmittal.Library (Core)
- **Database**: Microsoft.Data.Sqlite with Dapper ORM
- **HTTP Client**: System.Net.Http
- **JSON**: System.Text.Json
- **Utilities**: Humanizer.Core
- **IoC**: Microsoft.Extensions.Hosting

## Development Setup

### Project Configurations

The solution uses multiple build configurations for different Revit versions:

#### Revit Add-in Configurations
- **Debug/Release R21**: Revit 2021 (.NET Framework 4.8)
- **Debug/Release R22**: Revit 2022 (.NET Framework 4.8)
- **Debug/Release R23**: Revit 2023 (.NET Framework 4.8)
- **Debug/Release R24**: Revit 2024 (.NET Framework 4.8)
- **Debug/Release R25**: Revit 2025 (.NET 8)
- **Debug/Release R26**: Revit 2026 (.NET 8)

## Building the Solution

### Automated Build (NUKE)
The solution uses [NUKE](https://nuke.build/) for automated builds:

### Build Targets
- **Clean**: Remove build artifacts
- **Restore**: Restore NuGet packages
- **Compile**: Build all projects
- **Test**: Run unit tests
- **Sign**: Code sign assemblies (requires certificates)
- **Installer**: Create MSI installer using Advanced Installer

## Installation

### End Users
1. Download the latest release from [GitHub Releases](https://github.com/russgreen/Transmittal/releases)
2. Run the MSI installer
3. Restart Revit to load the add-in

## Documentation

Comprehensive documentation is available at: https://russgreen.github.io/Transmittal/

### Key Topics
- [Getting Started](https://russgreen.github.io/Transmittal/)
- [Settings Configuration](https://russgreen.github.io/Transmittal/settings/)
- [File Naming Tags](https://russgreen.github.io/Transmittal/settings/tags/)
- [Standalone Transmittal](https://russgreen.github.io/Transmittal/standalonetransmittal/)
- [Report Generation](https://russgreen.github.io/Transmittal/reports/)
- [Database Management](https://russgreen.github.io/Transmittal/settings/databaseupdate/)

## Testing

### Test Projects
- **Transmittal.Library.Tests**: Unit tests for core library functionality
  - Database operations
  - File naming and parsing
  - Model validation
  - Extension methods

## Version Management

Version numbering follows semantic versioning (Major.Minor.Patch):

- **Major**: Breaking changes or significant new features
- **Minor**: New features maintaining backwards compatibility  
- **Patch**: Bug fixes and minor improvements

Versions are managed through `Directory.Build.props`:
- `VersionPrefix`: Major.Minor.Patch (e.g., "3.2.3")
- `VersionSuffix`: Pre-release identifier (e.g., "beta", "alpha")

## Licensing

This project is licensed under the **GNU General Public License v3.0**.

### Third-Party Components
The solution includes the following open-source libraries:
- **CommunityToolkit.MVVM** (MIT License)
- **Dapper** (Apache 2.0 License)
- **Humanizer** (MIT License)
- **Microsoft.Extensions.Hosting** (MIT License)
- **Serilog** (Apache 2.0 License)
- **SQLite** (Public Domain)

Commercial components:
- **Syncfusion WPF Controls** (Commercial/Community License Required)

## Acknowledgments

- **Autodesk** for the Revit API
- **Nice3point** for the excellent Revit API packages
- **Syncfusion** for the WPF UI controls

## Support

- **Documentation**: https://russgreen.github.io/Transmittal/
- **Issues**: [GitHub Issues](https://github.com/russgreen/Transmittal/issues)
- **Discussions**: [GitHub Discussions](https://github.com/russgreen/Transmittal/discussions)
- **Releases**: [GitHub Releases](https://github.com/russgreen/Transmittal/releases)

---

**Copyright © 2024 Russell Green**  
Licensed under GPL v3.0 - see [LICENSE](LICENSE) for details.
