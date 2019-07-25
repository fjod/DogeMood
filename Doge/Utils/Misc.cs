using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Doge.Utils
{
    public static class ImageExtensions
    {
        public static byte[] ToByteArray(this Image image, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format);
                return ms.ToArray();
            }
        }

        public static byte[] ToThumbnail(this Bitmap image)
        {
            float ratio = image.Width / image.Height;
            SizeF newSize = new SizeF(200, 200 * ratio);
            Bitmap target = new Bitmap((int)newSize.Width, (int)newSize.Height);
          
            using (Graphics graphics = Graphics.FromImage(target))
            {
                graphics.CompositingQuality = CompositingQuality.HighSpeed;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.DrawImage(image, 0, 0, newSize.Width, newSize.Height);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    target.Save(memoryStream, ImageFormat.Jpeg);
                }
            }

            return target.ToByteArray(ImageFormat.Jpeg);
        }
    }

    
}
