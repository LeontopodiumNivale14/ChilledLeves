namespace ExplorersIcebox.Ui;

internal class SettingWindow : Window
{
    public new static readonly string WindowName = "Islanc Sanc Workshop Amounts";
    public SettingWindow() :
        base(WindowName)
    {
        Flags = ImGuiWindowFlags.NoCollapse;
        ImGui.SetNextWindowSize(new Vector2(450, 0), ImGuiCond.Always);
        P.windowSystem.AddWindow(this);
    }

    public void Dispose() { }

    // dropdown options
    private string[] options = { "All Items [By Slot]", "Route 1: Islefish | Clam", "Route 2: Islewort", "Route 3: Sugarcane | Vine", "Route 4: Tinsand | Sand",
    "Route 5: Apple | Beehive | Vine", "Route 6: Coconut | Palm Log | Palm leaf", "Route 7: Cotton", "Route 8: Clay | Sand [Ground XP Loop]",
    "Route 19: Quartz | Stone [Flying XP Loop]"};


    public static string currentOption = "All Items"; // Currently selected option

    public override void Draw()
    {
        // Display dropdown menu
        if (ImGui.BeginCombo("##Dropdown", currentOption))
        {
            foreach (var option in options)
            {
                // If this option is selected
                if (ImGui.Selectable(option, option == currentOption))
                {
                    currentOption = option;
                }

                // Set focus to the selected item for better UX
                if (option == currentOption)
                {
                    ImGui.SetItemDefaultFocus();
                }
            }
            ImGui.EndCombo();
        }

        // Render content based on selected option
        switch (currentOption)
        {
            case "All Items [By Slot]":
                RenderAllItems();
                break;
            case "Route 1: Islefish | Clam":
                Route0WorkshopGui();
                break;
            case "Route 2: Islewort":
                Route1WorkshopGui();
                break;
            case "Route 3: Sugarcane | Vine":
                Route2WorkshopGui();
                break;
            case "Route 4: Tinsand | Sand":
                Route3WorkshopGui();
                break;
            case "Route 5: Apple | Beehive | Vine":
                Route4WorkshopGui();
                break;
            case "Route 6: Coconut | Palm Log | Palm leaf":
                Route5WorkshopGui();
                break;
            case "Route 7: Cotton":
                Route6WorkshopGui();
                break;
            case "Route 8: Clay | Sand [Ground XP Loop]":
                Route7WorkshopGui();
                break;
            case "Route 19: Quartz | Stone [Flying XP Loop]":
                Route18WorkshopGui();
                break;
        }
    }

    private void RenderAllItems()
    {
        ImGui.Text("Displaying all items.");
        // Add code to display "All Items" content
    }
}

