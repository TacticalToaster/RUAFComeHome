using BepInEx.Logging;
using Mono.Cecil;
using MoreBotsAPI;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace RUAFComeHome.Prepatch
{
    public static class WildSpawnTypePatch
    {
        public static IEnumerable<string> TargetDLLs { get; } = new[] { "Assembly-CSharp.dll" };

        public static void Patch(ref AssemblyDefinition assembly)
        {
            var ruafBrains = new List<string>() { "PMC", "ExUsec" };
            var ruafLayers = new List<string>() {
                "Request",
                //"FightReqNull",
                //"PeacecReqNull",
                "KnightFight",
                //"PtrlBirdEye",
				"PmcBear",
                "PmcUsec",
                "ExURequest"
            };

            int ruafBrainInt = 24;//9;

            // rifleman
            var ruafBot = new CustomWildSpawnType(848400, "ruafRifleman", "RUAF", ruafBrainInt, true, true, false);

            ruafBot.SetCountAsBossForStatistics(false);
            ruafBot.SetShouldUseFenceNoBossAttack(false, false);
            ruafBot.SetExcludedDifficulties(new List<int> { 0, 2, 3 });

            SAINSettings settings = new SAINSettings(ruafBot.WildSpawnTypeValue)
            {
                Name = "RUAF Rifleman",
                Description = "A regular rifleman.",
                Section = "RUAF",
                BaseBrain = "PMC",
                BrainsToApply = ruafBrains,
                LayersToRemove = ruafLayers
            };

            ruafBot.SetSAINSettings(settings);

            CustomWildSpawnTypeManager.RegisterWildSpawnType(ruafBot, assembly);

            // senior rifleman
            ruafBot = new CustomWildSpawnType(848401, "ruafRiflemanSenior", "RUAF", ruafBrainInt, true, true, false);

            ruafBot.SetCountAsBossForStatistics(false);
            ruafBot.SetShouldUseFenceNoBossAttack(false, false);
            ruafBot.SetExcludedDifficulties(new List<int> { 0, 2, 3 });

            settings = new SAINSettings(ruafBot.WildSpawnTypeValue)
            {
                Name = "RUAF Senior Rifleman",
                Description = "A NCO rifleman with better rifles.",
                Section = "RUAF",
                BaseBrain = "PMC",
                BrainsToApply = ruafBrains,
                LayersToRemove = ruafLayers
            };

            ruafBot.SetSAINSettings(settings);

            CustomWildSpawnTypeManager.RegisterWildSpawnType(ruafBot, assembly);

            // autorifleman
            ruafBot = new CustomWildSpawnType(848402, "ruafAutorifleman", "RUAF", ruafBrainInt, true, true, false);

            ruafBot.SetCountAsBossForStatistics(false);
            ruafBot.SetShouldUseFenceNoBossAttack(false, false);
            ruafBot.SetExcludedDifficulties(new List<int> { 0, 2, 3 });

            settings = new SAINSettings(ruafBot.WildSpawnTypeValue)
            {
                Name = "RUAF Autorifleman",
                Description = "A regular equipped with a SAW/LMG.",
                Section = "RUAF",
                BaseBrain = "PMC",
                BrainsToApply = ruafBrains,
                LayersToRemove = ruafLayers
            };

            ruafBot.SetSAINSettings(settings);

            CustomWildSpawnTypeManager.RegisterWildSpawnType(ruafBot, assembly);

            // grenadier
            ruafBot = new CustomWildSpawnType(848403, "ruafGrenadier", "RUAF", ruafBrainInt, true, true, false);

            ruafBot.SetCountAsBossForStatistics(false);
            ruafBot.SetShouldUseFenceNoBossAttack(false, false);
            ruafBot.SetExcludedDifficulties(new List<int> { 0, 2, 3 });

            settings = new SAINSettings(ruafBot.WildSpawnTypeValue)
            {
                Name = "RUAF Grenadier",
                Description = "A grenadier equipped with a grenade launcher.",
                Section = "RUAF",
                BaseBrain = "PMC",
                BrainsToApply = ruafBrains,
                LayersToRemove = ruafLayers
            };

            ruafBot.SetSAINSettings(settings);

            CustomWildSpawnTypeManager.RegisterWildSpawnType(ruafBot, assembly);

            // marksman
            ruafBot = new CustomWildSpawnType(848404, "ruafMarksman", "RUAF", ruafBrainInt, true, true, false);

            ruafBot.SetCountAsBossForStatistics(false);
            ruafBot.SetShouldUseFenceNoBossAttack(false, false);
            ruafBot.SetExcludedDifficulties(new List<int> { 0, 2, 3 });

            settings = new SAINSettings(ruafBot.WildSpawnTypeValue)
            {
                Name = "RUAF Marksman",
                Description = "A marksman equipped with a DMR.",
                Section = "RUAF",
                BaseBrain = "PMC",
                BrainsToApply = ruafBrains,
                LayersToRemove = ruafLayers
            };

            ruafBot.SetSAINSettings(settings);

            CustomWildSpawnTypeManager.RegisterWildSpawnType(ruafBot, assembly);

            // machinegunner
            ruafBot = new CustomWildSpawnType(848405, "ruafMachinegunner", "RUAF", ruafBrainInt, true, true, false);

            ruafBot.SetCountAsBossForStatistics(false);
            ruafBot.SetShouldUseFenceNoBossAttack(false, false);
            ruafBot.SetExcludedDifficulties(new List<int> { 0, 2, 3 });

            settings = new SAINSettings(ruafBot.WildSpawnTypeValue)
            {
                Name = "RUAF Machinegunner",
                Description = "A machinegunner equipped with a MMG.",
                Section = "RUAF",
                BaseBrain = "PMC",
                BrainsToApply = ruafBrains,
                LayersToRemove = ruafLayers
            };

            ruafBot.SetSAINSettings(settings);

            CustomWildSpawnTypeManager.RegisterWildSpawnType(ruafBot, assembly);

            CustomWildSpawnTypeManager.AddSuitableGroup(new List<int> { 848400, 848401, 848402, 848403, 848404, 848405 });
        }

    }
}