using CodeNav.Extensions;
using CodeNav.Constants;
using Microsoft.VisualStudio.Extensibility;

namespace CodeNav.Mappers;

public static class IconMapper
{
    public static ImageMoniker MapMoniker(CodeItemKindEnum kind, CodeItemAccessEnum access)
    {
        var accessString = access.GetEnumDescription();

        string monikerString = kind switch
        {
            CodeItemKindEnum.Namespace => $"Namespace{accessString}",
            CodeItemKindEnum.Class => $"Class{accessString}",
            CodeItemKindEnum.Constant => $"Constant{accessString}",
            CodeItemKindEnum.Delegate => $"Delegate{accessString}",
            CodeItemKindEnum.Enum => $"Enumeration{accessString}",
            CodeItemKindEnum.EnumMember => $"EnumerationItem{accessString}",
            CodeItemKindEnum.Event => $"Event{accessString}",
            CodeItemKindEnum.Interface => $"Interface{accessString}",
            CodeItemKindEnum.Constructor or CodeItemKindEnum.Method => $"Method{accessString}",
            CodeItemKindEnum.Property or CodeItemKindEnum.Indexer => $"Property{accessString}",
            CodeItemKindEnum.Struct or CodeItemKindEnum.Record => $"Structure{accessString}",
            CodeItemKindEnum.Variable => $"Field{accessString}",
            CodeItemKindEnum.Switch => "FlowSwitch",
            CodeItemKindEnum.SwitchSection => "FlowDecision",
            _ => $"Property{accessString}",
        };

        var monikers = typeof(ImageMoniker.KnownValues).GetProperties();

        var imageMoniker = monikers.FirstOrDefault(m => monikerString.Equals(m.Name))?.GetValue(null, null);

        if (imageMoniker != null)
        {
            return (ImageMoniker)imageMoniker;
        }

        return ImageMoniker.KnownValues.QuestionMark;
    }
}
