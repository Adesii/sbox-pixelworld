using Pixel.Camera;

namespace Pixel;

public class PixelRenderer : SceneCustomObject
{
	public static PixelRenderer _instance;
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
	public static Dictionary<int, PixelLayer> Layers;

	public static CameraMode PlayerCam
	{
		get
		{
			var clientcam = Local.Client.Components.Get<CameraMode>();
			if ( clientcam != null ) return clientcam;

			var cam = Local.Pawn?.Components.Get<CameraMode>();
			if ( cam != null && cam is not ProxyCameraMode )
			{
				cam.Enabled = false;
				cam = new ProxyCameraMode( cam )
				{
					Enabled = true
				};
				Local.Pawn.Components.Add( cam );
				Log.Info( "ProxyCameraMode created" );
			}

			return cam;
		}
	}

	public static Material ScreenMaterial { get; set; } = Material.Load( "materials/screen_renderer.vmat" );

	public PixelRenderer() : base( Map.Scene )
	{
		ScreenMaterial = Material.Load( "materials/screen_renderer.vmat" );
		Event.Register( this );
	}
	[Event.Frame]
	public void UpdatePosition()
	{
		if ( Local.Pawn.IsValid() )
		{
			Position = Local.Pawn.Position + Local.Pawn.Rotation.Forward * 100;
			if ( PlayerCam is ProxyCameraMode proxy )
			{
				Position = proxy.RealPosition;
			}
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
		if ( Layers == null )
			Layers = new();
		if ( !Layers.ContainsKey( v ) )
		{
			Layers[v] = new PixelLayer
			{
				RenderOrder = v
			};
		}
		return Layers[v];
	}

	public void SetupMapWorld()
	{
		var layer = Get( 0 );
		layer.Settings = new()
		{
			IsQuantized = true,
			IsFullScreen = false,
			IsPixelPerfectWithOverscan = true,
			ScaleFactor = 32 / 3,// TODO: remove later on and put into a partial file. Is DarkBindsEvil Specific.
			SnapFactor = 32 / 8
		};

		//layer.Scene.Delete();
		//layer.Scene = Map.Scene;

		Log.Info( "Map world setup" );

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
	public override void RenderSceneObject()
	{

		Position = float.MinValue; // TODO: remove later on once i figure out a way to not render this inside of itself.

		if ( Layers == null || Layers.Count <= 0 ) return;

		foreach ( var item in Layers.OrderBy( x => x.Key ) )
		{
			var layer = item.Value;
			if ( !layer.IsInit ) continue;
			layer.RenderPosition = PlayerCam.Position;
			layer.RenderRotation = PlayerCam.Rotation;

			layer.RenderOrder = item.Key;
			layer.RenderLayer();
		}


	}
}
