using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorersIcebox.Ui.MainWindow;

internal static class VersionNotesUi
{
    private static string[] VersionOptions = { "1.0.3.2", "1.0.3.1", "1.0.2.1", "1.0.2", "1.0.1", "1.0.0" };
    public static string VersionSelected = "1.0.3.2"; // Currently selected option

    internal static void Draw()
    {
        ImGui.NewLine();

        if (ImGui.BeginCombo("Version Notes", VersionSelected))
        {
            foreach (var voption in VersionOptions)
            {
                // If this option is selected
                if (ImGui.Selectable(voption, voption == VersionSelected))
                {
                    VersionSelected = voption;
                }

                // Set focus to the selected item for better UX
                if (voption == VersionSelected)
                {
                    ImGui.SetItemDefaultFocus();
                }
            }
            ImGui.EndCombo();
        }

        // Render for all the version notes:
        switch (VersionSelected)
        {
            case "1.0.3.2":
                ImGui.TextWrapped($"V1.0.3.2\r\n" +
                                  $"→ Updated the Copper Route, got stuck flying trying to do down to one spot\r\n" +
                                  $"→ Fixed the Gathering Mode Ui, it now all has independant sliders that are checked instead of all using the same one (oversight on my part initally)\r\n" +
                                  $"→ Also fixed the \"About\" page. Try to add one link and accidentally copied a push style");
                break;

            case "1.0.3.1":
                ImGui.TextWrapped($"V1.0.3.1\n" +
                                  $"I forgot to include the cotton route update. WOOPS. Fixed now.\n" +
                                  $"Also made a link to the wiki page where I use all my routes/update the ones that I use.");

                ImGui.TextWrapped($"V1.0.3.0\n" +
                                  $"Shopping Update\n" +
                                  $"→ Now you can select select how many items that you want to gather from a route!\n" +
                                  $"→ Slider now only go to the amount that is possible in this mode (so you can't say you want to gather more than you can)\n" +
                                  $"→ Complete Ui Overhaul (Made them all into tables to easily line up all the information\n" +
                                  $"→ Fixed some mount issue routes (Route 4, 6, and 9 in particular)\n");
                break;
            case "1.0.2.1":
                ImGui.TextWrapped($"V1.0.2.1\n" +
                                  $"Small update\n" +
                                  $"→ Fixed it to where if you load switch to XP Grind Mode, it'll properly default to one of those routes\n" +
                                  $"→ Also added a delay check for making sure the item amount changes (should help w/ lag/bad ping?\n");
                                  break;
            case "1.0.2":
                ImGui.TextWrapped($"V1.0.2\n" +
                                  $"→ A LOT... of backend changed. Cleaned up quite a bit of code in the process. (Thank you Croizat for the tips)\n" +
                                  $"→ increased the time delay on the shop. Had it firing off to quickly and might of caused some un-necessary hanging...\n" +
                                  $"→ Next on the list is to make a shopping list (or moreso \"Gather this much items\" version of gathering.)\n" +
                                  $"→ Also add some more checks to routes so you can't accidentally activate certain ones before you unlock the pathways to them\n" +
                                  $"  → This is moreso in reference to the mountain pass underneath with the coal+ items, since that's locked behind atleast lv. 15?/questline\n" +
                                  $"→ cleaned up the version menu to something I'm finally happy with. Think this will be the formatting going foward\n");
                                  break;
            case "1.0.1":
                ImGui.TextWrapped($"V1.0.1\n" +
                                  $"First update within 24 hours? Still so much to do... \n" +
                                  $"→ Actually got the level requirement down to lv 4! Now you can do it from the earliest point.\n" +
                                  $"→ Made the routes more modular in accepting workshop amount from multiple item types (So Iron + Durium sand now dynamically updates properly\n" +
                                  $"→ Fixed the cabin 4 not pathing properly (Hopefully)... tested it against the rest of the cabins as well and didn't seem to have problems anymore\n" +
                                  $"→ Removed the \"All Item's Unlocked\" checkbox, partially due to it being redundant. But also it being true when you first enable the plugin probably wasn't the best idea...");
                                  break;
            case "1.0.0":
                ImGui.TextWrapped($"V1.0.0\n" +
                                  $"First actual... release. Holy fuck\n" +
                                  $"Initial creation of plugin, includes:)\n" +
                                  $"→ 2 Leveling/Grind Routes\n" +
                                  $"  → Ground [Clay/Sand\n" +
                                  $"  → Flying [Quartz]\n" +
                                  $"→ Island Gathering Mode\n" +
                                  $"Being able to select which routes to run + search through them for items\n" +
                                  $"For both: Set workshop values to keep a certain amount of items, allowing for dynamic loop amounts");
                ImGui.NewLine();
                ImGui.TextWrapped("Safety check to make sure you have the shovel unlocked. Will remove this whenever I update routes to be more dynamic\n" +
                                  "This has been completed in V1.0.1. WOO!");
                                  break;
        }
    }
}
