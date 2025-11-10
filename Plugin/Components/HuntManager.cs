using Comfort.Common;
using EFT;
using SPT.SinglePlayer.Utils.InRaid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RUAFComeHome.Components
{
    public class HuntManager : MonoBehaviourSingleton<HuntManager>
    {

        public Dictionary<BotsGroup, IPlayer> huntGroups = new();
        public Dictionary<string, WildSpawnType> huntEvents = new() {
            { "exUsecHunt", WildSpawnType.exUsec },
            { "ruafHunt", (WildSpawnType)848401 }
        };

        private bool startNewHunt = false;
        private WildSpawnType huntRole;
        private BotsGroup newHuntGroup;
        private List<string> unpickedHunts;

        private float nextUpdate = 0f;

        public void Update()
        {
            
            if (nextUpdate > Time.time || Singleton<GameWorld>.Instantiated == false) return;

            nextUpdate = Time.time + 240f;

            if (UnityEngine.Random.Range(0, 100) <= 30 && unpickedHunts.Count > 0)
            {
                var randomEvent = unpickedHunts.Random();
                unpickedHunts.Remove(randomEvent);
                StartHunt(randomEvent);
            }
        }

        public void StartHunt(string huntEvent)
        {
            huntRole = huntEvents[huntEvent];
            startNewHunt = true;
            Singleton<BotEventHandler>.Instance.AnyEvent(huntEvent);
            Plugin.LogSource.LogInfo($"[RUAF] Starting hunt event {huntEvent}");
        }

        public void InitRaid()
        {
            var ruafRoles = new List<WildSpawnType> { (WildSpawnType)848400, (WildSpawnType)848401, (WildSpawnType)848402, (WildSpawnType)848403, (WildSpawnType)848404, (WildSpawnType)848405 };
            AddHuntRoles(ruafRoles, new List<WildSpawnType> { WildSpawnType.exUsec });

            unpickedHunts = [.. huntEvents.Keys];

            nextUpdate = Time.time + 240f;

            Singleton<IBotGame>.Instance.BotsController.BotSpawner.OnBotCreated += OnBotCreated;
        }

        public void OnBotCreated(BotOwner bot)
        {
            var skipCheck = false;

            if (startNewHunt && bot.Profile.Info.Settings.Role == huntRole)
            {
                newHuntGroup = bot.BotsGroup;
                startNewHunt = false;
                skipCheck = true;
            }

            if (!huntGroups.ContainsKey(bot.BotsGroup) && skipCheck == false) return;

            var huntManager = bot.gameObject.GetOrAddComponent<BotHuntManager>();
            huntManager.Init(bot);
            FindFirstHuntTarget(huntManager);
        }

        public void AddHuntTarget(BotsGroup hunters, IPlayer hunted)
        {
            huntGroups.Add(hunters, hunted);
        }

        public void FindFirstHuntTarget(BotHuntManager hunter)
        {
            var role = hunter.botOwner.Profile.Info.Settings.Role;
            var allBots = Singleton<IBotGame>.Instance.BotsController.Players;

            if (huntGroups.TryGetValue(hunter.botOwner.BotsGroup, out var player))
            {
                hunter.huntTarget = player;
                return;
            }

            foreach (var bot in allBots)
            {
                if (!bot.HealthController.IsAlive) continue;

                var targetRole = bot.Profile.Info.Settings.Role;

                if (validHuntRoles.TryGetValue(role, out var huntList) && huntList.Contains(targetRole))
                {
                    if (!huntGroups.ContainsKey(hunter.botOwner.BotsGroup))
                    {
                        AddHuntTarget(hunter.botOwner.BotsGroup, bot);
                    }

                    hunter.huntTarget = huntGroups[hunter.botOwner.BotsGroup];
                    return;
                }
            }
        }

        public void FindNewHuntTarget(BotHuntManager hunter)
        {
            var role = hunter.botOwner.Profile.Info.Settings.Role;
            var allBots = Singleton<IBotGame>.Instance.BotsController.Players;

            foreach (var bot in allBots)
            {
                if (!bot.HealthController.IsAlive) continue;

                var targetRole = bot.Profile.Info.Settings.Role;

                if (validHuntRoles.TryGetValue(role, out var huntList) && huntList.Contains(targetRole))
                {
                    if (!huntGroups.ContainsKey(hunter.botOwner.BotsGroup))
                    {
                        AddHuntTarget(hunter.botOwner.BotsGroup, bot);
                    }
                    else
                    {
                        if (bot == huntGroups[hunter.botOwner.BotsGroup]) continue;

                        huntGroups[hunter.botOwner.BotsGroup] = bot;
                    }

                    hunter.huntTarget = huntGroups[hunter.botOwner.BotsGroup];
                    return;
                }
            }
        }

        public void AddHuntRoles(WildSpawnType hunter, List<WildSpawnType> hunted)
        {
            validHuntRoles.Add(hunter, hunted);
        }

        public void AddHuntRoles(List<WildSpawnType> hunters, List<WildSpawnType> hunted)
        {
            foreach (var hunter in hunters)
            {
                AddHuntRoles(hunter, hunted);
            }
        }

        public readonly Dictionary<WildSpawnType, List<WildSpawnType>> validHuntRoles = new() {
            { WildSpawnType.exUsec, new() { (WildSpawnType)848400, (WildSpawnType)848401, (WildSpawnType)848402, (WildSpawnType)848403, (WildSpawnType)848404, (WildSpawnType)848405 } }
        };
    }
}
