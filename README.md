![Logo](https://raw.githubusercontent.com/sboulema/CodeNav/master/CodeNav/Resources/DocumentOutline_256x.png)

# CodeNav
Visual Studio extension to show the code structure of your current document

## Features
- Quickly see all the important methods and properties in your document.
- Never get lost during a refactoring of a super long document.
- Clicking on an item in the list will take you to that location in the document.
- Sort by file order or by name 
- Toggle visibility by double-clicking the splitter bar
- Dark theme support
- Show as an editor margin (left side / right side / hidden) 
- Show as a seperate toolwindow (View -> Other Windows -> CodeNav)
- Filter items by kind (method, property), access (public, private) and name
- Cursor position will be reflected by highlighting the current method in the list
- Customizable fonts
- Synced collapsing/expanding regions

## Language support
Currently only C#

## Installing
[Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=SamirBoulema.CodeNav) ![Visual Studio Marketplace](http://vsmarketplacebadge.apphb.com/version-short/SamirBoulema.CodeNav.svg)

[Github Releases](https://github.com/sboulema/CodeNav/releases)

[Open VSIX Gallery](http://vsixgallery.com/extension/CodeNav.Samir%20Boulema.19687465-dc94-413d-ad72-6141e90c94d4/)

[AppVeyor](https://ci.appveyor.com/project/sboulema/codenav) [![Build status](https://ci.appveyor.com/api/projects/status/8g968p48t2rkia16?svg=true)](https://ci.appveyor.com/project/sboulema/codenav)

## Screenshots
![Preview](https://raw.githubusercontent.com/sboulema/CodeNav/master/CodeNav/Resources/Preview.png) ![Preview-Dark](https://raw.githubusercontent.com/sboulema/CodeNav/master/CodeNav/Resources/Preview-dark.png)

#### Filter window
![Preview](https://raw.githubusercontent.com/sboulema/CodeNav/master/CodeNav/Resources/FilterWindow.png) 

#### Options window
![Preview](https://raw.githubusercontent.com/sboulema/CodeNav/master/CodeNav/Resources/OptionsWindow.png) 

#### Tool window
![Preview](https://raw.githubusercontent.com/sboulema/CodeNav/master/CodeNav/Resources/ToolWindow.png) 

## Debugging
If you experience any problems with CodeNav you can debug what is going on by downloading a debug build of CodeNav.

The debug build will write a lot of useful information to the Visual Studio Output window. Please help me and include this information when creating an issue!

![Preview](https://raw.githubusercontent.com/sboulema/CodeNav/master/CodeNav/Resources/OutputWindow.png) 

## Thanks to
- Steve Cadwallader - For his amazing [CodeMaid](https://github.com/codecadwallader/codemaid) extension
