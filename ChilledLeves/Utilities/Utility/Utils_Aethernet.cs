using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Utilities;

public static partial class Utils
{
    public class AethershardInfo
    {
        public uint ShardId { get; set; } = 0;
        public uint MenuId { get; set; } = 0;
        public Vector3 Position { get; set; } = Vector3.Zero;
        public Vector3 MoveTo { get; set; } = Vector3.Zero;
    }

    public static Dictionary<uint, List<AethershardInfo>> Aethernet = new()
    {
        [129] = new()
        {
            new()
            {
                ShardId = 8,
                MenuId = 0,
                Position = new(-84.03f, 20.77f, 0.02f),
                MoveTo = new(-78.77f, 18.80f, 2.43f)
            },
            new()
            {
                ShardId = 43,
                MenuId = 3,
                Position = new(-333.29f, 12.00f, 54.81f),
                MoveTo = new(-335.16f, 12.62f, 56.38f),
            },
            new()
            {
                ShardId = 44,
                MenuId = 4,
                Position = new(-179.40f, 4.81f, 182.97f),
                MoveTo = new(-182.06f, 4.00f, 182.37f),
            },
            new()
            {
                ShardId = 49,
                MenuId = 6,
                Position = new(-213.70f, 16.00f, 49.78f),
                MoveTo = new(-213.61f, 16.74f, 51.80f),
            }
        }

    };
}