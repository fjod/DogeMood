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

        //one image for one post
        //one-to-one relation
        [ForeignKey("DogeImage")]
        public int DogeImageFK { get; set; }        
        public DogeImage DogeImage { get; set; }


        //each user can have multiple favorite posts
        //each post can have multiple users that favorited it
        //many-to-many relation
        public ICollection<UserPost> Users { get; set; }
    }
}
