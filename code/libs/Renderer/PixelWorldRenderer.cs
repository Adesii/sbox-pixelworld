using Pixel.Camera;

namespace Pixel;

public class PixelRenderer : SceneCustomObject
{
	private static PixelRenderer _instance;
	public static PixelRenderer Instance
	{
		get
		{
			if ( _instance == null )
			{
				_instance = new PixelRenderer();
			}
			return _instance;
		}
	}
	public Dictionary<int, PixelLayer> Layers;

	public static CameraMode PlayerCam
	{
		get
		{
			return Local.Client.Components.Get<CameraMode>() ?? Local.Pawn?.Components.Get<CameraMode>();
		}
	}

	public static Material ScreenMaterial { get; set; } = Material.Load( "materials/screen_renderer.vmat" );
	public static Material BlitMaterial { get; set; } = Material.Load( "materials/postprocess/passthrough.vmat" );
	public PixelRenderer() : base( Map.Scene )
	{
		ScreenMaterial = Material.Load( "materials/screen_renderer.vmat" );
		BlitMaterial = Material.Load( "materials/postprocess/passthrough.vmat" );

		Flags.IsOpaque = false;
		Flags.IsTranslucent = true;
		Event.Register( this );
	}

	~PixelRenderer()
	{
		Event.Unregister( this );

		foreach ( var item in Layers )
		{
			item.Value?.Dispose();
		}
	}
	[Event.Frame]
	public void UpdatePosition()
	{
		if ( Local.Pawn.IsValid )
		{
			Position = Local.Pawn.Position;
		}

		Bounds = new( -1000 + Position, 1000 + Position );
	}

	public static SceneWorld GetDefaultWorld()
	{
		var layer = Get( 0 );
		if ( !layer.IsInit )
			layer.Settings = new()
			{
				IsQuantized = true,
				IsFullScreen = false,
				IsPixelPerfectWithOverscan = true,
				ScaleFactor = 32 / 3,// TODO: remove later on and put into a partial file. Is DarkBindsEvil Specific.
				SnapFactor = 32 / 8

			};
		return layer.Scene;
	}
	public static SceneWorld GetDefaultCharacters()
	{
		var layer = Get( 1 );
		if ( !layer.IsInit )
			layer.Settings = new()
			{
				IsQuantized = false,
				IsFullScreen = true,
				IsPixelPerfectWithOverscan = false
			};
		return layer.Scene;
	}

	public static PixelLayer Get( int v )
	{
		if ( Instance.Layers == null )
			Instance.Layers = new();
		if ( !Instance.Layers.ContainsKey( v ) )
		{
			Instance.Layers[v] = new PixelLayer
			{
				RenderOrder = v
			};
		}
		return Instance.Layers[v];
	}

	public static void SetupMapWorld()
	{
		var v = 0;
		if ( Instance.Layers == null )
			Instance.Layers = new();
		if ( !Instance.Layers.ContainsKey( v ) )
		{
			Instance.Layers[v] = new PixelWorldLayer
			{
				RenderOrder = v
			};
		}

		var layer = Instance.Layers[v];
		layer.Settings = new()
		{
			IsQuantized = true,
			IsFullScreen = false,
			IsPixelPerfectWithOverscan = true,
			ScaleFactor = 32 / 3,// TODO: remove later on and put into a partial file. Is DarkBindsEvil Specific.
			SnapFactor = 32 / 8
		};
	}
	[Event.Tick]
	public void UpdateLayers()
	{
		if ( Layers == null || Layers.Count <= 0 ) return;
		foreach ( var item in Layers.OrderBy( x => x.Key ) )
		{
			var layer = item.Value;
			if ( !layer.IsInit || PlayerCam == null ) continue;
			layer.RenderPosition = PlayerCam.Position;
			layer.RenderRotation = PlayerCam.Rotation;
			layer.RenderOrder = item.Key;
			layer.UpdateLayer();
		}
	}
	public Texture LastDepth;

	public static bool Rendering = false;
	public override void RenderSceneObject()
	{

		if ( Layers == null || Layers.Count <= 0 || !RenderingEnabled ) return;
		RenderingEnabled = false;

		foreach ( var item in Layers.OrderBy( x => x.Key ) )
		{
			var layer = item.Value;
			if ( !layer.IsInit ) continue;
			layer.RenderPosition = PlayerCam.Position;
			layer.RenderRotation = PlayerCam.Rotation;

			layer.RenderOrder = item.Key;
			layer.RenderLayer();
		}

		RenderingEnabled = true;


	}
}
