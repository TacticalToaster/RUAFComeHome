using DrakiaXYZ.BigBrain.Brains;
using EFT;
using RUAFComeHome.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RUAFComeHome.Behavior.Actions
{

    internal class SearchForTargetAction : CustomLogic
    {
        private BotNodeAbstractClass baseAction;
        private float endTime;
        private BotHuntManager huntManager;

        public SearchForTargetAction(BotOwner botOwner) : base(botOwner)
        {
            baseAction = BotActionNodesClass.CreateNode(BotLogicDecision.search, botOwner);
        }

        public override void Start()
        {
            base.Start();

            huntManager = BotOwner.GetComponent<BotHuntManager>();

            BotOwner.Mover.Stop();

            endTime = Time.time + UnityEngine.Random.Range(25f, 45f);
        }

        public override void Stop()
        {
            base.Stop();
        }

        public override void Update(CustomLayer.ActionData data)
        {
            baseAction.UpdateNodeByMain(data);

            if (endTime < Time.time)
            {
                huntManager.shouldSearch = false;
            }
        }
    }
}
