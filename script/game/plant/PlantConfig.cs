using Godot;

[GlobalClass]
public partial class PlantConfig : Resource
{
    [Export] private string plantName;
    [Export] private Texture2D icon;
    [Export] private float growTime;
    [Export] private int cost, earn;
    [ExportGroup("生长周期")]
    [Export] private Texture2D seed, plant, flower, fruit;

    public string PlantName => plantName;
    public Texture2D Icon => icon;
    public float GrowTime => growTime;
    public int Cost => cost;
    public int Earn => earn;
    public Texture2D Seed => seed;
    public Texture2D Plant => plant;
    public Texture2D Flower => flower;
    public Texture2D Fruit => fruit;
}