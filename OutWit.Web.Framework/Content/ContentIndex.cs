using OutWit.Common.Abstract;
using OutWit.Common.Collections;

namespace OutWit.Web.Framework.Content;

/// <summary>
/// Index of all content files, loaded from content/index.json
/// </summary>
public class ContentIndex : ModelBase
{
    #region Model Base

    public override bool Is(ModelBase modelBase, double tolerance = 1E-07)
    {
        if (modelBase is not ContentIndex other)
            return false;
        
        return Blog.Is(other.Blog)
            && Projects.Is(other.Projects)
            && Docs.Is(other.Docs)
            && Articles.Is(other.Articles)
            && Features.Is(other.Features);
    }

    public override ContentIndex Clone()
    {
        return new ContentIndex
        {
            Blog = Blog.ToList(),
            Projects = Projects.ToList(),
            Docs = Docs.ToList(),
            Articles = Articles.ToList(),
            Features = Features.ToList()
        };
    }

    #endregion

    #region Properties

    public List<string> Blog { get; set; } = new();
    public List<string> Projects { get; set; } = new();
    public List<string> Docs { get; set; } = new();
    public List<string> Articles { get; set; } = new();
    public List<string> Features { get; set; } = new();

    #endregion

}