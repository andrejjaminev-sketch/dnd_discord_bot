\# D\&D Bot — управление персонажами через Discord



Discord-бот и REST API для настольной ролевой игры (D\&D-подобная система).  

Мастер управляет персонажами, классами, способностями, инвентарём и эффектами прямо в чате.



\## Что внутри



\- `src/DndWorldApi/` — ASP.NET Core Web API + Dapper + PostgreSQL

\- `src/DndBot/` — Discord-бот (Discord.Net)

\- `Database/` — SQL‑скрипты схем, индексов, тестовых данных и ER‑диаграммы



\## Ключевые цифры



\- \*\*85+ команд\*\* бота (полный список: \[`COMMANDS.md`](COMMANDS.md))

\- \*\*30 таблицы\*\* в финальной схеме БД (упрощена после обсуждения с заказчиком)

\- Эволюция схемы: \[v1 (42 таблицы)](Database/schema\_v1\_initial.sql) → \[v2 (33 таблицы)](Database/schema\_v2\_final.sql)



\## Стек



C#, .NET 8+, ASP.NET Core, Dapper, PostgreSQL, Discord.Net



\## Быстрый старт



1\. Создай базу `dnd` в PostgreSQL, выполни `Database/schema\_v2\_final.sql`.

2\. (Опционально) `Database/test\_data.sql` и `Database/indexes.sql`.

3\. Настрой строку подключения в `appsettings.json` проекта API.

4\. Укажи токен Discord-бота в `src/DndBot/Program.cs`.

5\. Запусти оба проекта — бот готов принимать команды.



\## Примеры команд



!char\_id 1

!class\_show\_all

!ability\_info\_name "Fireball"

!inv\_show 1



Полный справочник: \[`COMMANDS.md`](COMMANDS.md).

