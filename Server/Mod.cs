using MoreBotsServer.Models;
using RUAFComeHomeServer.Controllers;
using SPTarkov.Common.Extensions;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using System.Reflection;

namespace RUAFComeHomeServer;

public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.ruafcomehome.tacticaltoaster";
    public override string Name { get; init; } = "RUAF Come Home";
    public override string Author { get; init; } = "TacticalToaster";
    public override List<string>? Contributors { get; init; } = new() { };
    public override SemanticVersioning.Version Version { get; init; } = new(1, 0, 0);
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.0");
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; } = new()
    {
        { "com.morebotsapi.tacticaltoaster", new SemanticVersioning.Range(">=1.0.0") },
        { "com.wtt.commonlib", new SemanticVersioning.Range(">=2.0.0") }
    };
    public override string? Url { get; init; }
    public override bool? IsBundleMod { get; init; }
    public override string License { get; init; } = "MIT";
}

[Injectable(TypePriority = OnLoadOrder.PreSptModLoader + 1)]
public class RUAFModPreload : IOnLoad
{
    public static MainConfig ModConfig = new();

    private readonly ModHelper _modHelper;

    public RUAFModPreload(
        ModHelper modHelper
        )
    {
        _modHelper = modHelper;
    }

    Task IOnLoad.OnLoad()
    {
        var pathToMod = _modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());

        ModConfig = _modHelper.GetJsonDataFromFile<MainConfig>(pathToMod, "config.jsonc");

        return Task.CompletedTask;
    }
}

[Injectable(InjectionType = InjectionType.Singleton, TypePriority = MoreBotsServer.MoreBotsLoadOrder.LoadBots)]
public class RUAFComeHome(
    MoreBotsServer.MoreBotsAPI moreBotsLib,
    MoreBotsServer.Services.MoreBotsCustomBotTypeService customBotTypeService,
    MoreBotsServer.Services.FactionService factionService,
    MoreBotsServer.Services.LoadoutService loadoutService,
    WTTServerCommonLib.WTTServerCommonLib commonLib,
    IReadOnlyList<SptMod> modList,
    RUAFSpawnController ruafSpawnController
) : IOnLoad
{
    public async Task OnLoad()
    {
        var typeList = new List<string> {
            "ruafRifleman",
            "ruafRiflemanSenior",
            "ruafAutorifleman",
            "ruafGrenadier",
            "ruafMarksman",
            "ruafMachinegunner"
        };

        var assembly = Assembly.GetExecutingAssembly();

        // Load base bot types using a shared type
        await moreBotsLib.LoadBotsShared(assembly, "ruaf", typeList);

        // Load the loadouts for the bots, using a template
        await loadoutService.LoadLoadoutsWithTemplate(assembly, "ruaf_standard");

        // Replace some values in the bot types
        await customBotTypeService.LoadBotTypeReplace(Assembly.GetExecutingAssembly(), "ruaf_all", typeList);

        // Replace values per type based on files that correspond to the passed type list
        await customBotTypeService.LoadBotTypeReplaceByTypes(Assembly.GetExecutingAssembly(), typeList);

        // Add couturier mod related stuff
        if (modList.Any(mod => mod.ModMetadata.ModGuid == "com.turbodestroyer.couturier"))
        {
            // Replace the appearance settings of the bots so they use couturier clothes
            await customBotTypeService.LoadBotTypeReplace(Assembly.GetExecutingAssembly(), "ruaf_couturier", typeList);
        }

        // Add enemies based on factions
        factionService.AddEnemyByFaction(typeList, "savage");
        factionService.AddEnemyByFaction(typeList, "rogues");
        factionService.AddEnemyByFaction(typeList, "cultists");
        factionService.AddEnemyByFaction(typeList, "infected");

        // Add RUAF as enemies to those same factions
        factionService.AddEnemyByFaction("savage", "ruaf");
        factionService.AddEnemyByFaction("rogues", "ruaf");
        factionService.AddEnemyByFaction("cultists", "ruaf");
        factionService.AddEnemyByFaction("infected", "ruaf");

        factionService.AddRevengeByFaction(typeList, "ruaf");

        if (modList.Any(mod => mod.ModMetadata.ModGuid == "com.untargh.tacticaltoaster"))
        {
            factionService.AddWarnByFaction(typeList, "untar");
        }

        // Use WTT to add locales
        await commonLib.CustomLocaleService.CreateCustomLocales(Assembly.GetExecutingAssembly());

        // Add RUAF to spawns
        ruafSpawnController.AdjustAllRuafSpawns();

        await Task.CompletedTask;
    }
}

[Injectable(InjectionType = InjectionType.Singleton, TypePriority = MoreBotsServer.MoreBotsLoadOrder.LoadFactions)]
public class RUAFComeHomeLoadFaction(
    MoreBotsServer.Services.FactionService factionService
) : IOnLoad
{
    public async Task OnLoad()
    {
        // Create the new RUAF faction
        factionService.Factions.Add("ruaf", new Faction()
        {
            Name = "ruaf",
            BotTypes =
            {
                (WildSpawnType)848400,
                (WildSpawnType)848401,
                (WildSpawnType)848402,
                (WildSpawnType)848403,
                (WildSpawnType)848404,
                (WildSpawnType)848405
            }
        });

        await Task.CompletedTask;
    }
}

[Injectable]
public class CustomDynamicRouter : DynamicRouter
{
    private static HttpResponseUtil _httpResponseUtil;
    private static ConfigController _configController;

    public CustomDynamicRouter(
        JsonUtil jsonUtil,
        HttpResponseUtil httpResponseUtil,
        ConfigController configController) : base(jsonUtil, GetCustomRoutes())
    {
        _httpResponseUtil = httpResponseUtil;
        _configController = configController;
    }
    private static List<RouteAction> GetCustomRoutes()
    {
        return [
            new RouteAction(
                "/ruaf/checkpoints",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => {
                    var result = _configController.ModConfig;
                    return await new ValueTask<string>(_httpResponseUtil.NoBody(result));
                }
            )
        ];
    }
}

[Injectable]
public class CustomStaticRouter : StaticRouter
{
    private static HttpResponseUtil _httpResponseUtil;
    private static RUAFSpawnController _ruafSpawnController;

    public CustomStaticRouter(
        RUAFSpawnController untarSpawnController,
        JsonUtil jsonUtil,
        HttpResponseUtil httpResponseUtil) : base(jsonUtil, GetCustomRoutes())
    {
        _httpResponseUtil = httpResponseUtil;
        _ruafSpawnController = untarSpawnController;
    }

    private static List<RouteAction> GetCustomRoutes()
    {
        return
        [
            new RouteAction(
                "/client/match/local/end",
                async (
                    url,
                    info,
                    sessionID,
                    output
                ) => {
                    _ruafSpawnController.AdjustAllRuafSpawns();
                    return await new ValueTask<object>(output ?? string.Empty);
                }
            )
        ];
    }
}