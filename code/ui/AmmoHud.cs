using System;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

class AmmoHud : Panel
{
	public AmmoTypePanel ammo1;
	public AmmoTypePanel ammo2;

	protected static SmashPlayer Player {
		get {
			return Local.Pawn as SmashPlayer;
		}
	}

	protected static Weapon ActiveWeapon {
		get {
			return Player?.Inventory.Active as Weapon;
		}
	}

	public AmmoHud()
	{
		AddChild(ammo2 = new(new Ammo{
			valid = ()=>(ActiveWeapon?.UseClip2??false),
			getClip = ()=>ActiveWeapon?.Clip2??0,
			getClipSize = ()=>ActiveWeapon?.Clip2Size??0
		}, "two"));
		AddChild(ammo1 = new(new Ammo{
			valid = ()=>(ActiveWeapon?.UseClip1??false),
			getClip = ()=>ActiveWeapon?.Clip1??0,
			getClipSize = ()=>ActiveWeapon?.Clip1Size??0
		}, "one"));
	}

	public override void Tick()
	{
		ammo1.Tick();
		ammo2.Tick();
	}

	public class AmmoTypePanel : Panel {
		ClipPanel clip;
		public Label pocket;
		public Ammo ammo;
		public AmmoTypePanel(Ammo ammo, string classes=""){
			this.ammo = ammo;
			this.Classes = classes;
			this.AddClass("ammopanel");

			AddChild(clip = new(ammo, classes));
		}

		public override void Tick(){
			SetClass("hidden", !ammo.valid());
			clip.Tick();
		}
	}

	public class ClipPanel : Panel {
		Label inClip;
		Label clipSize;
		Ammo ammo;
		public ClipPanel(Ammo ammo, string classes=""){
			this.ammo = ammo;
			this.Classes = classes;
			this.AddClass("clippanel");
		 	inClip = Add.Label("0", classes);
			inClip.AddClass("clip");
		 	clipSize = Add.Label("/0", classes);
			clipSize.AddClass("clipsize");
		}

		public override void Tick(){
			var iclip = ammo.getClip();
			var iclipSize = ammo.getClipSize();
			clipSize.SetClass("hidden", iclipSize==0);
			if(ammo.valid()){
				if(iclipSize>0)
					clipSize.Text = $"/{iclipSize}";
				inClip.Text = $"{iclip}";
			}
		}
	}

	public class Ammo{
		public Func<bool> valid;
		public Func<int> getClip;
		public Func<int> getClipSize;
	}
}
