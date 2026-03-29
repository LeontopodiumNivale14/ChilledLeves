using ChilledLeves.Enums;
using ECommons.ExcelServices;
using System.Collections.Generic;

namespace ChilledLeves.Resources
{
    public class GatheringRoute
    {
        public uint LeveId { get; set; }
        public uint TerritoryId { get; set; }
        public string ZoneName { get; set; }
        public ExpansionIds ExpansionId { get; set; }
        public Job GatheringJob { get; set; }
        public List<GatheringNode> NodeInfo { get; set; }
    }

    public class GatheringNode
    {
        public uint BaseId { get; set; }
        public Vector3 Position { get; set; }
        public FanInfo Gathering_FanInfo { get; set; } = new();
        public FanInfo Flight_FanInfo { get; set; } = new();
    }

    public class FanInfo
    {
        public float Fan_StartAngle { get; set; } = 0.0f;
        public float Fan_EndAngle { get; set; } = 0.0f;
        public float Fan_DistanceMin { get; set; } = 1.0f;
        public float Fan_DistanceMax { get; set; } = 3.0f;
        public float Fan_Height { get; set; } = 0.0f;
    }
}
