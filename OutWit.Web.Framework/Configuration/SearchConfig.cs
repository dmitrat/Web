using OutWit.Common.Abstract;
using OutWit.Common.Attributes;
using OutWit.Common.Values;

namespace OutWit.Web.Framework.Configuration;

/// <summary>
/// Search configuration.
/// </summary>
public class SearchConfig : ModelBase
{
    #region Model Base

    public override bool Is(ModelBase modelBase, double tolerance = 1E-07)
    {
        if (modelBase is not SearchConfig other)
            return false;
        
        return Enabled.Is(other.Enabled)
            && Placeholder.Is(other.Placeholder);
    }

    public override SearchConfig Clone()
    {
        return new SearchConfig
        {
            Enabled = Enabled,
            Placeholder = Placeholder
        };
    }

    #endregion

    #region Properties

    [ToString]
    public bool Enabled { get; set; } = true;
    
    [ToString]
    public string Placeholder { get; set; } = "Search...";

    #endregion
  
}