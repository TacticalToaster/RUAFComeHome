using Comfort.Common;
using EFT;
using SPT.SinglePlayer.Utils.InRaid;
using System.Diagnostics;
using RUAFComeHome.Models;
using UnityEngine;

namespace RUAFComeHome.Components
{
    public class BotRuafManager : MonoBehaviour
    {
        public BotOwner botOwner;
        public bool AtCheckpoint = false;
        public bool ShouldSwitchCover = false;
        public bool guardPointDirty = true;
        public bool hasTeleportedToCheckpoint = false;
        public CustomNavigationPoint guardPoint;

        private RuafCheckpoint assignedCheckpoint;

        public void Init(BotOwner botOwner)
        {
            this.botOwner = botOwner;

            if (!WildSpawnTypeExtensions.IsRUAF(botOwner.Profile.Info.Settings.Role))
                return;


            if (RaidTimeUtil.GetSecondsSinceSpawning() <= 120)
            {
                Plugin.LogSource.LogInfo($"[{botOwner.Profile.Nickname}] Attempting to find and assign checkpoint on spawn.");
                MonoBehaviourSingleton<RuafCheckpointManager>.Instance.FindAndAssignCheckpoint(botOwner);
                Plugin.LogSource.LogInfo($"[{botOwner.Profile.Nickname}] Assigned checkpoint: {(HasAssignedCheckpoint() ? GetAssignedCheckpoint().Zone.NameZone : "None")}");

                if (HasAssignedCheckpoint())
                {
                    botOwner.Mover.Teleport(assignedCheckpoint.Position);
                    //botOwner.Mover.GoToPoint(assignedCheckpoint.Position, true, 1f, true);
                }
            }
        }

        public void OnBotActivate()
        {
            if (!WildSpawnTypeExtensions.IsRUAF(botOwner.Profile.Info.Settings.Role))
                return;

            CheckPlayerLoyalty();
        }

        public void CheckPlayerLoyalty()
        {
            var allBots = Singleton<IBotGame>.Instance.BotsController.Players;
            var player = Singleton<GameWorld>.Instance.AllAlivePlayersList[0];

            var pkLevel = player.Profile.GetTraderLoyalty("54cb50c76803fa8b248b4571"); // Prapor loyalty level

            Plugin.LogSource.LogInfo($"Checking Prapor loyalty {pkLevel}. Is the bot active? {botOwner.AIData.BotOwner.BotState}");

            if (pkLevel < 3)
                return;

            foreach (var groupie in Singleton<GameWorld>.Instance.AllAlivePlayersList.GroupPlayers(player.GroupId))
            {
                botOwner.BotsGroup.RemoveEnemy(groupie);
                botOwner.BotsGroup.AddAlly((Player)groupie);
                botOwner.Memory.DeleteInfoAboutEnemy(groupie);
            }

            /*foreach (IPlayer bot in allBots)
            {
                if ((!bot.AIData.IsAI || bot.AIData.BotOwner.BotState == EBotState.Active) && bot.IsAI && WildSpawnTypeExtensions.IsRUAF(bot.Profile.Info.Settings.Role))
                {
                    bot.AIData.BotOwner.BotsGroup.RemoveEnemy(player);
                    bot.AIData.BotOwner.BotsGroup.AddAlly(player);

                    foreach (Player potentialGroupMember in Singleton<GameWorld>.Instance.AllAlivePlayersList)
                    {
                        if (!string.IsNullOrEmpty(potentialGroupMember.GroupId) && potentialGroupMember.GroupId == player.GroupId)
                        {
                            bot.AIData.BotOwner.BotsGroup.RemoveEnemy(potentialGroupMember);
                            bot.AIData.BotOwner.BotsGroup.AddAlly(potentialGroupMember);
                        }
                    }

                    if (bot.AIData.BotOwner.BotState == EBotState.Active)
                    {
                        player.AIData.BotOwner.Brain.BaseBrain.CalcActionNextFrame();
                    }
                }
            }*/
        }

        public CustomNavigationPoint GetCheckpointCoverPoint()
        {
            var checkpoint = GetAssignedCheckpoint();
            var patrolPoint = checkpoint.Position;
            var randomPoint = patrolPoint + (Vector3)UnityEngine.Random.insideUnitCircle * checkpoint.Radius;

            var searchData = new CoverSearchData(randomPoint, botOwner.CoverSearchInfo, CoverShootType.hide, (checkpoint.Radius * .5f) * (checkpoint.Radius * .5f), 0f, CoverSearchType.distToToCenter, null, null, null, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(botOwner.Settings.FileSettings.Cover.MIN_DEFENCE_LEVEL), PointsArrayType.byShootType, true);
            return botOwner.BotsGroup.CoverPointMaster.GetCoverPointMain(searchData, true);
        }

        public void UpdateGuardPoint()
        {
            if (guardPointDirty)
            {
                Plugin.LogSource.LogInfo($"[{botOwner.Profile.Nickname}] Updating guard point...");
                guardPoint = GetCheckpointCoverPoint();
                guardPointDirty = false;
            }
        }

        public void SetGuardPoint(CustomNavigationPoint point)
        {
            Plugin.LogSource.LogInfo($"[{botOwner.Profile.Nickname}] Setting guard point to {point.Position}");
            guardPoint = point;
        }

        public bool CanDoCheckpointActions()
        {
            //Plugin.LogSource.LogInfo($"[{botOwner.Profile.Nickname}] can do checkpoint action? {HasAssignedCheckpoint() && stopwatch.ElapsedMilliseconds >= 5000}");
            return HasAssignedCheckpoint() && RaidTimeUtil.GetSecondsSinceSpawning() > 0;
        }

        public bool HasAssignedCheckpoint()
        {
            return assignedCheckpoint != null;
        }

        public RuafCheckpoint GetAssignedCheckpoint()
        {
            return assignedCheckpoint;
        }

        public void SetAssignedCheckpoint(RuafCheckpoint checkpoint)
        {
            assignedCheckpoint = checkpoint;
        }
    }
}
