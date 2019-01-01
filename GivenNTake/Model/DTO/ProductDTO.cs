using GivenNTake.Model.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GivenNTake.Model.DTOs
{
    public class ProductDTO
    {
        public int Id { get; set; }
        public OwnerDTO Owner { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Subcategory { get; set; }
        public CityDTO City { get; set; }
        public MediaDTO[] Media { get; set; }

    }
}
