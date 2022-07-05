namespace Pixel.Camera;

public class ProxyCameraMode : CameraMode
{
	public new Vector3 Position => Original.Position;
	public new Rotation Rotation => Original.Rotation;
	public new float FieldOfView => Original.FieldOfView;
	public new Entity Viewer => Original.Viewer;
	public new float DoFPoint => Original.DoFPoint;
	public new float DoFBlurSize => Original.DoFBlurSize;
	public new float ViewModelFieldOfView => Original.ViewModelFieldOfView;
	public new float ViewModelZNear => Original.ViewModelZNear;
	public new float ViewModelZFar => Original.ViewModelZFar;
	public new float ZNear => Original.ZNear;
	public new float ZFar => Original.ZFar;

	public Vector3 RealPosition { get; internal set; }

	private CameraMode Original;

	private CameraSetup originalSetup;

	public ProxyCameraMode( CameraMode cam ) : base()
	{
		this.Original = cam;

		originalSetup = new();

		cam.Enabled = false;
	}

	public override void Build( ref CameraSetup camSetup )
	{
		Log.Info( Position );
		base.Build( ref originalSetup );
		Original.Build( ref originalSetup );

		camSetup.Position = 100f;
		RealPosition = camSetup.Position;
	}

	public override void Update()
	{
		Log.Info( Position );
	}
}
