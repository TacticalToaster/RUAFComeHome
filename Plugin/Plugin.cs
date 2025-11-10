using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using RUAFComeHome.Components;
using RUAFComeHome.Patches;
using System;
using System.Collections.Generic;

namespace RUAFComeHome
{
    [BepInDependency("xyz.drakia.bigbrain", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("me.sol.sain", BepInDependency.DependencyFlags.SoftDependency)]
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
            var huntManger = this.GetOrAddComponent<HuntManager>();

            InitConfig();
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
