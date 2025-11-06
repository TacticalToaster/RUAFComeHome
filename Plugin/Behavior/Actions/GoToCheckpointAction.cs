using DrakiaXYZ.BigBrain.Brains;
using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RUAFComeHome.Components;
using RUAFComeHome.Controllers;
using UnityEngine;

namespace RUAFComeHome.Behavior.Actions
{
    public class GoToCheckpoint : GoToCustomAction
    {
        protected BotRuafManager ruafManager { get; private set; }


        public GoToCheckpoint(BotOwner botOwner) : base(botOwner)
        {
            ruafManager = botOwner.GetOrAddRuafManager();
        }

        public override void Start()
        {
            ruafManager.UpdateGuardPoint();
            BotOwner.Memory.SetCoverPoints(ruafManager.guardPoint, "");

            base.Start();
        }

        public override void Update(CustomLayer.ActionData data)
        {
            ruafManager.UpdateGuardPoint();
            BotOwner.Memory.SetCoverPoints(ruafManager.guardPoint, "");
            
            base.Update(data);

            if (BotOwner.Mover.IsComeTo(.7f, true, ruafManager.guardPoint))
            {
                ruafManager.AtCheckpoint = true;
            }
        }

        public override void BuildDebugText(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine($"-- GoToCheckpointAction -- {ruafManager.guardPoint.Position}");
            base.BuildDebugText(stringBuilder);
        }

        public override CustomNavigationPoint GetGoToPoint()
        {
            return ruafManager.guardPoint;
        }
    }
}
