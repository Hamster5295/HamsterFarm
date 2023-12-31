using Godot;

public partial class Plant : Sprite2D
{
    private PlantConfig config;

    public void Init(PlantConfig config)
    {
        this.config = config;
        Texture = config.Seed;
    }

    public void UpdateState(int state, long id = -1)
    {
        if (id == -1) Rpc(nameof(_UpdateState), state);
        else RpcId(id, nameof(_UpdateState), state);
    }

    [Rpc(CallLocal = true)]
    private void _UpdateState(int state)
    {
        switch (state)
        {
            case 0:
                Texture = config.Seed;
                break;

            case 1:
                Texture = config.Plant;
                break;

            case 2:
                Texture = config.Flower;
                break;

            case 3:
                Texture = config.Fruit;
                break;
        }
    }

    public void Rape()
    {
        Rpc(nameof(_Rape));
    }

    [Rpc(CallLocal = true)]
    private void _Rape()
    {
        QueueFree();
    }
}