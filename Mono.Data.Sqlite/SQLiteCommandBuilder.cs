/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

namespace Mono.Data.Sqlite
{
  using System;
  using System.Data.Common;
  using System.Globalization;
  using System.ComponentModel;

  /// <summary>
  /// SQLite implementation of DbCommandBuilder.
  /// </summary>
  public sealed class SqliteCommandBuilder : DbCommandBuilder
  {
    /// <summary>
    /// Default constructor
    /// </summary>
    public SqliteCommandBuilder()
    {
      QuotePrefix = "[";
      QuoteSuffix = "]";
    }

    /// <summary>
    /// Returns a valid named parameter
    /// </summary>
    /// <param name="parameterName">The name of the parameter</param>
    /// <returns>Error</returns>
    protected override string GetParameterName(string parameterName)
    {
      return String.Format(CultureInfo.InvariantCulture, "@{0}", parameterName);
    }

    /// <summary>
    /// Returns a named parameter for the given ordinal
    /// </summary>
    /// <param name="parameterOrdinal">The i of the parameter</param>
    /// <returns>Error</returns>
    protected override string GetParameterName(int parameterOrdinal)
    {
      return String.Format(CultureInfo.InvariantCulture, "@param{0}", parameterOrdinal);
    }

    /// <summary>
    /// Returns a placeholder character for the specified parameter i.
    /// </summary>
    /// <param name="parameterOrdinal">The index of the parameter to provide a placeholder for</param>
    /// <returns>Returns a named parameter</returns>
    protected override string GetParameterPlaceholder(int parameterOrdinal)
    {
      return GetParameterName(parameterOrdinal);
    }
      
    /// <summary>
    /// Overridden to hide its property from the designer
    /// </summary>
    [DefaultValue("[")]
    public override string QuotePrefix
    {
      get
      {
        return base.QuotePrefix;
      }
      set
      {
        base.QuotePrefix = value;
      }
    }

    /// <summary>
    /// Places brackets around an identifier
    /// </summary>
    /// <param name="unquotedIdentifier">The identifier to quote</param>
    /// <returns>The bracketed identifier</returns>
    public override string QuoteIdentifier(string unquotedIdentifier)
    {
      if (String.IsNullOrEmpty(QuotePrefix)
        || String.IsNullOrEmpty(QuoteSuffix)
        || String.IsNullOrEmpty(unquotedIdentifier))
        return unquotedIdentifier;

      return QuotePrefix + unquotedIdentifier.Replace(QuoteSuffix, QuoteSuffix + QuoteSuffix) + QuoteSuffix;
    }

    /// <summary>
    /// Removes brackets around an identifier
    /// </summary>
    /// <param name="quotedIdentifier">The quoted (bracketed) identifier</param>
    /// <returns>The undecorated identifier</returns>
    public override string UnquoteIdentifier(string quotedIdentifier)
    {
      if (String.IsNullOrEmpty(QuotePrefix)
        || String.IsNullOrEmpty(QuoteSuffix)
        || String.IsNullOrEmpty(quotedIdentifier))
        return quotedIdentifier;

      if (quotedIdentifier.StartsWith(QuotePrefix, StringComparison.OrdinalIgnoreCase) == false
        || quotedIdentifier.EndsWith(QuoteSuffix, StringComparison.OrdinalIgnoreCase) == false)
        return quotedIdentifier;

      return quotedIdentifier.Substring(QuotePrefix.Length, quotedIdentifier.Length - (QuotePrefix.Length + QuoteSuffix.Length)).Replace(QuoteSuffix + QuoteSuffix, QuoteSuffix);
    }
  }
}
