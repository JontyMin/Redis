﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Redis.Data;
using Redis.Models;
using StackExchange.Redis;

namespace Redis.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _distributedCache;

        public HomeController(ILogger<HomeController> logger,
            IConnectionMultiplexer redis,
            ApplicationDbContext context,
            IDistributedCache distributedCache)
        {
            _logger = logger;
            _redis = redis;
            _db = _redis.GetDatabase();
            _context = context;
            _distributedCache = distributedCache;
        }

        [HttpGet]
        public async  Task<IActionResult> ProductList()
        {
           var products=await  _context.Product.ToListAsync();
           var vms = products.Select(x => new ProductViewModel()
           {
               id=x.id,
               Name = x.Name,
               Url = x.Url
           }).ToList();

           //1、取出所有产品浏览量
           RedisKey[] keys = vms.Select(x => (RedisKey) $"product:{x.id}:views").ToArray();
           var viewCounts = await _db.StringGetAsync(keys);

           //2、把浏览量分配到每个vm上

           foreach (var productViewModel in vms)
           {
               var id = productViewModel.id;
               var key = $"product:{id}:views";
               var index = keys.IndexOf((RedisKey) key);
               if (index>-1)
               {
                   productViewModel.ViewCount = (int) viewCounts[index];
               }
           }
           return View(vms);
        }

        [HttpGet]
        public async Task<IActionResult> ProductDetails(Guid Id)
        {
            var key = $"product:{Id}:views";
            await _db.StringIncrementAsync(key);
            var viewCount = await _db.StringGetAsync(key);
            ViewData["viewCount"] = viewCount;

            var product = await _context.Product.FirstOrDefaultAsync(x => x.id == Id);
            if (product==null)
            {
                return NotFound();
            }
              
            var viewKey = "recentViewProducts";
            var element = $"产品：{product.Name}({product.id})";
            await _db.ListLeftPushAsync(viewKey, element);


            return View(product);
        }

        public async Task<IActionResult> RecentViewProducts()
        {
            var list = await _db.ListRangeAsync("recentViewProducts",0,4);
            await _db.ListTrimAsync("recentViewProducts", 0, 4);
            return View(list);
        }

        [HttpGet]
        public IActionResult CreateProduct()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateProduct(ProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                var product = new Products()
                {
                    Name = model.Name,
                    Url = model.Url
                };
                 _context.Product.Add(product);
                 _context.SaveChanges();
                 return RedirectToAction(nameof(ProductList));

            }

            return View(model);

        }
        public IActionResult Index()
        {
            _db.StringSet("fullName", "Jonty Wang");
            //var name = _db.StringGet("fullName");

            var value = _distributedCache.Get("name-key");
            if (value == null)
            {
                //没有获取数据
                var obj = new Dictionary<string,string>
                {
                    ["FirstName"]="Nike",
                    ["LastName"]="Carter"
                };
                var str = JsonConvert.SerializeObject(obj);
                byte[] encoded = Encoding.UTF8.GetBytes(str);
                var options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(30));
                _distributedCache.Set("name-key",encoded,options);
                return View(obj);
            }
            else
            {
                var str = Encoding.UTF8.GetString(value);
                var obj = JsonConvert.DeserializeObject<Dictionary<string, string>>(str);
                return View(obj);

            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
