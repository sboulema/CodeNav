# CodeNav v10

Version 10 of the CodeNav extension is one of the biggest rewrites done in the history of the extension.

We switched from the VSSDK extension framework to the newer VisualStudio.Extensibility framework. This allows for out-of-process
extensions which run separately from the Visual Studio process. This ensures that the extension does not have a negative impact
on the Visual Studio performance.

With the switch in framework also came a switch from .NET Framework to .NET 8.0. Finally we can use recent code features while
developing the extension.

Last but not least everything is written with async programming in mind.

## Features dropped in v10
- Top Margin
- Toggle visibility by double-clicking the splitter bar
- Show as an editor margin (left side / right side / top side / hidden) 
- Visual studio 2017, 2019 support

## Languages dropped in v10
- Visual Basic
- JavaScript
- CSS