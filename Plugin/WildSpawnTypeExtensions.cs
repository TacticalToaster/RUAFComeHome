using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RUAFComeHome
{
    public static class WildSpawnTypeExtensions
    {
        public static List<int> RUAFEnums = new List<int> { 848400, 848401, 848402, 848403, 848404, 848405 };
        public static List<int> RemnantEnums = new List<int> { 848406 };

        public static bool IsRUAF(WildSpawnType type)
        {
            return RUAFEnums.Contains((int)type);
        }

        public static bool IsRemnant(WildSpawnType type)
        {
            return RemnantEnums.Contains((int)type);
        }
    }
}
