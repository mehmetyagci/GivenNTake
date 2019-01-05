using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GivenNTake.Data;
using GivenNTake.Model;
using GivenNTake.Model.DTO;
using GivenNTake.Model.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace GivenNTake.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly GiveNTakeContext _context;
        private readonly ILogger<ProductController> _logger;
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
        
        public ProductController(GiveNTakeContext context, ILogger<ProductController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet]
        [HttpGet("all")]
        public async Task<ActionResult<ProductDTO[]>> GetProducts()
        {
            var products = await _context.Products
                 .Include(p => p.Owner)
                 .Include(p => p.City)
                 .Include(p => p.Category)
                 .ThenInclude(c => c.ParentCategory)
                 .ToListAsync();
            return _productMapper.Map<ProductDTO[]>(products);
        }

        //[HttpGet("searchcategory/{category}/{subcategory=all}")]
        //public string[] SearchByProduct(string category, string subcategory, string location = "all", bool imageOnly = false)
        //{
        //    return new[]
        //    {
        //        $"Category: {category}, Subcategory: {subcategory}, Location: {location}, Only with Images: {imageOnly}"
        //    };
        //}

        [Authorize]
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
            City city = _context.Cities.FirstOrDefault(c=> c.Name == newProduct.City);
            if(city == null)
            {
                return new BadRequestObjectResult("The provided city doesnt exist");
            }

            var user = await _context.Users.FindAsync(User.Identity.Name);
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

        [AllowAnonymous]
        [HttpGet("searchcategory/{category}/{subcategory=all}/")]
        public async Task<ActionResult<ProductDTO[]>> SearchByCategory(string category, string subcategory, string location = "all",
            bool imageOnly = false)
        {
            if (string.IsNullOrEmpty(category))
            {                
                _logger.LogWarning("An empty category was sent from the client. SubCategory: '{SubCategory}', Location: '{Location}'", subcategory, location);
                return BadRequest();
            }

            IQueryable<Product> productsQuery = _context.Products
                .Include(p => p.Owner)
                .Include(p => p.City)
                .Include(p => p.Media)
                .Include(p => p.Category)
                .ThenInclude(c => c.ParentCategory);

            if (location != "all")
            {
                productsQuery = productsQuery.Where(p => p.City.Name == location);
            }
            if (subcategory != "all")
            {
                productsQuery = productsQuery.Where(p => p.Category.Name == subcategory)
                    .Where(p => p.Category.ParentCategory.Name == category);
            }
            else
            {
                productsQuery = productsQuery.Where(p => p.Category.Name == category || p.Category.ParentCategory.Name == category);
            }
            var products = await productsQuery.ToListAsync();

            return Ok(_productMapper.Map<ProductDTO[]>(products));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("categories")]
        public async Task<ActionResult> AddCategory([FromBody] NewCategoryDTO newCategory)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var category = await _context.Categories
                .Include(c => c.Subcategories)
                .SingleOrDefaultAsync(c =>
                    c.Name == newCategory.CategoryName &&
                    c.ParentCategory == null);

            if(!string.IsNullOrEmpty(newCategory.SubcategoryName)) // adding subcategory to existing category
            {
                if(category == null)
                {
                    return NotFound(new SerializableError() { { nameof(newCategory.CategoryName), "Category not found" } });
                }

                if(category.Subcategories.Any(c => c.Name == newCategory.SubcategoryName)) // subcategory already exist
                {
                    return Ok();
                }
                _context.Categories.Add(new Category() { Name = newCategory.SubcategoryName, ParentCategory = category });
            } 
            else if(category == null) // Adding a new parent category
            {
                _context.Categories.Add(new Category() { Name = newCategory.CategoryName });
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [Authorize(Policy = "ExperiencedUser")]
        [HttpPost("categories2")]
        public Task<ActionResult> AddCategory2([FromBody] NewCategoryDTO newCategory)
        {
            return AddCategory(newCategory);
        }
    }
}