using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GiveNTake.Model
{
    public class NewProductDTO
    {
        [Required]
        [MinLength(3)]
        [MaxLength(50)]
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Subcategory { get; set; }
        public string City { get; set; }
    }
}
