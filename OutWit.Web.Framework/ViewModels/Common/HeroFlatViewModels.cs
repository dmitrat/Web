using Microsoft.AspNetCore.Components;
using OutWit.Common.MVVM.Blazor.ViewModels;

namespace OutWit.Web.Framework.ViewModels.Common;

public class HeroFlatViewModels : ViewModelBase
{
    #region Properties

    protected string ProjectsAnchor => FirstProjectAnchor ?? "#projects";

    #endregion

    #region Parameters

    /// <summary>
    /// Cascaded from HomePage - anchor to first project
    /// </summary>
    [CascadingParameter(Name = "FirstProjectAnchor")]
    public string? FirstProjectAnchor { get; set; }
    
    [Parameter]
    public string Title { get; set; } = "";
    
    [Parameter]
    public string AuthorName { get; set; } = "";
    
    [Parameter]
    public string AuthorTitle { get; set; } = "";
    
    [Parameter]
    public bool ShowActions { get; set; } = true;
    
    [Parameter]
    public string ProjectsButtonText { get; set; } = "Projects";
    
    [Parameter]
    public string BlogUrl { get; set; } = "/blog";
    
    [Parameter]
    public string BlogButtonText { get; set; } = "Blog";

    #endregion
}