using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace Classy.DotNet.Mvc.SitemapGenerator
{
  public abstract class BaseSitemapGenerator
  {
    #region Public Methods

    /// <summary>
    /// This will build an xml sitemap for better index with search engines.
    /// See http://en.wikipedia.org/wiki/Sitemaps for more information.
    /// </summary>
    /// <param name="rootUrl">Root domain url, i.e. http://www.intuitiveshopping.com</param>
    /// <returns>Sitemap.xml as string</returns>
    public string Generate(string rootUrl)
    {
      using (MemoryStream stream = new MemoryStream())
      {
        Generate(stream, rootUrl);
        return Encoding.UTF8.GetString(stream.ToArray());
      }
    }

    /// <summary>
    /// This will build an xml sitemap for better index with search engines.
    /// See http://en.wikipedia.org/wiki/Sitemaps for more information.
    /// </summary>
    /// <param name="stream">Stream of sitemap.</param>
    /// <param name="rootUrl">Root domain url, i.e. http://www.intuitiveshopping.com</param>
    public void Generate(Stream stream, string rootUrl)
    {
      _rootUrl = rootUrl.TrimEnd('/', '\\');
      _writer = new XmlTextWriter(stream, Encoding.UTF8);
      _writer.Formatting = Formatting.Indented;
      _writer.WriteStartDocument();
      _writer.WriteStartElement("urlset");
      _writer.WriteAttributeString("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9");
      _writer.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
      _writer.WriteAttributeString("xsi:schemaLocation", "http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd");

      WriteUrlLocation("", UpdateFrequency.Weekly, DateTime.Now);
      GenerateUrlNodes();
      
      _writer.WriteEndElement();
      _writer.Close();
    }

    #endregion

    #region Protected Members

    /// <summary>
    /// Method that is overridden, that handles creation of child urls.
    /// Use the method WriteUrlLocation() within this method.
    /// </summary>
    protected abstract void GenerateUrlNodes();

    /// <summary>
    /// Writes the url location to the writer.
    /// </summary>
    /// <param name="urlPath">Url of indexed location (don't put root url information in).</param>
    /// <param name="frequency">Update frequency - always, hourly, daily, weekly, yearly, never.</param>
    /// <param name="lastUpdated">Date last updated.</param>
    protected void WriteUrlLocation(string url, string updateFrequency, DateTime lastUpdated)
    {
      _writer.WriteStartElement("url");
      _writer.WriteElementString("loc", String.Format("{0}/{1}", _rootUrl, url));
      _writer.WriteElementString("changefreq", updateFrequency);
      _writer.WriteElementString("lastmod", lastUpdated.ToString(DateFormat));
      _writer.WriteEndElement();
    }

    #endregion

    #region Private Members

    private const string DateFormat = @"yyyy-MM-dd";
    private XmlTextWriter _writer = null;
    private string _rootUrl = "";

    #endregion
  }
}
