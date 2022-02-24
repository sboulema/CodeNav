#nullable enable

#nullable enable

using CodeNav.Helpers;
using CodeNav.Mappers;
using CodeNav.Models;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Linq;
using System.Windows.Media;
using Zu.TypeScript.TsTypes;
using TextSpan = Microsoft.CodeAnalysis.Text.TextSpan;

namespace CodeNav.Languages.JS.Mappers
{
    public static class BaseMapperJS
    {
        public static T MapBase<T>(Node member, string id, ICodeViewUserControl? control) where T : CodeItem
        {
            var element = Activator.CreateInstance<T>();

            var name = string.IsNullOrEmpty(id) ? "anonymous" : id;

            element.Name = name;
            element.FullName = name;
            element.Id = name;
            element.Tooltip = name;
            element.StartLine = GetLineNumber(member, member.NodeStart);
            element.StartLinePosition = new LinePosition(GetLineNumber(member, member.NodeStart) - 1, 0);
            element.EndLine = GetLineNumber(member, member.End);
            element.EndLinePosition = new LinePosition(GetLineNumber(member, member.End), 0);
            element.Span = new TextSpan(member.NodeStart, member.End.GetValueOrDefault() - member.NodeStart);
            element.ForegroundColor = Colors.Black;
            element.Access = CodeItemAccessEnum.Public;
            element.FontSize = SettingsHelper.Font.SizeInPoints;
            element.ParameterFontSize = SettingsHelper.Font.SizeInPoints - 1;
            element.FontFamily = new FontFamily(SettingsHelper.Font.FontFamily.Name);
            element.FontStyle = FontStyleMapper.Map(SettingsHelper.Font.Style);
            element.Control = control;
            element.FilePath = control?.CodeDocumentViewModel.FilePath ?? string.Empty;

            return element;
        }

        private static int GetLineNumber(Node member, int? pos)
        {
            return member.SourceStr.Take(pos.GetValueOrDefault(0)).Count(c => c == '\n') + 1;
        }
    }
}
