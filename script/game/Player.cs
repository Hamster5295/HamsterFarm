using Godot;

public partial class Player : Sprite2D
{
    public static bool InputLock { get; set; } = false;

    [Export] private float speed;
    [Export] private float hBorder = 1024, vBorder = 512;

    private Label txtName;

    private long peerId;
    private Vector2 targetPos, dir;

    public override void _Ready()
    {
        txtName = GetNode<Label>("Name");
    }

    public override void _Process(double delta)
    {
        Position = Position.Lerp(targetPos, 0.2f);
        if (peerId != Multiplayer.GetUniqueId()) return;

        if (Input.IsActionJustPressed("plant") && !InputLock)
        {
            Server.PlantManager.RequirePlant(Position);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (peerId == Multiplayer.GetUniqueId() && !InputLock)
        {
            dir = Vector2.Zero;
            if (Input.IsActionPressed("move_up")) dir += Vector2.Up;
            if (Input.IsActionPressed("move_down")) dir += Vector2.Down;
            if (Input.IsActionPressed("move_left")) dir += Vector2.Left;
            if (Input.IsActionPressed("move_right")) dir += Vector2.Right;
            if (dir != Vector2.Zero)
                RpcId(1, nameof(InputMove), dir.Normalized() * speed * (float)delta);
        }
    }

    public void Init(long peerId, string name, Vector2 pos)
    {
        this.peerId = peerId;
        Name = $"Player_{peerId}";
        txtName.Text = name;

        Position = pos;
        targetPos = pos;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void InputMove(Vector2 vel)
    {
        targetPos += vel;
        if (vel.X != 0) FlipH = vel.X > 0;
        Rpc(nameof(Move), targetPos, FlipH);
    }

    [Rpc(CallLocal = true)]
    private void Move(Vector2 target, bool flip)
    {
        target.X = Mathf.Clamp(target.X, -hBorder, hBorder);
        target.Y = Mathf.Clamp(target.Y, -vBorder, vBorder);
        targetPos = target;
        FlipH = flip;
    }
}