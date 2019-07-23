using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Doge.Models
{
    public class FavoriteDoges
    {
        [Key]
        public int Id { get; set; }

        public int PostId { get; set; }
        [ForeignKey("PostId")]
        public virtual DogePost DogePost { get; set; }


        public string DogeUserId { get; set; }       
        [ForeignKey("DogeUserId")]
        public virtual DogeUser DogeUser { get; set; }
    }
}
