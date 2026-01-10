using OutWit.Common.Abstract;
using OutWit.Common.Attributes;
using OutWit.Common.Collections;
using OutWit.Common.Values;
using OutWit.Web.Framework.Models;

namespace OutWit.Web.Framework.Configuration;

/// <summary>
/// Contact form configuration.
/// </summary>
public class ContactConfig : ModelBase
{
    #region Model Base

    public override bool Is(ModelBase modelBase, double tolerance = 1E-07)
    {
        if (modelBase is not ContactConfig other)
            return false;

        return ApiUrl.Is(other.ApiUrl)
               && FormId.Is(other.FormId)
               && TurnstileSiteKey.Is(other.TurnstileSiteKey)
               && MessageTypes.Is(other.MessageTypes);
    }

    public override ContactConfig Clone()
    {
        return new ContactConfig
        {
            ApiUrl = ApiUrl,
            FormId = FormId,
            TurnstileSiteKey = TurnstileSiteKey,
            MessageTypes = MessageTypes?.Select(option => option.Clone()).ToList()
        };
    }

    #endregion

    #region Properties

    [ToString]
    public string ApiUrl { get; set; } = string.Empty;
    
    [ToString]
    public string FormId { get; set; } = string.Empty;
    
    public string TurnstileSiteKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional list of message types for dropdown.
    /// If empty or null, no dropdown is shown.
    /// </summary>
    public List<MessageTypeOption>? MessageTypes { get; set; }

    #endregion


}