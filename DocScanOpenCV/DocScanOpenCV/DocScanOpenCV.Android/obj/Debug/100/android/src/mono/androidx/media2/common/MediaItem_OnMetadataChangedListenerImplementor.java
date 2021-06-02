package mono.androidx.media2.common;


public class MediaItem_OnMetadataChangedListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		androidx.media2.common.MediaItem.OnMetadataChangedListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onMetadataChanged:(Landroidx/media2/common/MediaItem;Landroidx/media2/common/MediaMetadata;)V:GetOnMetadataChanged_Landroidx_media2_common_MediaItem_Landroidx_media2_common_MediaMetadata_Handler:AndroidX.Media2.Common.MediaItem/IOnMetadataChangedListenerInvoker, Xamarin.AndroidX.Media2.Common\n" +
			"";
		mono.android.Runtime.register ("AndroidX.Media2.Common.MediaItem+IOnMetadataChangedListenerImplementor, Xamarin.AndroidX.Media2.Common", MediaItem_OnMetadataChangedListenerImplementor.class, __md_methods);
	}


	public MediaItem_OnMetadataChangedListenerImplementor ()
	{
		super ();
		if (getClass () == MediaItem_OnMetadataChangedListenerImplementor.class)
			mono.android.TypeManager.Activate ("AndroidX.Media2.Common.MediaItem+IOnMetadataChangedListenerImplementor, Xamarin.AndroidX.Media2.Common", "", this, new java.lang.Object[] {  });
	}


	public void onMetadataChanged (androidx.media2.common.MediaItem p0, androidx.media2.common.MediaMetadata p1)
	{
		n_onMetadataChanged (p0, p1);
	}

	private native void n_onMetadataChanged (androidx.media2.common.MediaItem p0, androidx.media2.common.MediaMetadata p1);

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
