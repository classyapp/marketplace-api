using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace Classy.DotNet.Mvc.SitemapGenerator
{
  [Serializable]
  public abstract class BaseSitemapIndexGenerator
  {
    #region Public Properties

    /// <summary>
    /// Sitemap index filename, default is sitemap.xml.
    /// </summary>
    public string SitemapIndexFileName
    {
      get { return _sitemapIndexFileName; }
      set 
      {
        ArgumentHelper.AssertNotEmptyAndNotNull(value, "value");
        _sitemapIndexFileName = value; 
      }
    }

    /// <summary>
    /// Sitemap filename format used for each file, default is sitemap{0}.xml.
    /// </summary>
    public string SitemapFileNameFormat
    {
      get { return _sitemapFileNameFormat; }
      set
      {
        ArgumentHelper.AssertNotEmptyAndNotNull(value, "value");
        _sitemapFileNameFormat = value;
      }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// This will build an xml sitemap index and sitemaps for better index with search engines.
    /// See http://en.wikipedia.org/wiki/Sitemaps for more information.
    /// </summary>
    /// <param name="rootUrl">Root url for sitemap (i.e. http://www.intuitiveshopping.com)</param>
    /// <param name="directoryLocation">Directory location to put all the files.</param>
    public void Generate(string rootUrl, string directoryLocation)
    {
      ArgumentHelper.AssertNotEmptyAndNotNull(rootUrl, "rootUrl");
      ArgumentHelper.AssertNotEmptyAndNotNull(directoryLocation, "directoryLocation");

      _rootUrl = rootUrl.TrimEnd('/', '\\');
      _directoryLocation = directoryLocation.TrimEnd('/', '\\');
      _indexWriter = new XmlTextWriter(Path.Combine(directoryLocation, _sitemapIndexFileName), Encoding.UTF8);
      _indexWriter.Formatting = Formatting.Indented;
      _indexWriter.WriteStartDocument();
      _indexWriter.WriteStartElement("sitemapindex");
      _indexWriter.WriteAttributeString("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9");
      _indexWriter.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
      _indexWriter.WriteAttributeString("xsi:schemaLocation", "http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/siteindex.xsd");

      GenerateUrlNodes();

      CloseCurrentSitemapWriter();
      _indexWriter.WriteEndElement();
      _indexWriter.Close();
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
      if ((_urlLocationCount >= MaximumUrlLocations) || ((_sitemapWriter != null) && (_sitemapWriter.BaseStream.Length >= MaximumFileSize)))
        CloseCurrentSitemapWriter();
      if (_sitemapWriter == null)
        CreateNewSitemapWriter();

      _sitemapWriter.WriteStartElement("url");
      _sitemapWriter.WriteElementString("loc", String.Format("{0}/{1}", _rootUrl, url.TrimStart('/')));
      _sitemapWriter.WriteElementString("changefreq", updateFrequency);
      _sitemapWriter.WriteElementString("lastmod", lastUpdated.ToString(DateFormat));
      _sitemapWriter.WriteEndElement();
      ++_urlLocationCount;
    }

    #endregion

    #region Private Members

    private const string DateFormat = @"yyyy-MM-dd";
    private const int MaximumUrlLocations = 50000;
    private const long MaximumFileSize = 10000000;
    private string _rootUrl = "";
    private string _directoryLocation = "";
    private XmlTextWriter _indexWriter = null;
    private XmlTextWriter _sitemapWriter = null;
    private string _sitemapIndexFileName = "sitemap.xml";
    private string _sitemapFileNameFormat = "sitemap{0}.xml";
    private int _sitemapCount = 0;
    private int _urlLocationCount = 0;

    private string GetCurrentSitemapFileLocation()
    {
      return Path.Combine(_directoryLocation, String.Format(_sitemapFileNameFormat, _sitemapCount));
    }

    private void CreateNewSitemapWriter()
    {
      _urlLocationCount = 0;
      ++_sitemapCount;
      _sitemapWriter = new XmlTextWriter(GetCurrentSitemapFileLocation(), Encoding.UTF8);
      _sitemapWriter.Formatting = Formatting.Indented;
      _sitemapWriter.WriteStartDocument();
      _sitemapWriter.WriteStartElement("urlset");
      _sitemapWriter.WriteAttributeString("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9");
      _sitemapWriter.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
      _sitemapWriter.WriteAttributeString("xsi:schemaLocation", "http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd");
    }

    private void CloseCurrentSitemapWriter()
    {
      //Close up the current sitemap and create a new one.
      _sitemapWriter.WriteEndElement();
      _sitemapWriter.Close();
      _sitemapWriter = null;
      string fileLocation = GetCurrentSitemapFileLocation();
      string compressedFileLocation = String.Format("{0}.gz", fileLocation);
      File.Delete(compressedFileLocation);
      File.Move(fileLocation, compressedFileLocation);
      GZip.CompressFile(compressedFileLocation);
      WriteSitemapLocation(Path.GetFileName(compressedFileLocation), DateTime.Now);
    }

    /// <summary>
    /// Writes the sitemap location to the writer.
    /// </summary>
    /// <param name="urlPath">Url of sitemap (don't put root url information in).</param>
    /// <param name="lastUpdated">Date last updated.</param>
    private void WriteSitemapLocation(string url, DateTime lastUpdated)
    {
      _indexWriter.WriteStartElement("sitemap");
      _indexWriter.WriteElementString("loc", String.Format("{0}/{1}", _rootUrl, url));
      _indexWriter.WriteElementString("lastmod", lastUpdated.ToString(DateFormat));
      _indexWriter.WriteEndElement();
    }

    #endregion
  }
}
