using System.Text.Json.Serialization;
using ECommons.Configuration;

namespace ChilledLeves;

public class Config : IEzConfig
{
    [JsonIgnore]
    public const int CurrentConfigVersion = 1;

    public bool UiItem = true;
    public bool UiNeededCurrent = true;
    public bool UiLocation = true;
    public bool UiGil = true;

    //

    public void Save()
    {
        EzConfig.Save();
    }
}
