using System.ComponentModel.DataAnnotations;
using AgriSmartAPI.Data;
using AgriSmartAPI.DTO;
using AgriSmartAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgriSmartAPI.Controllers;

[ApiController]
[Route("api")]
public class MarketPlaceController : ControllerBase
{
    private readonly AgriSmartContext _context;

    public MarketPlaceController(AgriSmartContext context)
    {
        _context = context;
    }

    // POST: /api/products/add
    [HttpPost("products/add")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AddPlantProduct([FromForm] PlantProductForm form, [FromServices] IWebHostEnvironment env)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (form.ImageFile is null || form.ImageFile.Length == 0)
            return BadRequest(new { message = "Image file is required." });

        // Validate CategoryId if provided
        if (form.CategoryId.HasValue)
        {
            var categoryExists = await _context.Set<Category>()
                .AnyAsync(c => c.Id == form.CategoryId.Value);
            if (!categoryExists)
                return BadRequest(new { message = "Invalid CategoryId." });
        }

        // Basic validation: size and extension
        const long maxBytes = 10L * 1024L * 1024L; // 10 MB
        if (form.ImageFile.Length > maxBytes)
            return BadRequest(new { message = "Image too large. Max 10 MB." });

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var ext = Path.GetExtension(form.ImageFile.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext))
            return BadRequest(new { message = $"Unsupported file type. Allowed: {string.Join(", ", allowedExtensions)}" });

        // Resolve wwwroot and ensure images directory exists
        var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        var imagesDir = Path.Combine(webRoot, "images");
        if (!Directory.Exists(imagesDir))
            Directory.CreateDirectory(imagesDir);

        // Generate a unique filename and save
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var physicalPath = Path.Combine(imagesDir, fileName);
        var relativePath = Path.Combine("images", fileName).Replace("\\", "/");

        await using (var stream = System.IO.File.Create(physicalPath))
        {
            await form.ImageFile.CopyToAsync(stream);
        }

        var product = new Product
        {
            ProductName = form.ProductName.Trim(),
            Description = string.IsNullOrWhiteSpace(form.Description) ? null : form.Description.Trim(),
            CategoryId = form.CategoryId,
            PricePerKg = form.PricePerKg,
            Rating = 0,
            Location = string.IsNullOrWhiteSpace(form.Location) ? null : form.Location.Trim(),
            Supplier = string.IsNullOrWhiteSpace(form.Supplier) ? null : form.Supplier.Trim(),
            ImageUrl = "/" + relativePath // stored as a web-relative path
        };

        _context.Set<Product>().Add(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAllProducts), null, product);
    }

    // POST: /api/categories/add
    [HttpPost("categories/add")]
    public async Task<IActionResult> AddCategory([FromBody] CategoryForm form)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var category = new Category
        {
            Name = form.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(form.Description) ? null : form.Description.Trim()
        };

        _context.Set<Category>().Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAllProducts), null, category);
    }
    
    // GET: /api/categories
    [HttpGet("categories")]
    public async Task<IActionResult> GetAllCategories()
    {
        var categories = await _context.Set<Category>()
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new
            {
                id = c.Id,
                name = c.Name,
                description = c.Description
            })
            .ToListAsync();

        return Ok(categories);
    }
    
    
    // GET: /api/products
    [HttpGet("products")]
    public async Task<IActionResult> GetAllProducts()
    {
        var products = await _context.Set<Product>()
            .AsNoTracking()
            .Include(p => p.Category)
            .OrderBy(p => p.ProductName)
            .Select(p => new
            {
                id = p.Id,
                productName = p.ProductName,
                description = p.Description,
                categoryId = p.CategoryId,
                categoryName = p.Category != null ? p.Category.Name : null,
                pricePerKg = p.PricePerKg,
                rating = p.Rating,
                location = p.Location,
                supplier = p.Supplier,
                imageUrl = p.ImageUrl
            })
            .ToListAsync();

        return Ok(products);
    }

    // GET: /api/products/{category}
    [HttpGet("products/bycategory/{categoryId:int}")]
    public async Task<IActionResult> GetProductsByCategoryId([FromRoute] int categoryId)
    {
        if (categoryId <= 0)
            return BadRequest(new { message = "Valid category ID is required." });

        var products = await _context.Set<Product>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId)
            .OrderBy(p => p.ProductName)
            .Select(p => new
            {
                id = p.Id,
                productName = p.ProductName,
                description = p.Description,
                categoryId = p.CategoryId,
                categoryName = p.Category != null ? p.Category.Name : null,
                pricePerKg = p.PricePerKg,
                rating = p.Rating,
                location = p.Location,
                supplier = p.Supplier,
                imageUrl = p.ImageUrl
            })
            .ToListAsync();

        return Ok(products);
    }


    // POST: /api/cart/add
    [HttpPost("cart/add")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var product = await _context.Set<Product>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId);

        if (product is null)
            return NotFound(new { message = "Product not found." });

        var existing = await _context.Set<Cart>()
            .FirstOrDefaultAsync(c => c.UserId == request.UserId && c.ProductId == request.ProductId);

        if (existing is null)
        {
            var entity = new Cart
            {
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                UserId = request.UserId.Trim()
            };
            _context.Set<Cart>().Add(entity);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCartByUser), new { userId = entity.UserId }, new { id = entity.Id });
        }
        else
        {
            existing.Quantity += request.Quantity;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Cart updated.", id = existing.Id, quantity = existing.Quantity });
        }
    }

    // GET: /api/cart/{userId}
    [HttpGet("cart/{userId}")]
    public async Task<IActionResult> GetCartByUser([FromRoute] string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest(new { message = "UserId is required." });

        var uid = userId.Trim();
        var items = await _context.Set<Cart>()
            .AsNoTracking()
            .Include(c => c.Product)
            .Where(c => c.UserId == uid)
            .Select(c => new
            {
                cartId = c.Id,
                productId = c.ProductId,
                quantity = c.Quantity,
                userId = c.UserId,
                product = c.Product == null ? null : new
                {
                    id = c.Product.Id,
                    name = c.Product.ProductName,
                    description = c.Product.Description,
                    category = c.Product.Category,
                    pricePerKg = c.Product.PricePerKg,
                    rating = c.Product.Rating,
                    location = c.Product.Location,
                    supplier = c.Product.Supplier,
                    imageUrl = c.Product.ImageUrl
                },
                totalAmount = c.Product == null ? 0 : (c.Quantity * c.Product.PricePerKg)
            })
            .ToListAsync();

        return Ok(items);
    }
    
    [HttpPost("cart/pay")]
    public async Task<IActionResult> MakePayment([FromBody] PaymentRequest paymentRequest)
    {
        if (paymentRequest == null)
            return BadRequest(new { message = "Invalid payment data." });

        // For example, use Stripe or other payment service here.
        // This is a pseudo-implementation for payment flow.

        // Validate User and Cart
        var cartItems = await _context.Set<Cart>()
            .Include(c => c.Product)
            .Where(c => c.UserId == paymentRequest.UserId)
            .ToListAsync();

        if (!cartItems.Any())
            return BadRequest(new { message = "No items in cart to pay for." });

        var totalAmount = cartItems.Sum(ci => ci.Quantity * ci.Product.PricePerKg);

        // To keep it simple, we'll "simulate" a payment
        bool paymentSuccess = SimulatePaymentApi(paymentRequest.PaymentMethod, totalAmount);

        if (!paymentSuccess)
        {
            return StatusCode(402, new { message = "Payment failed. Please try another payment method." });
        }

        // Optionally, create an order and clear cart
        var orders = cartItems.Select(ci => new Order
        {
            UserId = ci.UserId,
            ProductId = ci.ProductId,
            Quantity = ci.Quantity,
            TotalAmount = ci.Quantity * ci.Product.PricePerKg,
            OrderDate = DateTime.UtcNow
        }).ToList();

        await _context.Set<Order>().AddRangeAsync(orders);
        _context.Set<Cart>().RemoveRange(cartItems);

        await _context.SaveChangesAsync();

        return Ok(new { message = "Payment successful. Order placed.", totalAmount });
    }

    // Dummy payment simulation method; replace with real integration.
    private bool SimulatePaymentApi(string paymentMethod, decimal amount)
    {
        // Simulates calling a payment provider API
        // Always return true for success (you can expand logic as needed)
        return !string.IsNullOrWhiteSpace(paymentMethod) && amount > 0;
    }

    public class PaymentRequest
    {
        [Required, MaxLength(100)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string PaymentMethod { get; set; } = string.Empty; // e.g., "credit_card", "paypal"
    }

    // Provide a simple Order model for demonstration.
   

    // DELETE: /api/cart/remove/{id}
    [HttpDelete("cart/remove/{id:int}")]
    public async Task<IActionResult> RemoveFromCart([FromRoute] int id)
    {
        var item = await _context.Set<Cart>().FirstOrDefaultAsync(c => c.Id == id);
        if (item is null)
            return NotFound(new { message = "Cart item not found." });

        _context.Set<Cart>().Remove(item);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Removed from cart." });
    }

    public class AddToCartRequest
    {
        [Required]
        public int ProductId { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required, MaxLength(100)]
        public string UserId { get; set; } = string.Empty;
    }

    public class PlantProductRequest
    {
        [Required, MaxLength(200)]
        public string ProductName { get; set; } = string.Empty;

        [Required, MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        [Required, Range(0.01, double.MaxValue)]
        public decimal PricePerKg { get; set; }

        [Required, MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Supplier { get; set; } = string.Empty;

        [Required]
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class CategoryForm
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    

}