namespace Az.Storage;
using Microsoft.Extensions.Configuration;

public class ContextFactory
{
    private static Dictionary<string, AzureStorageContext> _contexts = new();
    private static IConfiguration _config = default!;

    public ContextFactory(IConfiguration config, string[] stores)
    {
        _config = config;
        foreach (var _ in stores) Add(_);
    }

    private static void Add(string name)
    {
        string conn = _config[name] ?? "";
        if (string.IsNullOrEmpty(conn)) throw new ArgumentException($"Connection string not found: [{name}]");
        var ctx = new AzureStorageContext(conn);
        _contexts.Add(name, ctx);
    }

    public AzureStorageContext this[string key]
    {
        get
        {
            if (!_contexts.ContainsKey(key)) Add(key);
            return _contexts[key];
        }
    }
}