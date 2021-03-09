using System;

namespace METSv2.Web.Application
{
  /// <summary>
  ///
  /// </summary>
  /// <remarks>
  /// See: https://www.meziantou.net/bind-parameters-from-the-query-string-in-blazor.htm
  /// </remarks>
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
  public class QueryStringParameterAttribute : Attribute
  {
    public QueryStringParameterAttribute()
    {
    }

    public QueryStringParameterAttribute(string name)
    {
      Name = name;
    }

    /// <summary>Name of the query string parameter. It uses the property name by default.</summary>
    public string Name { get; }
  }
}
