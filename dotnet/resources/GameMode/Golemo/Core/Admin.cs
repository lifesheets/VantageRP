﻿using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Data;
using Golemo.GUI;
using GolemoSDK;

namespace Golemo.Core
{
    class Admin : Script
    {
        private static nLog Log = new nLog("Admin");
        public static bool IsServerStoping = false;

        [ServerEvent(Event.ResourceStart)]
        public void Event_ResourceStart()
        {
            ColShape colShape = NAPI.ColShape.CreateCylinderColShape(DemorganPosition, 100, 50, 1337);
            colShape.OnEntityExitColShape += (s, e) =>
            {
                if (!Main.Players.ContainsKey(e)) return;
                if (Main.Players[e].DemorganTime > 0) NAPI.Entity.SetEntityPosition(e, DemorganPosition + new Vector3(0, 0, 1.5));
            };
            Group.LoadCommandsConfigs();
        }

        [RemoteEvent("openAdminPanel")]
        private static void OpenAdminPanel(Player player)
        {
            CharacterData acc = Main.Players[player];
            List<Group.GroupCommand> cmds = new List<Group.GroupCommand>();
            List<object> players = new List<object>();
            if (acc.AdminLVL > 0)
            {
                foreach (Group.GroupCommand item in Group.GroupCommands)
                {
                    if (item.IsAdmin)
                    {
                        if (item.MinLVL <= acc.AdminLVL)
                        {
                            cmds.Add(item);
                        }
                    }
                }
                foreach (var p in Main.Players.Keys.ToList())
                {
                    string[] data = { Main.Players[p].AdminLVL.ToString(), p.Value.ToString(), p.Name.ToString(), p.Ping.ToString() };
                    players.Add(data);
                }
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(cmds);
                string json2 = Newtonsoft.Json.JsonConvert.SerializeObject(players);
                Trigger.ClientEvent(player, "openAdminPanel", json, json2);
            }
            cmds.Clear();
            players.Clear();
        }

        [RemoteEvent("getPlayerInfoToAdminPanel")]
        private static void LoadPlayerInfoToPanel(Player player, int id)
        {
            Player target = Main.GetPlayerByID(id);
            if (target == null) return;
            CharacterData ccr = Main.Players[target];
            AccountData acc = Main.Accounts[target];
            Houses.House house = Houses.HouseManager.GetHouse(target);
            int houseID = -1;
            if (house != null) houseID = house.ID;
            List<object> data = new List<object>()
            {
                new Dictionary<string, object>()
                {
                    { "Character", ccr },
                    { "Account", acc },
                    { "Props", new List<object>()
                        {
                            houseID,
                            MoneySystem.Bank.Accounts[ccr.Bank].Balance,
                        }
                    }
                }
            };
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            Trigger.ClientEvent(player, "loadPlayerInfo", json);
        }

        public static void sendRedbucks(Player player, Player target, int amount)
        {
            if (!Group.CanUseCmd(player, "givereds")) return;

            if (Main.Accounts[target].RedBucks + amount < 0) amount = 0;
            Main.Accounts[target].RedBucks += amount;
            Trigger.ClientEvent(target, "starset", Main.Accounts[target].RedBucks);

            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"you sent {target.Name} {amount} redbucks", 3000);
            Notify.Send(target, NotifyType.Success, NotifyPosition.BottomCenter, $"+{amount} redbucks", 3000);

            GameLog.Admin(player.Name, $"givereds({amount})", target.Name);
        }

        public static void stopServer(string reason = "Сервер выключен.")
        {
            IsServerStoping = true;
            GameLog.Admin("server", $"stopServer({reason})", "");

            Log.Write("Force saving database...", nLog.Type.Warn);
            BusinessManager.SavingBusiness();
            Fractions.GangsCapture.SavingRegions();
            Houses.HouseManager.SavingHouses();
            Houses.FurnitureManager.Save();
            nInventory.SaveAll();
            Fractions.Stocks.saveStocksDic();
            Weapons.SaveWeaponsDB();
            Log.Write("All data has been saved!", nLog.Type.Success);

            Log.Write("Force kicking players...", nLog.Type.Warn);
            foreach (Player player in NAPI.Pools.GetAllPlayers())
                NAPI.Task.Run(() => NAPI.Player.KickPlayer(player, reason));
            Log.Write("All players has kicked!", nLog.Type.Success);

            NAPI.Task.Run(() =>
            {
                Environment.Exit(0);
            }, 60000);
        }

        public static void saveCoords(Player player, string msg)
        {
            if (!Group.CanUseCmd(player, "save")) return;
            Vector3 pos = NAPI.Entity.GetEntityPosition(player);
            pos.Z -= 1.12f;
            Vector3 rot = NAPI.Entity.GetEntityRotation(player);
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                Vehicle vehicle = player.Vehicle;
                pos = NAPI.Entity.GetEntityPosition(vehicle) + new Vector3(0, 0, 0.5);
                rot = NAPI.Entity.GetEntityRotation(vehicle);
            }
            try
            {
                StreamWriter saveCoords = new StreamWriter("coords.txt", true, Encoding.UTF8);
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                saveCoords.Write($"{msg}   Coords: new Vector3({pos.X}, {pos.Y}, {pos.Z}),    JSON: {Newtonsoft.Json.JsonConvert.SerializeObject(pos)}      \r\n");
                saveCoords.Write($"{msg}   Rotation: new Vector3({rot.X}, {rot.Y}, {rot.Z}),     JSON: {Newtonsoft.Json.JsonConvert.SerializeObject(rot)}    \r\n");
                saveCoords.Close();
            }
            catch (Exception error)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Exeption: " + error);
            }
            finally
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Coords: " + NAPI.Entity.GetEntityPosition(player));
            }
        }
        public static void setPlayerAdminGroup(Player player, Player target)
        {
            if (!Group.CanUseCmd(player, "setadmin")) return;
            if (Main.Players[target].AdminLVL >= 1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The player already has an admin. right", 3000);
                return;
            }
            Main.Players[target].AdminLVL = 1;
            target.SetSharedData("IS_ADMIN", true);
            target.SetSharedData("ALVL", 1);
            Fractions.GangsCapture.LoadBlips(target);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"You issued an admin. player rights {target.Name}", 3000);
            Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"{player.Name} Given to you by the admin. rights", 3000);
            GameLog.Admin($"{player.Name}", $"setAdmin", $"{target.Name}");
        }
        public static void delPlayerAdminGroup(Player player, Player target)
        {
            if (!Group.CanUseCmd(player, "deladmin")) return;
            if (player == target)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You cannot take away the admin. own rights", 3000);
                return;
            }
            if (Main.Players[target].AdminLVL >= Main.Players[player].AdminLVL)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You cannot take away rights from this administrator", 3000);
                return;
            }
            if (Main.Players[target].AdminLVL < 1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The player does not have an admin. rights", 3000);
                return;
            }
            Main.Players[target].AdminLVL = 0;
            target.ResetSharedData("IS_ADMIN");
            target.SetSharedData("ALVL", 0);
            Fractions.GangsCapture.UnLoadBlips(target);

            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"You took away rights from the administrator {target.Name}", 3000);
            Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"{player.Name} taken from you by the admin. rights", 3000);
            GameLog.Admin($"{player.Name}", $"delAdmin", $"{target.Name}");
        }
        public static void setPlayerAdminRank(Player player, Player target, int rank)
        {
            if (!Group.CanUseCmd(player, "setadminrank")) return;
            if (player == target)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You cannot rank yourself", 3000);
                return;
            }
            if (Main.Players[target].AdminLVL < 1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "The player is not an administrator!", 3000);
                return;
            }
            if (Main.Players[target].AdminLVL >= Main.Players[player].AdminLVL)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You cannot change the permission level of this administrator", 3000);
                return;
            }
            if (rank < 1 || rank >= Main.Players[player].AdminLVL)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"It is impossible to issue such a rank", 3000);
                return;
            }
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"You gave the player {target.Name} {rank} admin level. rights", 3000);
            Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"{player.Name} gave you {rank} admin level. rights", 3000);
            Main.Players[target].AdminLVL = rank;
            //Main.AdminSlots[target.GetData("RealSocialClub")].AdminLVL = rank;
            GameLog.Admin($"{player.Name}", $"setAdminRank({rank})", $"{target.Name}");
        }
        public static void setPlayerVipLvl(Player player, Player target, int rank)
        {
            if (!Group.CanUseCmd(player, "setviplvl")) return;
            if (rank > 4 || rank < 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"It is impossible to issue such a level of VIP account", 3000);
                return;
            }
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"You gave the player {target.Name} {Group.GroupNames[rank]}", 3000);
            Main.Accounts[target].VipLvl = rank;
            Main.Accounts[target].VipDate = DateTime.Now.AddDays(30);
            GUI.Dashboard.sendStats(target);
            GameLog.Admin($"{player.Name}", $"setVipLvl({rank})", $"{target.Name}");
        }

        public static void setFracLeader(Player sender, Player target, int fracid)
        {
            if (!Group.CanUseCmd(sender, "setleader")) return;
            if (fracid != 0 && fracid <= 19)
            {
                Fractions.Manager.UNLoad(target);
                int index = Fractions.Manager.AllMembers.FindIndex(m => m.Name == target.Name);
                if (index > -1) Fractions.Manager.AllMembers.RemoveAt(index);

                int new_fraclvl = Fractions.Configs.FractionRanks[fracid].Count;
                Main.Players[target].FractionLVL = new_fraclvl;
                Main.Players[target].FractionID = fracid;
                Main.Players[target].WorkID = 0;
                if (fracid == 15)
                {
                    Trigger.ClientEvent(target, "enableadvert", true);
                    Fractions.LSNews.onLSNPlayerLoad(target);
                }
                Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"You became a faction leader {Fractions.Manager.getName(fracid)}", 3000);
                Notify.Send(sender, NotifyType.Info, NotifyPosition.BottomCenter, $"you bet {target.Name} на лидерство {Fractions.Manager.getName(fracid)}", 3000);
                Fractions.Manager.Load(target, fracid, new_fraclvl);
                Dashboard.sendStats(target);
                GameLog.Admin($"{sender.Name}", $"setFracLeader({fracid})", $"{target.Name}");
                return;
            }
        }
        public static void delFracLeader(Player sender, Player target)
        {
            if (!Group.CanUseCmd(sender, "delleader")) return;
            if (Main.Players[target].FractionID != 0 && Main.Players[target].FractionID <= 19)
            {
                if (Main.Players[target].FractionLVL < Fractions.Configs.FractionRanks[Main.Players[target].FractionID].Count)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"The player is not the leader", 3000);
                    return;
                }
                Fractions.Manager.UNLoad(target);
                int index = Fractions.Manager.AllMembers.FindIndex(m => m.Name == target.Name);
                if (index > -1) Fractions.Manager.AllMembers.RemoveAt(index);

                if (Main.Players[target].FractionID == 15) Trigger.ClientEvent(target, "enableadvert", false);

                Main.Players[target].OnDuty = false;
                Main.Players[target].FractionID = 0;
                Main.Players[target].FractionLVL = 0;

                Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"{sender.Name.Replace('_', ' ')} removed you from the faction leader", 3000);
                Notify.Send(sender, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы сняли {target.Name.Replace('_', ' ')} as faction leader", 3000);
                Dashboard.sendStats(target);

                Customization.ApplyCharacter(target);
                NAPI.Player.RemoveAllPlayerWeapons(target);
                GameLog.Admin($"{sender.Name}", $"delFracLeader", $"{target.Name}");
            }
            else Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"The player does not have a faction", 3000);
        }
        public static void delJob(Player sender, Player target)
        {
            if (!Group.CanUseCmd(sender, "deljob")) return;
            if (Main.Players[target].WorkID != 0)
            {
                if (NAPI.Data.GetEntityData(target, "ON_WORK") == true)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"The player must be out of work uniform", 3000);
                    return;
                }
                Main.Players[target].WorkID = 0;
                Dashboard.sendStats(target);
                Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"{sender.Name.Replace('_', ' ')} removed employment from your character", 3000);
                Notify.Send(sender, NotifyType.Info, NotifyPosition.BottomCenter, $"you took off {target.Name.Replace('_', ' ')} with employment", 3000);
                Dashboard.sendStats(target);
                GameLog.Admin($"{sender.Name}", $"delJob", $"{target.Name}");
            }
            else Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"player has no job", 3000);
        }
        public static void delFrac(Player sender, Player target)
        {
            if (!Group.CanUseCmd(sender, "delfrac")) return;
            if (Main.Players[target].FractionID != 0 && Main.Players[target].FractionID <= 17)
            {
                if (Main.Players[target].FractionLVL >= Fractions.Configs.FractionRanks[Main.Players[target].FractionID].Count)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"The player is the leader of the faction", 3000);
                    return;
                }
                Fractions.Manager.UNLoad(target);
                int index = Fractions.Manager.AllMembers.FindIndex(m => m.Name == target.Name);
                if (index > -1) Fractions.Manager.AllMembers.RemoveAt(index);

                if (Main.Players[target].FractionID == 15) Trigger.ClientEvent(target, "enableadvert", false);

                Main.Players[target].OnDuty = false;
                Main.Players[target].FractionID = 0;
                Main.Players[target].FractionLVL = 0;

                Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"Administrator {sender.Name.Replace('_', ' ')} kicked you out of the faction", 3000);
                Notify.Send(sender, NotifyType.Info, NotifyPosition.BottomCenter, $"you kicked out {target.Name.Replace('_', ' ')} from the faction", 3000);
                Dashboard.sendStats(target);

                Customization.ApplyCharacter(target);
                NAPI.Player.RemoveAllPlayerWeapons(target);
                GameLog.Admin($"{sender.Name}", $"delFrac", $"{target.Name}");
            }
            else Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"The player does not have a faction", 3000);
        }

        public static void teleportTargetToPlayerWithCar(Player player, Player target)
        {
            if (!Group.CanUseCmd(player, "tpcar")) return;
            NAPI.Entity.SetEntityPosition(target.Vehicle, player.Position);
            NAPI.Entity.SetEntityRotation(target.Vehicle, player.Rotation);
            NAPI.Entity.SetEntityDimension(target.Vehicle, player.Dimension);
            NAPI.Entity.SetEntityDimension(target, player.Dimension);
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"you teleported {target.Name} to yourself", 3000);
            Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"Administrator {player.Name} teleported you to me", 3000);
        }
        public static void adminLSnews(Player player, string message)
        {
            if (!Group.CanUseCmd(player, "lsn")) return;
            NAPI.Chat.SendChatMessageToAll("!{#D47C00}" + $"LS News от {player.Name.Replace('_', ' ')} ({player.Value}): {message}");
        }
        public static void giveMoney(Player player, Player target, int amount)
        {
            if (!Group.CanUseCmd(player, "givemoney")) return;
            GameLog.Money($"player({Main.Players[player].UUID})", $"player({Main.Players[target].UUID})", amount, "admin");
            MoneySystem.Wallet.Change(target, amount);
            GameLog.Admin($"{player.Name}", $"giveMoney({amount})", $"{target.Name}");
        }
        public static void OffMutePlayer(Player player, string target, int time, string reason)
        {
            try
            {
                if (!Group.CanUseCmd(player, "mute")) return;
                if (NAPI.Player.GetPlayerFromName(target) != null)
                {
                    mutePlayer(player, NAPI.Player.GetPlayerFromName(target), time, reason);
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "The player was online, so offmute was changed to mute", 3000);
                    return;
                }
                if (player.Name.Equals(target)) return;
                if (time > 480)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You can't give mut more than 480 minutes", 3000);
                    return;
                }
                var split = target.Split('_');
                MySQL.QueryRead($"UPDATE `characters` SET `unmute`={time * 60} WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                NAPI.Chat.SendChatMessageToAll($"!{{#f25c49}}{player.Name} выдал мут игроку {target} на {time} минут");
                NAPI.Chat.SendChatMessageToAll($"!{{#f25c49}}Причина: {reason}");
                GameLog.Admin($"{player.Name}", $"mutePlayer({time}, {reason})", $"{target}");
            }
            catch { }

        }
        public static void mutePlayer(Player player, Player target, int time, string reason)
        {
            if (!Group.CanUseCmd(player, "mute")) return;
            if (player == target) return;
            if (time > 480)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You can't give mut more than 480 minutes", 3000);
                return;
            }
            Main.Players[target].Unmute = time * 60;
            Main.Players[target].VoiceMuted = true;
            if (target.HasData("MUTE_TIMER")) Timers.Stop(target.GetData<string>("MUTE_TIMER"));
            NAPI.Data.SetEntityData(target, "MUTE_TIMER", Timers.StartTask(1000, () => timer_mute(target)));
            target.SetSharedData("voice.muted", true);
            Trigger.ClientEvent(target, "voice.mute");
            NAPI.Chat.SendChatMessageToAll($"!{{#f25c49}}{player.Name} gave a mute to a player {target.Name} на {time} minutes");
            NAPI.Chat.SendChatMessageToAll($"!{{#f25c49}}Причина: {reason}");
            GameLog.Admin($"{player.Name}", $"mutePlayer({time}, {reason})", $"{target.Name}");
        }
        public static void unmutePlayer(Player player, Player target)
        {
            if (!Group.CanUseCmd(player, "unmute")) return;

            Main.Players[target].Unmute = 2;
            Main.Players[target].VoiceMuted = false;
            target.SetSharedData("voice.muted", false);

            NAPI.Chat.SendChatMessageToAll($"!{{#f25c49}}{player.Name} unmuted a player {target.Name}");
            GameLog.Admin($"{player.Name}", $"unmutePlayer", $"{target.Name}");
        }
        public static void banPlayer(Player player, Player target, int time, string reason, bool isSilence)
        {
            string cmd = (isSilence) ? "sban" : "ban";
            if (!Group.CanUseCmd(player, cmd)) return;
            if (player == target) return;
            if (Main.Players[target].AdminLVL >= Main.Players[player].AdminLVL)
            {
                Commands.SendToAdmins(3, $"!{{#d35400}}[BAN-DENIED] {player.Name} ({player.Value}) tried to ban {target.Name} ({target.Value}), who has a higher administrator level.");
                return;
            }
            DateTime unbanTime = DateTime.Now.AddMinutes(time);
            string banTimeMsg = "м";
            if (time > 60)
            {
                banTimeMsg = "ч";
                time /= 60;
                if (time > 24)
                {
                    banTimeMsg = "д";
                    time /= 24;
                }
            }

            if (!isSilence)
                NAPI.Chat.SendChatMessageToAll($"!{{#f25c49}}{player.Name} banned a player {target.Name} на {time}{banTimeMsg}");
            NAPI.Chat.SendChatMessageToAll($"!{{#f25c49}}Reason: {reason}");

            Ban.Online(target, unbanTime, false, reason, player.Name);

            Notify.Send(target, NotifyType.Warning, NotifyPosition.Center, $"You are locked up{unbanTime.ToString()}", 30000);
            Notify.Send(target, NotifyType.Warning, NotifyPosition.Center, $"Reason: {reason}", 30000);

            int AUUID = Main.Players[player].UUID;
            int TUUID = Main.Players[target].UUID;

            GameLog.Ban(AUUID, TUUID, unbanTime, reason, false);

            target.Kick(reason);
        }
        public static void hardbanPlayer(Player player, Player target, int time, string reason)
        {
            if (!Group.CanUseCmd(player, "ban")) return;
            if (player == target) return;
            if (Main.Players[target].AdminLVL >= Main.Players[player].AdminLVL)
            {
                Commands.SendToAdmins(3, $"!{{#d35400}}[HARDBAN-DENIED] {player.Name} ({player.Value}) tried to ban {target.Name} ({target.Value}), who has a higher administrator level.");
                return;
            }
            DateTime unbanTime = DateTime.Now.AddMinutes(time);
            string banTimeMsg = "м";
            if (time > 60)
            {
                banTimeMsg = "ч";
                time /= 60;
                if (time > 24)
                {
                    banTimeMsg = "д";
                    time /= 24;
                }
            }
            NAPI.Chat.SendChatMessageToAll($"!{{#f25c49}}{player.Name} hit a player with a banhammer {target.Name} на {time}{banTimeMsg}");
            NAPI.Chat.SendChatMessageToAll($"!{{#f25c49}}Reason: {reason}");

            Ban.Online(target, unbanTime, true, reason, player.Name);

            Notify.Send(target, NotifyType.Warning, NotifyPosition.Center, $"You caught the banhammer before {unbanTime.ToString()}", 30000);
            Notify.Send(target, NotifyType.Warning, NotifyPosition.Center, $"Reason: {reason}", 30000);

            int AUUID = Main.Players[player].UUID;
            int TUUID = Main.Players[target].UUID;

            GameLog.Ban(AUUID, TUUID, unbanTime, reason, true);

            target.Kick(reason);
        }
        public static void offBanPlayer(Player player, string name, int time, string reason)
        {
            if (!Group.CanUseCmd(player, "offban")) return;
            if (player.Name == name) return;
            Player target = NAPI.Player.GetPlayerFromName(name);
            if (target != null)
            {
                if (Main.Players.ContainsKey(target))
                {
                    if (Main.Players[target].AdminLVL >= Main.Players[player].AdminLVL)
                    {
                        Commands.SendToAdmins(3, $"!{{#d35400}}[OFFBAN-DENIED] {player.Name} ({player.Value}) tried to ban {target.Name} ({target.Value}), who has a higher administrator level.");
                        return;
                    }
                    else
                    {
                        target.Kick();
                        Notify.Send(player, NotifyType.Success, NotifyPosition.Center, "The player was online, but was kicked.", 3000);
                    }
                }
            }
            else
            {
                string[] split = name.Split('_');
                DataTable result = MySQL.QueryRead($"SELECT adminlvl FROM characters WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                DataRow row = result.Rows[0];
                int targetadminlvl = Convert.ToInt32(row[0]);
                if (targetadminlvl >= Main.Players[player].AdminLVL)
                {
                    Commands.SendToAdmins(3, $"!{{#d35400}}[OFFBAN-DENIED] {player.Name} ({player.Value}) tried to ban {name} (offline), who has a higher administrator level.");
                    return;
                }
            }

            int AUUID = Main.Players[player].UUID;
            int TUUID = Main.PlayerUUIDs[name];

            Ban ban = Ban.Get2(TUUID);
            if (ban != null)
            {
                string hard = (ban.isHard) ? "хард " : "";
                Notify.Send(player, NotifyType.Warning, NotifyPosition.Center, $"The player is already {hard}bath", 3000);
                return;
            }

            DateTime unbanTime = DateTime.Now.AddMinutes(time);
            string banTimeMsg = "м"; // Можно использовать char
            if (time > 60)
            {
                banTimeMsg = "ч";
                time /= 60;
                if (time > 24)
                {
                    banTimeMsg = "д";
                    time /= 24;
                }
            }

            Ban.Offline(name, unbanTime, false, reason, player.Name);

            GameLog.Ban(AUUID, TUUID, unbanTime, reason, false);

            NAPI.Chat.SendChatMessageToAll($"!{{#f25c49}}{player.Name} banned a player {target.Name} на {time}{banTimeMsg}");
            NAPI.Chat.SendChatMessageToAll($"!{{#f25c49}}Reason: {reason}");
        }
        public static void offHardBanPlayer(Player player, string name, int time, string reason)
        {
            if (!Group.CanUseCmd(player, "offban")) return;
            if (player.Name.Equals(name)) return;
            Player target = NAPI.Player.GetPlayerFromName(name);
            if (target != null)
            {
                if (Main.Players.ContainsKey(target))
                {
                    if (Main.Players[target].AdminLVL >= Main.Players[player].AdminLVL)
                    {
                        Commands.SendToAdmins(3, $"!{{#d35400}}[OFFHARDBAN-DENIED] {player.Name} ({player.Value}) tried to ban {target.Name} ({target.Value}), who has a higher administrator level.");
                        return;
                    }
                    else
                    {
                        target.Kick();
                        Notify.Send(player, NotifyType.Success, NotifyPosition.Center, "The player was online, but was kicked.", 3000);
                    }
                }
            }
            else
            {
                string[] split = name.Split('_');
                DataTable result = MySQL.QueryRead($"SELECT adminlvl FROM characters WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                DataRow row = result.Rows[0];
                int targetadminlvl = Convert.ToInt32(row[0]);
                if (targetadminlvl >= Main.Players[player].AdminLVL)
                {
                    Commands.SendToAdmins(3, $"!{{#d35400}}[OFFHARDBAN-DENIED] {player.Name} ({player.Value}) tried to ban {name} (offline), who has a higher administrator level.");
                    return;
                }
            }

            int AUUID = Main.Players[player].UUID;
            int TUUID = Main.PlayerUUIDs[name];

            Ban ban = Ban.Get2(TUUID);
            if (ban != null)
            {
                string hard = (ban.isHard) ? "хард " : "";
                Notify.Send(player, NotifyType.Warning, NotifyPosition.Center, $"The player is already {hard}bath", 3000);
                return;
            }

            DateTime unbanTime = DateTime.Now.AddMinutes(time);
            string banTimeMsg = "м";
            if (time > 60)
            {
                banTimeMsg = "ч";
                time /= 60;
                if (time > 24)
                {
                    banTimeMsg = "д";
                    time /= 24;
                }
            }

            Ban.Offline(name, unbanTime, true, reason, player.Name);

            GameLog.Ban(AUUID, TUUID, unbanTime, reason, true);

            NAPI.Chat.SendChatMessageToAll($"!{{#f25c49}}{player.Name} hit a player with a banhammer {name} на {time}{banTimeMsg}");
            NAPI.Chat.SendChatMessageToAll($"!{{#f25c49}}Reason: {reason}");
        }
        public static void unbanPlayer(Player player, string name)
        {
            if (!Main.PlayerNames.ContainsValue(name))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "There is no such name!", 3000);
                return;
            }
            if (!Ban.Pardon(name))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"{name} not in the bath!", 3000);
                return;
            }
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Player Unlocked!", 3000);
            GameLog.Admin($"{player.Name}", $"unban", $"{name}");
        }
        public static void unhardbanPlayer(Player player, string name)
        {
            if (!Main.PlayerNames.ContainsValue(name))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "There is no such name!", 3000);
                return;
            }
            if (!Ban.PardonHard(name))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"{name} not in the bath!", 3000);
                return;
            }
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Player removed from hardban!", 3000);
        }
        public static void kickPlayer(Player player, Player target, string reason, bool isSilence)
        {
            string cmd = (isSilence) ? "skick" : "kick";
            if (!Group.CanUseCmd(player, cmd)) return;
            if (Main.Players[target].AdminLVL >= Main.Players[player].AdminLVL)
            {
                Commands.SendToAdmins(3, $"!{{#d35400}}[KICK-DENIED] {player.Name} ({player.Value}) tried to kick {target.Name} ({target.Value}), who has a higher administrator level.");
                return;
            }
            if (!isSilence)
                NAPI.Chat.SendChatMessageToAll($"!{{#f25c49}}{player.Name} kicked a player {target.Name} because of {reason}");
            else
            {
                foreach (Player p in Main.Players.Keys.ToList())
                {
                    if (!Main.Players.ContainsKey(p)) continue;
                    if (Main.Players[p].AdminLVL >= 1)
                    {
                        p.SendChatMessage($"!{{#f25c49}}{player.Name} silently kicked a player {target.Name}");
                    }
                }
            }
            GameLog.Admin($"{player.Name}", $"kickPlayer({reason})", $"{target.Name}");
            NAPI.Player.KickPlayer(target, reason);
        }
        public static void warnPlayer(Player player, Player target, string reason)
        {
            if (!Group.CanUseCmd(player, "warn")) return;
            if (player == target) return;
            if (Main.Players[target].AdminLVL >= Main.Players[player].AdminLVL)
            {
                Commands.SendToAdmins(3, $"!{{#d35400}}[WARN-DENIED] {player.Name} ({player.Value}) tried to warn {target.Name} ({target.Value}), who has a higher administrator level.");
                return;
            }
            Main.Players[target].Warns++;
            Main.Players[target].Unwarn = DateTime.Now.AddDays(14);

            int index = Fractions.Manager.AllMembers.FindIndex(m => m.Name == target.Name);
            if (index > -1) Fractions.Manager.AllMembers.RemoveAt(index);

            Main.Players[target].OnDuty = false;
            Main.Players[target].FractionID = 0;
            Main.Players[target].FractionLVL = 0;

            NAPI.Chat.SendChatMessageToAll($"!{{#f25c49}}{player.Name} issued a warning to the player {target.Name} ({Main.Players[target].Warns}/3)");
            NAPI.Chat.SendChatMessageToAll($"!{{#f25c49}}Reason: {reason}");

            if (Main.Players[target].Warns >= 3)
            {
                DateTime unbanTime = DateTime.Now.AddMinutes(43200);
                Main.Players[target].Warns = 0;
                Ban.Online(target, unbanTime, false, "Warns 3/3", "Server_Serverniy");
            }

            GameLog.Admin($"{player.Name}", $"warnPlayer({reason})", $"{target.Name}");
            target.Kick("Warning");
        }
        public static void kickPlayerByName(Player player, string name)
        {
            if (!Group.CanUseCmd(player, "skick")) return;
            Player target = NAPI.Player.GetPlayerFromName(name);
            if (target == null) return;
            NAPI.Player.KickPlayer(target);
            GameLog.Admin($"{player.Name}", $"kickPlayer", $"{name}");
        }

        public static void killTarget(Player player, Player target)
        {
            if (!Group.CanUseCmd(player, "kill")) return;
            NAPI.Player.SetPlayerHealth(target, 0);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"You killed a player {target.Name}", 3000);
            GameLog.Admin($"{player.Name}", $"killPlayer", $"{target.Name}");
        }
        public static void healTarget(Player player, Player target, int hp)
        {
            if (!Group.CanUseCmd(player, "hp")) return;
            NAPI.Player.SetPlayerHealth(target, hp);
            GameLog.Admin($"{player.Name}", $"healPlayer({hp})", $"{target.Name}");
        }
        public static void armorTarget(Player player, Player target, int ar)
        {
            if (!Group.CanUseCmd(player, "ar")) return;

            nItem aItem = nInventory.Find(Main.Players[player].UUID, ItemType.BodyArmor);
            if (aItem == null)
                nInventory.Add(player, new nItem(ItemType.BodyArmor, 1, ar.ToString()));
            GameLog.Admin($"{player.Name}", $"armorPlayer({ar})", $"{target.Name}");
        }
        public static void ToEatTarget(Player player, Player target, int eat)
        {
            if (!Group.CanUseCmd(player, "eat")) return;
            EatManager.AddEat(target, eat);
            GameLog.Admin($"{player.Name}", $"healPlayer({eat})", $"{target.Name}");
        }
        public static void ToWaterTarget(Player player, Player target, int water)
        {
            if (!Group.CanUseCmd(player, "water")) return;
            EatManager.AddWater(target, water);
            GameLog.Admin($"{player.Name}", $"healPlayer({water})", $"{target.Name}");
        }
        public static void checkGamemode(Player player, Player target)
        {
            if (!Group.CanUseCmd(player, "gm")) return;
            int targetHealth = target.Health;
            int targetArmor = target.Armor;
            NAPI.Entity.SetEntityPosition(target, target.Position + new Vector3(0, 0, 10));
            NAPI.Task.Run(() => { try { Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"{target.Name} It was {targetHealth} HP {targetArmor} Armor | It became {target.Health} HP {target.Armor} Armor.", 3000); } catch { } }, 3000);
            GameLog.Admin($"{player.Name}", $"checkGm", $"{target.Name}");
        }
        public static void checkMoney(Player player, Player target)
        {
            try
            {
                if (!Group.CanUseCmd(player, "checkmoney")) return;
                MoneySystem.Bank.Data bankAcc = MoneySystem.Bank.Accounts.FirstOrDefault(a => a.Value.Holder == target.Name).Value;
                int bankMoney = 0;
                if (bankAcc != null) bankMoney = (int)bankAcc.Balance;
                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"У {target.Name} {Main.Players[target].Money}$ | Bank: {bankMoney}", 3000);
                GameLog.Admin($"{player.Name}", $"checkMoney", $"{target.Name}");
            }
            catch (Exception e) { Log.Write("CheckMoney: " + e.Message, nLog.Type.Error); }
        }

        public static void teleportTargetToPlayer(Player player, Player target, bool withveh = false)
        {
            if (!Group.CanUseCmd(player, "metp")) return;
            if (!withveh)
            {
                GameLog.Admin($"{player.Name}", $"metp", $"{target.Name}");
                NAPI.Entity.SetEntityPosition(target, player.Position);
                NAPI.Entity.SetEntityDimension(target, player.Dimension);
            }
            else
            {
                if (!target.IsInVehicle) return;
                NAPI.Entity.SetEntityPosition(target.Vehicle, player.Position + new Vector3(2, 2, 2));
                NAPI.Entity.SetEntityDimension(target.Vehicle, player.Dimension);
                GameLog.Admin($"{player.Name}", $"gethere", $"{target.Name}");
            }
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"you teleported {target.Name} to yourself", 3000);
            Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"{player.Name} teleported you to me", 3000);
        }

        public static void freezeTarget(Player player, Player target)
        {
            if (!Group.CanUseCmd(player, "fz")) return;
            Trigger.ClientEvent(target, "freeze", true);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"You have frozen a player {target.Name}", 3000);
            GameLog.Admin($"{player.Name}", $"freeze", $"{target.Name}");
        }
        public static void unFreezeTarget(Player player, Player target)
        {
            if (!Group.CanUseCmd(player, "ufz")) return;
            Trigger.ClientEvent(target, "freeze", false);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"You have unfrozen a player {target.Name}", 3000);
            GameLog.Admin($"{player.Name}", $"unfreeze", $"{target.Name}");
        }

        public static void giveTargetGun(Player player, Player target, string weapon, string serial)
        {
            if (!Group.CanUseCmd(player, "guns")) return;
            if (serial.Length != 9)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The serial number consists of 9 characters", 3000);
                return;
            }
            ItemType wType = (ItemType)Enum.Parse(typeof(ItemType), weapon);
            if (wType == ItemType.Mask || wType == ItemType.Gloves || wType == ItemType.Leg || wType == ItemType.Bag || wType == ItemType.Feet ||
                wType == ItemType.Jewelry || wType == ItemType.Undershit || wType == ItemType.BodyArmor || wType == ItemType.Decals || wType == ItemType.Top ||
                wType == ItemType.Hat || wType == ItemType.Glasses || wType == ItemType.Accessories)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Items of clothing are not allowed", 3000);
                return;
            }
            if (nInventory.TryAdd(player, new nItem(wType)) == -1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "The player does not have enough inventory space", 3000);
                return;
            }
            Weapons.GiveWeapon(target, wType, serial);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"You gave the player {target.Name} weapon ({weapon.ToString()})", 3000);
            GameLog.Admin($"{player.Name}", $"giveGun({weapon},{serial})", $"{target.Name}");
        }
        public static void giveTargetSkin(Player player, Player target, string pedModel)
        {
            if (!Group.CanUseCmd(player, "setskin")) return;
            if (pedModel.Equals("-1"))
            {
                if (target.HasData("AdminSkin"))
                {
                    target.ResetData("AdminSkin");
                    target.SetSkin((Main.Players[target].Gender) ? PedHash.FreemodeMale01 : PedHash.FreemodeFemale01);
                    Customization.ApplyCharacter(target);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "You restored the player's appearance", 3000);
                }
                else
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "The player did not change the appearance", 3000);
                    return;
                }
            }
            else
            {
                PedHash pedHash = NAPI.Util.PedNameToModel(pedModel);
                if (pedHash != 0)
                {
                    target.SetData("AdminSkin", true);
                    target.SetSkin(pedHash);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"You changed player {target.Name} appearance on ({pedModel})", 3000);
                }
                else
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Appearance with this name was not found", 3000);
                    return;
                }
            }
        }
        public static void giveTargetClothes(Player player, Player target, string weapon, string serial)
        {
            if (!Group.CanUseCmd(player, "giveclothes")) return;
            if (serial.Length < 6 || serial.Length > 12)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The serial number consists of 6-12 characters", 3000);
                return;
            }
            ItemType wType = (ItemType)Enum.Parse(typeof(ItemType), weapon);
            if (wType != ItemType.Mask && wType != ItemType.Gloves && wType != ItemType.Leg && wType != ItemType.Bag && wType != ItemType.Feet &&
                wType != ItemType.Jewelry && wType != ItemType.Undershit && wType != ItemType.BodyArmor && wType != ItemType.Decals && wType != ItemType.Top &&
                wType != ItemType.Hat && wType != ItemType.Glasses && wType != ItemType.Accessories)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "This command can only issue items of clothing", 3000);
                return;
            }
            if (nInventory.TryAdd(player, new nItem(wType)) == -1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The player does not have enough inventory space", 3000);
                return;
            }
            Weapons.GiveWeapon(target, wType, serial);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"You gave the player {target.Name} clothes ({weapon.ToString()})", 3000);
        }
        public static void takeTargetGun(Player player, Player target)
        {
            if (!Group.CanUseCmd(player, "oguns")) return;
            Weapons.RemoveAll(target, true);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"You took from a player {target.Name} all weapons", 3000);
            GameLog.Admin($"{player.Name}", $"takeGuns", $"{target.Name}");
        }

        public static void adminSMS(Player player, Player target, string message)
        {
            if (!Group.CanUseCmd(player, "asms")) return;
            target.SendChatMessage($"~y~{player.Name} ({player.Value}): {message}");
            player.SendChatMessage($"~y~{player.Name} ({player.Value}): {message}");
        }
        public static void answerReport(Player player, Player target, string message)
        {
            if (!Group.CanUseCmd(player, "ans")) return;
            if (!target.HasData("IS_REPORT")) return;

            player.SendChatMessage($"~y~you answered for {target.Name}:~w~ {message}");
            target.SendChatMessage($"~r~[Help] ~y~{player.Name} ({player.Value}):~w~ {message}");
            target.ResetData("IS_REPORT");
            foreach (Player p in Main.Players.Keys.ToList())
            {
                if (!Main.Players.ContainsKey(p)) continue;
                if (Main.Players[p].AdminLVL >= 1)
                {
                    p.SendChatMessage($"~b~[ANSWER] {player.Name}({player.Value})->{target.Name}({target.Value}): {message}");
                }
            }
            GameLog.Admin($"{player.Name}", $"answer({message})", $"{target.Name}");
        }
        public static void adminChat(Player player, string message)
        {
            if (!Group.CanUseCmd(player, "a")) return;
            foreach (Player p in Main.Players.Keys.ToList())
            {
                if (!Main.Players.ContainsKey(p)) continue;
                if (Main.Players[p].AdminLVL >= 1)
                {
                    p.SendChatMessage("!{#2b8234}" + $"[Admin chat] {player.Name} ({player.Value}): {message}");
                }
            }
        }
        public static void adminGlobal(Player player, string message)
        {
            if (!Group.CanUseCmd(player, "global")) return;
            NAPI.Chat.SendChatMessageToAll("!{#f25c49}" + $"{player.Name.Replace('_', ' ')}: {message}");
            GameLog.Admin($"{player.Name}", $"global({message})", $"");
        }
        public static void sendPlayerToDemorgan(Player admin, Player target, int time, string reason)
        {
            if (!Group.CanUseCmd(admin, "demorgan")) return;
            if (!Main.Players.ContainsKey(target)) return;
            if (admin == target)
            {
                Notify.Send(admin, NotifyType.Error, NotifyPosition.BottomCenter, "The command cannot be used on yourself.", 2500);
                return;
            }
            int firstTime = time * 60;
            string deTimeMsg = "м";
            if (time > 60)
            {
                deTimeMsg = "ч";
                time /= 60;
                if (time > 24)
                {
                    deTimeMsg = "д";
                    time /= 24;
                }
            }

            NAPI.Chat.SendChatMessageToAll($"!{{#f25c49}}{admin.Name} jailed a player {target.Name} на {time}{deTimeMsg}");
            NAPI.Chat.SendChatMessageToAll($"!{{#f25c49}}Причина: {reason}");
            Main.Players[target].ArrestTime = 0;
            Main.Players[target].DemorganTime = firstTime;
            Main.Players[target].DemroganReason = Newtonsoft.Json.JsonConvert.SerializeObject(reason);
            Main.Players[target].DemorganAdmin = Newtonsoft.Json.JsonConvert.SerializeObject(admin.Name);
            Trigger.ClientEvent(target, "DemorganShowhelp", firstTime, Newtonsoft.Json.JsonConvert.SerializeObject(reason), Newtonsoft.Json.JsonConvert.SerializeObject(admin.Name.Replace("_"," ")));
            Fractions.FractionCommands.unCuffPlayer(target);

            NAPI.Entity.SetEntityPosition(target, DemorganPosition + new Vector3(0, 0, 1.5));
            //if (target.HasData("ARREST_TIMER")) Main.StopT(target.GetData("ARREST_TIMER"), "timer_34");
            if (target.HasData("ARREST_TIMER")) Timers.Stop(target.GetData<string>("ARREST_TIMER"));
            //NAPI.Data.SetEntityData(target, "ARREST_TIMER", Main.StartT(1000, 1000, (o) => timer_demorgan(target), "DEMORGAN_TIMER"));
            NAPI.Data.SetEntityData(target, "ARREST_TIMER", Timers.StartTask(1000, () => timer_demorgan(target)));
            NAPI.Entity.SetEntityDimension(target, 1337);
            Weapons.RemoveAll(target, true);
            GameLog.Admin($"{admin.Name}", $"demorgan({time}{deTimeMsg},{reason})", $"{target.Name}");
        }
        public static void releasePlayerFromDemorgan(Player admin, Player target)
        {
            if (!Group.CanUseCmd(admin, "udemorgan")) return;
            if (!Main.Players.ContainsKey(target)) return;

            Main.Players[target].DemorganTime = 0;
            Main.Players[target].DemorganAdmin = null;
            Main.Players[target].DemroganReason = null;
            Notify.Send(admin, NotifyType.Warning, NotifyPosition.BottomCenter, $"you freed {target.Name} from admin. prisons", 3000);

            Fractions.Police.setPlayerWantedLevel(target, null);
            NAPI.Entity.SetEntityPosition(target, new Vector3(1846.4623, 2585.8774, 45.682013));
            NAPI.Entity.SetEntityDimension(target, 0);
            Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"You're out of jail, don't break anymore!", 3000);
            Trigger.ClientEvent(target, "DemorganShowhelpFalse");

            GameLog.Admin($"{admin.Name}", $"undemorgan", $"{target.Name}");
        }
        public static void freePlayerDemorgan(Player player)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    Trigger.ClientEvent(player, "DemorganShowhelpFalse");
                    Timers.Stop(NAPI.Data.GetEntityData(player, "ARREST_TIMER")); // still not fixed
                    NAPI.Data.ResetEntityData(player, "ARREST_TIMER");
                    //Fractions.Police.setPlayerWantedLevel(player, null);
                    NAPI.Entity.SetEntityPosition(player, new Vector3(1846.4623, 2585.8774, 45.682013));
                    NAPI.Entity.SetEntityDimension(player, 0);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"You're out of jail, don't break anymore!", 3000);
                }
                catch { }
            });
        }
        #region Demorgan
        public static Vector3 DemorganPosition = new Vector3(1680.3871, 2513.0054, 44.444855);
        public static void timer_demorgan(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].DemorganTime <= 0)
                {
                    freePlayerDemorgan(player);
                    return;
                }
                Main.Players[player].DemorganTime--;
                Trigger.ClientEvent(player, "DemorganShowhelp", Main.Players[player].DemorganTime, Main.Players[player].DemroganReason, Main.Players[player].DemorganAdmin.Replace("_"," "));
            }
            catch (Exception e)
            {
                Log.Write("DEMORGAN_TIMER: " + e.ToString(), nLog.Type.Error);
            }
        }
        public static void timer_mute(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].Unmute <= 0)
                {
                    if (!player.HasData("MUTE_TIMER")) return;
                    Timers.Stop(NAPI.Data.GetEntityData(player, "MUTE_TIMER"));
                    NAPI.Data.ResetEntityData(player, "MUTE_TIMER");
                    Main.Players[player].VoiceMuted = false;
                    player.SetSharedData("voice.muted", false);
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Mute has been lifted, don't break anymore!", 3000);
                    return;
                }
                Main.Players[player].Unmute--;
            }
            catch (Exception e)
            {
                Log.Write("MUTE_TIMER: " + e.ToString(), nLog.Type.Error);
            }
        }
        #endregion
        // need refactor
        public static void respawnAllCars(Player player)
        {
            if (!Group.CanUseCmd(player, "allspawncar")) return;
            List<Vehicle> all_vehicles = NAPI.Pools.GetAllVehicles();

            foreach (Vehicle vehicle in all_vehicles)
            {
                List<Player> occupants = VehicleManager.GetVehicleOccupants(vehicle);
                if (occupants.Count > 0)
                {
                    List<Player> newOccupants = new List<Player>();
                    foreach (Player occupant in occupants)
                        if (Main.Players.ContainsKey(occupant)) newOccupants.Add(occupant);
                    vehicle.SetData("OCCUPANTS", newOccupants);
                }
            }

            foreach (Vehicle vehicle in all_vehicles)
            {
                if (VehicleManager.GetVehicleOccupants(vehicle).Count >= 1) continue;
                if (vehicle.GetData<string>("ACCESS") == "PERSONAL")
                {
                    Player owner = vehicle.GetData<Player>("OWNER");
                    NAPI.Entity.DeleteEntity(vehicle);
                }
                else if (vehicle.GetData<string>("ACCESS") == "WORK")
                    RespawnWorkCar(vehicle);
                else if (vehicle.GetData<string>("ACCESS") == "FRACTION")
                    RespawnFractionCar(vehicle);
                else if (vehicle.GetData<string>("ACCESS") == "GANGDELIVERY" || vehicle.GetData<string>("ACCESS") == "MAFIADELIVERY")
                    NAPI.Entity.DeleteEntity(vehicle);
            }
        }

        public static void RespawnWorkCar(Vehicle vehicle)
        {
            if (vehicle.GetData<bool>("ON_WORK") && Main.Players.ContainsKey(vehicle.GetData<Player>("DRIVER"))) return;
            var type = vehicle.GetData<string>("TYPE");
            switch (type)
            {
                case "MOWER":
                    Jobs.Lawnmower.respawnCar(vehicle);
                    break;
                case "BUS":
                    Jobs.Bus.respawnBusCar(vehicle);
                    break;
                case "TAXI":
                    Jobs.Taxi.respawnCar(vehicle);
                    break;
                case "COLLECTOR":
                    Jobs.Collector.respawnCar(vehicle);
                    break;
                case "MECHANIC":
                    Jobs.AutoMechanic.respawnCar(vehicle);
                    break;
            }
        }

        public static void RespawnFractionCar(Vehicle vehicle)
        {
            if (NAPI.Data.HasEntityData(vehicle, "loaderMats"))
            {
                Player loader = NAPI.Data.GetEntityData(vehicle, "loaderMats");
                Trigger.ClientEvent(loader, "hideLoader");
                Notify.Send(loader, NotifyType.Warning, NotifyPosition.BottomCenter, $"Loading of materials canceled because the car left the checkpoint", 3000);
                if (loader.HasData("loadMatsTimer"))
                {
                    //Main.StopT(loader.GetData("loadMatsTimer"), "timer_35");
                    Timers.Stop(loader.GetData<string>("loadMatsTimer"));
                    loader.ResetData("loadMatsTime");
                }
                NAPI.Data.ResetEntityData(vehicle, "loaderMats");
            }
            Fractions.Configs.RespawnFractionCar(vehicle);
        }
    }

    public class Group
    {
        public static List<GroupCommand> GroupCommands = new List<GroupCommand>();
        public static void LoadCommandsConfigs()
        {
            DataTable result = MySQL.QueryRead($"SELECT * FROM adminaccess");
            if (result == null || result.Rows.Count == 0) return;
            List<GroupCommand> groupCmds = new List<GroupCommand>();
            foreach (DataRow Row in result.Rows)
            {
                string cmd = Convert.ToString(Row["command"]);
                bool isadmin = Convert.ToBoolean(Row["isadmin"]);
                int minrank = Convert.ToInt32(Row["minrank"]);

                groupCmds.Add(new GroupCommand(cmd, isadmin, minrank));
            }
            GroupCommands = groupCmds;
        }

        public static List<string> GroupNames = new List<string>()
        {
            "Игрок",
            "Bronze VIP",
            "Silver VIP",
            "Gold VIP",
            "Platinum VIP",
        };
        public static List<float> GroupPayAdd = new List<float>()
        {
            1.0f,
            1.0f,
            1.15f,
            1.25f,
            1.35f,
        };
        public static List<int> GroupAddPayment = new List<int>()
        {
            0,
            200,
            400,
            550,
            700
        };
        public static List<int> GroupMaxContacts = new List<int>()
        {
            50,
            60,
            70,
            80,
            100,
        };
        public static List<int> GroupMaxBusinesses = new List<int>()
        {
            1,
            1,
            1,
            1,
            1,
        };
        public static List<int> GroupEXP = new List<int>()
        {
            1,
            2,
            2,
            2,
            3,
        };

        public static bool CanUseCmd(Player player, string cmd, string args = "")
        {
            if (!Main.Players.ContainsKey(player)) return false;
            GroupCommand command = GroupCommands.FirstOrDefault(c => c.Command == cmd);
        check:
            if (command != null)
            {
                if (command.IsAdmin)
                {
                    if (Main.Players[player].AdminLVL >= command.MinLVL) return true;
                }
                else
                {
                    if (Main.Accounts[player].VipLvl >= command.MinLVL) return true;
                }
            }
            else
            {
                MySQL.Query($"INSERT INTO `adminaccess`(`command`, `isadmin`, `minrank`) VALUES ('{cmd}',1,7)");
                GroupCommand newGcmd = new GroupCommand(cmd, true, 7);
                command = newGcmd;
                GroupCommands.Add(newGcmd);
                goto check;
            }

            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Not enough rights", 3000);
            return false;
        }

        public class GroupCommand
        {
            public GroupCommand(string command, bool isAdmin, int minlvl)
            {
                Command = command;
                IsAdmin = isAdmin;
                MinLVL = minlvl;
            }

            public string Command { get; }
            public bool IsAdmin { get; }
            public int MinLVL { get; }
        }
    }
}
