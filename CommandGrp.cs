using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Chat;

namespace Permission
{
    public class CommandGrp : IRocketCommand
    {
        public AllowedCaller AllowedCaller
        {
            get { return AllowedCaller.Player; }
        }

        public string Name
        {
            get { return "grp"; }
        }

        public string Help
        {
            get { return "Check my Group"; }
        }

        public string Syntax
        {
            get { return ""; }
        }

        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public List<string> Permissions
        {
            get { return new List<string>() {"Permission.grp"}; }
        }

        public void Execute(IRocketPlayer caller, params string[] command)
        {
            UnturnedChat.Say(caller, Permission.Instance.PermissionManager.GetGroups(caller,false)[0].Id);
            return;
        }
    }
}