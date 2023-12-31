using Godot;
using System.Linq;
using System.Collections.Generic;

public partial class PlantMenu : ColorRect
{
    [Export] private PackedScene plantEntry;

    private GridContainer entryParent;
    private Label moneyText;

    private List<PlantConfig> plants;

    public override void _Ready()
    {
        entryParent = GetNode<GridContainer>("Scroll/PlantList");
        moneyText = GetNode<Label>("Money");

        PlantManager.Configs.ForEach(i =>
        {
            var entry = plantEntry.Instantiate<PlantEntry>();
            entryParent.AddChild(entry);
            entry.Init(i);
        });

        Disappear();
    }

    public void Appear(int money)
    {
        Visible = true;
        Player.InputLock = true;

        moneyText.Text = money + "";
    }

    public void Disappear()
    {
        Visible = false;
        Player.InputLock = false;
    }
}