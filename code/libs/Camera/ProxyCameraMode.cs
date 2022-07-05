namespace Pixel.Camera;

public partial class ProxyCameraMode : CameraMode
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

	[Net]
	private CameraMode Original { get; set; }

	private CameraSetup originalSetup;

	public Vector3 RealPosition { get; internal set; }


	public ProxyCameraMode()
	{
		originalSetup = new();
	}

	public override void Build( ref CameraSetup camSetup )
	{
		base.Build( ref camSetup );
		Original.Build( ref camSetup );

		//originalSetup.Position = 100f;
		//RealPosition = originalSetup.Position;

	}

	public override void Update()
	{
		Log.Info( Position );
	}


	public static ProxyCameraMode GetProxy<T>() where T : CameraMode, new()
	{
		var proxy = new ProxyCameraMode
		{
			Original = new T()
		};
		return proxy;
	}

	public static ProxyCameraMode GetProxy( CameraMode original )
	{
		var proxy = new ProxyCameraMode
		{
			Original = original
		};
		return proxy;
	}
}
