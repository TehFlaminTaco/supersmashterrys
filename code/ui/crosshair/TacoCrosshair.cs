using Sandbox;
using Sandbox.UI;

public class TacoCrosshair : Panel {
    Panel chevA;
    Panel chevB;
    Panel chevC;
    Panel chevD;
    public TacoCrosshair(){
        chevA = Add.Panel("chevA");
        chevA.AddClass("chev");
        chevB = Add.Panel("chevB");
        chevB.AddClass("chev");
        chevC = Add.Panel("chevC");
        chevC.AddClass("chev");
        chevD = Add.Panel("chevD");
        chevD.AddClass("chev");
    }
    
    public override void Tick(){
        if(Local.Pawn is not SmashPlayer ply)
            return;
        var startPos = Local.Pawn.EyePos;
        var dir = Local.Pawn.EyeRot.Forward;
        var tr = Trace.Ray( startPos, startPos + dir * 3000f )
            .Ignore( Local.Pawn )
            .Run();
        var pos = new Vector2(tr.EndPos.ToScreen());
        this.Style.Left = Length.Fraction(pos.x);
        this.Style.Top = Length.Fraction(pos.y);
        this.Style.Dirty();
    }
}