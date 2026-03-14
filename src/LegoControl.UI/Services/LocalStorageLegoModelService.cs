using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using LegoControl.Core.Models;
using LegoControl.Core.Services;
using Microsoft.JSInterop;

namespace LegoControl.UI.Services;

public class LocalStorageLegoModelService(HttpClient http, IJSRuntime js) : ILegoModelService
{
    private const string StorageKey = "lc_models";
    private const string OverridesKey = "lc_model_overrides";

    private static readonly JsonSerializerOptions ModelOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private List<LegoModel> _models = [];
    private readonly Dictionary<string, LegoModel> _builtInOriginals = new();

    public IReadOnlyList<LegoModel> Models => _models.AsReadOnly();

    public async Task InitializeAsync()
    {
        // Load built-in models from wwwroot/models/
        try
        {
            var manifest = await http.GetFromJsonAsync<ModelManifest>("models/manifest.json", ModelOptions);
            if (manifest?.Models is not null)
            {
                foreach (var filename in manifest.Models)
                {
                    try
                    {
                        var model = await http.GetFromJsonAsync<LegoModel>($"models/{filename}", ModelOptions);
                        if (model is not null)
                        {
                            model.IsBuiltIn = true;
                            _builtInOriginals[model.Id] = DeepCopy(model);
                            _models.Add(model);
                        }
                    }
                    catch { /* skip invalid or missing files */ }
                }
            }
        }
        catch { /* manifest not found */ }

        // Apply any user edits to built-in models
        var overridesJson = await js.InvokeAsync<string?>("localStorage.getItem", OverridesKey);
        if (overridesJson is not null)
        {
            try
            {
                var overrides = JsonSerializer.Deserialize<Dictionary<string, LegoModel>>(overridesJson, ModelOptions) ?? new();
                foreach (var (id, override_) in overrides)
                {
                    var model = _models.FirstOrDefault(m => m.Id == id);
                    if (model is not null)
                    {
                        model.Name = override_.Name;
                        model.SetNumber = override_.SetNumber;
                        model.ConnectionInstructions = override_.ConnectionInstructions;
                        model.DefaultConfig = override_.DefaultConfig;
                        model.IsEdited = true;
                    }
                }
            }
            catch { /* ignore corrupt data */ }
        }

        // Load user-added models from localStorage
        var json = await js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
        if (json is not null)
        {
            try
            {
                var userModels = JsonSerializer.Deserialize<List<LegoModel>>(json, ModelOptions) ?? [];
                foreach (var m in userModels)
                {
                    m.IsBuiltIn = false;
                    _models.Add(m);
                }
            }
            catch { /* ignore corrupt data */ }
        }
    }

    public async Task AddAsync(LegoModel model)
    {
        _models.Add(model);
        await SaveUserModelsAsync();
    }

    public async Task UpdateAsync(LegoModel model)
    {
        var idx = _models.FindIndex(m => m.Id == model.Id);
        if (idx < 0) return;

        if (_models[idx].IsBuiltIn)
        {
            model.IsBuiltIn = true;
            model.IsEdited = true;
            _models[idx] = model;
            await SaveOverridesAsync();
        }
        else
        {
            model.IsBuiltIn = false;
            _models[idx] = model;
            await SaveUserModelsAsync();
        }
    }

    public async Task RemoveAsync(string id)
    {
        _models.RemoveAll(m => m.Id == id && !m.IsBuiltIn);
        await SaveUserModelsAsync();
    }

    public async Task RevertAsync(string id)
    {
        var original = _builtInOriginals.GetValueOrDefault(id);
        if (original is null) return;

        var model = _models.FirstOrDefault(m => m.Id == id);
        if (model is null) return;

        model.Name = original.Name;
        model.SetNumber = original.SetNumber;
        model.ConnectionInstructions = [.. original.ConnectionInstructions];
        model.DefaultConfig = DeepCopy(original.DefaultConfig);
        model.IsEdited = false;

        await SaveOverridesAsync();
    }

    private async Task SaveUserModelsAsync()
    {
        var userModels = _models.Where(m => !m.IsBuiltIn).ToList();
        var json = JsonSerializer.Serialize(userModels, ModelOptions);
        await js.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
    }

    private async Task SaveOverridesAsync()
    {
        var overrides = _models
            .Where(m => m.IsBuiltIn && m.IsEdited)
            .ToDictionary(m => m.Id);
        var json = JsonSerializer.Serialize(overrides, ModelOptions);
        await js.InvokeVoidAsync("localStorage.setItem", OverridesKey, json);
    }

    private static LegoModel DeepCopy(LegoModel model)
    {
        var json = JsonSerializer.Serialize(model, ModelOptions);
        return JsonSerializer.Deserialize<LegoModel>(json, ModelOptions)!;
    }

    private static DeviceConfig DeepCopy(DeviceConfig config)
    {
        var json = JsonSerializer.Serialize(config, ModelOptions);
        return JsonSerializer.Deserialize<DeviceConfig>(json, ModelOptions)!;
    }

    private record ModelManifest(List<string> Models);
}
