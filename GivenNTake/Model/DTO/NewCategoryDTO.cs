using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GiveNTake.Model.DTO
{
    public class NewCategoryDTO
    {
        [Required]
        public string CategoryName { get; set; }
        public string SubcategoryName { get; set; }
    }
}
