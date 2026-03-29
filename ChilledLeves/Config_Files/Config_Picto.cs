using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Config_Files;

public partial class Config
{
    public Vector4 Picto_GatherFanColor { get; set; } = new Vector4(0f, 1f, 0.003921569f, 0.39215687f);
    public Vector4 Picto_GatherBase { get; set; } = new Vector4(0f, 1f, 0f, 1f);
    public Vector4 Picto_SelectedGatherBase { get; set; } = new Vector4(0f, 1f, 0f, 1f);
    public Vector4 Picto_FlightFanColor { get; set; } = new Vector4(1f, 0f, 0.9529412f, 0.39215687f);
    public Vector4 Picto_SelectedFan { get; set; } = new Vector4(0f, 0f, 1f, 0.39215687f);
    public Vector4 Picto_TextColor { get; set; } = new Vector4(1f, 1f, 1f, 1f);
}
