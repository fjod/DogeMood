﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Doge.Models
{
   

    public class DogeUser : IdentityUser
    {
        

        public ICollection<UserPost> FavoritePosts { get; set; }
    }
}
