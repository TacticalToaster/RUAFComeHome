using EFT;
using RUAFComeHome.Controllers;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RUAFComeHome.Patches
{
    internal class BotOwnerActivatePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner).GetMethod(nameof(BotOwner.method_10), BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        protected static void PatchPostfix(BotOwner __instance)
        {
            if (!WildSpawnTypeExtensions.IsRUAF(__instance.Profile.Info.Settings.Role))
                return;

            var manager = __instance.GetOrAddRuafManager();
            manager.OnBotActivate();
        }
    }
}
