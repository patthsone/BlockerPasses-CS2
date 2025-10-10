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

    public override void Load(bool hotReload)
    {
        _config = LoadConfig();
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
    [ConsoleCommand("css_bp_reload")]
    public void OnCmdReload(CCSPlayerController? player, CommandInfo info)
    {
        _config = LoadConfig();

        const string msg = "Configuration successfully rebooted";

        if (player == null)
            Console.WriteLine(msg);
        else
            player.PrintToChat(msg);
    }

    // Новая команда для получения позиции
    [RequiresPermissions("@css/root")]
    [ConsoleCommand("css_bp_getpos")]
    public void OnCmdGetPos(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.PawnIsAlive || player.PlayerPawn.Value == null)
        {
            info.ReplyToCommand("You must be alive to use this command!");
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

        // Выводим в консоль и чат
        var message = $"Position: {originStr} | Angles: {anglesStr}";
        
        info.ReplyToCommand(message);
        player.PrintToChat($" {ReplaceColorTags("{GREEN}" + message)}");
        
        // Также выводим в консоль сервера для удобства копирования
        Console.WriteLine($"BP_POS: {message}");
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

            SpawnProp(entity.ModelPath, new[] { color[0], color[1], color[2] },
                GetVectorFromString(entity.Origin), GetQAngleFromString(entity.Angles), entity.Scale);
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

    private void SpawnProp(string modelPath, int[] color, Vector origin, QAngle angles, float? entityScale)
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
    }

    private Config LoadConfig()
    {
        var configPath = Path.Combine(ModuleDirectory, "blocker_passes.json");
        if (!File.Exists(configPath)) return CreateConfig(configPath);

        var config = JsonSerializer.Deserialize<Config>(File.ReadAllText(configPath))!;

        return config;
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
            Maps = new Dictionary<string, List<Entities>>
            {
                {
                    "de_mirage", new List<Entities>
                    {
                        new()
                        {
                            ModelPath =
                                "models/props/de_dust/hr_dust/dust_windows/dust_rollupdoor_96x128_surface_lod.vmdl",
                            Color = new[] { 30, 144, 255 },
                            Origin = "-1600.46 -741.124 -172.965",
                            Angles = "0 180 0",
                            Scale = 0.0f
                        },
                        new()
                        {
                            ModelPath = "models/props/de_mirage/small_door_b.vmdl",
                            Color = new[] { 255, 255, 255 },
                            Origin = "588.428 704.941 -136.517",
                            Angles = "0 270.256 0",
                            Scale = 0.0f
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
}

public class Config
{
    public int Players { get; init; }
    public required string Message { get; init; }
    public Dictionary<string, List<Entities>> Maps { get; init; } = null!;
    public MenuSettings Menu { get; init; } = new();
}

public class MenuSettings
{
    public bool EnableMenu { get; init; } = true;
    public string MenuTitle { get; init; } = "BlockerPasses Management";
    public bool ShowEntityDetails { get; init; } = true;
    public bool EnablePositionCommands { get; init; } = true;
}

public class Entities
{
    public required string ModelPath { get; init; }
    public int[] Color { get; init; } = { 255, 255, 255 };
    public required string Origin { get; init; }
    public required string Angles { get; init; }
    public float? Scale { get; init; }
}