using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Doge
{

    [HtmlTargetElement("img")]
    public class ImageTagHelper : TagHelper
    {
        readonly IHostingEnvironment _env;
        public ImageTagHelper(IHostingEnvironment env) : base()
        {
            _env = env;
        }
        private bool RemoteFileExists(string url)
        {
            try
            {

                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "HEAD";
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                bool ret = response.StatusCode == HttpStatusCode.OK;

                response.Close();

                return (ret);
            }
            catch
            {
                return false;
            }
        }

        public Doge.Models.DogeSmallImage Image { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            string imgsrc = "";

            void usedDbImageIfPossible()
            {
                if (Image.DogeBigImage?.Image != null)
                {
                    var b64 = Convert.ToBase64String(Image.DogeBigImage.Image);
                    imgsrc = string.Format("data:image/jpg;base64,{0}", b64);
                }
            }

            try
            {

                if (Image.URL == null)//no url, so use db
                {
                    usedDbImageIfPossible();
                }
                else
                if (Image.URL.Length > 0)
                {
                    //there is some link, is it dead?
                    var goodUrl = RemoteFileExists(Image.URL);
                    if (goodUrl)
                    {
                        imgsrc = Image.URL;
                    }
                    else
                    {
                        usedDbImageIfPossible();
                    }
                }
            }
            catch 
            {

            }
            finally
            {
                if (imgsrc == "")
                {
                    //no image after all work, so provide sample

                    string webRootPath = _env.WebRootPath;
                    var imagePath = Path.Combine(webRootPath, "images\\sampleImage.jpg");
                    imgsrc = imagePath;
                }  
            }

            output.Attributes.SetAttribute("src", imgsrc);
        }
    }
}
