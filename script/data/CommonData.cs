using Godot;

namespace HamsterUtils
{
    public partial class CommonData : DictionaryData<string, Variant>
    {
        public static T Get<[MustBeVariant] T>(string key, T defaultValue)
            => GetValue(key, Variant.From<T>(defaultValue)).As<T>();

        public static void Set<[MustBeVariant] T>(string key, T value)
        {
            SetValue(key, Variant.From(value));
            Save();
        }

        public static bool Has(string key)
            => HasValue(key);
    }
}