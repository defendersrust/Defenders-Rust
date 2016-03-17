using Oxide.Core;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Oxide.Plugins
{
    [Info("IGather", "DylanSMR", "0.0.3", ResourceId = 1519)]
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
            
            Config["DefaultQuarryRate"] = 1;
            Config["DefaultResourceRate"] = 1;
            Config["DefaultCollectable"] = 1;
            
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
                storedData.Users.Add(player.userID, new UserInfo());
                storedData.Users[player.userID].Name = player.displayName;
                storedData.Users[player.userID].PlayerQuarry = Convert.ToInt32(Config["DefaultQuarryRate"]);
                storedData.Users[player.userID].ResourceRate = Convert.ToInt32(Config["DefaultResourceRate"]);
                storedData.Users[player.userID].CollectableRate = Convert.ToInt32(Config["DefaultCollectable"]);
                SaveData();
                return true;
            }
            else
            {
                return false;
            }  
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
            ["IG_HELP"] = "<color='#DD0000'>[IGather]Possible Commands:</igather gather> Shows your current gather rate of all 3 types.",
            ["IG_AHELP"] = "<color='#DD0000'>[IGather]Possible Commands:</igather set> Shows set help. | </igather default> Shows default help. | </igather defaultall> Sets all players gather rate to default. |</color>",
            ["IG_AAHELP"] = "<color='#DD0000'>[IGather]Possible Commands:</igather setall> Shows setall help. | </igather gatherp (player)> Shows the gather rate for a player.</color>",
			}, this);
            
            LoadPermissions();
        }
        
        ////////////////////////////////////////////////////
        //Permission Settings///////////////////////////////
        ////////////////////////////////////////////////////        
        
        void LoadPermissions()
        {
            permission.RegisterPermission("igather.admin", this);          
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
                if (permission.UserHasPermission(player.userID.ToString(), "pgather.admin"))
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
                case "set":
                    if(args.Length <= 3)
                    {
                        SendReply(player, string.Format(GetMessage("IG_SETHELP", player.UserIDString)));
                        return;
                    }
                    if (permission.UserHasPermission(player.userID.ToString(), "pgather.admin"))
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
                break;
                         
                case "default":
                    if(args.Length <= 2 || args.Length >= 4)
                    {
                        SendReply(player, string.Format(GetMessage("IG_DEFAULTHELP", player.UserIDString)));
                        return;
                    }                
                    if (permission.UserHasPermission(player.userID.ToString(), "pgather.admin"))
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
                break;
                
                case "defaultall":
                    if (!permission.UserHasPermission(player.userID.ToString(), "pgather.admin"))
                    {
                        return;
                    }
                    DefaultAll();
                break;
                
                case "setall":
                    if (!permission.UserHasPermission(player.userID.ToString(), "pgather.admin"))
                    {
                        return;
                    }
                    if(args.Length <= 1)
                    {
                        SendReply(player, string.Format(GetMessage("IG_SETALLHELP", player.UserIDString)));
                        return;
                    }
                    if(args[1] == "quarry")
                    {
                        var amount = Convert.ToInt32(args[2]);
                        quarryall(amount);  
                    }
                    else if(args[1] == "resource")
                    {
                        var amount = Convert.ToInt32(args[2]);
                        resourceall(amount);   
                    }
                    else if(args[1] == "collectable")
                    {
                        var amount = Convert.ToInt32(args[2]);
                        collectableall(amount); 
                    }
                    else if(args[1] == "all")
                    {
                        var amount = Convert.ToInt32(args[2]);
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
                    if (permission.UserHasPermission(player.userID.ToString(), "pgather.admin"))
                    {
                        object addPlayer = FindPlayer(args[1]);             
                        BasePlayer target = (BasePlayer)addPlayer;
                    
                        SendReply(player, string.Format(GetMessage("IG_GATHERPLAYER", player.UserIDString), target.displayName, storedData.Users[target.userID].CollectableRate, storedData.Users[target.userID].PlayerQuarry, storedData.Users[target.userID].ResourceRate));                      
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