
using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.IO;
using System.Threading.Tasks;


/// <summary>
/// This is your game class. This is an entity that is created serverside when
/// the game starts, and is replicated to the client. 
/// 
/// You can use this to create things like HUDs and declare which player class
/// to use for spawned players.
/// </summary>
public partial class SmashGame : Sandbox.Game
{
	public SmashGame()
	{
		if ( IsServer )
		{
			Log.Info( "My Gamemode Has Created Serverside!" );

			// Create a HUD entity. This entity is globally networked
			// and when it is created clientside it creates the actual
			// UI panels. You don't have to create your HUD via an entity,
			// this just feels like a nice neat way to do it.
			new SmashHud();

			AdminCore.OnReload();
			Command.SetupCommands();
		}

		if ( IsClient )
		{
			Log.Info( "My Gamemode Has Created Clientside!" );
		}
	}

	/// <summary>
	/// A client has joined the server. Make them a pawn to play with
	/// </summary>
	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );

		var player = new SmashPlayer();
		client.Pawn = player;

		player.Respawn();
	}

	public override void DoPlayerNoclip(Client player){
		DoPlayerNoclip(player, false);
	}

	public void DoPlayerNoclip( Client player, bool force = false  )
	{
		if(!player.HasCommand("noclip")) return;
		if ( player.Pawn is Player basePlayer )
		{
			if ( basePlayer.DevController is NoclipController )
			{
				basePlayer.DevController = null;
			}
			else
			{
				basePlayer.DevController = new NoclipController();
			}
		}
	}

	TimeSince lastGunDropped {get;set;} = 0f;
	public static Random rng = new();
	[Event.Tick]
	public void RainGuns(){
		if(Local.Pawn is not null)return;
		if(lastGunDropped > 3f){
			Weapon newGun=null;
			var gunType = rng.Next()%3;
			switch(gunType){
				case 0:
					newGun = new Pistol();
					break;
				case 1:
					newGun = new Crossbow();
					break;
				case 2:
					newGun = new Shotgun();
					break;
			}
			newGun.Position = new Vector3((float)(rng.NextDouble()-0.5f)*4000f,(float)(rng.NextDouble()-0.5f)*4000f,3000f);
			lastGunDropped = 0f;
		}
	}
}