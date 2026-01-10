using OutWit.Common.Enums;

namespace OutWit.Web.Framework.Models;

public sealed class ThemeMode : StringEnum<ThemeMode>
{

 #region Static Constants

 public static ThemeMode Dark = new("dark");

 public static ThemeMode Light = new("light");

 #endregion

 #region Constructors

 private ThemeMode(string value)
  : base(value)
 {

 }

 #endregion
}