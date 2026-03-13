<img src="https://raw.githubusercontent.com/sboulema/CodeNav/main/art/Logo_90x90.png" />

# CodeNav 
Visual Studio extension to show the code structure of your current document

[![Sponsor](https://img.shields.io/badge/-Sponsor-fafbfc?logo=GitHub%20Sponsors)](https://github.com/sponsors/sboulema)

## Features
- Quickly see all the important methods and properties in your document
- Never get lost during a refactoring of a super long document
- Clicking on an item in the list will take you to that location in the document
- Show/Dock as a separate tool window.
- Cursor position will be reflected by highlighting the current method in the list
- Light/Dark theme support
- Collapse/Expand all ranges
- History/edit indicators
- Sort by file order or by name 
- Regions
- Filter/search items by name
- Hide, ignore or change opacity of items based on item kind (method, property), access (public, private), empty state
- Bookmarks

### Features broken in v10
- Synced collapsing/expanding ranges [VSExtensibility - #545 - Feature request: Text Editor: Collapse/Expand ranges](https://github.com/microsoft/VSExtensibility/issues/545)

### Features dropped in v10
- Top Margin
- Toggle visibility by double-clicking the splitter bar
- Show as an editor margin (left side / right side / top side / hidden) 
- Visual studio 2017, 2019 support
- Customizable fonts

## Supported Visual Studio versions

- Visual Studio 2022 (Untested)
- Visual Studio 2026

## Language support
- C#

### Languages dropped in v10
- Visual Basic
- JavaScript
- CSS

## Installing
[Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=SamirBoulema.CodeNav2026) [![Visual Studio Marketplace](https://img.shields.io/vscode-marketplace/v/SamirBoulema.CodeNav2026.svg?style=flat)](https://marketplace.visualstudio.com/items?itemName=SamirBoulema.CodeNav2026)

[GitHub Releases](https://github.com/sboulema/CodeNav/releases)

[Open VSIX Gallery](https://www.vsixgallery.com/extension/CodeNav.dcdbcca4-3a88-432f-ba04-eb4a4cb64437)

## Usage
The CodeNav tool window can be opened via the entry in the `Extensions` menu.

## Screenshots
![Preview](https://raw.githubusercontent.com/sboulema/CodeNav/main/art/Screenshot-light.png) ![Preview-Dark](https://raw.githubusercontent.com/sboulema/CodeNav/main/art/Screenshot-dark.png)

#### Filter window
![Filters](https://raw.githubusercontent.com/sboulema/CodeNav/main/art/Filters.png) 

#### Options window
![Settings](https://raw.githubusercontent.com/sboulema/CodeNav/main/art/Settings.png)

#### Bookmarks
![Bookmarks](https://raw.githubusercontent.com/sboulema/CodeNav/main/art/Bookmarks.png)

## Documentation
Looking for more info? Read the full [Documentation](https://github.com/sboulema/CodeNav/blob/main/docs/index.md) with links, guides, troubleshooting, and tips. 