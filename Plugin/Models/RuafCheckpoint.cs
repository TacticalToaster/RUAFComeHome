using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RUAFComeHome.Models
{
    public class RuafCheckpoint
    {
        public BotZone Zone { get; set; }
        public Vector3 Position { get; set; }
        public float Radius { get; set; } = 20f;
        public List<BotOwner> AssignedBots { get; set; } = new List<BotOwner>();

        public RuafCheckpoint(BotZone zone, Vector3 position, float radius = 20f)
        {
            Zone = zone;
            Position = position;
            Radius = radius;
        }
    }
}
