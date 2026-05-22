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
    public class SpeciesModule : ModuleBase<SocketCommandContext>
    {
        private readonly IHttpClientFactory _http;
        public SpeciesModule(IHttpClientFactory http) => _http = http;
        private HttpClient Client => _http.CreateClient("DndApi");

        [Command("species_show_all")]
        public async Task ShowAll()
        {
            var res = await Client.GetAsync("api/species");
            if (!res.IsSuccessStatusCode) { await ReplyAsync("Ошибка загрузки."); return; }
            var list = await ReadJsonAsync<List<IdNameDto>>(res);
            var text = string.Join("\n", list.Select(x => $"{x.Name} (ID: {x.Id})"));
            await ReplyAsync(text);
        }

        [Command("species_create")]
        public async Task Create([Remainder] string args)
        {
            var parts = StringHelper.ParseArgs(args);
            if (parts.Count < 1) { await ReplyAsync("Укажи хотя бы название расы."); return; }
            var name = parts[0];
            var description = parts.Count > 1 ? parts[1] : null;
            var res = await Client.PostAsJsonAsync("api/species", new { name, description });
            if (res.IsSuccessStatusCode) { var id = await res.Content.ReadFromJsonAsync<int>(); await ReplyAsync($"Раса создана, ID: {id}"); }
            else await ReplyAsync("Ошибка.");
        }

        [Command("species_delete")]
        public async Task Delete(int id)
        {
            var res = await Client.DeleteAsync($"api/species/{id}");
            await ReplyAsync(res.IsSuccessStatusCode ? "Раса удалена." : "Ошибка.");
        }

        private async Task<T?> ReadJsonAsync<T>(HttpResponseMessage response)
        {
            var stream = await response.Content.ReadAsStreamAsync();
            return await System.Text.Json.JsonSerializer.DeserializeAsync<T>(stream,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}