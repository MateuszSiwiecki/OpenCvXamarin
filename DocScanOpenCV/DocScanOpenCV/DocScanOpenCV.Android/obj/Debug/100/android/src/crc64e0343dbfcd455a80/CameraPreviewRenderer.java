package crc64e0343dbfcd455a80;


public class CameraPreviewRenderer
	extends android.widget.FrameLayout
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("DocScanOpenCV.Droid.Renderers.CameraPreviewRenderer, DocScanOpenCV.Android", CameraPreviewRenderer.class, __md_methods);
	}


	public CameraPreviewRenderer (android.content.Context p0)
	{
		super (p0);
		if (getClass () == CameraPreviewRenderer.class)
			mono.android.TypeManager.Activate ("DocScanOpenCV.Droid.Renderers.CameraPreviewRenderer, DocScanOpenCV.Android", "Android.Content.Context, Mono.Android", this, new java.lang.Object[] { p0 });
	}


	public CameraPreviewRenderer (android.content.Context p0, android.util.AttributeSet p1)
	{
		super (p0, p1);
		if (getClass () == CameraPreviewRenderer.class)
			mono.android.TypeManager.Activate ("DocScanOpenCV.Droid.Renderers.CameraPreviewRenderer, DocScanOpenCV.Android", "Android.Content.Context, Mono.Android:Android.Util.IAttributeSet, Mono.Android", this, new java.lang.Object[] { p0, p1 });
	}


	public CameraPreviewRenderer (android.content.Context p0, android.util.AttributeSet p1, int p2)
	{
		super (p0, p1, p2);
		if (getClass () == CameraPreviewRenderer.class)
			mono.android.TypeManager.Activate ("DocScanOpenCV.Droid.Renderers.CameraPreviewRenderer, DocScanOpenCV.Android", "Android.Content.Context, Mono.Android:Android.Util.IAttributeSet, Mono.Android:System.Int32, mscorlib", this, new java.lang.Object[] { p0, p1, p2 });
	}


	public CameraPreviewRenderer (android.content.Context p0, android.util.AttributeSet p1, int p2, int p3)
	{
		super (p0, p1, p2, p3);
		if (getClass () == CameraPreviewRenderer.class)
			mono.android.TypeManager.Activate ("DocScanOpenCV.Droid.Renderers.CameraPreviewRenderer, DocScanOpenCV.Android", "Android.Content.Context, Mono.Android:Android.Util.IAttributeSet, Mono.Android:System.Int32, mscorlib:System.Int32, mscorlib", this, new java.lang.Object[] { p0, p1, p2, p3 });
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
