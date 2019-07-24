using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Doge.Models
{
    public class DogeImage
    {
        public byte[] Image { get; set; }

        public byte[] Pictogram { get; set; }

        [Key]
        public int Id { get; set; }

        public DogePost Post { get; set; }
    }
}
