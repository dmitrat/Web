using OutWit.Common.Abstract;
using OutWit.Common.Collections;
using OutWit.Common.Values;
using OutWit.Web.Framework.Models;

namespace OutWit.Web.Framework.Configuration;

/// <summary>
/// Footer configuration.
/// </summary>
public class FooterConfig : ModelBase
{
    #region Model Base

    public override bool Is(ModelBase modelBase, double tolerance = 1E-07)
    {
        if (modelBase is not FooterConfig other)
            return false;

        return Copyright.Is(other.Copyright)
               && LinkGroups.Is(other.LinkGroups)
               && SocialLinks.Is(other.SocialLinks);
    }

    public override FooterConfig Clone()
    {
        return new FooterConfig
        {
            Copyright = Copyright,
            LinkGroups = LinkGroups.Select(group => group.Clone()).ToList(),
            SocialLinks = SocialLinks.Select(link => link.Clone()).ToList()
        };
    }

    #endregion

    #region Properties

    public string Copyright { get; set; } = string.Empty;
    
    public List<FooterLinkGroup> LinkGroups { get; set; } = [];
    
    public List<SocialLink> SocialLinks { get; set; } = [];

    #endregion

}