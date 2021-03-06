namespace Pixel;

public class PixelLayer
{
	private LayerSettings _settings;
	public LayerSettings Settings
	{
		get => _settings;
		set
		{
			_settings = value;
			Init();
		}
	}

	public RenderAttributes Attributes { get; set; }
	public Vector3 RenderOffsetPosition { get; set; }

	public SceneWorld Scene { get; set; }
	public bool IsInit = false;

	public PixelTextures PixelTextures { get; set; }



	public Vector3 RenderPosition { get; set; }
	public Rotation RenderRotation { get; set; }

	public static Vector2 ReferenceSize = new( 1920, 1080 );

	public bool canRender = false;
	internal int RenderOrder = 0;

	public string LayerGUID { get; set; }

	public Texture QuantizeLUT { get; set; }

	~PixelLayer()
	{
		if ( Scene != null )
			Scene.Delete();
	}
	protected virtual void Init()
	{
		Scene = new();
		LayerGUID = Guid.NewGuid().ToString();
		ViewChanged();
		Attributes = new();
		Event.Register( this );

		IsInit = true;
	}

	[Event.Screen.SizeChanged]
	public void ViewChanged()
	{
		PixelTextures?.Dispose();
		//await GameTask.DelayRealtime( 100 );
		if ( Settings.IsFullScreen )
		{
			Settings.RenderSize = ReferenceSize;
		}
		else
		{
			Settings.RenderSize = new Vector2( ReferenceSize.x / Settings.ScaleFactor, ReferenceSize.y / Settings.ScaleFactor );
		}
		if ( Settings.RenderSize.Length <= 2 ) return;
		//PixelTextures = new( Settings.RenderSize, false );

		PixelTextures = new( Settings.RenderSize, false, false, false );
		canRender = true;

	}

	public void Dispose()
	{
		if ( Scene != null )
			Scene.Delete();
		PixelTextures?.Dispose();
	}

	public Vector2 OffsetDelta;
	public Vector3 OldPos;

	public virtual void RenderLayer()
	{
		if ( PixelRenderer.ScreenMaterial == null || PixelTextures == null || !canRender || !Scene.IsValid() )
		{
			return;
		}
		var cam = PixelRenderer.PlayerCam;
		Rect renderrect = new( 0, Settings.RenderSize );
		var renderpos = RenderPosition;
		if ( Settings.IsPixelPerfectWithOverscan )
		{
			OffsetDelta = 0;
			var oldpos = renderpos;
			renderpos = new Vector3( SnapToGridFloor( renderpos.x, Settings.SnapFactor ), SnapToGridFloor( renderpos.y, Settings.SnapFactor ), renderpos.z );
			var snappedPos = renderpos;
			OffsetDelta = snappedPos - oldpos;
			OldPos = snappedPos;
			Render.Attributes.Set( "ScaleFactor", Settings.ScaleFactor );
		}
		if ( Settings.IsQuantized && QuantizeLUT != null && QuantizeLUT.IsLoaded )
		{
			Render.Attributes.SetCombo( "D_IS_QUANTIZED", true );
			Render.Attributes.Set( "Quantization", QuantizeLUT );
		}
		Render.Draw.DrawScene( PixelTextures.Color, PixelTextures.Depth, Scene, Attributes, renderrect, renderpos, RenderRotation, cam.FieldOfView, cam.ZNear, cam.ZFar, cam.Ortho );
		Render.Draw2D.Material = PixelRenderer.ScreenMaterial;
		Render.Draw2D.Texture = PixelTextures.Color;
		Render.Draw2D.Color = Color.White;
		Rect rect = new( Settings.IsPixelPerfectWithOverscan ? (new Vector2( OffsetDelta.y, OffsetDelta.x ) * 2f) : 0, Screen.Size );
		Render.Draw2D.Quad( rect.TopLeft, rect.TopRight, rect.BottomRight, rect.BottomLeft );
		Render.Attributes.Clear();
	}

	public virtual void UpdateLayer()
	{
		//if ( QuantizeLUT == null || !QuantizeLUT.IsLoaded )
		QuantizeLUT = Sandbox.TextureLoader.Image.Load( FileSystem.Mounted, $"ui/pixelation/layer_{RenderOrder}_lut.png", false );

	}

	//
	// Summary:
	//     Snap number to grid
	public static float SnapToGridFloor( float f, float gridSize )
	{
		return MathF.Round( f / gridSize ) * gridSize;
	}
	public class LayerSettings
	{

		public bool IsQuantized { get; set; }
		public bool IsFullScreen { get; set; }
		public Vector2 RenderSize { get; set; }
		public int ScaleFactor { get; set; }
		public bool IsPixelPerfectWithOverscan { get; set; }
		public bool ManualRenderSize { get; internal set; }
		public int SnapFactor { get; internal set; }

		public static LayerSettings Default()
		{
			return new LayerSettings()
			{
				IsQuantized = true,
				RenderSize = Screen.Size / 4,
				IsPixelPerfectWithOverscan = true
			};
		}
	}
}
