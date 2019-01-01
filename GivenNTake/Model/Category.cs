using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GivenNTake.Model
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IList<Category> Subcategories { get; set; }
        public Category ParentCategory { get; set; }
    }
}
