using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text.Json;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;
using MenuManager;

namespace BlockerPasses;

[MinimumApiVersion(90)]
public class BlockerPasses : BasePlugin
{
    public override string ModuleAuthor => "PattHs";
    public override string ModuleName => "Blocker Passes";
    public override string ModuleVersion => "v0.0.9";

    private Config _config = null!;
    private IMenuApi? _menuApi;
    private readonly PluginCapability<IMenuApi?> _pluginCapability = new("menu:nfcore");
    private Dictionary<string, Dictionary<string, string>> _translations = new();

    private void LogToFile(string message)
    {
        var logsDir = Path.Combine(ModuleDirectory, "logs");
        Directory.CreateDirectory(logsDir);
        var logPath = Path.Combine(logsDir, "plugin_log.txt");
        File.AppendAllText(logPath, $"[{DateTime.Now}] {message}\n");
    }

    public override void Load(bool hotReload)
    {
        _config = LoadConfig();
        InitializeTranslations();
        RegisterEventHandler<EventRoundStart>(EventRoundStart);
    }

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        _menuApi = _pluginCapability.Get();
        if (_menuApi == null)
        {
            Logger.LogWarning("MenuManager Core not found, falling back to native menus");
        }
        else
        {
            Logger.LogInformation("MenuManager API detected and loaded");
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
        if (lang != "en" && lang != "ru" && lang != "uk")
        {
            var message = GetTranslation("invalid_language");
            if (player == null)
                LogToFile($"[BlockerPasses] {message}");
            else
                player.PrintToChat($" {ReplaceColorTags("{RED}[BlockerPasses] " + message)}");
            return;
        }

                var newConfig = _config with { Language = _config.Language with { CurrentLanguage = lang } };
        _config = newConfig;

                InitializeTranslations();

                var successMessage = lang switch
        {
            "ru" => "Язык изменен на русский",
            "uk" => "Мову змінено на українську",
            _ => "Language changed to English"
        };
        if (player == null)
            LogToFile($"[BlockerPasses] {successMessage}");
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
            LogToFile($"[BlockerPasses] {msg}");
        else
            player.PrintToChat($" {ReplaceColorTags("{GREEN}[BlockerPasses] " + msg)}");
    }

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

                var originStr = $"{origin.X.ToString("F2", CultureInfo.InvariantCulture)} " +
                       $"{origin.Y.ToString("F2", CultureInfo.InvariantCulture)} " +
                       $"{origin.Z.ToString("F2", CultureInfo.InvariantCulture)}";

        var anglesStr = $"{angles.X.ToString("F2", CultureInfo.InvariantCulture)} " +
                       $"{angles.Y.ToString("F2", CultureInfo.InvariantCulture)} " +
                       $"{angles.Z.ToString("F2", CultureInfo.InvariantCulture)}";

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

                var message = GetTranslation("position_info", originStr, anglesStr);

        player.PrintToChat($" {ReplaceColorTags("{BLUE}[BlockerPasses] " + message)}");
        player.PrintToChat($" {ReplaceColorTags("{YELLOW}[BlockerPasses] Template:")}");
        player.PrintToChat($" {ReplaceColorTags("{WHITE}" + template)}");

        LogToFile($"BP_POS: {message}");
        LogToFile($"BP_TEMPLATE: {template}");
    }

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
        LogToFile($"BP_EYE: {message}");
    }

        [RequiresPermissions("@css/root")]
    [ConsoleCommand("css_bp_add")]
    public void OnCmdAdd(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.PawnIsAlive || player.PlayerPawn.Value == null)
        {
            var errorMessage = GetTranslation("must_be_alive");
            if (player == null)
                LogToFile($"[BlockerPasses] {errorMessage}");
            else
                player.PrintToChat($" {ReplaceColorTags("{RED}[BlockerPasses] " + errorMessage)}");
            return;
        }

        var pawn = player.PlayerPawn.Value;
        var origin = pawn.AbsOrigin!;
        var angles = pawn.AbsRotation!;

                var invisibility = 255;
        var quota = 0;
        var scale = 1.0f;
        var color = new int[] { 255, 255, 255 };
        var modelPath = "models/props/de_dust/hr_dust/dust_windows/dust_rollupdoor_96x128_surface_lod.vmdl";

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

                var originStr = $"{origin.X.ToString("F2", CultureInfo.InvariantCulture)} " +
                       $"{origin.Y.ToString("F2", CultureInfo.InvariantCulture)} " +
                       $"{origin.Z.ToString("F2", CultureInfo.InvariantCulture)}";

        var anglesStr = $"{angles.X.ToString("F2", CultureInfo.InvariantCulture)} " +
                       $"{angles.Y.ToString("F2", CultureInfo.InvariantCulture)} " +
                       $"{angles.Z.ToString("F2", CultureInfo.InvariantCulture)}";

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

                if (!_config.Maps.ContainsKey(Server.MapName))
        {
            _config.Maps[Server.MapName] = new List<BlockEntity>();
        }
        _config.Maps[Server.MapName].Add(newBlock);

                SaveConfig(_config);

        var message = GetTranslation("block_added");
        player.PrintToChat($" {ReplaceColorTags("{GREEN}[BlockerPasses] " + message)}");
        LogToFile($"BP_ADD: Block added to {Server.MapName} with invisibility={invisibility}, quota={quota}");
    }

        [RequiresPermissions("@css/root")]
    [ConsoleCommand("css_bp_list")]
    public void OnCmdList(CCSPlayerController? player, CommandInfo info)
    {
        if (!_config.Maps.ContainsKey(Server.MapName))
        {
            var noEntitiesMessage = GetTranslation("no_entities");
            if (player == null)
                LogToFile($"[BlockerPasses] {noEntitiesMessage}");
            else
                player.PrintToChat($" {ReplaceColorTags("{YELLOW}[BlockerPasses] " + noEntitiesMessage)}");
            return;
        }

        var blocks = _config.Maps[Server.MapName];

        if (player == null)
        {
            LogToFile($"[BlockerPasses] Blocks on {Server.MapName}:");
            for (int i = 0; i < blocks.Count; i++)
            {
                var block = blocks[i];
                var blockInfo = $"#{i + 1}: {block.Name ?? $"Block_{i + 1}"} | Invisibility: {block.Invisibility} | Quota: {block.Quota}";
                LogToFile($"[BlockerPasses] {blockInfo}");
            }
        }
        else
        {
            player.PrintToChat($" {ReplaceColorTags("{BLUE}[BlockerPasses] Blocks on {Server.MapName}:")}");
            for (int i = 0; i < blocks.Count; i++)
            {
                var block = blocks[i];
                var blockInfo = $"#{i + 1}: {block.Name ?? $"Block_{i + 1}"} | Invisibility: {block.Invisibility} | Quota: {block.Quota}";
                player.PrintToChat($" {ReplaceColorTags("{WHITE}" + blockInfo)}");
            }
        }

        LogToFile($"BP_LIST: Listed {blocks.Count} blocks for {Server.MapName}");
    }

        [RequiresPermissions("@css/root")]
    [ConsoleCommand("css_bp_removeall")]
    public void OnCmdRemoveAll(CCSPlayerController? player, CommandInfo info)
    {
        if (!_config.Maps.ContainsKey(Server.MapName))
        {
            var noEntitiesMessage = GetTranslation("no_entities");
            if (player == null)
                LogToFile($"[BlockerPasses] {noEntitiesMessage}");
            else
                player.PrintToChat($" {ReplaceColorTags("{YELLOW}[BlockerPasses] " + noEntitiesMessage)}");
            return;
        }

        var count = _config.Maps[Server.MapName].Count;
        _config.Maps[Server.MapName].Clear();

                 SaveConfig(_config);

                 var message = GetTranslation("block_removed");
                 if (player == null)
                 {
                     LogToFile($"[BlockerPasses] {message} ({count} blocks)");
                 }
                 else
                 {
                     player.PrintToChat($" {ReplaceColorTags("{GREEN}[BlockerPasses] " + message)} ({count} blocks)");
                 }
                 LogToFile($"BP_REMOVEALL: Removed {count} blocks from {Server.MapName}");
    }

        [RequiresPermissions("@css/root")]
    [ConsoleCommand("css_bp_preview")]
    public void OnCmdPreview(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null)
        {
            LogToFile("[BlockerPasses] Preview command can only be used by players!");
            return;
        }

                var message = GetTranslation("preview_mode");
                player.PrintToChat($" {ReplaceColorTags("{CYAN}[BlockerPasses] " + message)}");
                LogToFile($"BP_PREVIEW: Preview mode toggled for {player.PlayerName}");
    }

        [RequiresPermissions("@css/root")]
    [ConsoleCommand("css_bp_createtexture")]
    public void OnCmdCreateTexture(CCSPlayerController? player, CommandInfo info)
    {
        if (info.ArgCount < 3)
        {
            var message = GetTranslation("texture_create_usage");
            if (player == null)
                LogToFile($"[BlockerPasses] {message}");
            else
                player.PrintToChat($" {ReplaceColorTags("{RED}[BlockerPasses] " + message)}");
            return;
        }

        var textureName = info.ArgByIndex(1);
        var displayName = info.ArgByIndex(2);
        var texturePath = info.ArgCount >= 4 ? info.ArgByIndex(3) : null;
        var category = info.ArgCount >= 5 ? info.ArgByIndex(4) : "custom";

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

                var newConfig = _config with 
        { 
            AvailableTextures = new Dictionary<string, TextureEntity>(_config.AvailableTextures) 
            { 
                [textureName] = newTexture 
            } 
        };
        _config = newConfig;

                SaveConfig(_config);

        var successMessage = GetTranslation("texture_created", textureName);
        if (player == null)
            LogToFile($"[BlockerPasses] {successMessage}");
        else
            player.PrintToChat($" {ReplaceColorTags("{GREEN}[BlockerPasses] " + successMessage)}");
    }

        [RequiresPermissions("@css/root")]
    [ConsoleCommand("css_bp_applytexture")]
    public void OnCmdApplyTexture(CCSPlayerController? player, CommandInfo info)
    {
        if (info.ArgCount < 3)
        {
            var message = GetTranslation("texture_apply_usage");
            if (player == null)
                LogToFile($"[BlockerPasses] {message}");
            else
                player.PrintToChat($" {ReplaceColorTags("{RED}[BlockerPasses] " + message)}");
            return;
        }

        if (!int.TryParse(info.ArgByIndex(1), out var blockIndex))
        {
            var message = GetTranslation("invalid_block_index");
            if (player == null)
                LogToFile($"[BlockerPasses] {message}");
            else
                player.PrintToChat($" {ReplaceColorTags("{RED}[BlockerPasses] " + message)}");
            return;
        }

        var textureName = info.ArgByIndex(2);

        if (!_config.AvailableTextures.TryGetValue(textureName, out var texture))
        {
            var message = GetTranslation("texture_not_found", textureName);
            if (player == null)
                LogToFile($"[BlockerPasses] {message}");
            else
                player.PrintToChat($" {ReplaceColorTags("{RED}[BlockerPasses] " + message)}");
            return;
        }

        if (!_config.Maps.ContainsKey(Server.MapName) || blockIndex < 1 || blockIndex > _config.Maps[Server.MapName].Count)
        {
            var message = GetTranslation("invalid_block_index");
            if (player == null)
                LogToFile($"[BlockerPasses] {message}");
            else
                player.PrintToChat($" {ReplaceColorTags("{RED}[BlockerPasses] " + message)}");
            return;
        }

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

                SaveConfig(_config);

        var successMessage = GetTranslation("texture_applied", textureName, blockIndex);
        if (player == null)
            LogToFile($"[BlockerPasses] {successMessage}");
        else
            player.PrintToChat($" {ReplaceColorTags("{GREEN}[BlockerPasses] " + successMessage)}");
    }

        [RequiresPermissions("@css/root")]
    [ConsoleCommand("css_bp_setorigin")]
    public void OnCmdSetOrigin(CCSPlayerController? player, CommandInfo info)
    {
        if (info.ArgCount < 5)
        {
            var message = GetTranslation("invalid_coordinates");
            if (player == null)
                LogToFile($"[BlockerPasses] {message}");
            else
                player.PrintToChat($" {ReplaceColorTags("{RED}[BlockerPasses] " + message)}");
            return;
        }

        if (!int.TryParse(info.ArgByIndex(1), out var blockIndex))
        {
            var message = GetTranslation("invalid_block_index");
            if (player == null)
                LogToFile($"[BlockerPasses] {message}");
            else
                player.PrintToChat($" {ReplaceColorTags("{RED}[BlockerPasses] " + message)}");
            return;
        }

        if (!float.TryParse(info.ArgByIndex(2), NumberStyles.Float, CultureInfo.InvariantCulture, out var x) ||
            !float.TryParse(info.ArgByIndex(3), NumberStyles.Float, CultureInfo.InvariantCulture, out var y) ||
            !float.TryParse(info.ArgByIndex(4), NumberStyles.Float, CultureInfo.InvariantCulture, out var z))
        {
            var message = GetTranslation("invalid_coordinates");
            if (player == null)
                LogToFile($"[BlockerPasses] {message}");
            else
                player.PrintToChat($" {ReplaceColorTags("{RED}[BlockerPasses] " + message)}");
            return;
        }

        if (!_config.Maps.TryGetValue(Server.MapName, out var entitiesMap) || blockIndex < 1 || blockIndex > entitiesMap.Count)
        {
            var message = GetTranslation("invalid_block_index");
            if (player == null)
                LogToFile($"[BlockerPasses] {message}");
            else
                player.PrintToChat($" {ReplaceColorTags("{RED}[BlockerPasses] " + message)}");
            return;
        }

        var blocks = entitiesMap.ToList();
        var block = blocks[blockIndex - 1];
        var newOrigin = $"{x.ToString("F2", CultureInfo.InvariantCulture)} {y.ToString("F2", CultureInfo.InvariantCulture)} {z.ToString("F2", CultureInfo.InvariantCulture)}";

        var updatedBlock = block with { Origin = newOrigin };
        blocks[blockIndex - 1] = updatedBlock;

        var newConfig = _config with
        {
            Maps = new Dictionary<string, List<BlockEntity>>(_config.Maps)
            {
                [Server.MapName] = blocks
            }
        };
        _config = newConfig;

        SaveConfig(_config);

        var successMessage = GetTranslation("position_updated", blockIndex);
        if (player == null)
            LogToFile($"[BlockerPasses] {successMessage}");
        else
            player.PrintToChat($" {ReplaceColorTags("{GREEN}[BlockerPasses] " + successMessage)}");
    }

        [RequiresPermissions("@css/root")]
    [ConsoleCommand("css_bp_setangles")]
    public void OnCmdSetAngles(CCSPlayerController? player, CommandInfo info)
    {
        if (info.ArgCount < 5)
        {
            var message = GetTranslation("invalid_coordinates");
            if (player == null)
                LogToFile($"[BlockerPasses] {message}");
            else
                player.PrintToChat($" {ReplaceColorTags("{RED}[BlockerPasses] " + message)}");
            return;
        }

        if (!int.TryParse(info.ArgByIndex(1), out var blockIndex))
        {
            var message = GetTranslation("invalid_block_index");
            if (player == null)
                LogToFile($"[BlockerPasses] {message}");
            else
                player.PrintToChat($" {ReplaceColorTags("{RED}[BlockerPasses] " + message)}");
            return;
        }

        if (!float.TryParse(info.ArgByIndex(2), NumberStyles.Float, CultureInfo.InvariantCulture, out var x) ||
            !float.TryParse(info.ArgByIndex(3), NumberStyles.Float, CultureInfo.InvariantCulture, out var y) ||
            !float.TryParse(info.ArgByIndex(4), NumberStyles.Float, CultureInfo.InvariantCulture, out var z))
        {
            var message = GetTranslation("invalid_coordinates");
            if (player == null)
                LogToFile($"[BlockerPasses] {message}");
            else
                player.PrintToChat($" {ReplaceColorTags("{RED}[BlockerPasses] " + message)}");
            return;
        }

        if (!_config.Maps.TryGetValue(Server.MapName, out var entitiesMap) || blockIndex < 1 || blockIndex > entitiesMap.Count)
        {
            var message = GetTranslation("invalid_block_index");
            if (player == null)
                LogToFile($"[BlockerPasses] {message}");
            else
                player.PrintToChat($" {ReplaceColorTags("{RED}[BlockerPasses] " + message)}");
            return;
        }

        var blocks = entitiesMap.ToList();
        var block = blocks[blockIndex - 1];
        var newAngles = $"{x.ToString("F2", CultureInfo.InvariantCulture)} {y.ToString("F2", CultureInfo.InvariantCulture)} {z.ToString("F2", CultureInfo.InvariantCulture)}";

        var updatedBlock = block with { Angles = newAngles };
        blocks[blockIndex - 1] = updatedBlock;

        var newConfig = _config with
        {
            Maps = new Dictionary<string, List<BlockEntity>>(_config.Maps)
            {
                [Server.MapName] = blocks
            }
        };
        _config = newConfig;

        SaveConfig(_config);

        var successMessage = GetTranslation("angles_updated", blockIndex);
        if (player == null)
            LogToFile($"[BlockerPasses] {successMessage}");
        else
            player.PrintToChat($" {ReplaceColorTags("{GREEN}[BlockerPasses] " + successMessage)}");
    }

        [RequiresPermissions("@css/root")]
    [ConsoleCommand("css_bp_textures")]
    public void OnCmdListTextures(CCSPlayerController? player, CommandInfo info)
    {
        if (_config.AvailableTextures.Count == 0)
        {
            var noTexturesMessage = GetTranslation("no_textures_available");
            if (player == null)
                LogToFile($"[BlockerPasses] {noTexturesMessage}");
            else
                player.PrintToChat($" {ReplaceColorTags("{YELLOW}[BlockerPasses] " + noTexturesMessage)}");
            return;
        }

        var texturesMessage = GetTranslation("available_textures");
        if (player == null)
            LogToFile($"[BlockerPasses] {texturesMessage}");
        else
            player.PrintToChat($" {ReplaceColorTags("{BLUE}[BlockerPasses] " + texturesMessage)}");

        foreach (var texture in _config.AvailableTextures.Values)
        {
            var textureInfo = $"• {texture.Name}: {texture.DisplayName} ({texture.Category})";
            if (player == null)
                LogToFile($"[BlockerPasses] {textureInfo}");
            else
                player.PrintToChat($" {ReplaceColorTags("{WHITE}" + textureInfo)}");
        }
    }

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

        if (_menuApi != null)
        {
            OpenBlockerPassesMenuManager(player);
        }
        else
        {
                        OpenBlockerPassesMenu(player);
        }
    }

    private void InitializeTranslations()
    {
        _translations = new Dictionary<string, Dictionary<string, string>>();
        
        var translationsPath = Path.Combine(ModuleDirectory, "translations");
        
                var supportedLanguages = new[] { "en", "ru", "uk" };
        
        foreach (var lang in supportedLanguages)
        {
            var translationFile = Path.Combine(translationsPath, $"{lang}.json");
            
            if (File.Exists(translationFile))
            {
                try
                {
                    var jsonContent = File.ReadAllText(translationFile);
                    var translations = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);
                    
                    if (translations != null)
                    {
                        _translations[lang] = translations;
                        Logger.LogInformation($"Loaded translations for language: {lang}");
                    }
                    else
                    {
                        Logger.LogWarning($"Failed to parse translations file: {translationFile}");
                        LoadDefaultTranslations(lang);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error loading translations from {translationFile}: {ex.Message}");
                    LoadDefaultTranslations(lang);
                }
            }
            else
            {
                Logger.LogWarning($"Translation file not found: {translationFile}, using defaults");
                LoadDefaultTranslations(lang);
            }
        }
        
                if (_translations.Count == 0)
        {
            Logger.LogWarning("No translations loaded, using hardcoded defaults");
            LoadDefaultTranslations("en");
            LoadDefaultTranslations("ru");
            LoadDefaultTranslations("uk");
        }
    }
    
    private void LoadDefaultTranslations(string lang)
    {
        if (_translations.ContainsKey(lang))
            return;
            
        _translations[lang] = lang switch
        {
            "en" => new Dictionary<string, string>
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
                ["invalid_language"] = "Invalid language. Available: en, ru, uk",
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
                ["list_textures"] = "List Textures",
                ["edit_block_positions"] = "Edit Block Positions",
                ["select_block_to_edit"] = "Select Block to Edit",
                ["edit_origin"] = "Edit Origin",
                ["edit_angles"] = "Edit Angles",
                ["current_origin"] = "Current Origin: {0}",
                ["current_angles"] = "Current Angles: {0}",
                ["enter_new_value"] = "Enter new value using command",
                ["position_updated"] = "Position updated for block #{0}",
                ["angles_updated"] = "Angles updated for block #{0}",
                ["invalid_coordinates"] = "Invalid coordinates. Usage: <x> <y> <z>",
                ["back_to_main_menu"] = "Back to Main Menu",
                ["no_entities_for_this_map"] = "No entities for this map"
            },
            "ru" => new Dictionary<string, string>
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
                ["invalid_language"] = "Неверный язык. Доступно: en, ru, uk",
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
                ["list_textures"] = "Список текстур",
                ["edit_block_positions"] = "Редактировать позиции блоков",
                ["select_block_to_edit"] = "Выберите блок для редактирования",
                ["edit_origin"] = "Редактировать позицию",
                ["edit_angles"] = "Редактировать углы",
                ["current_origin"] = "Текущая позиция: {0}",
                ["current_angles"] = "Текущие углы: {0}",
                ["enter_new_value"] = "Введите новое значение с помощью команды",
                ["position_updated"] = "Позиция обновлена для блока #{0}",
                ["angles_updated"] = "Углы обновлены для блока #{0}",
                ["invalid_coordinates"] = "Неверные координаты. Использование: <x> <y> <z>",
                ["back_to_main_menu"] = "Вернуться в главное меню",
                ["no_entities_for_this_map"] = "Для этой карты нет сущностей"
            },
            "uk" => new Dictionary<string, string>
            {
                ["menu_title"] = "Управління BlockerPasses",
                ["reload_config"] = "Перезавантажити конфіг",
                ["get_position"] = "Отримати позицію",
                ["get_eye_angles"] = "Отримати кути огляду",
                ["current_settings"] = "Поточні налаштування",
                ["map_entities"] = "Сущності карти",
                ["back"] = "Назад",
                ["config_reloaded"] = "Конфігурацію перезавантажено!",
                ["must_be_alive"] = "Ви повинні бути живі для отримання позиції!",
                ["position_info"] = "Позиція: {0} | Кути: {1}",
                ["eye_angles_info"] = "Кути огляду: {0}",
                ["settings_info"] = "Мін. гравців: {0} | Карта: {1}",
                ["no_entities"] = "Для цієї карти не налаштовано сущностей",
                ["entity_info"] = "Модель: {0}\nПозиція: {1}\nКути: {2}\nКолір: {3}\nМасштаб: {4}\nПрозорість: {5}\nЛіміт: {6}",
                ["language_changed"] = "Мову змінено на українську",
                ["invalid_language"] = "Невірна мова. Доступно: en, ru, uk",
                ["block_added"] = "Блок успішно додано",
                ["block_removed"] = "Всі блоки видалено",
                ["preview_mode"] = "Режим попереднього перегляду увімкнено",
                ["preview_disabled"] = "Режим попереднього перегляду вимкнено",
                ["texture_create_usage"] = "Використання: css_bp_createtexture <ім'я> <відображаєме_ім'я> [шлях_до_текстури] [категорія]",
                ["texture_apply_usage"] = "Використання: css_bp_applytexture <індекс_блока> <ім'я_текстури>",
                ["texture_created"] = "Текстуру '{0}' успішно створено",
                ["texture_applied"] = "Текстуру '{0}' застосовано до блоку #{1}",
                ["texture_not_found"] = "Текстуру '{0}' не знайдено",
                ["invalid_block_index"] = "Невірний індекс блока",
                ["no_textures_available"] = "Немає доступних текстур",
                ["available_textures"] = "Доступні текстури:",
                ["texture_management"] = "Управління текстурами",
                ["create_texture"] = "Створити текстуру",
                ["apply_texture"] = "Застосувати текстуру",
                ["list_textures"] = "Список текстур",
                ["edit_block_positions"] = "Редагувати позиції блоків",
                ["select_block_to_edit"] = "Виберіть блок для редагування",
                ["edit_origin"] = "Редагувати позицію",
                ["edit_angles"] = "Редагувати кути",
                ["current_origin"] = "Поточна позиція: {0}",
                ["current_angles"] = "Поточні кути: {0}",
                ["enter_new_value"] = "Введіть нове значення за допомогою команди",
                ["position_updated"] = "Позицію оновлено для блоку #{0}",
                ["angles_updated"] = "Кути оновлено для блоку #{0}",
                ["invalid_coordinates"] = "Невірні координати. Використання: <x> <y> <z>",
                ["back_to_main_menu"] = "Повернутися до головного меню",
                ["no_entities_for_this_map"] = "Для цієї карти немає сущностей"
            },
            _ => new Dictionary<string, string>()
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

                if (_translations["en"].TryGetValue(key, out var englishTranslation))
        {
            return args.Length > 0 ? string.Format(englishTranslation, args) : englishTranslation;
        }

        return key;     }

    private HookResult EventRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        var playersCount = Utilities.GetPlayers()
            .Where(u => u.PawnIsAlive && u.PlayerPawn.Value != null && u.TeamNum != (int)CsTeam.None &&
                        u.TeamNum != (int)CsTeam.Spectator && u.PlayerPawn.Value.IsValid).ToList();

        if (playersCount.Count >= _config.Players) return HookResult.Continue;

        if (!_config.Maps.TryGetValue(Server.MapName, out var entitiesMap)) return HookResult.Continue;

        var uniqueModels = entitiesMap.Select(e => e.ModelPath).Distinct();
        foreach (var model in uniqueModels)
        {
            if (!string.IsNullOrEmpty(model))
            {
                Server.PrecacheModel(model);
                Logger.LogInformation($"[BlockerPasses] Precaching model: {model}");
            }
        }

        foreach (var entity in entitiesMap)
        {
            var color = entity.Color;

                         if (entity.TextureSettings != null)
            {
                color = entity.TextureSettings.TextureColor;
            }

            SpawnProp(entity.ModelPath, new[] { color[0], color[1], color[2] },
                GetVectorFromString(entity.Origin), GetQAngleFromString(entity.Angles), entity.Scale, entity.Invisibility, entity.TextureSettings);
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

    private void SpawnProp(string modelPath, int[] color, Vector origin, QAngle angles, float? entityScale, int invisibility = 255, TextureSettings? textureSettings = null)
    {
        var logsDir = Path.Combine(ModuleDirectory, "logs");
        Directory.CreateDirectory(logsDir);
        var logPath = Path.Combine(logsDir, "spawn_log.txt");
        File.AppendAllText(logPath, $"[{DateTime.Now}] Attempting to spawn prop with model: '{modelPath}' at {origin}\n");

        Logger.LogInformation($"[BlockerPasses] Attempting to spawn prop with model: '{modelPath}' at {origin}");

        if (string.IsNullOrEmpty(modelPath))
        {
            Logger.LogWarning("[BlockerPasses] Empty model path provided, skipping prop spawn");
            File.AppendAllText(logPath, $"[{DateTime.Now}] Empty model path, skipping\n");
            return;
        }

        var prop = Utilities.CreateEntityByName<CBaseModelEntity>("prop_dynamic");

        if (prop == null)
        {
            Logger.LogError("[BlockerPasses] Failed to create prop_dynamic_override entity");
            return;
        }

        prop.Collision.SolidType = SolidType_t.SOLID_VPHYSICS;

                           var alpha = Math.Clamp(invisibility, 0, 255);
        prop.Render = Color.FromArgb(alpha, color[0], color[1], color[2]);

        prop.SetModel(modelPath);
        prop.Teleport(origin, angles, new Vector(0, 0, 0));
        prop.DispatchSpawn();
        Logger.LogInformation($"[BlockerPasses] Dispatched spawn for prop with model '{modelPath}'");
        File.AppendAllText(logPath, $"[{DateTime.Now}] Dispatched spawn for prop with model '{modelPath}'\n");

        Server.NextFrame(() =>
        {
            Server.NextFrame(() =>
            {
                if (prop == null || !prop.IsValid)
                {
                    Logger.LogWarning($"[BlockerPasses] Prop is null or invalid when trying to set scale");
                    return;
                }

                var bodyComponent = prop.CBodyComponent;
                if (bodyComponent?.SceneNode == null) return;

                try
                {
                    var skeletonInstance = bodyComponent.SceneNode.GetSkeletonInstance();
                    if (skeletonInstance != null && entityScale != null && entityScale != 0.0f)
                    {
                        skeletonInstance.Scale = entityScale.Value;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogWarning($"[BlockerPasses] Failed to set skeleton scale: {ex.Message}");
                }

                if (textureSettings != null)
                {
                    ApplyTextureToProp(prop, textureSettings, alpha);
                }
            });
        });
    }

    private void ApplyTextureToProp(CBaseModelEntity prop, TextureSettings textureSettings, int alpha = 255)
    {
                        
                if (textureSettings.TextureName == "2x2_pattern")
        {
                                    var patternColor = Color.FromArgb(alpha, 200, 200, 200);             prop.Render = patternColor;
        }
        else if (textureSettings.TextureColor != null && textureSettings.TextureColor.Length >= 3)
        {
                        var textureColor = Color.FromArgb(alpha, 
                textureSettings.TextureColor[0], 
                textureSettings.TextureColor[1], 
                textureSettings.TextureColor[2]);
            prop.Render = textureColor;
        }
        
                if (textureSettings.UseCustomTexture && !string.IsNullOrEmpty(textureSettings.CustomTexturePath))
        {
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
            Version = "1.1",
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

                 menu.AddMenuOption(GetTranslation("reload_config"), (player, option) => {
             _config = LoadConfig();
             player.PrintToChat($" {ReplaceColorTags("{GREEN}[BlockerPasses] " + GetTranslation("config_reloaded"))}");
         });

         if (_config.Menu.EnablePositionCommands)
         {
             menu.AddMenuOption(GetTranslation("get_position"), (player, option) => {
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
                LogToFile($"BP_POS: {message}");
            });

            menu.AddMenuOption(GetTranslation("get_eye_angles"), (player, option) => {
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
                LogToFile($"BP_EYE: {message}");
            });
        }

        menu.AddMenuOption(GetTranslation("current_settings"), (player, option) => {
            var message = GetTranslation("settings_info", _config.Players, Server.MapName);
            player.PrintToChat($" {ReplaceColorTags("{YELLOW}[BlockerPasses] " + message)}");
        });

        menu.AddMenuOption(GetTranslation("map_entities"), (player, option) => {
            OpenMapEntitiesMenu(player);
        });

        menu.AddMenuOption(GetTranslation("edit_block_positions"), (player, option) => {
            OpenEditPositionsMenu(player);
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
            menu.AddMenuOption(GetTranslation("no_entities_for_this_map"), (player, option) => { });
        }

        menu.AddMenuOption(GetTranslation("back_to_main_menu"), (player, option) => {
            OpenBlockerPassesMenu(player);
        });

        menu.Open(player);
    }

    private void OpenEditPositionsMenu(CCSPlayerController player)
    {
        var menu = new ChatMenu(GetTranslation("select_block_to_edit"));

        if (_config.Maps.TryGetValue(Server.MapName, out var entities))
        {
            for (int i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];
                var blockName = $"Block {i + 1}";
                var index = i + 1;

                menu.AddMenuOption(blockName, (player, option) => {
                    OpenEditBlockMenu(player, index, entity);
                });
            }
        }
        else
        {
            menu.AddMenuOption(GetTranslation("no_entities_for_this_map"), (player, option) => { });
        }

        menu.AddMenuOption(GetTranslation("back_to_main_menu"), (player, option) => {
            OpenBlockerPassesMenu(player);
        });

        menu.Open(player);
    }

    private void OpenEditBlockMenu(CCSPlayerController player, int index, BlockEntity entity)
    {
        var menu = new ChatMenu($"Edit Block {index}");

        menu.AddMenuOption(GetTranslation("edit_origin"), (player, option) => {
            var current = GetTranslation("current_origin", entity.Origin);
            var enter = GetTranslation("enter_new_value");
            player.PrintToChat($" {ReplaceColorTags("{BLUE}[BlockerPasses] " + current)}");
            player.PrintToChat($" {ReplaceColorTags("{YELLOW}[BlockerPasses] " + enter)}");
            player.PrintToChat($" {ReplaceColorTags("{WHITE}[BlockerPasses] Usage: css_bp_setorigin {index} <x> <y> <z>")}");
        });

        menu.AddMenuOption(GetTranslation("edit_angles"), (player, option) => {
            var current = GetTranslation("current_angles", entity.Angles);
            var enter = GetTranslation("enter_new_value");
            player.PrintToChat($" {ReplaceColorTags("{BLUE}[BlockerPasses] " + current)}");
            player.PrintToChat($" {ReplaceColorTags("{YELLOW}[BlockerPasses] " + enter)}");
            player.PrintToChat($" {ReplaceColorTags("{WHITE}[BlockerPasses] Usage: css_bp_setangles {index} <x> <y> <z>")}");
        });

        menu.AddMenuOption(GetTranslation("back"), (player, option) => {
            OpenEditPositionsMenu(player);
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

        if (_menuApi == null)
        {
            Logger.LogError("MenuManager API is null, falling back to native menu");
            OpenBlockerPassesMenu(player);
            return;
        }

        var menu = _menuApi.GetMenu($"🎯 {_config.Menu.MenuTitle}");
        
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
                var pos = pawn.AbsOrigin!;
                var angles = pawn.AbsRotation!;

                var message = $"Position: {pos.X:F2}, {pos.Y:F2}, {pos.Z:F2} | Angles: {angles.X:F2}, {angles.Y:F2}, {angles.Z:F2}";
                player.PrintToChat($" {ReplaceColorTags("{BLUE}[BlockerPasses] " + message)}");
                LogToFile($"BP_POS: {message}");
            });

            menu.AddMenuOption("👁️ " + GetTranslation("get_eye_angles"), (player, option) => {
                if (!player.PawnIsAlive || player.PlayerPawn.Value == null)
                {
                    player.PrintToChat($" {ReplaceColorTags("{RED}[BlockerPasses] You must be alive to get eye angles!")}");
                    return;
                }

                var pawn = player.PlayerPawn.Value;
                var eyeAngles = pawn.EyeAngles!;

                var message = $"Eye Angles: {eyeAngles.X:F2}, {eyeAngles.Y:F2}, {eyeAngles.Z:F2}";
                player.PrintToChat($" {ReplaceColorTags("{BLUE}[BlockerPasses] " + message)}");
                LogToFile($"BP_EYE: {message}");
            });
        }

        menu.AddMenuOption("⚙️ " + GetTranslation("current_settings"), (player, option) => {
            var message = GetTranslation("settings_info", _config.Players, Server.MapName);
            player.PrintToChat($" {ReplaceColorTags("{BLUE}[BlockerPasses] " + message)}");
        });

        menu.AddMenuOption("🗺️ " + GetTranslation("map_entities"), (player, option) => {
            OpenMapEntitiesMenuManager(player);
        });

        menu.AddMenuOption("✏️ " + GetTranslation("edit_block_positions"), (player, option) => {
            OpenEditPositionsMenuManager(player);
        });

        menu.AddMenuOption("🎨 " + GetTranslation("texture_management"), (player, option) => {
            OpenTextureManagementMenuManager(player);
        });

        menu.Open(player);
    }

    private void OpenMapEntitiesMenuManager(CCSPlayerController player)
    {
        if (_menuApi == null)
        {
            Logger.LogError("MenuManager API is null, falling back to native menu");
            OpenMapEntitiesMenu(player);
            return;
        }

        var menu = _menuApi.GetMenu($"🗺️ Entities for {Server.MapName}");

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
            menu.AddMenuOption("❌ " + GetTranslation("no_entities_for_this_map"), (player, option) => {});
        }

        menu.AddMenuOption("🔙 " + GetTranslation("back_to_main_menu"), (player, option) => {
            OpenBlockerPassesMenuManager(player);
        });

        menu.Open(player);
    }

    private void OpenEditPositionsMenuManager(CCSPlayerController player)
    {
        if (_menuApi == null)
        {
            Logger.LogError("MenuManager API is null, falling back to native menu");
            OpenEditPositionsMenu(player);
            return;
        }

        var menu = _menuApi.GetMenu($"✏️ {GetTranslation("select_block_to_edit")}");

        if (_config.Maps.TryGetValue(Server.MapName, out var entities))
        {
            for (int i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];
                var blockName = $"🔹 Block {i + 1}";
                var index = i + 1;

                menu.AddMenuOption(blockName, (player, option) => {
                    OpenEditBlockMenuManager(player, index, entity);
                });
            }
        }
        else
        {
            menu.AddMenuOption("❌ " + GetTranslation("no_entities_for_this_map"), (player, option) => {});
        }

        menu.AddMenuOption("🔙 " + GetTranslation("back_to_main_menu"), (player, option) => {
            OpenBlockerPassesMenuManager(player);
        });

        menu.Open(player);
    }

    private void OpenEditBlockMenuManager(CCSPlayerController player, int index, BlockEntity entity)
    {
        if (_menuApi == null)
        {
            Logger.LogError("MenuManager API is null, falling back to native menu");
            OpenEditBlockMenu(player, index, entity);
            return;
        }

        var menu = _menuApi.GetMenu($"✏️ Edit Block {index}");

        menu.AddMenuOption("📍 " + GetTranslation("edit_origin"), (player, option) => {
            var current = GetTranslation("current_origin", entity.Origin);
            var enter = GetTranslation("enter_new_value");
            player.PrintToChat($" {ReplaceColorTags("{BLUE}[BlockerPasses] " + current)}");
            player.PrintToChat($" {ReplaceColorTags("{YELLOW}[BlockerPasses] " + enter)}");
            player.PrintToChat($" {ReplaceColorTags("{WHITE}[BlockerPasses] Usage: css_bp_setorigin {index} <x> <y> <z>")}");
        });

        menu.AddMenuOption("🔄 " + GetTranslation("edit_angles"), (player, option) => {
            var current = GetTranslation("current_angles", entity.Angles);
            var enter = GetTranslation("enter_new_value");
            player.PrintToChat($" {ReplaceColorTags("{BLUE}[BlockerPasses] " + current)}");
            player.PrintToChat($" {ReplaceColorTags("{YELLOW}[BlockerPasses] " + enter)}");
            player.PrintToChat($" {ReplaceColorTags("{WHITE}[BlockerPasses] Usage: css_bp_setangles {index} <x> <y> <z>")}");
        });

        menu.AddMenuOption("🔙 " + GetTranslation("back"), (player, option) => {
            OpenEditPositionsMenuManager(player);
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
        if (_menuApi == null)
        {
            Logger.LogError("MenuManager API is null, falling back to native menu");
            OpenTextureManagementMenu(player);
            return;
        }

        var menu = _menuApi.GetMenu($"🎨 {GetTranslation("texture_management")}");

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
    public string Version { get; init; } = "1.0";
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
    public int Invisibility { get; init; } = 255;     public int Quota { get; init; } = 0;     public string? Name { get; init; }     public string? TexturePath { get; init; }     public TextureSettings? TextureSettings { get; init; } }

public record TextureSettings
{
    public string TextureName { get; init; } = "default";
    public int[] TextureColor { get; init; } = { 255, 255, 255 };
    public float TextureScale { get; init; } = 1.0f;
    public float TextureOffsetX { get; init; } = 0.0f;
    public float TextureOffsetY { get; init; } = 0.0f;
    public float TextureRotation { get; init; } = 0.0f;
    public bool UseCustomTexture { get; init; } = false;
    public string? CustomTexturePath { get; init; } }

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