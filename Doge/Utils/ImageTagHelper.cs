using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Doge
{
    [HtmlTargetElement("img")]
    public class ImageTagHelper : TagHelper
    {

        public Doge.Models.DogeSmallImage Image { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            string imgsrc = "";
            if (Image.DogeBigImage != null)
            {
                if (Image.DogeBigImage.Image == null)
                {
                    imgsrc = Image.URL;
                }
                else
                {
                    var b64 = Convert.ToBase64String(Image.DogeBigImage.Image);
                    imgsrc = string.Format("data:image/jpg;base64,{0}", b64);
                }
            }
            else
            {
                imgsrc = Image.URL;
            }

            output.Attributes.SetAttribute("src", imgsrc);
            //output.TagMode = TagMode.StartTagAndEndTag;          
        }
    }
}
