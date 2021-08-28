using Sandbox;

[Library( "weapon_shotgun", Title = "Shotgun", Spawnable = true )]
partial class Shotgun : Weapon
{
	public override float PrimaryRate => 1;
	public override float SecondaryRate => 1;
	public override float ReloadTime => 0.3f;
	public override int Clip1Size => 4;
	public override bool UseClip1 => true;

	public override void Spawn()
	{
		base.Spawn();
		Clip1 = 4;
		SetModel( "weapons/rust_pumpshotgun/rust_pumpshotgun.vmdl" );
	}

	public override bool CanSecondaryAttack()
    {
        if ( !Owner.IsValid() || !Input.Down( InputButton.Attack2 ) ) return false;
        if ( Clip1 <= 0 ) return false;

        var rate = SecondaryRate;
        if ( rate <= 0 ) return true;

        return TimeSinceSecondaryAttack > (1 / rate);
    }

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		(Owner as AnimEntity)?.SetAnimBool( "b_attack", true );

		//
		// Tell the clients to play the shoot effects
		//
		ShootEffects();
		PlaySound( "rust_pumpshotgun.shoot" );

		//
		// Shoot the bullets
		//
		//ShootBullets( 10, 0.1f, 10.0f, 9.0f, 3.0f );
		for(int i=-4; i<=4; i++){
			ShootBullet(Owner.EyePos, (Owner.EyeRot * Rotation.FromYaw(i*0.5f)).Forward, 0f,  0.8f, 1f, 3f);
		}
		Clip1--;
	}

	public override void AttackSecondary()
	{
		if(Clip1 <= 1){
			AttackPrimary();
			return;
		}
		TimeSincePrimaryAttack = -0.5f;
		TimeSinceSecondaryAttack = -0.5f;

		(Owner as AnimEntity)?.SetAnimBool( "b_attack", true );

		//
		// Tell the clients to play the shoot effects
		//
		DoubleShootEffects();
		PlaySound( "rust_pumpshotgun.shootdouble" );

		//
		// Shoot the bullets
		//
		//ShootBullets( 20, 0.4f, 20.0f, 8.0f, 3.0f );
		// CUSTOM SPREAD
		for(int i=-4; i<=4; i++){
			ShootBullet(Owner.EyePos, (Owner.EyeRot * Rotation.FromYaw(i*0.8f)).RotateAroundAxis(Vector3.Right, -0.5f).Forward, 0f, 0.8f, 2f, 3f);
			ShootBullet(Owner.EyePos, (Owner.EyeRot * Rotation.FromYaw(i*0.8f)).RotateAroundAxis(Vector3.Right, 0.5f).Forward, 0f, 0.8f, 2f, 3f);
		}
		Owner.Velocity += Owner.EyeRot.Forward * -800f;
		Clip1-=2;
	}

	[ClientRpc]
	protected override void ShootEffects()
	{
		Host.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );
		Particles.Create( "particles/pistol_ejectbrass.vpcf", EffectEntity, "ejection_point" );

		ViewModelEntity?.SetAnimBool( "fire", true );

		if ( IsLocalPawn )
		{
			new Sandbox.ScreenShake.Perlin( 1.0f, 1.5f, 2.0f );
		}

		CrosshairPanel?.CreateEvent( "fire" );
	}

	[ClientRpc]
	protected virtual void DoubleShootEffects()
	{
		Host.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );

		ViewModelEntity?.SetAnimBool( "fire_double", true );
		CrosshairPanel?.CreateEvent( "fire" );

		if ( IsLocalPawn )
		{
			new Sandbox.ScreenShake.Perlin( 3.0f, 3.0f, 3.0f );
		}
	}

	public override void OnReloadFinish()
	{
		Clip1++;
		IsReloading = false;
		if(Clip1<Clip1Size)
			Reload();

		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		FinishReload();
	}

	[ClientRpc]
	protected virtual void FinishReload()
	{
		ViewModelEntity?.SetAnimBool( "reload_finished", true );
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetParam( "holdtype", 3 ); // TODO this is shit
		anim.SetParam( "aimat_weight", 1.0f );
	}
}
