using System.Collections.Generic;
using Sandbox;
using Sandbox.UI;

public class CommandKill : Command {
    public override string Name => "Kill";
    public override string Category => "Fun";

    public override bool Run(Player executor, IEnumerable<string> args, bool silent){
        executor.Health = 1;
        (executor as SmashPlayer).GodMode = false;
        executor.OnKilled();
        ChatBox.AddChatEntry(AdminCore.SeeSilent(executor, silent), "white", "", $"⚠️ {executor.GetClientOwner().Name} killed themself."); //avatar:{executor.GetClientOwner().SteamId}
        return true;
    }
}