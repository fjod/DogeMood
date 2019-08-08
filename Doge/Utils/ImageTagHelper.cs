using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Doge
{

    [HtmlTargetElement("img")]
    public class ImageTagHelper : TagHelper
    {
        private bool RemoteFileExists(string url)
        {
            try
            {
                //Creating the HttpWebRequest
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                //Setting the Request method HEAD, you can also use GET too.
                request.Method = "HEAD";
                //Getting the Web Response.
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                //Returns TRUE if the Status code == 200
               
                bool ret = response.StatusCode == HttpStatusCode.OK;
                response.Close();
                return (ret);
            }
            catch
            {
                //Any exception will returns false.
                return false;
            }
        }

        public Doge.Models.DogeSmallImage Image { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            string imgsrc = "";
           
            void usedDbImageIfPossible()
            {
                if (Image.DogeBigImage != null)
                {
                    if (Image.DogeBigImage.Image != null)
                    {
                        var b64 = Convert.ToBase64String(Image.DogeBigImage.Image);
                        imgsrc = string.Format("data:image/jpg;base64,{0}", b64);
                    }
                }
            }

            if (Image.URL == null)
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

            output.Attributes.SetAttribute("src", imgsrc);
              
        }
    }
}
