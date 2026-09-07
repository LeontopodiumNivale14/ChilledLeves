using ChilledLeves.Scheduler.Tasks;
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
        public float DistanceTo => Task_Navmesh.PathDistance(PathList);
        public List<Vector3> PathList { get; set; } = new();
    }

    private static float Interact_CityAethernet = 9f;

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
            InteractDistance = Interact_CityAethernet,
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
            InteractDistance = Interact_CityAethernet,
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
            InteractDistance = Interact_CityAethernet,
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

        #region Foundation

        [70] = new()
        {
            ShardId = 70,
            TerritoryId = 418,
            ValidTerritories = new() { 418, 419 },
            Position = new(-63.98f, 11.15f, 43.99f),
            MoveTo = new(-63.59f, 8.11f, 37.46f),
            InteractDistance = Interact_CityAethernet,
        },
        [80] = new()
        {
            ShardId = 80,
            TerritoryId = 418,
            ValidTerritories = new() { 418, 419 },
            Position = new(45.79f, 24.55f, 0.99f),
            MoveTo = new(48.82f, 23.98f, -0.03f),
        },
        [81] = new()
        {
            ShardId = 81,
            TerritoryId = 418,
            ValidTerritories = new() { 418, 419 },
            Position = new(-111.44f, 16.13f, -27.05f),
            MoveTo = new(-110.45f, 15.14f, -29.49f),
        },
        [82] = new()
        {
            ShardId = 82,
            TerritoryId = 418,
            ValidTerritories = new() { 418, 419 },
            Position = new(49.42f, -11.15f, 66.70f),
            MoveTo = new(50.37f, -12.02f, 68.09f),
        },



        #endregion

        #region The Pillars

        [83] = new()
        {
            ShardId = 83,
            TerritoryId = 419,
            ValidTerritories = new() { 419, 418 },
            Position = new(133.38f, -8.87f, -64.77f),
            MoveTo = new(135.43f, -9.23f, -63.87f),
            InteractDistance = 10,
        },
        [84] = new()
        {
            ShardId = 84,
            TerritoryId = 419,
            ValidTerritories = new() { 419, 418 },
            Position = new(-134.69f, -11.80f, -15.40f),
            MoveTo = new(-136.91f, -12.63f, -17.26f),
        },
        [85] = new()
        {
            ShardId = 85,
            TerritoryId = 419,
            ValidTerritories = new() { 419, 418 },
            Position = new(-77.96f, 10.60f, -126.54f),
            MoveTo = new(-79.47f, 10.05f, -124.64f),
        },
        [86] = new()
        {
            ShardId = 86,
            TerritoryId = 419,
            ValidTerritories = new() { 419, 418 },
            Position = new(78.02f, 11.00f, -126.51f),
            MoveTo = new(79.10f, 10.05f, -124.60f),
        },
        [87] = new()
        {
            ShardId = 87,
            TerritoryId = 419,
            ValidTerritories = new() { 419, 418 },
            Position = new(0.02f, 16.53f, -32.52f),
            MoveTo = new(0.04f, 16.02f, -34.76f),
        },

        #endregion

        #region Kugane

        [111] = new()
        {
            ShardId = 111,
            TerritoryId = 628,
            ValidTerritories = new() { 628 },
            Position = new(47.50f, 8.44f, -37.31f),
            MoveTo = new(43.26f, 4.55f, -41.61f),
            InteractDistance = Interact_CityAethernet,
        },
        [112] = new()
        {
            ShardId = 112,
            TerritoryId = 628,
            ValidTerritories = new() { 628 },
            Position = new(-73.17f, -6.09f, -77.78f),
            MoveTo = new(-75.06f, -7.00f, -77.83f),
        },
        [113] = new()
        {
            ShardId = 113,
            TerritoryId = 628,
            ValidTerritories = new() { 628 },
            Position = new(-113.57f, -3.89f, 155.41f),
            MoveTo = new(-114.11f, -5.01f, 153.45f),
        },
        [114] = new()
        {
            ShardId = 114,
            TerritoryId = 628,
            ValidTerritories = new() { 628 },
            Position = new(27.18f, 9.05f, 141.59f),
            MoveTo = new(28.79f, 8.02f, 143.81f),
        },
        [115] = new()
        {
            ShardId = 115,
            TerritoryId = 628,
            ValidTerritories = new() { 628 },
            Position = new(26.69f, 4.93f, 73.35f),
            MoveTo = new(27.01f, 4.00f, 71.82f),
        },
        [116] = new()
        {
            ShardId = 116,
            TerritoryId = 628,
            ValidTerritories = new() { 628 },
            Position = new(-76.01f, 19.06f, -161.18f),
            MoveTo = new(-76.38f, 18.00f, -162.98f),
        },
        [117] = new()
        {
            ShardId = 117,
            TerritoryId = 628,
            ValidTerritories = new() { 628 },
            Position = new(132.40f, 12.95f, 83.02f),
            MoveTo = new(130.92f, 12.00f, 83.02f),
        },
        [118] = new()
        {
            ShardId = 118,
            TerritoryId = 628,
            ValidTerritories = new() { 628 },
            Position = new(119.10f, 13.02f, -92.88f),
            MoveTo = new(118.96f, 12.00f, -89.97f),
        },
        [119] = new()
        {
            ShardId = 119,
            TerritoryId = 628,
            ValidTerritories = new() { 628 },
            Position = new(24.64f, 7.00f, -152.97f),
            MoveTo = new(25.43f, 6.00f, -151.15f),
        },


        #endregion

        #region Crystarium

        [133] = new()
        {
            ShardId = 133,
            TerritoryId = 819,
            ValidTerritories = new() { 819 },
            Position = new(-65.02f, 4.53f, 0.02f),
            MoveTo = new(-67.90f, 3.94f, -0.08f),
            InteractDistance = Interact_CityAethernet,
        },
        [149] = new()
        {
            ShardId = 149,
            TerritoryId = 819,
            ValidTerritories = new() { 819 },
            Position = new(-6.15f, -7.74f, 148.73f),
            MoveTo = new(-6.06f, -7.70f, 147.10f),
        },
        [150] = new()
        {
            ShardId = 150,
            TerritoryId = 819,
            ValidTerritories = new() { 819 },
            Position = new(-107.38f, -0.02f, -58.76f),
            MoveTo = new(-108.51f, 0.00f, -59.36f),
        },
        [151] = new()
        {
            ShardId = 151,
            TerritoryId = 819,
            ValidTerritories = new() { 819 },
            Position = new(64.87f, -0.02f, -18.17f),
            MoveTo = new(63.01f, -0.00f, -18.03f),
        },
        [152] = new()
        {
            ShardId = 152,
            TerritoryId = 819,
            ValidTerritories = new() { 819 },
            Position = new(35.48f, -0.02f, 222.58f),
            MoveTo = new(35.50f, 0.00f, 220.92f),
        },
        [153] = new()
        {
            ShardId = 153,
            TerritoryId = 819,
            ValidTerritories = new() { 819 },
            Position = new(66.61f, 36.00f, -131.09f),
            MoveTo = new(66.74f, 36.00f, -132.20f),
        },
        [154] = new()
        {
            ShardId = 154,
            TerritoryId = 819,
            ValidTerritories = new() { 819 },
            Position = new(-52.51f, 19.97f, -173.36f),
            MoveTo = new(-52.40f, 20.00f, -171.59f),
        },
        [155] = new()
        {
            ShardId = 155,
            TerritoryId = 819,
            ValidTerritories = new() { 819 },
            Position = new(-54.40f, -37.71f, -241.08f),
            MoveTo = new(-55.71f, -37.70f, -239.90f),
        },


        #endregion

        #region Old Shar

        [182] = new()
        {
            ShardId = 182,
            TerritoryId = 962,
            ValidTerritories = new() { 962 },
            Position = new(0.08f, 4.81f, -0.11f),
            MoveTo = new(2.48f, 3.27f, -2.37f),
            InteractDistance = Interact_CityAethernet,
        },
        [185] = new()
        {
            ShardId = 185,
            TerritoryId = 962,
            ValidTerritories = new() { 962 },
            Position = new(-92.21f, 2.30f, 29.71f),
            MoveTo = new(-89.97f, 1.69f, 28.91f),
        },
        [189] = new()
        {
            ShardId = 189,
            TerritoryId = 962,
            ValidTerritories = new() { 962 },
            Position = new(16.49f, -16.25f, 127.73f),
            MoveTo = new(18.37f, -16.25f, 126.67f),
        },
        [184] = new()
        {
            ShardId = 184,
            TerritoryId = 962,
            ValidTerritories = new() { 962 },
            Position = new(-291.16f, 20.00f, -74.14f),
            MoveTo = new(-289.57f, 20.01f, -75.43f),
        },
        [186] = new()
        {
            ShardId = 186,
            TerritoryId = 962,
            ValidTerritories = new() { 962 },
            Position = new(-36.94f, 41.37f, -156.60f),
            MoveTo = new(-36.64f, 41.38f, -158.16f),
        },
        [187] = new()
        {
            ShardId = 187,
            TerritoryId = 962,
            ValidTerritories = new() { 962 },
            Position = new(204.79f, 21.77f, -118.73f),
            MoveTo = new(205.95f, 21.82f, -120.42f),
        },
        [188] = new()
        {
            ShardId = 188,
            TerritoryId = 962,
            ValidTerritories = new() { 962 },
            Position = new(206.23f, 1.85f, 13.78f),
            MoveTo = new(207.03f, 1.86f, 15.46f),
        },


        #endregion

        #region Tuliyolli

        [216] = new()
        {
            ShardId = 216,
            TerritoryId = 1185,
            ValidTerritories = new() { 1185 },
            Position = new(-24.09f, 0.78f, 7.58f),
            MoveTo = new(-20.26f, 0.50f, 11.36f),
            InteractDistance = Interact_CityAethernet,
        },
        [220] = new()
        {
            ShardId = 220,
            TerritoryId = 1185,
            ValidTerritories = new() { 1185 },
            Position = new(-149.74f, -15.03f, 198.90f),
            MoveTo = new(-150.69f, -15.00f, 197.58f),
        },
        [218] = new()
        {
            ShardId = 218,
            TerritoryId = 1185,
            ValidTerritories = new() { 1185 },
            Position = new(-413.69f, 2.98f, -45.98f),
            MoveTo = new(-415.07f, 3.00f, -47.19f),
        },
        [219] = new()
        {
            ShardId = 219,
            TerritoryId = 1185,
            ValidTerritories = new() { 1185 },
            Position = new(-187.12f, 39.93f, 6.09f),
            MoveTo = new(-187.14f, 39.95f, 8.28f),
        },
        [221] = new()
        {
            ShardId = 221,
            TerritoryId = 1185,
            ValidTerritories = new() { 1185 },
            Position = new(-15.00f, -10.03f, 135.58f),
            MoveTo = new(-16.94f, -10.00f, 136.33f),
        },
        [222] = new()
        {
            ShardId = 222,
            TerritoryId = 1185,
            ValidTerritories = new() { 1185 },
            Position = new(-99.14f, 100.72f, -222.03f),
            MoveTo = new(-97.35f, 100.75f, -221.50f),
        },
        [223] = new()
        {
            ShardId = 223,
            TerritoryId = 1185,
            ValidTerritories = new() { 1185 },
            Position = new(166.28f, -17.99f, 38.74f),
            MoveTo = new(167.96f, -17.96f, 38.19f),
        },
        [224] = new()
        {
            ShardId = 224,
            TerritoryId = 1185,
            ValidTerritories = new() { 1185 },
            Position = new(71.79f, 47.07f, -333.21f),
            MoveTo = new(69.44f, 47.00f, -331.64f),
        },

        #endregion

        #region RazDaHan [Rat Town]

        [183] = new()
        {
            ShardId = 183,
            TerritoryId = 963,
            ValidTerritories = new() { 963 },
            Position = new(25.99f, 3.25f, -27.02f),
            MoveTo = new(29.83f, 0.90f, -24.24f),
            InteractDistance = Interact_CityAethernet,
        },
        [191] = new()
        {
            ShardId = 191,
            TerritoryId = 963,
            ValidTerritories = new() { 963 },
            Position = new(-365.96f, 45.00f, -31.82f),
            MoveTo = new(-366.21f, 45.00f, -28.36f),
        },
        [192] = new()
        {
            ShardId = 192,
            TerritoryId = 963,
            ValidTerritories = new() { 963 },
            Position = new(-156.15f, 36.00f, 27.73f),
            MoveTo = new(-158.54f, 36.00f, 28.14f),
        },
        [193] = new()
        {
            ShardId = 193,
            TerritoryId = 963,
            ValidTerritories = new() { 963 },
            Position = new(-144.34f, 27.97f, 202.26f),
            MoveTo = new(-144.51f, 28.00f, 198.83f),
        },
        [194] = new()
        {
            ShardId = 194,
            TerritoryId = 963,
            ValidTerritories = new() { 963 },
            Position = new(6.61f, -2.03f, 110.55f),
            MoveTo = new(7.21f, -2.00f, 108.61f),
        },
        [195] = new()
        {
            ShardId = 195,
            TerritoryId = 963,
            ValidTerritories = new() { 963 },
            Position = new(-141.37f, 3.98f, -98.44f),
            MoveTo = new(-140.19f, 4.00f, -96.49f),
        },
        [196] = new()
        {
            ShardId = 196,
            TerritoryId = 963,
            ValidTerritories = new() { 963 },
            Position = new(-42.62f, -0.02f, -197.62f),
            MoveTo = new(-44.40f, 0.00f, -198.39f),
        },
        [198] = new()
        {
            ShardId = 198,
            TerritoryId = 963,
            ValidTerritories = new() { 963 },
            Position = new(129.59f, 26.99f, 13.47f),
            MoveTo = new(129.85f, 27.00f, 15.01f),
        },

        #endregion

        #region Solution Nine

        [217] = new()
        {
            ShardId = 217,
            TerritoryId = 1186,
            ValidTerritories = new() { 1186 },
            Position = new(-0.02f, 8.99f, -0.02f),
            MoveTo = new(-0.03f, 8.64f, 10.53f),
            InteractDistance = 10.5f,
        },
        [235] = new()
        {
            ShardId = 235,
            TerritoryId = 1186,
            ValidTerritories = new() { 1186 },
            Position = new(-160.05f, -0.02f, 21.59f),
            MoveTo = new(-157.68f, 0.00f, 20.44f),
        },
        [230] = new()
        {
            ShardId = 230,
            TerritoryId = 1186,
            ValidTerritories = new() { 1186 },
            Position = new(-30.44f, -6.06f, 209.34f),
            MoveTo = new(-29.77f, -6.05f, 211.34f),
        },
        [231] = new()
        {
            ShardId = 231,
            TerritoryId = 1186,
            ValidTerritories = new() { 1186 },
            Position = new(382.68f, 59.98f, 76.68f),
            MoveTo = new(382.73f, 60.00f, 74.21f),
        },
        [232] = new()
        {
            ShardId = 232,
            TerritoryId = 1186,
            ValidTerritories = new() { 1186 },
            Position = new(258.29f, 50.74f, 148.73f),
            MoveTo = new(260.17f, 50.75f, 146.73f),
        },
        [233] = new()
        {
            ShardId = 233,
            TerritoryId = 1186,
            ValidTerritories = new() { 1186 },
            Position = new(374.78f, 60.01f, 325.67f),
            MoveTo = new(372.62f, 60.12f, 325.10f),
        },
        [234] = new()
        {
            ShardId = 234,
            TerritoryId = 1186,
            ValidTerritories = new() { 1186 },
            Position = new(-32.06f, 38.04f, -345.24f),
            MoveTo = new(-30.20f, 38.06f, -344.66f),
        },
        [236] = new()
        {
            ShardId = 236,
            TerritoryId = 1186,
            ValidTerritories = new() { 1186 },
            Position = new(-378.13f, 13.99f, 136.49f),
            MoveTo = new(-376.81f, 14.00f, 138.55f),
        },


        #endregion

        #region Outside Shards

        [10] = new()
        {
            ShardId = 10,
            TerritoryId = 135,
            ValidTerritories = new() { 135 },
            Position = new(156.11f, 15.52f, 673.21f),
            MoveTo = new(151.46f, 14.11f, 674.10f),
        },
        [11] = new()
        {
            ShardId = 11,
            TerritoryId = 137,
            ValidTerritories = new() { 137 },
            Position = new(489.16f, 20.83f, 468.80f),
            MoveTo = new(486.69f, 17.44f, 464.00f),
        },
        [12] = new()
        {
            ShardId = 12,
            TerritoryId = 137,
            ValidTerritories = new() { 137 },
            Position = new(-18.39f, 72.68f, 3.83f),
            MoveTo = new(-15.89f, 70.60f, 5.82f),
        },
        [13] = new()
        {
            ShardId = 13,
            TerritoryId = 138,
            ValidTerritories = new() { 138 },
            Position = new(651.54f, 11.73f, 513.36f),
            MoveTo = new(653.11f, 9.17f, 510.45f),
        },
        [14] = new()
        {
            ShardId = 14,
            TerritoryId = 138,
            ValidTerritories = new() { 138 },
            Position = new(260.94f, -19.61f, 218.52f),
            MoveTo = new(260.95f, -22.75f, 223.07f),
        },
        [17] = new()
        {
            ShardId = 17,
            TerritoryId = 140,
            ValidTerritories = new() { 140 },
            Position = new(68.01f, 48.20f, -227.04f),
            MoveTo = new(69.45f, 45.27f, -221.76f),
        },
        [18] = new()
        {
            ShardId = 18,
            TerritoryId = 145,
            ValidTerritories = new() { 145 },
            Position = new(-386.34f, -57.18f, 142.60f),
            MoveTo = new(-385.60f, -59.00f, 136.43f),
        },
        [19] = new()
        {
            ShardId = 19,
            TerritoryId = 146,
            ValidTerritories = new() { 146 },
            Position = new(-159.38f, 30.11f, -415.46f),
            MoveTo = new(-153.89f, 26.14f, -418.09f),
        },
        [20] = new()
        {
            ShardId = 20,
            TerritoryId = 146,
            ValidTerritories = new() { 146 },
            Position = new(-326.62f, 10.70f, 406.64f),
            MoveTo = new(-322.17f, 8.26f, 405.97f),
        },
        [21] = new()
        {
            ShardId = 21,
            TerritoryId = 147,
            ValidTerritories = new() { 147 },
            Position = new(20.98f, 8.83f, 454.03f),
            MoveTo = new(23.57f, 6.96f, 454.60f),
        },
        [22] = new()
        {
            ShardId = 22,
            TerritoryId = 147,
            ValidTerritories = new() { 147 },
            Position = new(-26.60f, 49.88f, -30.84f),
            MoveTo = new(-24.68f, 48.31f, -28.54f),
        },
        [3] = new()
        {
            ShardId = 3,
            TerritoryId = 148,
            ValidTerritories = new() { 148 },
            Position = new(13.08f, 0.56f, 35.90f),
            MoveTo = new(12.40f, -1.16f, 31.52f),
        },
        [4] = new()
        {
            ShardId = 4,
            TerritoryId = 152,
            ValidTerritories = new() { 152 },
            Position = new(-186.54f, 3.80f, 297.57f),
            MoveTo = new(-190.16f, 4.44f, 294.80f),
        },
        [5] = new()
        {
            ShardId = 5,
            TerritoryId = 153,
            ValidTerritories = new() { 153 },
            Position = new(178.61f, 10.54f, -68.19f),
            MoveTo = new(184.12f, 8.61f, -67.14f),
        },
        [6] = new()
        {
            ShardId = 6,
            TerritoryId = 153,
            ValidTerritories = new() { 153 },
            Position = new(-230.06f, 22.63f, 355.46f),
            MoveTo = new(-227.16f, 21.13f, 353.69f),
        },
        [23] = new()
        {
            ShardId = 23,
            TerritoryId = 155,
            ValidTerritories = new() { 155 },
            Position = new(223.99f, 315.79f, -234.85f),
            MoveTo = new(228.44f, 312.00f, -238.23f),
        },
        [24] = new()
        {
            ShardId = 24,
            TerritoryId = 156,
            ValidTerritories = new() { 156 },
            Position = new(40.02f, 24.00f, -668.02f),
            MoveTo = new(38.60f, 20.30f, -675.33f),
        },

        #region Lakeland

        [132] = new()
        {
            ShardId = 132,
            TerritoryId = 813,
            ValidTerritories = new() { 813 },
            Position = new(753.78f, 24.34f, -28.82f),
            MoveTo = new(755.23f, 22.60f, -18.45f),
        },
        [136] = new()
        {
            ShardId = 136,
            TerritoryId = 813,
            ValidTerritories = new() { 813 },
            Position = new(-735.01f, 53.39f, -230.03f),
            MoveTo = new(-734.82f, 52.47f, -223.59f),
        },


        #endregion

        #region Kholusia

        [137] = new()
        {
            ShardId = 137,
            TerritoryId = 814,
            ValidTerritories = new() { 814 },
            Position = new(668.33f, 29.47f, 289.17f),
            MoveTo = new(671.99f, 28.66f, 289.84f),
        },
        [139] = new()
        {
            ShardId = 139,
            TerritoryId = 814,
            ValidTerritories = new() { 814 },
            Position = new(-426.38f, 419.27f, -623.53f),
            MoveTo = new(-422.58f, 417.26f, -618.05f),
        },
        [138] = new()
        {
            ShardId = 138,
            TerritoryId = 814,
            ValidTerritories = new() { 814 },
            Position = new(-244.01f, 20.74f, 385.46f),
            MoveTo = new(-242.53f, 18.60f, 391.13f),
        },


        #endregion

        #region Ahm Arang

        [161] = new()
        {
            ShardId = 161,
            TerritoryId = 815,
            ValidTerritories = new() { 815 },
            Position = new(399.10f, -24.52f, 307.97f),
            MoveTo = new(398.88f, -26.78f, 303.43f),
        },
        [140] = new()
        {
            ShardId = 140,
            TerritoryId = 815,
            ValidTerritories = new() { 815 },
            Position = new(246.39f, 12.99f, -220.29f),
            MoveTo = new(247.29f, 11.66f, -225.16f),
        },
        [141] = new()
        {
            ShardId = 141,
            TerritoryId = 815,
            ValidTerritories = new() { 815 },
            Position = new(-511.35f, 47.99f, -212.60f),
            MoveTo = new(-517.24f, 45.78f, -212.46f),
        },

        #endregion

        #region Il'Mheg

        [144] = new()
        {
            ShardId = 144,
            TerritoryId = 816,
            ValidTerritories = new() { 816 },
            Position = new(-344.72f, 48.72f, 512.26f),
            MoveTo = new(-347.14f, 48.44f, 509.37f),
        },
        [145] = new()
        {
            ShardId = 145,
            TerritoryId = 816,
            ValidTerritories = new() { 816 },
            Position = new(-72.56f, 103.96f, -857.36f),
            MoveTo = new(-69.80f, 103.28f, -857.13f),
        },
        [146] = new()
        {
            ShardId = 146,
            TerritoryId = 816,
            ValidTerritories = new() { 816 },
            Position = new(380.51f, 87.21f, -687.25f),
            MoveTo = new(384.52f, 86.80f, -686.30f),
        },


        #endregion

        #region Ra'tika

        [142] = new()
        {
            ShardId = 142,
            TerritoryId = 817,
            ValidTerritories = new() { 817 },
            Position = new(-103.41f, -19.33f, 297.23f),
            MoveTo = new(-107.48f, -19.92f, 295.60f),
        },
        [143] = new()
        {
            ShardId = 143,
            TerritoryId = 817,
            ValidTerritories = new() { 817 },
            Position = new(382.77f, 21.04f, -194.11f),
            MoveTo = new(383.11f, 20.71f, -198.19f),
        },


        #endregion

        #region Labyrinthos

        [166] = new()
        {
            ShardId = 166,
            TerritoryId = 956,
            ValidTerritories = new() { 956 },
            Position = new(443.53f, 170.64f, -476.19f),
            MoveTo = new(440.86f, 169.24f, -477.99f),
        },

        [167] = new()
        {
            ShardId = 167,
            TerritoryId = 956,
            ValidTerritories = new() { 956 },
            Position = new(8.38f, -27.54f, -46.68f),
            MoveTo = new(4.19f, -28.72f, -50.08f),
        },

        [168] = new()
        {
            ShardId = 168,
            TerritoryId = 956,
            ValidTerritories = new() { 956 },
            Position = new(-729.18f, -27.63f, 302.14f),
            MoveTo = new(-726.79f, -28.72f, 307.10f),
        },


        #endregion

        #region Thavnair

        [169] = new()
        {
            ShardId = 169,
            TerritoryId = 957,
            ValidTerritories = new() { 957 },
            Position = new(193.50f, 6.97f, 629.24f),
            MoveTo = new(187.70f, 5.80f, 626.51f),
        },

        [170] = new()
        {
            ShardId = 170,
            TerritoryId = 957,
            ValidTerritories = new() { 957 },
            Position = new(-527.49f, 4.78f, 36.76f),
            MoveTo = new(-527.62f, 2.59f, 42.84f),
        },

        [171] = new()
        {
            ShardId = 171,
            TerritoryId = 957,
            ValidTerritories = new() { 957 },
            Position = new(405.14f, 5.26f, -244.50f),
            MoveTo = new(401.47f, 3.81f, -244.41f),
        },



        #endregion

        #region Garlemald

        [172] = new()
        {
            ShardId = 172,
            TerritoryId = 958,
            ValidTerritories = new() { 958 },
            Position = new(-408.10f, 24.16f, 479.97f),
            MoveTo = new(-401.34f, 23.06f, 481.27f),
        },

        [173] = new()
        {
            ShardId = 173,
            TerritoryId = 958,
            ValidTerritories = new() { 958 },
            Position = new(518.91f, -35.32f, -178.36f),
            MoveTo = new(516.92f, -36.21f, -183.22f),
        },

        #endregion

        #region Urqopacha

        [201] = new()
        {
            ShardId = 201,
            TerritoryId = 1187,
            ValidTerritories = new() { 1187 },
            Position = new(465.63f, 114.95f, 634.91f),
            MoveTo = new(474.05f, 115.68f, 632.58f),
        },

        [200] = new()
        {
            ShardId = 200,
            TerritoryId = 1187,
            ValidTerritories = new() { 1187 },
            Position = new(332.97f, -160.11f, -416.22f),
            MoveTo = new(339.46f, -159.47f, -419.02f),
        },


        #endregion

        #region Kozama'uka

        [202] = new()
        {
            ShardId = 202,
            TerritoryId = 1188,
            ValidTerritories = new() { 1188 },
            Position = new(-169.51f, 6.58f, -479.42f),
            MoveTo = new(-172.20f, 6.28f, -489.70f),
        },

        [204] = new()
        {
            ShardId = 204,
            TerritoryId = 1188,
            ValidTerritories = new() { 1188 },
            Position = new(-477.53f, 124.04f, 311.33f),
            MoveTo = new(-483.36f, 122.74f, 304.36f),
        },

        [203] = new()
        {
            ShardId = 203,
            TerritoryId = 1188,
            ValidTerritories = new() { 1188 },
            Position = new(541.16f, 117.42f, 203.60f),
            MoveTo = new(547.98f, 116.92f, 202.74f),
        },

        [238] = new() // Don't use this one, it's beast tribe locked
        {
            ShardId = 238,
            TerritoryId = 1188,
            ValidTerritories = new() { 1188 },
            Position = new(787.59f, 14.18f, -236.22f),
            MoveTo = new(779.71f, 13.61f, -236.00f),
        },

        #endregion

        #region Yak'Tel

        [205] = new()
        {
            ShardId = 205,
            TerritoryId = 1189,
            ValidTerritories = new() { 1189 },
            Position = new(-397.06f, 23.51f, -431.94f),
            MoveTo = new(-404.51f, 24.08f, -432.71f),
        },

        [206] = new()
        {
            ShardId = 206,
            TerritoryId = 1189,
            ValidTerritories = new() { 1189 },
            Position = new(721.40f, -132.31f, 526.18f),
            MoveTo = new(719.54f, -132.74f, 531.61f),
        },


        #endregion

        #region Shaaloani

        [207] = new()
        {
            ShardId = 207,
            TerritoryId = 1190,
            ValidTerritories = new() { 1190 },
            Position = new(386.40f, -0.20f, 467.61f),
            MoveTo = new(381.32f, -0.04f, 467.42f),
        },
        [208] = new()
        {
            ShardId = 208,
            TerritoryId = 1190,
            ValidTerritories = new() { 1190 },
            Position = new(-291.71f, 19.09f, -114.55f),
            MoveTo = new(-292.42f, 19.46f, -108.76f),
        },
        [209] = new()
        {
            ShardId = 209,
            TerritoryId = 1190,
            ValidTerritories = new() { 1190 },
            Position = new(311.36f, -14.18f, -567.74f),
            MoveTo = new(317.01f, -13.79f, -568.60f),
        },


        #endregion

        #region Heritage Found

        [210] = new()
        {
            ShardId = 210,
            TerritoryId = 1191,
            ValidTerritories = new() { 1191 },
            Position = new(514.61f, 145.86f, 207.57f),
            MoveTo = new(509.79f, 146.16f, 212.50f),
        },

        [211] = new()
        {
            ShardId = 211,
            TerritoryId = 1191,
            ValidTerritories = new() { 1191 },
            Position = new(-223.04f, 31.94f, -584.04f),
            MoveTo = new(-230.76f, 30.20f, -579.17f),
        },

        [212] = new()
        {
            ShardId = 212,
            TerritoryId = 1191,
            ValidTerritories = new() { 1191 },
            Position = new(-219.53f, 32.91f, 120.78f),
            MoveTo = new(-225.55f, 31.00f, 117.44f),
        },


        #endregion

        #region Living Memory

        [213] = new()
        {
            ShardId = 213,
            TerritoryId = 1192,
            ValidTerritories = new() { 1192 },
            Position = new(-0.23f, 57.18f, 796.96f),
            MoveTo = new(-7.86f, 53.84f, 805.54f),
        },

        [214] = new()
        {
            ShardId = 214,
            TerritoryId = 1192,
            ValidTerritories = new() { 1192 },
            Position = new(657.98f, 28.98f, -284.02f),
            MoveTo = new(648.48f, 25.00f, -291.01f),
        },

        [215] = new()
        {
            ShardId = 215,
            TerritoryId = 1192,
            ValidTerritories = new() { 1192 },
            Position = new(-255.27f, 59.43f, -397.67f),
            MoveTo = new(-247.02f, 54.81f, -391.46f),
        },


        #endregion

        #endregion
    };
}