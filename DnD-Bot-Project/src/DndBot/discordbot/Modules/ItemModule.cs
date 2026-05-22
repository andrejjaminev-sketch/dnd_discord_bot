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
    public class ItemModule : ModuleBase<SocketCommandContext>
    {
        private readonly IHttpClientFactory _http;
        public ItemModule(IHttpClientFactory http) => _http = http;
        private HttpClient Client => _http.CreateClient("DndApi");

        // --- ПРЕДМЕТЫ (СПРАВОЧНИК) ---

        [Command("item_show_all")]
        public async Task ShowAll()
        {
            var res = await Client.GetAsync("api/items");
            if (!res.IsSuccessStatusCode) { await ReplyAsync("Ошибка загрузки предметов."); return; }
            var items = await res.Content.ReadFromJsonAsync<List<ItemShort>>();
            var text = string.Join("\n", items.Select(x => $"{x.ItemName} ({x.CategoryName}, ID: {x.ItemId})"));
            await ReplyAsync(text);
        }

        [Command("item_info_id")]
        public async Task InfoById(int id)
        {
            var res = await Client.GetAsync($"api/items/{id}");
            if (!res.IsSuccessStatusCode) { await ReplyAsync("Предмет не найден."); return; }
            var item = await res.Content.ReadFromJsonAsync<ItemFull>();
            await ReplyAsync(embed: BuildItemEmbed(item).Build());
        }

        [Command("item_info_name")]
        public async Task InfoByName([Remainder] string name)
        {
            var cleanName = StringHelper.CleanQuotes(name);
            var res = await Client.GetAsync($"api/items/by-name/{Uri.EscapeDataString(cleanName)}");
            if (!res.IsSuccessStatusCode) { await ReplyAsync("Предмет не найден."); return; }
            var item = await res.Content.ReadFromJsonAsync<ItemFull>();
            await ReplyAsync(embed: BuildItemEmbed(item).Build());
        }

        [Command("item_create")]
        public async Task Create(int categoryId, [Remainder] string args)
        {
            var parts = StringHelper.ParseArgs(args);
            if (parts.Count < 1) { await ReplyAsync("Укажи хотя бы название предмета."); return; }
            var dto = new Dictionary<string, object?>
            {
                ["name"] = parts[0],
                ["categoryId"] = categoryId,
                ["description"] = parts.Count > 1 ? parts[1] : null
            };
            var res = await Client.PostAsJsonAsync("api/items", dto);
            if (res.IsSuccessStatusCode) { var id = await res.Content.ReadFromJsonAsync<int>(); await ReplyAsync($"Предмет создан, ID: {id}"); }
            else await ReplyAsync("Ошибка создания.");
        }

        [Command("item_delete")]
        public async Task Delete(int id)
        {
            var res = await Client.DeleteAsync($"api/items/{id}");
            await ReplyAsync(res.IsSuccessStatusCode ? "Предмет удалён." : "Ошибка удаления.");
        }

        [Command("item_change")]
        public async Task Change(int id, [Remainder] string args)
        {
            // args: [name] [description] – поддерживает кавычки
            var parts = StringHelper.ParseArgs(args);
            var dto = new Dictionary<string, object?>();
            if (parts.Count >= 1 && !string.IsNullOrWhiteSpace(parts[0])) dto["name"] = parts[0];
            if (parts.Count >= 2) dto["description"] = parts[1];
            if (dto.Count == 0) { await ReplyAsync("Нет данных для изменения."); return; }
            var res = await Client.PutAsJsonAsync($"api/items/{id}", dto);
            await ReplyAsync(res.IsSuccessStatusCode ? "Предмет изменён." : "Ошибка изменения.");
        }

        // --- ИНВЕНТАРЬ ПЕРСОНАЖА ---

        [Command("inv_show")]
        public async Task ShowInventory(int charId)
        {
            var res = await Client.GetAsync($"api/characters/{charId}/inventory");
            if (!res.IsSuccessStatusCode) { await ReplyAsync("Инвентарь не найден."); return; }
            var inv = await res.Content.ReadFromJsonAsync<CharacterInventory>();
            var embed = new EmbedBuilder()
                .WithTitle($"Инвентарь персонажа #{charId}")
                .WithDescription($"Макс. вес: {inv.MaxWeight}")
                .WithColor(Color.Orange);
            foreach (var slot in inv.Items)
                embed.AddField($"{slot.ItemName} (x{slot.Quantity})", $"Вес: {slot.ItemWeight * slot.Quantity}", true);
            await ReplyAsync(embed: embed.Build());
        }

        [Command("inv_add")]
        public async Task AddInventory(int charId, int itemId, int quantity = 1)
        {
            var res = await Client.PostAsJsonAsync($"api/characters/{charId}/inventory", new { itemId, quantity });
            await ReplyAsync(res.IsSuccessStatusCode ? "Предмет добавлен в инвентарь." : "Ошибка.");
        }

        [Command("inv_set")]
        public async Task SetInventory(int charId, int itemId, int quantity)
        {
            var res = await Client.PutAsJsonAsync($"api/characters/{charId}/inventory", new { itemId, quantity });
            await ReplyAsync(res.IsSuccessStatusCode ? "Количество изменено." : "Ошибка.");
        }

        [Command("inv_remove")]
        public async Task RemoveInventory(int charId, int itemId)
        {
            var res = await Client.DeleteAsync($"api/characters/{charId}/inventory/{itemId}");
            await ReplyAsync(res.IsSuccessStatusCode ? "Предмет удалён из инвентаря." : "Ошибка.");
        }

        // --- ЭКИПИРОВКА ---

        [Command("equip_show")]
        public async Task ShowEquipment(int charId)
        {
            var res = await Client.GetAsync($"api/characters/{charId}/equipment");
            if (!res.IsSuccessStatusCode) { await ReplyAsync("Экипировка не найдена."); return; }
            var equip = await res.Content.ReadFromJsonAsync<CharacterEquipment>();
            var embed = new EmbedBuilder()
                .WithTitle($"Экипировка персонажа #{charId}")
                .WithColor(Color.DarkGreen);
            foreach (var slot in equip.Slots)
                embed.AddField(slot.GearSlot, slot.ItemName, true);
            await ReplyAsync(embed: embed.Build());
        }

        [Command("equip")]
        public async Task Equip(int charId, int itemId, int slotId)
        {
            var res = await Client.PostAsJsonAsync($"api/characters/{charId}/equipment", new { itemId, gearSlotId = slotId });
            await ReplyAsync(res.IsSuccessStatusCode ? "Предмет экипирован." : "Ошибка экипировки.");
        }

        [Command("unequip")]
        public async Task Unequip(int charId, int slotId)
        {
            var res = await Client.DeleteAsync($"api/characters/{charId}/equipment/{slotId}");
            await ReplyAsync(res.IsSuccessStatusCode ? "Предмет снят." : "Ошибка.");
        }

        // ---------- Вспомогательные ----------
        private EmbedBuilder BuildItemEmbed(ItemFull item)
        {
            var embed = new EmbedBuilder()
                .WithTitle(item.ItemName)
                .WithDescription(item.ItemDescription ?? "—")
                .AddField("Категория", item.CategoryName, true);
            if (item.Weight != null) embed.AddField("Вес", item.Weight.ToString(), true);
            if (item.Price != null) embed.AddField("Цена", item.Price, true);
            if (item.MaxDurability != null) embed.AddField("Прочность", $"{item.CurrentDurability}/{item.MaxDurability}", true);
            embed.WithColor(Color.Blue);
            return embed;
        }
    }
}