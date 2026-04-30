using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using RUAFComeHome.Components;
using RUAFComeHome.Patches;
using System;
using System.Collections.Generic;
using Comfort.Common;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using MoreBotsAPI.Behavior.Layers;
using MoreBotsAPI.Components;
using RUAFComeHome.Behavior.Layers;

namespace RUAFComeHome
{
    [BepInDependency("xyz.drakia.bigbrain", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("me.sol.sain", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.morebotsapi.tacticaltoaster")]
    [BepInPlugin(ClientInfo.GUID, ClientInfo.PluginName, ClientInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource LogSource;

        public ConfigEntry<bool> SpawnHunt;
        public ConfigEntry<bool> SpawnExUsecHunt;

        // BaseUnityPlugin inherits MonoBehaviour, so you can use base unity functions like Awake() and Update()
        private void Awake()
        {
            // save the Logger to variable so we can use it elsewhere in the project
            LogSource = Logger;

            new TarkovInitPatch().Enable();
            new BotOwnerActivatePatch().Enable();
            new BotsControllerInitPatch().Enable();

            this.GetOrAddComponent<RuafCheckpointManager>();
            
            var ruafEnums = new List<int> { 848400, 848401, 848402, 848403, 848404, 848405 }
                .ConvertAll(x => (WildSpawnType)x);
            
            var remnantEnums = new List<int> { 848406 }
                .ConvertAll(x => (WildSpawnType)x);
            
            MonoBehaviourSingleton<HuntManager>.Instance.AddHuntRoles(ruafEnums, new List<WildSpawnType>()
                {
                    WildSpawnType.exUsec
                });
            
            MonoBehaviourSingleton<HuntManager>.Instance.AddHuntRoles(remnantEnums, new List<WildSpawnType>()
            {
                WildSpawnType.exUsec
            });
            MonoBehaviourSingleton<HuntManager>.Instance.AddHuntRoles(new List<WildSpawnType>() { WildSpawnType.exUsec }, ruafEnums);
            
            MonoBehaviourSingleton<HuntManager>.Instance.AddHuntSides(remnantEnums, new List<EPlayerSide>()
                { 
                    EPlayerSide.Usec
                });

            MonoBehaviourSingleton<HuntManager>.Instance.OnBotHuntInit += manager =>
            {
                if (WildSpawnTypeExtensions.IsRemnant(manager.botOwner.Profile.Info.Settings.Role))
                {
                    foreach (var player in Singleton<GameWorld>.Instance.AllAlivePlayersList)
                    {
                        if (player.IsAI) continue;
                        if (MonoBehaviourSingleton<FactionManager>.Instance.ShouldRevengeByID(player.ProfileId, "ruaf"))
                        {
                            manager.botOwner.GetOrAddComponent<BotHuntManager>().priorityTargets.Add(player);
                        }
                    }
                }
            };

            var ruafBrainList = new List<string>() { "PMC", "Pmc", "ExUsec", "Assault", "PmcUsec", "PmcBear", "PmcUSEC", "PmcBEAR" };
            var ruafTypes = new List<int>() { 848400, 848401, 848402, 848403, 848404, 848405, 848406 }.ConvertAll(x => (WildSpawnType)x);

            BrainManager.AddCustomLayer(typeof(GoToCheckpointLayer), ruafBrainList, 4, ruafTypes);
            BrainManager.AddCustomLayer(typeof(HuntTargetLayer), ruafBrainList, 8, ruafTypes);
            BrainManager.AddCustomLayer(typeof(HuntTargetLayer), new List<string> { "ExUsec" }, 5, new List<WildSpawnType>{ WildSpawnType.exUsec });
            
            LogSource.LogInfo($"RUAFComeHome: LAYERS ADDED");
            //InitConfig();
        }

        private void InitConfig()
        {
            SpawnHunt = Config.Bind(
                "DEBUG",
                "Spawn RUAF hunt",
                false,
                "Spawn RUAF hunt"
                );
            SpawnExUsecHunt = Config.Bind(
                "DEBUG",
                "Spawn Rogue hunt",
                false,
                "Spawn Rogue hunt"
                );

            SpawnHunt.SettingChanged += SpawnRuafHunt;
            SpawnExUsecHunt.SettingChanged += SpawnRogueHunt;
        }

        private void SpawnRuafHunt(object sender, EventArgs e)
        {
            MonoBehaviourSingleton<HuntManager>.Instance.StartHunt("ruafHunt");
        }

        private void SpawnRogueHunt(object sender, EventArgs e)
        {
            MonoBehaviourSingleton<HuntManager>.Instance.StartHunt("exUsecHunt");
        }
    }
}
