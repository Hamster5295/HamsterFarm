using Godot;

public static class Log
{
    public static void Info(string header, string msg)
        => GD.Print(Format(header, msg));

    public static void Warn(string header, string msg)
        => GD.PushWarning(Format(header, msg));

    public static void Err(string header, string msg)
        => GD.PushError(Format(header, msg));

    private static string Format(string header, string msg)
        => $"[{header}]: {msg}";
}