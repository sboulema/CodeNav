using CodeNav.Models;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using System;
using System.ComponentModel;
using System.Linq;

namespace CodeNav.Mappers
{
    public static class IconMapper
    {
        public static ImageMoniker MapMoniker(CodeItemKindEnum kind, CodeItemAccessEnum access)
        {
            string monikerString;
            var accessString = GetEnumDescription(access);

            switch (kind)
            {
                case CodeItemKindEnum.Class:
                    monikerString = $"Class{accessString}";
                    break;
                case CodeItemKindEnum.Constant:
                    monikerString = $"Constant{accessString}";
                    break;
                case CodeItemKindEnum.Delegate:
                    monikerString = $"Delegate{accessString}";
                    break;
                case CodeItemKindEnum.Enum:
                    monikerString = $"Enumeration{accessString}";
                    break;
                case CodeItemKindEnum.EnumMember:
                    monikerString = $"EnumerationItem{accessString}";
                    break;
                case CodeItemKindEnum.Event:
                    monikerString = $"Event{accessString}";
                    break;
                case CodeItemKindEnum.Interface:
                    monikerString = $"Interface{accessString}";
                    break;
                case CodeItemKindEnum.Constructor:
                case CodeItemKindEnum.Method:
                    monikerString = $"Method{accessString}";
                    break;
                case CodeItemKindEnum.Property:
                    monikerString = $"Property{accessString}";
                    break;
                case CodeItemKindEnum.Struct:
                    monikerString = $"Structure{accessString}";
                    break;
                case CodeItemKindEnum.Variable:
                    monikerString = $"Field{accessString}";
                    break;
                case CodeItemKindEnum.Switch:
                    monikerString = "FlowSwitch";
                    break;
                case CodeItemKindEnum.SwitchSection:
                    monikerString = "FlowDecision";
                    break;
                default:
                    monikerString = $"Property{accessString}";
                    break;
            }

            var monikers = typeof(KnownMonikers).GetProperties();

            var imageMoniker = monikers.FirstOrDefault(m => monikerString.Equals(m.Name))?.GetValue(null, null);
            if (imageMoniker != null)
            {
                return (ImageMoniker)imageMoniker;
            }

            return KnownMonikers.QuestionMark;
        }

        private static string GetEnumDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            return attribute == null ? value.ToString() : attribute.Description;
        }
    }
}
