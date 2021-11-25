using CodeNav.Helpers;
using CodeNav.Mappers;
using CodeNav.Models;
using ExCSS;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Windows.Media;
using TextSpan = Microsoft.CodeAnalysis.Text.TextSpan;
using Colors = System.Windows.Media.Colors;

namespace CodeNav.Languages.CSS.Mappers
{
    public static class BaseMapper
    {
        public static T MapBase<T>(Rule member, string id, ICodeViewUserControl control) where T : CodeItem
        {
            var element = Activator.CreateInstance<T>();

            var name = string.IsNullOrEmpty(id) ? "anonymous" : id;
            var startPos = member.StylesheetText.Range.Start.Position;
            var endPos = member.StylesheetText.Range.End.Position;

            element.Name = name;
            element.FullName = name;
            element.Id = name;
            element.Tooltip = name;
            element.StartLine = member.StylesheetText.Range.Start.Line - 1;
            element.StartLinePosition = new LinePosition(member.StylesheetText.Range.Start.Line - 1, 0);
            element.EndLine = member.StylesheetText.Range.End.Line - 1;
            element.EndLinePosition = new LinePosition(member.StylesheetText.Range.End.Line - 1, 0);
            element.Span = new TextSpan(startPos, endPos - startPos);
            element.ForegroundColor = Colors.Black;
            element.Access = CodeItemAccessEnum.Public;
            element.FontSize = SettingsHelper.Font.SizeInPoints;
            element.ParameterFontSize = SettingsHelper.Font.SizeInPoints - 1;
            element.FontFamily = new FontFamily(SettingsHelper.Font.FontFamily.Name);
            element.FontStyle = FontStyleMapper.Map(SettingsHelper.Font.Style);
            element.Control = control;
            element.FilePath = control.CodeDocumentViewModel.FilePath;

            return element;
        }
    }
}
