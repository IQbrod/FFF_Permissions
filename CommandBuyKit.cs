using System.Collections.Generic;
using Rocket.API;
using System.IO;
using System;
using System.Text;
using Rocket.Unturned.Chat;
using fr34kyn01535.Uconomy;

namespace Permission
{
    public class CommandBuyKit : IRocketCommand
    {
        public AllowedCaller AllowedCaller
        {
            get { return AllowedCaller.Player; }
        }

        public string Name
        {
            get { return "buykit"; }
        }

        public string Help
        {
            get { return "Buy a Kit"; }
        }

        public string Syntax
        {
            get { return "<kitname>"; }
        }

        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public List<string> Permissions
        {
            get { return new List<string>() {"Permission.buykit"}; }
        }

        public void Execute(IRocketPlayer caller, params string[] command)
        {
            if (command.Length == 1)
            {
                /** KIT FILE DOES NOT EXIST **/
                string kitFilePath = Permission.Instance.PermissionConfiguration.dataPath + Permission.Instance.PermissionConfiguration._kitRep + command[0].ToLower() + Permission.Instance.PermissionConfiguration.fileFormat;
                if (!File.Exists(kitFilePath))
                {
                    UnturnedChat.Say(caller, Permission.Instance.Translate("command_buykit_kit_inexistant"));
                    return;
                }

                string playerFilePath = Permission.Instance.PermissionConfiguration.dataPath + Permission.Instance.PermissionConfiguration._playersRep + caller.Id + Permission.Instance.PermissionConfiguration.fileFormat;

                /* -- Kit Variable -- */
                DateTime d = DateTime.Now.AddMinutes(int.Parse(Permission.Instance.PermissionManager.GetTimeKit(command[0].ToLower())));
                string s = Permission.Instance.PermissionConfiguration.fpMajorBeg
                           + Permission.Instance.PermissionConfiguration.trKit
                           + Permission.Instance.PermissionConfiguration.fpMajorEnd
                           + " "
                           + command[0].ToLower()
                           + " "
                           + Permission.Instance.PermissionConfiguration.fpKitBeg
                           + d.Year.ToString("0000") + "," + d.Month.ToString("00") + "," + d.Day.ToString("00") + "," + d.Hour.ToString("00") + "," + d.Minute.ToString("00")
                           + Permission.Instance.PermissionConfiguration.fpKitEnd;

                /** PLAYER FILE DOES NOT EXIST **/
                if (!File.Exists(playerFilePath))
                {
                    try
                    {
                        using (FileStream fs = File.Create(playerFilePath))
                        {
                            string t = Environment.NewLine
                                       + Permission.Instance.PermissionConfiguration.fpMajorBeg
                                       + Permission.Instance.PermissionConfiguration.trCurrentKit
                                       + Permission.Instance.PermissionConfiguration.fpMajorEnd
                                       + " "
                                       + command[0].ToLower();
                            Byte[] lByt = new UTF8Encoding(true).GetBytes(s);
                            fs.Write(lByt, 0, lByt.Length);
                            lByt = new UTF8Encoding(true).GetBytes(t);
                            fs.Write(lByt, 0, lByt.Length);
                            //UnturnedChat.Say(caller, Permission.Instance.Translate("command_buykit_valid"));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    return;
                }
                /** PLAYER HASNT THE KIT **/
                string tmp = Permission.Instance.PermissionManager.HasPlayerKit(caller, command[0].ToLower());
                if (tmp.Equals(""))
                {
                    File.AppendAllText(playerFilePath, Environment.NewLine+s);
                    return;
                }
                /** PLAYER HAS KIT **/
                d = new DateTime(int.Parse(tmp.Substring(0, 4)), int.Parse(tmp.Substring(5, 2)), int.Parse(tmp.Substring(8, 2)), int.Parse(tmp.Substring(11, 2)), int.Parse(tmp.Substring(14, 2)), 0);
                if (d < DateTime.Now) d = DateTime.Now;
                d.AddMinutes(int.Parse(Permission.Instance.PermissionManager.GetTimeKit(command[0].ToLower())));
                s = Permission.Instance.PermissionConfiguration.fpMajorBeg
                           + Permission.Instance.PermissionConfiguration.trKit
                           + Permission.Instance.PermissionConfiguration.fpMajorEnd
                           + " "
                           + command[0].ToLower()
                           + " "
                           + Permission.Instance.PermissionConfiguration.fpKitBeg
                           + d.Year.ToString("0000") + "," + d.Month.ToString("00") + "," + d.Day.ToString("00") + "," + d.Hour.ToString("00") + "," + d.Minute.ToString("00")
                           + Permission.Instance.PermissionConfiguration.fpKitEnd;
                string[] arrLine = File.ReadAllLines(playerFilePath);
                arrLine[Permission.Instance.PermissionManager.findLineIntoFile(caller,command[0].ToLower())] = s;
                File.WriteAllLines(playerFilePath, arrLine);
                return;
            }
            else
            {
                UnturnedChat.Say(caller, Permission.Instance.Translate("command_buykit_arg"));
                return;
            }
        }
    }
}