using Discord;
using Discord.Commands;
using DndBot.Helpers;
using DndWorldApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DndBot.Modules
{
    public class PoolModule : ModuleBase<SocketCommandContext>
    {
        private readonly IHttpClientFactory _http;
        public PoolModule(IHttpClientFactory http) => _http = http;
        private HttpClient Client => _http.CreateClient("DndApi");

        [Command("pool_show_all")]
        public async Task ShowAll()
        {
            var res = await Client.GetAsync("api/pools");
            if (!res.IsSuccessStatusCode) { await ReplyAsync("Ошибка загрузки."); return; }
            var list = await res.Content.ReadFromJsonAsync<List<IdNameDto>>();
            var text = string.Join("\n", list.Select(x => $"{x.Name} ({x.Id})"));
            await ReplyAsync(text);
        }

        [Command("pool_show_id")]
        public async Task ShowId(int id)
        {
            var res = await Client.GetAsync($"api/pools/{id}");
            if (!res.IsSuccessStatusCode) { await ReplyAsync("Пул не найден."); return; }
            var pool = await res.Content.ReadFromJsonAsync<IdNameDescDto>();
            await ReplyAsync($"{pool.Name}: {pool.Description}");
        }

        [Command("pool_show_name")]
        public async Task ShowName([Remainder] string name)
        {
            var clean = StringHelper.CleanQuotes(name);
            var res = await Client.GetAsync($"api/pools/by-name/{Uri.EscapeDataString(clean)}");
            if (!res.IsSuccessStatusCode) { await ReplyAsync("Не найден."); return; }
            var pool = await res.Content.ReadFromJsonAsync<IdNameDescDto>();
            await ReplyAsync($"{pool.Name}: {pool.Description}");
        }

        [Command("pool_create")]
        public async Task Create([Remainder] string args)
        {
            var parts = StringHelper.ParseArgs(args);
            if (parts.Count < 2) { await ReplyAsync("Укажи имя и описание пула."); return; }
            var name = parts[0];
            var description = parts[1];
            var res = await Client.PostAsJsonAsync("api/pools", new { name, description });
            if (res.IsSuccessStatusCode)
            {
                var id = await res.Content.ReadFromJsonAsync<int>();
                await ReplyAsync($"Пул создан, ID: {id}");
            }
            else await ReplyAsync("Ошибка.");
        }

        [Command("pool_delete")]
        public async Task Delete(int id)
        {
            var res = await Client.DeleteAsync($"api/pools/{id}");
            await ReplyAsync(res.IsSuccessStatusCode ? "Пул удалён." : "Ошибка.");
        }
    }
}