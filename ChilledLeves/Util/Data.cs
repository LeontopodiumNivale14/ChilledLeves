using Dalamud.Interface.Textures;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilledLeves.Util;

public static unsafe class Data
{
    // This is where the nightmare begins
    // Figuring how how the *-FUCKK-* to manage this in a neat and nicely manner

    // Some things to think about:
    // -> Godbert has virtually *-all-* the information in the "Leve" Sheet, which includes but not limited to
    // *THIS IS IN THE "Leve" Sheet
    // Key (leve id) (can be an int in this case... hopefully)
    // 0: Name (String)
    // 4: Assignment Type (Aka class it's tied to in the sheet) (int32)
    // 5: Town (ZoneID aka Int32)
    // 6: Level required for job (Uint16)
    // 8: Allowance Cost (Byte, usually 1 but this can change for grand scale FC stuff (think HW))
    // 9: Start Town {Int32)
    // 10: End Town (Int32)
    // 13: ClassJobCategory (Byte)
    // 17: Item Data ID (aka item being turned in) (Int32)
    // 
    // IF I START CARING TO PUT THIS IN AS WELL:
    // 21: Experience Reward (Uint32)
    // 22: Gil per leve (Uint32)

    // Now to get the actual information on what the leve entails more of, that's in "CraftLeve"
    // The key for this is from the sheet "Leve" on row #17 (DataId)
    // This value is given to give the sheet to tell
    // -> #2 How many times it can be repeated
    // -> # 3/4 | 5/6 | 7/8 | 9/10 are the item ID's + the amount that it needs to turnin

    // LeveAssignmentType is also a tab to keep note of
    // This has the icons for the leve types
    // 5 = CRP | 6 = BSM | 7 = ARM | 8 = GSM | 9 = LTW | 10 = WVR | 11 = ALC | 12 = CUL | BTN = 3 | MIN = 2 | FSH = 4

    // Baseline for the tables to be setup, this is moreso for organization of starting things (Can grab majority from the sheets but, need to have something to reference to begin with for Ui reasons I feel)
    // I can hear Croizat bitching at me for this already

    // List of all of the Crafter Job ID's into a nice checklist for self
    public static HashSet<uint> CrafterJobs = new() { 5, 6, 7, 8, 9, 10, 11, 12};

    // Old/Depreciated. Don't wanna delete yet thought because it ties to old sheets that need to be removed once I'm confident they can go
    public static List<uint> CraftingClass = new List<uint>();
    public static List<bool> CraftingClassActive = new List<bool>();

    public static List<string> AllLocations = new List<string>();
    public static List<bool> LocationsActive = new List<bool>();

    public static List<uint> LeveNumber = new List<uint>();
    public static List<int> LeveAmount = new List<int>();
    // End of that

    public static int JobSelected = 0;

    public class LeveDataforTable
    {
        public uint LeveID { get; set; }
        public int Amount { get; set; }

        public LeveDataforTable(uint leveID, int amount)
        {
            LeveID = leveID;
            Amount = amount;
        }
    }

    public static class LevesTableOld
    {
        // Tuli (1185)
        public static List<LeveDataforTable> CrafterLeves = 
        [

            #region Lv. 90-98

            // CRP
            new LeveDataforTable(1697, 0),
            new LeveDataforTable(1696, 0),
            new LeveDataforTable(1695, 0),
            new LeveDataforTable(1694, 0),
            new LeveDataforTable(1693, 0),
            new LeveDataforTable(1692, 0),
            new LeveDataforTable(1691, 0),
            new LeveDataforTable(1690, 0),
            new LeveDataforTable(1689, 0),
            new LeveDataforTable(1688, 0),

            // BSM
            new LeveDataforTable(1707, 0),
            new LeveDataforTable(1706, 0),
            new LeveDataforTable(1705, 0),
            new LeveDataforTable(1704, 0),
            new LeveDataforTable(1703, 0),
            new LeveDataforTable(1702, 0),
            new LeveDataforTable(1701, 0),
            new LeveDataforTable(1700, 0),
            new LeveDataforTable(1699, 0),
            new LeveDataforTable(1698, 0),

            // ARM
            new LeveDataforTable(1717, 0),
            new LeveDataforTable(1716, 0),
            new LeveDataforTable(1715, 0),
            new LeveDataforTable(1714, 0),
            new LeveDataforTable(1713, 0),
            new LeveDataforTable(1712, 0),
            new LeveDataforTable(1711, 0),
            new LeveDataforTable(1710, 0),
            new LeveDataforTable(1709, 0),
            new LeveDataforTable(1708, 0),

            // GSM
            new LeveDataforTable(1727, 0),
            new LeveDataforTable(1726, 0),
            new LeveDataforTable(1725, 0),
            new LeveDataforTable(1724, 0),
            new LeveDataforTable(1723, 0),
            new LeveDataforTable(1722, 0),
            new LeveDataforTable(1721, 0),
            new LeveDataforTable(1720, 0),
            new LeveDataforTable(1719, 0),
            new LeveDataforTable(1718, 0),

            // LTW
            new LeveDataforTable(1737, 0),
            new LeveDataforTable(1736, 0),
            new LeveDataforTable(1735, 0),
            new LeveDataforTable(1734, 0),
            new LeveDataforTable(1733, 0),
            new LeveDataforTable(1732, 0),
            new LeveDataforTable(1731, 0),
            new LeveDataforTable(1730, 0),
            new LeveDataforTable(1729, 0),
            new LeveDataforTable(1728, 0),

            // WVR
            new LeveDataforTable(1747, 0),
            new LeveDataforTable(1746, 0),
            new LeveDataforTable(1745, 0),
            new LeveDataforTable(1744, 0),
            new LeveDataforTable(1743, 0),
            new LeveDataforTable(1742, 0),
            new LeveDataforTable(1741, 0),
            new LeveDataforTable(1740, 0),
            new LeveDataforTable(1739, 0),
            new LeveDataforTable(1738, 0),

            // ALC
            new LeveDataforTable(1757, 0),
            new LeveDataforTable(1756, 0),
            new LeveDataforTable(1755, 0),
            new LeveDataforTable(1754, 0),
            new LeveDataforTable(1753, 0),
            new LeveDataforTable(1752, 0),
            new LeveDataforTable(1751, 0),
            new LeveDataforTable(1750, 0),
            new LeveDataforTable(1749, 0),
            new LeveDataforTable(1748, 0),

            // CUL
            new LeveDataforTable(1767, 0),
            new LeveDataforTable(1766, 0),
            new LeveDataforTable(1765, 0),
            new LeveDataforTable(1764, 0),
            new LeveDataforTable(1763, 0),
            new LeveDataforTable(1762, 0),
            new LeveDataforTable(1761, 0),
            new LeveDataforTable(1760, 0),
            new LeveDataforTable(1759, 0),
            new LeveDataforTable(1758, 0),

            #endregion

            #region Lv. 80-88

            // CRP
            new LeveDataforTable(1577, 0),
            new LeveDataforTable(1576, 0),
            new LeveDataforTable(1575, 0),
            new LeveDataforTable(1574, 0),
            new LeveDataforTable(1573, 0),
            new LeveDataforTable(1572, 0),
            new LeveDataforTable(1571, 0),
            new LeveDataforTable(1570, 0),
            new LeveDataforTable(1569, 0),
            new LeveDataforTable(1568, 0),

            // BSM
            new LeveDataforTable(1587, 0),
            new LeveDataforTable(1586, 0),
            new LeveDataforTable(1585, 0),
            new LeveDataforTable(1584, 0),
            new LeveDataforTable(1583, 0),
            new LeveDataforTable(1582, 0),
            new LeveDataforTable(1581, 0),
            new LeveDataforTable(1580, 0),
            new LeveDataforTable(1579, 0),
            new LeveDataforTable(1578, 0),

            // ARM
            new LeveDataforTable(1597, 0),
            new LeveDataforTable(1596, 0),
            new LeveDataforTable(1595, 0),
            new LeveDataforTable(1594, 0),
            new LeveDataforTable(1593, 0),
            new LeveDataforTable(1592, 0),
            new LeveDataforTable(1591, 0),
            new LeveDataforTable(1590, 0),
            new LeveDataforTable(1589, 0),
            new LeveDataforTable(1588, 0),

            // GSM
            new LeveDataforTable(1607, 0),
            new LeveDataforTable(1606, 0),
            new LeveDataforTable(1605, 0),
            new LeveDataforTable(1604, 0),
            new LeveDataforTable(1603, 0),
            new LeveDataforTable(1602, 0),
            new LeveDataforTable(1601, 0),
            new LeveDataforTable(1600, 0),
            new LeveDataforTable(1599, 0),
            new LeveDataforTable(1598, 0),

            // LTW
            new LeveDataforTable(1617, 0),
            new LeveDataforTable(1616, 0),
            new LeveDataforTable(1615, 0),
            new LeveDataforTable(1614, 0),
            new LeveDataforTable(1613, 0),
            new LeveDataforTable(1612, 0),
            new LeveDataforTable(1611, 0),
            new LeveDataforTable(1610, 0),
            new LeveDataforTable(1609, 0),
            new LeveDataforTable(1608, 0),

            // WVR
            new LeveDataforTable(1627, 0),
            new LeveDataforTable(1626, 0),
            new LeveDataforTable(1625, 0),
            new LeveDataforTable(1624, 0),
            new LeveDataforTable(1623, 0),
            new LeveDataforTable(1622, 0),
            new LeveDataforTable(1621, 0),
            new LeveDataforTable(1620, 0),
            new LeveDataforTable(1619, 0),
            new LeveDataforTable(1618, 0),

            // ALC
            new LeveDataforTable(1637, 0),
            new LeveDataforTable(1636, 0),
            new LeveDataforTable(1635, 0),
            new LeveDataforTable(1634, 0),
            new LeveDataforTable(1633, 0),
            new LeveDataforTable(1632, 0),
            new LeveDataforTable(1631, 0),
            new LeveDataforTable(1630, 0),
            new LeveDataforTable(1629, 0),
            new LeveDataforTable(1628, 0),

            // CUL
            new LeveDataforTable(1647, 0),
            new LeveDataforTable(1646, 0),
            new LeveDataforTable(1645, 0),
            new LeveDataforTable(1644, 0),
            new LeveDataforTable(1643, 0),
            new LeveDataforTable(1642, 0),
            new LeveDataforTable(1641, 0),
            new LeveDataforTable(1640, 0),
            new LeveDataforTable(1639, 0),
            new LeveDataforTable(1638, 0),

            #endregion

            #region Lv. 70-78

            // CRP
            new LeveDataforTable(1417, 0),
            new LeveDataforTable(1416, 0),
            new LeveDataforTable(1415, 0),
            new LeveDataforTable(1414, 0),
            new LeveDataforTable(1413, 0),
            new LeveDataforTable(1412, 0),
            new LeveDataforTable(1411, 0),
            new LeveDataforTable(1410, 0),
            new LeveDataforTable(1409, 0),
            new LeveDataforTable(1408, 0),
            new LeveDataforTable(1407, 0),
            new LeveDataforTable(1406, 0),
            new LeveDataforTable(1405, 0),
            new LeveDataforTable(1404, 0),
            new LeveDataforTable(1403, 0),

            // BSM
            new LeveDataforTable(1447, 0),
            new LeveDataforTable(1446, 0),
            new LeveDataforTable(1445, 0),
            new LeveDataforTable(1444, 0),
            new LeveDataforTable(1443, 0),
            new LeveDataforTable(1442, 0),
            new LeveDataforTable(1441, 0),
            new LeveDataforTable(1440, 0),
            new LeveDataforTable(1439, 0),
            new LeveDataforTable(1438, 0),
            new LeveDataforTable(1437, 0),
            new LeveDataforTable(1436, 0),
            new LeveDataforTable(1435, 0),
            new LeveDataforTable(1434, 0),
            new LeveDataforTable(1433, 0),

            // ARM
            new LeveDataforTable(1462, 0),
            new LeveDataforTable(1461, 0),
            new LeveDataforTable(1460, 0),
            new LeveDataforTable(1459, 0),
            new LeveDataforTable(1458, 0),
            new LeveDataforTable(1457, 0),
            new LeveDataforTable(1456, 0),
            new LeveDataforTable(1455, 0),
            new LeveDataforTable(1454, 0),
            new LeveDataforTable(1453, 0),
            new LeveDataforTable(1452, 0),
            new LeveDataforTable(1451, 0),
            new LeveDataforTable(1450, 0),
            new LeveDataforTable(1449, 0),
            new LeveDataforTable(1448, 0),

            // GSM

            // LTW
            new LeveDataforTable(1432, 0),
            new LeveDataforTable(1431, 0),
            new LeveDataforTable(1430, 0),
            new LeveDataforTable(1429, 0),
            new LeveDataforTable(1428, 0),
            new LeveDataforTable(1427, 0),
            new LeveDataforTable(1426, 0),
            new LeveDataforTable(1425, 0),
            new LeveDataforTable(1424, 0),
            new LeveDataforTable(1423, 0),
            new LeveDataforTable(1422, 0),
            new LeveDataforTable(1421, 0),
            new LeveDataforTable(1420, 0),
            new LeveDataforTable(1419, 0),
            new LeveDataforTable(1418, 0),

            // WVR
            new LeveDataforTable(1507, 0),
            new LeveDataforTable(1506, 0),
            new LeveDataforTable(1505, 0),
            new LeveDataforTable(1504, 0),
            new LeveDataforTable(1503, 0),
            new LeveDataforTable(1502, 0),
            new LeveDataforTable(1501, 0),
            new LeveDataforTable(1500, 0),
            new LeveDataforTable(1499, 0),
            new LeveDataforTable(1498, 0),
            new LeveDataforTable(1497, 0),
            new LeveDataforTable(1496, 0),
            new LeveDataforTable(1495, 0),
            new LeveDataforTable(1494, 0),
            new LeveDataforTable(1493, 0),

            // ALC
            new LeveDataforTable(1492, 0),
            new LeveDataforTable(1491, 0),
            new LeveDataforTable(1490, 0),
            new LeveDataforTable(1489, 0),
            new LeveDataforTable(1488, 0),
            new LeveDataforTable(1487, 0),
            new LeveDataforTable(1486, 0),
            new LeveDataforTable(1485, 0),
            new LeveDataforTable(1484, 0),
            new LeveDataforTable(1483, 0),
            new LeveDataforTable(1482, 0),
            new LeveDataforTable(1481, 0),
            new LeveDataforTable(1480, 0),
            new LeveDataforTable(1479, 0),
            new LeveDataforTable(1478, 0),

            // CUL

            #endregion

            #region Lv. 60-68

            // CRP

            // BSM

            // ARM

            // GSM

            // LTW

            // WVR

            // ALC

            // CUL

            #endregion

            #region Lv. 50-58

            // CRP

            // BSM

            // ARM

            // GSM

            // LTW

            // WVR

            // ALC

            // CUL

            #endregion
        ];
    }

    public class LeveDataDict
    {
        // Universal information across the leves
        /// <summary>
        /// Job ID Attached to the leve
        /// </summary>
        public uint JobID { get; set; } 
        /// <summary>
        /// Name of the Leve
        /// </summary>
        public string LeveName { get; set; }
        /// <summary>
        /// Amount of times you want to do the leve. Good place to keep (in the dictionary... hopefully)
        /// </summary>
        public int Amount { get; set; }
        /// <summary>
        /// DataID (Row #17) aka QuestID of it. Leads to CraftLeves for crafters. Not sure about the rest though...
        /// </summary>
        public uint QuestID { get; set; }
        /// <summary>
        /// Starting city that the leve is in (good for specifics)
        /// </summary>
        public uint StartingCity { get; set; }
        /// <summary>
        /// Zone name that the leve starts in, (Gridania, Lower La Noscea... as examples
        /// </summary>
        public string ZoneName { get; set; }

        // Crafter Specific
        /// <summary>
        /// Crafter, ItemID for the turnin item
        /// </summary>
        public uint ItemID { get; set; }
        /// <summary>
        /// Crafter, Item Name itself for the Turnin
        /// </summary>
        public string ItemName { get; set; }
        /// <summary>
        /// Crafter, Item Icon itself, not really useful? Moreso visual flare for me
        /// </summary>
        public ISharedImmediateTexture? ItemIcon { get; set; }
        /// <summary>
        /// Crafter, amount of times you can do this turnin per leve. 
        /// Moreso for backend, might be able to show it somehow though...
        /// </summary>
        public int RepeatAmount { get; set; }
        /// <summary>
        /// Crafter, amount of items overall that is necessary for this leve for the full potentional
        /// </summary>
        public int TurninAmount { get; set; }
        /// <summary>
        /// Crafter, current amount of items that you have in your inventory. 
        /// Updates currently every second. 
        /// </summary>
        public int CurrentItemAmount { get; set; }
    }

    public class LeveType
    {
        /// <summary>
        /// Job icon that is grabbed from the sheet, just an easier way to pull it.
        /// </summary>
        public ISharedImmediateTexture? AssignmentIcon { get; set; }
        /// <summary>
        /// Type of leve that is assigned to this. Quick and easy way to access this.
        /// </summary>
        public string LeveClassType { get; set; }
    }

    public static Dictionary<uint, LeveDataDict> LeveDict = new();

    public static Dictionary<uint, LeveType> LeveTypeDict = new();
}
