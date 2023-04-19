namespace Pixel;

[GameResource("Pixel Layer", "pxl", "Describes what render layers exist and in what order they are rendered in")]
public class PixelLayerAsset : GameResource
{
    public static List<PixelLayerAsset> LoadedLayers = new();
    public static Dictionary<int, PixelLayerAsset> Layers = new();
    public static Dictionary<string, PixelLayerAsset> LayersByName = new();
    public int Order { get; set; }

    public bool IsQuantized { get; set; }
    public bool IsFullScreen { get; set; } = true;
    //Higher Value means less pixels
    [Sandbox.Range(1, 512, 1)]
    public int ScaleFactorX { get; set; } = 32;
    [Sandbox.Range(1, 512, 1)]
    public int ScaleFactorY { get; set; } = 4;
    public bool IsPixelPerfectWithOverscan { get; set; } = true;
    //Same as ScaleFactor but for Camera Snapping (Not working currently)
    [Sandbox.Range(1, 512, 1)]
    public int SnapFactorX { get; set; } = 32;
    [Sandbox.Range(1, 512, 1)]
    public int SnapFactorY { get; set; } = 8;

    [Sandbox.Range(1, 6, 0.1f)]
    public int ScaleReference { get; set; } = 1;

    public Color AmbientLight { get; set; } = Color.Transparent;
    public float AmbientLightIntensity { get; set; } = 100f;

    protected override void PostLoad()
    {
        if (!Game.IsClient) return;

        base.PostLoad();
        if (!LoadedLayers.Contains(this))
        {
            LoadedLayers.Add(this);
            var layer = PixelRenderer.Get(ResourceName.ToLower(), true);
            SetSettings(layer);
            layer.ViewChanged();
        }
        Layers[Order] = this;
        LayersByName[ResourceName.ToLower()] = this;

    }

    private void SetSettings(PixelLayer layer)
    {
        if (!Game.IsClient) return;
        layer.Settings = new()
        {
            IsFullScreen = IsFullScreen,
            IsQuantized = IsQuantized,
            IsPixelPerfectWithOverscan = IsPixelPerfectWithOverscan,
            ScaleFactor = (MathF.Max(ScaleFactorY, ScaleFactorX) / MathF.Min(ScaleFactorY, ScaleFactorX)).CeilToInt(),
            SnapFactor = (MathF.Max(SnapFactorY, SnapFactorX) / MathF.Min(SnapFactorY, SnapFactorX)).CeilToInt(),
            ScaleReference = ScaleReference
        };
        if (layer.Scene.IsValid())
        {

            layer.Scene.AmbientLightColor = AmbientLight * AmbientLightIntensity;
        }
        layer.LayerOrder = Order;
    }

    protected override void PostReload()
    {
        if (!Game.IsClient) return;

        base.PostReload();
        if (!LoadedLayers.Contains(this))
        {
            LoadedLayers.Add(this);
        }
        var layer = PixelRenderer.Get(ResourceName.ToLower(), true);
        SetSettings(layer);
        layer.ViewChanged();

        Layers[Order] = this;
        LayersByName[ResourceName.ToLower()] = this;
    }

    public static SceneWorld GetSceneWorld(string FileName)
    {
        if (LayersByName.TryGetValue(FileName.ToLower(), out PixelLayerAsset value))
        {
            return PixelRenderer.Get(value.ResourceName.ToLower()).Scene;
        }
        return null;
    }

    public static PixelLayerAsset GetLayerAsset(string FileName)
    {
        if (LayersByName.TryGetValue(FileName.ToLower(), out PixelLayerAsset value))
        {
            return value;
        }
        return null;
    }

    public static PixelLayer GetLayer(string FileName)
    {
        if (LayersByName.TryGetValue(FileName.ToLower(), out PixelLayerAsset value))
        {
            return PixelRenderer.Get(value.ResourceName.ToLower());
        }
        return null;
    }

}
