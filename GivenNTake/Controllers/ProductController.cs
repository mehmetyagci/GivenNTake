using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GivenNTake.Data;
using GivenNTake.Model;
using GivenNTake.Model.DTO;
using GivenNTake.Model.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GivenNTake.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private static readonly IMapper _productMapper;

        static ProductController()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Product, ProductDTO>()
                    .ForMember(dto => dto.City, opt => opt.MapFrom(product => product.City))
                    .ForMember(dto => dto.Category, opt => opt.MapFrom(product => product.Category.ParentCategory.Name))
                    .ForMember(dto => dto.Subcategory, opt => opt.MapFrom(product => product.Category.Name));

                cfg.CreateMap<User, OwnerDTO>()
                    .ForMember(dto => dto.Id, opt => opt.MapFrom(product => product.Id));

                cfg.CreateMap<City, CityDTO>()
                    .ForMember(dto => dto.Id, opt => opt.MapFrom(city => city.Id));

                cfg.CreateMap<ProductMedia, MediaDTO>()
                    .ForMember(dto => dto.Url, opt => opt.MapFrom(media => media.Url));

                cfg.CreateMap<Category, CategoryDTO>();
                cfg.CreateMap<Category, SubCategoryDTO>();

            });
            _productMapper = config.CreateMapper();
        }

        private readonly GiveNTakeContext _context;
        public ProductController(GiveNTakeContext context)
        {
            _context = context;
        }

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
        public string[] SearchByProduct(string category, string subcategory, string location = "all", bool imageOnly = false)
        {
            return new[]
            {
                $"Category: {category}, Subcategory: {subcategory}, Location: {location}, Only with Images: {imageOnly}"
            };
        }

        [HttpPost("")]
        public async Task<IActionResult> AddNewProduct([FromBody] NewProductDTO newProduct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Category category = _context.Categories
                .Include(c => c.ParentCategory)
                .SingleOrDefault(c => c.Name == newProduct.Subcategory || c.ParentCategory.Name == newProduct.Category);
            if(category == null)
            {
                return new BadRequestObjectResult("The provided category and sub category doesnt exist");
            }
            City city = _context.Cities.Single(c=> c.Name == newProduct.City);
            if(city == null)
            {
                return new BadRequestObjectResult("The provided city doesnt exist");
            }

            var user = await _context.Users.FindAsync("seller1@seller.com");
            var product = new Product()
            {
                Owner = user,
                Category = category,
                Title = newProduct.Title,
                Description = newProduct.Description,
                City = city,
                PublishDate = DateTime.UtcNow
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return CreatedAtAction(
                nameof(GetProduct),
                new { id = product.Id },
                _productMapper.Map<ProductDTO>(product));
        }

        [HttpGet("search/{date:datetime}/{keyword}/")]
        public async Task<IActionResult> Search(DateTime date, string keyword)
        {
            var products = await _context.Products
                .Where(p => p.Title.Contains(keyword))
                .Where(p => p.PublishDate.Date == date.Date)
                .ToListAsync();

            // returning a response with the found products
            return null;
        }

        [HttpGet("{id}", Name = nameof(GetProduct))]
        public async Task<ActionResult<ProductDTO>> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .ThenInclude(c => c.ParentCategory)
                .SingleOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }
            return Ok(_productMapper.Map<ProductDTO>(product));
        }

        [HttpGet("search/{keyword}")]
        public async Task<ActionResult<ProductDTO[]>> SearchProducts(string keyword)
        {
            List<Product> products = await _context.Products
                .Include(p => p.Category)
                .ThenInclude(c => c.ParentCategory)
                .Where(p => p.Title.Contains(keyword))
                .ToListAsync();

            return Ok(_productMapper.Map<ProductDTO[]>(products));

        }
    }
}