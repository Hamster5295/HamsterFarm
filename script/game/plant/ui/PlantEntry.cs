using Godot;

public partial class PlantEntry : PanelContainer
{
    private PlantConfig config;

    private TextureRect icon;
    private Label name, growTime, cost, earn;

    public override void _Ready()
    {
        icon = GetNode<TextureRect>("VBox/Icon");
        name = GetNode<Label>("VBox/Name");
        growTime = GetNode<Label>("VBox/GrowTime");
        cost = GetNode<Label>("VBox/Cost");
        earn = GetNode<Label>("VBox/Earn");
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton btn)
        {
            if (!btn.Pressed && btn.ButtonIndex == MouseButton.Left)
            {
                Server.PlantManager.Plant(config);
                Server.PlantManager.Menu.Disappear();
            }
        }
    }

    public void Init(PlantConfig config)
    {
        this.config = config;
        icon.Texture = config.Icon;
        name.Text = config.PlantName;
        growTime.Text = $"生长时间: {config.GrowTime}s";
        cost.Text = $"价格: {config.Cost}$";
        earn.Text = $"出售: {config.Earn}$";
    }
}