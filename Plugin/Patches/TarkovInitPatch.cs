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
            
        }
    }
}
