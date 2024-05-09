using Cashing.Data;
using Cashing.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;

namespace Cashing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class productController : ControllerBase
    {
        private readonly AppDbContext _AppDbContext;
        private readonly IMemoryCache _MemoryCache;
        private readonly ILogger<productController> _Logger;
        private readonly string cashingId = "ProductCashing";

        public productController(
            AppDbContext appDbContext,
            IMemoryCache memoryCache,
            ILogger<productController> logger
            )
        {
            _AppDbContext = appDbContext;
            _MemoryCache = memoryCache;
            _Logger = logger;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> Get()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            if (_MemoryCache.TryGetValue(cashingId, out IEnumerable<Product> products))
            {
                _Logger.LogInformation("product found in cash");
            }
            else
            {
                _Logger.LogInformation("product not found in cash");

                products = await _AppDbContext.products.ToListAsync();
                var cashOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(45))
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(45))
                    .SetPriority(CacheItemPriority.Normal);
                _MemoryCache.Set(cashingId, products, cashOptions);
            }
            stopwatch.Stop();
            _Logger.Log(LogLevel.Information, $"Time is {stopwatch.ElapsedMilliseconds}");

            return Ok(products);
        }

        [HttpGet("Clear")]
        public IActionResult Clear()
        {
            _MemoryCache.Remove(cashingId);
            _Logger.LogInformation("Cash Cleared");

            return Ok();
        }

        [HttpPost("Add")]
        public async Task<IActionResult> Add(string name, string des)
        {
            await _AppDbContext.AddAsync(new Product()
            {
                name = name,
                description = des
            });

            return Ok(await _AppDbContext.SaveChangesAsync());
        }
    }
}