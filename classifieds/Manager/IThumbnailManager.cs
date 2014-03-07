using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace classy.Manager
{
    public interface IThumbnailManager
    {
        Stream CreateThumbnail(string originKey, int width, int height);
    }
}