using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace NonWPF.Data
{
    public static class Base64Utils
    {
        public static BitmapImage ConvertFrom(string base64)
        {
            byte[] bytes = Convert.FromBase64String(base64);
            BitmapImage image = new();
            image.BeginInit();
            image.StreamSource = new MemoryStream(bytes);
            image.EndInit();
            return image;
        }

        public static string ConvertTo(BitmapImage image)
        {
            byte[] bytes;
            using (MemoryStream stream = new())
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(stream);
                bytes = stream.ToArray();
            }
            return Convert.ToBase64String(bytes);
        }
    }
}
