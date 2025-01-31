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

    public static List<uint> CraftingClass = new List<uint>();
    public static List<bool> CraftingClassActive = new List<bool>();

    public static List<string> AllLocations = new List<string>();
    public static List<bool> LocationsActive = new List<bool>();

    public static List<uint> LeveNumber = new List<uint>();
    public static List<int> LeveAmount = new List<int>();


    public class LeveData
    {
        public uint LeveID { get; set; }
        public int Amount { get; set; }

        public LeveData(uint leveID, int amount)
        {
            LeveID = leveID;
            Amount = amount;
        }
    }

    public static class Leves
    {
        // Tuli (1185)
        public static List<LeveData> CrafterLeves = 
        [

            #region Lv. 90-98

            // CRP
            new LeveData(1697, 0),
            new LeveData(1696, 0),
            new LeveData(1695, 0),
            new LeveData(1694, 0),
            new LeveData(1693, 0),
            new LeveData(1692, 0),
            new LeveData(1691, 0),
            new LeveData(1690, 0),
            new LeveData(1689, 0),
            new LeveData(1688, 0),

            // BSM
            new LeveData(1707, 0),
            new LeveData(1706, 0),
            new LeveData(1705, 0),
            new LeveData(1704, 0),
            new LeveData(1703, 0),
            new LeveData(1702, 0),
            new LeveData(1701, 0),
            new LeveData(1700, 0),
            new LeveData(1699, 0),
            new LeveData(1698, 0),

            // ARM
            new LeveData(1717, 0),
            new LeveData(1716, 0),
            new LeveData(1715, 0),
            new LeveData(1714, 0),
            new LeveData(1713, 0),
            new LeveData(1712, 0),
            new LeveData(1711, 0),
            new LeveData(1710, 0),
            new LeveData(1709, 0),
            new LeveData(1708, 0),

            // GSM
            new LeveData(1727, 0),
            new LeveData(1726, 0),
            new LeveData(1725, 0),
            new LeveData(1724, 0),
            new LeveData(1723, 0),
            new LeveData(1722, 0),
            new LeveData(1721, 0),
            new LeveData(1720, 0),
            new LeveData(1719, 0),
            new LeveData(1718, 0),

            // LTW
            new LeveData(1737, 0),
            new LeveData(1736, 0),
            new LeveData(1735, 0),
            new LeveData(1734, 0),
            new LeveData(1733, 0),
            new LeveData(1732, 0),
            new LeveData(1731, 0),
            new LeveData(1730, 0),
            new LeveData(1729, 0),
            new LeveData(1728, 0),

            // WVR
            new LeveData(1747, 0),
            new LeveData(1746, 0),
            new LeveData(1745, 0),
            new LeveData(1744, 0),
            new LeveData(1743, 0),
            new LeveData(1742, 0),
            new LeveData(1741, 0),
            new LeveData(1740, 0),
            new LeveData(1739, 0),
            new LeveData(1738, 0),

            // ALC
            new LeveData(1757, 0),
            new LeveData(1756, 0),
            new LeveData(1755, 0),
            new LeveData(1754, 0),
            new LeveData(1753, 0),
            new LeveData(1752, 0),
            new LeveData(1751, 0),
            new LeveData(1750, 0),
            new LeveData(1749, 0),
            new LeveData(1748, 0),

            // CUL
            new LeveData(1767, 0),
            new LeveData(1766, 0),
            new LeveData(1765, 0),
            new LeveData(1764, 0),
            new LeveData(1763, 0),
            new LeveData(1762, 0),
            new LeveData(1761, 0),
            new LeveData(1760, 0),
            new LeveData(1759, 0),
            new LeveData(1758, 0),

            #endregion

            #region Lv. 80-88

            // CRP
            new LeveData(1577, 0),
            new LeveData(1576, 0),
            new LeveData(1575, 0),
            new LeveData(1574, 0),
            new LeveData(1573, 0),
            new LeveData(1572, 0),
            new LeveData(1571, 0),
            new LeveData(1570, 0),
            new LeveData(1569, 0),
            new LeveData(1568, 0),

            // BSM
            new LeveData(1587, 0),
            new LeveData(1586, 0),
            new LeveData(1585, 0),
            new LeveData(1584, 0),
            new LeveData(1583, 0),
            new LeveData(1582, 0),
            new LeveData(1581, 0),
            new LeveData(1580, 0),
            new LeveData(1579, 0),
            new LeveData(1578, 0),

            // ARM
            new LeveData(1597, 0),
            new LeveData(1596, 0),
            new LeveData(1595, 0),
            new LeveData(1594, 0),
            new LeveData(1593, 0),
            new LeveData(1592, 0),
            new LeveData(1591, 0),
            new LeveData(1590, 0),
            new LeveData(1589, 0),
            new LeveData(1588, 0),

            // GSM
            new LeveData(1607, 0),
            new LeveData(1606, 0),
            new LeveData(1605, 0),
            new LeveData(1604, 0),
            new LeveData(1603, 0),
            new LeveData(1602, 0),
            new LeveData(1601, 0),
            new LeveData(1600, 0),
            new LeveData(1599, 0),
            new LeveData(1598, 0),

            // LTW
            new LeveData(1617, 0),
            new LeveData(1616, 0),
            new LeveData(1615, 0),
            new LeveData(1614, 0),
            new LeveData(1613, 0),
            new LeveData(1612, 0),
            new LeveData(1611, 0),
            new LeveData(1610, 0),
            new LeveData(1609, 0),
            new LeveData(1608, 0),

            // WVR
            new LeveData(1627, 0),
            new LeveData(1626, 0),
            new LeveData(1625, 0),
            new LeveData(1624, 0),
            new LeveData(1623, 0),
            new LeveData(1622, 0),
            new LeveData(1621, 0),
            new LeveData(1620, 0),
            new LeveData(1619, 0),
            new LeveData(1618, 0),

            // ALC
            new LeveData(1637, 0),
            new LeveData(1636, 0),
            new LeveData(1635, 0),
            new LeveData(1634, 0),
            new LeveData(1633, 0),
            new LeveData(1632, 0),
            new LeveData(1631, 0),
            new LeveData(1630, 0),
            new LeveData(1629, 0),
            new LeveData(1628, 0),

            // CUL
            new LeveData(1647, 0),
            new LeveData(1646, 0),
            new LeveData(1645, 0),
            new LeveData(1644, 0),
            new LeveData(1643, 0),
            new LeveData(1642, 0),
            new LeveData(1641, 0),
            new LeveData(1640, 0),
            new LeveData(1639, 0),
            new LeveData(1638, 0),

            #endregion

            #region Lv. 70-78

            // CRP
            new LeveData(1417, 0),
            new LeveData(1416, 0),
            new LeveData(1415, 0),
            new LeveData(1414, 0),
            new LeveData(1413, 0),
            new LeveData(1412, 0),
            new LeveData(1411, 0),
            new LeveData(1410, 0),
            new LeveData(1409, 0),
            new LeveData(1408, 0),
            new LeveData(1407, 0),
            new LeveData(1406, 0),
            new LeveData(1405, 0),
            new LeveData(1404, 0),
            new LeveData(1403, 0),

            // BSM
            new LeveData(1447, 0),
            new LeveData(1446, 0),
            new LeveData(1445, 0),
            new LeveData(1444, 0),
            new LeveData(1443, 0),
            new LeveData(1442, 0),
            new LeveData(1441, 0),
            new LeveData(1440, 0),
            new LeveData(1439, 0),
            new LeveData(1438, 0),
            new LeveData(1437, 0),
            new LeveData(1436, 0),
            new LeveData(1435, 0),
            new LeveData(1434, 0),
            new LeveData(1433, 0),

            // ARM
            new LeveData(1462, 0),
            new LeveData(1461, 0),
            new LeveData(1460, 0),
            new LeveData(1459, 0),
            new LeveData(1458, 0),
            new LeveData(1457, 0),
            new LeveData(1456, 0),
            new LeveData(1455, 0),
            new LeveData(1454, 0),
            new LeveData(1453, 0),
            new LeveData(1452, 0),
            new LeveData(1451, 0),
            new LeveData(1450, 0),
            new LeveData(1449, 0),
            new LeveData(1448, 0),

            // GSM

            // LTW
            new LeveData(1432, 0),
            new LeveData(1431, 0),
            new LeveData(1430, 0),
            new LeveData(1429, 0),
            new LeveData(1428, 0),
            new LeveData(1427, 0),
            new LeveData(1426, 0),
            new LeveData(1425, 0),
            new LeveData(1424, 0),
            new LeveData(1423, 0),
            new LeveData(1422, 0),
            new LeveData(1421, 0),
            new LeveData(1420, 0),
            new LeveData(1419, 0),
            new LeveData(1418, 0),

            // WVR
            new LeveData(1507, 0),
            new LeveData(1506, 0),
            new LeveData(1505, 0),
            new LeveData(1504, 0),
            new LeveData(1503, 0),
            new LeveData(1502, 0),
            new LeveData(1501, 0),
            new LeveData(1500, 0),
            new LeveData(1499, 0),
            new LeveData(1498, 0),
            new LeveData(1497, 0),
            new LeveData(1496, 0),
            new LeveData(1495, 0),
            new LeveData(1494, 0),
            new LeveData(1493, 0),

            // ALC
            new LeveData(1492, 0),
            new LeveData(1491, 0),
            new LeveData(1490, 0),
            new LeveData(1489, 0),
            new LeveData(1488, 0),
            new LeveData(1487, 0),
            new LeveData(1486, 0),
            new LeveData(1485, 0),
            new LeveData(1484, 0),
            new LeveData(1483, 0),
            new LeveData(1482, 0),
            new LeveData(1481, 0),
            new LeveData(1480, 0),
            new LeveData(1479, 0),
            new LeveData(1478, 0),

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


}
