namespace Pixel;

public class PixelRenderer //: SceneCustomObject
{
    private static PixelRenderer _instance;
    public static PixelRenderer Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new PixelRenderer();
            }
            return _instance;
        }
    }
    public List<PixelLayer> Layers;

    public static Material ScreenMaterial = Material.FromShader("shaders/screen_sprite_renderer_layer.shader");
    public static Material BlitMaterial { get; set; } = Material.Load("materials/postprocess/passthrough.vmat");
    public PixelRenderer() //: base( Map.Scene )
    {
        //ScreenMaterial = Material.Load( "materials/screen_renderer.vmat" );
        BlitMaterial = Material.Load("materials/postprocess/passthrough.vmat");

        //Flags.IsOpaque = false;
        //Flags.IsTranslucent = true;
        Event.Register(this);
    }
    /* [Event.Frame]
	public void UpdatePosition()
	{
		if ( Local.Pawn.IsValid )
		{
			Position = Local.Pawn.Position + Local.Pawn.Rotation.Forward * 1000f;
		}

		Bounds = new( -1000 + Position, 1000 + Position );
	} */

    public static PixelLayer Get(string v, bool CreateIfNotExists = false)
    {
        if (Instance.Layers == null)
            Instance.Layers = new();
        if (!Instance.Layers.Any(x => x.LayerName == v) && CreateIfNotExists)
        {
            Instance.Layers.Add(new PixelLayer()
            {
                LayerName = v
            });
        }
        return Instance.Layers.Where(x => x.LayerName == v).First();
    }

    /* public static void SetupMapWorld()
	{
		var v = -1;
		if ( Instance.Layers == null )
			Instance.Layers = new();
		if ( !Instance.Layers.ContainsKey( v ) )
		{
			Instance.Layers[v] = new PixelWorldLayer
			{
				RenderOrder = v
			};
		}

		Instance.Layers[v].Settings = new()
		{
			IsQuantized = true,
			IsFullScreen = false,
			IsPixelPerfectWithOverscan = true,
			ScaleFactor = 4,// TODO: remove later on and put into a partial file. Is DarkBindsEvil Specific.
			SnapFactor = 4
		};
	} */
    [Event.Tick]
    public void UpdateLayers()
    {
        if (Layers == null || Layers.Count <= 0) return;

        foreach (var item in Layers.OrderBy(x => x.LayerOrder))
        {
            var layer = item;
            if (!layer.IsInit) continue;
            layer.RenderPosition = Camera.Position;
            layer.RenderRotation = Camera.Rotation;
            layer.UpdateLayer();
        }
    }
    public Texture LastDepth;

    //public static bool RenderingEnabled = true;
    public void RenderSceneObject()
    {
        if (Layers == null || Layers.Count <= 0) return;

        //Log.Info( "Rendering SceneObjects" );

        foreach (var item in Layers.OrderBy(x => x.LayerOrder))
        {
            var layer = item;
            if (!layer.IsInit) continue;
            layer.RenderPosition = Camera.Position;
            layer.RenderRotation = Camera.Rotation;
            layer.RenderLayer();
        }

    }
}

