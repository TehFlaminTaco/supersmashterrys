using System;
using System.Collections.Generic;
using Sandbox;

[Library( "weapon_pistol", Title = "Pistol", Spawnable = true, Group = "Weapon" )]
partial class Pistol : Weapon
{
	public override float PrimaryRate => 3.0f;

	public override bool UseClip1 => true;
    public override int Clip1Size => 12;

	public virtual float Damage => 9.0f;
	public virtual float Force => 1.5f;

	[Net, Predicted]
	public int shotIndex {get; set;} = 0;
	
	public override float ReloadTime => 1.4f;

	public override void Spawn()
	{
		base.Spawn();

		Clip1 = 12;

		SetModel( "weapons/rust_pistol/rust_pistol.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
	}

	public override void Simulate( Client owner ){
		if(shotIndex > 0){
			if(TimeSincePrimaryAttack > 0.05f){
				TimeSincePrimaryAttack = 0f;
				FireShot();
			}
		}
		base.Simulate( owner );
	}

	public override bool CanPrimaryAttack()
	{
		return base.CanPrimaryAttack() && Input.Down( InputButton.Attack1 ) && Clip1 > 0;
	}

	public override bool CanSecondaryAttack(){
		if ( !Owner.IsValid() || !Input.Down( InputButton.Attack2 ) ) return false;
        if ( Clip1 <= 0 ) return false;

        var rate = PrimaryRate;
        if ( rate <= 0 ) return true;

        return TimeSincePrimaryAttack > (1 / rate);
	}

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;
		
		(Owner as AnimEntity)?.SetAnimBool( "b_attack", true );
		FireShot();
		shotIndex = 2;
	}

	public override void AttackSecondary()
	{
		TimeSincePrimaryAttack = -1f;
		
		(Owner as AnimEntity)?.SetAnimBool( "b_attack", true );
		//Clip1--;
		ShootEffects();
		PlaySound( "rust_pistol.shoot" );
		var n = Math.Min(Clip1, 5);
		float radius = 1f;
		for(var i=0;i<n;i++){
			double up = Math.Sin(i*(Math.Tau/n));
			double right = Math.Cos(i*(Math.Tau/n));
			ShootBullet( Owner.EyePos, (
				Owner.EyeRot
				.RotateAroundAxis(Vector3.Right, (float)right*radius)
				.RotateAroundAxis(Vector3.Up, (float)up*radius)
			).Forward, .0f, Force, Damage, 3.0f );
		}
		Owner.Velocity -= Owner.EyeRot.Forward * 50f * n;
		Clip1-=n;
	}

	public void FireShot(){
		shotIndex--;
		if(Clip1>0){
			ShootEffects();
			PlaySound( "rust_pistol.shoot" );
			ShootBullet( 0.05f, Force, Damage, 3.0f );
			Clip1--;
		}
	}
}
