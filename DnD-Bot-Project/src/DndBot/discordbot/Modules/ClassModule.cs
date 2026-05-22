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
    public class ClassModule : ModuleBase<SocketCommandContext>
    {
        private readonly IHttpClientFactory _http;
        public ClassModule(IHttpClientFactory http) => _http = http;
        private HttpClient Client => _http.CreateClient("DndApi");

        [Command("class_show_all")]
        public async Task ShowAll()
        {
            var res = await Client.GetAsync("api/classes");
            if (!res.IsSuccessStatusCode) { await ReplyAsync("Ошибка загрузки классов."); return; }
            var list = await res.Content.ReadFromJsonAsync<List<IdNameDto>>();
            var text = string.Join("\n", list.Select(x => $"{x.Name} (ID: {x.Id})"));
            await ReplyAsync(text);
        }

        [Command("class_show_id")]
        public async Task ShowById(int id)
        {
            var res = await Client.GetAsync($"api/classes/{id}");
            if (!res.IsSuccessStatusCode) { await ReplyAsync("Класс не найден."); return; }
            var cls = await res.Content.ReadFromJsonAsync<ClassTree>();
            await ReplyAsync(embed: BuildClassEmbed(cls).Build());
        }

        [Command("class_show_name")]
        public async Task ShowByName([Remainder] string name)
        {
            var clean = StringHelper.CleanQuotes(name);
            var res = await Client.GetAsync($"api/classes/by-name/{Uri.EscapeDataString(clean)}");
            if (!res.IsSuccessStatusCode) { await ReplyAsync("Класс не найден."); return; }
            var cls = await res.Content.ReadFromJsonAsync<ClassTree>();
            await ReplyAsync(embed: BuildClassEmbed(cls).Build());
        }

        [Command("class_create")]
        public async Task Create([Remainder] string args)
        {
            var parts = StringHelper.ParseArgs(args);
            if (parts.Count < 2) { await ReplyAsync("Укажи имя и описание класса."); return; }
            var name = parts[0];
            var description = parts[1];
            var res = await Client.PostAsJsonAsync("api/classes", new { name, description });
            if (res.IsSuccessStatusCode)
            {
                var id = await res.Content.ReadFromJsonAsync<int>();
                await ReplyAsync($"Класс создан, ID: {id}");
            }
            else await ReplyAsync("Ошибка создания.");
        }

        [Command("class_delete")]
        public async Task Delete(int id)
        {
            var res = await Client.DeleteAsync($"api/classes/{id}");
            await ReplyAsync(res.IsSuccessStatusCode ? "Класс удалён." : "Ошибка удаления.");
        }

        [Command("class_name_change")]
        public async Task NameChange(int id, [Remainder] string newName)
        {
            var clean = StringHelper.CleanQuotes(newName);
            var res = await Client.PatchAsJsonAsync($"api/classes/{id}/name", clean);
            await ReplyAsync(res.IsSuccessStatusCode ? "Имя класса изменено." : "Ошибка.");
        }

        [Command("class_description_change")]
        public async Task DescChange(int id, [Remainder] string newDesc)
        {
            var clean = StringHelper.CleanQuotes(newDesc);
            var res = await Client.PatchAsJsonAsync($"api/classes/{id}/description", clean);
            await ReplyAsync(res.IsSuccessStatusCode ? "Описание изменено." : "Ошибка.");
        }

        [Command("class_show_all_branches")]
        public async Task ShowBranches(int id)
        {
            var res = await Client.GetAsync($"api/classes/{id}/branches");
            if (!res.IsSuccessStatusCode) { await ReplyAsync("Класс не найден или нет веток."); return; }
            var branches = await res.Content.ReadFromJsonAsync<List<IdNameDto>>();
            var text = string.Join("\n", branches.Select(b => $"{b.Name} (ID: {b.Id})"));
            await ReplyAsync(text);
        }

        private EmbedBuilder BuildClassEmbed(ClassTree cls)
        {
            var embed = new EmbedBuilder()
                .WithTitle($"Класс: {cls.ClassName}")
                .WithDescription(cls.ClassDescription ?? "Описание отсутствует")
                .WithColor(Color.Gold);

            if (cls.Branches != null && cls.Branches.Count > 0)
            {
                foreach (var branch in cls.Branches)
                {
                    var branchText = $"{branch.BranchDescription ?? "—"}\n";
                    if (branch.Levels != null && branch.Levels.Count > 0)
                    {
                        foreach (var lvl in branch.Levels.OrderBy(l => l.Level))
                        {
                            branchText += $"Ур.{lvl.Level} (треб. очков: {lvl.RequiredPoints})";
                            if (lvl.Abilities != null && lvl.Abilities.Count > 0)
                                branchText += ": " + string.Join(", ", lvl.Abilities.Select(a => a.AbilityName));
                            branchText += "\n";
                        }
                    }
                    embed.AddField(branch.BranchName, branchText, false);
                }
            }
            else embed.AddField("Ветки", "Нет веток", false);
            return embed;
        }
    }
}