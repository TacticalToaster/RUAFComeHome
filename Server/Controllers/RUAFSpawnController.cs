using SPTarkov.Common.Extensions;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Json;
using System.Reflection;
using System.Text.Json;

namespace RUAFComeHomeServer.Controllers;

[Injectable(InjectionType.Singleton)]
public class RUAFSpawnController(
    JsonUtil jsonUtil,
    RandomUtil randomUtil,
    ConfigController configController,
    DatabaseService databaseService,
    RUAFLogger logger,
    HttpResponseUtil httpResponse
)
{
    public void AdjustAllRuafSpawns()
    {
        try
        {
            var tables = databaseService.GetTables();
            var locations = databaseService.GetLocations();
            var mainConfig = configController.ModConfig;

            var factoryspecial = new Location();
            var factoryspecialPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "db", "maps", "factory4_special");
            factoryspecial.Base = jsonUtil.DeserializeFromFile<LocationBase>(Path.Combine(factoryspecialPath, "base.json"));
            //factoryspecial.LooseLoot = jsonUtil.DeserializeFromFile<LazyLoad<LooseLoot>>(Path.Combine(factoryspecialPath, "looseLoot.json"));
            //factoryspecial.StaticAmmo = jsonUtil.DeserializeFromFile<Dictionary<string,IEnumerable<StaticAmmoDetails>>>(Path.Combine(factoryspecialPath, "staticAmmo.json"));
            //factoryspecial.StaticContainers = jsonUtil.DeserializeFromFile<LazyLoad<StaticContainerDetails>>(Path.Combine(factoryspecialPath, "staticContainers.json"));
            //factoryspecial.StaticLoot = jsonUtil.DeserializeFromFile<LazyLoad<Dictionary<MongoId,StaticLootDetails>>>(Path.Combine(factoryspecialPath, "staticLoot.json"));
            //factoryspecial.Statics = jsonUtil.DeserializeFromFile<StaticContainer>(Path.Combine(factoryspecialPath, "statics.json"));

            logger.Error($"Factory: {factoryspecial.Base.Name}");

            //locations.AddToExtensionData("factory4_special", factoryspecial);
            locations.Develop.Base = factoryspecial.Base;
            //locations.Develop.AllExtracts = factoryspecial.AllExtracts;
            locations.Develop.LooseLoot = locations.Factory4Day.LooseLoot;//new(() => new LooseLoot());//;
            locations.Develop.StaticAmmo = locations.Factory4Day.StaticAmmo;//new();//;
            locations.Develop.StaticContainers = locations.Factory4Day.StaticContainers;//new(() => new StaticContainerDetails());//;
            locations.Develop.StaticLoot = locations.Factory4Day.StaticLoot;//new(() => new Dictionary<MongoId, StaticLootDetails>());//;
            locations.Develop.Statics = factoryspecial.Statics;

            tables.Globals.Configuration.BTRSettings.LocationsWithBTR = tables.Globals.Configuration.BTRSettings.LocationsWithBTR.Except(new[] { "develop" });

            //logger.Warn($"Locations: {locations.GetByJsonProperty<Location>("factory4_special").Base.Name}");

            foreach (var map in mainConfig.locations.Keys)
            {
                logger.Info($"Adjusting RUAF spawns for {map}.");

                if (!locations.GetDictionary().ContainsKey(locations.GetMappedKey(map)))
                {
                    logger.Info($"No location data found for {map}. Skipping RUAF spawn adjustment.");
                    continue;
                }

                var mapConfig = mainConfig.locations[map];
                var patrolConfig = mapConfig.patrol;
                var checkpointConfig = mapConfig.checkpoint;
                var huntConfig = mapConfig.hunt;
                var spawns = locations.GetDictionary()[locations.GetMappedKey(map)].Base.BossLocationSpawn;

                // Remove existing RUAF spawns
                spawns.RemoveAll(x => x.BossName.Contains("ruaf"));

                if (patrolConfig.enablePatrols)
                {
                    AdjustPatrolSpawnsForMap(map, mapConfig, mainConfig, spawns);
                }

                if (checkpointConfig.enableCheckpoints)
                {
                    AdjustCheckpointSpawnsForMap(map, mapConfig, mainConfig, spawns);
                }

                if (huntConfig.enableHunts)
                {
                    AdjustHuntSpawnsForMap(map, mapConfig, mainConfig, spawns);
                }
            }

            locations.Lighthouse.Base.BossLocationSpawn = locations.Lighthouse.Base.BossLocationSpawn.FindAll(x => x.BossName.Contains("exUsec") || x.BossName.Contains("Knight"));
            locations.Lighthouse.Base.Waves.Clear();

            var goons = locations.Lighthouse.Base.BossLocationSpawn.Find(x => x.BossName.Contains("Knight"));
            if (goons != null)
            {
                goons.BossChance = 100;
                goons.ForceSpawn = true;
                goons.BossZone = "Zone_TreatmentContainers";
            }

            foreach (var spawn in locations.Lighthouse.Base.BossLocationSpawn)
            {
                if (spawn.BossZone == "Zone_Island")
                    continue;

                spawn.BossChance = 100;
                spawn.IgnoreMaxBots = true;
                spawn.Delay = -1;
                spawn.Time = -1;
            }

            var hunt1 = GeneratePatrol(3, 100, true);
            var hunt2 = GeneratePatrol(3, 100, true);
            var hunt3 = GeneratePatrol(5, 100, true);

            hunt1.BossZone = "Zone_LongRoad";
            hunt1.Time = -1;
            hunt1.Delay = -1;

            hunt2.BossZone = "Zone_Village";
            hunt2.Time = -1;
            hunt2.Delay = -1;

            hunt3.BossZone = "Zone_Bridge";
            hunt3.Time = -1;
            hunt3.Delay = -1;

            locations.Lighthouse.Base.BossLocationSpawn.Add(hunt1);
            locations.Lighthouse.Base.BossLocationSpawn.Add(hunt2);
            locations.Lighthouse.Base.BossLocationSpawn.Add(hunt3);
        }
        catch (Exception ex)
        {
            logger.Error($"Error adjusting RUAF spawns: {ex.Message}");
            throw;
        }
    }

    private void AdjustPatrolSpawnsForMap(string map, MapConfig mapConfig, MainConfig mainConfig, List<BossLocationSpawn> spawns)
    {
        var patrolConfig = mapConfig.patrol;

        logger.Info($"Enabling RUAF patrols for {map}.");
        var validZones = new List<string>(patrolConfig.patrolZones);

        for (int i = 0; i < patrolConfig.patrolAmount; i++)
        {
            var patrolSize = randomUtil.GetInt(patrolConfig.patrolMin, patrolConfig.patrolMax);
            var patrol = GeneratePatrol(patrolSize, mainConfig.debug.spawnAlways ? 100 : patrolConfig.patrolChance);

            patrol.BossZone = randomUtil.GetArrayValue(validZones);
            validZones.Remove(patrol.BossZone);

            if (validZones.Count == 0)
            {
                validZones = new List<string>(patrolConfig.patrolZones);
            }

            patrol.Time = randomUtil.GetInt(patrolConfig.patrolTimeMin, patrolConfig.patrolTimeMax);

            if (mainConfig.debug.spawnInstantlyAlways)
            {
                logger.Info($"Instantly spawning RUAF patrol for {map}.");
                patrol.Time = -1;
            }

            spawns.Add(patrol);

            logger.Info($"Added ({patrolConfig.patrolChance}% chance) RUAF patrol of size {patrolSize} to {map} in zone {patrol.BossZone} with a spawn time of {patrol.Time} seconds.");
        }
    }

    private void AdjustCheckpointSpawnsForMap(string map, MapConfig mapConfig, MainConfig mainConfig, List<BossLocationSpawn> spawns)
    {
        var checkpointConfig = mapConfig.checkpoint;

        logger.Info($"Enabling RUAF checkpoint for {map}.");
        var validZones = new List<ZoneCheckpointConfig>(checkpointConfig.checkpointZones);

        for (int i = 0; i < checkpointConfig.checkpointAmount; i++)
        {
            var checkpointZoneConfig = randomUtil.GetArrayValue(validZones);
            validZones.Remove(checkpointZoneConfig);

            var patrolSize = randomUtil.GetInt(checkpointZoneConfig.checkpointMin, checkpointZoneConfig.checkpointMax);
            var patrol = GeneratePatrol(patrolSize, mainConfig.debug.spawnAlways ? 100 : checkpointZoneConfig.checkpointChance, false);

            patrol.BossZone = checkpointZoneConfig.checkpointZone;

            if (validZones.Count == 0)
            {
                validZones = [.. checkpointConfig.checkpointZones];
            }

            patrol.Time = -1;//_randomUtil.GetInt(patrolConfig.patrolTimeMin, patrolConfig.patrolTimeMax);

            if (mainConfig.debug.spawnInstantlyAlways)
            {
                logger.Info($"Instantly spawning RUAF checkpoint for {map}.");
                patrol.Time = -1;
            }

            spawns.Add(patrol);

            logger.Info($"Added ({checkpointZoneConfig.checkpointChance}% chance) RUAF checkpoint of size {patrolSize} to {map} in zone {patrol.BossZone} with a spawn time of {patrol.Time} seconds.");
        }
    }

    private void AdjustHuntSpawnsForMap(string map, MapConfig mapConfig, MainConfig mainConfig, List<BossLocationSpawn> spawns)
    {
        if (mapConfig.hunt.hunts.ContainsKey("ruaf"))
        {
            spawns.RemoveAll(x => x.TriggerId == "ruafHunt");
            AddRuafHuntToMap(map, mapConfig, mainConfig, spawns);
        }

        if (mapConfig.hunt.hunts.ContainsKey("exUsec"))
        {
            spawns.RemoveAll(x => x.TriggerId == "exUsecHunt");
            AddExUsecHuntToMap(map, mapConfig, mainConfig, spawns);
        }
    }

    private void AddRuafHuntToMap(string map, MapConfig? mapConfig, MainConfig mainConfig, List<BossLocationSpawn> spawns)
    {
        var huntConfig = mapConfig.hunt.hunts["ruaf"];

        logger.Info($"Enabling RUAF hunt for {map}.");

        var patrolSize = randomUtil.GetInt(huntConfig.huntMin, huntConfig.huntMax);
        var patrol = GeneratePatrol(patrolSize, mainConfig.debug.spawnAlways ? 100 : 100, false);

        patrol.Time = -1;

        patrol.BossZone = huntConfig.huntZones;
        patrol.TriggerName = "botEvent";
        patrol.TriggerId = "ruafHunt";
        patrol.ForceSpawn = true;

        spawns.Add(patrol);

        logger.Info($"Added RUAF Hunt of size {patrolSize} to {map} in zone {patrol.BossZone} with a spawn time of {patrol.Time} seconds.");
    }

    private void AddExUsecHuntToMap(string map, MapConfig? mapConfig, MainConfig mainConfig, List<BossLocationSpawn> spawns)
    {
        var huntConfig = mapConfig.hunt.hunts["exUsec"];

        logger.Info($"Enabling EXUSEC hunt for {map}.");

        var patrolSize = randomUtil.GetInt(huntConfig.huntMin, huntConfig.huntMax);
        var patrol = new BossLocationSpawn
        {
            BossChance = 100,
            BossDifficulty = "normal",
            BossEscortAmount = patrolSize.ToString(),
            BossEscortDifficulty = "normal",
            BossEscortType = "exUsec",
            BossName = "exUsec",
            IsBossPlayer = false,
            BossZone = string.Empty,
            ForceSpawn = false,
            IgnoreMaxBots = true,
            IsRandomTimeSpawn = false,
            SpawnMode = new[] { "regular", "pve" },
            Supports = new List<BossSupport>(),
            Time = -1,
            TriggerId = string.Empty,
            TriggerName = string.Empty
        };

        patrol.Time = -1;

        patrol.BossZone = huntConfig.huntZones;
        patrol.TriggerName = "botEvent";
        patrol.TriggerId = "exUsecHunt";
        patrol.ForceSpawn = true;

        spawns.Add(patrol);

        logger.Info($"Added EXUSEC Hunt of size {patrolSize} to {map} in zone {patrol.BossZone} with a spawn time of {patrol.Time} seconds.");
    }

    private BossLocationSpawn GeneratePatrol(int patrolSize, float chance, bool isPatrol = true)
    {
        var bossType = "ruafRiflemanSenior";
        var secondLeader = string.Empty;
        var followers = patrolSize - 1;
        var mainConfig = configController.ModConfig;
        var genConfig = mainConfig.patrols;

        var specialRoles = new List<string> {
            "ruafMarksman",
            "ruafMachinegunner",
            "ruafAutorifleman"
        };

        var validRoles = new List<string>(specialRoles);

        var numOfSpecialists = patrolSize / mainConfig.patrols.specialistEveryPerson;

        if (isPatrol == false)
            genConfig = mainConfig.checkpoints;

        logger.Info($"Generating RUAF patrol of size {patrolSize}.");


        if (patrolSize >= genConfig.minSecondLeaderSize && randomUtil.GetChance100(genConfig.secondLeaderChance))
        {
            logger.Info("RUAF patrol second leader added.");
            secondLeader = "ruafRiflemanSenior";
            followers--;
        }

        var supportsList = new List<BossSupport>();

        for (int i = 0; i < numOfSpecialists; i++)
        {
            var specialRole = randomUtil.GetArrayValue(validRoles);

            supportsList.Add(new BossSupport
            {
                BossEscortAmount = "1",
                BossEscortDifficulty = new ListOrT<string>(["normal"], null),
                BossEscortType = specialRole
            });

            followers--;

            if (validRoles.Count == 0)
                validRoles = [.. specialRoles];

        }


        if (!string.IsNullOrEmpty(secondLeader))
        {
            supportsList.Add(new BossSupport
            {
                BossEscortAmount = "1",
                BossEscortDifficulty = new ListOrT<string>(["normal"], null),
                BossEscortType = secondLeader
            });
        }

        supportsList.Add(new BossSupport
        {
            BossEscortAmount = followers.ToString(),
            BossEscortDifficulty = new ListOrT<string>(["normal"], null),
            BossEscortType = "ruafRifleman"
        });

        var bossInfo = new BossLocationSpawn
        {
            BossChance = chance,
            BossDifficulty = "normal",
            BossEscortAmount = "1",
            BossEscortDifficulty = "normal",
            BossEscortType = "ruafRifleman",
            BossName = bossType,
            IsBossPlayer = false,
            BossZone = string.Empty,
            ForceSpawn = false,
            IgnoreMaxBots = true,
            IsRandomTimeSpawn = false,
            SpawnMode = new[] { "regular", "pve" },
            Supports = supportsList,
            Time = -1,
            TriggerId = string.Empty,
            TriggerName = string.Empty
        };

        return bossInfo;
    }
}