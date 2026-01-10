using OutWit.Common.Abstract;
using OutWit.Common.Attributes;
using OutWit.Common.Values;

namespace OutWit.Web.Framework.Models;

public class ContactResponse : ModelBase
{
    #region Model Base

    public override bool Is(ModelBase modelBase, double tolerance = 1E-07)
    {
        if (modelBase is not ContactResponse other)
            return false;
        
        return Success.Is(other.Success)
            && Error.Is(other.Error);
    }

    public override ContactResponse Clone()
    {
        return new ContactResponse
        {
            Success = Success,
            Error = Error
        };
    }

    #endregion

    #region Properties

    [ToString]
    public bool Success { get; set; }
    
    [ToString]
    public string? Error { get; set; }

    #endregion

}