using Mono.Cecil;
using MoreBotsAPI;
using System.Collections.Generic;

namespace RUAFComeHome.Prepatch
{
    public static class WildSpawnTypePatch
    {
        private const int BaseBrainType = 9;

        private const int RiflemanId = 848400;
        private const int SeniorRiflemanId = 848401;
        private const int AutoriflemanId = 848402;
        private const int GrenadierId = 848403;
        private const int MarksmanId = 848404;
        private const int MachinegunnerId = 848405;
        private const int RemnantRiflemanId = 848406;

        private static readonly List<int> ExcludedDifficulties = new()
        {
            0,
            2,
            3
        };

        private static readonly List<int> RuafGroup = new()
        {
            RiflemanId,
            SeniorRiflemanId,
            AutoriflemanId,
            GrenadierId,
            MarksmanId,
            MachinegunnerId
        };

        private static readonly List<int> RemnantGroup = new()
        {
            RemnantRiflemanId
        };

        public static IEnumerable<string> TargetDLLs { get; } = new[]
        {
            "Assembly-CSharp.dll"
        };

        public static void Patch(ref AssemblyDefinition assembly)
        {
            RegisterBot(assembly, RiflemanId, "ruafRifleman", "RUAF");
            RegisterBot(assembly, SeniorRiflemanId, "ruafRiflemanSenior", "RUAF");
            RegisterBot(assembly, AutoriflemanId, "ruafAutorifleman", "RUAF");
            RegisterBot(assembly, GrenadierId, "ruafGrenadier", "RUAF");
            RegisterBot(assembly, MarksmanId, "ruafMarksman", "RUAF");
            RegisterBot(assembly, MachinegunnerId, "ruafMachinegunner", "RUAF");

            RegisterBot(assembly, RemnantRiflemanId, "remnantRifleman", "REMNANT");

            CustomWildSpawnTypeManager.AddSuitableGroup(RuafGroup);
            CustomWildSpawnTypeManager.AddSuitableGroup(RemnantGroup);
        }

        private static void RegisterBot(
            AssemblyDefinition assembly,
            int id,
            string botDbKey,
            string role)
        {
            var bot = new CustomWildSpawnType(
                id,
                botDbKey,
                role,
                BaseBrainType,
                true,
                true,
                false);

            bot.SetCountAsBossForStatistics(false);
            bot.SetShouldUseFenceNoBossAttack(false, false);
            bot.SetExcludedDifficulties(ExcludedDifficulties);

            CustomWildSpawnTypeManager.RegisterWildSpawnType(bot, assembly);
        }
    }
}