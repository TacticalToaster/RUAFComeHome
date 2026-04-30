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
using MoreBotsServer.Services;
using SPTarkov.Server.Core.Models.Eft.Match;

namespace RUAFComeHomeServer.Controllers;

[Injectable(InjectionType.Singleton)]
public class RUAFSpawnController(
    JsonUtil jsonUtil,
    RandomUtil randomUtil,
    ConfigController configController,
    DatabaseService databaseService,
    FactionService factionService,
    RUAFLogger logger,
    HttpResponseUtil httpResponse
)
{
    public float RemnantChance = 1f;
    
    public void AdjustAllRuafSpawns(EndLocalRaidRequestData info, MongoId sessionId, string output)
    {
        var revenges = factionService.GetFactionsRevenges();
        if (revenges.ContainsKey(info?.Results?.Profile?.Id ?? "") &&
            revenges[info.Results.Profile.Id].Contains("ruaf"))
            RemnantChance += 1f;
        else
        {
            RemnantChance -= .5f;
            if (RemnantChance < 1f) RemnantChance = 1f;
        }
        
        AdjustAllRuafSpawns();
    }
    
    public void AdjustAllRuafSpawns()
    {
        try
        {
            var tables = databaseService.GetTables();
            var locations = databaseService.GetLocations();
            var mainConfig = configController.ModConfig;

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
                var location = locations.GetDictionary()[locations.GetMappedKey(map)].Base;
                var spawns = location.BossLocationSpawn;

                // Remove existing RUAF spawns
                spawns.RemoveAll(x => x.BossName.Contains("ruaf"));

                if (patrolConfig.enablePatrols)
                {
                    AdjustPatrolSpawnsForMap(location, mapConfig, mainConfig, spawns);
                }

                if (checkpointConfig.enableCheckpoints)
                {
                    AdjustCheckpointSpawnsForMap(location, mapConfig, mainConfig, spawns);
                }

                if (huntConfig.enableHunts)
                {
                    AdjustHuntSpawnsForMap(location, mapConfig, mainConfig, spawns);
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error($"Error adjusting RUAF spawns: {ex.Message}");
            throw;
        }
    }

    private void AdjustPatrolSpawnsForMap(LocationBase location, MapConfig mapConfig, MainConfig mainConfig, List<BossLocationSpawn> spawns)
    {
        var patrolConfig = mapConfig.patrol;

        logger.Info($"Enabling RUAF patrols for {location.Name}.");
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
                logger.Info($"Instantly spawning RUAF patrol for {location.Name}.");
                patrol.Time = -1;
            }

            spawns.Add(patrol);

            logger.Info($"Added ({patrolConfig.patrolChance}% chance) RUAF patrol of size {patrolSize} to {location.Name} in zone {patrol.BossZone} with a spawn time of {patrol.Time} seconds.");
        }
    }

    private void AdjustCheckpointSpawnsForMap(LocationBase location, MapConfig mapConfig, MainConfig mainConfig, List<BossLocationSpawn> spawns)
    {
        var checkpointConfig = mapConfig.checkpoint;

        logger.Info($"Enabling RUAF checkpoint for {location.Name}.");
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
                logger.Info($"Instantly spawning RUAF checkpoint for {location.Name}.");
                patrol.Time = -1;
            }

            spawns.Add(patrol);

            logger.Info($"Added ({checkpointZoneConfig.checkpointChance}% chance) RUAF checkpoint of size {patrolSize} to {location.Name} in zone {patrol.BossZone} with a spawn time of {patrol.Time} seconds.");
        }
    }

    private void AdjustHuntSpawnsForMap(LocationBase location, MapConfig mapConfig, MainConfig mainConfig, List<BossLocationSpawn> spawns)
    {
        if (mapConfig.hunt.hunts.ContainsKey("ruaf"))
        {
            spawns.RemoveAll(x => x.TriggerId == "hunt" && x.BossName.Contains("ruaf"));
            AddRuafHuntToMap(location, mapConfig, mainConfig, spawns);
        }
        
        if (mapConfig.hunt.hunts.ContainsKey("remnant"))
        {
            spawns.RemoveAll(x => x.TriggerId == "hunt" && x.BossName.Contains("remnant"));
            AddRemnantHuntToMap(location, mapConfig, mainConfig, spawns, GetRemnantChanceMod());
        }

        if (mapConfig.hunt.hunts.ContainsKey("exUsec"))
        {
            spawns.RemoveAll(x => x.TriggerId == "hunt" && x.BossName.Contains("exUsec"));
            AddExUsecHuntToMap(location, mapConfig, mainConfig, spawns);
        }
    }

    private void AddRuafHuntToMap(LocationBase location, MapConfig? mapConfig, MainConfig mainConfig, List<BossLocationSpawn> spawns)
    {
        var huntConfig = mapConfig.hunt.hunts["ruaf"];

        logger.Info($"Enabling RUAF hunt for {location.Name}.");

        var patrolSize = randomUtil.GetInt(huntConfig.huntMin, huntConfig.huntMax);
        var patrol = GeneratePatrol(patrolSize, mainConfig.debug.spawnAlways ? 100 : huntConfig.huntChance, false);

        patrol.Time = (location.EscapeTimeLimit ?? 45) * randomUtil.GetDouble(0.1, 0.9) * 60;

        patrol.BossZone = huntConfig.huntZones;
        patrol.TriggerName = "botEvent";
        patrol.TriggerId = "hunt";
        patrol.ForceSpawn = true;

        spawns.Add(patrol);

        logger.Info($"Added RUAF Hunt of size {patrolSize} to {location.Name} in zone {patrol.BossZone} with a spawn time of {patrol.Time} seconds.");
    }
    
    private void AddRemnantHuntToMap(LocationBase location, MapConfig? mapConfig, MainConfig mainConfig, List<BossLocationSpawn> spawns, float chanceMod = 1)
    {
        var huntConfig = mapConfig.hunt.hunts["remnant"];

        logger.Info($"Enabling Remnant hunt for {location.Name}.");

        var patrolSize = randomUtil.GetInt(huntConfig.huntMin, huntConfig.huntMax);
        var patrol = GenerateRemnantPatrol(patrolSize, mainConfig.debug.spawnAlways ? 100 : huntConfig.huntChance * chanceMod, false);

        patrol.Time = (location.EscapeTimeLimit ?? 45) * randomUtil.GetDouble(0.01, 0.02) * 60;

        patrol.BossZone = huntConfig.huntZones;
        patrol.TriggerName = "botEvent";
        patrol.TriggerId = "hunt";
        patrol.ForceSpawn = true;

        spawns.Add(patrol);

        logger.Info($"Added Remnant Hunt of size {patrolSize} to {location.Name} in zone {patrol.BossZone} with a spawn time of {patrol.Time} seconds.");
    }

    private void AddExUsecHuntToMap(LocationBase location, MapConfig? mapConfig, MainConfig mainConfig, List<BossLocationSpawn> spawns)
    {
        var huntConfig = mapConfig.hunt.hunts["exUsec"];

        logger.Info($"Enabling EXUSEC hunt for {location.Name}.");

        var patrolSize = randomUtil.GetInt(huntConfig.huntMin, huntConfig.huntMax);
        var patrol = new BossLocationSpawn
        {
            BossChance = huntConfig.huntChance,
            BossDifficulty = "normal",
            BossEscortAmount = patrolSize.ToString(),
            BossEscortDifficulty = "normal",
            BossEscortType = "exUsec",
            BossName = "exUsec",
            IsBossPlayer = false,
            BossZone = string.Empty,
            ForceSpawn = false,
            IgnoreMaxBots = false,
            IsRandomTimeSpawn = false,
            SpawnMode = new[] { "regular", "pve" },
            Supports = new List<BossSupport>(),
            Time = -1,
            TriggerId = string.Empty,
            TriggerName = string.Empty
        };

        patrol.Time = (location.EscapeTimeLimit ?? 45) * randomUtil.GetDouble(0.1, 0.9) * 60;

        patrol.BossZone = huntConfig.huntZones;
        patrol.TriggerName = "botEvent";
        patrol.TriggerId = "hunt";
        patrol.ForceSpawn = true;

        spawns.Add(patrol);

        logger.Info($"Added EXUSEC Hunt of size {patrolSize} to {location.Name} in zone {patrol.BossZone} with a spawn time of {patrol.Time} seconds.");
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
            IgnoreMaxBots = false,
            IsRandomTimeSpawn = false,
            SpawnMode = new[] { "regular", "pve" },
            Supports = supportsList,
            Time = -1,
            TriggerId = string.Empty,
            TriggerName = string.Empty
        };

        return bossInfo;
    }
    
    private BossLocationSpawn GenerateRemnantPatrol(int patrolSize, float chance, bool isPatrol = true)
    {
        var bossType = "remnantRifleman";
        var followers = patrolSize - 1;

        logger.Info($"Generating Remnant patrol of size {patrolSize}.");
        

        var bossInfo = new BossLocationSpawn
        {
            BossChance = chance,
            BossDifficulty = "normal",
            BossEscortAmount = followers.ToString(),
            BossEscortDifficulty = "normal",
            BossEscortType = "remnantRifleman",
            BossName = bossType,
            IsBossPlayer = false,
            BossZone = string.Empty,
            ForceSpawn = true,
            IgnoreMaxBots = true,
            IsRandomTimeSpawn = false,
            SpawnMode = new[] { "regular", "pve" },
            Supports = new List<BossSupport>(),
            Time = -1,
            TriggerId = string.Empty,
            TriggerName = string.Empty
        };//

        return bossInfo;
    }

    public float GetRemnantChanceMod()
    {
        return RemnantChance;
    }
}