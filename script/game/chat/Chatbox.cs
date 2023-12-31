using Godot;
using Godot.Collections;

public partial class Chatbox : ColorRect
{
    private static Chatbox instance;

    [Export] private PackedScene chatMsg;

    private ScrollContainer scrollBox;
    private VBoxContainer msgBox;
    private LineEdit inputMsg;

    private Array<string> msgs;
    private int scrollBuffer = 0;

    public override void _EnterTree()
    {
        instance = this;
    }

    public override void _Ready()
    {
        scrollBox = GetNode<ScrollContainer>("Scroll");
        inputMsg = GetNode<LineEdit>("TextBox");
        msgBox = GetNode<VBoxContainer>("Scroll/VBox");

        inputMsg.Visible = false;

        if (Multiplayer.IsServer()) msgs = new();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (scrollBuffer > 0)
        {
            scrollBuffer--;
            if (scrollBuffer == 0)
                UpdateScroll();
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey key && !key.Pressed)
        {
            if (key.Keycode == Key.Enter)
            {
                if (inputMsg.Visible)
                {
                    var msg = inputMsg.Text.Trim();
                    inputMsg.Text = "";

                    if (msg.Length == 0) return;

                    RpcId(1, nameof(SendChat), msg);
                    inputMsg.Visible = false;
                    Player.InputLock = false;

                    GetViewport().SetInputAsHandled();
                }
                else if (!Player.InputLock)
                {
                    inputMsg.Visible = true;
                    inputMsg.GrabFocus();
                    Player.InputLock = true;
                }
            }
        }
    }

    public static void SendMsg(string content)
    {
        instance.CallDeferred(Chatbox.MethodName.Rpc, nameof(instance.UpdateChat), 1, content);
    }

    public static void InitMsg(long id)
    {
        instance.RpcId(id, nameof(instance.UpdateAllChats), instance.msgs);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferChannel = 1)]
    private void SendChat(string content)
    {
        var id = Multiplayer.GetRemoteSenderId();
        var msg = $"{Server.GetUserName(id)}: {content}";
        Rpc(nameof(UpdateChat), id, msg);

        msgs.Add($"{id}@{msg}");
    }

    [Rpc(CallLocal = true, TransferChannel = 1)]
    private void UpdateChat(long id, string content)
    {
        var msg = chatMsg.Instantiate<Label>();
        msg.CustomMinimumSize = Vector2.Right * (Size.X - 10);
        msg.Text = content;
        msgBox.AddChild(msg);
        if (id == Multiplayer.GetUniqueId()) msg.Modulate = Colors.Yellow;
        else if (id == 1) msg.Modulate = Colors.Green;

        scrollBuffer = 2;
    }

    [Rpc(TransferChannel = 1)]
    private void UpdateAllChats(Array<string> msgs)
    {
        foreach (var item in msgs)
        {
            var parts = item.Split('@', 2);
            var id = long.Parse(parts[0]);
            UpdateChat(id, parts[1]);
        }
    }

    private void UpdateScroll()
    {
        var value = Mathf.RoundToInt(Mathf.Max(0, msgBox.Size.Y - scrollBox.Size.Y));
        scrollBox.SetDeferred(ScrollContainer.PropertyName.ScrollVertical, value);
    }
}