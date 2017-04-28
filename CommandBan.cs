using System.Collections.Generic;
using Rocket.API;
using System.IO;
using System;
using System.Text;
using Rocket.Unturned.Chat;
using fr34kyn01535.Uconomy;
using Steamworks;

namespace Permission
{
    public class CommandBan : IRocketCommand
    {
        public AllowedCaller AllowedCaller
        {
            get { return AllowedCaller.Player; }
        }

        public string Name
        {
            get { return "ban"; }
        }

        public string Help
        {
            get { return "Ban a Player"; }
        }

        public string Syntax
        {
            get { return "<player> [reason] [duration _ T/M/D-J/H]"; }
        }

        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public List<string> Permissions
        {
            get { return new List<string>() {"Permission.ban"}; }
        }

        public void Execute(IRocketPlayer caller, params string[] command)
        {
            if (command.Length >= 1)
            {
                DateTime duration = DateTime.Now;
                string reason = "";
                bool isDuration = false;
                KeyValuePair<CSteamID, string> lTarget = Permission.GetPlayer(command[0]);
                /** Target **/
                if (lTarget.Value == null)
                {
                    UnturnedChat.Say(caller, Permission.Instance.Translate("command_ban_player_not_found"));
                    return;
                }
                /** Duration **/
                if (command.Length > 1)
                {
                    if (command[command.Length - 1].Contains("_Y"))
                    {
                        duration.AddYears(int.Parse(command[command.Length - 1].Substring(0, command[command.Length].Length - 2)));
                        isDuration = true;
                    }
                    else if (command[command.Length - 1].Contains("_M"))
                    {
                        duration.AddMonths(int.Parse(command[command.Length - 1].Substring(0, command[command.Length].Length - 2)));
                        isDuration = true;
                    }
                    else if (command[command.Length - 1].Contains("_D") || command[command.Length - 1].Contains("_J"))
                    {
                        duration.AddDays(int.Parse(command[command.Length - 1].Substring(0, command[command.Length].Length - 2)));
                        isDuration = true;
                    }
                    else if (command[command.Length - 1].Contains("_H"))
                    {
                        duration.AddHours(int.Parse(command[command.Length - 1].Substring(0, command[command.Length].Length - 2)));
                        isDuration = true;
                    }
                }
                if (!isDuration)
                {
                    duration.AddYears(500);
                }
                /** Reason **/
                for(int i=1; i<command.Length-1; i++)
                {
                    reason += command[i]+" ";
                }
                if (!isDuration)
                {
                    reason += command[command.Length - 1];
                } else
                {
                    reason = reason.Substring(1, reason.Length - 1); // Last Space
                }

                /** HATERS GONNA HATE BITCHES !! GET READY FOR THE BAN HAMMER **/
                UnturnedChat.Say(caller, reason);

                return;
            }
            else
            {
                UnturnedChat.Say(caller, Permission.Instance.Translate("command_ban_missing_player"));
                return;
            }
        }
    }
}