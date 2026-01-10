using OutWit.Common.Abstract;
using OutWit.Common.Attributes;
using OutWit.Common.Values;

namespace OutWit.Web.Framework.Models;

/// <summary>
/// Message type option for contact form dropdown.
/// </summary>
public class MessageTypeOption : ModelBase
{
    #region Model Base

    public override bool Is(ModelBase modelBase, double tolerance = 1E-07)
    {
        if(modelBase is not MessageTypeOption other)
            return false;

        return Value.Is(other.Value)
               && Label.Is(other.Label);
    }

    public override MessageTypeOption Clone()
    {
        return new MessageTypeOption
        {
            Value = Value,
            Label = Label
        };
    }

    #endregion

    #region Properties

    public string Value { get; set; } = string.Empty;

    [ToString]
    public string Label { get; set; } = string.Empty;

    #endregion

}