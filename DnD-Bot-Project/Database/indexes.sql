-- ============================================================
-- Индексы для ускорения работы D&D Bot API
-- ============================================================

-- Поиск персонажа по полному имени (используется в api/characters/by-name/{name})
-- ILIKE с константным началом может использовать этот индекс.
CREATE INDEX idx_char_full_name ON dnd.char (full_name varchar_pattern_ops);

-- Поиск персонажа по ID пользователя (api/characters/by-user/{userId})
CREATE INDEX idx_char_user_id ON dnd.char (user_id);

-- Поиск пользователя по имени (api/characters/by-username/{username})
CREATE INDEX idx_user_username ON dnd."user" (username varchar_pattern_ops);

-- Связь персонаж-класс (частые JOIN и фильтрация по char_id, class_id)
CREATE INDEX idx_char_class_char_id ON dnd.char_class (char_id);
CREATE INDEX idx_char_class_class_id ON dnd.char_class (class_id);

-- Связь персонаж-ветка
CREATE INDEX idx_char_branch_char_id ON dnd.char_branch (char_id);
CREATE INDEX idx_char_branch_branch_id ON dnd.char_branch (branch_id);

-- Связь персонаж-способность
CREATE INDEX idx_ability_char_char_id ON dnd.ability_char (char_id);
CREATE INDEX idx_ability_char_ability_id ON dnd.ability_char (ability_id);

-- Связь персонаж-заметки
CREATE INDEX idx_char_notes_char_id ON dnd.char_notes (char_id);
CREATE INDEX idx_char_notes_note_id ON dnd.char_notes (note_id);

-- Связь персонаж-эффекты
CREATE INDEX idx_effect_char_char_id ON dnd.effect_char (char_id);
CREATE INDEX idx_effect_char_effect_id ON dnd.effect_char (effect_id);

-- Поиск ветки по имени
CREATE INDEX idx_branch_name ON dnd.branch (name varchar_pattern_ops);

-- Поиск способности по имени
CREATE INDEX idx_ability_name ON dnd.ability (name varchar_pattern_ops);

-- Поиск предмета по имени
CREATE INDEX idx_item_name ON dnd.item (name varchar_pattern_ops);

-- Поиск класса по имени
CREATE INDEX idx_class_name ON dnd.class (name varchar_pattern_ops);

-- Поиск стата / пула / расы по имени
CREATE INDEX idx_stat_name ON dnd.stat (name varchar_pattern_ops);
CREATE INDEX idx_pool_name ON dnd.pool (name varchar_pattern_ops);
CREATE INDEX idx_species_name ON dnd.species (name varchar_pattern_ops);

-- Поиск заметок по заголовку (используется ILIKE)
CREATE INDEX idx_note_title ON dnd.note (title varchar_pattern_ops);

-- Инвентарь: ускоряет JOIN inventory_item с inventory
CREATE INDEX idx_inventory_item_inventory_id ON dnd.inventory_item (inventory_id);
CREATE INDEX idx_inventory_item_item_id ON dnd.inventory_item (item_id);

-- Экипировка: поиск по персонажу и слоту
CREATE INDEX idx_equipped_item_char_id ON dnd.equipped_item (char_id);
CREATE INDEX idx_equipped_item_slot_id ON dnd.equipped_item (gear_slot_id);

-- Статы и пулы персонажа
CREATE INDEX idx_stat_char_char_id ON dnd.stat_char (char_id);
CREATE INDEX idx_pool_char_char_id ON dnd.pool_char (char_id);

-- Способности в ветках: ускоряет построение дерева классов
CREATE INDEX idx_branch_ability_branch_id ON dnd.branch_ability (branch_id);
CREATE INDEX idx_branch_ability_ability_id ON dnd.branch_ability (ability_id);

-- Уровни веток: поиск по ветке и уровню
CREATE INDEX idx_branch_level_branch_id ON dnd.branch_level (branch_id);

-- Стоимость способностей
CREATE INDEX idx_ability_cost_ability_id ON dnd.ability_cost (ability_id);

-- Категории предметов и эффектов (для списков)
CREATE INDEX idx_item_category_name ON dnd.item_category (name varchar_pattern_ops);
CREATE INDEX idx_effect_category_name ON dnd.effect_category (name varchar_pattern_ops);

-- Типы способностей
CREATE INDEX idx_ability_type_name ON dnd.ability_type (name varchar_pattern_ops);