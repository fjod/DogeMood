using Doge.Areas.User.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            byte[] ret = target.ToByteArray(ImageFormat.Jpeg);
            target.Dispose();
            return ret;
        }
    }

    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this String st)
        {
            return String.IsNullOrEmpty(st);
        }
    }

    public static class LogExtensions
    {
        public static Doge.Areas.Admin.Controllers.LogEntry Convert(this Serilog.Events.LogEvent reader)
        {
            Doge.Areas.Admin.Controllers.LogEntry lt = new Doge.Areas.Admin.Controllers.LogEntry();
            lt.t = reader.Timestamp;
            lt.SourceContext = reader.MessageTemplate.Text;
            lt.mt = reader.Level.ToString();
            return lt;
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

    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }
        public List<DogePostForUser> Posts;

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            this.AddRange(items);
        }

        public bool HasPreviousPage
        {
            get
            {
                return (PageIndex > 1);
            }
        }

        public bool HasNextPage
        {
            get
            {
                return (PageIndex < TotalPages);
            }
        }

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }

}
