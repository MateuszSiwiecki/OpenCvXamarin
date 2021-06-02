package mono.androidx.media2.widget;


public class VideoView_OnViewTypeChangedListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		androidx.media2.widget.VideoView.OnViewTypeChangedListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onViewTypeChanged:(Landroid/view/View;I)V:GetOnViewTypeChanged_Landroid_view_View_IHandler:AndroidX.Media2.Widget.VideoView/IOnViewTypeChangedListenerInvoker, Xamarin.AndroidX.Media2.Widget\n" +
			"";
		mono.android.Runtime.register ("AndroidX.Media2.Widget.VideoView+IOnViewTypeChangedListenerImplementor, Xamarin.AndroidX.Media2.Widget", VideoView_OnViewTypeChangedListenerImplementor.class, __md_methods);
	}


	public VideoView_OnViewTypeChangedListenerImplementor ()
	{
		super ();
		if (getClass () == VideoView_OnViewTypeChangedListenerImplementor.class)
			mono.android.TypeManager.Activate ("AndroidX.Media2.Widget.VideoView+IOnViewTypeChangedListenerImplementor, Xamarin.AndroidX.Media2.Widget", "", this, new java.lang.Object[] {  });
	}


	public void onViewTypeChanged (android.view.View p0, int p1)
	{
		n_onViewTypeChanged (p0, p1);
	}

	private native void n_onViewTypeChanged (android.view.View p0, int p1);

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
