using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace Redis.ViewComponents
{
    public class CounterViewComponent:ViewComponent
    {
        private readonly IDatabase _db;

        public CounterViewComponent(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }
       
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var controller = RouteData.Values["Controller"] as string;
            var action = RouteData.Values["Action"] as string;
            if (!string.IsNullOrWhiteSpace(controller) && !string.IsNullOrWhiteSpace(action))
            {
                var pageId = $"{controller}-{action}";
                await _db.StringIncrementAsync(pageId);
                var count = await _db.StringGetAsync(pageId);
                return View("Default", pageId + ":" + count);
            }
            throw new Exception("Cannot get pageId");
        }
    }
}