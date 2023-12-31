using Godot;

public partial class PlantStat : GodotObject
{
    public readonly PlantConfig Config;
    public readonly float StartTime;
    public readonly Plant Plant;

    private int lastState = 0;

    public PlantStat(PlantConfig config, float startTime, Plant plant)
    {
        Config = config;
        StartTime = startTime;
        Plant = plant;
    }

    public void UpdateState(long id = -1)
    {
        var prog = GetProgress();
        var state = 0;
        if (prog <= 0.2f) state = 0;
        else if (prog <= 0.7f) state = 1;
        else if (prog < 1) state = 2;
        else state = 3;

        if (state > lastState || id > 0)
        {
            Plant.UpdateState(state, id);
            lastState = state;
        }
    }

    public bool IsRape()
        => GetProgress() >= 1;

    private float GetProgress()
        => (Time.GetTicksMsec() - StartTime) / (1000 * Config.GrowTime);
}