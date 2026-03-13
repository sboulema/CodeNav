using CodeNav.Constants;
using CodeNav.Mappers;
using Microsoft.VisualStudio.Extensibility.UI;
using System.Runtime.Serialization;

namespace CodeNav.ViewModels;

[DataContract]
public class FilterRuleViewModel : NotifyPropertyChangedObject
{
    private CodeItemKindEnum _kind = CodeItemKindEnum.All;

    [DataMember]
    public CodeItemKindEnum Kind
    {
        get => _kind;
        set
        {
            SetProperty(ref _kind, value);
            AccessArray = FilterRuleMapper.MapAccess(value);
            IsAccessEnabled = AccessArray.Length > 1;
            IsEmptyEnabled = FilterRuleMapper.MapEmpty(value);
            IsHideEnabled = FilterRuleMapper.MapHide(value);
            IsIgnoreEnabled = FilterRuleMapper.MapIgnore(value);
            IsOpacityEnabled = FilterRuleMapper.MapOpacity(Hide, Ignore);
        }
    }

    private CodeItemAccessEnum _access = CodeItemAccessEnum.All;

    [DataMember]
    public CodeItemAccessEnum Access
    {
        get => _access;
        set => SetProperty(ref _access, value);
    }

    [DataMember]
    public bool IsEmpty { get; set; }

    private bool _hide;

    [DataMember]
    public bool Hide
    {
        get => _hide;
        set
        {
            SetProperty(ref _hide, value);
            IsOpacityEnabled = FilterRuleMapper.MapOpacity(Hide, Ignore);
        }
    }

    private bool _ignore;

    [DataMember]
    public bool Ignore
    {
        get => _ignore;
        set
        {
            SetProperty(ref _ignore, value);
            IsOpacityEnabled = FilterRuleMapper.MapOpacity(Hide, Ignore);
        }
    }

    private int _opacity = 100;

    [DataMember]
    public int Opacity
    {
        get => _opacity;
        set => SetProperty(ref _opacity, value);
    }

    #region IsEnabled

    private bool _isAccessEnabled;

    /// <summary>
    /// Indicator if the Access modifier filter is enabled for this rule
    /// </summary>
    [DataMember]
    public bool IsAccessEnabled
    {
        get => _isAccessEnabled;
        set => SetProperty(ref _isAccessEnabled, value);
    }

    private bool _isEmptyEnabled;

    /// <summary>
    /// Indicator if the Empty modifier filter is enabled for this rule
    /// </summary>
    [DataMember]
    public bool IsEmptyEnabled
    {
        get => _isEmptyEnabled;
        set => SetProperty(ref _isEmptyEnabled, value);
    }

    private bool _isIgnoreEnabled;

    /// <summary>
    /// Indicator if the Ignore action filter is enabled for this rule
    /// </summary>
    [DataMember]
    public bool IsIgnoreEnabled
    {
        get => _isIgnoreEnabled;
        set => SetProperty(ref _isIgnoreEnabled, value);
    }

    private bool _isHideEnabled;

    /// <summary>
    /// Indicator if the Hide action filter is enabled for this rule
    /// </summary>
    [DataMember]
    public bool IsHideEnabled
    {
        get => _isHideEnabled;
        set => SetProperty(ref _isHideEnabled, value);
    }

    private bool _isOpacityEnabled;

    /// <summary>
    /// Indicator if the Opacity action filter is enabled for this rule
    /// </summary>
    [DataMember]
    public bool IsOpacityEnabled
    {
        get => _isOpacityEnabled;
        set => SetProperty(ref _isOpacityEnabled, value);
    }

    #endregion

    #region ComboBox ItemsSources

    private CodeItemAccessEnum[] _accessArray = [];

    /// <summary>
    /// List of possible filter rule access values
    /// </summary>
    [DataMember]
    public CodeItemAccessEnum[] AccessArray
    {
        get => _accessArray;
        set => SetProperty(ref _accessArray, value);
    }

    private CodeItemKindEnum[] _kindArray = [.. Enum
        .GetValues(typeof(CodeItemKindEnum))
        .Cast<CodeItemKindEnum>()
        .Except([CodeItemKindEnum.Unknown])];

    /// <summary>
    /// List of possible filter rule kind values
    /// </summary>
    [DataMember]
    public CodeItemKindEnum[] KindArray
    {
        get => _kindArray;
        set => SetProperty(ref _kindArray, value);
    }

    #endregion
}
