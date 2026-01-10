using Microsoft.AspNetCore.Components;
using OutWit.Common.MVVM.Blazor.ViewModels;

namespace OutWit.Web.Framework.ViewModels.Common;

public class TagListViewModel : ViewModelBase
{
    #region Functions

    protected string GetVariantClass() => Variant switch
    {
        "filled" => "tag--filled",
        _ => "tag--outline"
    };

    #endregion
    
    #region Parameters

    [Parameter]
    public List<string>? Tags { get; set; }
    
    [Parameter]
    public int MaxTags { get; set; } = 10;
    
    [Parameter]
    public bool ShowOverflow { get; set; } = true;
    
    /// <summary>
    /// Tag style variant: outline, filled
    /// </summary>
    [Parameter]
    public string Variant { get; set; } = "outline";
    
    [Parameter]
    public string? CssClass { get; set; }

    #endregion

}