using ECommons.Configuration;
namespace ChilledLeves.Config_Files;

public partial class Config
{
    public int ConfigVersion { get; set; } = 1;

    public string RunUntilSelected = "Lv.";

    public void Save()
    {
        EzConfig.Save();
    }
}