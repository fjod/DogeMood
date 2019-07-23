using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Doge.Models
{
    public class DogePost
    {
        [Key]
        public int Id { get; set; }

        public int UpVotes { get; set; }
        public DateTime AddDate { get; set; }
        public bool IsApproved { get; set; }

       
        public int ImageId { get; set; }
        [ForeignKey("ImageId")]
        public virtual DogeImage DogeImage { get; set; }
    }
}
