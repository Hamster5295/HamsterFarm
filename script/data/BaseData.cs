using Godot;

namespace HamsterUtils
{
    public abstract partial class BaseData : Node
    {
        protected const string SAVE_PATH = "user://save.dat";

        private bool Read(out Variant result)
        {
            result = new Variant();
            var path = SAVE_PATH;
            if (!FileAccess.FileExists(path)) return false;
            var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
            result = Json.ParseString(file.GetLine());
            file.Close();
            return true;
        }

        protected bool Read<[MustBeVariant] T>(out T result)
        {
            var isSuccessful = Read(out var r);
            result = r.As<T>();
            return isSuccessful;
        }

        protected static void Save(Variant obj)
        {
            var file = FileAccess.Open(SAVE_PATH, FileAccess.ModeFlags.Write);
            file.StoreLine(Json.Stringify(obj));
            file.Flush();
            file.Close();
        }
    }
}