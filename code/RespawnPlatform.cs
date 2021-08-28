using Sandbox;

public partial class RespawnPlatform : Prop {
    public TimeSince firstCreated {get; set;} = 0f;
    public override void Spawn(){
        SetModel("models/citizen_props/coin01.vmdl");
        Scale = 3f;
        SetupPhysicsFromModel(PhysicsMotionType.Static);
        firstCreated = 0f;
    }

    [Event.Tick]
    public void AwaitDestruction(){
        if(firstCreated > 10f){
            if(IsServer)Delete();
            firstCreated = 0f; // Just incase it tries to run it twice.
            return;
        }
    }
}