using System.Collections.Generic;

namespace ChilledLeves.Utilities;

public static partial class Utils
{
    public class AethershardInfo
    {
        public uint ShardId { get; set; } = 0;
        public uint TerritoryId { get; set; } = 0;
        public List<uint> ValidTerritories { get; set; } = new();
        public Vector3 Position { get; set; } = Vector3.Zero;
        public Vector3 MoveTo { get; set; } = Vector3.Zero;
        public float InteractDistance { get; set; } = 5f;

        public float DistanceTo { get; set; } = 0f;
    }

    public static Dictionary<uint, AethershardInfo> Aethernet = new()
    {

        #region Limsa Lower

        [8] = new()
        {
            ShardId = 8,
            TerritoryId = 129,
            ValidTerritories = new() { 128, 129 },
            Position = new(-84.03f, 20.77f, 0.02f),
            MoveTo = new(-78.77f, 18.80f, 2.43f),
            InteractDistance = 10f
        },
        [43] = new()
        {
            ShardId = 43,
            TerritoryId = 129,
            ValidTerritories = new() { 128, 129 },
            Position = new(-333.29f, 12.00f, 54.81f),
            MoveTo = new(-335.16f, 12.62f, 56.38f),
        },
        [44] = new()
        {
            ShardId = 44,
            TerritoryId = 129,
            ValidTerritories = new() { 128, 129 },
            Position = new(-179.40f, 4.81f, 182.97f),
            MoveTo = new(-182.06f, 4.00f, 182.37f),
        },
        [49] = new()
        {
            ShardId = 49,
            TerritoryId = 129,
            ValidTerritories = new() { 128, 129 },
            Position = new(-213.70f, 16.00f, 49.78f),
            MoveTo = new(-213.61f, 16.74f, 51.80f),
        },

        #endregion

        #region Limsa Upper

        [41] = new()
        {
            ShardId = 41,
            TerritoryId = 128,
            ValidTerritories = new() { 128, 129 },
            Position = new(16.07f, 40.79f, 68.80f),
            MoveTo = new(14.92f, 40.00f, 70.86f),
        },
        [42] = new()
        {
            ShardId = 42,
            TerritoryId = 128,
            ValidTerritories = new() { 128, 129 },
            Position = new(-56.50f, 44.48f, -131.46f),
            MoveTo = new(-56.42f, 42.00f, -129.53f),
        },
        [48] = new()
        {
            ShardId = 48,
            TerritoryId = 128,
            ValidTerritories = new() { 128, 129 },
            Position = new(-5.17f, 44.63f, -218.07f),
            MoveTo = new(-3.49f, 44.00f, -218.09f),
        },

        #endregion

        #region New Gridania

        [2] = new()
        {
            ShardId = 2,
            TerritoryId = 132,
            ValidTerritories = new() { 132, 133 },
            Position = new(32.91f, 2.67f, 30.01f),
            MoveTo = new(34.87f, 2.20f, 33.14f),
            InteractDistance = 10f
        },
        [25] = new()
        {
            ShardId = 25,
            TerritoryId = 132,
            ValidTerritories = new() { 132, 133 },
            Position = new(166.58f, -1.72f, 86.14f),
            MoveTo = new(165.94f, -2.50f, 83.66f),
        },

        #endregion

        #region Old Gridania

        [26] = new()
        {
            ShardId = 26,
            TerritoryId = 133,
            ValidTerritories = new() { 132, 133 },
            Position = new(101.27f, 9.02f, -111.31f),
            MoveTo = new(102.15f, 8.52f, -108.71f),
        },
        [27] = new()
        {
            ShardId = 27,
            TerritoryId = 133,
            ValidTerritories = new() { 132, 133 },
            Position = new(121.23f, 12.65f, -229.63f),
            MoveTo = new(116.55f, 11.56f, -231.89f),
        },
        [28] = new()
        {
            ShardId = 28,
            TerritoryId = 133,
            ValidTerritories = new() { 132, 133 },
            Position = new(-145.16f, 4.96f, -11.76f),
            MoveTo = new(-147.42f, 4.00f, -13.33f),
        },
        [29] = new()
        {
            ShardId = 29,
            TerritoryId = 133,
            ValidTerritories = new() { 132, 133 },
            Position = new(-311.09f, 7.95f, -177.05f),
            MoveTo = new(-308.36f, 7.06f, -176.81f),
        },
        [30] = new()
        {
            ShardId = 30,
            TerritoryId = 133,
            ValidTerritories = new() { 132, 133 },
            Position = new(-73.93f, 7.98f, -140.15f),
            MoveTo = new(-73.83f, 7.12f, -137.96f),
        },

        #endregion

        #region Ul'Dah - Main

        [9] = new()
        {
            ShardId = 9,
            TerritoryId = 130,
            ValidTerritories = new() { 130, 131 },
            Position = new(-144.52f, -1.36f, -169.67f),
            MoveTo = new(-140.06f, -3.15f, -165.86f),
            InteractDistance = 10f
        },
        [33] = new()
        {
            ShardId = 33,
            TerritoryId = 130,
            ValidTerritories = new() { 130, 131 },
            Position = new(64.23f, 4.53f, -115.31f),
            MoveTo = new(63.54f, 4.10f, -117.56f),
        },
        [34] = new()
        {
            ShardId = 34,
            TerritoryId = 130,
            ValidTerritories = new() { 130, 131 },
            Position = new(-154.83f, 14.63f, 73.08f),
            MoveTo = new(-155.43f, 14.01f, 70.93f),
        },

        #endregion

        #region Ul'Dah - Alt

        [35] = new()
        {
            ShardId = 35,
            TerritoryId = 131,
            ValidTerritories = new() { 130, 131 },
            Position = new(-53.85f, 10.70f, 12.22f),
            MoveTo = new(-52.61f, 10.00f, 10.75f),
        },
        [36] = new()
        {
            ShardId = 36,
            TerritoryId = 131,
            ValidTerritories = new() { 130, 131 },
            Position = new(33.49f, 13.23f, 113.21f),
            MoveTo = new(31.30f, 12.06f, 111.96f),
        },
        [47] = new()
        {
            ShardId = 47,
            TerritoryId = 131,
            ValidTerritories = new() { 130, 131 },
            Position = new(89.65f, 12.92f, 58.27f),
            MoveTo = new(90.97f, 12.00f, 59.50f),
        },
        [50] = new()
        {
            ShardId = 50,
            TerritoryId = 131,
            ValidTerritories = new() { 130, 131 },
            Position = new(89.65f, 12.92f, 58.27f),
            MoveTo = new(90.97f, 12.00f, 59.50f),
        },
        [125] = new()
        {
            ShardId = 125,
            TerritoryId = 131,
            ValidTerritories = new() { 130, 131 },
            Position = new(131.94f, 4.71f, -29.80f),
            MoveTo = new(131.10f, 4.00f, -31.64f),
        },

        #endregion
    };

    public static Dictionary<uint, AethershardInfo> TerritoryAethernet(uint territoryId) => Aethernet.All(x => x.Value.ValidTerritories.Contains(territoryId));
}