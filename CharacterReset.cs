using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TShockAPI;
using TShockAPI.DB;
using Terraria;
using Terraria.Localization;
using TerrariaApi.Server;

namespace CharacterReset
{
    [ApiVersion(2, 1)]
    public class CharacterReset : TerrariaPlugin
    {
        #region Plugin Info
        public override Version Version
        {
            get { return new Version("1.4"); }
        }
        public override string Name
        {
            get { return "CharacterReset"; }
        }
        public override string Author
        {
            get { return "Bippity, updated by moisterrific"; }
        }
        public override string Description
        {
            get { return "Reset your character back to default."; }
        }
        public CharacterReset(Main game)
            : base(game)
        {
            Order = 4;
        }
        #endregion

        #region Initialize/Dispose
        public List<string> PlayerList = new List<string>();
        public List<NetItem> StarterItems = new List<NetItem>();
        public int startHealth, startMana;

        public override void Initialize()
        {
            ServerApi.Hooks.NetGetData.Register(this, OnGetData);

            Commands.ChatCommands.Add(new Command(new List<string>() { "character.reset.stats", "character.reset.inventory", "character.reset.quests" }, ResetCharacter, "resetcharacter"));
            Commands.ChatCommands.Add(new Command("character.reset.players", ResetPlayers, "resetplayers"));
            Commands.ChatCommands.Add(new Command("character.reset.player", ResetPlayer, "resetplayer"));

            if (Main.ServerSideCharacter)
            {
                //StarterItems = TShock.ServerSideCharacterConfig.StartingInventory;
                StarterItems = TShock.ServerSideCharacterConfig.Settings.StartingInventory;

                if (TShock.ServerSideCharacterConfig.Settings.StartingHealth > 500)
                    startHealth = 500;
                else if (TShock.ServerSideCharacterConfig.Settings.StartingHealth < 100)
                    startHealth = 100;
                else
                    startHealth = TShock.ServerSideCharacterConfig.Settings.StartingHealth;

                if (TShock.ServerSideCharacterConfig.Settings.StartingMana > 200)
                    startMana = 200;
                else if (TShock.ServerSideCharacterConfig.Settings.StartingMana < 20)
                    startMana = 20;
                else
                    startMana = TShock.ServerSideCharacterConfig.Settings.StartingMana;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.NetGetData.Deregister(this, OnGetData);
            }
            base.Dispose(disposing);
        }
        #endregion

        #region CharacterReset
        private void OnGetData(GetDataEventArgs args)
        {
            if (args.MsgID == PacketTypes.TileGetSection)
            {
                if (Netplay.Clients[args.Msg.whoAmI].State == 2)
                {
                    CleanInventory(args.Msg.whoAmI);
                }
            }
        }

        private void CleanInventory(int who) //original method from ClearInvSSC to prevent exploits
        {
            if (!Main.ServerSideCharacter)
            {
                //TShock.Log.ConsoleError("[CharacterReset] This plugin will not work properly with ServerSidedCharacters disabled.");
                TShock.Log.ConsoleError("[CharacterReset]: SSC (Server Side Characters) must be enabled for this plugin to work!");
                return;
            }

            if (!TShock.Players[who].IsLoggedIn)
            {
                var player = TShock.Players[who];
                player.TPlayer.SpawnX = -1;
                player.TPlayer.SpawnY = -1;
                player.sX = -1;
                player.sY = -1;

                ClearInventory(player);
            }
        }

        private void ClearInventory(TSPlayer player) //The inventory clearing method from ClearInvSSC
        {
            for (int i = 0; i < NetItem.MaxInventory; i++)
            {
                if (i < NetItem.InventorySlots) //Main Inventory
                {
                    player.TPlayer.inventory[i].netDefaults(0);
                }
                else if (i < NetItem.InventorySlots + NetItem.ArmorSlots) //Armor&Accessory slots
                {
                    var index = i - NetItem.InventorySlots;
                    player.TPlayer.armor[index].netDefaults(0);
                }
                else if (i < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots) //Dye Slots
                {
                    var index = i - (NetItem.InventorySlots + NetItem.ArmorSlots);
                    player.TPlayer.dye[index].netDefaults(0);
                }
                else if (i < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots) //Misc Equip slots
                {
                    var index = i - (NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots);
                    player.TPlayer.miscEquips[index].netDefaults(0);
                }
                else if (i < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots) // MIsc dye slots
                {
                    var index = i - (NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots);
                    player.TPlayer.miscDyes[index].netDefaults(0);
                }
                else if (i < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots + NetItem.PiggySlots) //piggy Bank
                {
                    var index = i - (NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots);
                    player.TPlayer.bank.item[index].netDefaults(0);
                }
                else if (i < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots + NetItem.PiggySlots + NetItem.SafeSlots) //safe Bank
                {
                    var index = i - (NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots + NetItem.PiggySlots);
                    player.TPlayer.bank2.item[index].netDefaults(0);
                }
                else if (i < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots + NetItem.PiggySlots + NetItem.SafeSlots + NetItem.ForgeSlots) //Defender's Forge
                {
                    var index = i - (NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots + NetItem.PiggySlots + NetItem.SafeSlots);
                    player.TPlayer.bank3.item[index].netDefaults(0);
                }
                else if (i < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots + NetItem.PiggySlots + NetItem.SafeSlots + NetItem.ForgeSlots + NetItem.VoidSlots) // Void Bank
                {
                    var index = i - (NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots + NetItem.PiggySlots + NetItem.SafeSlots + NetItem.ForgeSlots);
                    player.TPlayer.bank4.item[index].netDefaults(0);
                }
                else
                {
                    player.TPlayer.trashItem.netDefaults(0);
                }
            }

            for (int k = 0; k < NetItem.MaxInventory - (NetItem.SafeSlots + NetItem.PiggySlots + NetItem.ForgeSlots); k++) //clear all slots excluding bank slots, bank slots cleared in ResetBanks method
            {
                NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.Empty, player.Index, (float)k, 0f, 0f, 0);
            }

            var trashSlot = NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots + NetItem.PiggySlots + NetItem.SafeSlots;
            NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.Empty, player.Index, (float)trashSlot, 0f, 0f, 0); //trash slot

            for (int k = 0; k < Player.maxBuffs; k++)
            {
                player.TPlayer.buffType[k] = 0;
            }

            NetMessage.SendData((int)PacketTypes.PlayerInfo, -1, -1, NetworkText.FromLiteral(player.Name), player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData((int)PacketTypes.PlayerMana, -1, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData((int)PacketTypes.PlayerHp, -1, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData((int)PacketTypes.PlayerBuff, -1, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);

            for (int k = 0; k < NetItem.MaxInventory - (NetItem.SafeSlots + NetItem.PiggySlots); k++)
            {
                NetMessage.SendData((int)PacketTypes.PlayerSlot, player.Index, -1, NetworkText.Empty, player.Index, (float)k, 0f, 0f, 0);
            }
            NetMessage.SendData((int)PacketTypes.PlayerSlot, player.Index, -1, NetworkText.Empty, player.Index, (float)trashSlot, 0f, 0f, 0);

            for (int k = 0; k < Player.maxBuffs; k++)
            {
                player.TPlayer.buffType[k] = 0;
            }

            NetMessage.SendData((int)PacketTypes.PlayerInfo, player.Index, -1, NetworkText.FromLiteral(player.Name), player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData((int)PacketTypes.PlayerMana, player.Index, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData((int)PacketTypes.PlayerHp, player.Index, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData((int)PacketTypes.PlayerBuff, player.Index, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
        }

        private void ResetCharacter(CommandArgs args)
        {
            var cmd = TShock.Config.Settings.CommandSpecifier;
            TSPlayer player = args.Player;
            if (player != null && player.RealPlayer)
            {
                if (Main.ServerSideCharacter)
                {
                    if (args.Parameters.Count == 0)
                    {
                        //player.SendErrorMessage("Invalid syntax! Proper syntax: /resetcharacter <all|stats|inventory|quests|banks>");
                        player.SendErrorMessage($"Invalid syntax! Proper syntax: {cmd}resetcharacter <all|stats|inventory|quests|banks>");
                        return;
                    }

                    var subcmd = args.Parameters[0].ToLower();

                    switch (subcmd)
                    {
                        case "all": 
                            try
                            {
                                if (player.Group.HasPermission("characterreset.stats") && player.Group.HasPermission("characterreset.inventory") && player.Group.HasPermission("characterreset.quests"))
                                {
                                    ResetStats(player);
                                    ResetInventory(player);
                                    ResetQuests(player);
                                    ResetBanks(player);
                                    player.SendSuccessMessage("Your character was reset to default!");
                                }
                                else
                                {
                                    player.SendErrorMessage("You don't have permission to reset everything.");
                                }
                            }
                            catch (Exception ex)
                            {
                                TShock.Log.ConsoleError(ex.ToString());
                                player.SendErrorMessage("An error occurred while resetting all!");
                            }
                            break;

                        case "stats":
                            try
                            {
                                if (player.Group.HasPermission("characterreset.stats"))
                                {
                                    ResetStats(player);
                                    player.SendSuccessMessage("Your Health & Mana were reset to default!");
                                }
                                else
                                {
                                    player.SendErrorMessage("You don't have permission to reset your stats.");
                                }
                            }
                            catch (Exception ex)
                            {
                                TShock.Log.ConsoleError(ex.ToString());
                                player.SendErrorMessage("An error occurred while resetting stats!");
                            }
                            break;

                        case "inventory":
                            try
                            {
                                if (player.Group.HasPermission("characterreset.inventory"))
                                {
                                    ResetInventory(player);
                                    player.SendSuccessMessage("Your inventory was reset to default!");
                                }
                                else
                                {
                                    player.SendErrorMessage("You don't have permission to reset your inventory.");
                                }
                            }
                            catch (Exception ex)
                            {
                                TShock.Log.ConsoleError(ex.ToString());
                                player.SendErrorMessage("An error occurred while resetting inventory!");
                            }
                            break;

                        case "quests":
                            try
                            {
                                if (player.Group.HasPermission("characterreset.quests"))
                                {
                                    ResetQuests(player);
                                    player.SendSuccessMessage("Your quests were reset to 0!");
                                }
                                else
                                {
                                    player.SendErrorMessage("You don't have permission to reset your quests.");
                                }
                            }
                            catch (Exception ex)
                            {
                                TShock.Log.ConsoleError(ex.ToString());
                                player.SendErrorMessage("An error occurred while resetting quests!");
                            }
                            break;

                        case "banks":
                            try
                            {
                                if (player.Group.HasPermission("characterreset.banks"))
                                {
                                    ResetBanks(player);
                                    player.SendSuccessMessage("Your banks have been reset!");
                                }
                                else
                                {
                                    player.SendErrorMessage("You don't have permission to reset your banks.");
                                }
                            }
                            catch (Exception ex)
                            {
                                TShock.Log.ConsoleError(ex.ToString());
                                player.SendErrorMessage("An error occurred while resetting banks!");
                            }
                            break;

                        default:
                            //player.SendErrorMessage("Invalid syntax! Proper syntax: /resetcharacter <all|stats|inventory|quests|banks>");
                            player.SendErrorMessage($"Invalid syntax! Proper syntax: {cmd}resetcharacter <all|stats|inventory|quests|banks>");
                            break;

                    }
                }
                else
                    player.SendErrorMessage("SSC is not enabled on this server! \nThis plugin will only work if SSC (Server Side Characters) is enabled!");
            }
            else
            {
                player.SendErrorMessage("Server console cannot use this command, because it is not a real player!");
            }
        }

        public void ResetPlayer(CommandArgs args)
        {
            var cmd = TShock.Config.Settings.CommandSpecifier;
            TSPlayer player = args.Player;

            if (Main.ServerSideCharacter)
            {
                if (args.Parameters.Count < 2)
                {
                    player.SendErrorMessage($"Invalid syntax! Proper syntax: {cmd}resetplayer <username> <all|stats|inventory|quests>");
                    return;
                }

                var username = args.Parameters[0];
                var subcmd = args.Parameters[1].ToLower();
                IDbConnection db = TShock.CharacterDB.database;
                bool online = true;
                int userid = 0;

                List<TSPlayer> players = TSPlayer.FindByNameOrID(username);
                if (players.Count < 1) // If player is not found online
                {
                    online = false;
                    //player.SendWarningMessage("Player not found online. Searching database...");
                    player.SendInfoMessage("Player not found online. Searching database... (Offline queries are case sensitive)");
                }
                else if (players.Count > 1)
                {
                    args.Player.SendMultipleMatchError(players.Select(p => p.Name));        
                    return;
                }

                if (!online)
                {
                    if (TShock.UserAccounts.GetUserAccountByName(username) == null)
                    {
                        //player.SendErrorMessage("Username \"{0}\" not found in database. (Usernames are case-sensitive)", username);
                        player.SendErrorMessage($"Username {username} not found in database. (Usernames are case-sensitive)");
                        return;
                    }
                    else
                    {
                        //userid = TShock.Users.GetUserByName(username).ID; // Can't reset offline players unless using resetplayers to reset ALL players
                        userid = TShock.UserAccounts.GetUserAccountID(username); // works but is case sensitive 
                    }
                }

                switch (subcmd)
                {
                    case "all":
                        try
                        {
                            if (online)
                            {
                                ResetStats(players[0]);
                                ResetInventory(players[0]);
                                ResetQuests(players[0]);
                                ResetBanks(players[0]);
                                player.SendSuccessMessage($"{players[0].Name}'s character has been reset!");
                                players[0].SendInfoMessage("Your character has been reset!");
                            }
                            else
                            {
                                TShock.CharacterDB.RemovePlayer(userid);
                                player.SendSuccessMessage($"{username}'s character has been reset!");
                            }
                        }
                        catch (Exception ex)
                        {
                            player.SendErrorMessage($"An error occurred while resetting all for: {username}");
                            TShock.Log.ConsoleError(ex.ToString());
                        }
                        break;

                    case "stats":
                        try
                        {
                            if (online)
                            {
                                ResetStats(players[0]);
                                player.SendSuccessMessage($"{players[0].Name}'s stats have been reset!");
                                players[0].SendInfoMessage("Your stats have been reset!");
                            }
                            else
                            {
                                db.Query("UPDATE tsCharacter SET Health = @0, MaxHealth = @1, Mana = @2, MaxMana = @3 WHERE Account = @4;", startHealth, startHealth, startMana, startMana, userid);
                                player.SendSuccessMessage($"{players[0].Name}'s stats have been reset!");
                            }
                        }
                        catch (Exception ex)
                        {
                            player.SendErrorMessage($"An error occurred while resetting stats for: {players[0].Name}");
                            TShock.Log.ConsoleError(ex.ToString());
                        }
                        break;

                    case "inventory":
                        try
                        {
                            if (online)
                            {
                                ResetInventory(players[0]);
                                ResetBanks(players[0]);
                                player.SendSuccessMessage($"{players[0].Name}'s inventory has been reset!");
                                players[0].SendInfoMessage("Your inventory has been reset!");
                            }
                            else
                            {
                                var inventory = new StringBuilder();
                                //for (int i = 0; i < Terraria.Main.maxInventory; i++)
                                for (int i = 0; i < 58 ; i++)
                                {
                                    if (i > 0)
                                    {
                                        inventory.Append("~");
                                    }
                                    if (i < TShock.ServerSideCharacterConfig.Settings.StartingInventory.Count)
                                    {
                                        var item = TShock.ServerSideCharacterConfig.Settings.StartingInventory[i];
                                        inventory.Append(item.NetId).Append(',').Append(item.Stack).Append(',').Append(item.PrefixId);
                                    }
                                    else
                                    {
                                        inventory.Append("0,0,0");
                                    }
                                }
                                string initialItems = inventory.ToString();
                                db.Query("UPDATE tsCharacter SET Inventory = @0 WHERE Account = @1;", initialItems, userid);
                                player.SendSuccessMessage($"{username}'s inventory has been reset!");
                            }
                        }
                        catch (Exception ex)
                        {
                            player.SendErrorMessage($"An error occurred while resetting inventory for: {username}");
                            TShock.Log.ConsoleError(ex.ToString());
                        }
                        break;

                    case "quests":
                        try
                        {
                            if (online)
                            {
                                ResetQuests(players[0]);
                                player.SendSuccessMessage($"{players[0].Name}'s quests have been reset to 0!");
                                players[0].SendInfoMessage("Your quests have been reset to 0!");
                            }
                            else
                            {
                                db.Query("UPDATE tsCharacter SET questsCompleted = @0 WHERE Account = @1;", 0, userid);
                                player.SendSuccessMessage($"{username}'s quests have been reset to 0!");
                            }
                        }
                        catch (Exception ex)
                        {
                            player.SendErrorMessage($"An error occurred while resetting quests for: {username}");
                            TShock.Log.ConsoleError(ex.ToString());
                        }
                        break;

                    default:
                        player.SendErrorMessage($"Invalid syntax! Proper syntax: {cmd}resetplayer <username> <all|stats|inventory|quests>");
                        break;
                }
            }
            else
            {
                player.SendErrorMessage("SSC is not enabled on this server! \nThis plugin will only work if SSC (Server Side Characters) is enabled!");
            }
        }

        public void ResetPlayers(CommandArgs args)
        {
            var cmd = TShock.Config.Settings.CommandSpecifier;
            TSPlayer player = args.Player;

            if (Main.ServerSideCharacter)
            {
                if (args.Parameters.Count == 0)
                {
                    player.SendErrorMessage($"Invalid syntax! Proper syntax: {cmd}resetplayers <all|stats|inventory|quests>");
                    return;
                }

                var subcmd = args.Parameters[0].ToLower();
                IDbConnection db = TShock.CharacterDB.database;

                switch (subcmd)
                {
                    case "all":
                        try
                        {
                            if (args.Silent)
                            {
                                player.SendWarningMessage("Resetting SSC data for all players...");
                            }
                            else
                            {
                                TSPlayer.All.SendWarningMessage("Resetting SSC data for all players...");
                            }

                            foreach (TSPlayer temp in TShock.Players) //resets online players
                            {
                                if (temp != null)
                                {
                                    ResetStats(temp);
                                    ResetInventory(temp);
                                    ResetQuests(temp);
                                    ResetBanks(temp);
                                }
                            }

                            db.Query("DELETE FROM tsCharacter;"); //deletes all characters in database. Doesn't affect online players unless improper shutdown happens.

                            if (args.Silent)
                            {
                                player.SendSuccessMessage("SSC data for all players have been reset!");
                            }
                            else
                            {
                                TSPlayer.All.SendSuccessMessage("SSC data for all players have been reset!");
                            }
                            TShock.Log.ConsoleInfo("All players have been reset.");
                        }
                        catch (Exception ex)
                        {
                            player.SendErrorMessage("An error occurred while resetting all for all players!");
                            TShock.Log.ConsoleError(ex.ToString());
                        }
                        break;

                    case "stats":
                        try
                        {
                            if (args.Silent)
                            {
                                player.SendWarningMessage("Resetting all players' stats...");
                            }
                            else 
                            {
                                TSPlayer.All.SendWarningMessage("Resetting all players' stats...");
                            }

                            foreach (TSPlayer temp in TShock.Players)
                            {
                                if (temp != null)
                                    ResetStats(temp);
                            }

                            db.Query("UPDATE tsCharacter SET Health = @0, MaxHealth = @1, Mana = @2, MaxMana = @3", startHealth, startHealth, startMana, startMana);

                            if (args.Silent)
                            {
                                player.SendSuccessMessage("All players' stats have been reset!");
                            }
                            else
                            {
                                TSPlayer.All.SendSuccessMessage("All players' stats have been reset!");
                            }
                            TShock.Log.ConsoleInfo("All players' stats have been reset.");
                        }
                        catch (Exception ex)
                        {
                            player.SendErrorMessage("An error occurred while resetting stats for all players!");
                            TShock.Log.ConsoleError(ex.ToString());
                        }
                        break;

                    case "inventory":
                        try
                        {
                            if (args.Silent)
                            {
                                player.SendWarningMessage("Resetting all players' inventory...");
                            }
                            else
                            {
                                TSPlayer.All.SendWarningMessage("Resetting all players' inventory...");
                            }

                            foreach (TSPlayer temp in TShock.Players)
                            {
                                if (temp != null)
                                {
                                    ResetInventory(temp);
                                    ResetBanks(temp);
                                }
                            }

                            var inventory = new StringBuilder(); //TShock's SeedInitialData method
                            //for (int i = 0; i < Terraria.Main.maxInventory; i++)
                            for (int i = 0; i < 58; i++)
                            {
                                if (i > 0)
                                {
                                    inventory.Append("~");
                                }
                                if (i < TShock.ServerSideCharacterConfig.Settings.StartingInventory.Count)
                                {
                                    var item = TShock.ServerSideCharacterConfig.Settings.StartingInventory[i];
                                    inventory.Append(item.NetId).Append(',').Append(item.Stack).Append(',').Append(item.PrefixId);
                                }
                                else
                                {
                                    inventory.Append("0,0,0");
                                }
                            }
                            string initialItems = inventory.ToString();
                            db.Query("UPDATE tsCharacter SET Inventory = @0;", initialItems);

                            if (args.Silent)
                            {
                                player.SendSuccessMessage("All players' inventory has been reset!");
                            }
                            else
                            {
                                TSPlayer.All.SendSuccessMessage("All players' inventory has been reset!");
                            }
                            TShock.Log.ConsoleInfo("All players' inventory has been reset.");
                        }
                        catch (Exception ex)
                        {
                            player.SendErrorMessage("An error occurred while resetting inventory for all players!");
                            TShock.Log.ConsoleError(ex.ToString());
                        }
                        break;

                    case "quests":
                        try
                        {
                            if (args.Silent)
                            {
                                player.SendWarningMessage("Resetting all players' quests...");
                            }
                            else
                            {
                                TSPlayer.All.SendWarningMessage("Resetting all players' quests...");
                            }

                            foreach (TSPlayer temp in TShock.Players)
                            {
                                if (temp != null)
                                    ResetQuests(temp);
                            }

                            db.Query("UPDATE tsCharacter SET questsCompleted = @0;", 0);

                            if (args.Silent)
                            {
                                player.SendSuccessMessage("All players' quests have been reset to 0!");
                            }
                            else
                            {
                                TSPlayer.All.SendSuccessMessage("All players' quests have been reset to 0!");
                            }
                            TShock.Log.ConsoleInfo("All players' quests have been reset to 0");
                        }
                        catch (Exception ex)
                        {
                            player.SendErrorMessage("An error occurred while resetting quests for all players!");
                            TShock.Log.ConsoleError(ex.ToString());
                        }
                        break;

                    default:
                        player.SendErrorMessage($"Invalid syntax! Proper syntax: {cmd}resetplayers <all|stats|inventory|quests>");
                        break;
                }
            }
            else
                player.SendErrorMessage("SSC is disabled on this server! \nThis plugin will only work if SSC (Server Side Characters) is enabled!");
        }

        public void ResetStats(TSPlayer player)
        {
            player.TPlayer.statLife = startHealth;
            player.TPlayer.statLifeMax = startHealth;
            player.TPlayer.statMana = startMana;
            player.TPlayer.statManaMax = startMana;

            NetMessage.SendData(4, -1, -1, NetworkText.FromLiteral(player.Name), player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData(42, -1, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData(16, -1, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData(50, -1, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);

            NetMessage.SendData(4, player.Index, -1, NetworkText.FromLiteral(player.Name), player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData(42, player.Index, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData(16, player.Index, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
            NetMessage.SendData(50, player.Index, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
        }

        public void ResetInventory(TSPlayer player)
        {
            ClearInventory(player);

            int slot = 0;
            Item give;
            foreach (NetItem item in StarterItems)
            {
                give = TShock.Utils.GetItemById(item.NetId);
                give.stack = item.Stack;
                give.prefix = item.PrefixId;

                if (player.InventorySlotAvailable)
                {
                    player.TPlayer.inventory[slot] = give;
                    NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.Empty, player.Index, slot);
                    slot++;
                }
            }
        }

        public void ResetQuests(TSPlayer player)
        {
            player.TPlayer.anglerQuestsFinished = 0;

            NetMessage.SendData((int)PacketTypes.NumberOfAnglerQuestsCompleted, -1, -1, NetworkText.Empty, player.Index);
            NetMessage.SendData((int)PacketTypes.NumberOfAnglerQuestsCompleted, player.Index, -1, NetworkText.Empty, player.Index);
        }

        public void ResetBanks(TSPlayer player)
        {
            for (int k = 0; k < NetItem.PiggySlots; k++)
            {
                player.TPlayer.bank.item[k].netDefaults(0);
            }
            for (int k = 0; k < NetItem.SafeSlots; k++)
            {
                player.TPlayer.bank2.item[k].netDefaults(0);
            }
            for (int k = 0; k < NetItem.ForgeSlots; k++)
            {
                player.TPlayer.bank3.item[k].netDefaults(0);
            }
            for (int k = 0; k < NetItem.VoidSlots; k++) // New Void Vault in 1.4
            {
                player.TPlayer.bank4.item[k].netDefaults(0);
            }

            for (int k = NetItem.MaxInventory - (NetItem.PiggySlots + NetItem.SafeSlots + NetItem.ForgeSlots + NetItem.VoidSlots) - 1; k < NetItem.MaxInventory; k++)
            {
                NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.Empty, player.Index, (float)k, 0f, 0f, 0);
            }
            for (int k = NetItem.MaxInventory - (NetItem.PiggySlots + NetItem.SafeSlots + NetItem.ForgeSlots + NetItem.VoidSlots) - 1; k < NetItem.MaxInventory; k++)
            {
                NetMessage.SendData((int)PacketTypes.PlayerSlot, player.Index, -1, NetworkText.Empty, player.Index, (float)k, 0f, 0f, 0);
            }
        }
        #endregion
    }
}