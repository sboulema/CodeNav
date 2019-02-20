using CodeNav.Models;
using CodeNav.Properties;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Linq;
using System.Windows.Media;
using Zu.TypeScript.TsTypes;
using TextSpan = Microsoft.CodeAnalysis.Text.TextSpan;

namespace CodeNav.Mappers.JavaScript
{
    public static class BaseMapperJS
    {
        public static T MapBase<T>(Node member, string id, CodeViewUserControl control) where T : CodeItem
        {
            var element = Activator.CreateInstance<T>();

            var name = string.IsNullOrEmpty(id) ? "anonymous" : id;

            element.Name = name;
            element.FullName = name;
            element.Id = name;
            element.Tooltip = name;
            element.StartLine = GetLineNumber(member, member.NodeStart);
            element.StartLinePosition = new LinePosition(GetLineNumber(member, member.NodeStart), 0);
            element.EndLine = GetLineNumber(member, member.End);
            element.EndLinePosition = new LinePosition(GetLineNumber(member, member.End), 0);
            element.Span = new TextSpan(member.First.Pos.GetValueOrDefault(0), member.Last.End.GetValueOrDefault(0));
            element.ForegroundColor = Colors.Black;
            element.Access = CodeItemAccessEnum.Public;
            element.FontSize = Settings.Default.Font.SizeInPoints;
            element.ParameterFontSize = Settings.Default.Font.SizeInPoints - 1;
            element.FontFamily = new FontFamily(Settings.Default.Font.FontFamily.Name);
            element.FontStyle = FontStyleMapper.Map(Settings.Default.Font.Style);
            element.Control = control;

            return element;
        }

        private static int GetLineNumber(Node member, int? pos)
        {
            return member.SourceStr.Take(pos.GetValueOrDefault(0)).Count(c => c == '\n');
        }
    }
}
