using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Permissions;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using Rocket.API;
using Rocket.Core;
using System.Collections.Generic;

namespace Permission
{
    public class Permission : RocketPlugin<PermissionConfiguration>
    {
        public PermissionManager PermissionManager;
        public PermissionConfiguration PermissionConfiguration;
        public static Permission Instance;
        public static Dictionary<CSteamID, string> Players = new Dictionary<CSteamID, string>();
        static IRocketPermissionsProvider OriginalPermissions;

        protected override void Load()
        {
            Instance = this;
            PermissionManager = new PermissionManager();
            PermissionConfiguration = new PermissionConfiguration();

            OriginalPermissions = R.Permissions;
            R.Permissions = PermissionManager;

            PermissionConfiguration.LoadDefaults();
            UnturnedPermissions.OnJoinRequested += Events_OnJoinRequested;
            U.Events.OnPlayerConnected += RocketServerEvents_OnPlayerConnected;
        }

        protected override void Unload()
        {
            UnturnedPermissions.OnJoinRequested -= Events_OnJoinRequested;
            U.Events.OnPlayerConnected -= RocketServerEvents_OnPlayerConnected;
            R.Permissions = OriginalPermissions;
        }

        /** Dictionnary **/

        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList()
                {
                    {"command_buykit_kit_inexistant" , "Kit inexistant"},
                    //{"command_buykit_valid" , "SUCCESS BITCH <3"},
                    {"command_buykit_arg" , "Arguments Invalides : ./help buykit"},
                    {"command_ban_missing_player" , "Aucun Joueur : ./help ban"},
                    {"command_ban_default_reason" , "Aucune Raison Spécifiée"},
                    {"command_ban_player_not_found", "Joueur Inconnu" }
                };
            }
        }

        /** Rocket Edit **/

        void RocketServerEvents_OnPlayerConnected(UnturnedPlayer player)
        {
            if (!Players.ContainsKey(player.CSteamID))
                Players.Add(player.CSteamID, player.CharacterName);
            if (Instance.PermissionManager.isBanned(player.CSteamID) && (!player.IsAdmin))
            {
                string banned = Instance.PermissionManager.getBanMessage(player.CSteamID);
                Provider.ban(player.CSteamID, banned, 1);
                Provider.kick(player.CSteamID, banned);
                //rejection = ESteamRejection.AUTH_PUB_BAN;
            }
            PermissionManager.AddPlayerToGroup(PermissionManager.GetGroups(player, false)[0].Id, player);
        }

        public void Events_OnJoinRequested(CSteamID player, ref ESteamRejection? rejection)
        {
            /*try
            {
                
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }*/
        }

        /** Other Methods **/
        public static KeyValuePair<CSteamID, string> GetPlayer(string search)
        {
            foreach (KeyValuePair<CSteamID, string> pair in Players)
            {
                if (pair.Key.ToString().ToLower().Contains(search.ToLower()) || pair.Value.ToLower().Contains(search.ToLower()))
                {
                    return pair;
                }
            }
            return new KeyValuePair<CSteamID, string>(new CSteamID(0), null);
        }
    }
}