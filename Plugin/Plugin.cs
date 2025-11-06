using BepInEx;
using BepInEx.Logging;
using RUAFComeHome.Components;
using RUAFComeHome.Patches;

namespace RUAFComeHome
{
    [BepInDependency("xyz.drakia.bigbrain", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("me.sol.sain", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(ClientInfo.GUID, ClientInfo.PluginName, ClientInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource LogSource;

        // BaseUnityPlugin inherits MonoBehaviour, so you can use base unity functions like Awake() and Update()
        private void Awake()
        {
            // save the Logger to variable so we can use it elsewhere in the project
            LogSource = Logger;

            new TarkovInitPatch().Enable();
            new BotOwnerActivatePatch().Enable();
            new BotsControllerInitPatch().Enable();

            this.GetOrAddComponent<RuafCheckpointManager>();
        }
    }
}
