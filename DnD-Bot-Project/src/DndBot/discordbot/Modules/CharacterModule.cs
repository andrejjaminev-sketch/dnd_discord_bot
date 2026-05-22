using Discord;
using Discord.Commands;
using DndBot.Helpers;
using DndWorldApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace DndBot.Modules
{
    public class CharacterModule : ModuleBase<SocketCommandContext>
    {
        private readonly IHttpClientFactory _http;
        public CharacterModule(IHttpClientFactory http) => _http = http;
        private HttpClient Client => _http.CreateClient("DndApi");

        // 1. !char_id <id>
        [Command("char_id")]
        public async Task CharId(int id) => await ShowChar(id);

        // 2. !char_name <name>
        [Command("char_name")]
        public async Task CharName([Remainder] string name) =>
            await SendCharByApi($"api/characters/by-name/{Uri.EscapeDataString(StringHelper.CleanQuotes(name))}");

        // 3. !char_user_id <user_id>
        [Command("char_user_id")]
        public async Task CharUserId(int userId)
        {
            var res = await Client.GetAsync($"api/characters/by-user/{userId}");
            if (!res.IsSuccessStatusCode) { await ReplyAsync("Не найдено."); return; }
            var chars = await res.Content.ReadFromJsonAsync<List<CharFull>>();
            var reply = string.Join("\n", chars.Select(c => $"{c.FullName} (ID: {c.Id})"));
            await ReplyAsync(reply);
        }

        // 4. !char_username <username>
        [Command("char_username")]
        public async Task CharUsername([Remainder] string username)
        {
            var clean = StringHelper.CleanQuotes(username);
            var res = await Client.GetAsync($"api/characters/by-username/{Uri.EscapeDataString(clean)}");
            if (!res.IsSuccessStatusCode) { await ReplyAsync("Не найдено."); return; }
            var chars = await res.Content.ReadFromJsonAsync<List<CharFull>>();
            var reply = string.Join("\n", chars.Select(c => $"{c.FullName} (ID: {c.Id})"));
            await ReplyAsync(reply);
        }

        // 5. !char_name_change <id> <new_name>
        [Command("char_name_change")]
        public async Task NameChange(int id, [Remainder] string newName)
        {
            var clean = StringHelper.CleanQuotes(newName);
            var res = await Client.PatchAsJsonAsync($"api/characters/{id}/name", clean);
            await ReplyAsync(res.IsSuccessStatusCode ? "Имя изменено." : "Ошибка.");
        }

        // 6. !char_age_change <id> <new_age>
        [Command("char_age_change")]
        public async Task AgeChange(int id, int newAge) =>
            await PatchAndReply($"api/characters/{id}/age", newAge, "Возраст изменён.");

        // 7. !char_lvl_change <id> <new_lvl>
        [Command("char_lvl_change")]
        public async Task LevelChange(int id, int newLvl) =>
            await PatchAndReply($"api/characters/{id}/level", newLvl, "Уровень изменён.");

        // 8. !char_user_id_change <id> <new_user_id>
        [Command("char_user_id_change")]
        public async Task UserIdChange(int id, int newUserId) =>
            await PatchAndReply($"api/characters/{id}/user", newUserId, "Пользователь изменён.");

        // 9. !char_class_add <id> <class_id>
        [Command("char_class_add")]
        public async Task ClassAdd(int id, int classId) =>
            await PostAndReply($"api/characters/{id}/class", classId, "Класс добавлен.");

        // 10. !char_class_lvl_change <id> <class_id> <new_lvl>
        [Command("char_class_lvl_change")]
        public async Task ClassLevelChange(int id, int classId, int newLvl) =>
            await PatchAndReply($"api/characters/{id}/class/{classId}/level", newLvl, "Уровень класса изменён.");

        // 11. !char_class_remove <id> <class_id>
        [Command("char_class_remove")]
        public async Task ClassRemove(int id, int classId) =>
            await DeleteAndReply($"api/characters/{id}/class/{classId}", "Класс удалён.");

        // 12. !char_branch_invest_points <id> <branch_id> <amount>
        [Command("char_branch_invest_points")]
        public async Task BranchInvest(int id, int branchId, int amount) =>
            await PatchAndReply($"api/characters/{id}/branch/{branchId}/invest", amount, "Очки вложены.");

        // 13. !char_branch_reset <id> <branch_id>
        [Command("char_branch_reset")]
        public async Task BranchReset(int id, int branchId) =>
            await PatchAndReply($"api/characters/{id}/branch/{branchId}/reset", null, "Ветка сброшена.");

        // 14. !char_branch_lvl_set <id> <branch_id> <new_lvl>
        [Command("char_branch_lvl_set")]
        public async Task BranchLevelSet(int id, int branchId, int newLvl) =>
            await PatchAndReply($"api/characters/{id}/branch/{branchId}/level", newLvl, "Уровень ветки изменён.");

        // 15. !char_branch_invested_points_set <id> <branch_id> <points>
        [Command("char_branch_invested_points_set")]
        public async Task BranchPointsSet(int id, int branchId, int points) =>
            await PatchAndReply($"api/characters/{id}/branch/{branchId}/points", points, "Очки ветки изменены.");

        // 16. !char_ability_add <id> <ability_id>
        [Command("char_ability_add")]
        public async Task AbilityAdd(int id, int abilityId) =>
            await PostAndReply($"api/characters/{id}/ability", abilityId, "Способность добавлена.");

        // 17. !char_ability_remove <id> <ability_id>
        [Command("char_ability_remove")]
        public async Task AbilityRemove(int id, int abilityId) =>
            await DeleteAndReply($"api/characters/{id}/ability/{abilityId}", "Способность удалена.");

        // 18. !char_pool_add <id> <pool_id>
        [Command("char_pool_add")]
        public async Task PoolAdd(int id, int poolId) =>
            await PostAndReply($"api/characters/{id}/pool", new { poolId, baseValue = 10, coefficient = 1.0 }, "Пул добавлен.");

        // 19. !char_pool_remove <id> <pool_id>
        [Command("char_pool_remove")]
        public async Task PoolRemove(int id, int poolId) =>
            await DeleteAndReply($"api/characters/{id}/pool/{poolId}", "Пул удалён.");

        // 20. !char_pool_base_change <id> <pool_id> <new_base>
        [Command("char_pool_base_change")]
        public async Task PoolBaseChange(int id, int poolId, int newBase) =>
            await PatchAndReply($"api/characters/{id}/pool/{poolId}/base", newBase, "Базовое значение пула изменено.");

        // 21. !char_pool_coefficient_change <id> <pool_id> <new_coefficient>
        [Command("char_pool_coefficient_change")]
        public async Task PoolCoefChange(int id, int poolId, string newCoefStr)
        {
            if (!float.TryParse(newCoefStr.Replace(',', '.'),
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out float newCoef))
            {
                await ReplyAsync("Неверный формат коэффициента.");
                return;
            }
            await PatchAndReply($"api/characters/{id}/pool/{poolId}/coefficient", newCoef, "Коэффициент изменён.");
        }

        // 22. !char_pool_current_value_change <id> <pool_id> <new_value>
        [Command("char_pool_current_value_change")]
        public async Task PoolCurrentChange(int id, int poolId, int newValue) =>
            await PatchAndReply($"api/characters/{id}/pool/{poolId}/current", newValue, "Текущее значение пула изменено.");

        // 23. !char_notes_add <id> <title> <content>
        [Command("char_notes_add")]
        public async Task NoteAdd(int id, [Remainder] string args)
        {
            var parts = StringHelper.ParseArgs(args);
            if (parts.Count < 2) { await ReplyAsync("Укажи заголовок и содержание заметки."); return; }
            var title = parts[0];
            var content = parts[1];
            var res = await Client.PostAsJsonAsync($"api/characters/{id}/note", new { title, content });
            await ReplyAsync(res.IsSuccessStatusCode ? "Заметка добавлена." : "Ошибка добавления.");
        }

        // 24. !char_notes_show_all <id>
        [Command("char_notes_show_all")]
        public async Task NotesShowAll(int id)
        {
            var res = await Client.GetAsync($"api/characters/{id}/notes");
            if (!res.IsSuccessStatusCode) { await ReplyAsync("Заметки не найдены."); return; }
            var notes = await res.Content.ReadFromJsonAsync<List<CharFull.CharNote>>();
            var text = string.Join("\n", notes.Select(n => $"{n.NoteTitle} (ID: {n.NoteId})"));
            await ReplyAsync(text);
        }

        // 25. !char_notes_show_one_id <id> <note_id>
        [Command("char_notes_show_one_id")]
        public async Task NoteShowById(int id, int noteId)
        {
            var res = await Client.GetAsync($"api/characters/{id}/note/{noteId}");
            if (!res.IsSuccessStatusCode) { await ReplyAsync("Заметка не найдена."); return; }
            var note = await res.Content.ReadFromJsonAsync<CharFull.CharNote>();
            await ReplyAsync($"**{note.NoteTitle}**: {note.NoteContent}");
        }

        // 26. !char_notes_show_one_title <id> <note_title>
        [Command("char_notes_show_one_title")]
        public async Task NoteShowByTitle(int id, [Remainder] string title)
        {
            var clean = StringHelper.CleanQuotes(title);
            var res = await Client.GetAsync($"api/characters/{id}/note/by-title/{Uri.EscapeDataString(clean)}");
            if (!res.IsSuccessStatusCode) { await ReplyAsync("Заметка не найдена."); return; }
            var note = await res.Content.ReadFromJsonAsync<CharFull.CharNote>();
            await ReplyAsync($"**{note.NoteTitle}**: {note.NoteContent}");
        }

        // 27. !char_stat_add <id> <stat_id>
        [Command("char_stat_add")]
        public async Task StatAdd(int id, int statId) =>
            await PostAndReply($"api/characters/{id}/stat", statId, "Стат добавлен.");

        // 28. !char_stat_remove <id> <stat_id>
        [Command("char_stat_remove")]
        public async Task StatRemove(int id, int statId) =>
            await DeleteAndReply($"api/characters/{id}/stat/{statId}", "Стат удалён.");

        // 29. !char_stat_lvl_change <id> <stat_id> <new_value>
        [Command("char_stat_lvl_change")]
        public async Task StatValueChange(int id, int statId, int newValue) =>
            await PatchAndReply($"api/characters/{id}/stat/{statId}/value", newValue, "Значение стата изменено.");

        // 30. !char_create <name>
        [Command("char_create")]
        public async Task CharCreate([Remainder] string name)
        {
            var clean = StringHelper.CleanQuotes(name);
            var res = await Client.PostAsJsonAsync("api/characters", clean);
            if (res.IsSuccessStatusCode)
            {
                var id = await res.Content.ReadFromJsonAsync<int>();
                await ReplyAsync($"Персонаж создан, ID: {id}");
            }
            else await ReplyAsync("Ошибка создания.");
        }

        // 31. !char_delete <id>
        [Command("char_delete")]
        public async Task CharDelete(int id) =>
            await DeleteAndReply($"api/characters/{id}", "Персонаж удалён.");

        // 32. !char_species_change <id> <species_id>
        [Command("char_species_change")]
        public async Task SpeciesChange(int id, int speciesId) =>
            await PatchAndReply($"api/characters/{id}/species", speciesId, "Раса изменена.");

        // 33. !char_notes_remove <id> <note_id>
        [Command("char_notes_remove")]
        public async Task NoteRemove(int id, int noteId) =>
            await DeleteAndReply($"api/characters/{id}/note/{noteId}", "Заметка удалена.");

        // 34. !char_class_info <id> <class_id>
        [Command("char_class_info")]
        public async Task ClassInfo(int id, int classId)
        {
            var res = await Client.GetAsync($"api/characters/{id}/class/{classId}");
            if (!res.IsSuccessStatusCode) { await ReplyAsync("Класс не найден."); return; }
            var cls = await res.Content.ReadFromJsonAsync<CharFull.CharClass>();
            var embed = new EmbedBuilder()
                .WithTitle($"Класс {cls.ClassName} (ур. {cls.Lvl})");
            if (cls.Branches != null)
                foreach (var branch in cls.Branches)
                    embed.AddField(branch.BranchName,
                        $"Уровень: {branch.CurrentLvl}, Очков: {branch.InvestedPoints}", false);
            await ReplyAsync(embed: embed.Build());
        }

        // 35. !char_ability_buy <id> <ability_id>
        [Command("char_ability_buy")]
        public async Task AbilityBuy(int id, int abilityId) =>
            await PostAndReply($"api/characters/{id}/buy-ability", abilityId, "Способность куплена.");

        // ---------- Вспомогательные ----------
        private async Task ShowChar(int id)
        {
            var res = await Client.GetAsync($"api/characters/{id}");
            if (!res.IsSuccessStatusCode) { await ReplyAsync("Персонаж не найден."); return; }
            var c = await res.Content.ReadFromJsonAsync<CharFull>();
            await ReplyAsync(embed: BuildCharEmbed(c).Build());
        }

        private async Task SendCharByApi(string url)
        {
            var res = await Client.GetAsync(url);
            if (!res.IsSuccessStatusCode) { await ReplyAsync("Не найдено."); return; }
            var c = await res.Content.ReadFromJsonAsync<CharFull>();
            await ReplyAsync(embed: BuildCharEmbed(c).Build());
        }

        private async Task PatchAndReply(string url, object value, string successMsg)
        {
            var res = await Client.PatchAsJsonAsync(url, value);
            await ReplyAsync(res.IsSuccessStatusCode ? successMsg : "Ошибка.");
        }

        private async Task PostAndReply(string url, object value, string successMsg)
        {
            var res = await Client.PostAsJsonAsync(url, value);
            await ReplyAsync(res.IsSuccessStatusCode ? successMsg : "Ошибка.");
        }

        private async Task DeleteAndReply(string url, string successMsg)
        {
            var res = await Client.DeleteAsync(url);
            await ReplyAsync(res.IsSuccessStatusCode ? successMsg : "Ошибка.");
        }

        private string GetClassesSummary(List<CharFull.CharClass> classes)
        {
            if (classes == null || classes.Count == 0) return "Нет";
            var sb = new StringBuilder();
            foreach (var cls in classes)
            {
                sb.AppendLine($"**{cls.ClassName}** (ур. {cls.Lvl})");
                if (cls.Branches != null)
                    foreach (var branch in cls.Branches)
                        sb.AppendLine($"  ▫ {branch.BranchName} (ур. {branch.CurrentLvl}, очков: {branch.InvestedPoints})");
            }
            return sb.ToString();
        }

        private EmbedBuilder BuildCharEmbed(CharFull c)
        {
            var embed = new EmbedBuilder()
                .WithTitle(c.FullName)
                .WithDescription(
                    $"**Раса:** {c.SpeciesName}\n" +
                    $"**Возраст:** {c.Age?.ToString() ?? "неизвестно"}\n" +
                    $"**Очки развития:** {c.FreePoint}"
                );

            if (c.Stats != null && c.Stats.Count > 0)
                embed.AddField("📊 Статы", string.Join("\n", c.Stats.Select(s => $"{s.StatName}: {s.Value}")), true);

            if (c.CharsPools != null && c.CharsPools.Count > 0)
                embed.AddField("💧 Пулы", string.Join("\n", c.CharsPools.Select(p => $"{p.PoolName}: {p.CurrentValue}/{p.MaxValue}")), true);

            embed.AddField("⚔️ Классы", GetClassesSummary(c.CharsClasses), false);

            var allAbilities = new List<string>();
            if (c.CharsClasses != null)
                foreach (var cls in c.CharsClasses)
                    if (cls.Branches != null)
                        foreach (var branch in cls.Branches)
                            if (branch.Abilities != null)
                                allAbilities.AddRange(branch.Abilities.Select(a => a.AbilityName));
            if (c.NonClassAbilities != null)
                allAbilities.AddRange(c.NonClassAbilities.Select(a => a.AbilityName));
            if (allAbilities.Count > 0)
                embed.AddField("✨ Способности", string.Join(", ", allAbilities.Distinct()), false);

            embed.WithColor(Color.Blue)
                 .WithFooter(footer => footer.Text = $"ID персонажа: {c.Id}");
            return embed;
        }
    }
}