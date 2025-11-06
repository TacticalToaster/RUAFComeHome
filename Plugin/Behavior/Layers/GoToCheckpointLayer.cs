using DrakiaXYZ.BigBrain.Brains;
using EFT;
using System;
using RUAFComeHome.Behavior.Actions;
using RUAFComeHome.Components;
using RUAFComeHome.Controllers;
using UnityEngine;

namespace RUAFComeHome.Behavior.Layers
{
    internal class GoToCheckpointLayer : CustomLayer
    {
        protected BotRuafManager ruafManager { get; private set; }

        public static float innerRadius = 10f;
        public static float outerRadius = 45f;
        public static Vector3 patrolPoint = new Vector3(-140f, -1f, 410f);

        public Type lastAction;
        public Type nextAction;
        public string nextActionReason;

        public GoToCheckpointLayer(BotOwner botOwner, int priority) : base(botOwner, priority)
        {
            ruafManager = botOwner.GetOrAddRuafManager();
        }

        public override void Start()
        {
            base.Start();
        }

        public override void Stop()
        {
            ruafManager.guardPointDirty = true;
            ruafManager.AtCheckpoint = false;
            ruafManager.ShouldSwitchCover = false;
            base.Stop();
        }

        public override string GetName()
        {
            return "GuardCheckpointRuaf";
        }

        public void setNextAction(Type actionType, string reason)
        {
            nextAction = actionType;
            nextActionReason = reason;
        }

        public override Action GetNextAction()
        {
            lastAction = nextAction;

            return new Action(lastAction, nextActionReason);
        }

        public override bool IsActive()
        {
            if (!ruafManager.CanDoCheckpointActions())
                return false;

            
            getNextAction();
            BotOwner.PatrollingData.Pause();
            return true;
        }

        public void getNextAction()
        {
            lastAction = nextAction;

            if (ruafManager.AtCheckpoint)
            {
                if (ruafManager.ShouldSwitchCover)
                {
                    nextAction = typeof(SwitchCheckpointCover);
                    nextActionReason = "SwitchCover";
                    return;
                }
                nextAction = typeof(SitAtCheckpoint);
                nextActionReason = "AtCheckpoint";
                return;
            }
            nextAction = typeof(GoToCheckpoint);
            nextActionReason = "ToCheckpoint";
        }

        public override bool IsCurrentActionEnding()
        {
            return nextAction != lastAction;
        }
    }
}
