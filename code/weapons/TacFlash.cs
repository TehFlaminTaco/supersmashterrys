using Sandbox;

[Library( "weapon_tacflash", Title = "TacFlash", Spawnable = true )]
public class TacFlash : Weapon {
    public override void Spawn()
	{
		base.Spawn();
		SetModel( "models/citizen_props/crowbar01.vmdl" );
	}

    public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetParam( "holdtype", 4 ); // TODO this is shit
		anim.SetParam( "aimat_weight", 1.0f );
		anim.SetParam( "holdtype_pose_hand", 4.0f );
	}
}