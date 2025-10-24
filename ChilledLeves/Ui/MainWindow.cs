using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilledLeves.Ui
{
    internal class MainWindow : Window
    {
        public MainWindow() :
#if DEBUG
        base($"ChilledLeves | Debug Build {P.GetType().Assembly.GetName().Version} ##ChilledLevesMainWindow")
#else
        base($"ChilledLeves {P.GetType().Assembly.GetName().Version} ##ChilledLevesMainWindow");
#endif
        {
            Flags = ImGuiWindowFlags.None;
            SizeConstraints = new()
            {
                MinimumSize = new Vector2(1050, 650)
            };
            P.windowSystem.AddWindow(this);
        }

        public void Dispose()
        {
            P.windowSystem.RemoveWindow(this);
        }

        public override void Draw()
        {
            
        }

    }
}
