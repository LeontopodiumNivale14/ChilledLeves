namespace ChilledLeves.Utilities.OldUtils;

public static class Old_Data
{
    // MSQ Quest ID's
    public static readonly uint LevesofWineport = 65550;
    public static readonly uint LevesofCampBluefog = 65551;
    public static readonly uint LevesoftheObservatorium = 65552;
    public static readonly uint LevesofWhitbrim = 65553;
    public static readonly uint LevesofSaintCoinachsFind = 65554;
    public static readonly uint LevesofHawthorne = 65757;
    public static readonly uint LevesofQuarrymill = 65979;
    public static readonly uint LevesofCampTranquil = 65980;
    public static readonly uint LevesofCampDrybone = 66224;
    public static readonly uint LevesofLittleAlaMhigo = 66228;
    public static readonly uint LevesofAleport = 66230;
    public static readonly uint LevesofMoraby = 66231;
    public static readonly uint LevesofCostadelSol = 66232;

    public static readonly uint SimplyTheHestLimsa = 65595;
    public static readonly uint SimplyTheHestGridania = 65596;
    public static readonly uint SimplyTheHestUldah = 65594;

    public static uint ComingtoIshgard = 67116; // Heavensword
    public static uint NotWithoutIncident = 68005; // Stormblood
    public static uint CityoftheFirst = 68816; // Shadowbringers
    public static uint TheNextShiptoSail = 69893; // Endwalker
    public static uint ANewWorldtoExplore = 70396; // Endwalker

    // The 3 possible starting leves:
    public static bool LevesofBentbranch => IsMSQComplete(65756); // Gridania startpoint
    public static bool LevesofHorizon => IsMSQComplete(66223); // Ul'Dah startpoint
    public static bool LevesofSwiftperch => IsMSQComplete(66229); // Limsa startpoint

    public static bool StartMSQCheck()
    {
        bool LimsaLeveNPC = IsMSQComplete(66005); // Just Deserts
        bool UlDahLeveNPC = IsMSQComplete(65856); // Way down in the hole
        bool GridaniaNPC = IsMSQComplete(65665); // Spirithold Broken

        if (LimsaLeveNPC)
            return LevesofSwiftperch;
        else if (UlDahLeveNPC)
            return LevesofHorizon;
        else if (GridaniaNPC)
            return LevesofBentbranch;
        else
            return false;
    }

    public static bool GuildHestCheck()
    {
        if (LevesofSwiftperch)
            return IsMSQComplete(SimplyTheHestLimsa);
        else if (LevesofHorizon)
            return IsMSQComplete(SimplyTheHestUldah);
        else if (LevesofBentbranch)
            return IsMSQComplete(SimplyTheHestGridania);
        else
            return false;
    }

    public class LeveEntry
    {
        public uint LeveID { get; set; }
        public int InputValue { get; set; }
    }
}
