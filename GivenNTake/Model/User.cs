using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GivenNTake.Model
{
    public class User
    {
        public string Id { get; set; }
        public IList<Product> Products { get; set; }
    }
}
