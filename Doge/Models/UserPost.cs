using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Doge.Models
{
    //table will be created by ef, no need to make a DbSet for this
    public class UserPost
    {
        public string UserId { get; set; }
        public DogeUser DogeUser { get; set; }
        public int PostId { get; set; }
        public DogePost DogePost { get; set; }
    }
}
