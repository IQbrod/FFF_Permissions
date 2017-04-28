
using Rocket.API;
using Steamworks;
using System;
using System.IO;
using System.Text;
using Rocket.API.Serialisation;
using System.Collections.Generic;

namespace Permission
{
    public class PermissionManager : IRocketPermissionsProvider
    {
        public bool HasPermission(IRocketPlayer player, List<string> requestedPermissions)
        {
            return true; /** DEBUG **/
            bool hasPerm = true;
            foreach (string perm in requestedPermissions)
            {
                hasPerm &= HasPermission(player, perm);
            }
            return hasPerm;
        }

        public bool HasPermission(IRocketPlayer player, string s)
        {
            return true; /** DEBUG **/
            bool hasPerm = true;
            if (player.IsAdmin)
                return hasPerm;
            /** KITS **/
            if(s.StartsWith("kit."))
            {
                string kName = s.Substring(4).ToLower();
                hasPerm &= HasPlayerValidKit(player, kName);
                string ck = GetPlayerCurrentKit(player);
                if (!(ck.Equals(kName) || ck.Equals("vip")))
                    hasPerm = false;

                if (hasPerm)
                    return hasPerm;
                else {
                    return GroupHasPerm(GetGroups(player, false)[0].Id,s);
                }
            }
            /** PAS KITS */
            else
            {
                string pattern = Permission.Instance.PermissionConfiguration.fpMajorBeg + Permission.Instance.PermissionConfiguration.trPerm + Permission.Instance.PermissionConfiguration.fpMajorEnd + " " + s.ToLower();
                List <string> p = findPatternIntoPlayer(player.Id, pattern);
                if (p.Count > 0)
                    return true;
                else
                {
                    return GroupHasPerm(GetGroups(player, false)[0].Id,s);
                }
            }
        }

        public bool GroupHasPerm(string gID, string perm)
        /** Recursive function */
        {
            List<string> x = findPatternIntoGroup(gID, Permission.Instance.PermissionConfiguration.fpMajorBeg + Permission.Instance.PermissionConfiguration.trPerm + Permission.Instance.PermissionConfiguration.fpMajorEnd + " " + perm);
            if (x.Count > 0)
                return true;
            if (gID.Equals("default"))
                return false;
            else
                return GroupHasPerm(GetGroup(gID).ParentGroup, perm);
        }

        public void Reload() { }

        public RocketPermissionsProviderResult SaveGroup(RocketPermissionsGroup g) { return RocketPermissionsProviderResult.Success; }

        public RocketPermissionsProviderResult RemovePlayerFromGroup(string g, IRocketPlayer p)
        {
            /*if (g != "default") {
                string gPath = Permission.Instance.PermissionConfiguration.dataPath + Permission.Instance.PermissionConfiguration._groupRep + g + Permission.Instance.PermissionConfiguration.fileFormat;
                string pPath = Permission.Instance.PermissionConfiguration.dataPath + Permission.Instance.PermissionConfiguration._playersRep + p.Id + Permission.Instance.PermissionConfiguration.fileFormat;
                if (!File.Exists(gPath)) { return RocketPermissionsProviderResult.GroupNotFound; }
                if (!File.Exists(pPath)) { return RocketPermissionsProviderResult.PlayerNotFound; }
                string gContent = Permission.Instance.PermissionConfiguration.fpMajorBeg + Permission.Instance.PermissionConfiguration.trMember + Permission.Instance.PermissionConfiguration.fpMajorEnd + " " + p.Id;
                string pContent = Permission.Instance.PermissionConfiguration.fpMajorBeg + Permission.Instance.PermissionConfiguration.trGroup + Permission.Instance.PermissionConfiguration.fpMajorEnd + " " + g;
                deleteLine(gPath, gContent);
                deleteLine(pPath, pContent);
            }*/
            return RocketPermissionsProviderResult.Success;
        }

        public void deleteLine(string path, string content)
        /* File must exist @ path */
        {
            string[] lines = System.IO.File.ReadAllLines(path);
            string[] l2 = new string[lines.Length-1];

            int i = 0;
            foreach(string s in lines)
            {
                if (! s.Contains(content))
                {
                    l2[i] = s;
                    i++;
                }
            }
        }

        public RocketPermissionsProviderResult AddPlayerToGroup(string g, IRocketPlayer p)
        {
            /*if (g != "default")
            {
                string gPath = Permission.Instance.PermissionConfiguration.dataPath + Permission.Instance.PermissionConfiguration._groupRep + g + Permission.Instance.PermissionConfiguration.fileFormat;
                string pPath = Permission.Instance.PermissionConfiguration.dataPath + Permission.Instance.PermissionConfiguration._playersRep + p.Id + Permission.Instance.PermissionConfiguration.fileFormat;
                if (!File.Exists(gPath)) { return RocketPermissionsProviderResult.GroupNotFound; }
                string gContent = "\n" + Permission.Instance.PermissionConfiguration.fpMajorBeg + Permission.Instance.PermissionConfiguration.trMember + Permission.Instance.PermissionConfiguration.fpMajorEnd + " " + p.Id;
                string pContent = "\n" + Permission.Instance.PermissionConfiguration.fpMajorBeg + Permission.Instance.PermissionConfiguration.trGroup + Permission.Instance.PermissionConfiguration.fpMajorEnd + " " + g;
                File.AppendAllText(gPath, gContent);
                File.AppendAllText(pPath, pContent);
            }*/
            return RocketPermissionsProviderResult.Success;
        }

        public List<Rocket.API.Serialisation.Permission> GetPermissions(IRocketPlayer player)
        {
            List<string> l = findPatternIntoPlayer(player.Id, Permission.Instance.PermissionConfiguration.fpMajorBeg + Permission.Instance.PermissionConfiguration.trPerm + Permission.Instance.PermissionConfiguration.fpMajorEnd + " ");
            // (name,cd)
            List<Rocket.API.Serialisation.Permission> u = new List<Rocket.API.Serialisation.Permission>();
            int beg = (Permission.Instance.PermissionConfiguration.fpMajorBeg + Permission.Instance.PermissionConfiguration.trColor + Permission.Instance.PermissionConfiguration.fpMajorEnd + " ").Length;
            foreach (string s in l)
            {
                //"[Perm] name cd" --> "name cd"
                int nameS = s.Substring(beg).IndexOf(" ");
                string name = s.Substring(beg, nameS);
                uint cd;
                uint.TryParse(s.Substring(beg + nameS + 1), out cd);
                u.Add(new Rocket.API.Serialisation.Permission(name, cd));

            }
            return u;
        }

        public List<Rocket.API.Serialisation.Permission> GetPermissions(IRocketPlayer player, List<string> a) {  return GetPermissions(player); }

        public List<RocketPermissionsGroup> GetGroups(IRocketPlayer player, bool IncludeParentGroup)
        {
            List<RocketPermissionsGroup> ret = new List<RocketPermissionsGroup>();
            string filePath = Permission.Instance.PermissionConfiguration.dataPath + Permission.Instance.PermissionConfiguration._playersRep + player.Id + Permission.Instance.PermissionConfiguration.fileFormat;
            if (!File.Exists(filePath)) { ret.Add(GetGroup("default")); return ret; }

            string[] lines = System.IO.File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                if (line.Contains(Permission.Instance.PermissionConfiguration.fpMajorBeg + Permission.Instance.PermissionConfiguration.trGroup + Permission.Instance.PermissionConfiguration.fpMajorEnd + " "))
                {
                    int beg = (Permission.Instance.PermissionConfiguration.fpMajorBeg + Permission.Instance.PermissionConfiguration.trGroup + Permission.Instance.PermissionConfiguration.fpMajorEnd + " ").Length;
                    string st = line.Substring(beg).ToLower();
                    ret.Add(GetGroup(st));
                    return ret;
                }
            }
            ret.Add(GetGroup("default"));
            return ret;
        }

        public RocketPermissionsProviderResult AddGroup(RocketPermissionsGroup g)
        {
            /* NEVER USED ==> WebSite --> Parent Group is Child Group in our case */
            // Create the file.
            string pathFile = Permission.Instance.PermissionConfiguration.dataPath + Permission.Instance.PermissionConfiguration._groupRep + g.DisplayName + Permission.Instance.PermissionConfiguration.fileFormat;
            string data = Permission.Instance.PermissionConfiguration.fpMajorBeg + Permission.Instance.PermissionConfiguration.trColor + Permission.Instance.PermissionConfiguration.fpMajorEnd + " " + g.Color.ToString() + "\n"
                    + Permission.Instance.PermissionConfiguration.fpMajorBeg + Permission.Instance.PermissionConfiguration.trFils + Permission.Instance.PermissionConfiguration.fpMajorEnd + " " + g.ParentGroup.ToString() + "\n"
                    + Permission.Instance.PermissionConfiguration.fpMajorBeg + Permission.Instance.PermissionConfiguration.trIncome + Permission.Instance.PermissionConfiguration.fpMajorEnd + " " + "10";
            using (FileStream fs = File.Create(pathFile))
            {
                Byte[] info = new UTF8Encoding(true).GetBytes(data);
                fs.Write(info, 0, info.Length);
            }
            return RocketPermissionsProviderResult.Success;
        }

        public string getChildGroup(string groupID)
        {
            List<string> l = findPatternIntoGroup(groupID, Permission.Instance.PermissionConfiguration.fpMajorBeg + Permission.Instance.PermissionConfiguration.trFils + Permission.Instance.PermissionConfiguration.fpMajorEnd + " ");
            if (l.Count == 0)  { return "default"; }
            return l[0].Substring((Permission.Instance.PermissionConfiguration.fpMajorBeg + Permission.Instance.PermissionConfiguration.trFils + Permission.Instance.PermissionConfiguration.fpMajorEnd + " ").Length);
        }
        public string getColorGroup(string groupID)
        {
            List<string> l = findPatternIntoGroup(groupID, Permission.Instance.PermissionConfiguration.fpMajorBeg + Permission.Instance.PermissionConfiguration.trColor + Permission.Instance.PermissionConfiguration.fpMajorEnd + " ");
            if (l.Count == 0) { return ""; }
            return l[0].Substring((Permission.Instance.PermissionConfiguration.fpMajorBeg + Permission.Instance.PermissionConfiguration.trColor + Permission.Instance.PermissionConfiguration.fpMajorEnd + " ").Length);
        }
        public List<string> getMembersGroup(string groupID) {
            return findPatternIntoGroup(groupID, Permission.Instance.PermissionConfiguration.fpMajorBeg + Permission.Instance.PermissionConfiguration.trMember + Permission.Instance.PermissionConfiguration.fpMajorEnd + " ");
        }
        public List<Rocket.API.Serialisation.Permission> getPermGroup(string groupID) {
            List<string> l = findPatternIntoGroup(groupID, Permission.Instance.PermissionConfiguration.fpMajorBeg + Permission.Instance.PermissionConfiguration.trPerm + Permission.Instance.PermissionConfiguration.fpMajorEnd + " ");
            // (name,cd)
            List <Rocket.API.Serialisation.Permission> u = new List<Rocket.API.Serialisation.Permission>();
            int beg = (Permission.Instance.PermissionConfiguration.fpMajorBeg + Permission.Instance.PermissionConfiguration.trColor + Permission.Instance.PermissionConfiguration.fpMajorEnd + " ").Length;
            foreach (string s in l)
            {
                //"[Perm] name cd" --> "name cd"
                string NCD = s.Substring(beg);
                string name = " ";
                uint cd = 0;
                if (NCD.Contains(" "))
                {
                    name = s.Substring(beg, NCD.IndexOf(" ")-1);
                    uint.TryParse(s.Substring(NCD.IndexOf(" ") + 1), out cd);
                }
                else
                {
                    name = s.Substring(beg);
                }
                u.Add(new Rocket.API.Serialisation.Permission(name,  cd));
                 
            }
            return u;
        }

        public RocketPermissionsGroup GetGroup(string groupID)
        {
            string filePath = Permission.Instance.PermissionConfiguration.dataPath + Permission.Instance.PermissionConfiguration._groupRep + groupID + Permission.Instance.PermissionConfiguration.fileFormat;
            string groupName = "default";
            if (File.Exists(filePath))
            {
                groupName = groupID;
            }
            RocketPermissionsGroup RPG = new RocketPermissionsGroup(groupName, groupName, getChildGroup(groupName),getMembersGroup(groupName),getPermGroup(groupName), getColorGroup(groupName));
            return RPG;
        }

        public List<string> findPatternIntoGroup(string groupID, string pattern)
        {
            List<string> l = new List<string>();
            string filePath = Permission.Instance.PermissionConfiguration.dataPath + Permission.Instance.PermissionConfiguration._groupRep + groupID + Permission.Instance.PermissionConfiguration.fileFormat;

            if (!File.Exists(filePath)) { return l; }

            string[] lines = System.IO.File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                if (line.Contains(pattern))
                {
                    l.Add(line);
                    break;
                }
            }
            return l;
        }

        public List<string> findPatternIntoPlayer(string steamID, string pattern)
        {
            List<string> l = new List<string>();
            string filePath = Permission.Instance.PermissionConfiguration.dataPath + Permission.Instance.PermissionConfiguration._playersRep + steamID + Permission.Instance.PermissionConfiguration.fileFormat;

            if (!File.Exists(filePath)) { return l; }

            string[] lines = System.IO.File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                if (line.Contains(pattern))
                {
                    l.Add(line);
                    break;
                }
            }
            return l;
        }

        public RocketPermissionsProviderResult DeleteGroup(string g)
        {
            /* NEVER USED  ==> WebSite*/
            return RocketPermissionsProviderResult.Success;
        }



        public string HasPlayerKit(IRocketPlayer caller, string kitName)
        /*
             Return YYYY,MM,DD,HH,mm (expiration time) or "" if not 
             Kit may be Invalid
        */
        {
            // Init
            kitName = Permission.Instance.PermissionConfiguration.fpMajorBeg + Permission.Instance.PermissionConfiguration.trKit + Permission.Instance.PermissionConfiguration.fpMajorEnd + " " + kitName + " ";
            string kitFound = "";
            // Find Kit
            string filePath = Permission.Instance.PermissionConfiguration.dataPath + Permission.Instance.PermissionConfiguration._playersRep + caller.Id + Permission.Instance.PermissionConfiguration.fileFormat;

            if (!File.Exists(filePath)) { return ""; }

            string[] lines = System.IO.File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                if (line.Contains(kitName))
                {
                    kitFound = line;
                    break;
                }
            }

            if (kitFound.Equals(""))
            {
                return "";
            }
            // Duration
            return kitFound.Substring(kitFound.IndexOf(Permission.Instance.PermissionConfiguration.fpKitBeg) + 1, 16); // YYYY,MM,DD,HH,mm
        }

        public int findLineIntoFile(IRocketPlayer caller, string kitName)
        /* Return the line number of Kit named kitName */
        {
            kitName = Permission.Instance.PermissionConfiguration.fpMajorBeg + Permission.Instance.PermissionConfiguration.trKit + Permission.Instance.PermissionConfiguration.fpMajorEnd + " " + kitName + " ";
            string filePath = Permission.Instance.PermissionConfiguration.dataPath + Permission.Instance.PermissionConfiguration._playersRep + caller.Id + Permission.Instance.PermissionConfiguration.fileFormat;

            if (!File.Exists(filePath)) { return -1; } // Must be useless ... but anyway

            string[] lines = System.IO.File.ReadAllLines(filePath);
            for(int i=0; i<lines.Length; i++)
            {
                if (lines[i].Contains(kitName))
                {
                    return i;
                }
            }
            return -1;
        }

        public bool HasPlayerValidKit(IRocketPlayer caller, string kitName)
        {
            string d = HasPlayerKit(caller, kitName); // YYYY,MM,DD,HH,mm
            if (d.Equals(""))
            {
                return false;
            }
            DateTime d1 = new DateTime(int.Parse(d.Substring(0,4)), int.Parse(d.Substring(5, 2)), int.Parse(d.Substring(8, 2)), int.Parse(d.Substring(11, 2)), int.Parse(d.Substring(14, 2)), 0);
            return d1 >= DateTime.Now;
        }

        public string GetPlayerCurrentKit(IRocketPlayer caller)
        {
            string filePath = Permission.Instance.PermissionConfiguration.dataPath + Permission.Instance.PermissionConfiguration._playersRep + caller.Id + Permission.Instance.PermissionConfiguration.fileFormat;
            string myStr = "";

            if (!File.Exists(filePath)) {return "";}

            string[] lines = System.IO.File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                if (line.Contains(Permission.Instance.PermissionConfiguration.fpMajorBeg + Permission.Instance.PermissionConfiguration.trCurrentKit + Permission.Instance.PermissionConfiguration.fpMajorEnd))
                {
                    myStr = line;
                    break;
                }
            }
            return myStr.Substring(1 + Permission.Instance.PermissionConfiguration.trCurrentKit.Length + Permission.Instance.PermissionConfiguration.fpMajorBeg.Length + Permission.Instance.PermissionConfiguration.fpMajorEnd.Length);
        }

        public string[] ListPlayerKits(IRocketPlayer caller)
        /* Kits may not be valid --> Check it with HasPlayerValidKit */
        {
            string filePath = Permission.Instance.PermissionConfiguration.dataPath + Permission.Instance.PermissionConfiguration._playersRep + caller.Id + Permission.Instance.PermissionConfiguration.fileFormat;
            if (!File.Exists(filePath)) { return new string[] {}; }
            // How Many Kits ?
            string[] lines = System.IO.File.ReadAllLines(filePath);
            int taille = 0;
            foreach (string line in lines)
            {
                if (line.Contains(Permission.Instance.PermissionConfiguration.fpMajorBeg + Permission.Instance.PermissionConfiguration.trKit + Permission.Instance.PermissionConfiguration.fpMajorEnd + " "))
                {
                    taille++;
                }
            }
            // Save Them
            string[] myArr = new string[taille];
            taille = 0;
            foreach (string line in lines)
            {
                if (line.Contains(Permission.Instance.PermissionConfiguration.fpMajorBeg + Permission.Instance.PermissionConfiguration.trKit + Permission.Instance.PermissionConfiguration.fpMajorEnd + " "))
                {
                    int sizePattern = (Permission.Instance.PermissionConfiguration.fpMajorBeg + Permission.Instance.PermissionConfiguration.trKit + Permission.Instance.PermissionConfiguration.fpMajorEnd).Length + 1;
                    myArr[taille] = line.Substring(sizePattern,line.IndexOf(Permission.Instance.PermissionConfiguration.fpKitBeg)-sizePattern-1);
                }
            }
            return myArr;
        }

        public string GetTimeKit(string kitName)
        {
            string kitFilePath = Permission.Instance.PermissionConfiguration.dataPath + Permission.Instance.PermissionConfiguration._kitRep + kitName.ToLower() + Permission.Instance.PermissionConfiguration.fileFormat;
            if (!File.Exists(kitFilePath)) { return ""; }
            string[] lines = System.IO.File.ReadAllLines(kitFilePath);
            foreach (string line in lines)
            {
                if (line.Contains(Permission.Instance.PermissionConfiguration.fpMajorBeg + Permission.Instance.PermissionConfiguration.trDuration + Permission.Instance.PermissionConfiguration.fpMajorEnd + " "))
                {
                    int sizePattern = (Permission.Instance.PermissionConfiguration.fpMajorBeg + Permission.Instance.PermissionConfiguration.trDuration + Permission.Instance.PermissionConfiguration.fpMajorEnd).Length + 1;
                    return line.Substring(sizePattern);
                }
            }
            return "";
        }

        public bool isBanned(CSteamID playerID)
        {
            string banDur = "";
            string filePath = Permission.Instance.PermissionConfiguration.dataPath + Permission.Instance.PermissionConfiguration._playersRep + playerID.ToString() + Permission.Instance.PermissionConfiguration.fileFormat;

            if (!File.Exists(filePath)) { return false; }

            string[] lines = System.IO.File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                if (line.Contains(Permission.Instance.PermissionConfiguration.fpMajorBeg + Permission.Instance.PermissionConfiguration.trBan + Permission.Instance.PermissionConfiguration.fpMajorEnd + " "))
                {
                    banDur = line;
                    break;
                }
            }

            if (banDur.Equals(""))
            {
                return false;
            }
            // Duration
            string d = banDur.Substring(banDur.IndexOf(Permission.Instance.PermissionConfiguration.fpBanDurBeg) + 1, 16); // YYYY,MM,DD,HH,mm
            DateTime d1 = new DateTime(int.Parse(d.Substring(0, 4)), int.Parse(d.Substring(5, 2)), int.Parse(d.Substring(8, 2)), int.Parse(d.Substring(11, 2)), int.Parse(d.Substring(14, 2)), 0);
            return d1 >= DateTime.Now;
        }

        public string getBanMessage(CSteamID playerID)
        /* Prec: isBanned(playerID) == true */
        {
            /* Read File */
            string filePath = Permission.Instance.PermissionConfiguration.dataPath + Permission.Instance.PermissionConfiguration._playersRep + playerID.ToString() + Permission.Instance.PermissionConfiguration.fileFormat;
            string[] lines = System.IO.File.ReadAllLines(filePath);
            string tmp = "";
            foreach (string line in lines)
            {
                if (line.Contains(Permission.Instance.PermissionConfiguration.fpMajorBeg + Permission.Instance.PermissionConfiguration.trBan + Permission.Instance.PermissionConfiguration.fpMajorEnd + " "))
                {
                    tmp = line;
                    break;
                }
            }

            /* Get data */
            string adm = tmp.Substring(tmp.IndexOf(Permission.Instance.PermissionConfiguration.fpBanAdmMid)+2, tmp.IndexOf(Permission.Instance.PermissionConfiguration.fpBanAdmEnd) - tmp.IndexOf(Permission.Instance.PermissionConfiguration.fpBanAdmMid) - 2);
            string date = tmp.Substring(tmp.IndexOf(Permission.Instance.PermissionConfiguration.fpBanDateBeg) + 1, 16);
            date = date.Substring(8, 2)+"/"+ date.Substring(5, 2)+"/"+date.Substring(0,4)+" "+date.Substring(11, 2)+":"+ date.Substring(14, 2);
            string duration = tmp.Substring(tmp.IndexOf(Permission.Instance.PermissionConfiguration.fpBanDurBeg) + 1, 16);
            duration = duration.Substring(8, 2) + "/" + duration.Substring(5, 2) + "/" + duration.Substring(0, 4) + " " + duration.Substring(11, 2) + ":" + duration.Substring(14, 2);
            string serv = tmp.Substring(tmp.IndexOf(Permission.Instance.PermissionConfiguration.fpBanDateEnd) + 2);
            string reason = tmp.Substring(tmp.IndexOf(Permission.Instance.PermissionConfiguration.fpMajorEnd) + 2, tmp.IndexOf(Permission.Instance.PermissionConfiguration.fpBanAdmBeg) - tmp.IndexOf(Permission.Instance.PermissionConfiguration.fpMajorEnd)-3);
            if (reason.Equals("")) { Permission.Instance.Translate("command_ban_default_reason"); }
            return reason + " | " + adm + " sur " + serv + " " + " le " + date
                    + Environment.NewLine
                    + "Contestez ici: http://steamcommunity.com/groups/FFF-Unturned/discussions/0/" + " (Unban le " + duration + ")";
        }
    }
}