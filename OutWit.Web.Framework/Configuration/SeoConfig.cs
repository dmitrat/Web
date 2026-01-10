using OutWit.Common.Abstract;
using OutWit.Common.Values;

namespace OutWit.Web.Framework.Configuration;

/// <summary>
/// SEO configuration.
/// </summary>
public class SeoConfig : ModelBase
{
    #region Model Base

    public override bool Is(ModelBase modelBase, double tolerance = 1E-07)
    {
        if (modelBase is not SeoConfig other)
            return false;
        
        return DefaultImage.Is(other.DefaultImage)
            && TwitterHandle.Is(other.TwitterHandle)
            && FacebookAppId.Is(other.FacebookAppId);
    }

    public override SeoConfig Clone()
    {
        return new SeoConfig
        {
            DefaultImage = DefaultImage,
            TwitterHandle = TwitterHandle,
            FacebookAppId = FacebookAppId
        };
    }

    #endregion

    #region Properties

    public string DefaultImage { get; set; } = "/images/social-card.png";
    
    public string? TwitterHandle { get; set; }
    
    public string? FacebookAppId { get; set; }

    #endregion

}