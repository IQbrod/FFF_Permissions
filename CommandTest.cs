using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Chat;

namespace Permission
{
    public class CommandTest : IRocketCommand
    {
        public AllowedCaller AllowedCaller
        {
            get { return AllowedCaller.Player; }
        }

        public string Name
        {
            get { return "test"; }
        }

        public string Help
        {
            get { return "TestCommand"; }
        }

        public string Syntax
        {
            get { return "<player>"; }
        }

        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public List<string> Permissions
        {
            get { return new List<string>(); }
        }

        public void Execute(IRocketPlayer caller, params string[] command)
        {
            if (Permission.Instance.PermissionManager.HasPlayerValidKit(caller, "Test")) {
                UnturnedChat.Say(caller, "Vous possedez le kit test");
            } else {
                UnturnedChat.Say(caller, "Pas Kit Test");
            }
        }
    }
}