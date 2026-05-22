using Discord;
using Discord.Commands;
using DndBot.Helpers;
using DndWorldApi.Models;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DndBot.Modules
{
    public class BranchModule : ModuleBase<SocketCommandContext>
    {
        private readonly IHttpClientFactory _http;
        public BranchModule(IHttpClientFactory http) => _http = http;
        private HttpClient Client => _http.CreateClient("DndApi");

        [Command("branch_info_id")]
        public async Task InfoId(int id)
        {
            var res = await Client.GetAsync($"api/branches/{id}");
            if (!res.IsSuccessStatusCode) { await ReplyAsync("Ветка не найдена."); return; }
            var branch = await res.Content.ReadFromJsonAsync<BranchTree>();
            await ReplyAsync(embed: BuildBranchEmbed(branch).Build());
        }

        [Command("branch_info_name")]
        public async Task InfoName([Remainder] string name)
        {
            var clean = StringHelper.CleanQuotes(name);
            var res = await Client.GetAsync($"api/branches/by-name/{Uri.EscapeDataString(clean)}");
            if (!res.IsSuccessStatusCode) { await ReplyAsync("Ветка не найдена."); return; }
            var branch = await res.Content.ReadFromJsonAsync<BranchTree>();
            await ReplyAsync(embed: BuildBranchEmbed(branch).Build());
        }

        [Command("branch_create")]
        public async Task Create(int classId, [Remainder] string args)
        {
            var parts = StringHelper.ParseArgs(args);
            if (parts.Count < 2) { await ReplyAsync("Укажи имя и описание ветки."); return; }
            var name = parts[0];
            var description = parts[1];
            var res = await Client.PostAsJsonAsync("api/branches", new { classId, name, description });
            if (res.IsSuccessStatusCode)
            {
                var id = await res.Content.ReadFromJsonAsync<int>();
                await ReplyAsync($"Ветка создана, ID: {id}");
            }
            else await ReplyAsync("Ошибка создания.");
        }

        [Command("branch_delete")]
        public async Task Delete(int id)
        {
            var res = await Client.DeleteAsync($"api/branches/{id}");
            await ReplyAsync(res.IsSuccessStatusCode ? "Ветка удалена." : "Ошибка удаления.");
        }

        [Command("branch_name_change")]
        public async Task NameChange(int id, [Remainder] string newName)
        {
            var clean = StringHelper.CleanQuotes(newName);
            var res = await Client.PatchAsJsonAsync($"api/branches/{id}/name", clean);
            await ReplyAsync(res.IsSuccessStatusCode ? "Имя ветки изменено." : "Ошибка.");
        }

        [Command("branch_description_change")]
        public async Task DescChange(int id, [Remainder] string newDesc)
        {
            var clean = StringHelper.CleanQuotes(newDesc);
            var res = await Client.PatchAsJsonAsync($"api/branches/{id}/description", clean);
            await ReplyAsync(res.IsSuccessStatusCode ? "Описание изменено." : "Ошибка.");
        }

        [Command("branch_lvl_create")]
        public async Task LvlCreate(int branchId, int lvl, int requiredPoints, int abilityId)
        {
            var res = await Client.PostAsJsonAsync($"api/branches/{branchId}/level", new { level = lvl, requiredPoints, abilityId });
            await ReplyAsync(res.IsSuccessStatusCode ? "Уровень ветки создан." : "Ошибка создания.");
        }

        [Command("branch_lvl_delete")]
        public async Task LvlDelete(int lvlId)
        {
            var res = await Client.DeleteAsync($"api/branches/level/{lvlId}");
            await ReplyAsync(res.IsSuccessStatusCode ? "Уровень удалён." : "Ошибка удаления.");
        }

        private EmbedBuilder BuildBranchEmbed(BranchTree branch)
        {
            var embed = new EmbedBuilder()
                .WithTitle(branch.BranchName)
                .WithDescription(branch.BranchDescription);
            if (branch.Levels != null)
                foreach (var lvl in branch.Levels.OrderBy(l => l.Level))
                {
                    var abilities = lvl.Abilities != null ? string.Join(", ", lvl.Abilities.Select(a => a.AbilityName)) : "нет";
                    embed.AddField($"Уровень {lvl.Level} (очков: {lvl.RequiredPoints})", $"Способности: {abilities}");
                }
            return embed;
        }
    }
}