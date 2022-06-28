using System.Collections.ObjectModel;

namespace ImageToPaletteGenerator
{
    public static class ExtensionLists
    {
        public static readonly ReadOnlyCollection<string> ImageExtensions =
            new ReadOnlyCollection<string>(new[] { ".jpg", ".png", ".jpeg", ".bmp" });
        
        public static readonly ReadOnlyCollection<string> VideoExtensions =
            new ReadOnlyCollection<string>(new[] { ".flv", ".mpg", ".mpeg", ".mp4" });
    }
}