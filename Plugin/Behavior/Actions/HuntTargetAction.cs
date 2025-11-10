using Comfort.Common;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using HarmonyLib;
using RUAFComeHome.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RUAFComeHome.Behavior.Actions
{
    internal class HuntTargetAction : CustomLogic
    {
        private float nextUpdate;
        private BotHuntManager huntManager;
        private FieldInfo botZoneField = null;

        public HuntTargetAction(BotOwner botOwner) : base(botOwner)
        {
            if (botZoneField == null)
            {
                botZoneField = AccessTools.Field(typeof(BotsGroup), "<BotZone>k__BackingField");
            }
        }

        public override void Start()
        {
            base.Start();

            huntManager = BotOwner.GetComponent<BotHuntManager>();

            BotOwner.Mover.Stop();
            BotOwner.PatrollingData.Pause();
        }

        public override void Stop()
        {
            base.Stop();
            BotOwner.PatrollingData.Unpause();
        }

        public override void Update(CustomLayer.ActionData data)
        {
            if (nextUpdate > Time.time) return;

            nextUpdate = Time.time + 3f;

            updateBotZone();

            BotOwner.Sprint(true);
            BotOwner.SetPose(1f);
            BotOwner.AimingManager.CurrentAiming.LoseTarget();

            BotOwner.GoToPoint(huntManager.knownLocation);

            if (BotOwner.Position.SqrDistance(huntManager.knownLocation) < 25f * 25f)
            {
                huntManager.shouldSearch = true;
            }
        }

        // taken from QuestingBots
        private void updateBotZone()
        {
            BotSpawner botSpawnerClass = Singleton<IBotGame>.Instance.BotsController.BotSpawner;
            BotZone closestBotZone = botSpawnerClass.GetClosestZone(BotOwner.Position, out float dist);

            if (BotOwner.BotsGroup.BotZone == closestBotZone)
            {
                return;
            }

            // Do not allow followers to set the BotZone
            if (!BotOwner.Boss.IamBoss && (BotOwner.BotsGroup.MembersCount > 1))
            {
                return;
            }

            botZoneField.SetValue(BotOwner.BotsGroup, closestBotZone);
            BotOwner.PatrollingData.PointChooser.ShallChangeWay(true);
        }
    }
}
