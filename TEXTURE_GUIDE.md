# Руководство по использованию системы текстур BlockerPasses

## Быстрый старт

### 1. Создание текстуры с картинкой (как в режиме 2x2)

```bash
# Создать текстуру с паттерном 2x2
css_bp_createtexture 2x2_pattern "Паттерн 2x2" null patterns

# Применить к блоку
css_bp_applytexture 1 2x2_pattern
```

### 2. Создание пользовательской текстуры

```bash
# Создать текстуру с пользовательским изображением
css_bp_createtexture my_logo "Мой логотип" materials/custom/my_logo.vmt custom

# Применить к блоку
css_bp_applytexture 2 my_logo
```

### 3. Использование через меню

1. Введите `css_bp_menu` или `css_bp`
2. Выберите "🎨 Управление текстурами"
3. Выберите нужную опцию

## Доступные команды

| Команда | Описание | Пример |
|---------|----------|--------|
| `css_bp_createtexture` | Создать новую текстуру | `css_bp_createtexture name "Display Name"` |
| `css_bp_applytexture` | Применить текстуру к блоку | `css_bp_applytexture 1 texture_name` |
| `css_bp_textures` | Показать все доступные текстуры | `css_bp_textures` |

## Предустановленные текстуры

- **white_block** - Белый блок (по умолчанию)
- **blue_block** - Синий блок
- **red_block** - Красный блок  
- **green_block** - Зеленый блок
- **2x2_pattern** - Паттерн 2x2 как в соревновательном режиме

## Примеры использования

### Создание текстуры с логотипом сервера
```bash
css_bp_createtexture server_logo "Логотип сервера" materials/logos/server_logo.vmt branding
css_bp_applytexture 1 server_logo
```

### Применение паттерна 2x2 к нескольким блокам
```bash
css_bp_applytexture 1 2x2_pattern
css_bp_applytexture 2 2x2_pattern
css_bp_applytexture 3 2x2_pattern
```

### Просмотр всех доступных текстур
```bash
css_bp_textures
```

## Структура файлов

```
BlockerPasses-CS2/
├── BlockerPasses.cs          # Основной код плагина
├── blocker_passes_example.json # Пример конфигурации
├── TEXTURE_SYSTEM.md         # Подробная документация
└── README.md                 # Основное руководство
```

## Совместимость

- ✅ Полная совместимость с существующими блоками
- ✅ Поддержка пользовательских текстур
- ✅ Интеграция с меню управления
- ✅ Многоязычная поддержка (EN/RU)

## Поддержка

Если у вас возникли вопросы или проблемы, проверьте:
1. Правильность синтаксиса команд
2. Существование указанных путей к текстурам
3. Корректность индексов блоков

Для получения подробной информации см. файл `TEXTURE_SYSTEM.md`.
