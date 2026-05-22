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
    public class StatModule : ModuleBase<SocketCommandContext>
    {
        private readonly IHttpClientFactory _http;
        public StatModule(IHttpClientFactory http) => _http = http;
        private HttpClient Client => _http.CreateClient("DndApi");

        [Command("stat_show_all")]
        public async Task ShowAll()
        {
            var res = await Client.GetAsync("api/stats");
            if (!res.IsSuccessStatusCode) { await ReplyAsync("Ошибка загрузки."); return; }
            var list = await res.Content.ReadFromJsonAsync<List<IdNameDto>>();
            var text = string.Join("\n", list.Select(x => $"{x.Name} ({x.Id})"));
            await ReplyAsync(text);
        }

        [Command("stat_show_id")]
        public async Task ShowId(int id)
        {
            var res = await Client.GetAsync($"api/stats/{id}");
            if (!res.IsSuccessStatusCode) { await ReplyAsync("Стат не найден."); return; }
            var stat = await res.Content.ReadFromJsonAsync<IdNameDescDto>();
            await ReplyAsync($"{stat.Name}: {stat.Description}");
        }

        [Command("stat_show_name")]
        public async Task ShowName([Remainder] string name)
        {
            var clean = StringHelper.CleanQuotes(name);
            var res = await Client.GetAsync($"api/stats/by-name/{Uri.EscapeDataString(clean)}");
            if (!res.IsSuccessStatusCode) { await ReplyAsync("Не найден."); return; }
            var stat = await res.Content.ReadFromJsonAsync<IdNameDescDto>();
            await ReplyAsync($"{stat.Name}: {stat.Description}");
        }

        [Command("stat_create")]
        public async Task Create([Remainder] string args)
        {
            var parts = StringHelper.ParseArgs(args);
            if (parts.Count < 2) { await ReplyAsync("Укажи имя и описание стата."); return; }
            var name = parts[0];
            var description = parts[1];
            var res = await Client.PostAsJsonAsync("api/stats", new { name, description });
            if (res.IsSuccessStatusCode)
            {
                var id = await res.Content.ReadFromJsonAsync<int>();
                await ReplyAsync($"Стат создан, ID: {id}");
            }
            else await ReplyAsync("Ошибка.");
        }

        [Command("stat_delete")]
        public async Task Delete(int id)
        {
            var res = await Client.DeleteAsync($"api/stats/{id}");
            await ReplyAsync(res.IsSuccessStatusCode ? "Стат удалён." : "Ошибка.");
        }
    }
}