<img src="https://raw.githubusercontent.com/sboulema/CodeNav/main/art/Logo_90x90.png" />

# CodeNav 
Visual Studio extension to show the code structure of your current document

[![Sponsor](https://img.shields.io/badge/-Sponsor-fafbfc?logo=GitHub%20Sponsors)](https://github.com/sponsors/sboulema)

## Features
- Quickly see all the important methods and properties in your document
- Never get lost during a refactoring of a super long document
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
- Clicking on an item in the list will take you to that location in the document [VSExtensibility - #554 - Feature request: Text Editor: Scroll to line](https://github.com/microsoft/VSExtensibility/issues/554)

### Features dropped in v10
- Top Margin
- Toggle visibility by double-clicking the splitter bar
- Show as an editor margin (left side / right side / top side / hidden) 
- Visual studio 2017, 2019 support
- Customizable fonts

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

## Screenshots
![Preview](https://raw.githubusercontent.com/sboulema/CodeNav/main/art/Screenshot-light.png) ![Preview-Dark](https://raw.githubusercontent.com/sboulema/CodeNav/main/art/Screenshot-dark.png)

#### Filter window
![Filters](https://raw.githubusercontent.com/sboulema/CodeNav/main/art/Filters.png) 

#### Options window
![Settings](https://raw.githubusercontent.com/sboulema/CodeNav/main/art/Settings.png)

#### Bookmarks
![Bookmarks](https://raw.githubusercontent.com/sboulema/CodeNav/main/art/Bookmarks.png)

## Links

[DataTriggers in a XAML ItemsControl](https://github.com/microsoft/VSExtensibility/issues/456)

[Splitting up the tool window XAML in resource dictionaries](https://github.com/MicrosoftDocs/visualstudio-docs/blob/main/docs/extensibility/visualstudio.extensibility/inside-the-sdk/advanced-remote-ui.md#user-xaml-resource-dictionaries)

[Getting a clientContext in an AsyncCommand bound to a XAML Command property](https://github.com/microsoft/VSExtensibility/issues/520)

[Settings Sample](https://github.com/microsoft/VSExtensibility/blob/2f74457eb241552cd725c8ca544fd99797ef546e/New_Extensibility_Model/Samples/SettingsSample/README.md)

[Settings Sample 2](https://github.com/MicrosoftDocs/visualstudio-docs/blob/c70d685e945ff6cea8dd0f7bbb54f780ef67170e/docs/extensibility/visualstudio.extensibility/settings/settings.md?plain=1#L201)

[Fix having multiple projects](https://github.com/microsoft/VSExtensibility/issues/388)

[Making a WPF TextBox binding fire on each new character](https://stackoverflow.com/questions/10619596/making-a-wpf-textbox-binding-fire-on-each-new-character)

[Bind width of a textbox inside VisualBrush](https://stackoverflow.com/questions/30670911/bind-width-of-a-textbox-inside-visualbrush)

[How to add a vertical Separator](https://stackoverflow.com/questions/13584998/how-to-add-a-vertical-separator)

[Change color of Button when Mouse is over](https://stackoverflow.com/questions/20073294/change-color-of-button-when-mouse-is-over)

[Solving class dependencies](https://github.com/dotnet/roslyn/issues/23878)

[How to access the currently active Project](https://github.com/microsoft/VSExtensibility/issues/411)

[ListView with a GridView](https://wpf-tutorial.com/listview-control/listview-with-gridview/)

[Header not showing up in WPF ListView](https://stackoverflow.com/questions/1238249/header-not-showing-up-in-wpf-listview)

[Make WPF ComboBoxes fill a whole column width](https://stackoverflow.com/questions/826985/make-wpf-comboboxes-fill-a-whole-column-width)

[How Do I Set Up Grid Lines for my ListView?](https://matthiasshapiro.com/how-do-i-set-up-grid-lines-for-my-listview/)

[Styling the ListView Column Header](https://matthiasshapiro.com/styling-the-listview-column-header/)

[DataGrid in ToolWindow cannot be set to editmode](https://github.com/microsoft/VSExtensibility/issues/389)

[Setting WPF border visible or not depending on ScrollViewer's VerticalScrollBarVisibility property](https://stackoverflow.com/questions/73199311/setting-wpf-border-visible-or-not-depending-on-scrollviewers-verticalscrollbarv/73199480#73199480)