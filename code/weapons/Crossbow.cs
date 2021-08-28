using System;
using Sandbox;

[Library( "weapon_crossbow", Title = "Crossbow", Spawnable = true )]
public partial class Crossbow : Weapon {
    static SoundEvent Fire = new("weapons/rust_crossbow/sounds/crossbow-attack-1.vsnd");
    public override bool UseClip1 => true;
    public override int Clip1Size => 1;
    public override float PrimaryRate => 1;
	public override float ReloadTime => 1.0f;

    public override void Spawn()
	{
		base.Spawn();
		SetModel( "weapons/rust_crossbow/rust_crossbow.vmdl" );
        Clip1 = 1;
	}

    public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetParam( "holdtype", 3 ); // TODO this is shit
		anim.SetParam( "aimat_weight", 1.0f );
	}

    public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;
		
		(Owner as AnimEntity)?.SetAnimBool( "b_attack", true );

		ShootEffects();
		PlaySound( "Crossbow.Fire" );
		//ShootBullet( 0.05f, Force, Damage, 3.0f );
        if(IsServer){
            var bolt = new CrossbowBolt{
                weapon = this
            };
            bolt.Position = Owner.EyePos + Owner.EyeRot.Forward * 30f;
            bolt.Rotation = Owner.EyeRot;
            bolt.ApplyAbsoluteImpulse(Owner.EyeRot.Forward * 30000f);
        }
		Clip1--;
	}
}

[Library]
public partial class CrossbowBolt : Prop {
    float armTime = 0.0f;
    bool stuck = false;
    [Net]
    public Crossbow weapon {get; set;}

    public override void Spawn(){
        base.Spawn();
		SetModel( "weapons/rust_crossbow/rust_crossbow_bolt.vmdl" );
        SetupPhysicsFromSphere(PhysicsMotionType.Dynamic, Vector3.Zero, 4f );
        armTime = Time.Now + 0.1f;
    }
    protected override void OnPhysicsCollision( CollisionEventData eventData )
	{
        if(eventData.Normal.Dot(eventData.PreVelocity.Normal)>0.5f){
            stuck = true;
            Rotation = Rotation.LookAt(eventData.PreVelocity);
            if(eventData.Entity.IsWorld){
                PhysicsBody.BodyType = PhysicsBodyType.Static;
            }else{
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
                    if(nearestBone != null){
                        var pos = ae.GetBoneTransform(ae.GetBoneIndex(nearestBone)).Position;
                        Position = (pos - Position)/100f + pos;
                    }
                }
                SetParent(eventData.Entity, nearestBone);
                eventData.Entity.TakeDamage(
                DamageInfo.Generic(PhysicsBody.Velocity.Length/30f)
                    .WithPosition(eventData.Pos)
                    .WithForce(eventData.PreVelocity / 15f)
                    .WithWeapon(weapon)
                    .WithAttacker(weapon.Owner)
                );

            }
            //Position = eventData.Pos;
        }
		//if(Time.Now > armTime)
		//	Delete();
	}

    [Event.Physics.PostStep]
    public void UpdateRotation(){
        if(Time.Now > armTime + 10f){
            if(IsServer)
                Delete();
        }
        try{
            if(PhysicsBody is null || !PhysicsBody.IsValid())
                return;
            if(!stuck)
                Rotation = Rotation.LookAt(PhysicsBody?.Velocity??Vector3.Zero);
        }catch(Exception){}
    }
}