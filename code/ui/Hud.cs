using Sandbox;
using Sandbox.UI;

//
// You don't need to put things in a namespace, but it doesn't hurt.
//


/// <summary>
/// This is the HUD entity. It creates a RootPanel clientside, which can be accessed
/// via RootPanel on this entity, or Local.Hud.
/// </summary>
public partial class SmashHud : Sandbox.HudEntity<RootPanel>
{
	public static SmashHud Instance = null;
	public SmashHud()
	{
		Instance = this;
		if ( !IsClient )
			return;
		//RootPanel.SetTemplate( "/minimalhud.html" );
		RootPanel.StyleSheet.Load("ui/Hud.scss");
		RootPanel.AddChild<TacoChatBox>();
		RootPanel.AddChild<KillFeed>();
		RootPanel.AddChild<AmmoHud>();
		RootPanel.AddChild<TacoCrosshair>();
		RootPanel.AddChild<TargetHealth>();
	}

	[ClientCmd("hudrefresh")]
	public static void hudrefresh(){
		Instance.RootPanel.DeleteChildren(true);
		Instance.RootPanel.AddChild<TacoChatBox>();
		Instance.RootPanel.AddChild<KillFeed>();
		Instance.RootPanel.AddChild<AmmoHud>();
		Instance.RootPanel.AddChild<TacoCrosshair>();
		Instance.RootPanel.AddChild<TargetHealth>();
	}
}
