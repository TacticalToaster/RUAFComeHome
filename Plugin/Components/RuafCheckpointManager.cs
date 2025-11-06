using Comfort.Common;
using EFT;
using Newtonsoft.Json;
using SPT.Common.Http;
using SPT.SinglePlayer.Utils.InRaid;
using System.Collections.Generic;
using RUAFComeHome.Controllers;
using RUAFComeHome.Models;
using UnityEngine;

namespace RUAFComeHome.Components
{
    public class RuafCheckpointManager : MonoBehaviourSingleton<RuafCheckpointManager>
    {
        public Dictionary<BotZone, RuafCheckpoint> ZoneCheckpoints = new Dictionary<BotZone, RuafCheckpoint>();

        public void InitRaid()
        {
            Plugin.LogSource.LogInfo("Initializing RuafCheckpointManager for raid...");
            LoadCheckpoints();
        }

        public void FindAndAssignCheckpoint(BotOwner botOwner)
        {
            Plugin.LogSource.LogInfo($"[{botOwner.Profile.Nickname}] Finding and assigning checkpoint...");

            var botSpawner = Singleton<IBotGame>.Instance.BotsController.BotSpawner;
            var botZone = botOwner.SpawnBotZone != null ? botOwner.SpawnBotZone : botSpawner.GetClosestZone(botOwner.Position, out float dist);

            Plugin.LogSource.LogInfo($"[{botOwner.Profile.Nickname}] Spawn Bot zone: {(botZone != null ? botZone.NameZone : "None")}");

            if (botZone != null)
            {
                var checkpoint = GetCheckpointForZone(botZone);
                if (checkpoint != null)
                {
                    AssignBotToCheckpoint(botOwner, checkpoint);
                }
            }
        }

        public void AssignBotToCheckpoint(BotOwner botOwner, RuafCheckpoint checkpoint)
        {
            var botRuafManager = botOwner.GetOrAddRuafManager();
            botRuafManager.SetAssignedCheckpoint(checkpoint);
            checkpoint.AssignedBots.Add(botOwner);
        }

        public RuafCheckpoint GetCheckpointForZone(BotZone zone)
        {
            if (ZoneCheckpoints.TryGetValue(zone, out var checkpoint))
            {
                Plugin.LogSource.LogInfo($"Found checkpoint {checkpoint.Position} for zone {zone.NameZone}");
                return checkpoint;
            }
            return null;
        }

        public void LoadCheckpoints()
        {
            var result = RequestHandler.GetJson("/ruaf/checkpoints");
            Plugin.LogSource.LogInfo($"Loading checkpoints from config... {RaidChangesUtil.LocationId}");
            var mainConfig = JsonConvert.DeserializeObject<MainConfig>(result);

            if (!mainConfig.locations.ContainsKey(RaidChangesUtil.LocationId.ToLower()))
            {
                Plugin.LogSource.LogWarning($"No configuration found for location {RaidChangesUtil.LocationId}");
                return;
            }

            var checkpointConfig = mainConfig.locations[RaidChangesUtil.LocationId.ToLower()].checkpoint;

            ZoneCheckpoints.Clear();
            
            if (!checkpointConfig.enableCheckpoints)
                return;

            var botSpawner = Singleton<IBotGame>.Instance.BotsController.BotSpawner;

            foreach (var zoneConfig in checkpointConfig.checkpointZones)
            {
                var zone = botSpawner.GetZoneByName(zoneConfig.checkpointZone);
                if (zone != null)
                {
                    var ruafCheckpoint = new RuafCheckpoint
                    (
                        zone,
                        new Vector3(zoneConfig.x, zoneConfig.y, zoneConfig.z),
                        zoneConfig.checkpointRadius
                    );

                    ZoneCheckpoints[zone] = ruafCheckpoint;
                }
            }
        }
    }
}
