package mono.androidx.media2.widget;


public class MediaControlView_OnFullScreenListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		androidx.media2.widget.MediaControlView.OnFullScreenListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onFullScreen:(Landroid/view/View;Z)V:GetOnFullScreen_Landroid_view_View_ZHandler:AndroidX.Media2.Widget.MediaControlView/IOnFullScreenListenerInvoker, Xamarin.AndroidX.Media2.Widget\n" +
			"";
		mono.android.Runtime.register ("AndroidX.Media2.Widget.MediaControlView+IOnFullScreenListenerImplementor, Xamarin.AndroidX.Media2.Widget", MediaControlView_OnFullScreenListenerImplementor.class, __md_methods);
	}


	public MediaControlView_OnFullScreenListenerImplementor ()
	{
		super ();
		if (getClass () == MediaControlView_OnFullScreenListenerImplementor.class)
			mono.android.TypeManager.Activate ("AndroidX.Media2.Widget.MediaControlView+IOnFullScreenListenerImplementor, Xamarin.AndroidX.Media2.Widget", "", this, new java.lang.Object[] {  });
	}


	public void onFullScreen (android.view.View p0, boolean p1)
	{
		n_onFullScreen (p0, p1);
	}

	private native void n_onFullScreen (android.view.View p0, boolean p1);

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
