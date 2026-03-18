using CodeNav.OutOfProc.Constants;
using CodeNav.OutOfProc.Mappers;
using Microsoft.VisualStudio.Extensibility.UI;
using System.Runtime.Serialization;

namespace CodeNav.OutOfProc.ViewModels;

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
            IsOpacityEnabled = FilterRuleMapper.MapBasedOnHideIgnore(Hide, Ignore);
            IsItalicEnabled = FilterRuleMapper.MapBasedOnHideIgnore(Hide, Ignore);
            IsFontScaleEnabled = FilterRuleMapper.MapBasedOnHideIgnore(Hide, Ignore);
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
    public bool? IsEmpty { get; set; } = false;

    private bool _hide;

    [DataMember]
    public bool Hide
    {
        get => _hide;
        set
        {
            SetProperty(ref _hide, value);
            IsOpacityEnabled = FilterRuleMapper.MapBasedOnHideIgnore(Hide, Ignore);
            IsItalicEnabled = FilterRuleMapper.MapBasedOnHideIgnore(Hide, Ignore);
            IsFontScaleEnabled = FilterRuleMapper.MapBasedOnHideIgnore(Hide, Ignore);
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
            IsOpacityEnabled = FilterRuleMapper.MapBasedOnHideIgnore(Hide, Ignore);
            IsItalicEnabled = FilterRuleMapper.MapBasedOnHideIgnore(Hide, Ignore);
            IsFontScaleEnabled = FilterRuleMapper.MapBasedOnHideIgnore(Hide, Ignore);
        }
    }

    private int _opacity = 100;

    [DataMember]
    public int Opacity
    {
        get => _opacity;
        set => SetProperty(ref _opacity, value);
    }

    private bool _italic;

    [DataMember]
    public bool Italic
    {
        get => _italic;
        set => SetProperty(ref _italic, value);
    }

    private int _fontScale = 100;

    [DataMember]
    public int FontScale
    {
        get => _fontScale;
        set => SetProperty(ref _fontScale, value);
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

    private bool _isItalicEnabled;

    /// <summary>
    /// Indicator if the Italic action filter is enabled for this rule
    /// </summary>
    [DataMember]
    public bool IsItalicEnabled
    {
        get => _isItalicEnabled;
        set => SetProperty(ref _isItalicEnabled, value);
    }

    private bool _isFontScaleEnabled;

    /// <summary>
    /// Indicator if the Font Scale action filter is enabled for this rule
    /// </summary>
    [DataMember]
    public bool IsFontScaleEnabled
    {
        get => _isFontScaleEnabled;
        set => SetProperty(ref _isFontScaleEnabled, value);
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
        .GetValues<CodeItemKindEnum>()
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
