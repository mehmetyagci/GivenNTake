using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GivenNTake.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GivenNTake.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        public string[] GetProducts()
        {
            return new[]
            {
                "1 - Microwave",
                "2 - Washing Machine",
                "3 - Mirror"
            };
        }

        [HttpGet("searchcategory/{category}/{subcategory=all}")]
        public string[] SearchByProduct(string category, string subcategory, string location="all", bool imageOnly = false)
        {
            return new[]
            {
                $"Category: {category}, Subcategory: {subcategory}, Location: {location}, Only with Images: {imageOnly}"
            };
        }

        [HttpPost("")]
        public ActionResult<NewProductDTO> AddNewProduct([FromBody] NewProductDTO newProduct)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(newProduct);
        }

    }
}