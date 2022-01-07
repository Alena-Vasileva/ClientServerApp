using Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;

namespace Client.Services
{
    public class MockDataStore : IDataStore<Item>
    {
        private static string url = "https://ecfb-149-154-117-63.ngrok.io";
        private HttpClient client = new HttpClient();

        public async Task<bool> AddItemAsync(Item item)
        {
            var stringContent = new StringContent(JsonSerializer.Serialize<Item>(item));
            await client.PostAsync(url + "/add/", stringContent);
            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateItemAsync(Item item)
        {
            var stringContent = new StringContent(JsonSerializer.Serialize<Item>(item));
            await client.PostAsync(url + "/update/", stringContent);
            return await Task.FromResult(true);
        }

        public async Task<bool> DeleteItemAsync(string id)
        {
            var stringContent = new StringContent(JsonSerializer.Serialize<string>(id));
            await client.PostAsync(url + "/delete/", stringContent);
            return await Task.FromResult(true);
        }

        public async Task<Item> GetItemAsync(string id)
        {
            List<Item> items = new List<Item>(await GetItemsAsync());
            return await Task.FromResult(items.FirstOrDefault(s => s.Id == id));
        }

        public async Task<IEnumerable<Item>> GetItemsAsync(bool forceRefresh = false)
        {
            string result = await client.GetStringAsync(url + "/get/");
            var items = JsonSerializer.Deserialize<IEnumerable<Item>>(result);
            return await Task.FromResult(items);
        }
    }
}