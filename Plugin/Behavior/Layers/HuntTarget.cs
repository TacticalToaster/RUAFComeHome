using DrakiaXYZ.BigBrain.Brains;
using EFT;
using RUAFComeHome.Behavior.Actions;
using RUAFComeHome.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RUAFComeHome.Behavior.Layers
{
    internal class HuntTargetLayer : CustomLayer
    {
        public BotHuntManager huntManager;

        public Type lastAction;
        public Type nextAction;
        public string nextActionReason;

        public override string GetName()
        {
            return "HuntTarget";
        }

        public HuntTargetLayer(BotOwner botOwner, int priority) : base(botOwner, priority)
        {
        }

        public override void Start()
        {
            base.Start();
        }

        public override void Stop()
        {
            base.Stop();
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
            if (huntManager == null)
            {
                if (BotOwner.TryGetComponent<BotHuntManager>(out var manager)) huntManager = manager;
                else return false;
            }

            if (!huntManager.HasHuntTarget())
                return false;


            getNextAction();
            return true;
        }

        public void getNextAction()
        {
            lastAction = nextAction;

            if (huntManager.shouldSearch)
            {
                nextAction = typeof(SearchForTargetAction);
                nextActionReason = "SearchingForTarget";
                return;
            }
            nextAction = typeof(HuntTargetAction);
            nextActionReason = "HuntingTarget";
        }

        public override bool IsCurrentActionEnding()
        {
            return nextAction != lastAction;
        }
    }
}
