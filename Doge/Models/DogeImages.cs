using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Doge.Models
{
    /*
    this is the table from the DB, the class was used for migrations
    public class DogeImage
    {
        public byte[] Image { get; set; }

        public string URL { get; set; }

        public byte[] Pictogram { get; set; }

        [Key]
        public int Id { get; set; }

        public DogePost Post { get; set; }
    }*/

    [Table("Images")]
    public class DogeBigImage
    {
        public int Id { get; set; }
        public byte[] Image { get; set; }       

        public DogeSmallImage DogeSmallImage { get; set; }
    }

    [Table("Images")]
    public class DogeSmallImage
    {
        public byte[] Pictogram { get; set; }
      
        public int Id { get; set; }

        public DogeBigImage DogeBigImage { get; set; }

        public string URL { get; set; }        

        public DogePost Post { get; set; }
    }
}
