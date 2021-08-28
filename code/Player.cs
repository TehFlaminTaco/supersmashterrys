using Sandbox;
using System;
using System.Linq;

partial class SmashPlayer : Player
{
	private TimeSince timeSinceDropped;
	public SmashPlayer()
	{
		Inventory = new Inventory( this );
	}

	[Net]
	public bool GodMode {get; set;} = false;

	private DamageInfo lastDamage;

	public override void Respawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );

		Controller = new SmashMoveController();
		Animator = new SmashPlayerAnimator();
		Camera = new SmashCam();

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		Inventory.Add(new Pistol(), true);
		base.Respawn();
		Health = 0;
		LastAttacker=null;
		LastAttackerWeapon=null;
	}

	/// <summary>
	/// Called every tick, clientside and serverside.
	/// </summary>
	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		//
		// If you have active children (like a weapon etc) you should call this to 
		// simulate those too.
		//
		SimulateActiveChild( cl, ActiveChild );

		if(ActiveChild is null && Animator is SmashPlayerAnimator sa){
			if(sa.TimeSinceLastMelee > 0.6f && (Input.Pressed(InputButton.Attack1) || Input.Pressed(InputButton.Attack2))){
				sa.TimeSinceLastMelee = 0f;
				SetAnimInt("holdtype", 4);
				SetAnimFloat("holdtype_attack", 2);
				SetAnimInt("holdtype_handedness", Input.Pressed(InputButton.Attack2)?2:1);
				SetAnimBool("b_attack", true);
				if(IsServer)
					Do.After(0.1f, ()=>{
						bool any = false;
						foreach(var target in Entity.All.OfType<Player>().Where(c=>c.WorldSpaceBounds.Center.Distance(WorldSpaceBounds.Center)<85f).Where(c=>(c.Position-Position).Normal.Dot(EyeRot.Forward)>0.6f)){
							target.TakeDamage(DamageInfo
								.Generic(10f)
								.WithForce(200f * EyeRot.Forward)
								.WithAttacker(this)
							);
							any = true;
						}
						if(any)Sound.FromEntity("impact-bullet-flesh", this);
					});
			}
		}

		if ( Input.Pressed( InputButton.Drop ) )
		{
			var dropped = Inventory.DropActive();
			if ( dropped is Weapon wep )
			{
				wep.CollisionGroup = CollisionGroup.Prop;
				wep.thrower = this;
				var joint = PhysicsJoint.Generic.From(wep.PhysicsBody)
									.To(this.PhysicsBody)
									.Create();
				joint.EnableCollision = false;
				Do.After(0.3f, ()=>joint.Remove());
				dropped.PhysicsGroup.ApplyImpulse( Velocity + EyeRot.Forward * 1000.0f + Vector3.Up * 100.0f, true );
				dropped.PhysicsGroup.ApplyAngularImpulse( dropped.Rotation.Right * -3000f, true );
				timeSinceDropped = 0;
			}
		}


		if(IsServer && LifeState == LifeState.Alive && Position.Length > 8000f){
			OnKilled();
		}
	}

	public override void StartTouch( Entity other )
	{
		if (other is Weapon && other.Owner is null && other.Velocity.Length > 30f){
			/*TakeDamage(DamageInfo
				.Generic(other.Velocity.Length / 30f)
				.WithForce(other.Velocity / 30f)
			);*/
		}
		if ( timeSinceDropped < 1 ) return;

		base.StartTouch( other );
	}

	public override void TakeDamage( DamageInfo info )
	{
		Velocity += info.Force;
		if(GodMode)return;

		if ( GetHitboxGroup( info.HitboxIndex ) == 1 )
		{
			info.Damage *= 2.0f;
			info.Force *= 2.0f;
		}

		lastDamage = info;
		TookDamage( lastDamage.Flags, lastDamage.Position, lastDamage.Force );

		//base.TakeDamage( info );
		Health += info.Damage;
		if(Health > 300)
			Health = 300;
		Velocity += info.Force * (Health / 100f);

		if(info.Attacker is Player){
			LastAttacker = info.Attacker;
			LastAttackerWeapon = info.Weapon;
		}
	}

	[ClientRpc]
	public void TookDamage( DamageFlags damageFlags, Vector3 forcePos, Vector3 force )
	{
	}

	public override void OnKilled()
	{
		base.OnKilled();

		if ( lastDamage.Flags.HasFlag( DamageFlags.Vehicle ) )
		{
			Particles.Create( "particles/impact.flesh.bloodpuff-big.vpcf", lastDamage.Position );
			Particles.Create( "particles/impact.flesh-big.vpcf", lastDamage.Position );
			PlaySound( "kersplat" );
		}

		//VehicleController = null;
		//VehicleAnimator = null;
		//VehicleCamera = null;
		//Vehicle = null;

		BecomeRagdollOnClient( Velocity, lastDamage.Flags, lastDamage.Position, lastDamage.Force, GetHitboxBone( lastDamage.HitboxIndex ) );
		//LastCamera = MainCamera;
		//MainCamera = new SpectateRagdollCamera();
		//Camera = MainCamera;
		//Controller = null;

		EnableAllCollisions = false;
		EnableDrawing = false;

		//Inventory.DropActive();
		Inventory.DeleteContents();
	}
}