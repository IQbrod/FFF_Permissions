using Rocket.API;

namespace Permission
{
    public class PermissionConfiguration : IRocketPluginConfiguration
    {
        /* DIRECTORIES : "_" is a node into dataPath */
        public string dataPath;
        public string _playersRep;
        public string _kitRep;
        public string _groupRep;
        public string fileFormat;

        /* FILE PATTERN */
        public string fpMajorBeg;
        public string fpMajorEnd;
        public string fpKitBeg;
        public string fpKitEnd;
        public string fpBanDurBeg;
        public string fpBanDurEnd;
        public string fpBanAdmBeg;
        public string fpBanAdmEnd;
        public string fpBanAdmMid;
        public string fpBanDateBeg;
        public string fpBanDateEnd;

        public string trKit;
        public string trCurrentKit;
        public string trDuration;
        public string trBan;
        public string trColor;
        public string trFils;
        public string trIncome;
        public string trPerm;
        public string trMember;
        public string trGroup;

        public string ServerName;

        public void LoadDefaults() {
            /* This part is a secret */
			
			/* ASSIGN PATH AND ENCODING HERE */
        }
    }
}