﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GiveNTake.Model.DTO
{
    public class CategoryDTO
    {
        public string Name { get; set; }
        public IList<SubCategoryDTO> Subcategories { get; set; }
    }
}
