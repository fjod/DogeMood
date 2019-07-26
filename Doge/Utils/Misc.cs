using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


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

    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this String st)
        {
            return String.IsNullOrEmpty(st);
        }
    }

    public class AlertController : Controller
    {
        public void Alert(string message, NotificationType notificationType)
        {
            var msg = "<script language='javascript'>swal('" + notificationType.ToString().ToUpper() + "', '" + message + "','" + notificationType + "')" + "</script>";
            TempData["notification"] = msg;
        }

    }
    public enum NotificationType
    {
            error,
            success,
            warning,
            info
    }    

  
    public static class UserRoles
    {
        public static string DogeAdmin = "DogeAdmin";
        public static string DogeUser = "DogeUser";

        
    }


}
