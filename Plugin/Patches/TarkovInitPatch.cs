using DrakiaXYZ.BigBrain.Brains;
using EFT;
using EFT.InputSystem;
using RUAFComeHome.Behavior.Layers;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Reflection;

namespace RUAFComeHome.Patches
{
    internal class TarkovInitPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(TarkovApplication).GetMethod(nameof(TarkovApplication.Init), BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        protected static void PatchPostfix(IAssetsManager assetsManager, InputTree inputTree)
        {
            var ruafBrainList = new List<string>() { "PMC", "ExUsec", "Assault", "PmcUsec", "PmcBear", "PmcUSEC", "PmcBEAR" };
            var ruafTypes = new List<int>() { 848400, 848401, 848402, 848403, 848404, 848405 }.ConvertAll(x => (WildSpawnType)x);

            BrainManager.AddCustomLayer(typeof(GoToCheckpointLayer), ruafBrainList, 4, ruafTypes);
            BrainManager.AddCustomLayer(typeof(HuntTargetLayer), ruafBrainList, 5, ruafTypes);
            BrainManager.AddCustomLayer(typeof(HuntTargetLayer), new List<string> { "ExUsec" }, 5, new List<WildSpawnType>{ WildSpawnType.exUsec });
        }
    }
}
