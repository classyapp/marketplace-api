using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace Classy.DotNet.Mvc.SitemapGenerator
{
  public static class GZip
  {
    #region Public Static Properties

    public static readonly int BufferSize = 1048576;

    #endregion

    #region Public Static Methods

    /// <summary>
    /// Compresses the file.
    /// </summary>
    /// <param name="filePath">File to compress.</param>
    public static void CompressFile(string filePath)
    {
      string compressedFilePath = Path.GetTempFileName();
      using (FileStream compressedFileStream = new FileStream(compressedFilePath, FileMode.Append, FileAccess.Write, FileShare.Write))
      {
        GZipStream gzipStream = new GZipStream(compressedFileStream, CompressionMode.Compress);
        using (FileStream uncompressedFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
          int offset = 0;
          while (true)
          {
            byte[] buffer = new byte[offset + BufferSize];
            int bytesRead = uncompressedFileStream.Read(buffer, offset, BufferSize);
            if (bytesRead == 0)
              break;

            gzipStream.Write(buffer, offset, bytesRead);
            offset += bytesRead;
          }
        }
        gzipStream.Close();
      }
      //Copy the compressed file over the top of the uncompressed file.
      File.Delete(filePath);
      File.Move(compressedFilePath, filePath);
    }

    /// <summary>
    /// Decompresses the file.
    /// </summary>
    /// <param name="filePath">File to decompress.</param>
    public static void DecompressFile(string filePath)
    {
      string uncompressedFilePath = Path.GetTempFileName();
      using (FileStream compressedFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
      {
        GZipStream gzipStream = new GZipStream(compressedFileStream, CompressionMode.Decompress);      
        using (FileStream uncompressedFileStream = new FileStream(uncompressedFilePath, FileMode.Append, FileAccess.Write, FileShare.Write))
        {
          int offset = 0;
          while (true)
          {
            byte[] buffer = new byte[offset + BufferSize];
            int bytesRead = gzipStream.Read(buffer, offset, BufferSize);
            if (bytesRead == 0)
              break;

            uncompressedFileStream.Write(buffer, offset, bytesRead);
            offset += bytesRead;
          }
        }
        gzipStream.Close();
      }
      //Copy the uncompressed file over the top of the compressed file.
      File.Delete(filePath);
      File.Move(uncompressedFilePath, filePath);
    }

    #endregion
  }
}
