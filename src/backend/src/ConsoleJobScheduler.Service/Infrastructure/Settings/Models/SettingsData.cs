namespace ConsoleJobScheduler.Service.Infrastructure.Settings.Models;

using System.Globalization;

public delegate bool SettingConvertFunc<T>(string? s, out T v);

public sealed class SettingsData
{
    private readonly IDictionary<string, string?> _data;

    public int Count => _data.Count;

    public SettingsData()
        : this(new Dictionary<string, string?>())
    {
    }

    public SettingsData(IDictionary<string, string?> data)
    {
        _data = data ?? throw new ArgumentNullException(nameof(data));
    }

    public void Set(string key, bool? value)
    {
        _data[key] = value?.ToString(CultureInfo.InvariantCulture);
    }

    public void Set(string key, int? value)
    {
        _data[key] = value?.ToString(CultureInfo.InvariantCulture);
    }

    public void Set(string key, string? value)
    {
        _data[key] = value;
    }

    public string GetString(string key, string defaultValue)
    {
        return GetValue(key) ?? defaultValue;
    }

    public int GetInt(string key, int defaultValue)
    {
        return Get(key, TryParse, defaultValue);
    }

    public bool GetBool(string key, bool defaultValue)
    {
        return Get(key, bool.TryParse, defaultValue);
    }

    public TValue Get<TValue>(string key, SettingConvertFunc<TValue> settingConvertFunc, TValue defaultValue)
    {
        ArgumentNullException.ThrowIfNull(settingConvertFunc);

        var value = GetValue(key);
        if (value == null)
        {
            return defaultValue;
        }

        return settingConvertFunc(value, out var result) ? result : defaultValue;
    }

    private string? GetValue(string key)
    {
        return _data.TryGetValue(key, out var value) ? value : null;
    }

    public static bool TryParse(string? s, out int result)
    {
        return int.TryParse(s, CultureInfo.InvariantCulture, out result);
    }

    public IEnumerator<KeyValuePair<string, string?>> GetEnumerator()
    {
        return _data.GetEnumerator();
    }
}