package crc64e0343dbfcd455a80;


public class CameraCapture
	extends androidx.appcompat.app.AppCompatActivity
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("DocScanOpenCV.Droid.Renderers.CameraCapture, DocScanOpenCV.Android", CameraCapture.class, __md_methods);
	}


	public CameraCapture ()
	{
		super ();
		if (getClass () == CameraCapture.class)
			mono.android.TypeManager.Activate ("DocScanOpenCV.Droid.Renderers.CameraCapture, DocScanOpenCV.Android", "", this, new java.lang.Object[] {  });
	}


	public CameraCapture (int p0)
	{
		super (p0);
		if (getClass () == CameraCapture.class)
			mono.android.TypeManager.Activate ("DocScanOpenCV.Droid.Renderers.CameraCapture, DocScanOpenCV.Android", "System.Int32, mscorlib", this, new java.lang.Object[] { p0 });
	}

	public CameraCapture (crc6446793fef7ad4d27c.MainActivity p0)
	{
		super ();
		if (getClass () == CameraCapture.class)
			mono.android.TypeManager.Activate ("DocScanOpenCV.Droid.Renderers.CameraCapture, DocScanOpenCV.Android", "DocScanOpenCV.Droid.MainActivity, DocScanOpenCV.Android", this, new java.lang.Object[] { p0 });
	}

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
