using OutWit.Common.Abstract;
using OutWit.Common.Attributes;
using OutWit.Common.Values;

namespace OutWit.Web.Framework.Models;

/// <summary>
/// Social media link.
/// </summary>
public class SocialLink : ModelBase
{
    #region Model Base

    public override bool Is(ModelBase modelBase, double tolerance = 1E-07)
    {
        if (modelBase is not SocialLink other)
            return false;

        return Platform.Is(other.Platform)
            && Url.Is(other.Url)
            && Icon.Is(other.Icon);
    }

    public override SocialLink Clone()
    {
        return new SocialLink
        {
            Platform = Platform,
            Url = Url,
            Icon = Icon
        };
    }

    #endregion

    #region Propertes

    public string Platform { get; set; } = string.Empty;

    [ToString]
    public string Url { get; set; } = string.Empty;

    public string? Icon { get; set; }

    #endregion

}