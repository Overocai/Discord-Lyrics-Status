using System.IO;
using System.Windows.Media.Imaging;

namespace DiscordLyrics.Infrastructure;

public static class ImageHelper
{
    /// <summary>Decode raw image bytes into a frozen, UI-thread-safe bitmap.</summary>
    public static BitmapImage? FromBytes(byte[]? data)
    {
        if (data is null || data.Length == 0) return null;
        try
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.StreamSource = new MemoryStream(data);
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }
        catch
        {
            return null;
        }
    }
}
