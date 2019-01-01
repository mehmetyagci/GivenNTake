using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GivenNTake.Model
{
    public class Message
    {
        public int Id { get; set; }
        public string Body { get; set; }
        public Product Product { get; set; }
        public string Title { get; set; }

        public User FromUser { get; set; }
        public User ToUser { get; set; }
    }
}
