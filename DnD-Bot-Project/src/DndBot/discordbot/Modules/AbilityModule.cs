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
    public class AbilityModule : ModuleBase<SocketCommandContext>
    {
        private readonly IHttpClientFactory _http;
        public AbilityModule(IHttpClientFactory http) => _http = http;
        private HttpClient Client => _http.CreateClient("DndApi");

        [Command("ability_show_all")]
        public async Task ShowAll()
        {
            var res = await Client.GetAsync("api/abilities");
            if (!res.IsSuccessStatusCode) { await ReplyAsync("Ошибка загрузки."); return; }
            var list = await res.Content.ReadFromJsonAsync<List<IdNameDto>>();
            var text = string.Join("\n", list.Select(x => $"{x.Name} (ID: {x.Id})"));
            await ReplyAsync(text);
        }

        [Command("ability_info_id")]
        public async Task InfoId(int id)
        {
            var res = await Client.GetAsync($"api/abilities/{id}");
            if (!res.IsSuccessStatusCode) { await ReplyAsync("Способность не найдена."); return; }
            var abil = await res.Content.ReadFromJsonAsync<AbilityTree>();
            await ReplyAsync(embed: BuildAbilityEmbed(abil).Build());
        }

        [Command("ability_info_name")]
        public async Task InfoName([Remainder] string name)
        {
            var clean = StringHelper.CleanQuotes(name);
            var res = await Client.GetAsync($"api/abilities/by-name/{Uri.EscapeDataString(clean)}");
            if (!res.IsSuccessStatusCode) { await ReplyAsync("Не найдена."); return; }
            var abil = await res.Content.ReadFromJsonAsync<AbilityTree>();
            await ReplyAsync(embed: BuildAbilityEmbed(abil).Build());
        }

        [Command("ability_create")]
        public async Task Create([Remainder] string args)
        {
            var parts = StringHelper.ParseArgs(args);
            if (parts.Count < 2) { await ReplyAsync("Укажи имя и описание способности."); return; }
            var name = parts[0];
            var description = parts[1];
            var res = await Client.PostAsJsonAsync("api/abilities", new { name, description });
            if (res.IsSuccessStatusCode)
            {
                var id = await res.Content.ReadFromJsonAsync<int>();
                await ReplyAsync($"Способность создана, ID: {id}");
            }
            else await ReplyAsync("Ошибка создания.");
        }

        [Command("ability_delete")]
        public async Task Delete(int id)
        {
            var res = await Client.DeleteAsync($"api/abilities/{id}");
            await ReplyAsync(res.IsSuccessStatusCode ? "Способность удалена." : "Ошибка удаления.");
        }

        [Command("ability_change")]
        public async Task Change(int id, string newName, [Remainder] string newDescription)
        {
            var cleanDesc = StringHelper.CleanQuotes(newDescription);
            var res = await Client.PutAsJsonAsync($"api/abilities/{id}", new { name = newName, description = cleanDesc });
            await ReplyAsync(res.IsSuccessStatusCode ? "Способность изменена." : "Ошибка.");
        }

        [Command("ability_cost_change")]
        public async Task CostChange(int abilityId, int poolId, int cost)
        {
            var res = await Client.PostAsJsonAsync($"api/abilities/{abilityId}/cost", new { poolId, cost });
            await ReplyAsync(res.IsSuccessStatusCode ? "Стоимость задана." : "Ошибка.");
        }

        [Command("ability_cost_remove")]
        public async Task CostRemove(int abilityId)
        {
            var res = await Client.DeleteAsync($"api/abilities/{abilityId}/cost");
            await ReplyAsync(res.IsSuccessStatusCode ? "Стоимости удалены." : "Ошибка.");
        }

        private EmbedBuilder BuildAbilityEmbed(AbilityTree abil)
        {
            var embed = new EmbedBuilder()
                .WithTitle(abil.AbilityName)
                .WithDescription(abil.AbilityDescription ?? "Нет описания");
            if (abil.Costs != null && abil.Costs.Count > 0)
            {
                var costsText = string.Join("\n", abil.Costs.Select(c => $"{c.PoolName}: {c.Cost}"));
                embed.AddField("💰 Стоимость", costsText, true);
            }
            else embed.AddField("💰 Стоимость", "Без затрат", true);
            return embed;
        }
    }
}