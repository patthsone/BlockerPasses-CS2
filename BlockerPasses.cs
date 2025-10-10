using System.Drawing;
using System.Globalization;
using System.Text.Json;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;

namespace BlockerPasses;

[MinimumApiVersion(90)]
public class BlockerPasses : BasePlugin
{
    public override string ModuleAuthor => "PattHs";
    public override string ModuleName => "Blocker Passes";
    public override string ModuleVersion => "v0.1.0";

    private Config _config = null!;
    private bool _menuManagerAvailable = false;
    private Dictionary<string, Dictionary<string, string>> _translations = new();

    public override void Load(bool hotReload)
    {
        _config = LoadConfig();
        InitializeTranslations();
        RegisterEventHandler<EventRoundStart>(EventRoundStart);
    }

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        try
        {
            // Проверяем доступность MenuManager через рефлексию
            var menuCapabilityType = Type.GetType("MenuCapability");
            if (menuCapabilityType != null)
            {
                var getMethod = menuCapabilityType.GetMethod("Get");
                if (getMethod != null)
                {
                    var menuApi = getMethod.Invoke(null, null);
                    if (menuApi != null)
                    {
                        _menuManagerAvailable = true;
                        Logger.LogInformation("MenuManager API detected and loaded");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _menuManagerAvailable = false;
            Logger.LogWarning($"MenuManager API not available: {ex.Message}");
        }
    }

    [RequiresPermissions("@css/root")]
    [ConsoleCommand("css_bp_lang")]
    public void OnCmdLang(CCSPlayerController? player, CommandInfo info)
    {
        if (info.ArgCount < 2)
        {
            var message = GetTranslation("invalid_language");
            if (player == null)
                Console.WriteLine($"[BlockerPasses] {message}");
            else
                player.PrintToChat($" {ReplaceColorTags("{RED}[BlockerPasses] " + message)}");
            return;
        }

        var lang = info.ArgString.ToLower();
        if (lang != "en" && lang != "ru")
        {
            var message = GetTranslation("invalid_language");
            if (player == null)
                Console.WriteLine($"[BlockerPasses] {message}");
            else
                player.PrintToChat($" {ReplaceColorTags("{RED}[BlockerPasses] " + message)}");
            return;
        }

        // Update config with new language
        var newConfig = _config with { Language = _config.Language with { CurrentLanguage = lang } };
        _config = newConfig;

        var successMessage = GetTranslation("language_changed");
        if (player == null)
            Console.WriteLine($"[BlockerPasses] {successMessage}");
        else
            player.PrintToChat($" {ReplaceColorTags("{GREEN}[BlockerPasses] " + successMessage)}");
    }

    [RequiresPermissions("@css/root")]
    [ConsoleCommand("css_bp_reload")]
    public void OnCmdReload(CCSPlayerController? player, CommandInfo info)
    {
        _config = LoadConfig();
        InitializeTranslations();

        var msg = GetTranslation("config_reloaded");

        if (player == null)
            Console.WriteLine($"[BlockerPasses] {msg}");
        else
            player.PrintToChat($" {ReplaceColorTags("{GREEN}[BlockerPasses] " + msg)}");
    }

    // Новая команда для получения позиции
    [RequiresPermissions("@css/root")]
    [ConsoleCommand("css_bp_getpos")]
    public void OnCmdGetPos(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.PawnIsAlive || player.PlayerPawn.Value == null)
        {
            var errorMessage = GetTranslation("must_be_alive");
            if (player == null)
                Console.WriteLine($"[BlockerPasses] {errorMessage}");
            else
                player.PrintToChat($" {ReplaceColorTags("{RED}[BlockerPasses] " + errorMessage)}");
            return;
        }

        var pawn = player.PlayerPawn.Value;
        var origin = pawn.AbsOrigin!;
        var angles = pawn.AbsRotation!;

        // Форматируем координаты для конфига
        var originStr = $"{origin.X.ToString("F2", CultureInfo.InvariantCulture)} " +
                       $"{origin.Y.ToString("F2", CultureInfo.InvariantCulture)} " +
                       $"{origin.Z.ToString("F2", CultureInfo.InvariantCulture)}";

        var anglesStr = $"{angles.X.ToString("F2", CultureInfo.InvariantCulture)} " +
                       $"{angles.Y.ToString("F2", CultureInfo.InvariantCulture)} " +
                       $"{angles.Z.ToString("F2", CultureInfo.InvariantCulture)}";

        // Создаем шаблон для добавления блока
        var template = $@"
{{
    ""ModelPath"": ""models/props/de_dust/hr_dust/dust_windows/dust_rollupdoor_96x128_surface_lod.vmdl"",
    ""Color"": [255, 255, 255],
    ""Origin"": ""{originStr}"",
    ""Angles"": ""{anglesStr}"",
    ""Scale"": 1.0,
    ""Invisibility"": 255,
    ""Quota"": 0,
    ""Name"": ""Block_{Server.MapName}_{DateTime.Now:HHmmss}""
}}";

        // Выводим в консоль и чат
        var message = GetTranslation("position_info", originStr, anglesStr);
        
        player.PrintToChat($" {ReplaceColorTags("{BLUE}[BlockerPasses] " + message)}");
        player.PrintToChat($" {ReplaceColorTags("{YELLOW}[BlockerPasses] Template:")}");
        player.PrintToChat($" {ReplaceColorTags("{WHITE}" + template)}");
        
        Console.WriteLine($"BP_POS: {message}");
        Console.WriteLine($"BP_TEMPLATE: {template}");
    }

    // Дополнительная команда для получения позиции с прицелом (куда смотрит игрок)
    [RequiresPermissions("@css/root")]
    [ConsoleCommand("css_bp_geteye")]
    public void OnCmdGetEye(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.PawnIsAlive || player.PlayerPawn.Value == null)
        {
            info.ReplyToCommand("You must be alive to use this command!");
            return;
        }

        var pawn = player.PlayerPawn.Value;
        var eyeAngles = pawn.EyeAngles!;

        var anglesStr = $"{eyeAngles.X.ToString("F2", CultureInfo.InvariantCulture)} " +
                       $"{eyeAngles.Y.ToString("F2", CultureInfo.InvariantCulture)} " +
                       $"{eyeAngles.Z.ToString("F2", CultureInfo.InvariantCulture)}";

        var message = $"Eye Angles: {anglesStr}";
        
        info.ReplyToCommand(message);
        player.PrintToChat($" {ReplaceColorTags("{BLUE}" + message)}");
        Console.WriteLine($"BP_EYE: {message}");
    }

    // Команда для добавления блока
    [RequiresPermissions("@css/root")]
    [ConsoleCommand("css_bp_add")]
    public void OnCmdAdd(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.PawnIsAlive || player.PlayerPawn.Value == null)
        {
            var errorMessage = GetTranslation("must_be_alive");
            if (player == null)
                Console.WriteLine($"[BlockerPasses] {errorMessage}");
            else
                player.PrintToChat($" {ReplaceColorTags("{RED}[BlockerPasses] " + errorMessage)}");
            return;
        }

        var pawn = player.PlayerPawn.Value;
        var origin = pawn.AbsOrigin!;
        var angles = pawn.AbsRotation!;

        // Параметры по умолчанию
        var invisibility = 255;
        var quota = 0;
        var scale = 1.0f;
        var color = new int[] { 255, 255, 255 };
        var modelPath = "models/props/de_dust/hr_dust/dust_windows/dust_rollupdoor_96x128_surface_lod.vmdl";

        // Парсим аргументы команды
        if (info.ArgCount >= 2)
        {
            if (int.TryParse(info.ArgByIndex(1), out var invis))
                invisibility = Math.Clamp(invis, 0, 255);
        }
        if (info.ArgCount >= 3)
        {
            if (int.TryParse(info.ArgByIndex(2), out var quotaVal))
                quota = Math.Max(0, quotaVal);
        }

        // Форматируем координаты
        var originStr = $"{origin.X.ToString("F2", CultureInfo.InvariantCulture)} " +
                       $"{origin.Y.ToString("F2", CultureInfo.InvariantCulture)} " +
                       $"{origin.Z.ToString("F2", CultureInfo.InvariantCulture)}";

        var anglesStr = $"{angles.X.ToString("F2", CultureInfo.InvariantCulture)} " +
                       $"{angles.Y.ToString("F2", CultureInfo.InvariantCulture)} " +
                       $"{angles.Z.ToString("F2", CultureInfo.InvariantCulture)}";

        // Создаем новый блок
        var newBlock = new BlockEntity
        {
            ModelPath = modelPath,
            Color = color,
            Origin = originStr,
            Angles = anglesStr,
            Scale = scale,
            Invisibility = invisibility,
            Quota = quota,
            Name = $"Block_{Server.MapName}_{DateTime.Now:HHmmss}"
        };

        // Добавляем блок в конфиг
        if (!_config.Maps.ContainsKey(Server.MapName))
        {
            _config.Maps[Server.MapName] = new List<BlockEntity>();
        }
        _config.Maps[Server.MapName].Add(newBlock);

        // Сохраняем конфиг
        SaveConfig(_config);

        var message = GetTranslation("block_added");
        player.PrintToChat($" {ReplaceColorTags("{GREEN}[BlockerPasses] " + message)}");
        Console.WriteLine($"BP_ADD: Block added to {Server.MapName} with invisibility={invisibility}, quota={quota}");
    }

    // Команда для просмотра всех блоков на карте
    [RequiresPermissions("@css/root")]
    [ConsoleCommand("css_bp_list")]
    public void OnCmdList(CCSPlayerController? player, CommandInfo info)
    {
        if (!_config.Maps.ContainsKey(Server.MapName))
        {
            var noEntitiesMessage = GetTranslation("no_entities");
            if (player == null)
                Console.WriteLine($"[BlockerPasses] {noEntitiesMessage}");
            else
                player.PrintToChat($" {ReplaceColorTags("{YELLOW}[BlockerPasses] " + noEntitiesMessage)}");
            return;
        }

        var blocks = _config.Maps[Server.MapName];
        player.PrintToChat($" {ReplaceColorTags("{BLUE}[BlockerPasses] Blocks on {Server.MapName}:")}");
        
        for (int i = 0; i < blocks.Count; i++)
        {
            var block = blocks[i];
            var blockInfo = $"#{i + 1}: {block.Name ?? $"Block_{i + 1}"} | Invisibility: {block.Invisibility} | Quota: {block.Quota}";
            player.PrintToChat($" {ReplaceColorTags("{WHITE}" + blockInfo)}");
        }
        
        Console.WriteLine($"BP_LIST: Listed {blocks.Count} blocks for {Server.MapName}");
    }

    // Команда для удаления всех блоков на карте
    [RequiresPermissions("@css/root")]
    [ConsoleCommand("css_bp_removeall")]
    public void OnCmdRemoveAll(CCSPlayerController? player, CommandInfo info)
    {
        if (!_config.Maps.ContainsKey(Server.MapName))
        {
            var noEntitiesMessage = GetTranslation("no_entities");
            if (player == null)
                Console.WriteLine($"[BlockerPasses] {noEntitiesMessage}");
            else
                player.PrintToChat($" {ReplaceColorTags("{YELLOW}[BlockerPasses] " + noEntitiesMessage)}");
            return;
        }

        var count = _config.Maps[Server.MapName].Count;
        _config.Maps[Server.MapName].Clear();
        
        // Сохраняем конфиг
        SaveConfig(_config);

        var message = GetTranslation("block_removed");
        player.PrintToChat($" {ReplaceColorTags("{GREEN}[BlockerPasses] " + message)} ({count} blocks)");
        Console.WriteLine($"BP_REMOVEALL: Removed {count} blocks from {Server.MapName}");
    }

    // Команда для предварительного просмотра блоков
    [RequiresPermissions("@css/root")]
    [ConsoleCommand("css_bp_preview")]
    public void OnCmdPreview(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null)
        {
            Console.WriteLine("[BlockerPasses] Preview command can only be used by players!");
            return;
        }

        // Переключаем режим предпросмотра (пока что просто сообщение)
        var message = GetTranslation("preview_mode");
        player.PrintToChat($" {ReplaceColorTags("{CYAN}[BlockerPasses] " + message)}");
        Console.WriteLine($"BP_PREVIEW: Preview mode toggled for {player.PlayerName}");
    }

    // Команда для создания текстуры
    [RequiresPermissions("@css/root")]
    [ConsoleCommand("css_bp_createtexture")]
    public void OnCmdCreateTexture(CCSPlayerController? player, CommandInfo info)
    {
        if (info.ArgCount < 3)
        {
            var message = GetTranslation("texture_create_usage");
            if (player == null)
                Console.WriteLine($"[BlockerPasses] {message}");
            else
                player.PrintToChat($" {ReplaceColorTags("{RED}[BlockerPasses] " + message)}");
            return;
        }

        var textureName = info.ArgByIndex(1);
        var displayName = info.ArgByIndex(2);
        var texturePath = info.ArgCount >= 4 ? info.ArgByIndex(3) : null;
        var category = info.ArgCount >= 5 ? info.ArgByIndex(4) : "custom";

        // Создаем новую текстуру
        var newTexture = new TextureEntity
        {
            Name = textureName,
            DisplayName = displayName,
            TexturePath = texturePath,
            BaseColor = new[] { 255, 255, 255 },
            Description = $"Custom texture: {displayName}",
            IsCustom = true,
            Category = category
        };

        // Добавляем текстуру в конфиг
        var newConfig = _config with 
        { 
            AvailableTextures = new Dictionary<string, TextureEntity>(_config.AvailableTextures) 
            { 
                [textureName] = newTexture 
            } 
        };
        _config = newConfig;

        // Сохраняем конфиг
        SaveConfig(_config);

        var successMessage = GetTranslation("texture_created", textureName);
        if (player == null)
            Console.WriteLine($"[BlockerPasses] {successMessage}");
        else
            player.PrintToChat($" {ReplaceColorTags("{GREEN}[BlockerPasses] " + successMessage)}");
    }

    // Команда для применения текстуры к блоку
    [RequiresPermissions("@css/root")]
    [ConsoleCommand("css_bp_applytexture")]
    public void OnCmdApplyTexture(CCSPlayerController? player, CommandInfo info)
    {
        if (info.ArgCount < 3)
        {
            var message = GetTranslation("texture_apply_usage");
            if (player == null)
                Console.WriteLine($"[BlockerPasses] {message}");
            else
                player.PrintToChat($" {ReplaceColorTags("{RED}[BlockerPasses] " + message)}");
            return;
        }

        if (!int.TryParse(info.ArgByIndex(1), out var blockIndex))
        {
            var message = GetTranslation("invalid_block_index");
            if (player == null)
                Console.WriteLine($"[BlockerPasses] {message}");
            else
                player.PrintToChat($" {ReplaceColorTags("{RED}[BlockerPasses] " + message)}");
            return;
        }

        var textureName = info.ArgByIndex(2);

        if (!_config.AvailableTextures.TryGetValue(textureName, out var texture))
        {
            var message = GetTranslation("texture_not_found", textureName);
            if (player == null)
                Console.WriteLine($"[BlockerPasses] {message}");
            else
                player.PrintToChat($" {ReplaceColorTags("{RED}[BlockerPasses] " + message)}");
            return;
        }

        if (!_config.Maps.ContainsKey(Server.MapName) || blockIndex < 1 || blockIndex > _config.Maps[Server.MapName].Count)
        {
            var message = GetTranslation("invalid_block_index");
            if (player == null)
                Console.WriteLine($"[BlockerPasses] {message}");
            else
                player.PrintToChat($" {ReplaceColorTags("{RED}[BlockerPasses] " + message)}");
            return;
        }

        // Применяем текстуру к блоку
        var blocks = _config.Maps[Server.MapName].ToList();
        var block = blocks[blockIndex - 1];
        
        var textureSettings = new TextureSettings
        {
            TextureName = textureName,
            TextureColor = texture.BaseColor,
            TextureScale = 1.0f,
            UseCustomTexture = texture.IsCustom,
            CustomTexturePath = texture.TexturePath
        };

        var updatedBlock = block with 
        { 
            TextureSettings = textureSettings,
            TexturePath = texture.TexturePath
        };

        blocks[blockIndex - 1] = updatedBlock;

        var newConfig = _config with 
        { 
            Maps = new Dictionary<string, List<BlockEntity>>(_config.Maps) 
            { 
                [Server.MapName] = blocks 
            } 
        };
        _config = newConfig;

        // Сохраняем конфиг
        SaveConfig(_config);

        var successMessage = GetTranslation("texture_applied", textureName, blockIndex);
        if (player == null)
            Console.WriteLine($"[BlockerPasses] {successMessage}");
        else
            player.PrintToChat($" {ReplaceColorTags("{GREEN}[BlockerPasses] " + successMessage)}");
    }

    // Команда для просмотра доступных текстур
    [RequiresPermissions("@css/root")]
    [ConsoleCommand("css_bp_textures")]
    public void OnCmdListTextures(CCSPlayerController? player, CommandInfo info)
    {
        if (_config.AvailableTextures.Count == 0)
        {
            var noTexturesMessage = GetTranslation("no_textures_available");
            if (player == null)
                Console.WriteLine($"[BlockerPasses] {noTexturesMessage}");
            else
                player.PrintToChat($" {ReplaceColorTags("{YELLOW}[BlockerPasses] " + noTexturesMessage)}");
            return;
        }

        var texturesMessage = GetTranslation("available_textures");
        if (player == null)
            Console.WriteLine($"[BlockerPasses] {texturesMessage}");
        else
            player.PrintToChat($" {ReplaceColorTags("{BLUE}[BlockerPasses] " + texturesMessage)}");

        foreach (var texture in _config.AvailableTextures.Values)
        {
            var textureInfo = $"• {texture.Name}: {texture.DisplayName} ({texture.Category})";
            if (player == null)
                Console.WriteLine($"[BlockerPasses] {textureInfo}");
            else
                player.PrintToChat($" {ReplaceColorTags("{WHITE}" + textureInfo)}");
        }
    }

    // Команда для открытия меню управления блокировщиками
    [RequiresPermissions("@css/root")]
    [ConsoleCommand("css_bp_menu")]
    [ConsoleCommand("css_bp")]
    public void OnCmdMenu(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null)
        {
            info.ReplyToCommand("This command can only be used by players!");
            return;
        }

        if (_menuManagerAvailable)
        {
            OpenBlockerPassesMenuManager(player);
        }
        else
        {
            // Fallback to native menu if MenuManager is not available
            OpenBlockerPassesMenu(player);
        }
    }

    private void InitializeTranslations()
    {
        _translations = new Dictionary<string, Dictionary<string, string>>
        {
            ["en"] = new Dictionary<string, string>
            {
                ["menu_title"] = "BlockerPasses Management",
                ["reload_config"] = "Reload Config",
                ["get_position"] = "Get Position",
                ["get_eye_angles"] = "Get Eye Angles",
                ["current_settings"] = "Current Settings",
                ["map_entities"] = "Map Entities",
                ["back"] = "Back",
                ["config_reloaded"] = "Configuration reloaded!",
                ["must_be_alive"] = "You must be alive to get position!",
                ["position_info"] = "Position: {0} | Angles: {1}",
                ["eye_angles_info"] = "Eye Angles: {0}",
                ["settings_info"] = "Min Players: {0} | Map: {1}",
                ["no_entities"] = "No entities configured for this map",
                ["entity_info"] = "Model: {0}\nOrigin: {1}\nAngles: {2}\nColor: {3}\nScale: {4}\nInvisibility: {5}\nQuota: {6}",
                ["language_changed"] = "Language changed to English",
                ["invalid_language"] = "Invalid language. Available: en, ru",
                ["block_added"] = "Block added successfully",
                ["block_removed"] = "All blocks removed",
                ["preview_mode"] = "Preview mode enabled",
                ["preview_disabled"] = "Preview mode disabled",
                ["texture_create_usage"] = "Usage: css_bp_createtexture <name> <display_name> [texture_path] [category]",
                ["texture_apply_usage"] = "Usage: css_bp_applytexture <block_index> <texture_name>",
                ["texture_created"] = "Texture '{0}' created successfully",
                ["texture_applied"] = "Texture '{0}' applied to block #{1}",
                ["texture_not_found"] = "Texture '{0}' not found",
                ["invalid_block_index"] = "Invalid block index",
                ["no_textures_available"] = "No textures available",
                ["available_textures"] = "Available textures:",
                ["texture_management"] = "Texture Management",
                ["create_texture"] = "Create Texture",
                ["apply_texture"] = "Apply Texture",
                ["list_textures"] = "List Textures"
            },
            ["ru"] = new Dictionary<string, string>
            {
                ["menu_title"] = "Управление BlockerPasses",
                ["reload_config"] = "Перезагрузить конфиг",
                ["get_position"] = "Получить позицию",
                ["get_eye_angles"] = "Получить углы взгляда",
                ["current_settings"] = "Текущие настройки",
                ["map_entities"] = "Сущности карты",
                ["back"] = "Назад",
                ["config_reloaded"] = "Конфигурация перезагружена!",
                ["must_be_alive"] = "Вы должны быть живы для получения позиции!",
                ["position_info"] = "Позиция: {0} | Углы: {1}",
                ["eye_angles_info"] = "Углы взгляда: {0}",
                ["settings_info"] = "Мин. игроков: {0} | Карта: {1}",
                ["no_entities"] = "Для этой карты не настроены сущности",
                ["entity_info"] = "Модель: {0}\nПозиция: {1}\nУглы: {2}\nЦвет: {3}\nМасштаб: {4}\nПрозрачность: {5}\nЛимит: {6}",
                ["language_changed"] = "Язык изменен на русский",
                ["invalid_language"] = "Неверный язык. Доступно: en, ru",
                ["block_added"] = "Блок успешно добавлен",
                ["block_removed"] = "Все блоки удалены",
                ["preview_mode"] = "Режим предпросмотра включен",
                ["preview_disabled"] = "Режим предпросмотра отключен",
                ["texture_create_usage"] = "Использование: css_bp_createtexture <имя> <отображаемое_имя> [путь_к_текстуре] [категория]",
                ["texture_apply_usage"] = "Использование: css_bp_applytexture <индекс_блока> <имя_текстуры>",
                ["texture_created"] = "Текстура '{0}' успешно создана",
                ["texture_applied"] = "Текстура '{0}' применена к блоку #{1}",
                ["texture_not_found"] = "Текстура '{0}' не найдена",
                ["invalid_block_index"] = "Неверный индекс блока",
                ["no_textures_available"] = "Нет доступных текстур",
                ["available_textures"] = "Доступные текстуры:",
                ["texture_management"] = "Управление текстурами",
                ["create_texture"] = "Создать текстуру",
                ["apply_texture"] = "Применить текстуру",
                ["list_textures"] = "Список текстур"
            }
        };
    }

    private string GetTranslation(string key, params object[] args)
    {
        var currentLang = _config.Language.CurrentLanguage;
        if (!_translations.ContainsKey(currentLang))
            currentLang = "en";

        if (_translations[currentLang].TryGetValue(key, out var translation))
        {
            return args.Length > 0 ? string.Format(translation, args) : translation;
        }

        // Fallback to English
        if (_translations["en"].TryGetValue(key, out var englishTranslation))
        {
            return args.Length > 0 ? string.Format(englishTranslation, args) : englishTranslation;
        }

        return key; // Return key if no translation found
    }

    private HookResult EventRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        var playersCount = Utilities.GetPlayers()
            .Where(u => u.PlayerPawn.Value != null && u.TeamNum != (int)CsTeam.None &&
                        u.TeamNum != (int)CsTeam.Spectator && u.PlayerPawn.Value.IsValid).ToList();

        if (playersCount.Count >= _config.Players) return HookResult.Continue;

        if (!_config.Maps.TryGetValue(Server.MapName, out var entitiesMap)) return HookResult.Continue;

        foreach (var entity in entitiesMap)
        {
            var color = entity.Color;
            
            // Если у блока есть настройки текстуры, используем их цвет
            if (entity.TextureSettings != null)
            {
                color = entity.TextureSettings.TextureColor;
            }

            SpawnProp(entity.ModelPath, new[] { color[0], color[1], color[2] },
                GetVectorFromString(entity.Origin), GetQAngleFromString(entity.Angles), entity.Scale, entity.TextureSettings);
        }

        Server.PrintToChatAll(
            " " + ReplaceColorTags(_config.Message.Replace("{MINPLAYERS}", _config.Players.ToString())));

        return HookResult.Continue;
    }

    private Vector GetVectorFromString(string vector) => GetFromString(vector, (x, y, z) => new Vector(x, y, z));

    private QAngle GetQAngleFromString(string angles) => GetFromString(angles, (x, y, z) => new QAngle(x, y, z));

    private static T GetFromString<T>(string values, Func<float, float, float, T> createInstance)
    {
        var split = values.Split(' ');

        if (split.Length >= 3 &&
            float.TryParse(split[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x) &&
            float.TryParse(split[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y) &&
            float.TryParse(split[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var z))
        {
            return createInstance(x, y, z);
        }

        return default!;
    }

    private void SpawnProp(string modelPath, int[] color, Vector origin, QAngle angles, float? entityScale, TextureSettings? textureSettings = null)
    {
        var prop = Utilities.CreateEntityByName<CBaseModelEntity>("prop_dynamic_override");

        if (prop == null) return;

        prop.Collision.SolidType = SolidType_t.SOLID_VPHYSICS;
        prop.Render = Color.FromArgb(color[0], color[1], color[2]);
        prop.Teleport(origin, angles, new Vector(0, 0, 0));
        prop.DispatchSpawn();
        Server.NextFrame(() => prop.SetModel(modelPath));

        var bodyComponent = prop.CBodyComponent;
        if (bodyComponent is not { SceneNode: not null }) return;

        if (entityScale != null && entityScale != 0.0f)
            bodyComponent.SceneNode.GetSkeletonInstance().Scale = entityScale.Value;

        // Применяем настройки текстуры, если они есть
        if (textureSettings != null)
        {
            ApplyTextureToProp(prop, textureSettings);
        }
    }

    private void ApplyTextureToProp(CBaseModelEntity prop, TextureSettings textureSettings)
    {
        // Здесь можно добавить логику применения текстуры к пропу
        // В CS2 это может включать изменение материала или применение пользовательской текстуры
        
        // Для паттерна 2x2 можно создать специальный эффект
        if (textureSettings.TextureName == "2x2_pattern")
        {
            // Применяем специальный эффект для паттерна 2x2
            // Это может включать изменение цвета или создание визуального эффекта
            var patternColor = Color.FromArgb(200, 200, 200); // Слегка серый для паттерна
            prop.Render = patternColor;
        }
        
        // Если используется пользовательская текстура
        if (textureSettings.UseCustomTexture && !string.IsNullOrEmpty(textureSettings.CustomTexturePath))
        {
            // Здесь можно добавить логику загрузки и применения пользовательской текстуры
            // Пока что просто логируем
            Logger.LogInformation($"Applying custom texture: {textureSettings.CustomTexturePath}");
        }
    }

    private Config LoadConfig()
    {
        var configPath = Path.Combine(ModuleDirectory, "blocker_passes.json");
        if (!File.Exists(configPath)) return CreateConfig(configPath);

        var config = JsonSerializer.Deserialize<Config>(File.ReadAllText(configPath))!;

        return config;
    }

    private void SaveConfig(Config config)
    {
        var configPath = Path.Combine(ModuleDirectory, "blocker_passes.json");
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        
        File.WriteAllText(configPath, JsonSerializer.Serialize(config, jsonOptions));
    }

    private Config CreateConfig(string configPath)
    {
        var config = new Config
        {
            Players = 6,
            Message =
                "[{BLUE} BlockerPasses {DEFAULT}] Some passageways are blocked. Unblocking requires {RED}{MINPLAYERS}{DEFAULT} players",
            Menu = new MenuSettings
            {
                EnableMenu = true,
                MenuTitle = "BlockerPasses Management",
                ShowEntityDetails = true,
                EnablePositionCommands = true
            },
            Language = new LanguageSettings
            {
                CurrentLanguage = "en",
                Translations = new Dictionary<string, Dictionary<string, string>>()
            },
            AvailableTextures = new Dictionary<string, TextureEntity>
            {
                ["white_block"] = new TextureEntity
                {
                    Name = "white_block",
                    DisplayName = "White Block",
                    TexturePath = null,
                    BaseColor = new[] { 255, 255, 255 },
                    Description = "Classic white block texture",
                    IsCustom = false,
                    Category = "basic"
                },
                ["blue_block"] = new TextureEntity
                {
                    Name = "blue_block",
                    DisplayName = "Blue Block",
                    TexturePath = null,
                    BaseColor = new[] { 30, 144, 255 },
                    Description = "Blue colored block",
                    IsCustom = false,
                    Category = "basic"
                },
                ["red_block"] = new TextureEntity
                {
                    Name = "red_block",
                    DisplayName = "Red Block",
                    TexturePath = null,
                    BaseColor = new[] { 255, 0, 0 },
                    Description = "Red colored block",
                    IsCustom = false,
                    Category = "basic"
                },
                ["green_block"] = new TextureEntity
                {
                    Name = "green_block",
                    DisplayName = "Green Block",
                    TexturePath = null,
                    BaseColor = new[] { 0, 255, 0 },
                    Description = "Green colored block",
                    IsCustom = false,
                    Category = "basic"
                },
                ["2x2_pattern"] = new TextureEntity
                {
                    Name = "2x2_pattern",
                    DisplayName = "2x2 Pattern",
                    TexturePath = null,
                    BaseColor = new[] { 255, 255, 255 },
                    Description = "2x2 checkerboard pattern like in competitive mode",
                    IsCustom = false,
                    Category = "patterns"
                }
            },
            DefaultTextureSettings = new TextureSettings
            {
                TextureName = "white_block",
                TextureColor = new[] { 255, 255, 255 },
                TextureScale = 1.0f,
                TextureOffsetX = 0.0f,
                TextureOffsetY = 0.0f,
                TextureRotation = 0.0f,
                UseCustomTexture = false,
                CustomTexturePath = null
            },
            Maps = new Dictionary<string, List<BlockEntity>>
            {
                {
                    "de_mirage", new List<BlockEntity>
                    {
                        new()
                        {
                            ModelPath =
                                "models/props/de_dust/hr_dust/dust_windows/dust_rollupdoor_96x128_surface_lod.vmdl",
                            Color = new[] { 30, 144, 255 },
                            Origin = "-1600.46 -741.124 -172.965",
                            Angles = "0 180 0",
                            Scale = 0.0f,
                            Invisibility = 255,
                            Quota = 0,
                            Name = "Mirage_Block_1"
                        },
                        new()
                        {
                            ModelPath = "models/props/de_mirage/small_door_b.vmdl",
                            Color = new[] { 255, 255, 255 },
                            Origin = "588.428 704.941 -136.517",
                            Angles = "0 270.256 0",
                            Scale = 0.0f,
                            Invisibility = 255,
                            Quota = 0,
                            Name = "Mirage_Block_2"
                        },
                        new()
                        {
                            ModelPath = "models/props/de_mirage/large_door_c.vmdl",
                            Color = new[] { 255, 255, 255 },
                            Origin = "-1007.87 -359.812 -323.64",
                            Angles = "0 270.106 0",
                            Scale = 0.0f
                        },
                        new()
                        {
                            ModelPath =
                                "models/props/de_nuke/hr_nuke/chainlink_fence_001/chainlink_fence_001_256_capped.vmdl",
                            Color = new[] { 255, 255, 255 },
                            Origin = "-961.146 -14.2419 -169.489",
                            Angles = "0 269.966 0",
                            Scale = 0.0f
                        },
                        new()
                        {
                            ModelPath =
                                "models/props/de_nuke/hr_nuke/chainlink_fence_001/chainlink_fence_001_256_capped.vmdl",
                            Color = new[] { 255, 255, 255 },
                            Origin = "-961.146 -14.2419 -43.0083",
                            Angles = "0 269.966 0",
                            Scale = 0.0f
                        }
                    }
                }
            }
        };

        File.WriteAllText(configPath,
            JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));

        return config;
    }

    private string ReplaceColorTags(string input)
    {
        string[] colorPatterns =
        {
            "{DEFAULT}", "{WHITE}", "{DARKRED}", "{GREEN}", "{LIGHTYELLOW}", "{LIGHTBLUE}", "{OLIVE}", "{LIME}",
            "{RED}", "{LIGHTPURPLE}", "{PURPLE}", "{GREY}", "{YELLOW}", "{GOLD}", "{SILVER}", "{BLUE}", "{DARKBLUE}",
            "{BLUEGREY}", "{MAGENTA}", "{LIGHTRED}", "{ORANGE}"
        };

        string[] colorReplacements =
        {
            $"{ChatColors.Default}", $"{ChatColors.White}", $"{(char)2}", $"{ChatColors.Green}",
            $"{ChatColors.LightYellow}", $"{ChatColors.LightBlue}", $"{ChatColors.Olive}", $"{ChatColors.Lime}",
            $"{ChatColors.Red}", $"{ChatColors.LightPurple}", $"{ChatColors.Purple}", $"{ChatColors.Grey}",
            $"{ChatColors.Yellow}", $"{ChatColors.Gold}", $"{ChatColors.Silver}", $"{ChatColors.Blue}",
            $"{(char)12}", $"{(char)13}", $"{(char)14}", $"{(char)15}", $"{ChatColors.Orange}"
        };

        for (var i = 0; i < colorPatterns.Length; i++)
            input = input.Replace(colorPatterns[i], colorReplacements[i]);

        return input;
    }

    private void OpenBlockerPassesMenu(CCSPlayerController player)
    {
        if (!_config.Menu.EnableMenu)
        {
            player.PrintToChat($" {ReplaceColorTags("{RED}[BlockerPasses] Menu is disabled in configuration!")}");
            return;
        }

        var menu = new ChatMenu(_config.Menu.MenuTitle);
        
        // Основные опции меню
        menu.AddMenuOption("Reload Config", (player, option) => {
            _config = LoadConfig();
            player.PrintToChat($" {ReplaceColorTags("{GREEN}[BlockerPasses] Configuration reloaded!")}");
        });

        if (_config.Menu.EnablePositionCommands)
        {
            menu.AddMenuOption("Get Position", (player, option) => {
                if (!player.PawnIsAlive || player.PlayerPawn.Value == null)
                {
                    player.PrintToChat($" {ReplaceColorTags("{RED}[BlockerPasses] You must be alive to use this!")}");
                    return;
                }

                var pawn = player.PlayerPawn.Value;
                var origin = pawn.AbsOrigin!;
                var angles = pawn.AbsRotation!;

                var originStr = $"{origin.X.ToString("F2", CultureInfo.InvariantCulture)} " +
                               $"{origin.Y.ToString("F2", CultureInfo.InvariantCulture)} " +
                               $"{origin.Z.ToString("F2", CultureInfo.InvariantCulture)}";

                var anglesStr = $"{angles.X.ToString("F2", CultureInfo.InvariantCulture)} " +
                               $"{angles.Y.ToString("F2", CultureInfo.InvariantCulture)} " +
                               $"{angles.Z.ToString("F2", CultureInfo.InvariantCulture)}";

                var message = $"Position: {originStr} | Angles: {anglesStr}";
                player.PrintToChat($" {ReplaceColorTags("{GREEN}[BlockerPasses] " + message)}");
                Console.WriteLine($"BP_POS: {message}");
            });

            menu.AddMenuOption("Get Eye Angles", (player, option) => {
                if (!player.PawnIsAlive || player.PlayerPawn.Value == null)
                {
                    player.PrintToChat($" {ReplaceColorTags("{RED}[BlockerPasses] You must be alive to use this!")}");
                    return;
                }

                var pawn = player.PlayerPawn.Value;
                var eyeAngles = pawn.EyeAngles!;

                var anglesStr = $"{eyeAngles.X.ToString("F2", CultureInfo.InvariantCulture)} " +
                               $"{eyeAngles.Y.ToString("F2", CultureInfo.InvariantCulture)} " +
                               $"{eyeAngles.Z.ToString("F2", CultureInfo.InvariantCulture)}";

                var message = $"Eye Angles: {anglesStr}";
                player.PrintToChat($" {ReplaceColorTags("{BLUE}[BlockerPasses] " + message)}");
                Console.WriteLine($"BP_EYE: {message}");
            });
        }

        menu.AddMenuOption("Current Settings", (player, option) => {
            var message = $"Min Players: {_config.Players} | Current Map: {Server.MapName}";
            player.PrintToChat($" {ReplaceColorTags("{YELLOW}[BlockerPasses] " + message)}");
        });

        menu.AddMenuOption("Map Entities", (player, option) => {
            OpenMapEntitiesMenu(player);
        });

        menu.AddMenuOption(GetTranslation("texture_management"), (player, option) => {
            OpenTextureManagementMenu(player);
        });

        menu.Open(player);
    }

    private void OpenMapEntitiesMenu(CCSPlayerController player)
    {
        var menu = new ChatMenu($"Entities for {Server.MapName}");

        if (_config.Maps.TryGetValue(Server.MapName, out var entities))
        {
            for (int i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];
                var entityName = $"Entity {i + 1}";
                
                menu.AddMenuOption(entityName, (player, option) => {
                    if (_config.Menu.ShowEntityDetails)
                    {
                        var info = $"Model: {entity.ModelPath}\n" +
                                  $"Color: RGB({entity.Color[0]}, {entity.Color[1]}, {entity.Color[2]})\n" +
                                  $"Position: {entity.Origin}\n" +
                                  $"Angles: {entity.Angles}\n" +
                                  $"Scale: {entity.Scale}";
                        
                        player.PrintToChat($" {ReplaceColorTags("{CYAN}[BlockerPasses] Entity Info:")}");
                        player.PrintToChat($" {ReplaceColorTags("{WHITE}" + info)}");
                    }
                    else
                    {
                        player.PrintToChat($" {ReplaceColorTags("{YELLOW}[BlockerPasses] Entity details are disabled in configuration!")}");
                    }
                });
            }
        }
        else
        {
            menu.AddMenuOption("No entities for this map", (player, option) => { });
        }

        menu.AddMenuOption("Back to Main Menu", (player, option) => {
            OpenBlockerPassesMenu(player);
        });

        menu.Open(player);
    }

    private void OpenBlockerPassesMenuManager(CCSPlayerController player)
    {
        if (!_config.Menu.EnableMenu)
        {
            player.PrintToChat($" {ReplaceColorTags("{RED}[BlockerPasses] Menu is disabled in configuration!")}");
            return;
        }

        // Пока что используем встроенные меню, но с улучшенным интерфейсом
        var menu = new ChatMenu($"🎯 {_config.Menu.MenuTitle}");
        
        // Основные опции меню
        menu.AddMenuOption("🔄 Reload Config", (player, option) => {
            _config = LoadConfig();
            player.PrintToChat($" {ReplaceColorTags("{GREEN}[BlockerPasses] Configuration reloaded!")}");
        });

        if (_config.Menu.EnablePositionCommands)
        {
            menu.AddMenuOption("📍 Get Position", (player, option) => {
                if (!player.PawnIsAlive || player.PlayerPawn.Value == null)
                {
                    player.PrintToChat($" {ReplaceColorTags("{RED}[BlockerPasses] You must be alive to get position!")}");
                    return;
                }

                var pawn = player.PlayerPawn.Value;
                var pos = pawn.AbsOrigin;
                var angles = pawn.AbsRotation;
                
                var message = $"Position: {pos.X:F2}, {pos.Y:F2}, {pos.Z:F2} | Angles: {angles.X:F2}, {angles.Y:F2}, {angles.Z:F2}";
                player.PrintToChat($" {ReplaceColorTags("{BLUE}[BlockerPasses] " + message)}");
                Console.WriteLine($"BP_POS: {message}");
            });

            menu.AddMenuOption("👁️ Get Eye Angles", (player, option) => {
                if (!player.PawnIsAlive || player.PlayerPawn.Value == null)
                {
                    player.PrintToChat($" {ReplaceColorTags("{RED}[BlockerPasses] You must be alive to get eye angles!")}");
                    return;
                }

                var pawn = player.PlayerPawn.Value;
                var eyeAngles = pawn.EyeAngles;
                
                var message = $"Eye Angles: {eyeAngles.X:F2}, {eyeAngles.Y:F2}, {eyeAngles.Z:F2}";
                player.PrintToChat($" {ReplaceColorTags("{BLUE}[BlockerPasses] " + message)}");
                Console.WriteLine($"BP_EYE: {message}");
            });
        }

        menu.AddMenuOption("⚙️ Current Settings", (player, option) => {
            var message = $"Min Players: {_config.Players} | Map: {Server.MapName}";
            player.PrintToChat($" {ReplaceColorTags("{BLUE}[BlockerPasses] " + message)}");
        });

        menu.AddMenuOption("🗺️ Map Entities", (player, option) => {
            OpenMapEntitiesMenuManager(player);
        });

        menu.AddMenuOption("🎨 " + GetTranslation("texture_management"), (player, option) => {
            OpenTextureManagementMenuManager(player);
        });

        menu.Open(player);
    }

    private void OpenMapEntitiesMenuManager(CCSPlayerController player)
    {
        var menu = new ChatMenu($"🗺️ Entities for {Server.MapName}");

        if (_config.Maps.TryGetValue(Server.MapName, out var entities))
        {
            for (int i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];
                var entityName = $"🔹 Entity {i + 1}";
                
                menu.AddMenuOption(entityName, (player, option) => {
                    if (_config.Menu.ShowEntityDetails)
                    {
                        var info = $"Model: {entity.ModelPath}\n" +
                                 $"Origin: {entity.Origin}\n" +
                                 $"Angles: {entity.Angles}\n" +
                                 $"Color: {string.Join(", ", entity.Color)}\n" +
                                 $"Scale: {entity.Scale?.ToString() ?? "Default"}";
                        
                        player.PrintToChat($" {ReplaceColorTags("{BLUE}[BlockerPasses] " + info)}");
                    }
                });
            }
        }
        else
        {
            menu.AddMenuOption("❌ No entities configured for this map", (player, option) => {});
        }

        menu.AddMenuOption("🔙 Back to Main Menu", (player, option) => {
            OpenBlockerPassesMenuManager(player);
        });

        menu.Open(player);
    }

    private void OpenTextureManagementMenu(CCSPlayerController player)
    {
        var menu = new ChatMenu(GetTranslation("texture_management"));

        menu.AddMenuOption(GetTranslation("create_texture"), (player, option) => {
            player.PrintToChat($" {ReplaceColorTags("{YELLOW}[BlockerPasses] " + GetTranslation("texture_create_usage"))}");
        });

        menu.AddMenuOption(GetTranslation("apply_texture"), (player, option) => {
            player.PrintToChat($" {ReplaceColorTags("{YELLOW}[BlockerPasses] " + GetTranslation("texture_apply_usage"))}");
        });

        menu.AddMenuOption(GetTranslation("list_textures"), (player, option) => {
            OnCmdListTextures(player, null!);
        });

        menu.AddMenuOption(GetTranslation("back"), (player, option) => {
            OpenBlockerPassesMenu(player);
        });

        menu.Open(player);
    }

    private void OpenTextureManagementMenuManager(CCSPlayerController player)
    {
        var menu = new ChatMenu($"🎨 {GetTranslation("texture_management")}");

        menu.AddMenuOption("✨ " + GetTranslation("create_texture"), (player, option) => {
            player.PrintToChat($" {ReplaceColorTags("{YELLOW}[BlockerPasses] " + GetTranslation("texture_create_usage"))}");
        });

        menu.AddMenuOption("🎯 " + GetTranslation("apply_texture"), (player, option) => {
            player.PrintToChat($" {ReplaceColorTags("{YELLOW}[BlockerPasses] " + GetTranslation("texture_apply_usage"))}");
        });

        menu.AddMenuOption("📋 " + GetTranslation("list_textures"), (player, option) => {
            OnCmdListTextures(player, null!);
        });

        menu.AddMenuOption("🔙 " + GetTranslation("back"), (player, option) => {
            OpenBlockerPassesMenuManager(player);
        });

        menu.Open(player);
    }
}

public record Config
{
    public int Players { get; init; }
    public required string Message { get; init; }
    public Dictionary<string, List<BlockEntity>> Maps { get; init; } = null!;
    public MenuSettings Menu { get; init; } = new();
    public LanguageSettings Language { get; init; } = new();
    public Dictionary<string, TextureEntity> AvailableTextures { get; init; } = new();
    public TextureSettings DefaultTextureSettings { get; init; } = new();
}

public record MenuSettings
{
    public bool EnableMenu { get; init; } = true;
    public string MenuTitle { get; init; } = "BlockerPasses Management";
    public bool ShowEntityDetails { get; init; } = true;
    public bool EnablePositionCommands { get; init; } = true;
}

public record LanguageSettings
{
    public string CurrentLanguage { get; init; } = "en";
    public Dictionary<string, Dictionary<string, string>> Translations { get; init; } = new();
}

public record BlockEntity
{
    public required string ModelPath { get; init; }
    public int[] Color { get; init; } = { 255, 255, 255 };
    public required string Origin { get; init; }
    public required string Angles { get; init; }
    public float? Scale { get; init; }
    public int Invisibility { get; init; } = 255; // 0-255, где 0 = полностью прозрачный, 255 = полностью видимый
    public int Quota { get; init; } = 0; // Лимит игроков, 0 = без ограничений
    public string? Name { get; init; } // Имя блока для идентификации
    public string? TexturePath { get; init; } // Путь к текстуре (опционально)
    public TextureSettings? TextureSettings { get; init; } // Настройки текстуры
}

public record TextureSettings
{
    public string TextureName { get; init; } = "default";
    public int[] TextureColor { get; init; } = { 255, 255, 255 };
    public float TextureScale { get; init; } = 1.0f;
    public float TextureOffsetX { get; init; } = 0.0f;
    public float TextureOffsetY { get; init; } = 0.0f;
    public float TextureRotation { get; init; } = 0.0f;
    public bool UseCustomTexture { get; init; } = false;
    public string? CustomTexturePath { get; init; } // Путь к пользовательской текстуре
}

public record TextureEntity
{
    public required string Name { get; init; }
    public required string DisplayName { get; init; }
    public string? TexturePath { get; init; }
    public int[] BaseColor { get; init; } = { 255, 255, 255 };
    public string Description { get; init; } = "";
    public bool IsCustom { get; init; } = false;
    public string? Category { get; init; } = "default";
}