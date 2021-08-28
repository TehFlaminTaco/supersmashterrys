
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sandbox.UI
{
	public partial class SmashFeed : KillFeed
	{

		public SmashFeed() : base()
		{
		}

		public override Panel AddEntry( ulong lsteamid, string left, ulong rsteamid, string right, string method )
		{
			/*var e = Current.AddChild<KillFeedEntry>();

			e.Left.Text = left;
			e.Left.SetClass( "me", lsteamid == (Local.Client?.SteamId) );

			e.Method.Text = method;

			e.Right.Text = right;
			e.Right.SetClass( "me", rsteamid == (Local.Client?.SteamId) );

			return e;*/
            var e = base.AddEntry(lsteamid, left, rsteamid, right, "smashed") as KillFeedEntry;

            e.Left.Style.FontColor = Client.All.FirstOrDefault(c=>c.SteamId==lsteamid)?.GetRank().NameColor??"white";
            e.Right.Style.FontColor = Client.All.FirstOrDefault(c=>c.SteamId==rsteamid)?.GetRank().NameColor??"white";

            return e;
		}
	}
}
