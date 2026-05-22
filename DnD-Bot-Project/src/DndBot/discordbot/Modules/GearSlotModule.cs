using Discord;
using Discord.Commands;
using DndWorldApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DndBot.Helpers;

namespace DndBot.Modules
{
    public class GearSlotModule : ModuleBase<SocketCommandContext>
    {
        private readonly IHttpClientFactory _http;
        public GearSlotModule(IHttpClientFactory http) => _http = http;
        private HttpClient Client => _http.CreateClient("DndApi");

        [Command("gearslot_show_all")]
        public async Task ShowAll()
        {
            var res = await Client.GetAsync("api/gearslots");
            if (!res.IsSuccessStatusCode) { await ReplyAsync("Ошибка загрузки."); return; }
            var list = await ReadJsonAsync<List<IdNameDto>>(res);
            var text = string.Join("\n", list.Select(x => $"{x.Name} (ID: {x.Id})"));
            await ReplyAsync(text);
        }

        [Command("gearslot_create")]
        public async Task Create([Remainder] string name)
        {
            var clean = StringHelper.CleanQuotes(name);
            var res = await Client.PostAsJsonAsync("api/gearslots", clean);
            if (res.IsSuccessStatusCode) { var id = await res.Content.ReadFromJsonAsync<int>(); await ReplyAsync($"Слот создан, ID: {id}"); }
            else await ReplyAsync("Ошибка.");
        }

        [Command("gearslot_delete")]
        public async Task Delete(int id)
        {
            var res = await Client.DeleteAsync($"api/gearslots/{id}");
            await ReplyAsync(res.IsSuccessStatusCode ? "Слот удалён." : "Ошибка.");
        }

        private async Task<T?> ReadJsonAsync<T>(HttpResponseMessage response)
        {
            var stream = await response.Content.ReadAsStreamAsync();
            return await System.Text.Json.JsonSerializer.DeserializeAsync<T>(stream,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}