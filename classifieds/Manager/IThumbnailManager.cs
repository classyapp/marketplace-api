using System.IO;

namespace classy.Manager
{
    public interface IThumbnailManager
    {
        byte[] CreateThumbnail(string originKey, int width, int height);
        byte[] CreateCollage(string imageKeys);
    }
}