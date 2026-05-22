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
    public class AbilityTypeModule : ModuleBase<SocketCommandContext>
    {
        private readonly IHttpClientFactory _http;
        public AbilityTypeModule(IHttpClientFactory http) => _http = http;
        private HttpClient Client => _http.CreateClient("DndApi");

        [Command("abilitytype_show_all")]
        public async Task ShowAll()
        {
            var res = await Client.GetAsync("api/abilitytypes");
            if (!res.IsSuccessStatusCode) { await ReplyAsync("Ошибка загрузки."); return; }
            var list = await ReadJsonAsync<List<IdNameDto>>(res);
            var text = string.Join("\n", list.Select(x => $"{x.Name} (ID: {x.Id})"));
            await ReplyAsync(text);
        }

        [Command("abilitytype_create")]
        public async Task Create([Remainder] string args)
        {
            var parts = StringHelper.ParseArgs(args);
            if (parts.Count < 1) { await ReplyAsync("Укажи название типа способности."); return; }
            var name = parts[0];
            var description = parts.Count > 1 ? parts[1] : null;
            var res = await Client.PostAsJsonAsync("api/abilitytypes", new { name, description });
            if (res.IsSuccessStatusCode) { var id = await res.Content.ReadFromJsonAsync<int>(); await ReplyAsync($"Тип способности создан, ID: {id}"); }
            else await ReplyAsync("Ошибка.");
        }

        [Command("abilitytype_delete")]
        public async Task Delete(int id)
        {
            var res = await Client.DeleteAsync($"api/abilitytypes/{id}");
            await ReplyAsync(res.IsSuccessStatusCode ? "Тип удалён." : "Ошибка.");
        }

        private async Task<T?> ReadJsonAsync<T>(HttpResponseMessage response)
        {
            var stream = await response.Content.ReadAsStreamAsync();
            return await System.Text.Json.JsonSerializer.DeserializeAsync<T>(stream,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}