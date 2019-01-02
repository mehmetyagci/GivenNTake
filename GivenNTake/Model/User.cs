using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GivenNTake.Model
{
    public class User : IdentityUser
    {        
        public IList<Product> Products { get; set; }
    }
}
