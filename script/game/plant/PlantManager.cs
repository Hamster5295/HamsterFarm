using System.Linq;
using Godot;
using Godot.Collections;

public partial class PlantManager : Node
{
    public const float CELL_SIZE = 128;
    public static System.Collections.Generic.List<PlantConfig> Configs { get; private set; }

    public PlantMenu Menu => plantMenu;

    [Export] private Array<PlantConfig> configs;
    [Export] private PlantMenu plantMenu;
    [Export] private PackedScene plant;

    private Dictionary<Vector2, PlantStat> plants;
    private Vector2 plantPos;

    private static int counter = 0;

    public override void _EnterTree()
    {
        Configs = new(configs);
    }

    public override void _Process(double delta)
    {
        if (!Multiplayer.IsServer()) return;
        foreach (var item in plants)
        {
            item.Value.UpdateState();
        }
    }

    public void Init()
    {
        plants = new();
    }

    public void RequirePlant(Vector2 globalPos)
    {
        plantPos = GetCell(globalPos);
        RpcId(1, nameof(InputPlant), plantPos);
    }

    public void Plant(PlantConfig config)
    {
        RpcId(1, nameof(InputPlantAt), plantPos, Configs.IndexOf(config));
    }

    public void InitPlant(long id)
    {
        foreach (var item in plants)
        {
            RpcId(id, nameof(PlantAt), item.Key, Configs.IndexOf(item.Value.Config), item.Value.Plant.Name);
            item.Value.UpdateState(id);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void InputPlant(Vector2 cellPos)
    {
        var id = Multiplayer.GetRemoteSenderId();
        if (plants.ContainsKey(cellPos))
        {
            var target = plants[cellPos];
            if (target.IsRape())
            {
                target.Plant.Rape();
                Server.SetMoney(id, Server.GetMoney(id) + target.Config.Earn);
                Toast.MakeText(id, $"+{target.Config.Earn}$", Colors.Yellow);
                plants.Remove(cellPos);
            }
            else
            {
                Toast.MakeText(id, "作物尚未成熟！", Colors.Red);
            }
        }
        else RpcId(id, nameof(ShowPlantMenu), Server.GetMoney(id));
    }

    [Rpc]
    private void ShowPlantMenu(int money)
    {
        plantMenu.Appear(money);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void InputPlantAt(Vector2 cellPos, int configIdx)
    {
        if (plants.ContainsKey(cellPos)) return;
        var config = Configs[configIdx];

        var id = Multiplayer.GetRemoteSenderId();
        var money = Server.GetMoney(id);
        if (money >= config.Cost)
        {
            Server.SetMoney(id, money - config.Cost);
            Toast.MakeText(id, $"-{config.Cost}$", Colors.Yellow);
            var plantName = $"Plant_{counter++}";
            plants.Add(cellPos, new(config, Time.GetTicksMsec(), PlantAt(cellPos, configIdx, plantName)));
            Rpc(nameof(PlantAt), cellPos, configIdx, plantName);
        }
        else Toast.MakeText(id, "金钱不足!", Colors.Red);
    }

    [Rpc]
    private Plant PlantAt(Vector2 cellPos, int configIdx, string name)
    {
        var p = plant.Instantiate<Plant>();
        Server.Background.AddChild(p);
        p.Name = name;
        p.Position = GetCellCenter(cellPos);
        p.Init(Configs[configIdx]);
        return p;
    }

    public Vector2 GetCell(Vector2 worldPos)
        => (worldPos / CELL_SIZE).Floor();

    public Vector2 GetCellCenter(Vector2 cellPos)
     => cellPos * CELL_SIZE + CELL_SIZE * Vector2.One / 2;
}