using System.Net;
using System.Threading;
using Godot;
using HamsterUtils;

public partial class MultiplayerInitializer : Node
{
    [Export] private bool serverOnly = false;
    [Export(PropertyHint.File)] private string gameScenePath;
    [Export] private LineEdit inputIp, inputPort, inputUserName;
    [Export] private Label txtHint;

    public override void _Ready()
    {
        inputUserName.Text = CommonData.Get(Server.USERNAME_KEY, $"用户{GD.Randi()}");
        if (serverOnly) CallDeferred(MethodName.CreateServer);
    }

    private void CreateServer()
    {
        if (int.TryParse(inputPort.Text, out var port))
        {
            var peer = new ENetMultiplayerPeer();
            var err = peer.CreateServer(port);
            if (err != Error.Ok)
            {
                txtHint.Text = err.ToString();
            }
            else
            {
                Multiplayer.MultiplayerPeer = peer;
                GetTree().ChangeSceneToFile(gameScenePath);
            }
        }
        else
        {
            txtHint.Text = "端口号必须为数字!";
        }
    }

    private void CreateClient()
    {
        if (int.TryParse(inputPort.Text, out var port))
        {
            var peer = new ENetMultiplayerPeer();
            var err = peer.CreateClient(inputIp.Text, port);
            if (err != Error.Ok)
            {
                txtHint.Text = err.ToString();
            }
            else
            {
                Multiplayer.MultiplayerPeer = peer;
                CommonData.Set(Server.USERNAME_KEY, inputUserName.Text);
                GetTree().ChangeSceneToFile(gameScenePath);
            }
        }
        else
        {
            txtHint.Text = "端口号必须为数字!";
        }
    }
}
