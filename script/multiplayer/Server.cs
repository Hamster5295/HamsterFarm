using Godot;
using HamsterUtils;
using System.Collections.Generic;
using System.Net;
using System.Linq;

public partial class Server : Node
{
    public const string USERNAME_KEY = "Username";

    public static Sprite2D Background => instance.background;
    public static PlantManager PlantManager => instance.plantManager;
    public static Player Player { get; private set; }
    private static Server instance;

    [Export(PropertyHint.File)] private string menuPath;
    [Export] private PackedScene player;

    private FollowCamera cam;
    private Sprite2D background;
    private PlantManager plantManager;

    private Dictionary<long, PlayerStat> clientMap;
    private Dictionary<long, Node2D> clientObjMap = new();

    public override void _Ready()
    {
        cam = GetNode<FollowCamera>("../Cam");
        background = GetNode<Sprite2D>("../Background");
        plantManager = GetNode<PlantManager>("PlantManager");

        if (Multiplayer.IsServer())
        {
            Multiplayer.PeerConnected += OnPeerConnected;
            Multiplayer.PeerDisconnected += OnPeerDisconnected;

            clientMap = new();
            plantManager.Init();

            Chatbox.SendMsg($"服务器地址: {Dns.GetHostAddresses(Dns.GetHostName()).First(i => i.ToString().Contains('.'))}");
            Log.Info("Server", "服务器已就绪");
        }
        else
        {
            Multiplayer.ServerDisconnected += OnServerDisconnected;
        }
        instance = this;
    }

    [Rpc]
    private void RequireRegister()
    {
        RpcId(1, nameof(RegisterClient), CommonData.Get(USERNAME_KEY, ""));
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RegisterClient(string name)
    {
        var id = Multiplayer.GetRemoteSenderId();
        clientMap.Add(id, new() { UserName = name });
        Log.Info("Server", $"Client {id} 被注册为 {name}");

        Rpc(nameof(CreatePlayer), id, name, Vector2.Zero);
        foreach (var item in clientMap)
            if (item.Key != id)
                RpcId(id, nameof(CreatePlayer), item.Key, item.Value.UserName, clientObjMap[item.Key].Position);

        Chatbox.InitMsg(id);
        Chatbox.SendMsg($"{name} 加入游戏");

        plantManager.InitPlant(id);
    }

    [Rpc(CallLocal = true)]
    private void CreatePlayer(long id, string userName, Vector2 pos)
    {
        var pl = player.Instantiate<Player>();
        background.AddChild(pl);
        pl.Init(id, userName, pos);
        clientObjMap.Add(id, pl);

        if (id == Multiplayer.GetUniqueId())
        {
            Player = pl;
            cam.Target = pl;
        }
    }

    [Rpc(CallLocal = true)]
    private void DestroyPlayer(long id)
    {
        clientObjMap[id].QueueFree();
        clientObjMap.Remove(id);
    }

    public static PlayerStat GetPlayerStat(long id)
        => instance.clientMap[id];

    public static string GetUserName(long id)
    {
        return id == 1 ? "服务器" : instance.clientMap[id].UserName;
    }

    public static int GetMoney(long id)
        => GetPlayerStat(id).Money;

    public static void SetMoney(long id, int money)
        => GetPlayerStat(id).Money = money;

    private void OnPeerConnected(long id)
    {
        Log.Info("Server", $"Client {id} 已连接");
        RpcId(id, nameof(RequireRegister));
    }

    private void OnPeerDisconnected(long id)
    {
        Log.Info("Server", $"Client {id} 断开连接");
        Rpc(nameof(DestroyPlayer), id);
        Chatbox.SendMsg($"{GetUserName(id)} 退出游戏");

        clientMap.Remove(id);
    }

    private void OnServerDisconnected()
    {
        GetTree().ChangeSceneToFile(menuPath);
    }
}