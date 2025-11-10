using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Json;

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
            var mainConfig = configController.ModConfig;

            foreach (var map in mainConfig.locations.Keys)
            {
                logger.Info($"Adjusting RUAF spawns for {map}.");

                if (!tables.Locations.GetDictionary().ContainsKey(map))
                {
                    logger.Info($"No location data found for {map}. Skipping RUAF spawn adjustment.");
                    continue;
                }

                var mapConfig = mainConfig.locations[map];
                var patrolConfig = mapConfig.patrol;
                var checkpointConfig = mapConfig.checkpoint;
                var huntConfig = mapConfig.hunt;
                var spawns = tables.Locations.GetDictionary()[map].Base.BossLocationSpawn;

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

                if (huntConfig.enableHunt)
                {
                    AdjustHuntSpawnsForMap(map, mapConfig, mainConfig, spawns);
                }
            }
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

    private void AdjustHuntSpawnsForMap(string map, MapConfig? mapConfig, MainConfig mainConfig, List<BossLocationSpawn> spawns)
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