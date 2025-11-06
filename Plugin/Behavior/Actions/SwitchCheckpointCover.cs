using DrakiaXYZ.BigBrain.Brains;
using EFT;
using RUAFComeHome.Components;
using RUAFComeHome.Controllers;
using System.Text;

namespace RUAFComeHome.Behavior.Actions
{
    internal class SwitchCheckpointCover : GoToCustomAction
    {
        protected BotRuafManager ruafManager { get; private set; }

        public SwitchCheckpointCover(BotOwner botOwner) : base(botOwner)
        {
            ruafManager = botOwner.GetOrAddRuafManager();
        }

        public override void Start()
        {
            BotOwner.Memory.SetCoverPoints(ruafManager.guardPoint, "");

            base.Start();
        }

        public override void Update(CustomLayer.ActionData data)
        {
            ruafManager.UpdateGuardPoint();
            BotOwner.Memory.SetCoverPoints(ruafManager.guardPoint, "");

            base.Update(data);

            if (BotOwner.Mover.IsComeTo(.5f, true, ruafManager.guardPoint))
            {
                ruafManager.ShouldSwitchCover = false;
            }
        }

        public override void BuildDebugText(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine($"-- SwitchCheckpointCover -- {ruafManager.guardPoint.Position}");
            base.BuildDebugText(stringBuilder);
        }

        public override CustomNavigationPoint GetGoToPoint()
        {
            return ruafManager.guardPoint;
        }
    }
}
