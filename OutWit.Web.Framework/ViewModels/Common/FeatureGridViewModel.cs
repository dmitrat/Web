using Microsoft.AspNetCore.Components;
using OutWit.Common.MVVM.Blazor.ViewModels;
using OutWit.Web.Framework.Content;
using OutWit.Web.Framework.Services;

namespace OutWit.Web.Framework.ViewModels.Common;

public class FeatureGridViewModel : ViewModelBase
{
    #region Constructors

    public FeatureGridViewModel()
    {
        Features = new List<FeatureCard>();
    }
    
    #endregion
    
    #region Event Handlers

    protected override async Task OnInitializedAsync()
    {
        // Only load content if no ChildContent is provided
        if (ChildContent != null)
            return;

        List<FeatureCard> allFeatures = await ContentService.GetFeaturesAsync();
        Features = MaxItems > 0
            ? allFeatures.Take(MaxItems).ToList()
            : allFeatures;

    }

    #endregion
    
    #region Properties

    protected List<FeatureCard> Features { get; private set; }

    #endregion
    
    #region Parameters

    [Parameter]
    public string Title { get; set; } = "Features";
    
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
    
    /// <summary>
    /// Maximum number of features to display (0 = all)
    /// </summary>
    [Parameter]
    public int MaxItems { get; set; } = 0;

    #endregion

    #region Injected Dependencies

    [Inject]
    public ContentService ContentService { get; private set; } = null!;

    #endregion
    
}