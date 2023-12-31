using Godot;

public partial class Toast : Node
{
    private static Toast instance;

    [Export] private PackedScene toast;
    [Export] private Vector2 offset;

    public override void _EnterTree()
    {
        instance = this;
    }

    public static void MakeText(long id, string text)
        => MakeText(id, text, Colors.White);

    public static void MakeText(long id, string text, Color color)
    {
        instance.RpcId(id, nameof(instance._MakeText), text, color);
    }

    [Rpc]
    private void _MakeText(string text, Color color)
    {
        if (Multiplayer.IsServer()) return;

        var label = instance.toast.Instantiate<Label>();
        Server.Player.AddChild(label);
        label.Text = text;
        label.Modulate = color;
        label.Position = instance.offset;

        var tween = GetTree().CreateTween();
        tween.TweenProperty(label, "position:y", -300, 1.5f);
        tween.Parallel().TweenProperty(label, "modulate:a", 0, 1.5f);
        tween.TweenCallback(Callable.From(() => label.QueueFree()));
        tween.Play();
    }
}