using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

class TargetHealth : Panel {

    public Dictionary<Player, HealthIndicator> activeHIs = new();
    public override void Tick(){
        //base.Tick();
        var deleteList = new List<Player>();
        deleteList.AddRange(activeHIs.Keys);
        foreach(var player in Entity.All.OfType<Player>()){
            if(UpdateIndicator(player)){
                deleteList.Remove(player);
            }
        }

        foreach(var player in deleteList){
            activeHIs[player].Delete();
            activeHIs.Remove(player);
        }
    }

    public bool UpdateIndicator(Player p){
        if(p is null || !p.IsValid())
            return false;
        if(!activeHIs.ContainsKey(p))
            AddChild(activeHIs[p]=new(p));
        activeHIs[p].Tick();
        return true;
    }
}

class HealthIndicator : Panel {
    public Player target;
    Label text;
    public HealthIndicator(Player target){
        this.target = target;
        text = Add.Label("0%");
    }

    public override void Tick(){
        text.Text = target.Health.CeilToInt()+"%";
        var pos = new Vector2((target.Position+Vector3.Up*80).ToScreen());
        text.SetClass("dying", target.Health.CeilToInt() >= 100);
        this.Style.Left = Length.Fraction(pos.x);
        this.Style.Top = Length.Fraction(pos.y);
        this.Style.ZIndex = (-target.Position.Distance((Local.Pawn.Camera as Camera).Pos)).CeilToInt();
        this.Style.FontSize = Length.Pixels((10000f/target.Position.Distance((Local.Pawn.Camera as Camera).Pos)).CeilToInt());
        this.Style.Dirty();
    }
}