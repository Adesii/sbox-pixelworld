using Sandbox.UI;

namespace Pixel;

public class PixelWorldLayer : PixelLayer
{

	protected override void Init()
	{
		Scene = Game.SceneWorld;
		LayerGUID = Guid.NewGuid().ToString();
		ViewChanged();
		Attributes = new();
		Event.Register( this );

		IsInit = true;
	}

	public override void RenderLayer()
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
			Graphics.Attributes.Set( "ScaleFactor", Settings.ScaleFactor );
		}
		if ( Settings.IsQuantized && QuantizeLUT != null && QuantizeLUT.IsLoaded )
		{
			Graphics.Attributes.SetCombo( "D_IS_QUANTIZED", true );
			Graphics.Attributes.Set( "Quantization", QuantizeLUT );
			Log.Info( "Quantized" );
		}
		Camera.Main.World = Scene;
		Graphics.RenderToTexture( Camera.Current, PixelTextures.Color );
		Rect rect = new( Settings.IsPixelPerfectWithOverscan ? (new Vector2( OffsetDelta.y, OffsetDelta.x )) : 0, Screen.Size );
		Graphics.DrawQuad( rect, PixelRenderer.ScreenMaterial, Color.White, Graphics.Attributes );

		Log.Info( "RenderLayer" );
		Graphics.Attributes.Clear();

		Camera.Main.World = Game.SceneWorld;
	}
}
