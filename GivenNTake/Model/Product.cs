using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GivenNTake.Model
{
    public class Product
    {
        // Primary key
        public int Id { get; set; }

        // Value properties
        public User Owner { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        // Navigation properties - represents relationships
        public Category Category { get; set; }
        public City City { get; set; }
        public IList<ProductMedia> Media { get; set; }
        public DateTime PublishDate { get; set; }

    }
}
