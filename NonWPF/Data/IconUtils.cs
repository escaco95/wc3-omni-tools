using System;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace NonWPF.Data
{
    public static class IconUtils
    {
        public static Icon Convert(BitmapImage bitmapImage)
        {
            // Step 1: BitmapImage를 MemoryStream으로 변환
            using MemoryStream memoryStream = new MemoryStream();
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
            encoder.Save(memoryStream);

            // Step 2: MemoryStream으로부터 Bitmap 생성
            using var bitmap = new Bitmap(memoryStream);

            // Step 3: Bitmap을 Icon으로 변환
            IntPtr hIcon = bitmap.GetHicon(); // Bitmap에서 HIcon 핸들을 얻음
            Icon icon = Icon.FromHandle(hIcon);

            return icon;
        }

    }
}
