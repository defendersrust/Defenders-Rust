using Oxide.Core;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ConVar;

namespace Oxide.Plugins
{
    [Info("IGather", "DylanSMR", "0.0.8", ResourceId = 1763)]
    [Description("Adds the ability to set a gather rate for each person.")]
    class IGather : RustPlugin
    {
        ////////////////////////////////////////////////////
        //Configuration Settings////////////////////////////
        ////////////////////////////////////////////////////
        
        void LoadDefaultConfig()
        {
            Puts("Creating a new config file");
            Config.Clear();
            
            //Regular Default Rate//
            Config["DefaultQuarryRate"] = 1;
            Config["DefaultResourceRate"] = 1;
            Config["DefaultCollectable"] = 1;
            //VIP Default Rate//
            Config["VIPQuarryRate"] = 1;
            Config["VIPResourceRate"] = 1;
            Config["VIPCollectable"] = 1;
            
            Config.Save();
        } 
           
        ////////////////////////////////////////////////////
        //Data Settings|Config//////////////////////////////
        //////////////////////////////////////////////////// 
  
        Dictionary<ulong, BasePlayer> quarryOwners = new Dictionary<ulong, BasePlayer>();
        class StoredData
        {
            public Dictionary<ulong, UserInfo> Users = new Dictionary<ulong, UserInfo>();
            public StoredData()
            {
            }
        }

        class UserInfo
        {
            public string Name;
            public int PlayerQuarry;
            public int ResourceRate;
            public int CollectableRate;            
            public UserInfo()
            {
            }
        }  
        
        StoredData storedData;
        
        void Loaded()
        {
            LoadLang();
            storedData = Interface.GetMod().DataFileSystem.ReadObject<StoredData>(this.Title);
        } 

        void OnPlayerInit(BasePlayer player)
        {
            if (!storedData.Users.ContainsKey(player.userID)) InitUserData(player);
        }
        
        private bool InitUserData(BasePlayer player)
        {
            if(!storedData.Users.ContainsKey(player.userID))
            {
                if (!permission.UserHasPermission(player.userID.ToString(), "igather.vip"))
                {
                    storedData.Users.Add(player.userID, new UserInfo());
                    storedData.Users[player.userID].Name = player.displayName;
                    storedData.Users[player.userID].PlayerQuarry = Convert.ToInt32(Config["DefaultQuarryRate"]);
                    storedData.Users[player.userID].ResourceRate = Convert.ToInt32(Config["DefaultResourceRate"]);
                    storedData.Users[player.userID].CollectableRate = Convert.ToInt32(Config["DefaultCollectable"]);
                    Puts(""+player.displayName+" was given the player default rates.");
                    SaveData();
                }
                else
                {
                    storedData.Users.Add(player.userID, new UserInfo());
                    storedData.Users[player.userID].Name = player.displayName;
                    storedData.Users[player.userID].PlayerQuarry = Convert.ToInt32(Config["VIPQuarryRate"]);
                    storedData.Users[player.userID].ResourceRate = Convert.ToInt32(Config["VIPResourceRate"]);
                    storedData.Users[player.userID].CollectableRate = Convert.ToInt32(Config["VIPCollectable"]);
                    SaveData();  
                    Puts(""+player.displayName+" was given the VIP default rates.");                  
                }
                return true;
            }
            else
            {
                return false;
            }  
        }
        
        void Unload()
        {
            SaveData();
        }
        
        ////////////////////////////////////////////////////
        //Language Settings/////////////////////////////////
        ////////////////////////////////////////////////////

        private string GetMessage(string name, string sid = null) {
            return lang.GetMessage(name, this, sid);
        }
        
        void LoadLang()
        {
			lang.RegisterMessages(new Dictionary<string,string>{
            ["IG_GATHERATE"] = "<color='#DD0000'>[IGather]Collectable Rate({0}x) Quarry Rate({1}x) Resource Rate({2}x).</color>",
            ["IG_GATHERPLAYER"] = "<color='#DD0000'>[IGather]{0}'s Gather Rate|Collectable Rate({1}x) Quarry Rate({2}x) Resource Rate({3}x).</color>",
            ["IG_SETALLHELP"] = "<color='#DD0000'>[IGather]Commands for (setall) | </igather setall (type) (rate)> | <Types:quarry, resource, collectable, all> | <Rate is the amount of extra materials they get(Multiplied)></color>",
            ["IG_DEFAULTHELP"] = "<color='#DD0000'>[IGather]Commands for (default) | </igather default (player) (type) | <Types:quarry, resource, collectable, all></color>",
            ["IG_DEFAULTALL"] = "<color='#DD0000'>[IGather]You have reset {0}'s stats.</color>",
            ["IG_DEFAULTP"] = "<color='#DD0000'>[IGather]{0} has reset your player stats to default.</color>",
            ["IG_SETP"] = "<color='#DD0000'>[IGather]{0} has changed your data file of ({1}) to {2}x.</color>",
            ["IG_SETHELP"] = "<color='#DD0000'>[IGather]Commands for (set) | </igather set (player) (type) (rate)> | <Types:QuarryR, ResourceR, CollectableR, all> | <Rate is the amount of extra materials they get(Multiplied)></color>",
            ["IG_DEFAULTR"] = "<color='#DD0000'>[IGather]{0} has reset your data of ({1}) to the default settings.</color>",
            ["IG_SETPA"] = "<color='#DD0000'>[IGather]{0} has set all of your rate data to {1}x.</color>",
            ["IG_SETA"] = "<color='#DD0000'>[IGather]You have set {0}'s ({1}) to {2}x.</color>",
            ["IG_HELP"] = "<color='#DD0000'>[IGather]Possible Commands:</igather gather> Shows your current gather rate of all 3 types | </igather help> Shows possible commands.",
            ["IG_AHELP"] = "<color='#DD0000'>[IGather]Possible Commands:</igather set> Shows set help. | </igather default> Shows default help. | </igather defaultall> Sets all players gather rate to default. |</color>",
            ["IG_AAHELP"] = "<color='#DD0000'>[IGather]Possible Commands:</igather setall> Shows setall help. | </igather gatherp (player)> Shows the gather rate for a player.</color>",
            ["IG_NOPERMS"] = "<color='#DD0000'>[IGather]You do not have any permissions for this command.</color>",
            ["IG_DEFAULTEDALL"] = "<color='#DD0000'>[IGather]{0} has reset everyones player data to the default! Do /igather gather to see your new gather rate.</color>",
            ["IG_SETTALL"] = "<color='#DD0000'>[IGather]{0} has set everyones ({1} data) to {2}.</color>",            
			}, this);
            
            LoadPermissions();
        }
        
        ////////////////////////////////////////////////////
        //Permission Settings///////////////////////////////
        ////////////////////////////////////////////////////        
        
        void LoadPermissions()
        {
            //VIP//
            permission.RegisterPermission("igather.vip", this);
            //ADMIN//
            permission.RegisterPermission("igather.admin", this);
            permission.RegisterPermission("igather.defaultall", this);
            permission.RegisterPermission("igather.default", this);
            permission.RegisterPermission("igather.set", this);
            permission.RegisterPermission("igather.setall", this);
            permission.RegisterPermission("igather.gatherp", this);          
        }        
        
        ////////////////////////////////////////////////////
        //Plugin Functions/Settings/////////////////////////
        //////////////////////////////////////////////////// 
        
        private object FindPlayer(string arg)
        {
            var foundPlayers = new List<BasePlayer>();
            ulong steamid;
            ulong.TryParse(arg, out steamid);
            string lowerarg = arg.ToLower();

            foreach (var p in BasePlayer.activePlayerList)
            {
                if (steamid != 0L)
                    if (p.userID == steamid)
                    {
                        foundPlayers.Clear();
                        foundPlayers.Add(p);
                        return foundPlayers;
                    }
                string lowername = p.displayName.ToLower();
                if (lowername.Contains(lowerarg))
                {
                    foundPlayers.Add(p);
                }
            }
            return foundPlayers[0];
        }
        
        void DefaultAll()
        {    
            foreach(BasePlayer player in BasePlayer.activePlayerList)
            {
                storedData.Users[player.userID].PlayerQuarry = Convert.ToInt32(Config["DefaultQuarryRate"]);
                storedData.Users[player.userID].ResourceRate = Convert.ToInt32(Config["DefaultResourceRate"]); 
                storedData.Users[player.userID].CollectableRate = Convert.ToInt32(Config["DefaultCollectable"]);
                SaveData();                       
            }            
        }
        
        void collectableall(int amount)
        {
            foreach(BasePlayer player in BasePlayer.activePlayerList)
            {
                storedData.Users[player.userID].CollectableRate = amount;
                SaveData();                       
            }               
        }
        
        void resourceall(int amount)
        {
            foreach(BasePlayer player in BasePlayer.activePlayerList)
            {
                storedData.Users[player.userID].ResourceRate = amount;
                SaveData();                       
            }               
        }
        
        void quarryall(int amount)
        {
            foreach(BasePlayer player in BasePlayer.activePlayerList)
            {
                storedData.Users[player.userID].PlayerQuarry = amount; 
                SaveData();                 
            }               
        }
        
        void allamount(int amount)
        {
            foreach(BasePlayer player in BasePlayer.activePlayerList)
            {
                storedData.Users[player.userID].PlayerQuarry = amount;
                storedData.Users[player.userID].ResourceRate = amount; 
                storedData.Users[player.userID].CollectableRate = amount;
                SaveData(); 
            }
        }
        
        void SaveData()
        {
            Interface.Oxide.DataFileSystem.WriteObject(this.Title, storedData);    
        }
                
        ////////////////////////////////////////////////////
        //Chat Commands/PG//////////////////////////////////
        ////////////////////////////////////////////////////
        
        [ChatCommand("igather")]
        void ChatPG(BasePlayer player, string command, string[] args)
        {
            if(args.Length == 0)
            {
                if (permission.UserHasPermission(player.userID.ToString(), "igather.admin"))
                {
                    SendReply(player, string.Format(GetMessage("IG_AHELP", player.UserIDString)));
                    SendReply(player, string.Format(GetMessage("IG_AAHELP", player.UserIDString))); 
                    return;       
                }
                else
                {
                    SendReply(player, string.Format(GetMessage("IG_HELP", player.UserIDString))); 
                    return;       
                }    
            }               
            switch(args[0])
            {
                case "help":
                    if (permission.UserHasPermission(player.userID.ToString(), "igather.admin"))
                    {
                        SendReply(player, string.Format(GetMessage("IG_AHELP", player.UserIDString)));
                        SendReply(player, string.Format(GetMessage("IG_AAHELP", player.UserIDString))); 
                        return;                     
                    }
                    else
                    {
                        SendReply(player, string.Format(GetMessage("IG_HELP", player.UserIDString))); 
                        return;                      
                    }    
                break;
                   
                case "set":
                    if(args.Length <= 3)
                    {
                        SendReply(player, string.Format(GetMessage("IG_SETHELP", player.UserIDString)));
                        return;
                    }
                    if (permission.UserHasPermission(player.userID.ToString(), "igather.admin") || permission.UserHasPermission(player.userID.ToString(), "igather.set"))
                    {
                        object addPlayer = FindPlayer(args[1]);             
                        BasePlayer target = (BasePlayer)addPlayer;                
                                
                        var NewData = Convert.ToInt32(args[3]);
                        if(args[2] == "QuarryR")
                        {
                            storedData.Users[target.userID].PlayerQuarry = NewData;
                            SaveData();
                            var quarrys = "QuarryRate";
                            SendReply(target, string.Format(GetMessage("IG_SETP", player.UserIDString), player.displayName, quarrys, NewData));
                            SendReply(player, string.Format(GetMessage("IG_SETA", player.UserIDString), target.displayName, quarrys,NewData));       
                        }
                        else if(args[2] == "ResourceR")
                        {
                            storedData.Users[target.userID].ResourceRate = NewData;
                            SaveData();
                            var Resources = "ResourceRate";
                            SendReply(target, string.Format(GetMessage("IG_SETP", player.UserIDString), player.displayName, Resources, NewData));
                            SendReply(player, string.Format(GetMessage("IG_SETA", player.UserIDString), target.displayName, Resources, NewData));         
                        }
                        else if(args[2] == "CollectableR") 
                        {
                            storedData.Users[target.userID].CollectableRate = NewData;
                            SaveData();
                            var CollectableS = "CollectableRate";
                            SendReply(target, string.Format(GetMessage("IG_SETP", player.UserIDString), player.displayName, CollectableS, NewData));
                            SendReply(player, string.Format(GetMessage("IG_SETA", player.UserIDString), target.displayName, CollectableS, NewData));          
                        } 
                        else if(args[2] == "all")
                        {
                            storedData.Users[target.userID].PlayerQuarry = NewData;
                            storedData.Users[target.userID].ResourceRate = NewData; 
                            storedData.Users[target.userID].CollectableRate = NewData;    
                            SaveData();
                            SendReply(target, string.Format(GetMessage("IG_SETPA", player.UserIDString), player.displayName, NewData));                                                      
                        }
                        else
                        {
                            return;
                            SendReply(player, string.Format(GetMessage("IG_SETHELP", player.UserIDString)));
                        }
                    }
                    else
                    {
                        SendReply(player, string.Format(GetMessage("IG_NOPERMS", player.UserIDString)));
                        return;    
                    }            
                break;
                         
                case "default":
                    if(args.Length <= 2 || args.Length >= 4)
                    {
                        SendReply(player, string.Format(GetMessage("IG_DEFAULTHELP", player.UserIDString)));
                        return;
                    }                
                    if (permission.UserHasPermission(player.userID.ToString(), "igather.admin") || permission.UserHasPermission(player.userID.ToString(), "igather.default"))
                    {
                        
                        object addPlayer = FindPlayer(args[1]);             
                        BasePlayer target = (BasePlayer)addPlayer;                       
                
                        if(args[2] == "all")
                        {
                            storedData.Users[target.userID].PlayerQuarry = Convert.ToInt32(Config["DefaultQuarryRate"]);
                            storedData.Users[target.userID].ResourceRate = Convert.ToInt32(Config["DefaultResourceRate"]); 
                            storedData.Users[target.userID].CollectableRate = Convert.ToInt32(Config["DefaultCollectable"]);
                            SaveData();
                            SendReply(player, string.Format(GetMessage("IG_DEFAULTALL", player.UserIDString), target.displayName));
                            SendReply(target, string.Format(GetMessage("IG_DEFAULTP", player.UserIDString), player.displayName));   
                        } 
                        else if(args[2] == "quarry")
                        {
                            storedData.Users[target.userID].PlayerQuarry = Convert.ToInt32(Config["DefaultQuarryRate"]);
                            SaveData();    
                            SendReply(player, string.Format(GetMessage("IG_DEFAULTALL", player.UserIDString), target.displayName));
                            var quarryr = "QuarryRate";
                            SendReply(target, string.Format(GetMessage("IG_DEFAULTR", player.UserIDString), player.displayName, quarryr));  
                        }   
                        else if(args[2] == "resource")
                        {
                            storedData.Users[target.userID].ResourceRate = Convert.ToInt32(Config["DefaultResourceRate"]);
                            SaveData();
                            SendReply(player, string.Format(GetMessage("IG_DEFAULTALL", player.UserIDString), target.displayName));
                            var resourcer = "ResourceRate"; 
                            SendReply(target, string.Format(GetMessage("IG_DEFAULTR", player.UserIDString), player.displayName, resourcer));     
                        }
                        else if(args[2] == "collectable")
                        {
                            storedData.Users[target.userID].CollectableRate = Convert.ToInt32(Config["DefaultCollectable"]);
                            SaveData(); 
                            SendReply(player, string.Format(GetMessage("IG_DEFAULTALL", player.UserIDString), target.displayName));
                            var collect = "CollectableRate";
                            SendReply(target, string.Format(GetMessage("IG_DEFAULTR", player.UserIDString), player.displayName, collect));     
                        }
                        else
                        {
                            SendReply(player, string.Format(GetMessage("IG_DEFAULTHELP", player.UserIDString)));    
                        }
                    }
                    else
                    {
                        SendReply(player, string.Format(GetMessage("IG_NOPERMS", player.UserIDString)));
                        return;
                    }            
                break;
                
                case "defaultall":
                    if (!permission.UserHasPermission(player.userID.ToString(), "igather.admin") && permission.UserHasPermission(player.userID.ToString(), "igather.defaultall"))
                    {
                        SendReply(player, string.Format(GetMessage("IG_NOPERMS", player.UserIDString)));
                        return;
                    }
                    PrintToChat(string.Format(GetMessage("IG_DEFAULTEDALL", player.UserIDString), player.displayName));
                    DefaultAll();
                break;
                
                case "setall":
                    if (!permission.UserHasPermission(player.userID.ToString(), "igather.admin") && permission.UserHasPermission(player.userID.ToString(), "igather.setall"))
                    {
                        SendReply(player, string.Format(GetMessage("IG_NOPERMS", player.UserIDString)));
                        return;
                    }
                    if(args.Length <= 1)
                    {
                        SendReply(player, string.Format(GetMessage("IG_SETALLHELP", player.UserIDString)));
                        return;
                    }
                    if(args[1] == "quarry")
                    {
                        var qr = "QuarryRate";
                        var amount = Convert.ToInt32(args[2]);
                        PrintToChat(string.Format(GetMessage("IG_SETTALL", player.UserIDString), player.displayName, qr, amount));
                        quarryall(amount);  
                    }
                    else if(args[1] == "resource")
                    {
                        var rr = "ResourceRate";
                        var amount = Convert.ToInt32(args[2]);
                        PrintToChat(string.Format(GetMessage("IG_SETTALL", player.UserIDString), player.displayName, rr, amount));
                        resourceall(amount);   
                    }
                    else if(args[1] == "collectable")
                    {
                        var cr = "CollectableRate";
                        var amount = Convert.ToInt32(args[2]);
                        PrintToChat(string.Format(GetMessage("IG_SETTALL", player.UserIDString), player.displayName, cr, amount));
                        collectableall(amount); 
                    }
                    else if(args[1] == "all")
                    {
                        var ar = "AllRate";
                        var amount = Convert.ToInt32(args[2]);
                        PrintToChat(string.Format(GetMessage("IG_SETTALL", player.UserIDString), player.displayName, ar, amount));
                        allamount(amount);
                    }
                    else
                    {
                        SendReply(player, string.Format(GetMessage("IG_SETALLHELP", player.UserIDString)));   
                        return;
                    } 
                break;
                
                case "gather":
                    SendReply(player, string.Format(GetMessage("IG_GATHERATE", player.UserIDString), storedData.Users[player.userID].CollectableRate, storedData.Users[player.userID].PlayerQuarry, storedData.Users[player.userID].ResourceRate));
                break;
                
                case "gatherp":
                    if (permission.UserHasPermission(player.userID.ToString(), "igather.admin") || permission.UserHasPermission(player.userID.ToString(), "igather.gatherp"))
                    {
                        object addPlayer = FindPlayer(args[1]);             
                        BasePlayer target = (BasePlayer)addPlayer;
                    
                        SendReply(player, string.Format(GetMessage("IG_GATHERPLAYER", player.UserIDString), target.displayName, storedData.Users[target.userID].CollectableRate, storedData.Users[target.userID].PlayerQuarry, storedData.Users[target.userID].ResourceRate));                      
                    }
                    else
                    {
                        SendReply(player, string.Format(GetMessage("IG_NOPERMS", player.UserIDString)));
                        return;
                    }
                break;
                
                case "resetdata":
                    if (!permission.UserHasPermission(player.userID.ToString(), "igather.admin") || permission.UserHasPermission(player.userID.ToString(), "igather.gatherp"))
                    { 
                        object addPlayer = FindPlayer(args[1]);
                        BasePlayer target = (BasePlayer)addPlayer;
                        
                        storedData.Users.Add(target.userID, new UserInfo());
                        SaveData();   
                    }
                    else
                    {
                        SendReply(player, string.Format(GetMessage("IG_NOPERMS", player.UserIDString)));
                        return;
                    }                        
                break;
            }  
        }
        
        ////////////////////////////////////////////////////
        //Gather Rate Amounts Fixes/////////////////////////
        ////////////////////////////////////////////////////
        
        private void OnDispenserGather(ResourceDispenser dispenser, BaseEntity entity, Item item)
        {
            BasePlayer player = entity.ToPlayer();
            item.amount = (int)(item.amount * storedData.Users[player.userID].ResourceRate);
        }

        private void OnQuarryGather(MiningQuarry quarry, Item item)
        {
            BasePlayer player;
            if (!quarryOwners.TryGetValue(quarry.OwnerID, out player))
            {
                player = BasePlayer.FindByID(quarry.OwnerID) ?? BasePlayer.FindSleeping(quarry.OwnerID);
                quarryOwners[quarry.OwnerID] = player;
                item.amount = (int)(item.amount * storedData.Users[player.userID].PlayerQuarry);
            }
            if (player == null) return;            
        }

        private void OnCollectiblePickup(Item item, BasePlayer player)
        {
            item.amount = (int)(item.amount * storedData.Users[player.userID].CollectableRate);      
        }             
    }
}