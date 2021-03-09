using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

namespace BlazorUtilityComponents.Navigation
{
  /// <summary>
  ///
  /// </summary>
  /// <remarks>
  /// see: https://chrissainty.com/working-with-query-strings-in-blazor/
  /// </remarks>
  public static class NavigationManagerExtensions
  {
    public static bool TryGetQueryString<T>(this NavigationManager navManager, string key, out T value)
    {
      var uri = navManager.ToAbsoluteUri(navManager.Uri);

      if (QueryHelpers.ParseQuery(uri.Query).TryGetValue(key, out var valueFromQueryString))
      {
        if (typeof(T) == typeof(int) && int.TryParse(valueFromQueryString, out var valueAsInt))
        {
          value = (T)(object)valueAsInt;
          return true;
        }

        if (typeof(T) == typeof(string))
        {
          value = (T)(object)valueFromQueryString.ToString();
          return true;
        }

        if (typeof(T) == typeof(decimal) && decimal.TryParse(valueFromQueryString, out var valueAsDecimal))
        {
          value = (T)(object)valueAsDecimal;
          return true;
        }
      }

      value = default;
      return false;
    }

    public static string QueryString(this NavigationManager navManager)
    {
      string result = null;
      if (Uri.TryCreate(navManager.Uri, UriKind.RelativeOrAbsolute, out var uri))
      {
        result = System.Text.RegularExpressions.Regex.Replace(uri.Query, @"^\?", "");
      }
      return result;
    }
  }
}
