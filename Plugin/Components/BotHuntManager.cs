using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RUAFComeHome.Components
{
    public class BotHuntManager : MonoBehaviour
    {
        public BotOwner botOwner;
        public IPlayer huntTarget;
        public Vector3 knownLocation;
        public bool shouldSearch = false;

        private float updateTime;

        public void Update()
        {
            if (!HasHuntTarget() || Time.time < updateTime) return;

            knownLocation = huntTarget.Position;

            updateTime = Time.time + 30f;
        }

        public void Init(BotOwner bot)
        {
            botOwner = bot;
        }

        public bool HasHuntTarget()
        {
            return huntTarget != null && huntTarget.HealthController.IsAlive;
        }
    }
}
