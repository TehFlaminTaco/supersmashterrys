using Sandbox;

public partial class Weapon : BaseWeapon, IUse
{
	public virtual float ReloadTime => 3.0f;

	public PickupTrigger PickupTrigger { get; protected set; }

	[Net, Predicted]
	public TimeSince TimeSinceReload { get; set; }

	[Net, Predicted]
	public bool IsReloading { get; set; }

	[Net, Predicted]
	public TimeSince TimeSinceDeployed { get; set; }
	public TimeSince TimeSinceHeld {get; set;} = 0;

	public virtual bool UseClip1 => false;
	public virtual bool UseClip2 => false;
	public virtual int Clip1Size => 0;
    public virtual int Clip2Size => 0;
	[Net, Predicted]
    public int Clip1 {get; set;} = 0;
    [Net, Predicted]
    public int Clip2 {get; set;} = 0;

	[Net]
	public Entity thrower {get; set;}


	public override void Spawn()
	{
		base.Spawn();

		PickupTrigger = new PickupTrigger
		{
			Parent = this,
			Position = Position,
			EnableTouch = true,
			EnableSelfCollisions = false
		};

		PickupTrigger.PhysicsBody.EnableAutoSleeping = false;

		GlowActive = true;
		GlowState = GlowStates.GlowStateOn;
	}

	public override void ActiveStart( Entity ent )
	{
		base.ActiveStart( ent );

		TimeSinceDeployed = 0;
	}

	public override void Reload()
	{
		if ( IsReloading )
			return;
		
		if(Clip1 >= Clip1Size && Clip2 >= Clip2Size)
			return;

		TimeSinceReload = 0;
		IsReloading = true;

		(Owner as AnimEntity)?.SetAnimBool( "b_reload", true );

		StartReloadEffects();
	}

	public override void Simulate( Client owner )
	{
		if ( TimeSinceDeployed < 0.6f )
			return;

		if ( !IsReloading )
		{
			base.Simulate( owner );
		}

		if ( IsReloading && TimeSinceReload > ReloadTime )
		{
			OnReloadFinish();
		}
	}

	public virtual void OnReloadFinish()
	{
		IsReloading = false;
		Clip1 = Clip1Size;
		Clip2 = Clip2Size;
	}

	[ClientRpc]
	public virtual void StartReloadEffects()
	{
		ViewModelEntity?.SetAnimBool( "reload", true );

		// TODO - player third person model reload
	}

	public override void CreateViewModel()
	{
		Host.AssertClient();

		if ( string.IsNullOrEmpty( ViewModelPath ) )
			return;

		ViewModelEntity = new ViewModel
		{
			Position = Position,
			Owner = Owner,
			EnableViewmodelRendering = true
		};

		ViewModelEntity.SetModel( ViewModelPath );
	}

	public bool OnUse( Entity user )
	{
		if ( Owner != null )
			return false;

		if ( !user.IsValid() )
			return false;

		user.StartTouch( this );

		return false;
	}

	public virtual bool IsUsable( Entity user )
	{
		if ( Owner != null ) return false;

		if ( user.Inventory is Inventory inventory )
		{
			return inventory.CanAdd( this );
		}

		return true;
	}

	public void Remove()
	{
		PhysicsGroup?.Wake();
		Delete();
	}

	[Event.Tick]
	public void FallOutOfWorld(){
		if(IsClient)return;
		if(Owner is not null)
			TimeSinceHeld = 0f;
		GlowActive = Owner is null;
		if(TimeSinceHeld > 30f)
			Delete();
		if(Owner is null && Position.Length > 8000f){
			Delete();
		}
	}

	[ClientRpc]
	protected virtual void ShootEffects()
	{
		Host.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );

		if ( IsLocalPawn )
		{
			_ = new Sandbox.ScreenShake.Perlin();
		}

		ViewModelEntity?.SetAnimBool( "fire", true );
		CrosshairPanel?.CreateEvent( "fire" );
	}

	/// <summary>
	/// Shoot a single bullet
	/// </summary>
	public virtual void ShootBullet( Vector3 pos, Vector3 dir, float spread, float force, float damage, float bulletSize )
	{
		var forward = dir;
		forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
		forward = forward.Normal;

		//
		// ShootBullet is coded in a way where we can have bullets pass through shit
		// or bounce off shit, in which case it'll return multiple results
		//
		foreach ( var tr in TraceBullet( pos, pos + forward * 5000, bulletSize ) )
		{
			tr.Surface.DoBulletImpact( tr );

			if ( !IsServer ) continue;
			if ( !tr.Entity.IsValid() ) continue;

			//
			// We turn predictiuon off for this, so any exploding effects don't get culled etc
			//
			using ( Prediction.Off() )
			{
				var damageInfo = DamageInfo.FromBullet( tr.EndPos, forward * 100 * force, damage )
					.UsingTraceResult( tr )
					.WithAttacker( Owner )
					.WithWeapon( this );

				tr.Entity.TakeDamage( damageInfo );
			}
		}
	}

	/// <summary>
	/// Shoot a single bullet from owners view point
	/// </summary>
	public virtual void ShootBullet( float spread, float force, float damage, float bulletSize )
	{
		ShootBullet( Owner.EyePos, Owner.EyeRot.Forward, spread, force, damage, bulletSize );
	}

	/// <summary>
	/// Shoot a multiple bullets from owners view point
	/// </summary>
	public virtual void ShootBullets( int numBullets, float spread, float force, float damage, float bulletSize )
	{
		var pos = Owner.EyePos;
		var dir = Owner.EyeRot.Forward;

		for ( int i = 0; i < numBullets; i++ )
		{
			ShootBullet( pos, dir, spread, force / numBullets, damage, bulletSize );
		}
	}

	public override bool CanPrimaryAttack()
    {
        if ( UseClip1 && Clip1 <= 0 ){if(!IsReloading)Reload(); return false;};
        if ( !Owner.IsValid() || !Input.Down( InputButton.Attack1 ) ) return false;

        var rate = PrimaryRate;
        if ( rate <= 0 ) return true;

        return TimeSincePrimaryAttack > (1 / rate);
    }

    public override bool CanSecondaryAttack()
    {
        if ( UseClip2 && Clip2 <= 0 ){if(!IsReloading)Reload(); return false;};
        if ( !Owner.IsValid() || !Input.Down( InputButton.Attack2 ) ) return false;

        var rate = SecondaryRate;
        if ( rate <= 0 ) return true;

        return TimeSinceSecondaryAttack > (1 / rate);
    }

	protected override void OnPhysicsCollision( CollisionEventData eventData )
	{
		if(eventData.PreVelocity.Length < 90f)return;
		string nearestBone = null;
		float boneDist = float.MaxValue;
		if(eventData.Entity is AnimEntity ae){
			for(int i=0; i<ae.BoneCount; i++){
				var d = ae.GetBoneTransform(i).Position.Distance(eventData.Pos);
				if(d < boneDist){
					boneDist = d;
					nearestBone = ae.GetBoneName(i);
				}
			}
		}
		eventData.Entity.TakeDamage(
		DamageInfo.Generic(PhysicsBody.Velocity.Length/9f)
			.WithPosition(eventData.Pos)
			.WithForce(eventData.PreVelocity / 2f)
			.WithWeapon(this)
			.WithAttacker(thrower)
		);
	}

}
