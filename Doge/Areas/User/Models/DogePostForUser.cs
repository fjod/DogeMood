using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Doge.Areas.User.Models
{
    public class DogePostForUser
    {
        public Doge.Models.DogePost Post { get; set; }
        public bool WasFavorited { get; set; }
        public bool WasLiked { get; set; }
    }

  

}
