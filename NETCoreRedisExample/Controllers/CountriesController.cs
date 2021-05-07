using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using NETCoreRedisExample.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;

namespace NETCoreRedisExample.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    public class CountriesController : ControllerBase
    {
        private readonly IDistributedCache _distributedCache;
        private const string CountriesKey = "Countries";
        private static HttpClient _Client;
        public CountriesController(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
            _Client = new HttpClient();
        }
        [HttpGet]
        [Route("Search/{name}")]
        public async Task<IActionResult> GetContry([FromRoute][Bind][Required] string name)
        {
            var countriesObject = await _distributedCache.GetStringAsync(CountriesKey);
            if (!string.IsNullOrWhiteSpace(countriesObject))
            {
                return Ok(countriesObject);
            }
            else
            {
                var _list = await GetCountry(name).ConfigureAwait(true);
                return Ok(_list);
            }

        }
        [HttpGet]
        [Route("All")]
        public async Task<IActionResult> GetCountries()
        {
            var countriesObject = await _distributedCache.GetStringAsync(CountriesKey);

            if (!string.IsNullOrWhiteSpace(countriesObject))
            {
                return Ok(countriesObject);
            }
            else
            {
                var _list = await GetAllCountries().ConfigureAwait(true);
                return Ok(_list);
            }

        }
        private async ValueTask<IEnumerable<Country>> GetAllCountries()
        {

            var response = await _Client.GetAsync("https://restcountries.eu/rest/v2/all");
            var responseData = await response.Content.ReadAsStringAsync();
            var countries = JsonConvert.DeserializeObject<List<Country>>(responseData);
            var memoryCacheEntryOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(3600),
                SlidingExpiration = TimeSpan.FromSeconds(1200)
            };

            await _distributedCache.SetStringAsync(CountriesKey, responseData, memoryCacheEntryOptions);

            return countries;
        }
        private async ValueTask<IEnumerable<Country>> GetCountry(string name)
        {

            var response = await _Client.GetAsync("https://restcountries.eu/rest/v2/all");
            var responseData = await response.Content.ReadAsStringAsync();
            var countries = JsonConvert.DeserializeObject<List<Country>>(responseData);

            var searche_countries = countries.Where(x => x.Name.Contains(name));
            var memoryCacheEntryOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(3600),
                SlidingExpiration = TimeSpan.FromSeconds(1200)
            };

            await _distributedCache.SetStringAsync(name, responseData, memoryCacheEntryOptions);

            return searche_countries;
        }
    }
}
