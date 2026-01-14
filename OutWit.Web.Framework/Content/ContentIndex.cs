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
               && Features.Is(other.Features)
               && Sections.Keys.Is(other.Sections.Keys)
               && Sections.Values.SelectMany(x => x).Is(other.Sections.Values.SelectMany(x => x));
    }

    public override ContentIndex Clone()
    {
        return new ContentIndex
        {
            Blog = Blog.ToList(),
            Projects = Projects.ToList(),
            Docs = Docs.ToList(),
            Articles = Articles.ToList(),
            Features = Features.ToList(),
            Sections = Sections.ToDictionary(kv => kv.Key, kv => kv.Value.ToList())
        };
    }

    #endregion

    #region Properties

    /// <summary>
    /// Blog posts (hardcoded section).
    /// </summary>
    public List<string> Blog { get; set; } = new();

    /// <summary>
    /// Project pages (hardcoded section - special behavior).
    /// </summary>
    public List<string> Projects { get; set; } = new();

    /// <summary>
    /// Documentation pages (legacy, for backward compatibility).
    /// New sections should use Sections dictionary.
    /// </summary>
    public List<string> Docs { get; set; } = new();

    /// <summary>
    /// Article pages (legacy, for backward compatibility).
    /// New sections should use Sections dictionary.
    /// </summary>
    public List<string> Articles { get; set; } = new();

    /// <summary>
    /// Feature descriptions (hardcoded - special behavior).
    /// </summary>
    public List<string> Features { get; set; } = new();

    /// <summary>
    /// Dynamic content sections defined in site.config.json.
    /// Key = folder name, Value = list of files.
    /// </summary>
    public Dictionary<string, List<string>> Sections { get; set; } = new();

    #endregion

}