package mono.androidx.media2.session;


public class SessionToken_OnSessionTokenCreatedListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		androidx.media2.session.SessionToken.OnSessionTokenCreatedListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onSessionTokenCreated:(Landroid/support/v4/media/session/MediaSessionCompat$Token;Landroidx/media2/session/SessionToken;)V:GetOnSessionTokenCreated_Landroid_support_v4_media_session_MediaSessionCompat_Token_Landroidx_media2_session_SessionToken_Handler:AndroidX.Media2.Session.SessionToken/IOnSessionTokenCreatedListenerInvoker, Xamarin.AndroidX.Media2.Session\n" +
			"";
		mono.android.Runtime.register ("AndroidX.Media2.Session.SessionToken+IOnSessionTokenCreatedListenerImplementor, Xamarin.AndroidX.Media2.Session", SessionToken_OnSessionTokenCreatedListenerImplementor.class, __md_methods);
	}


	public SessionToken_OnSessionTokenCreatedListenerImplementor ()
	{
		super ();
		if (getClass () == SessionToken_OnSessionTokenCreatedListenerImplementor.class)
			mono.android.TypeManager.Activate ("AndroidX.Media2.Session.SessionToken+IOnSessionTokenCreatedListenerImplementor, Xamarin.AndroidX.Media2.Session", "", this, new java.lang.Object[] {  });
	}


	public void onSessionTokenCreated (android.support.v4.media.session.MediaSessionCompat.Token p0, androidx.media2.session.SessionToken p1)
	{
		n_onSessionTokenCreated (p0, p1);
	}

	private native void n_onSessionTokenCreated (android.support.v4.media.session.MediaSessionCompat.Token p0, androidx.media2.session.SessionToken p1);

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
