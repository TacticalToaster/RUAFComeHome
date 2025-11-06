using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RUAFComeHome.Components;

namespace RUAFComeHome.Controllers
{
    public static class BotRuafManagerFactory
    {
        private static Dictionary<BotOwner, BotRuafManager> managers = new Dictionary<BotOwner, BotRuafManager>();

        public static BotRuafManager GetManager(this BotOwner botOwner)
        {
            if (managers.TryGetValue(botOwner, out var manager))
            {
                return manager;
            }
            return null;
        }

        public static BotRuafManager GetOrAddRuafManager(this BotOwner botOwner)
        {
            var manager = GetManager(botOwner);

            if (manager != null)
            {
                return manager;
            }

            manager = botOwner.GetPlayer.gameObject.GetOrAddComponent<BotRuafManager>();
            managers[botOwner] = manager;
            manager.Init(botOwner);

            return manager;
        }
    }
}
