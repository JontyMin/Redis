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

        }
    }
}