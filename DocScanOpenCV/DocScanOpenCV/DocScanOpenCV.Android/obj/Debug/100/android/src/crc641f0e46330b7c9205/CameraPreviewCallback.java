package crc641f0e46330b7c9205;


public class CameraPreviewCallback
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		android.hardware.Camera.PreviewCallback
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onPreviewFrame:([BLandroid/hardware/Camera;)V:GetOnPreviewFrame_arrayBLandroid_hardware_Camera_Handler:Android.Hardware.Camera/IPreviewCallbackInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n" +
			"";
		mono.android.Runtime.register ("OpenCvSharp.Android.CameraPreviewCallback, OpenCvSharp.Android", CameraPreviewCallback.class, __md_methods);
	}


	public CameraPreviewCallback ()
	{
		super ();
		if (getClass () == CameraPreviewCallback.class)
			mono.android.TypeManager.Activate ("OpenCvSharp.Android.CameraPreviewCallback, OpenCvSharp.Android", "", this, new java.lang.Object[] {  });
	}


	public void onPreviewFrame (byte[] p0, android.hardware.Camera p1)
	{
		n_onPreviewFrame (p0, p1);
	}

	private native void n_onPreviewFrame (byte[] p0, android.hardware.Camera p1);

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
