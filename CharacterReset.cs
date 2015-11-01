using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TShockAPI;
using TShockAPI.DB;
using TShockAPI.Hooks;
using Terraria;
using TerrariaApi.Server;

namespace CharacterReset
{
    [ApiVersion(1, 22)]
    public class CharacterReset : TerrariaPlugin
    {
        #region Plugin Info
            public override Version Version
            {
                get { return new Version("1.2"); }
            }
            public override string Name
            {
                get { return "CharacterReset"; }
            }
            public override string Author
            {
                get { return "Bippity"; }
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

                Commands.ChatCommands.Add(new Command(new List<string>() { "characterreset.stats", "characterreset.inventory", "characterreset.quests" }, ResetCharacter, "resetcharacter"));
                Commands.ChatCommands.Add(new Command("characterreset.players", ResetPlayers, "resetplayers"));
                Commands.ChatCommands.Add(new Command("characterreset.players", ResetPlayer, "resetplayer"));

                if (Main.ServerSideCharacter)
                {
                    StarterItems = TShock.ServerSideCharacterConfig.StartingInventory;

                    if (TShock.ServerSideCharacterConfig.StartingHealth > 500)
                        startHealth = 500;
                    else if (TShock.ServerSideCharacterConfig.StartingHealth < 100)
                        startHealth = 100;
                    else
                        startHealth = TShock.ServerSideCharacterConfig.StartingHealth;

                    if (TShock.ServerSideCharacterConfig.StartingMana > 200)
                        startMana = 200;
                    else if (TShock.ServerSideCharacterConfig.StartingMana < 20)
                        startMana = 20;
                    else
                        startMana = TShock.ServerSideCharacterConfig.StartingMana;
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
                    TShock.Log.ConsoleError("[CharacterReset] This plugin will not work properly with ServerSidedCharacters disabled.");
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
                    if (i < NetItem.InventorySlots) //main inventory excluding the special slots //replace with NetItem.InventorySlots?
                    {
                        player.TPlayer.inventory[i].netDefaults(0);
                    }
                    else if (i < NetItem.InventorySlots + NetItem.ArmorSlots)
                    {
                        var index = i - NetItem.InventorySlots;
                        player.TPlayer.armor[index].netDefaults(0);
                    }
                    else if (i < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots)
                    {
                        var index = i - (NetItem.InventorySlots + NetItem.ArmorSlots);
                        player.TPlayer.dye[index].netDefaults(0);
                    }
                    else if (i < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots)
                    {
                        var index = i - (NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots);
                        player.TPlayer.miscEquips[index].netDefaults(0);
                    }
                    else if (i < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots)
                    {
                        var index = i - (NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots);
                        player.TPlayer.miscDyes[index].netDefaults(0);
                    }
                    else if (i < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots + NetItem.PiggySlots) //piggy Bank
                    {
                        //var index = i - (NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots);
                        //player.TPlayer.bank.item[index].netDefaults(0);
                    }
                    else if (i < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots + NetItem.PiggySlots + NetItem.SafeSlots) //safe Bank
                    {
                        //var index = i - (NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots + NetItem.PiggySlots);
                        //player.TPlayer.bank2.item[index].netDefaults(0);
                    }
                    else
                    {
                        player.TPlayer.trashItem.netDefaults(0);
                    }
                }

                for (int k = 0; k < NetItem.MaxInventory - (NetItem.SafeSlots + NetItem.PiggySlots); k++)
                {
                    NetMessage.SendData(5, -1, -1, "", player.Index, (float)k, 0f, 0f, 0);
                }
                NetMessage.SendData(5, -1, -1, "", player.Index, (float)NetItem.MaxInventory, 0f, 0f, 0); //trash item

                for (int k = 0; k < Player.maxBuffs; k++)
                {
                    player.TPlayer.buffType[k] = 0;
                }

                NetMessage.SendData(4, -1, -1, player.Name, player.Index, 0f, 0f, 0f, 0);
                NetMessage.SendData(42, -1, -1, "", player.Index, 0f, 0f, 0f, 0);
                NetMessage.SendData(16, -1, -1, "", player.Index, 0f, 0f, 0f, 0);
                NetMessage.SendData(50, -1, -1, "", player.Index, 0f, 0f, 0f, 0);

                for (int k = 0; k < NetItem.MaxInventory - (NetItem.SafeSlots + NetItem.PiggySlots); k++)
                {
                    NetMessage.SendData(5, player.Index, -1, "", player.Index, (float)k, 0f, 0f, 0);
                }
                NetMessage.SendData(5, player.Index, -1, "", player.Index, (float)NetItem.MaxInventory, 0f, 0f, 0);

                for (int k = 0; k < Player.maxBuffs; k++)
                {
                    player.TPlayer.buffType[k] = 0;
                }

                NetMessage.SendData(4, player.Index, -1, player.Name, player.Index, 0f, 0f, 0f, 0);
                NetMessage.SendData(42, player.Index, -1, "", player.Index, 0f, 0f, 0f, 0);
                NetMessage.SendData(16, player.Index, -1, "", player.Index, 0f, 0f, 0f, 0);
                NetMessage.SendData(50, player.Index, -1, "", player.Index, 0f, 0f, 0f, 0);
            }

            private void ResetCharacter(CommandArgs args)
            {
                TSPlayer player = args.Player;
                if (player != null)
                {
                    if (Main.ServerSideCharacter)
                    {
                        if (args.Parameters.Count == 0)
                        {
                            player.SendErrorMessage("Invalid syntax! Proper syntax: /resetcharacter <all/stats/inventory/quests/banks>");
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
                                    player.SendErrorMessage("An error occurred while resetting!");
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
                                    player.SendErrorMessage("An error occurred while resetting!"); 
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
                                    player.SendErrorMessage("An error occurred while resetting!");
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
                                    player.SendErrorMessage("An error occurred while resetting!");
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
                                    player.SendErrorMessage("An error occurred while resetting!");
                                }
                                break;

                            default:
                                player.SendErrorMessage("Invalid syntax! Proper syntax: /resetcharacter <all/stats/inventory/quests/banks>");
                                break;

                        }
                    }
                    else
                    player.SendErrorMessage("SSC isn't enabled on this server!");
                }
            }

            public void ResetPlayer(CommandArgs args)
            {
                TSPlayer player = args.Player;

                if (Main.ServerSideCharacter)
                {
                    if (args.Parameters.Count < 2)
                    {
                        player.SendErrorMessage("Invalid syntax! Proper syntax: /resetplayer <username> <all/stats/inventory/quests>");
                        return;
                    }

                    string username = args.Parameters[0];
                    var subcmd = args.Parameters[1].ToLower();
                    IDbConnection db = TShock.CharacterDB.database;
                    bool online = true;
                    int userid = 0;

                    List<TSPlayer> players = TShock.Utils.FindPlayer(username);
                    if (players.Count < 1) //if player not found online
                    {
                        online = false;
                        player.SendWarningMessage("Player not found online. Searching database...");
                    }
                    else if (players.Count > 1)
                    {
                        string list = string.Join(", ", players.Select(p => p.Name));
                        player.SendErrorMessage("Multiple players found: " + list);
                        return;
                    }
                    
                    if (!online)
                    {
                        if (TShock.Users.GetUserByName(username) == null)
                        {
                            player.SendErrorMessage("Username \"{0}\" not found in database.", username);
                            return;
                        }
                        else
                        {
                            userid = TShock.Users.GetUserByName(username).ID;
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
                                    player.SendSuccessMessage(players[0].User.Name + "'s character has been reset!");
                                    players[0].SendInfoMessage("Your character has been reset!");
                                }
                                else
                                {
                                    TShock.CharacterDB.RemovePlayer(userid);
                                    player.SendSuccessMessage(username + "'s character has been reset!");
                                }
                            }
                            catch (Exception ex)
                            {
                                player.SendErrorMessage("An error occurred while resetting!");
                                TShock.Log.ConsoleError(ex.ToString());
                            }
                            break;

                        case "stats":
                            try
                            {
                                if (online)
                                {
                                    ResetStats(players[0]);
                                    player.SendSuccessMessage(players[0].User.Name + "'s stats have been reset!");
                                    players[0].SendInfoMessage("Your stats have been reset!");
                                }
                                else
                                {
                                    db.Query("UPDATE tsCharacter SET Health = @0, MaxHealth = @1, Mana = @2, MaxMana = @3 WHERE Account = @4;", startHealth, startHealth, startMana, startMana, userid);
                                    player.SendSuccessMessage(username + "'s stats have been reset!");
                                }
                            }
                            catch (Exception ex)
                            {
                                player.SendErrorMessage("An error occurred while resetting!");
                                TShock.Log.ConsoleError(ex.ToString());
                            }
                            break;

                        case "inventory":
                            try
                            {
                                if (online)
                                {
                                    ResetInventory(players[0]);
                                    player.SendSuccessMessage(players[0].User.Name + "'s inventory has been reset!");
                                    players[0].SendInfoMessage("Your inventory has been reset!");
                                }
                                else
                                {
                                    var inventory = new StringBuilder();
                                    for (int i = 0; i < Terraria.Main.maxInventory; i++)
                                    {
                                        if (i > 0)
                                        {
                                            inventory.Append("~");
                                        }
                                        if (i < TShock.ServerSideCharacterConfig.StartingInventory.Count)
                                        {
                                            var item = TShock.ServerSideCharacterConfig.StartingInventory[i];
                                            inventory.Append(item.NetId).Append(',').Append(item.Stack).Append(',').Append(item.PrefixId);
                                        }
                                        else
                                        {
                                            inventory.Append("0,0,0");
                                        }
                                    }
                                    string initialItems = inventory.ToString();
                                    db.Query("UPDATE tsCharacter SET Inventory = @0 WHERE Account = @1;", initialItems, userid);
                                    player.SendSuccessMessage(username + "'s inventory has been reset!");
                                }
                            }
                            catch (Exception ex)
                            {
                                player.SendErrorMessage("An error occurred while resetting!");
                                TShock.Log.ConsoleError(ex.ToString());
                            }
                            break;

                        case "quests":
                            try
                            {
                                if (online)
                                {
                                    ResetQuests(players[0]);
                                    player.SendSuccessMessage(players[0].User.Name + "'s quests have been reset to 0!");
                                    players[0].SendInfoMessage("Your quests have been reset to 0!");
                                }
                                else
                                {
                                    db.Query("UPDATE tsCharacter SET questsCompleted = @0 WHERE Account = @1;", 0, userid);
                                    player.SendSuccessMessage(username + "'s quests have been reset to 0!");
                                }
                            }
                            catch (Exception ex)
                            {
                                player.SendErrorMessage("An error occurred while resetting!");
                                TShock.Log.ConsoleError(ex.ToString());
                            }
                            break;

                        default:
                            player.SendErrorMessage("Invalid syntax! Proper syntax: /resetplayer <username> <all/stats/inventory/quests>");
                            break;
                    }
                }
                else
                {
                    player.SendErrorMessage("SSC isn't enabled on this server!");
                }
            }

            public void ResetPlayers(CommandArgs args)
            {
                TSPlayer player = args.Player;

                if (Main.ServerSideCharacter)
                {
                    if (args.Parameters.Count == 0)
                    {
                        player.SendErrorMessage("Invalid syntax! Proper syntax: /resetplayers <all/stats/inventory/quests>");
                        return;
                    }

                    var subcmd = args.Parameters[0].ToLower();
                    IDbConnection db = TShock.CharacterDB.database;

                    switch (subcmd)
                    {
                        case "all":
                            try
                            {
                                TSPlayer.All.SendWarningMessage("Resetting all players...");

                                foreach (TSPlayer temp in TShock.Players) //resets online players
                                {
                                    if (temp != null)
                                    {
                                        ResetStats(temp);
                                        ResetInventory(temp);
                                        ResetQuests(temp);
                                    }
                                }

                                db.Query("DELETE FROM tsCharacter;"); //deletes all characters in database. Doesn't affect online players unless improper shutdown happens.

                                TSPlayer.All.SendSuccessMessage("All players have been reset!");
                                TShock.Log.ConsoleInfo("All players have been reset.");
                            }
                            catch (Exception ex)
                            {
                                TSPlayer.All.SendErrorMessage("An error occurred while resetting!");
                                TShock.Log.ConsoleError(ex.ToString());   
                            }
                            break;

                        case "stats":
                            try
                            {
                                TSPlayer.All.SendWarningMessage("Resetting all players' stats...");

                                foreach (TSPlayer temp in TShock.Players)
                                {
                                    if (temp != null)
                                    ResetStats(temp);
                                }

                                db.Query("UPDATE tsCharacter SET Health = @0, MaxHealth = @1, Mana = @2, MaxMana = @3", startHealth, startHealth, startMana, startMana);

                                TSPlayer.All.SendSuccessMessage("All players' stats have been reset!");
                                TShock.Log.ConsoleInfo("All players' stats have been reset.");
                            }
                            catch (Exception ex)
                            {
                                TSPlayer.All.SendErrorMessage("An error occurred while resetting!");
                                TShock.Log.ConsoleError(ex.ToString());
                            }
                            break;

                        case "inventory":
                            try
                            {
                                TSPlayer.All.SendWarningMessage("Resetting all players' inventory...");

                                foreach (TSPlayer temp in TShock.Players)
                                {
                                    if (temp != null)
                                    ResetInventory(temp);
                                }

                                var inventory = new StringBuilder(); //TShock's SeedInitialData method
                                for (int i = 0; i < Terraria.Main.maxInventory; i++)
                                {
                                    if (i > 0)
                                    {
                                        inventory.Append("~");
                                    }
                                    if (i < TShock.ServerSideCharacterConfig.StartingInventory.Count)
                                    {
                                        var item = TShock.ServerSideCharacterConfig.StartingInventory[i];
                                        inventory.Append(item.NetId).Append(',').Append(item.Stack).Append(',').Append(item.PrefixId);
                                    }
                                    else
                                    {
                                        inventory.Append("0,0,0");
                                    }
                                }
                                string initialItems = inventory.ToString();
                                db.Query("UPDATE tsCharacter SET Inventory = @0;", initialItems);

                                TSPlayer.All.SendSuccessMessage("All players' inventory has been reset!");
                                TShock.Log.ConsoleInfo("All players' inventory has been reset.");
                            }
                            catch (Exception ex)
                            {
                                TSPlayer.All.SendErrorMessage("An error occurred while resetting!");
                                TShock.Log.ConsoleError(ex.ToString());
                            }
                            break;

                        case "quests":
                            try
                            {
                                TSPlayer.All.SendWarningMessage("Resetting all players' quests...");

                                foreach (TSPlayer temp in TShock.Players)
                                {
                                    if (temp != null)
                                    ResetQuests(temp);
                                }

                                db.Query("UPDATE tsCharacter SET questsCompleted = @0;", 0);
                             
                                TSPlayer.All.SendSuccessMessage("All players' quests have been reset to 0!");
                                TShock.Log.ConsoleInfo("All players' quests have been reset to 0");
                            }
                            catch (Exception ex)
                            {
                                TSPlayer.All.SendErrorMessage("An error occurred while resetting!");
                                TShock.Log.ConsoleError(ex.ToString());
                            }
                            break;

                        default:
                            player.SendErrorMessage("Invalid syntax! Proper syntax: /resetplayers <all/stats/inventory/quests>");
                            break;
                    }
                }
                else
                    player.SendErrorMessage("SSC isn't enabled on this server!");
            }

            public void ResetStats(TSPlayer player)
            {
                player.TPlayer.statLife = startHealth;
                player.TPlayer.statLifeMax = startHealth;
                player.TPlayer.statMana = startMana;
                player.TPlayer.statManaMax = startMana;

                NetMessage.SendData(4, -1, -1, player.Name, player.Index, 0f, 0f, 0f, 0);
                NetMessage.SendData(42, -1, -1, "", player.Index, 0f, 0f, 0f, 0);
                NetMessage.SendData(16, -1, -1, "", player.Index, 0f, 0f, 0f, 0);
                NetMessage.SendData(50, -1, -1, "", player.Index, 0f, 0f, 0f, 0);

                NetMessage.SendData(4, player.Index, -1, player.Name, player.Index, 0f, 0f, 0f, 0);
                NetMessage.SendData(42, player.Index, -1, "", player.Index, 0f, 0f, 0f, 0);
                NetMessage.SendData(16, player.Index, -1, "", player.Index, 0f, 0f, 0f, 0);
                NetMessage.SendData(50, player.Index, -1, "", player.Index, 0f, 0f, 0f, 0);
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
                        NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, string.Empty, player.Index, slot);
                        slot++;
                    }
                }
            }

            public void ResetQuests(TSPlayer player)
            {
                player.TPlayer.anglerQuestsFinished = 0;

                NetMessage.SendData(76, -1, -1, "", player.Index);
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

                for (int k = NetItem.MaxInventory - (NetItem.SafeSlots + NetItem.PiggySlots); k < NetItem.MaxInventory-1; k++)
                {
                    NetMessage.SendData(5, -1, -1, "", player.Index, (float)k, 0f, 0f, 0);
                }
                for (int k = NetItem.MaxInventory - (NetItem.SafeSlots + NetItem.PiggySlots); k < NetItem.MaxInventory-1; k++)
                {
                    NetMessage.SendData(5, player.Index, -1, "", player.Index, (float)k, 0f, 0f, 0);
                }
            }
        #endregion
    }
}