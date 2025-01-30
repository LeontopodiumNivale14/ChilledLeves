using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorersIcebox.Ui.MainWindow;

internal static unsafe class HelpUi
{
    internal static void Draw()
    {
        ImGui.Text("XP | Cowries Grind");
        ImGui.Text("The purpose of this mode is one of two reasons.");
        ImGui.TextWrapped("→ Reason 1: A quick and surefire way to level up your characters. This uses what Overseas Casuals (aka Island Sanctuary Discord) consideres/have figured to be the best XP Routes.");
        ImGui.TextWrapped("→ Reason 2: if you want to get green Cowries for things, this is also great! Personally use this to refill my cordial stash");
        ImGui.TextWrapped("→ Currently, you need to be alteast lv. 5 (to unlock the shovel) to be able to use the leveling mode. I'm working on making it work at Lv. 4, it's going to take a little bit");
        ImGui.Text("Island Gathering Mode");
        ImGui.TextWrapped("Here, you can select all the routes that you would like to max out on items on.");
        ImGui.TextWrapped("You can pick and choose which routes to enable/disable quickly, and also input the amount of workshop items you want to keep of that kind");
        ImGui.TextWrapped("This will automatically update how many routes you can do in that particular item, so it SHOULDN'T get locked");
        ImGui.TextWrapped("Search bar is also there if you want to quickly look up a particular item to farm.");
        ImGui.NewLine();
        ImGui.EndTabItem();
    }
}
