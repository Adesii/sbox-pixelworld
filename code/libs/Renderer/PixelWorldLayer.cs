using Sandbox.UI;

namespace Pixel;

public class PixelWorldLayer : PixelLayer
{

	protected override void Init()
	{
		Scene = Map.Scene;
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
			Render.Attributes.Set( "ScaleFactor", Settings.ScaleFactor );
		}
		if ( Settings.IsQuantized && QuantizeLUT != null && QuantizeLUT.IsLoaded )
		{
			Render.Attributes.SetCombo( "D_IS_QUANTIZED", true );
			Render.Attributes.Set( "Quantization", QuantizeLUT );
			Log.Info( "Quantized" );
		}
		Render.Draw.DrawScene( PixelTextures.Color, PixelTextures.Depth, Map.Scene, Attributes, renderrect, renderpos, RenderRotation, cam.FieldOfView, cam.ZNear, cam.ZFar, cam.Ortho );
		Render.Draw2D.Material = PixelRenderer.ScreenMaterial;
		Render.Draw2D.Texture = PixelTextures.Color;
		Render.Draw2D.Color = Color.White;
		Rect rect = new( Settings.IsPixelPerfectWithOverscan ? (new Vector2( OffsetDelta.y, OffsetDelta.x )) : 0, Screen.Size );
		Render.Draw2D.Quad( rect.TopLeft, rect.TopRight, rect.BottomRight, rect.BottomLeft );
		Render.Attributes.Clear();
	}
}
