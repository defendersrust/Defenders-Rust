using Oxide.Core;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ConVar;

namespace Oxide.Plugins
{
    [Info("IGather", "DylanSMR", "1.0.2", ResourceId = 1763)]
    [Description("Adds the ability to set a gather rate for each person.")]
    class IGather : RustPlugin
    {
        public List<string> ListHelp = new List<string>();
        public List<string> ListTypes = new List<string>();
        public List<string> ListGather = new List<string>();
        
        //////////////////////////////////////////////////////////////////////////////////////
        // Configuration File
        ////////////////////////////////////////////////////////////////////////////////////// 
        
        void LoadDefaultConfig()
        {
            Puts("Creating a new configuration file.");
            Config.Clear();
                //DEFAULT CONFIGURATION FILES//
                Config["DefaultQuarryRate"] = 1;
                Config["DefaultResourceRate"] = 1;
                Config["DefaultCollectable"] = 1;
                //VIP CONFIGURATION FILES//
                Config["VIPQuarryRate"] = 5;
                Config["VIPResourceRate"] = 5;
                Config["VIPCollectable"] = 5; 
                //OTHER CONFIGURATION FILES//
                Config["TimeBetweenSave"] = 300; 
                //GROUP CONFIGURATION FILES//     
            Config.Save();        
        }
        
        //////////////////////////////////////////////////////////////////////////////////////
        // Loaded
        //////////////////////////////////////////////////////////////////////////////////////            
        
        void Loaded()
        {
            storedData = Interface.GetMod().DataFileSystem.ReadObject<StoredData>(this.Title);
            //VIP//
                permission.RegisterPermission("igather.vip", this);
            //ADMIN//
                permission.RegisterPermission("igather.admin", this);
                permission.RegisterPermission("igather.defaultall", this);
                permission.RegisterPermission("igather.default", this);
                permission.RegisterPermission("igather.set", this);
                permission.RegisterPermission("igather.setall", this);
                permission.RegisterPermission("igather.gatherp", this);
                permission.RegisterPermission("igather.setvip", this);
            //Add Language Options//
                AddLang(); 
            //Add Help//
                SetHelp();
            //Start Timer//
                StartTimers();             
        }        
        
        //////////////////////////////////////////////////////////////////////////////////////
        // Auto Save Timer
        //////////////////////////////////////////////////////////////////////////////////////    
        
        void StartTimers()
        {
            timer.Once(Convert.ToInt32(Config["TimeBetweenSave"]), () =>
            {
                SaveData();
                Puts("Data Saved!!!");      
            });
        }        
        
        //////////////////////////////////////////////////////////////////////////////////////
        // AddLang
        //////////////////////////////////////////////////////////////////////////////////////            
        
        void AddLang()
        {
			lang.RegisterMessages(new Dictionary<string,string>{
                ["IG_NOTAUTH"] = "<color='#aeff00'>[IGather]You do not have the correct permissions to preform this command.</color>",
                ["IG_HELP"] = "{0}",
                ["IG_SETTO"] = "<color='#aeff00'>[IGather]You have set {0}'s {1} too ({2}x).</color>",
                ["IG_SETBYTO"] = "<color='#aeff00'>[IGather]{0} has set your {1} too ({2}x).</color>", 
                ["IG_DEFAULTTO"] = "<color='#aeff00'>[IGather]You have defaulted {0}'s {1} to the default rate({2}x).</color>",
                ["IG_DEFAULTBYTO"] = "<color='#aeff00'>[IGather]You'r {0} was set to the default rate by {1} which is ({2}x).</color>",  
                ["IG_SETALL"] = "<color='#aeff00'>[IGather]{0} has set all {1} to ({2}x).</color>", 
                ["IG_DEFAULTALL"] = "<color='#aeff00'>[IGather]{0} has defaulted  all of your gather rates to the default rate!</color>",      
			}, this);            
        }
        
        //////////////////////////////////////////////////////////////////////////////////////
        // Catch Language Strings
        //////////////////////////////////////////////////////////////////////////////////////       
        
        private string GetMessage(string name, string sid = null) {
            return lang.GetMessage(name, this, sid);
        }
        
        //////////////////////////////////////////////////////////////////////////////////////
        // Data Files
        //////////////////////////////////////////////////////////////////////////////////////    
                
        Dictionary<ulong, BasePlayer> quarryOwners = new Dictionary<ulong, BasePlayer>();
                
        void SaveData()
        {
            Interface.Oxide.DataFileSystem.WriteObject(this.Title, storedData);    
        }
            
        class StoredData
        {
            public Dictionary<ulong, PlayerGather> Gathers = new Dictionary<ulong, PlayerGather>();
            public StoredData()
            {          
            }   
        }
        
        class PlayerGather
        {
            public bool isVIP;
            public int QuarryRate;
            public int ResourceRate;
            public int CollectableRate;
            public string Name;
            public float NameID;
            public PlayerGather()
            {          
            }
        }
        
        StoredData storedData; 
        
        //////////////////////////////////////////////////////////////////////////////////////
        // Write Data To Player
        ////////////////////////////////////////////////////////////////////////////////////// 
        
        void OnPlayerInit(BasePlayer player)
        {
            if (!storedData.Gathers.ContainsKey(player.userID)) InitUserData(player);             
        }     
        
        void SetHelp()
        {
            //Help List//
            ListHelp.Add("<color='#aeff00'>[IGather]--------------------Set Commands--------------------</color>"); 
            ListHelp.Add("<color='#36939e'>[IGather]</igather set (player) (r-type) (r-amount[X])> | Sets a players gather rate to (R-TYPE) and (R-AMOUNT).</color>");
            ListHelp.Add("<color='#36939e'>[IGather]</igather default (player) (r-type)> | Sets a players (R-TYPE) to default.</color>");
            ListHelp.Add("<color='#aeff00'>[IGather]------------------Gather Commands-------------------</color>"); 
            ListHelp.Add("<color='#36939e'>[IGather]</igather gather> | Prints out a players gather rate to him.</color>"); 
            ListHelp.Add("<color='#36939e'>[IGather]</igather gatherp (player) | Prints a targets gather rate to a player.</color>");     
            ListHelp.Add("<color='#aeff00'>[IGather]-------------------All Commands---------------------</color>"); 
            ListHelp.Add("<color='#36939e'>[IGather]</igather defaultall> | Defaults all players gather rates to the default rates.</color>");     
            ListHelp.Add("<color='#36939e'>[IGather]</igather setall (r-type) (r-amount[X])> | Sets all players (R-TYPES) to (R-AMOUNT).</color>");   
            ListHelp.Add("<color='#36939e'>[IGather]</igather addvip (player)> | Sets a players data to VIP if they have the permission.</color>");
            ListHelp.Add("<color='#aeff00'>[IGather]--------------------Other Commands------------------</color>");   
            ListHelp.Add("<color='#36939e'>[IGather]</igather help> | Gives you a list exactly like this one.</color>"); 
            ListHelp.Add("<color='#36939e'>[IGather]</igather types> | Gives you a list of all (R-TYPES) for /igather set or /igather default.</color>");                     
            //Type List//
            ListTypes.Add("<color='#aeff00'>[IGather]Types:</color>");
            ListTypes.Add("<color='#36939e'>[IGather]<resource> | Tree's, Stone's, Animals.</color>");
            ListTypes.Add("<color='#36939e'>[IGather]<collectable> | Collectable items that are on the ground(Wood piles, stone piles, etc.)</color>");
            ListTypes.Add("<color='#36939e'>[IGather]<quarry> | Quarry rates - the amount a quarry creates.</color>");
            ListTypes.Add("<color='#36939e'>[IGather]<all> | Sets every type of gather rate shown above |^^^|.</color>");               
        }
        
        private bool InitUserData(BasePlayer player)
        {
            //Add Player Data//        
            if(!storedData.Gathers.ContainsKey(player.userID) && !permission.UserHasPermission(player.userID.ToString(), "igather.vip"))
            {
                storedData.Gathers.Add(player.userID, new PlayerGather());
                storedData.Gathers[player.userID].Name = player.displayName;
                storedData.Gathers[player.userID].NameID = player.userID;
                storedData.Gathers[player.userID].QuarryRate = Convert.ToInt32(Config["DefaultQuarryRate"]);
                storedData.Gathers[player.userID].ResourceRate = Convert.ToInt32(Config["DefaultResourceRate"]);
                storedData.Gathers[player.userID].CollectableRate = Convert.ToInt32(Config["DefaultCollectable"]);
                storedData.Gathers[player.userID].isVIP = false;
                SaveData();                
                return true;
            }                                  
            else
            {
                return false;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // Player Finder
        ////////////////////////////////////////////////////////////////////////////////////// 
        
        private BasePlayer FindPlayer(BasePlayer player, string arg)
        {
            var foundPlayers = new List<BasePlayer>();
            ulong steamid;
            ulong.TryParse(arg, out steamid);
            string lowerarg = arg.ToLower();

            foreach (var p in BasePlayer.activePlayerList)
            {
                if (p != null)
                {
                    if (steamid != 0L)
                        if (p.userID == steamid) return p;
                    string lowername = p.displayName.ToLower();
                    if (lowername.Contains(lowerarg))
                    {
                        foundPlayers.Add(p);
                    }
                }
            }
            if (foundPlayers.Count == 0)
            {
                foreach (var sleeper in BasePlayer.sleepingPlayerList)
                {
                    if (sleeper != null)
                    {
                        if (steamid != 0L)
                            if (sleeper.userID == steamid)
                            {
                                foundPlayers.Clear();
                                foundPlayers.Add(sleeper);
                                return foundPlayers[0];
                            }
                        string lowername = player.displayName.ToLower();
                        if (lowername.Contains(lowerarg))
                        {
                            foundPlayers.Add(sleeper);
                        }
                    }
                }
            }
            if (foundPlayers.Count == 0)
            {
                if (player != null)
                    SendReply(player, string.Format(GetMessage("noPlayers", player.UserIDString)));
                return null;
            }
            if (foundPlayers.Count > 1)
            {
                if (player != null)
                    SendReply(player, string.Format(GetMessage("multiPlayers", player.UserIDString)));
                return null;
            }

            return foundPlayers[0];
        }        
        
        //////////////////////////////////////////////////////////////////////////////////////
        // Unload Save
        //////////////////////////////////////////////////////////////////////////////////////            
  
        void Unload()
        {
            SaveData();
        }        
            
        //////////////////////////////////////////////////////////////////////////////////////
        // isAuth boolean
        ////////////////////////////////////////////////////////////////////////////////////// 

        bool isAuth(BasePlayer player)
        {
            if (player.net.connection != null)            
                if (player.net.connection.authLevel < 1)
                    return false; 
            return true;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////
        // Chat Commands And Such
        //////////////////////////////////////////////////////////////////////////////////////         
        
        [ChatCommand("igather")]
        void GatherCommands(BasePlayer player, string command, string[] args)
        {                   
            if(args.Length == 0)
            {    
                foreach(var entry in ListHelp)
                {
                    SendReply(player, "<color='#36939e'>"+entry+"</color>");
                } 
                return;                
            }
            if(args.Length >= 5)
            { 
                foreach(var entry in ListHelp)
                {
                    SendReply(player, "<color='#36939e'>"+entry+"</color>");
                } 
                return;               
            }            
            switch(args[0])
            {
                //Help Commands//
                case "help":
                    foreach(var entry in ListHelp)
                    {
                        SendReply(player, "<color='#36939e'>"+entry+"</color>");
                    } 
                    return;                  
                break;
                //Set Commands//
                case "set":
                    if(args.Length <= 3 || args.Length == 2 || args.Length == 1 || args.Length == 0)
                    {
                        foreach(var entry in ListHelp)
                        {
                            SendReply(player, "<color='#36939e'>"+entry+"</color>");
                        } 
                        return;       
                    }
                    if(permission.UserHasPermission(player.userID.ToString(), "igather.admin") || permission.UserHasPermission(player.userID.ToString(), "igather.set") || isAuth(player))
                    {
                        object addPlayer = FindPlayer(player, args[1]);             
                        BasePlayer target = (BasePlayer)addPlayer;
                        
                        var NewData = Convert.ToInt32(args[3]);
                        
                        switch(args[2])
                        {
                            //Set Resource//
                            case "resource":
                                //Set Gather//
                                    storedData.Gathers[player.userID].ResourceRate = NewData;
                                    SaveData();
                                //Reply//
                                    SendReply(player, string.Format(GetMessage("IG_SETTO", player.UserIDString), target.displayName, "Resource-Rate", NewData));
                                    SendReply(target, string.Format(GetMessage("IG_SETBYTO", player.UserIDString), player.displayName, "Resource-Rate", NewData));
                                return;
                            break;
                            //Set Quarry//
                            case "quarry":
                                //Set Gather//
                                    storedData.Gathers[player.userID].QuarryRate = NewData;
                                    SaveData();
                                //Reply//
                                    SendReply(player, string.Format(GetMessage("IG_SETTO", player.UserIDString), target.displayName, "Quarry-Rate", NewData));
                                    SendReply(target, string.Format(GetMessage("IG_SETBYTO", player.UserIDString), player.displayName, "Quarry-Rate", NewData));  
                                return;                              
                            break;
                            //Set Collectable//
                            case "collectable":
                                //Set Gather//
                                    storedData.Gathers[player.userID].CollectableRate = NewData;
                                    SaveData();
                                //Reply//
                                    SendReply(player, string.Format(GetMessage("IG_SETTO", player.UserIDString), target.displayName, "Collectable-Rate", NewData));
                                    SendReply(target, string.Format(GetMessage("IG_SETBYTO", player.UserIDString), player.displayName, "Collectable-Rate", NewData));   
                                return;                            
                            break;
                            //Set All//
                            case "all":
                                //Set Gather//
                                    storedData.Gathers[player.userID].ResourceRate = NewData;
                                    storedData.Gathers[player.userID].QuarryRate = NewData;
                                    storedData.Gathers[player.userID].CollectableRate = NewData;
                                    SaveData();
                                SaveData();
                                //Reply//
                                    SendReply(player, string.Format(GetMessage("IG_SETTO", player.UserIDString), target.displayName, "All-Rate", NewData));
                                    SendReply(target, string.Format(GetMessage("IG_SETBYTO", player.UserIDString), player.displayName, "All-Rate", NewData));   
                                return;                               
                            break;
                            //Auto Default//                
                            default:
                                foreach(var entry in ListHelp)
                                {
                                    SendReply(player, "<color='#36939e'>"+entry+"</color>");
                                } 
                                return;                              
                            break;
                        }                              
                    }
                    else
                    {
                        SendReply(player, string.Format(GetMessage("IG_NOTAUTH", player.UserIDString)));
                        return;       
                    }                                  
                break;
                //Default Commands//
                case "default":
                    if(args.Length <= 2 || args.Length == 1 || args.Length == 0)
                    {
                        foreach(var entry in ListHelp)
                        {
                            SendReply(player, "<color='#36939e'>"+entry+"</color>");
                        } 
                        return;       
                    }
                    if(permission.UserHasPermission(player.userID.ToString(), "igather.admin") || permission.UserHasPermission(player.userID.ToString(), "igather.default") || isAuth(player))
                    {
                        object addPlayer = FindPlayer(player, args[1]);             
                        BasePlayer target = (BasePlayer)addPlayer;
                        
                        switch(args[2])
                        {
                            case "resource":
                                //Default Gather//
                                    storedData.Gathers[player.userID].ResourceRate = Convert.ToInt32(Config["DefaultResourceRate"]);
                                    SaveData();
                                //Reply//
                                    SendReply(player, string.Format(GetMessage("IG_DEFAULTTO", player.UserIDString), target.displayName, "Resource-Rate", Convert.ToInt32(Config["DefaultResourceRate"])));
                                    SendReply(target, string.Format(GetMessage("IG_DEFAULTBYTO", player.UserIDString), "Resource-Rate", player.displayName, Convert.ToInt32(Config["DefaultResourceRate"])));
                                return;                                  
                            break;

                            case "quarry":
                                //Default Gather//
                                    storedData.Gathers[player.userID].QuarryRate = Convert.ToInt32(Config["DefaultQuarryRate"]);
                                    SaveData();
                                //Reply//
                                    SendReply(player, string.Format(GetMessage("IG_DEFAULTTO", player.UserIDString), target.displayName, "Quarry-Rate", Convert.ToInt32(Config["DefaultQuarryRate"])));
                                    SendReply(target, string.Format(GetMessage("IG_DEFAULTBYTO", player.UserIDString), "Quarry-Rate", player.displayName, Convert.ToInt32(Config["DefaultQuarryRate"])));
                                return;                                  
                            break;
                            
                            case "collectable":
                                //Default Gather//
                                    storedData.Gathers[player.userID].CollectableRate = Convert.ToInt32(Config["DefaultCollectable"]);
                                    SaveData();
                                //Reply//
                                    SendReply(player, string.Format(GetMessage("IG_DEFAULTTO", player.UserIDString), target.displayName, "Collectable-Rate", Convert.ToInt32(Config["DefaultCollectable"])));
                                    SendReply(target, string.Format(GetMessage("IG_DEFAULTBYTO", player.UserIDString), "Collectable-Rate", player.displayName, Convert.ToInt32(Config["DefaultCollectable"])));
                                return;                                  
                            break;
                            
                            case "all":
                                //Vars//
                                var newT = "?";
                                //Default Gather//
                                    storedData.Gathers[player.userID].ResourceRate = Convert.ToInt32(Config["DefaultResourceRate"]);
                                    storedData.Gathers[player.userID].QuarryRate = Convert.ToInt32(Config["DefaultQuarryRate"]);
                                    storedData.Gathers[player.userID].CollectableRate = Convert.ToInt32(Config["DefaultCollectable"]);
                                    SaveData();
                                //Reply//
                                    SendReply(player, string.Format(GetMessage("IG_DEFAULTTO", player.UserIDString), target.displayName, "Resource-Rate", newT));
                                    SendReply(target, string.Format(GetMessage("IG_DEFAULTBYTO", player.UserIDString), "Resource-Rate", player.displayName, newT));
                                return;                                  
                            break;                            
                            default:
                                foreach(var entry in ListHelp)
                                {
                                    SendReply(player, "<color='#36939e'>"+entry+"</color>");
                                } 
                                return;                              
                            break;                            
                        }                              
                    }
                    else
                    {
                        SendReply(player, string.Format(GetMessage("IG_NOTAUTH", player.UserIDString)));
                        return;      
                    }                     
                break;
                //Types Commands//
                case "types":
                    foreach(var entry in ListTypes)
                    {
                        SendReply(player, "<color='#36939e'>"+entry+"</color>");
                    }
                    return;           
                break;
                
                //Gather Commands//
                case "gather":
                    SendReply(player, "<color='#aeff00'>[IGather]Rates:</color>");
                    SendReply(player, "<color='#36939e'>[IGather]Gather Rate:"+storedData.Gathers[player.userID].ResourceRate+"</color>");
                    SendReply(player, "<color='#36939e'>[IGather]Quarry Rate:"+storedData.Gathers[player.userID].QuarryRate+"</color>");
                    SendReply(player, "<color='#36939e'>[IGather]Collectable Rate:"+storedData.Gathers[player.userID].CollectableRate+"</color>");
                    return;
                break;
                //GatherP Commands//
                case "gatherp":
                    if(args.Length == 0)
                    {
                        foreach(var entry in ListHelp)
                        {
                            SendReply(player, "<color='#36939e'>"+entry+"</color>");
                        } 
                        return;                           
                    }
                    if(permission.UserHasPermission(player.userID.ToString(), "igather.admin") || permission.UserHasPermission(player.userID.ToString(), "igather.gatherp") || isAuth(player))
                    {
                        object addPlayer = FindPlayer(player, args[1]);             
                        BasePlayer target = (BasePlayer)addPlayer;

                        SendReply(player, "<color='#aeff00'>[IGather]Target Rates:</color>");
                        SendReply(player, "<color='#36939e'>[IGather]Target Gather Rate:"+storedData.Gathers[target.userID].ResourceRate+"</color>");
                        SendReply(player, "<color='#36939e'>[IGather]Target Quarry Rate:"+storedData.Gathers[target.userID].QuarryRate+"</color>");
                        SendReply(player, "<color='#36939e'>[IGather]Target Collectable Rate:"+storedData.Gathers[target.userID].CollectableRate+"</color>");
                        return;
                    }
                    else
                    {
                        SendReply(player, string.Format(GetMessage("IG_NOTAUTH", player.UserIDString)));
                        return;                            
                    }                                            
                break;
                //SetAll Commands//
                case "setall":
                    if(args.Length >= 4 || args.Length == 1 || args.Length == 2)
                    {
                        foreach(var entry in ListHelp)
                        {
                            SendReply(player, "<color='#36939e'>"+entry+"</color>");
                        } 
                        return;                             
                    } 
                    if(permission.UserHasPermission(player.userID.ToString(), "igather.admin") || permission.UserHasPermission(player.userID.ToString(), "igather.setall") || isAuth(player))
                    {
                        var NewData = Convert.ToInt32(args[2]);
                        switch(args[1])
                        {
                            case "resource":
                                foreach(var p in BasePlayer.activePlayerList)
                                {
                                    storedData.Gathers[p.userID].ResourceRate = NewData;
                                    SaveData();                                     
                                }                              
                                PrintToChat(string.Format(GetMessage("IG_SETALL", player.UserIDString), player.displayName, "Resource-Rate", NewData));        
                            break;
                         
                            case "quarry":
                                foreach(var p in BasePlayer.activePlayerList)
                                {
                                    storedData.Gathers[p.userID].QuarryRate = NewData;
                                    SaveData();                                       
                                }                               
                                PrintToChat(string.Format(GetMessage("IG_SETALL", player.UserIDString), player.displayName, "Quarry-Rate", NewData));                                  
                            break;
                            
                            case "collectable":
                                foreach(var p in BasePlayer.activePlayerList)
                                {
                                    storedData.Gathers[p.userID].CollectableRate = NewData;
                                    SaveData(); 
                                }
                                PrintToChat(string.Format(GetMessage("IG_SETALL", player.UserIDString), player.displayName, "Collectable-Rate", NewData));                                  
                            break;                          
                            
                            case "all":
                                foreach(var p in BasePlayer.activePlayerList)
                                {
                                    storedData.Gathers[p.userID].ResourceRate = NewData;
                                    storedData.Gathers[p.userID].CollectableRate = NewData;
                                    storedData.Gathers[p.userID].QuarryRate = NewData;
                                    SaveData();                                    
                                }
                                PrintToChat(string.Format(GetMessage("IG_SETALL", player.UserIDString), player.displayName, "All-Rate", NewData));                                  
                            break;
                            
                            default:
                                foreach(var entry in ListHelp)
                                {
                                    SendReply(player, "<color='#36939e'>"+entry+"</color>");
                                } 
                                return;                              
                            break;                                 
                        } 
                    } 
                    else 
                    {
                        SendReply(player, string.Format(GetMessage("IG_NOTAUTH", player.UserIDString)));
                        return;                            
                    }           
                break;
                //DefaultAll Command//
                case "defaultall":
                    if(args.Length >= 2)
                    {
                        foreach(var entry in ListHelp)
                        {
                            SendReply(player, "<color='#36939e'>"+entry+"</color>");
                        } 
                        return;                         
                    }
                    if(permission.UserHasPermission(player.userID.ToString(), "igather.admin") || permission.UserHasPermission(player.userID.ToString(), "igather.defaultall") || isAuth(player))
                    {
                        foreach(var p in BasePlayer.activePlayerList)
                        {
                            storedData.Gathers[p.userID].ResourceRate = Convert.ToInt32(Config["DefaultResourceRate"]);
                            storedData.Gathers[p.userID].CollectableRate = Convert.ToInt32(Config["DefaultCollectable"]);
                            storedData.Gathers[p.userID].QuarryRate = Convert.ToInt32(Config["DefaultQuarryRate"]);
                            SaveData();   
                        }
                        PrintToChat(string.Format(GetMessage("IG_DEFAULTALL", player.UserIDString), player.displayName));                        
                    }
                    else 
                    {
                        SendReply(player, string.Format(GetMessage("IG_NOTAUTH", player.UserIDString)));
                    }                
                break;
                //WipeData Commands//
                case "addvip":
                    if(args.Length == 0)
                    {
                        foreach(var entry in ListHelp)
                        {
                            SendReply(player, "<color='#36939e'>"+entry+"</color>");
                        } 
                        return;                             
                    }
                    if(permission.UserHasPermission(player.userID.ToString(), "igather.admin") || permission.UserHasPermission(player.userID.ToString(), "igather.setvip") || isAuth(player))
                    {
                        object addPlayer = FindPlayer(player, args[1]);             
                        BasePlayer target = (BasePlayer)addPlayer;
                        
                        storedData.Gathers.Remove(player.userID);
                        
                        if(!storedData.Gathers.ContainsKey(target.userID) && !permission.UserHasPermission(target.userID.ToString(), "igather.vip"))
                        {
                            storedData.Gathers.Add(target.userID, new PlayerGather());
                            storedData.Gathers[target.userID].Name = target.displayName;
                            storedData.Gathers[target.userID].NameID = target.userID;
                            storedData.Gathers[target.userID].QuarryRate = Convert.ToInt32(Config["DefaultQuarryRate"]);
                            storedData.Gathers[target.userID].ResourceRate = Convert.ToInt32(Config["DefaultResourceRate"]);
                            storedData.Gathers[target.userID].CollectableRate = Convert.ToInt32(Config["DefaultCollectable"]);
                            storedData.Gathers[target.userID].isVIP = false;
                            SaveData();                
                        }                 
                        else if(!storedData.Gathers.ContainsKey(player.userID) && permission.UserHasPermission(player.userID.ToString(), "igather.vip"))
                        {
                            storedData.Gathers.Add(target.userID, new PlayerGather());
                            storedData.Gathers[target.userID].Name = target.displayName;
                            storedData.Gathers[target.userID].NameID = target.userID;
                            storedData.Gathers[target.userID].QuarryRate = Convert.ToInt32(Config["VIPQuarryRate"]);
                            storedData.Gathers[target.userID].ResourceRate = Convert.ToInt32(Config["VIPResourceRate"]);
                            storedData.Gathers[target.userID].CollectableRate = Convert.ToInt32(Config["VIPCollectable"]);
                            storedData.Gathers[target.userID].isVIP = true;
                            SaveData();                              
                        }                                                                     
                    }
                    else 
                    {
                        SendReply(player, string.Format(GetMessage("IG_NOTAUTH", player.UserIDString)));
                    }                 
                break;                           
            } 
       }
       
        //////////////////////////////////////////////////////////////////////////////////////
        // Gather Rate Changes
        //////////////////////////////////////////////////////////////////////////////////////   
        
        private void OnDispenserGather(ResourceDispenser dispenser, BaseEntity entity, Item item)
        {
            BasePlayer player = entity.ToPlayer();
            if(player == null) return;
            if (!storedData.Gathers.ContainsKey(player.userID)) InitUserData(player);
            else
                item.amount = (int)(item.amount * storedData.Gathers[player.userID].ResourceRate);
        }

        private void OnQuarryGather(MiningQuarry quarry, Item item)
        {
            BasePlayer player;
            player = BasePlayer.FindByID(quarry.OwnerID) ?? BasePlayer.FindSleeping(quarry.OwnerID);
            quarryOwners[quarry.OwnerID] = player;            
            if(player == null) return;
            if (!storedData.Gathers.ContainsKey(player.userID)) InitUserData(player);
            else            
                item.amount = (int)(item.amount * storedData.Gathers[player.userID].QuarryRate);         
        }

        private void OnCollectiblePickup(Item item, BasePlayer player)
        {
            if(player == null) return;
            if (!storedData.Gathers.ContainsKey(player.userID)) InitUserData(player);
            else            
                item.amount = (int)(item.amount * storedData.Gathers[player.userID].CollectableRate);      
        }                                                        
    }
}