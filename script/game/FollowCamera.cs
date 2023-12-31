using Godot;

public partial class FollowCamera : Camera2D
{
    public Node2D Target { get; set; }

    public override void _Process(double delta)
    {
        if (Target != null) Position = Target.GlobalPosition;
    }
}