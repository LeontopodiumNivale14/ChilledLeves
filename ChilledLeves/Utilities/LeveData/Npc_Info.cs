using Lumina.Excel.Sheets;
using System.Collections.Generic;

namespace ChilledLeves.Utilities.LeveData;

public static partial class LeveInfo
{
    public class VendorInfo
    {
        public string Name { get; set; } = "";
        public uint TerritoryId { get; set; } = 0;
        public Vector3 Npc_InteractZone { get; set; } = Vector3.Zero;
        public Vector3 Npc_Location { get; set; } = Vector3.Zero;
        public Vector2 Npc_Flag { get; set; } = Vector2.Zero;
        public uint Aetheryte { get; set; } = 0;

        // Button Selections
        public int Button_BattleCraft { get; set; } = -1;
        public int Button_Gatherers { get; set; } = -1;
        public int Button_Crafters { get; set; } = -1;
        public int Button_Leave { get; set; } = -1;
        public int Button_LSGather { get; set; } = -1;
        public int Button_LSCrafter { get; set; } = -1;

        // Movement Options
        public bool Mount { get; set; } = false;
        public bool Fly { get; set; } = false;

        // For Grabbing Leves
        public List<uint> Leves { get; set; } = new();
    }

    public static Dictionary<uint, VendorInfo> LeveNpc_Info = new()
    {
        #region ARR Leve NPC's

        [1000970] = new()
        { // T'mokkri
            Name = NPCName(1000970),
            TerritoryId = 128, // Upper Limsa
            Aetheryte = 8,
            Npc_InteractZone = new Vector3(-10.42f, 40.02f, -10.17f),
            Npc_Location = new Vector3(-12.38f, 40f, -12.16f),
            Npc_Flag = new(-12.38f, -12.16f),
            Button_BattleCraft = 0,
            Button_Gatherers = 1,
            Button_Crafters = 2,
            Button_Leave = 4,
            Leves = new()
            {
                141, 142, 143, 147, 148, 149, 153, 154, 155, 159, 160, 161, 165, 166, 167,
                171, 172, 173, 177, 178, 179, 183, 184, 185, 189, 190, 191, 195, 196, 197,
                201, 202, 203, 207, 208, 209, 213, 214, 215, 219, 220, 221, 225, 226, 227,
                231, 232, 233, 237, 238, 239, 243, 244, 245, 249, 250, 251, 255, 256, 257,
                261, 262, 263, 267, 268, 269, 273, 274, 275, 279, 280, 281, 285, 286, 287,
                291, 292, 293, 297, 298, 299, 303, 304, 305, 309, 310, 311, 315, 316, 317,
                511, 626, 698, 699, 700, 701,
            }
        },
        [1000101] = new()
        { // Gontrant
            Name = NPCName(1000101),
            TerritoryId = 132, // Gridania
            Aetheryte = 2,
            Npc_InteractZone = new Vector3(27.67f, -8f, 108f),
            Npc_Location = new Vector3(25.04f, -8f, 108.08f),
            Npc_Flag = new(25.04f, 108.08f),
            Button_BattleCraft = 0,
            Button_Gatherers = 1,
            Button_Crafters = 2,
            Button_Leave = 4,
            Leves = new()
            {
                21, 22, 23, 27, 28, 29, 33, 34, 35, 39, 40, 41, 45, 46, 47, 51, 52, 53,
                57, 58, 59, 63, 64, 65, 69, 70, 71, 75, 76, 77, 81, 82, 83, 87, 88, 89,
                93, 94, 95, 99, 100, 101, 105, 106, 107, 111, 112, 113, 117, 118, 119,
                123, 124, 125, 129, 130, 131, 135, 136, 137, 674, 675, 676, 677, 678,
                679, 680, 681, 682, 683, 684, 685, 686, 687, 688, 689, 690, 691, 692,
                693, 694, 695, 696, 697,
            }
        },
        [1001794] = new()
        { // Gontrant (Ul'dah)
            Name = NPCName(1001794),
            TerritoryId = 130, // Ul'dah
            Aetheryte = 9,
            Npc_InteractZone = new Vector3(40.17f, 8.01f, -106.55f),
            Npc_Location = new Vector3(42.01f, 8.01f, -107.59f),
            Npc_Flag = new(42.01f, -107.59f),
            Button_BattleCraft = 0,
            Button_Gatherers = 1,
            Button_Crafters = 2,
            Button_Leave = 4,
            Leves = new()
            {
                321, 322, 323, 327, 328, 329, 333, 334, 335, 339, 340, 341, 345, 346, 347,
                351, 352, 353, 357, 358, 359, 363, 364, 365, 369, 370, 371, 375, 376, 377,
                381, 382, 383, 387, 388, 389, 393, 394, 395, 399, 400, 401, 405, 406, 407,
                411, 412, 413, 417, 418, 419, 423, 424, 425, 429, 430, 431, 435, 436, 437,
                441, 442, 443, 447, 448, 449, 453, 454, 455, 459, 460, 461, 465, 466, 467,
                471, 472, 473, 477, 478, 479, 483, 484, 485, 489, 490, 491, 495, 496, 497,
            }
        },
        [1004342] = new()
        { // Wyrkholsk
            Name = NPCName(1004342),
            TerritoryId = 135, // Lower La Noscea, Red Rooster Stead
            Aetheryte = 10,
            Npc_InteractZone = new Vector3(500.70f, 79.31f, -72.31f),
            Npc_Location = new Vector3(499.6f, 79.72f, -74.57f),
            Npc_Flag = new(499.60f, -74.57f),
            Button_BattleCraft = 0,
            Button_Gatherers = 1,
            Button_Crafters = 2,
            Button_Leave = 4,
            Mount = true,
            Leves = new()
            {
                144, 145, 146, 150, 151, 152, 204, 205, 206, 210, 211, 212, 264, 265, 266,
                270, 271, 272, 507, 509, 510, 511, 512, 526, 527, 528, 529, 530, 754, 755,
                756, 757, 758, 759, 760, 761,
            }
        },
        [1004735] = new()
        { // Eugene, GC leve person
            Name = NPCName(1004735),
            TerritoryId = 135, // Lower La Noscea
            Aetheryte = 10,
            Npc_InteractZone = new Vector3(117.30f, 22.88f, 674.81f),
            Npc_Location = new Vector3(114.49f, 22.88f, 674.95f),
            Npc_Flag = new(114.49f, 674.95f),
            Mount = true,
            Fly = true,
            Leves = new()
            {
                794, 796, 797, 795, 808, 806, 807, 809,
            }
        },
        [1004347] = new()
        { // Ourawann
            Name = NPCName(1004347),
            TerritoryId = 135, // Lower La Noscea
            Aetheryte = 10,
            Npc_InteractZone = new Vector3(121.90f, 23.00f, 581.59f),
            Npc_Location = new Vector3(122.33f, 23f, 578.27f),
            Npc_Flag = new(122.33f, 578.27f),
            Button_BattleCraft = 0,
            Button_Gatherers = 1,
            Button_Crafters = 2,
            Button_Leave = 4,
            Mount = true,
            Fly = true,
            Leves = new()
            {
                600, 598, 602, 601, 603, 599,
            }
        },
        [1000105] = new()
        { // Tierney
            Name = NPCName(1000105),
            TerritoryId = 148, // Central Shroud
            Aetheryte = 3,
            Npc_InteractZone = new Vector3(52.00f, -6.00f, 40.91f),
            Npc_Location = new Vector3(54.98f, -6f, 40.79f),
            Npc_Flag = new(54.98f, 40.79f),
            Button_BattleCraft = 0,
            Button_Gatherers = 1,
            Button_Crafters = 2,
            Button_Leave = 4,
            Mount = true,
            Fly = true,
            Leves = new()
            {
                36, 37, 38, 96, 97, 98, 537, 538, 539, 540, 541, 542, 543, 544, 545, 546,
                682, 683, 684, 685,
            }
        },
        [1001866] = new()
        { // Muriaule
            Name = NPCName(1001866),
            TerritoryId = 148, // Central Shroud
            Aetheryte = 3,
            Npc_InteractZone = new Vector3(120.64f, -6.99f, -94.16f),
            Npc_Location = new Vector3(120.13f, -6.78f, -97.18f),
            Npc_Flag = new(120.13f, -97.18f),
            Button_BattleCraft = 0,
            Button_Gatherers = 1,
            Button_Crafters = 2,
            Button_Leave = 4,
            Mount = true,
            Leves = new()
            {
                24, 25, 26, 30, 31, 32, 84, 85, 86, 90, 91, 92, 501, 502, 503, 504, 505,
                506, 519, 520, 521, 522, 523, 524, 674, 675, 676, 677, 678, 679, 680, 681,
            }
        },
        [1003888] = new()
        { // Graceful Song
            Name = NPCName(1003888),
            TerritoryId = 140, // Western Thanalan
            Aetheryte = 17,
            Npc_InteractZone = new Vector3(226.83f, 52.04f, 153.97f),
            Npc_Location = new Vector3(229.88f, 52.04f, 153.98f),
            Npc_Flag = new(229.88f, 153.98f),
            Button_BattleCraft = 0,
            Button_Gatherers = 1,
            Button_Crafters = 2,
            Button_Leave = 4,
            Mount = true,
            Leves = new()
            {
                324, 325, 326, 330, 331, 332, 384, 385, 386, 390, 391, 392, 444, 445, 446,
                450, 451, 452, 513, 515, 516, 517, 518, 532, 533, 534, 535, 536, 714, 715,
                716, 717, 718, 719, 720, 721,
            }
        },
        [1001796] = new()
        { // Totonowa
            Name = NPCName(1001796),
            TerritoryId = 140, // Western Thanalan
            Aetheryte = 17,
            Npc_InteractZone = new Vector3(83.07f, 46.00f, -243.82f),
            Npc_Location = new Vector3(85.16f, 46f, -245.84f),
            Npc_Flag = new(85.16f, -245.84f),
            Button_BattleCraft = 0,
            Button_Gatherers = 1,
            Button_Crafters = 2,
            Button_Leave = 4,
            Mount = true,
            Leves = new()
            {
                336, 337, 338, 396, 397, 398, 456, 457, 458, 557, 558, 559, 560, 561, 563,
                565, 566, 722, 723, 724, 725,
            }
        },
        [1001788] = new()
        { // Swygskyf
            Name = NPCName(1001788),
            TerritoryId = 138,
            Aetheryte = 13,
            Npc_InteractZone = new Vector3(666.72f, 9.18f, 513.39f),
            Npc_Location = new Vector3(669.18f, 9.2f, 512.99f),
            Npc_Flag = new(669.18f, 512.99f),
            Button_BattleCraft = 0,
            Button_Gatherers = 1,
            Button_Crafters = 2,
            Button_Leave = 4,
            Mount = true,
            Leves = new()
            {
                156, 157, 158, 216, 217, 218, 276, 277, 278, 547, 548, 549, 550, 551, 553,
                555, 556, 762, 763, 764, 765,
            }
        },
        [1001791] = new()
        { // Orwen, Locks leves behind quest
            Name = NPCName(1001791),
            TerritoryId = 138,
            Aetheryte = 14,
            Npc_InteractZone = new Vector3(309.51f, -31.90f, 283.59f),
            Npc_Location = new Vector3(312.58f, -31.9f, 283.96f),
            Npc_Flag = new(312.58f, 283.96f),
            Button_BattleCraft = 0,
            Button_Gatherers = 1,
            Button_Crafters = 2,
            Button_Leave = 4,
            Mount = true,
            Leves = new()
            {
                162, 163, 164, 222, 223, 224, 282, 283, 284, 574, 575, 576, 577, 578, 579,
                580, 766, 767, 768, 769,
            }
        },
        [1000821] = new()
        { // Qina Lyehga
            Name = NPCName(1000821),
            TerritoryId = 152,
            Aetheryte = 4,
            Npc_InteractZone = new Vector3(-211.93f, 1.09f, 287.70f),
            Npc_Location = new Vector3(-212.7f, 1.04f, 285.6f),
            Npc_Flag = new(-212.70f, 285.60f),
            Button_BattleCraft = 0,
            Button_Gatherers = 1,
            Button_Crafters = 2,
            Button_Leave = 4,
            Mount = true,
            Leves = new()
            {
                572, 569, 567, 570, 568, 571, 573, 44, 42, 43, 104, 102, 103, 689, 687,
                688, 686,
            }
        },
        [1004737] = new()
        { // Cedrepierre, GC Leve NPC
            Name = NPCName(1004737),
            TerritoryId = 152,
            Aetheryte = 4,
            Npc_InteractZone = new Vector3(-258.74f, 1.57f, 303.50f),
            Npc_Location = new Vector3(-257.68f, 1.63f, 305.81f),
            Npc_Flag = new(-257.68f, 305.81f),
            Mount = true,
            Leves = new()
            {
                798, 799, 800, 801, 810, 811, 812, 813,
            }
        },
        [1001799] = new()
        { // Poponagu
            Name = NPCName(1001799),
            TerritoryId = 145,
            Aetheryte = 18,
            Npc_InteractZone = new Vector3(-376.29f, -57.08f, 127.11f),
            Npc_Location = new Vector3(-378.19f, -57.33f, 129.26f),
            Npc_Flag = new(-378.19f, 129.26f),
            Button_BattleCraft = 0,
            Button_Gatherers = 1,
            Button_Crafters = 2,
            Button_Leave = 4,
            Mount = true,
            Leves = new()
            {
                583, 584, 586, 585, 581, 588, 587, 344, 343, 342, 463, 464, 462, 404, 402,
                403, 726, 727, 728, 729,
            }
        },
        [1004739] = new()
        { // Kikiri, GC Leve NPC
            Name = NPCName(1004739),
            TerritoryId = 145,
            Aetheryte = 18,
            Npc_InteractZone = new Vector3(-365.65f, -56.24f, 118.72f),
            Npc_Location = new Vector3(-364.13f, -56.13f, 120.68f),
            Npc_Flag = new(-364.13f, 120.68f),
            Mount = true,
            Leves = new()
            {
                802, 803, 804, 805, 814, 815, 816, 817,
            }
        },
        [1000823] = new()
        { // Nyell
            Name = NPCName(1000823),
            TerritoryId = 153,
            Aetheryte = 5,
            Npc_InteractZone = new Vector3(198.88f, 8.82f, -62.75f),
            Npc_Location = new Vector3(201.53f, 9.74f, -61.45f),
            Npc_Flag = new(201.53f, -61.45f),
            Button_BattleCraft = 0,
            Button_Gatherers = 1,
            Button_Crafters = 2,
            Button_Leave = 4,
            Mount = true,
            Leves = new()
            {
                48, 49, 50, 54, 55, 56, 108, 109, 110, 114, 115, 116, 168, 169, 170, 174,
                175, 176, 228, 229, 230, 234, 235, 236, 288, 289, 290, 294, 295, 296, 348,
                349, 350, 354, 355, 356, 408, 409, 410, 414, 415, 416, 468, 469, 470, 474,
                475, 476, 589, 590, 591, 592, 593, 594, 595, 596, 604, 605, 606, 607, 608,
                609, 610, 690, 691, 692, 693, 694, 695, 696, 697, 730, 731, 732, 733, 734,
                735, 736, 737, 770, 771, 772, 773, 774, 775, 776, 777,
            }
        },
        [1002397] = new()
        { // Merthelin
            Name = NPCName(1002397),
            TerritoryId = 153,
            Aetheryte = 6,
            Npc_InteractZone = new Vector3(-236.37f, 21.58f, 346.52f),
            Npc_Location = new Vector3(-238.97f, 22.02f, 344.38f),
            Npc_Flag = new(-238.97f, 344.38f),
            Button_BattleCraft = 0,
            Button_Gatherers = 1,
            Button_Crafters = 2,
            Button_Leave = 4,
            Mount = true,
            Leves = new()
            {
                617, 618, 619, 620, 621, 622,
            }
        },
        [1004738] = new()
        { // H'amneko, GC Leve NPC
            Name = NPCName(1004738),
            TerritoryId = 153,
            Aetheryte = 6,
            Npc_InteractZone = new Vector3(-196.36f, 14.96f, 445.59f),
            Npc_Location = new Vector3(-198.05f, 15.06f, 444.6f),
            Npc_Flag = new(-198.05f, 444.60f),
            Mount = true,
            Fly = true,
            Leves = new()
            {
                823, 824, 825, 826,
            }
        },
        [1002365] = new()
        { // Esmond
            Name = NPCName(1002365),
            TerritoryId = 146,
            Aetheryte = 19,
            Npc_InteractZone = new Vector3(-166.31f, 27.25f, -396.98f),
            Npc_Location = new Vector3(-167.28f, 27.44f, -395.44f),
            Npc_Flag = new(-167.28f, -395.44f),
            Button_BattleCraft = 0,
            Button_Gatherers = 1,
            Button_Crafters = 2,
            Button_Leave = 4,
            Mount = true,
            Leves = new()
            {
                611, 612, 614, 613, 616, 615,
            }
        },
        [1004740] = new()
        { // Blue Herring, GC Leve NPC
            Name = NPCName(1004740),
            TerritoryId = 146,
            Aetheryte = 19,
            Npc_InteractZone = new Vector3(-140.74f, 27.16f, -416.86f),
            Npc_Location = new Vector3(-137.9f, 27.64f, -417.93f),
            Npc_Flag = new(-137.90f, -417.93f),
            Mount = true,
            Leves = new()
            {
                828, 829, 830, 831,
            }
        },
        [1004344] = new()
        { // Nahctahr
            Name = NPCName(1004344),
            TerritoryId = 137, // Eastern La Noscea
            Aetheryte = 11,
            Npc_InteractZone = new Vector3(454.62418f, 16.995407f, 467.68893f),
            Npc_Location = new Vector3(450.22f, 17.75f, 470.3f),
            Npc_Flag = new(450.22f, 470.30f),
            Button_BattleCraft = 0,
            Button_Gatherers = 1,
            Button_Crafters = 2,
            Button_Leave = 4,
            Mount = true,
            Leves = new()
            {
                60, 61, 62, 120, 121, 122, 180, 181, 182, 240, 241, 242, 300, 301, 302,
                360, 361, 362, 420, 421, 422, 480, 481, 482, 623, 624, 625, 626, 627, 628,
                629, 698, 699, 700, 701, 738, 739, 740, 741, 778, 779, 780, 781,
            }
        },
        [1004736] = new()
        { // C'lafumyn, GC Leve NPC
            Name = NPCName(1004736),
            TerritoryId = 137,
            Aetheryte = 11,
            Npc_InteractZone = new Vector3(602.63f, 23.94f, 459.58f),
            Npc_Location = new Vector3(605.58f, 23.94f, 458.21f),
            Npc_Flag = new(605.58f, 458.21f),
            Mount = true,
            Leves = new()
            {
                818, 819, 820, 821,
            }
        },
        [1002367] = new()
        { // Aileen
            Name = NPCName(1002367),
            TerritoryId = 137,
            Aetheryte = 12,
            Npc_InteractZone = new Vector3(5.55f, 71.19f, -2.65f),
            Npc_Location = new Vector3(5.75f, 71.19f, 0.69f),
            Npc_Flag = new(5.75f, 0.69f),
            Button_BattleCraft = 0,
            Button_Gatherers = 1,
            Button_Crafters = 2,
            Button_Leave = 4,
            Mount = true,
            Leves = new()
            {
                630, 631, 632, 633, 634, 635,
            }
        },
        [1002384] = new()
        { // Cimeautrant
            Name = NPCName(1002384),
            TerritoryId = 155,
            Aetheryte = 23,
            Npc_InteractZone = new Vector3(228.83f, 222.00f, 339.32f),
            Npc_Location = new Vector3(227.59f, 222f, 341.66f),
            Npc_Flag = new(227.59f, 341.66f),
            Button_BattleCraft = 0,
            Button_Gatherers = 1,
            Button_Crafters = 2,
            Button_Leave = 4,
            Mount = true,
            Leves = new()
            {
                66, 67, 68, 126, 127, 128, 186, 187, 188, 246, 247, 248, 306, 307, 308,
                366, 367, 368, 426, 427, 428, 486, 487, 488, 636, 637, 638, 639, 640, 641,
                642, 702, 703, 704, 705, 742, 743, 744, 745, 782, 783, 784, 785,
            }
        },
        [1007068] = new()
        { // Haisie, GC Leve NPC
            Name = NPCName(1007068),
            TerritoryId = 155,
            Aetheryte = 23,
            Npc_InteractZone = new Vector3(183.58f, 222.83f, 355.89f),
            Npc_Location = new Vector3(179.8f, 223.02f, 359f),
            Npc_Flag = new(179.80f, 359.00f),
            Mount = true,
            Leves = new()
            {
                833, 834, 835, 838, 839, 840, 843, 844, 845,
            }
        },
        [1002401] = new()
        { // Voilnaut
            Name = NPCName(1002401),
            TerritoryId = 155,
            Aetheryte = 23,
            Npc_InteractZone = new Vector3(-443.83f, 211.00f, -234.07f),
            Npc_Location = new Vector3(-441.12f, 211f, -235.52f),
            Npc_Flag = new(-441.12f, -235.52f),
            Button_BattleCraft = 0,
            Button_Gatherers = 1,
            Button_Crafters = 2,
            Button_Leave = 4,
            Mount = true,
            Leves = new()
            {
                72, 73, 74, 132, 133, 134, 192, 193, 194, 252, 253, 254, 312, 313, 314,
                372, 373, 374, 432, 433, 434, 492, 493, 494, 649, 650, 651, 652, 653, 654,
                655, 706, 707, 708, 709, 746, 747, 748, 749, 786, 787, 788, 789,
            }
        },
        [1007069] = new()
        { // Lodille, GC Leve Npc
            Name = NPCName(1007069),
            TerritoryId = 155,
            Aetheryte = 23,
            Npc_InteractZone = new Vector3(-472.71f, 211.00f, -233.49f),
            Npc_Location = new Vector3(-476.22f, 211f, -233.42f),
            Npc_Flag = new(-476.22f, -233.42f),
            Mount = true,
            Leves = new()
            {
                848, 849, 850, 853, 854, 855, 858, 859, 860,
            }
        },
        [1002398] = new()
        { // Rurubana
            Name = NPCName(1002398),
            TerritoryId = 147,
            Aetheryte = 21,
            Npc_InteractZone = new Vector3(33.48f, 4.53f, 400.60f),
            Npc_Location = new Vector3(34.9f, 4.94f, 396.72f),
            Npc_Flag = new(34.90f, 396.72f),
            Button_BattleCraft = 0,
            Button_Gatherers = 1,
            Button_Crafters = 2,
            Button_Leave = 4,
            Mount = true,
            Leves = new()
            {
                643, 644, 645, 646, 647, 648,
            }
        },
        [1004348] = new()
        { // K'leytai
            Name = NPCName(1004348),
            TerritoryId = 156,
            Aetheryte = 24,
            Npc_InteractZone = new Vector3(418.62f, -5.81f, -447.56f),
            Npc_Location = new Vector3(415.98f, -6.06f, -444.42f),
            Npc_Flag = new(415.98f, -444.42f),
            Button_BattleCraft = 0,
            Button_Gatherers = 1,
            Button_Crafters = 2,
            Button_Leave = 4,
            Mount = true,
            Fly = true,
            Leves = new()
            {
                78, 79, 80, 138, 139, 140, 198, 199, 200, 258, 259, 260, 318, 319, 320,
                378, 379, 380, 438, 439, 440, 498, 499, 500, 656, 657, 658, 659, 660, 661,
                710, 711, 712, 713, 750, 751, 752, 753, 790, 791, 792, 793,
            }
        },
        [1007070] = new()
        { // Eidhart, GC Leve NPC
            Name = NPCName(1007070),
            TerritoryId = 156,
            Aetheryte = 24,
            Npc_InteractZone = new Vector3(462.87f, -4.39f, -470.22f),
            Npc_Location = new Vector3(464.59f, -4.06f, -467.43f),
            Npc_Flag = new(464.59f, -467.43f),
            Mount = true,
            Leves = new()
            {
                869, 863, 864, 865, 868, 870, 873, 874, 875,
            }
        },

        #endregion

        #region ARR Turnin NPC's

        [1001218] = new()
        { // Audrie
            Name = NPCName(1001218),
            TerritoryId = 148,
            Aetheryte = 3,
            Npc_InteractZone = new Vector3(46.55f, -5.97f, 3.54f),
            Npc_Location = new Vector3(49.24f, -6f, 1.14f),
            Npc_Flag = new(49.24f, 1.14f),
            Mount = true,
        },
        [1001220] = new()
        { // Juliembert
            Name = NPCName(1001220),
            TerritoryId = 153,
            Aetheryte = 5,
            Npc_InteractZone = new Vector3(167.13f, 8.70f, -48.60f),
            Npc_Location = new Vector3(167.71f, 9.21f, -45.94f),
            Npc_Flag = new(167.71f, -45.94f),
            Mount = true,
        },
        [1001868] = new()
        { // Lanverlais
            Name = NPCName(1001868),
            TerritoryId = 148,
            Aetheryte = 3,
            Npc_InteractZone = new Vector3(139.88f, -7.00f, -85.38f),
            Npc_Location = new Vector3(142.41f, -7f, -86.23f),
            Npc_Flag = new(142.41f, -86.23f),
            Mount = true,
        },
        [1001219] = new()
        { // Ayled
            Name = NPCName(1001219),
            TerritoryId = 152,
            Aetheryte = 4,
            Npc_InteractZone = new Vector3(-202.95f, 2.77f, 308.60f),
            Npc_Location = new Vector3(-206.44f, 2.56f, 307.79f),
            Npc_Flag = new(-206.44f, 307.79f),
            Mount = true,
        },
        [1001276] = new()
        { // Maisenta
            Name = NPCName(1001276),
            TerritoryId = 132, // New Gridania
            Aetheryte = 2,
            Npc_InteractZone = new Vector3(10.81f, 0.12f, 2.32f),
            Npc_Location = new Vector3(13.96f, 0.14f, 2.09f),
            Npc_Flag = new(13.96f, 2.09f),
            Mount = false,
        },
        [1004345] = new()
        { // Ririphon
            Name = NPCName(1004345),
            TerritoryId = 137, // Eastern La Noscea
            Aetheryte = 11,
            Npc_InteractZone = new Vector3(454.62418f, 16.995407f, 467.68893f),
            Npc_Location = new Vector3(452.41f, 17.75f, 464.84f),
            Npc_Flag = new(452.41f, 464.84f),
            Mount = true,
        },
        [1001787] = new()
        { // Bango Zango
            Name = NPCName(1001787),
            TerritoryId = 129,
            Aetheryte = 8,
            Npc_InteractZone = new Vector3(-64.87f, 18.00f, 8.23f),
            Npc_Location = new Vector3(-62.12f, 18f, 9.41f),
            Npc_Flag = new(-62.12f, 9.41f),
            Mount = false,
        },
        [1004343] = new()
        { // Zwynwyda
            Name = NPCName(1004343),
            TerritoryId = 135, // Lower La Noscea
            Aetheryte = 10,
            Npc_InteractZone = new Vector3(544.81f, 87.74f, -47.56f),
            Npc_Location = new Vector3(547.63f, 88.89f, -50.61f),
            Npc_Flag = new(547.63f, -50.61f),
            Mount = true,
        },
        [1001790] = new()
        { // Fewon Bulion
            Name = NPCName(1001790),
            TerritoryId = 138,
            Aetheryte = 13,
            Npc_InteractZone = new Vector3(652.15f, 9.54f, 503.27f),
            Npc_Location = new Vector3(649.5f, 9.55f, 503.62f),
            Npc_Flag = new(649.50f, 503.62f),
            Mount = true,
        },
        [1001793] = new()
        { // H'rhanbolo
            Name = NPCName(1001793),
            TerritoryId = 138,
            Aetheryte = 14,
            Npc_InteractZone = new Vector3(292.18f, -24.99f, 238.09f),
            Npc_Location = new Vector3(292.1f, -25f, 235.34f),
            Npc_Flag = new(292.10f, 235.34f),
            Mount = true,
        },
        [1004417] = new()
        { // Roarich
            Name = NPCName(1004417),
            TerritoryId = 130,
            Aetheryte = 9,
            Npc_InteractZone = new Vector3(-30.76f, 9.00f, -85.04f),
            Npc_Location = new Vector3(-33.62f, 9.11f, -84.28f),
            Npc_Flag = new(-33.62f, -84.28f),
            Mount = false,
        },
        [1003889] = new()
        { // Gigiyon
            Name = NPCName(1003889),
            TerritoryId = 140,
            Aetheryte = 17,
            Npc_InteractZone = new Vector3(213.17f, 52.04f, 153.74f),
            Npc_Location = new Vector3(213.46f, 52.04f, 151.45f),
            Npc_Flag = new(213.46f, 151.45f),
            Mount = true,
        },
        [1001798] = new()
        { // Mimina
            Name = NPCName(1001798),
            TerritoryId = 140,
            Aetheryte = 17,
            Npc_InteractZone = new Vector3(69.39f, 46.00f, -248.66f),
            Npc_Location = new Vector3(69.35f, 46f, -251.94f),
            Npc_Flag = new(69.35f, -251.94f),
            Mount = true,
        },
        [1001801] = new()
        { // Frediswitha
            Name = NPCName(1001801),
            TerritoryId = 145,
            Aetheryte = 18,
            Npc_InteractZone = new Vector3(-396.09f, -57.08f, 125.65f),
            Npc_Location = new Vector3(-394.28f, -57.33f, 128.92f),
            Npc_Flag = new(-394.28f, 128.92f),
            Mount = true,
        },
        [1002385] = new()
        { // Vivenne
            Name = NPCName(1002385),
            TerritoryId = 155,
            Aetheryte = 23,
            Npc_InteractZone = new Vector3(240.60f, 221.73f, 331.01f),
            Npc_Location = new Vector3(239.06f, 222.21f, 327.87f),
            Npc_Flag = new(239.06f, 327.87f),
            Mount = true,
        },
        [1002402] = new()
        { // Lanquairt
            Name = NPCName(1002402),
            TerritoryId = 155,
            Aetheryte = 23,
            Npc_InteractZone = new Vector3(-410.40f, 211.00f, -266.98f),
            Npc_Location = new Vector3(-405.75f, 210.79f, -267.75f),
            Npc_Flag = new(-405.75f, -267.75f),
            Mount = true,
        },
        [1004349] = new()
        { // Syele
            Name = NPCName(1004349),
            TerritoryId = 156,
            Aetheryte = 24,
            Npc_InteractZone = new Vector3(443.09f, -4.79f, -455.26f),
            Npc_Location = new Vector3(445f, -4.49f, -453.03f),
            Npc_Flag = new(445f, -453.03f),
            Mount = true,
        },
        [1007060] = new()
        { // Unsynwilf
            Name = NPCName(1007060),
            TerritoryId = 128,
            Aetheryte = 8,
            Npc_InteractZone = new Vector3(-62.886135f, 42.023273f, -132.9867f),
            Npc_Location = new Vector3(-61.54f, 42f, -134.94f),
            Npc_Flag = new(-61.54f, -134.94f),
            Mount = false,
        },
        [1007061] = new()
        { // Q'molosi
            Name = NPCName(1007061),
            TerritoryId = 138,
            Aetheryte = 13,
            Npc_InteractZone = new Vector3(653.0555f, 9.460755f, 505.43054f),
            Npc_Location = new Vector3(649.81f, 9.51f, 505f),
            Npc_Flag = new(649.80f, 505f),
            Mount = true,
        },
        [1007062] = new()
        { // Fupepe
            Name = NPCName(1007062),
            TerritoryId = 138,
            Aetheryte = 14,
            Npc_InteractZone = new Vector3(291.06613f, -24.990849f, 237.48279f),
            Npc_Location = new Vector3(290.97f, -24.99f, 235.61f),
            Npc_Flag = new(291.06f, 237.48f),
            Mount = true,
        },
        [1007063] = new()
        { // Daca Jinjahl
            Name = NPCName(1007063),
            TerritoryId = 153,
            Aetheryte = 5,
            Npc_InteractZone = new Vector3(168.63585f, 8.649968f, -48.859486f),
            Npc_Location = new Vector3(168.99f, 9.09f, -46.01f),
            Npc_Flag = new(168.99365f, -46.00598f),
            Mount = false,
        },
        [1007064] = new()
        { // F'abodji
            Name = NPCName(1007064),
            TerritoryId = 137,
            Aetheryte = 11,
            Npc_InteractZone = new Vector3(454.03705f, 17.493235f, 475.09576f),
            Npc_Location = new Vector3(454.98f, 17.25f, 479.97f),
            Npc_Flag = new(489.15845f, 468.80298f),
            Mount = false,
        },
        [1007065] = new()
        { // F'abobji
            Name = NPCName(1007065),
            TerritoryId = 155,
            Aetheryte = 23,
            Npc_InteractZone = new Vector3(241.34195f, 222.04169f, 329.3354f),
            Npc_Location = new Vector3(239.98f, 222.23f, 327.47f),
            Npc_Flag = new(239.97852f, 327.47388f),
            Mount = true,
            Fly = true,
        },
        [1007066] = new()
        { // Drividot
            Name = NPCName(1007066),
            TerritoryId = 155,
            Aetheryte = 23,
            Npc_InteractZone = new Vector3(-408.24942f, 210.78818f, -268.77814f),
            Npc_Location = new Vector3(-405.51f, 210.79f, -269f),
            Npc_Flag = new(-405.50854f, -269.00134f),
            Mount = true,
            Fly = true,
        },
        [1007067] = new()
        { // Clifton
            Name = NPCName(1007067),
            TerritoryId = 156,
            Aetheryte = 24,
            Npc_InteractZone = new Vector3(442.1942f, -4.776814f, -447.35138f),
            Npc_Location = new Vector3(445f, -4.34f, -446.01f),
            Npc_Flag = new(444.99866f, -446.00598f),
            Mount = true,
            Fly = true,
        },

        #endregion

        #region Heavensword

        [1011208] = new()
        { // Eloin
            Name = NPCName(1011208),
            TerritoryId = 418,
            Aetheryte = 70,
            Npc_InteractZone = new Vector3(-56.18f, 15.14f, -41.45f),
            Npc_Location = new Vector3(-53.54f, 15.2f, -42.68f),
            Npc_Flag = new(-53.54f, -42.68f),
            Button_BattleCraft = 0,
            Button_Gatherers = 1,
            Button_Crafters = 2,
            Button_LSGather = 4,
            Button_LSCrafter = 5,
            Button_Leave = 7,
            Mount = false,
            Leves = new()
            {
                878, 879, 880, 881, 882, 883, 884, 885, 886, 887, 888, 889, 890, 891, 892,
                893, 894, 895, 896, 897, 898, 899, 900, 901, 902, 903, 904, 905, 906, 907,
                908, 909, 910, 911, 912, 913, 914, 915, 916, 917, 918, 919, 920, 921, 922,
                923, 924, 925, 926, 927, 928, 929, 930, 931, 932, 933, 934, 935, 936, 937,
                938, 939, 940, 941, 942, 943, 944, 945, 946, 947, 948, 949, 950, 951, 952,
                953, 954, 955, 956, 957, 958, 959, 960, 961, 962, 963, 964, 965, 966, 967,
                968, 969, 970, 971, 972, 973, 974, 975, 976, 977, 978, 979, 980, 981, 982,
                983, 984, 985, 986, 987, 988, 989, 990, 991, 992, 993, 994, 995, 996, 997,
                998, 999, 1000, 1001, 1002, 1003, 1004, 1005, 1006, 1007, 1008, 1009, 1010,
                1011, 1012, 1013, 1014, 1015, 1016, 1017, 1018, 1019, 1020, 1021, 1022,
                1023, 1024, 1025, 1026, 1027, 1028, 1029, 1030, 1031, 1032, 1033, 1034,
                1035, 1036, 1037, 1038, 1039, 1040, 1041, 1042, 1043, 1044, 1045, 1046,
                1047, 1048, 1049, 1050, 1051, 1052, 1053, 1054, 1055, 1056, 1057, 1058,
                1059, 1060, 1061, 1062, 1063, 1064, 1065, 1066, 1067, 1068, 1069, 1070,
                1071, 1072, 1073, 1074, 1075, 1076, 1077, 1078, 1079, 1080, 1081, 1082,
                1083, 1084, 1085, 1086, 1087, 1088, 1089, 1090, 1091, 1092, 1093, 1094,
                1095, 1096, 1097, 1098, 1099, 1100, 1101, 1102, 1103, 1104, 1105, 1106,
                1107, 1108, 1109, 1110, 1111, 1112, 1113, 1114, 1115, 1116, 1117, 1118,
                1119, 1120, 1121, 1122, 1123, 1124, 1125, 1126, 1127, 1128, 1129, 1130,
                1131, 1132, 1133, 1134, 1135, 1136, 1137, 1138, 1139, 1140, 1141, 1142,
                1143, 1144, 1145, 1146, 1147, 1148, 1149, 1150, 1151, 1152, 1153, 1154,
                1155, 1156, 1157, 1158, 1159, 1160, 1161, 1162, 1163, 1164, 1165, 1166,
                1167, 1168, 1169, 1170, 1171, 1172, 1173, 1174, 1175, 1176, 1177, 1178,
                1179, 1180, 1181, 1182, 1183, 1184, 1185, 1186, 1187, 1188, 1189, 1190,
                1191, 1192, 1193, 1194, 1195, 1196, 1197, 1198, 1199, 1200, 1201, 1202,
                1203, 1204, 1205, 1206, 1207, 1208, 1209, 1210, 1211, 1212, 1213, 1214,
                1215, 1216, 1217, 1218, 1219, 1220, 1221, 1222, 1223, 1224, 1225, 1226,
                1227, 1228, 1229, 1230, 1231, 1232, 1233, 1234, 1235, 1236, 1237,
            }
        },
        [1011209] = new()
        { // Fionnuala
            Name = NPCName(1011209),
            TerritoryId = 418,
            Aetheryte = 70,
            Npc_InteractZone = new Vector3(-56.18f, 15.14f, -41.45f),
            Npc_Location = new Vector3(-54.31f, 15.19f, -44.51f),
            Npc_Flag = new(-54.31f, -44.51f),
            Mount = false,
        },
        [1011210] = new()
        { // Cesteline
            Name = NPCName(1011210),
            TerritoryId = 418,
            Aetheryte = 70,
            Npc_InteractZone = new Vector3(-56.18f, 15.14f, -41.45f),
            Npc_Location = new Vector3(-52.69f, 15.23f, -40.82f),
            Npc_Flag = new(-52.69f, -40.82f),
            Mount = false,
        },

        #endregion

        #region Stormblood

        [1018997] = new()
        { // Keltraeng
            Name = NPCName(1018997),
            TerritoryId = 628,
            Aetheryte = 111,
            Npc_InteractZone = new Vector3(20.61f, 0.00f, -77.82f),
            Npc_Location = new Vector3(20.49f, -0f, -80.95f),
            Npc_Flag = new(20.49f, -80.95f),
            Button_Gatherers = 0,
            Button_Crafters = 1,
            Button_Leave = 3,
            Mount = false,
            Leves = new()
            {
                1238, 1239, 1240, 1241, 1242, 1243, 1244, 1245, 1246, 1247, 1248, 1249,
                1250, 1251, 1252, 1253, 1254, 1255, 1256, 1257, 1258, 1259, 1260, 1261,
                1262, 1263, 1264, 1265, 1266, 1267, 1268, 1269, 1270, 1271, 1272, 1273,
                1274, 1275, 1276, 1277, 1278, 1279, 1280, 1281, 1282, 1283, 1284, 1285,
                1286, 1287, 1288, 1289, 1290, 1291, 1292, 1293, 1294, 1295, 1296, 1297,
                1298, 1299, 1300, 1301, 1302, 1303, 1304, 1305, 1306, 1307, 1308, 1309,
                1310, 1311, 1312, 1313, 1314, 1315, 1316, 1317, 1318, 1319, 1320, 1321,
                1322, 1323, 1324, 1325, 1326, 1327, 1328, 1329, 1330, 1331, 1332, 1333,
                1334, 1335, 1336, 1337, 1338, 1339, 1340, 1341, 1342, 1343, 1344, 1345,
                1346, 1347, 1348, 1349, 1350, 1351, 1352, 1353, 1354, 1355, 1356, 1357,
                1358, 1359, 1360, 1361, 1362, 1363, 1364, 1365, 1366, 1367, 1368, 1369,
                1370, 1371, 1372, 1373, 1374, 1375, 1376, 1377, 1378, 1379, 1380, 1381,
                1382, 1383, 1384, 1385, 1386, 1387, 1388, 1389, 1390, 1391, 1392, 1393,
                1394, 1395, 1396, 1397, 1398, 1399, 1400, 1401, 1402,
            }
        },
        [1018998] = new()
        { // Chantine
            Name = NPCName(1018998),
            TerritoryId = 628,
            Aetheryte = 111,
            Npc_InteractZone = new Vector3(20.61f, 0.00f, -77.82f),
            Npc_Location = new Vector3(17.26f, -0f, -81.32f),
            Npc_Flag = new(17.26f, -81.32f),
            Mount = false,
        },
        [1018999] = new()
        { // Geimrael
            Name = NPCName(1018999),
            TerritoryId = 628,
            Aetheryte = 111,
            Npc_InteractZone = new Vector3(20.61f, 0.00f, -77.82f),
            Npc_Location = new Vector3(23.67f, -8.06f, -81.38f),
            Npc_Flag = new(23.67f, -81.38f),
            Mount = false,
        },

        #endregion

        #region Shadowbringers

        [1027847] = new()
        { // Eirikur
            Name = NPCName(1027847),
            TerritoryId = 819,
            Aetheryte = 133,
            Npc_InteractZone = new Vector3(-73.40f, 20.00f, -110.90f),
            Npc_Location = new Vector3(-76.34f, 20.05f, -110.98f),
            Npc_Flag = new(-76.34f, -110.98f),
            Button_Gatherers = 0,
            Button_Crafters = 1,
            Button_Leave = 3,
            Mount = false,
            Leves = new()
            {
                1403, 1404, 1405, 1406, 1407, 1408, 1409, 1410, 1411, 1412, 1413, 1414,
                1415, 1416, 1417, 1418, 1419, 1420, 1421, 1422, 1423, 1424, 1425, 1426,
                1427, 1428, 1429, 1430, 1431, 1432, 1433, 1434, 1435, 1436, 1437, 1438,
                1439, 1440, 1441, 1442, 1443, 1444, 1445, 1446, 1447, 1448, 1449, 1450,
                1451, 1452, 1453, 1454, 1455, 1456, 1457, 1458, 1459, 1460, 1461, 1462,
                1463, 1464, 1465, 1466, 1467, 1468, 1469, 1470, 1471, 1472, 1473, 1474,
                1475, 1476, 1477, 1478, 1479, 1480, 1481, 1482, 1483, 1484, 1485, 1486,
                1487, 1488, 1489, 1490, 1491, 1492, 1493, 1494, 1495, 1496, 1497, 1498,
                1499, 1500, 1501, 1502, 1503, 1504, 1505, 1506, 1507, 1508, 1509, 1510,
                1511, 1512, 1513, 1514, 1515, 1516, 1517, 1518, 1519, 1520, 1521, 1522,
                1523, 1524, 1525, 1526, 1527, 1528, 1529, 1530, 1531, 1532, 1533, 1534,
                1535, 1536, 1537, 1538, 1539, 1540, 1541, 1542, 1543, 1544, 1545, 1546,
                1547, 1548, 1549, 1550, 1551, 1552, 1553, 1554, 1555, 1556, 1557, 1558,
                1559, 1560, 1561, 1562, 1563, 1564, 1565, 1566, 1567,
            }
        },
        [1027848] = new()
        { // Moyce
            Name = NPCName(1027848),
            TerritoryId = 819,
            Aetheryte = 133,
            Npc_InteractZone = new Vector3(-73.40f, 20.00f, -110.90f),
            Npc_Location = new Vector3(-76.19f, 20.05f, -113.88f),
            Npc_Flag = new(-76.49f, -107.96f),
            Mount = false,
        },
        [1027849] = new()
        { // Shue-Hann
            Name = NPCName(1027849),
            TerritoryId = 819,
            Aetheryte = 133,
            Npc_InteractZone = new Vector3(-73.40f, 20.00f, -110.90f),
            Npc_Location = new Vector3(-76.49f, 20.05f, -107.96f),
            Npc_Flag = new(-76.49f, -107.96f),
            Mount = false,
        },

        #endregion

        #region Endwalker

        [1037263] = new()
        { // Grigge
            Name = NPCName(1037263),
            TerritoryId = 962,
            Aetheryte = 182,
            Npc_InteractZone = new Vector3(49.96f, -15.65f, 111.81f),
            Npc_Location = new Vector3(46.83f, -15.65f, 107.87f),
            Npc_Flag = new(46.83f, 107.87f),
            Button_Gatherers = 0,
            Button_Crafters = 1,
            Button_Leave = 3,
            Mount = false,
            Leves = new()
            {
                1568, 1569, 1570, 1571, 1572, 1573, 1574, 1575, 1576, 1577, 1578, 1579,
                1580, 1581, 1582, 1583, 1584, 1585, 1586, 1587, 1588, 1589, 1590, 1591,
                1592, 1593, 1594, 1595, 1596, 1597, 1598, 1599, 1600, 1601, 1602, 1603,
                1604, 1605, 1606, 1607, 1608, 1609, 1610, 1611, 1612, 1613, 1614, 1615,
                1616, 1617, 1618, 1619, 1620, 1621, 1622, 1623, 1624, 1625, 1626, 1627,
                1628, 1629, 1630, 1631, 1632, 1633, 1634, 1635, 1636, 1637, 1638, 1639,
                1640, 1641, 1642, 1643, 1644, 1645, 1646, 1647, 1648, 1649, 1650, 1651,
                1652, 1653, 1654, 1655, 1656, 1657, 1658, 1659, 1660, 1661, 1662, 1663,
                1664, 1665, 1666, 1667, 1668, 1669, 1670, 1671, 1672, 1673, 1674, 1675,
                1676, 1677, 1678, 1679, 1680, 1681, 1682, 1683, 1684, 1685, 1686, 1687,
            }
        },
        [1037264] = new()
        { // Ahldiyrn
            Name = NPCName(1037264),
            TerritoryId = 962,
            Aetheryte = 182,
            Npc_InteractZone = new Vector3(49.96f, -15.65f, 111.81f),
            Npc_Location = new Vector3(53.48f, -15.65f, 109.73f),
            Npc_Flag = new(53.48f, 109.73f),
            Mount = false,
        },
        [1037265] = new()
        { // (name missing in original)
            Name = NPCName(1037265),
            TerritoryId = 962,
            Aetheryte = 182,
            Npc_InteractZone = new Vector3(49.96f, -15.65f, 111.81f),
            Npc_Location = new Vector3(55.38f, -15.65f, 109.73f),
            Npc_Flag = new(55.38f, 109.73f),
            Mount = false,
        },

        #endregion

        #region Dawntrail

        [1048390] = new()
        { // Malihali
            Name = NPCName(1048390),
            TerritoryId = 1185,
            Aetheryte = 216,
            Npc_InteractZone = new Vector3(20.58f, -14f, 86.46f),
            Npc_Location = new Vector3(15.24f, -14f, 85.83f),
            Npc_Flag = new(15.24f, 85.83f),
            Button_Gatherers = 0,
            Button_Crafters = 1,
            Button_Leave = 3,
            Mount = false,
            Leves = new()
            {
                1688, 1689, 1690, 1691, 1692, 1693, 1694, 1695, 1696, 1697, 1698, 1699,
                1700, 1701, 1702, 1703, 1704, 1705, 1706, 1707, 1708, 1709, 1710, 1711,
                1712, 1713, 1714, 1715, 1716, 1717, 1718, 1719, 1720, 1721, 1722, 1723,
                1724, 1725, 1726, 1727, 1728, 1729, 1730, 1731, 1732, 1733, 1734, 1735,
                1736, 1737, 1738, 1739, 1740, 1741, 1742, 1743, 1744, 1745, 1746, 1747,
                1748, 1749, 1750, 1751, 1752, 1753, 1754, 1755, 1756, 1757, 1758, 1759,
                1760, 1761, 1762, 1763, 1764, 1765, 1766, 1767, 1768, 1769, 1770, 1771,
                1772, 1773, 1774, 1775, 1776, 1777, 1778, 1779, 1780, 1781, 1782, 1783,
                1784, 1785, 1786, 1787, 1788, 1789, 1790, 1791, 1792, 1793, 1794, 1795,
                1796, 1797, 1798, 1799, 1800, 1801, 1802, 1803, 1804, 1805, 1806, 1807,
            }
        },
        [1048391] = new()
        { // Ponawme
            Name = NPCName(1048391),
            TerritoryId = 1185,
            Aetheryte = 216,
            Npc_InteractZone = new Vector3(23.84f, -14f, 83.45f),
            Npc_Location = new Vector3(21.23f, -14f, 80.22f),
            Npc_Flag = new(21.23f, 80.22f),
            Mount = false,
        },
        [1048392] = new()
        { // Br'uk Ts'on
            Name = NPCName(1048392),
            TerritoryId = 1185,
            Aetheryte = 216,
            Npc_InteractZone = new Vector3(23.84f, -14f, 83.45f),
            Npc_Location = new Vector3(23.09f, -14f, 78.39f),
            Npc_Flag = new(23.09f, 78.39f),
            Mount = false,
        },

        #endregion
    };

    public static HashSet<uint> IshgardTurnin = new() 
    { 
        153, 151, 154, 156, 155, 152, 157 
    };
    public static HashSet<uint> KuganeTurnin = new() 
    { 
        159, 161, 162, 171, 172, 174, 
        175, 176, 177, 178, 179, 180, 
        181, 182, 183, 184, 185, 186, 
        187, 188, 189, 190, 191, 192, 
        193, 194 
    };
    public static HashSet<uint> CrystariumTurnin = new() 
    { 
        195, 196, 199, 200, 205, 206, 
        207, 208, 209, 210, 211, 212, 
        213, 214, 215, 216, 217, 218, 
        219, 220, 221, 222, 223, 224, 
        225, 226, 227, 228 
    };
    public static HashSet<uint> SharlayanTurnin = new() 
    { 
        229, 230, 231, 232, 233, 234, 
        235, 236, 237, 238, 239, 240, 
        241, 242, 243, 244, 245, 246, 
        247, 248, 254, 255, 256 
    };
    public static HashSet<uint> TuliyoliTurnin = new() 
    { 
        257, 258, 259, 260, 261, 262, 
        263, 264, 265, 266, 267, 268, 
        269, 270, 271, 272, 273, 274, 
        275, 276, 277, 278, 279, 280, 
        281, 282, 
    };

    private static readonly Dictionary<uint, uint> LeveClientToTurninNpc = new()
    {
        // Central Shroud, Audrie
        [5] = 1001218,
        // East Shroud, Ayled
        [7] = 1001219,
        // New Gridania, Maisenta
        [9] = 1001276,
        // South Shroud, Juliembert
        [13] = 1001220,
        // Central Shroud, Lanverlais
        [19] = 1001868,
        // Eastern La Noscea, Ririphon
        [113] = 1004345,
        // Limsa Lominsa Lower Decks, Bango Zango
        [114] = 1001787,
        // Lower La Noscea, Zwynwyda
        [115] = 1004343,
        // Western La Noscea, Fewon Bulion
        [116] = 1001790,
        // Western La Noscea, H'rhanbolo
        [117] = 1001793,
        // Ul'dah - Steps of Nald, Roarich
        [118] = 1004417,
        // Western Thanalan, Gigiyon
        [119] = 1003889,
        // Western Thanalan, Mimina
        [120] = 1001798,
        // Eastern Thanalan, Frediswitha
        [121] = 1001801,
        // Coerthas Central Highlands, Vivenne
        [122] = 1002385,
        // Coerthas Central Highlands, Lanquairt
        [123] = 1002402,
        // Mor Dhona, Syele
        [124] = 1004349,
        // Unsynwilf, Upper Limsa
        [141] = 1007060,
        // Q'molosi, Western La Noscea, Swiftperch
        [142] = 1007061,
        // Fupepe, Western La Noscea, Aleport
        [143] = 1007062,
        // Daca Jinjahl
        [144] = 1007063,
        // F'abodji, Eastern La Noscea
        [145] = 1007064,
        // Coerthas Central Highlands, SE corner
        [146] = 1007065,
        // Louviaune, Coerthas Central Highlands, NW corner
        [147] = 1007066,
        [148] = 1007067,
    };

    // Expansion-specific turnin NPCs (post-ARR pattern: gatherer/crafter split)
    private static readonly Dictionary<string, (HashSet<uint> LeveClients, uint GathererNpc, uint CrafterNpc)> ExpansionTurnins = new()
    {
        ["Ishgard"] = (IshgardTurnin, 1011209, 1011210),       // Fionnuala, Cesteline
        ["Kugane"] = (KuganeTurnin, 1018998, 1018999),         // Chantine, Geimrael
        ["Crystarium"] = (CrystariumTurnin, 1027848, 1027849), // Moyce, Shue-Hann
        ["Sharlayan"] = (SharlayanTurnin, 1037264, 1037265),   // Ahldiyrn, Wurtwyb
        ["Tuliyoli"] = (TuliyoliTurnin, 1048391, 1048392),     // 
    };

    public static uint TurninNpcId(uint leveClient, uint JobType = 0)
    {
        // Check ARR direct mapping first
        if (LeveClientToTurninNpc.TryGetValue(leveClient, out uint npcId))
        {
            return npcId;
        }

        // Check expansion-specific NPCs (post-ARR)
        foreach (var (leveClients, gathererNpc, crafterNpc) in ExpansionTurnins.Values)
        {
            if (leveClients.Contains(leveClient))
            {
                return JobType == 4 ? crafterNpc : gathererNpc;
            }
        }

        return 0; // No match found
    }

    public static string NPCName(uint NpcID)
    {
        var NPCSheet = Svc.Data.GetExcelSheet<ENpcResident>();
        return NPCSheet.GetRow(NpcID).Singular.ToString();
    }
}
