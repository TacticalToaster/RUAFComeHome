using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using RUAFComeHome.Components;
using RUAFComeHome.Patches;
using System;
using System.Collections.Generic;
using EFT;
using MoreBotsAPI.Components;

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
            
            MonoBehaviourSingleton<HuntManager>.Instance.AddHuntRoles(ruafEnums, new List<WildSpawnType>()
                {
                    WildSpawnType.exUsec
                });
            MonoBehaviourSingleton<HuntManager>.Instance.AddHuntRoles(new List<WildSpawnType>() { WildSpawnType.exUsec }, ruafEnums);
            /*
             MonoBehaviourSingleton<HuntManager>.Instance.AddHuntSides(ruafEnums, new List<EPlayerSide>()
                {
                    EPlayerSide.Usec
                });
            */

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
