using GTANetworkAPI;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using Golemo.GUI;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using Newtonsoft.Json;
using GolemoSDK;
using Golemo.Core.Character;
using Golemo.MoneySystem;
using System.Text.RegularExpressions;
using MySqlConnector;
using Golemo.Houses;
using Golemo.Voice;
using Golemo.Fractions;

namespace Golemo.Core
{
    class Commands : Script
    {
        private static nLog Log = new nLog("Commands");
        private static Random rnd = new Random();

        #region Chat logic

        public static void SendToAdmins(ushort minLVL, string message)
        {
            foreach (var p in Main.Players.Keys.ToList())
            {
                if (!Main.Players.ContainsKey(p)) continue;
                if (Main.Players[p].AdminLVL >= minLVL)
                {
                    p.SendChatMessage(message);
                }
            }
        }

        private static string RainbowExploit(Player sender, string message)
        {
            if (message.Contains("!{"))
            {
                foreach (var p in Main.Players.Keys.ToList())
                {
                    if (!Main.Players.ContainsKey(p)) continue;
                    if (Main.Players[p].AdminLVL >= 1)
                    {
                        p.SendChatMessage($"~y~[CHAT-EXPLOIT] {sender.Name} ({sender.Value}) - {message}");
                    }
                }
                return Regex.Replace(message, "!", string.Empty);
            }
            return message;
        }

        [ServerEvent(Event.ChatMessage)]
        public void API_onChatMessage(Player sender, string message)
        {
            try
            {
                if (!Main.Players.ContainsKey(sender)) return;
                if (Main.Players[sender].Unmute > 0)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"Are you tortured yet on {Main.Players[sender].Unmute / 60} minutes", 3000);
                    return;
                }
                else if (Main.Players[sender].VoiceMuted)
                {
                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            Main.Players[sender].VoiceMuted = false;
                            sender.SetSharedData("voice.muted", false);
                        }
                        catch { }
                    });
                }
                //await Log.WriteAsync($"<{sender.Name.ToString()}> {message}", nLog.Type.Info);

                message = RainbowExploit(sender, message);
                //message = Main.BlockSymbols(message);
                List<Player> players = Main.GetPlayersInRadiusOfPosition(sender.Position, 10f, sender.Dimension);
                NAPI.Task.Run(() =>
                {
                    try
                    {
                        int[] id = new int[] { sender.Value };
                        foreach (Player c in players)
                        {
                            Trigger.ClientEvent(c, "sendRPMessage", "chat", "{name}: " + message, id);
                        }

                        if (!sender.HasData("PhoneVoip")) return;
                        Voice.VoicePhoneMetaData phoneMeta = sender.GetData<VoicePhoneMetaData>("PhoneVoip");
                        if (phoneMeta.CallingState == "talk" && Main.Players.ContainsKey(phoneMeta.Target))
                        {
                            var pSim = Main.Players[sender].Sim;
                            var contactName = (Main.Players[phoneMeta.Target].Contacts.ContainsKey(pSim)) ? Main.Players[phoneMeta.Target].Contacts[pSim] : pSim.ToString();
                            phoneMeta.Target.SendChatMessage($"[In phone] {contactName}: {message}");
                        }
                    }
                    catch (Exception e) { Log.Write("ChatMessage_TaskRun: " + e.Message, nLog.Type.Error); }
                });
                return;
            }
            catch (Exception e) { Log.Write("ChatMessage: " + e.Message, nLog.Type.Error); }
        }
        #endregion Chat logic

        #region AdminCommands

        [Command("tpp", GreedyArg = true)]
        public static void CMD_tpAtCoordString(Player player, string coords)
        {
            if (!Group.CanUseCmd(player, "tpp")) return;
            double[] elem = coords.Split(',', ' ').
                      Where(x => !string.IsNullOrWhiteSpace(x)).
                      Select(x => double.Parse(x.Replace('.', ','))).ToArray();
            if (elem.Length != 3) return;
            NAPI.Entity.SetEntityPosition(player, new Vector3(elem[0], elem[1], elem[2]));
        }

        [Command("showworkstat")]
        public static void CMD_ShowWorkStats(Player player, int workid = 0)
        {
            if (!Core.Group.CanUseCmd(player, "showworkstat")) return;
            Character.Character data = Main.Players[player];
            if (workid == 0) workid = data.WorkID;
            string[] message = new string[6];
            if(!data.WorksStats.Contains(data.WorksStats.Find(x => x.WorkID == workid)))
            {
                Notify.Error(player, "You have never taken this job");
                return;
            }
            message[0] = $"WorkID: {Convert.ToString(data.WorksStats[data.GetWorkStatsIndex()].WorkID)}";
            message[1] = $"Level: {Convert.ToString(data.WorksStats[data.GetWorkStatsIndex()].Level)}";
            message[2] = $"Exp: {Convert.ToString(data.WorksStats[data.GetWorkStatsIndex()].Exp)}";
            message[3] = $"Allpoints: {Convert.ToString(data.WorksStats[data.GetWorkStatsIndex()].AllPoints)}";
            message[4] = $"MaxLevel: {Convert.ToString(data.WorksStats[data.GetWorkStatsIndex()]._maxLevel)}";
            message[5] = $"RequiredExp: {Convert.ToString(data.WorksStats[data.GetWorkStatsIndex()]._expCountForUpLevel)}";
            player.SendChatMessage("===================Work Stats===================");
            for (int i = 0; i < message.Length; i++)
            {
                player.SendChatMessage(message[i]);
            }
            player.SendChatMessage("================================================");
        }

        [Command("ahelp")]
        public static void CMD_getInfoHelp(Player player, int personid)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "ahelp")) return;
                if (Main.Players[player].AdminLVL > 6)
                {
                    player.SendChatMessage($"/createstock-Create warehouse");
                    player.SendChatMessage($"/givemoney-Give out money");
                    player.SendChatMessage($"/givereds-Issue donated currency");
                    player.SendChatMessage($"/takeoffbiz-Pick up business");
                    player.SendChatMessage($"/createsafe-Put up a safe");
                    player.SendChatMessage($"/allspawncar-Spawn all faction vehicles");
                    player.SendChatMessage($"/save-Save coordinates");
                    player.SendChatMessage($"/setfracveh-Set fractional transport");
                    player.SendChatMessage($"/createbusiness-Create a business");
                    player.SendChatMessage($"/newfracveh-Create fractional vehicles");
                    player.SendChatMessage($"/udemorgan-Release from demorgan");
                    player.SendChatMessage($"/newrentveh-Create a rental car");
                    player.SendChatMessage($"/newjobveh-Create a machine to work");
                    player.SendChatMessage($"/ar-Issue armor");
                    player.SendChatMessage($"/oguns-Issue a weapon");
                    player.SendChatMessage($"/giveclothes-Give out clothes");
                    player.SendChatMessage($"/delhouseowner-Remove the owner of the house");
                    player.SendChatMessage($"/deletebusiness-Delete business");
                    player.SendChatMessage($"/stop-Server stop");
                    player.SendChatMessage($"/createunloadpoint-Create a truck unloading point");
                    player.SendChatMessage($"/pa-Start animation");
                    player.SendChatMessage($"/newsimcard-Issue a new card");
                    player.SendChatMessage($"/fixgovbizprices-Fix state prices");
                    player.SendChatMessage($"/housetypeprice-Apply price for houses of the same type");
                    player.SendChatMessage($"/svm-Put tuning");
                    player.SendChatMessage($"/lsn-Send news out of turn");
                    player.SendChatMessage($"/mtp-Teleport by mask id");
                    player.SendChatMessage($"/removesafe-Remove safe");
                    player.SendChatMessage($"/paydaymultiplier-Payday money x2 multiplier");
                    player.SendChatMessage($"/setproductbyindex-Assign product by index (debug)");
                    player.SendChatMessage($"/deleteproducts-Delete Products");
                    player.SendChatMessage($"/changebizprice-Change business value");
                    player.SendChatMessage($"/guns-Issue a weapon");
                    player.SendChatMessage($"/stt-Tuning for speed");
                    player.SendChatMessage($"/expmultiplier-Payday x2 experience multiplier");
                    player.SendChatMessage($"/payday-run payday");
                    player.SendChatMessage($"/givelic-Вissue a license");
                    player.SendChatMessage($"/vehhash-?");
                    player.SendChatMessage($"/setbizmafia-Assign mafia for business");
                    player.SendChatMessage($"/setcolour-Change the color of the territory (capt)");
                    player.SendChatMessage($"/startmatwars-Start a mate war");
                    player.SendChatMessage($"/setadmin-Make player admin");
                    player.SendChatMessage($"/setadminrank-Set admin rank");
                    player.SendChatMessage($"/deladmin-Removing an admin");
                    player.SendChatMessage($"/delleader-Remove leader from office");
                    player.SendChatMessage($"/setviplvl-Set brew level");
                    player.SendChatMessage($"/sw set-weather");
                    player.SendChatMessage($"/deljob delete-job");
                    player.SendChatMessage($"/giveexp issue-experience");
                    player.SendChatMessage($"/changestock-Change warehouse status");
                    player.SendChatMessage($"/setleader-Appoint a leader");
                    player.SendChatMessage($"/giveammo-Issue ammo");
                    player.SendChatMessage($"/createmp-Create MP");
                    player.SendChatMessage($"/startmp-Start MP");
                    player.SendChatMessage($"/stopmp-Stop MP");
                    player.SendChatMessage($"/sendcreator-Send the player to the character editor");
                    player.SendChatMessage($"/changename-Change name");
                    player.SendChatMessage($"/offdelfrac-Kick out of the faction (offline)");
                    player.SendChatMessage($"/vehc-Spawn vehicles with color customization");
                    player.SendChatMessage($"/setdim-Move to dimension");
                    player.SendChatMessage($"/skick-Quiet kick");
                    player.SendChatMessage($"/global-global chat");
                    player.SendChatMessage($"/setskin-Give skin");
                    player.SendChatMessage($"/veh-Sleep transport");
                    player.SendChatMessage($"/fixcar-Repair transport");
                    player.SendChatMessage($"/ban-Ban player");
                    player.SendChatMessage($"/delfrac-Kick out of the faction");
                    player.SendChatMessage($"/warn-Issue a warning");
                    player.SendChatMessage($"/unwarn-Remove warning");
                    player.SendChatMessage($"/checkmoney-View the player's money");
                    player.SendChatMessage($"/fz-Freeze Player");
                    player.SendChatMessage($"/delacar-Delete all spawned vehicles");
                    player.SendChatMessage($"/ufz-Unfreeze player");
                    player.SendChatMessage($"/offban-Ban player (offline))");
                    player.SendChatMessage($"/rescar-Respawn machines");
                    player.SendChatMessage($"/checkprop-Check player for property");
                    player.SendChatMessage($"/offjail-Release from demorgan (offline)");
                    player.SendChatMessage($"/offwarn-Remove warning (offline)");
                    player.SendChatMessage($"/kill-Kill player");
                    player.SendChatMessage($"/tpc-Teleport by coordinates");
                    player.SendChatMessage($"/hp-Issue health");
                    player.SendChatMessage($"/metp-Teleport the player to you");
                    player.SendChatMessage($"/demorgan-Poison a player into a demoorgan");
                    player.SendChatMessage($"/tp-Teleport to player");
                    player.SendChatMessage($"/admins-List of admins online");
                    player.SendChatMessage($"/gm-Check for GM (toss up)");
                    player.SendChatMessage($"/sp-follow player");
                    player.SendChatMessage($"/mute-Give a mut to a player");
                    player.SendChatMessage($"/agm-Enable GM");
                    player.SendChatMessage($"/unmute-Remove mute from a player");
                    player.SendChatMessage($"/inv-Invisibility");
                    player.SendChatMessage($"/afuel-Fill the car ");
                    player.SendChatMessage($"/stats-Character stats");
                    player.SendChatMessage($"/delad-Delete ad");
                    player.SendChatMessage($"/kick-Kick a player");
                    player.SendChatMessage($"/tpcar-Teleport car");
                    player.SendChatMessage($"/redname-Red nickname ");
                    player.SendChatMessage($"/a-Admin chat");
                    player.SendChatMessage($"/id-Player ID ");
                    player.SendChatMessage($"/asms-Write to the player");
                    player.SendChatMessage($"/ans-Answer to the report");
                }
                else if (Main.Players[player].AdminLVL > 1)
                {
                    player.SendChatMessage($"/createstock-Create warehouse");
                    player.SendChatMessage($"/givemoney-Give out money");
                    player.SendChatMessage($"/givereds-Issue donated currency");
                    player.SendChatMessage($"/takeoffbiz-Pick up business");
                    player.SendChatMessage($"/createsafe-Put up a safe");
                    player.SendChatMessage($"/allspawncar-Spawn all faction vehicles");
                    player.SendChatMessage($"/save-Save coordinates");
                    player.SendChatMessage($"/setfracveh-Set fractional transport");
                    player.SendChatMessage($"/createbusiness-Create a business");
                    player.SendChatMessage($"/newfracveh-Create fractional vehicles");
                    player.SendChatMessage($"/udemorgan-Release from demoorgan");
                    player.SendChatMessage($"/newrentveh-Create a rental car");
                    player.SendChatMessage($"/newjobveh-Create a machine to work");
                    player.SendChatMessage($"/ar-Issue armor");
                    player.SendChatMessage($"/oguns-Issue weapons");
                    player.SendChatMessage($"/giveclothes-Give out clothes");
                    player.SendChatMessage($"/delhouseowner-Remove the owner of the house");
                    player.SendChatMessage($"/deletebusiness-Delete business");
                    player.SendChatMessage($"/stop-Server stop");
                    player.SendChatMessage($"/createunloadpoint-Create an unloading point for truckers");
                    player.SendChatMessage($"/pa-Start animation");
                    player.SendChatMessage($"/newsimcard-Issue a new siccard");
                    player.SendChatMessage($"/fixgovbizprices-Fix state prices");
                    player.SendChatMessage($"/housetypeprice-Apply price for houses of the same type");
                    player.SendChatMessage($"/svm-Put tuning");
                    player.SendChatMessage($"/lsn-Send news out of turn");
                    player.SendChatMessage($"/mtp-Teleport by mask id");
                    player.SendChatMessage($"/removesafe-Remove safe");
                    player.SendChatMessage($"/paydaymultiplier-Payday x2 money multiplier");
                    player.SendChatMessage($"/setproductbyindex-Assign product by index (debug)");
                    player.SendChatMessage($"/deleteproducts-Delete Products");
                    player.SendChatMessage($"/changebizprice-Change business value");
                    player.SendChatMessage($"/guns-Issue weapons");
                    player.SendChatMessage($"/stt-Tuning for speed");
                    player.SendChatMessage($"/expmultiplier-Payday experience x2 multiplier");
                    player.SendChatMessage($"/payday-Run payday");
                    player.SendChatMessage($"/givelic-issue a license");
                    player.SendChatMessage($"/vehhash-?");
                    player.SendChatMessage($"/setbizmafia-Appoint mafia for business");
                    player.SendChatMessage($"/setcolour-Change territory color (capt)");
                    player.SendChatMessage($"/startmatwars-Start a mating war");
                    player.SendChatMessage($"/setadmin-Make player admin");
                    player.SendChatMessage($"/setadminrank-Set admin rank");
                    player.SendChatMessage($"/deladmin-Removing an admin");
                    player.SendChatMessage($"/delleader-Remove leader from office");
                    player.SendChatMessage($"/setviplvl-Set Whip Level");
                    player.SendChatMessage($"/sw set-weather");
                    player.SendChatMessage($"/deljob delete-job");
                    player.SendChatMessage($"/giveexp issue-experience");
                    player.SendChatMessage($"/changestock-Change warehouse status");
                    player.SendChatMessage($"/setleader-Appoint a leader");
                    player.SendChatMessage($"/giveammo-Issue ammo");
                    player.SendChatMessage($"/createmp-Create MP");
                    player.SendChatMessage($"/startmp-Start MP");
                    player.SendChatMessage($"/stopmp-Stop MP");
                    player.SendChatMessage($"/sendcreator-Send the player to the character editor");
                    player.SendChatMessage($"/changename-Change name");
                    player.SendChatMessage($"/offdelfrac-Kick out of faction (offline))");
                    player.SendChatMessage($"/vehc-Spawn vehicles with custom colors");
                    player.SendChatMessage($"/setdim-Move to dimension");
                    player.SendChatMessage($"/skick-Quiet kick");
                    player.SendChatMessage($"/global-global chat");
                    player.SendChatMessage($"/setskin-Issue skin");
                    player.SendChatMessage($"/veh-Sleep transport");
                    player.SendChatMessage($"/fixcar-Repair transport");
                    player.SendChatMessage($"/ban-Ban player");
                    player.SendChatMessage($"/delfrac-Kick out of the faction");
                    player.SendChatMessage($"/warn-Issue a warning");
                    player.SendChatMessage($"/unwarn-Remove warning");
                    player.SendChatMessage($"/checkmoney-View the player's money");
                    player.SendChatMessage($"/fz-Freeze Player");
                    player.SendChatMessage($"/delacar-Delete all spawned vehicles");
                    player.SendChatMessage($"/ufz-Unfreeze player");
                    player.SendChatMessage($"/offban-Ban player (offline)");
                    player.SendChatMessage($"/rescar-Respawn machines");
                    player.SendChatMessage($"/checkprop-Check player for property");
                    player.SendChatMessage($"/offjail-Release from demorgan (offline)");
                    player.SendChatMessage($"/offwarn-Remove warning (offline)");
                    player.SendChatMessage($"/kill-Kill player");
                    player.SendChatMessage($"/tpc-Teleport by coordinates");
                    player.SendChatMessage($"/hp-Issue health");
                    player.SendChatMessage($"/metp-Teleport the player to you");
                    player.SendChatMessage($"/demorgan-Poison a player into a demoorgan");
                    player.SendChatMessage($"/tp-Teleport to player");
                    player.SendChatMessage($"/admins-List of admins online");
                    player.SendChatMessage($"/gm-Check for GM (toss up)");
                    player.SendChatMessage($"/sp-follow player");
                    player.SendChatMessage($"/mute-Give a mut to a player");
                    player.SendChatMessage($"/agm-Enable GM");
                    player.SendChatMessage($"/unmute-Remove mute from a player");
                    player.SendChatMessage($"/inv-Invisibility");
                    player.SendChatMessage($"/afuel-Fill the car ");
                    player.SendChatMessage($"/stats-Character stats ");
                    player.SendChatMessage($"/delad-Delete ad ");
                    player.SendChatMessage($"/kick-Kick a player ");
                    player.SendChatMessage($"/tpcar-Teleport car");
                    player.SendChatMessage($"/redname-Red nickname ");
                    player.SendChatMessage($"/a-Admin chat ");
                    player.SendChatMessage($"/id-Player ID ");
                    player.SendChatMessage($"/asms-Write to the player");
                    player.SendChatMessage($"/ans-Answer to the report");
                }
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\"/id/:\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("drone", "Use: /drone [type]")]
        public static void CMD_drone(Player client, int type = 0)
        {
            try
            {
                if (!Group.CanUseCmd(client, "drone")) return;


                client.SetData<int>("drone_type", type);
                Trigger.ClientEvent(client, "client:StartAdminDrone");
            }
            catch (Exception e)
            {
                { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
            }
        }


        [Command("getlastbonus")] //todo lastbonus
        public static void GetLastBonus(Player player, int id)
        {
            if (!Group.CanUseCmd(player, "getlastbonus")) return;

            var target = Main.GetPlayerByID(id);
            if (target == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player with this ID was not found", 3000);
                return;
            }
            DateTime date = new DateTime((new DateTime().AddMinutes(Main.Players[target].LastBonus)).Ticks);
            var hour = date.Hour;
            var min = date.Minute;
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"LastBonus player({id}): {hour} hours and {min} minutes ({Main.Players[target].LastBonus})", 3000);
        }
        [Command("lastbonus")] //todo lastbonus
        public static void LastBonus(Player player)
        {
            if (!Group.CanUseCmd(player, "lastbonus")) return;
            DateTime date = new DateTime((new DateTime().AddMinutes(Main.oldconfig.LastBonusMin)).Ticks);
            var hour = date.Hour;
            var min = date.Minute;
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"LastBonus is: {hour} hours and {min} minutes", 2500);
        }
        [Command("setproductforallbizz")]
        public static void CMD_setProductForAllBiz(Player player)
        {
            try
            {
                if (player == null || !Main.Players.ContainsKey(player)) return;

                BusinessManager.setProductForAllBiz(player);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("setlastbonus")] //todo lastbonus
        public static void SetLastBonus(Player player, int id, int count)
        {
            if (!Group.CanUseCmd(player, "setlastbonus")) return;

            var target = Main.GetPlayerByID(id);
            if (target == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player with this ID was not found", 3000);
                return;
            }
            count = Convert.ToInt32(Math.Abs(count));
            if (count > Main.oldconfig.LastBonusMin)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The entered number exceeds the config value. Maximum: {Main.oldconfig.LastBonusMin}", 3000);
                return;
            }
            Main.Players[target].LastBonus = count;
            DateTime date = new DateTime((new DateTime().AddMinutes(Main.Players[target].LastBonus)).Ticks);
            var hour = date.Hour;
            var min = date.Minute;
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"LastBonus for the player({id}) is set to {hour} hours and {min} minutes ({Main.Players[target].LastBonus})", 3000);
        }

        [Command("delfveh")]
        public static void CMD_deletefveh(Player client, string number)
        {
            try
            {
                if (!Group.CanUseCmd(client, "delfveh")) return;

                MySqlCommand queryCommand = new MySqlCommand(@" DELETE FROM `fractionvehicles` WHERE `number` = @NUMBER ");

                queryCommand.Parameters.AddWithValue("@NUMBER", number);

                MySQL.Query(queryCommand);

                client.SendChatMessage($"{number} was deleted");
            }
            catch { }
        }

        #region Add-Ons
        [Command("eat")] //Добавляет еду игроку
        public static void CMD_adminEat(Player player, int id, int eat)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player with this ID was not found", 3000);
                    return;
                }
                Admin.ToEatTarget(player, Main.GetPlayerByID(id), eat);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("water")] //Добавляет воду игроку
        public static void CMD_adminWater(Player player, int id, int water)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player with this ID was not found", 3000);
                    return;
                }
                Admin.ToWaterTarget(player, Main.GetPlayerByID(id), water);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        #endregion

        [Command("createrod")] // Создать место для рыбалки (7 лвл)
        public static void CMD_createRod(Player player, float radius)
        {
            try
            {
                RodManager.createRodAreaCommand(player, radius);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("givelic")] // Выдать лицензии (7 лвл)
        public static void CMD_giveLicense(Player player, int id, int lic)
        {
            try
            {
                if (!Group.CanUseCmd(player, "givelic")) return;

                var target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player with this ID was not found", 3000);
                    return;
                }

                if (lic < 0 || lic >= Main.Players[target].Licenses.Count)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"lic = from 0 to {Main.Players[target].Licenses.Count - 1}", 3000);
                    return;
                }

                Main.Players[target].Licenses[lic] = true;
                Dashboard.sendStats(target);

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Issued successfully", 3000);
            }
            catch { }
        }
        
        [Command("vehchange")] // Change car type (lvl 7)
        public static void CMD_vehchage(Player client, string newmodel)
        {
            try
            {
                if (!Group.CanUseCmd(client, "vehchange")) return;

                if (!client.IsInVehicle) return;

                if (!client.Vehicle.HasData("ACCESS"))
                    return;
                else if (client.Vehicle.GetData<string>("ACCESS") == "PERSONAL")
                {
                    VehicleManager.Vehicles[client.Vehicle.NumberPlate].Model = newmodel;
                    Notify.Send(client, NotifyType.Warning, NotifyPosition.BottomCenter, "The car will be available after respawn", 3000);
                }
                else if (client.Vehicle.GetData<string>("ACCESS") == "WORK")
                    return;
                else if (client.Vehicle.GetData<string>("ACCESS") == "FRACTION")
                    return;
                else if (client.Vehicle.GetData<string>("ACCESS") == "GANGDELIVERY" || client.Vehicle.GetData<string>("ACCESS") == "MAFIADELIVERY")
                    return;
            }
            catch { }
        }

        [Command("bankfix")] // Удалить банковский счет (7 лвл)
        public static void CMD_bankfix(Player client, int bank)
        {
            try
            {
                if (!Group.CanUseCmd(client, "bankfix")) return;
                if (Bank.Accounts.ContainsKey(bank))
                {
                    Bank.RemoveByID(bank);
                    Notify.Send(client, NotifyType.Success, NotifyPosition.BottomCenter, $"You have successfully deleted your bank account number {bank}", 3000);
                }
                else Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, $"Bank account {bank} not found", 3000);
            }
            catch { }
        }

        [Command("gethwid")] // Получить HWID игрока (7 лвл)
        public static void CMD_gethwid(Player client, int ID)
        {
            try
            {
                if (!Group.CanUseCmd(client, "gethwid")) return;
                Player target = Main.GetPlayerByID(ID);
                if (target == null)
                {
                    Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, "Player with this ID was not found", 3000);
                    return;
                }
                client.SendChatMessage("Real HWID " + target.Name + ": " + target.GetData<string>("RealHWID"));
            }
            catch { }
        }

        [Command("getsocialclub")] // Получить Social Club игрока (7 лвл)
        public static void CMD_getsc(Player client, int ID)
        {
            try
            {
                if (!Group.CanUseCmd(client, "getsocialclub")) return;
                Player target = Main.GetPlayerByID(ID);
                if (target == null)
                {
                    Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, "Player with this ID was not found", 3000);
                    return;
                }
                client.SendChatMessage("Real SocialClub y" + target.Name + ": " + target.GetData<string>("RealSocialClub"));
            }
            catch { }
        }

        [Command("loggedinfix")] // Фикс авторизации (хз) (7 лвл)
        public static void CMD_loggedinfix(Player player, string login)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "loggedinfix")) return;
                if (Main.LoggedIn.ContainsKey(login))
                {
                    if (NAPI.Player.IsPlayerConnected(Main.LoggedIn[login]))
                    {
                        Main.LoggedIn[login].Kick();
                        Main.LoggedIn.Remove(login);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "You kicked a character from the server, in a minute you can try to log into your account.", 3000);
                    }
                    else
                    {
                        Main.LoggedIn.Remove(login);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "The character was not online, the account was removed from the list of authorized.", 3000);
                    }
                }
                else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"No account found with login {login}", 3000);
            }
            catch { }
        }

        [Command("vconfigload")]
        public static void CMD_loadConfigVehicles(Player player, int type, int number)
        {
            try
            {
                if (!Group.CanUseCmd(player, "vconfigload")) return;
                if (type == 0) // fractionvehicles
                {
                    Fractions.Configs.FractionVehicles[number] = new Dictionary<string, Tuple<string, Vector3, Vector3, int, int, int, VehicleManager.VehicleCustomization>>();
                    DataTable result = MySQL.QueryRead($"SELECT * FROM `fractionvehicles` WHERE `fraction`={number}");
                    if (result == null || result.Rows.Count == 0) return;
                    foreach (DataRow Row in result.Rows)
                    {
                        var fraction = Convert.ToInt32(Row["fraction"]);
                        var numberplate = Row["number"].ToString();
                        var model = Row["model"].ToString();
                        var position = JsonConvert.DeserializeObject<Vector3>(Row["position"].ToString());
                        var rotation = JsonConvert.DeserializeObject<Vector3>(Row["rotation"].ToString());
                        var minrank = Convert.ToInt32(Row["rank"]);
                        var color1 = Convert.ToInt32(Row["colorprim"]);
                        var color2 = Convert.ToInt32(Row["colorsec"]);
                        VehicleManager.VehicleCustomization components = JsonConvert.DeserializeObject<VehicleManager.VehicleCustomization>(Row["components"].ToString());

                        Fractions.Configs.FractionVehicles[fraction].Add(numberplate, new Tuple<string, Vector3, Vector3, int, int, int, VehicleManager.VehicleCustomization>(model, position, rotation, minrank, color1, color2, new VehicleManager.VehicleCustomization()));
                    }

                    NAPI.Task.Run(() =>
                    {
                        try
                        {

                            foreach (var v in NAPI.Pools.GetAllVehicles())
                            {
                                if (v.HasData("ACCESS") && v.GetData<string>("ACCESS") == "FRACTION" && v.GetData<int>("FRACTION") == number)
                                    v.Delete();
                            }
                            Fractions.Configs.SpawnFractionCars(number);
                        }
                        catch { }
                    });
                }
                else // othervehicles
                {
                    var result = MySQL.QueryRead($"SELECT * FROM `othervehicles` WHERE `type`={number}");
                    if (result == null || result.Rows.Count == 0) return;

                    switch (number)
                    {
                        case 3:
                            Jobs.Taxi.CarInfos = new List<CarInfo>();
                            break;
                        case 4:
                            Jobs.Bus.CarInfos = new List<CarInfo>();
                            break;
                        case 5:
                            Jobs.Lawnmower.CarInfos = new List<CarInfo>();
                            break;
                        case 7:
                            Jobs.Collector.CarInfos = new List<CarInfo>();
                            break;
                        case 8:
                            Jobs.AutoMechanic.CarInfos = new List<CarInfo>();
                            break;
                    }

                    foreach (DataRow Row in result.Rows)
                    {
                        var numberPlate = Row["number"].ToString();
                        var model = (VehicleHash)NAPI.Util.GetHashKey(Row["model"].ToString());
                        var position = JsonConvert.DeserializeObject<Vector3>(Row["position"].ToString());
                        var rotation = JsonConvert.DeserializeObject<Vector3>(Row["rotation"].ToString());
                        var color1 = Convert.ToInt32(Row["color1"]);
                        var color2 = Convert.ToInt32(Row["color2"]);
                        var price = Convert.ToInt32(Row["price"]);
                        var data = new CarInfo(numberPlate, model, position, rotation, color1, color2, price);

                        switch (number)
                        {
                            case 3:
                                Jobs.Taxi.CarInfos.Add(data);
                                break;
                            case 4:
                                Jobs.Bus.CarInfos.Add(data);
                                break;
                            case 5:
                                Jobs.Lawnmower.CarInfos.Add(data);
                                break;
                            case 7:
                                Jobs.Collector.CarInfos.Add(data);
                                break;
                            case 8:
                                Jobs.AutoMechanic.CarInfos.Add(data);
                                break;
                        }
                    }

                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            foreach (var v in NAPI.Pools.GetAllVehicles())
                            {
                                if (v.HasData("ACCESS") && ((v.GetData<string>("ACCESS") == "RENT" && number == 0) || (v.GetData<string>("ACCESS") == "WORK" && v.GetData<int>("WORK") == number)))
                                    v.Delete();
                            }
                            switch (number)
                            {
                                case 0:
                                    //Rentcar.rentCarsSpawner();
                                    Log.Write("I had to respawn cars for rent");
                                    break;
                                case 3:
                                    Jobs.Taxi.taxiCarsSpawner();
                                    break;
                                case 4:
                                    Jobs.Bus.busCarsSpawner();
                                    break;
                                case 5:
                                    Jobs.Lawnmower.mowerCarsSpawner();
                                    break;
                                case 7:
                                    Jobs.Collector.collectorCarsSpawner();
                                    break;
                                case 8:
                                    Jobs.AutoMechanic.mechanicCarsSpawner();
                                    break;
                            }
                        }
                        catch { }
                    });
                }
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Cars respawned", 3000);
            }
            catch (Exception e) { Log.Write("vconfigload: " + e.Message, nLog.Type.Error); }
        }

        [Command("addpromo")] // Добавить промокод (7 лвл)
        public static void CMD_addPromo(Player player, int uuid, string promocode)
        {
            try
            {
                if (!Group.CanUseCmd(player, "addpromo")) return;
                promocode = promocode.ToLower();

                Main.PromoCodes.Add(promocode, new Tuple<int, int, int>(1, 0, uuid));

                MySqlCommand queryCommand = new MySqlCommand(@"
                    INSERT INTO `promocodes` (`name`, `type`, `count`, `owner`)
                    VALUES
                    (@NAME, @TYPE, @COUNT, @OWNER)
                ");

                queryCommand.Parameters.AddWithValue("@NAME", promocode);
                queryCommand.Parameters.AddWithValue("@TYPE", 1);
                queryCommand.Parameters.AddWithValue("@COUNT", 0);
                queryCommand.Parameters.AddWithValue("@OWNER", uuid);

                MySQL.Query(queryCommand);

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"success", 3000);
            }
            catch { }
        }


        [Command("giveammo")] // Выдать патроны (4 лвл)
        public static void CMD_ammo(Player client, int ID, int type, int amount = 1)
        {
            try
            {
                if (!Group.CanUseCmd(client, "giveammo")) return;

                var target = Main.GetPlayerByID(ID);
                if (target == null)
                {
                    Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, $"Player with this ID was not found", 3000);
                    return;
                }

                List<ItemType> types = new List<ItemType>
                {
                    ItemType.PistolAmmo,
                    ItemType.RiflesAmmo,
                    ItemType.ShotgunsAmmo,
                    ItemType.PistolAmmo2,
                    ItemType.SniperAmmo
                };

                if (type > types.Count || type < -1) return;

                var tryAdd = nInventory.TryAdd(target, new nItem(types[type], amount));
                if (tryAdd == -1 || tryAdd > 0)
                {
                    Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, "Insufficient inventory space", 3000);
                    return;
                }
                nInventory.Add(target, new nItem(types[type], amount));
            }
            catch { }
        }
        
        [Command("newvnum")] // Новый номер на транспорт (7 лвл)
        public static void CMD_newVehicleNumber(Player player, string oldNum, string newNum)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "newvnum")) return;
                if (!VehicleManager.Vehicles.ContainsKey(oldNum))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Such a machine does not exist.", 3000);
                    return;
                }

                if (VehicleManager.Vehicles.ContainsKey(newNum))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"This number already exists", 3000);
                    return;
                }

                var vData = VehicleManager.Vehicles[oldNum];
                VehicleManager.Vehicles.Remove(oldNum);
                VehicleManager.Vehicles.Add(newNum, vData);

                var house = Houses.HouseManager.GetHouse(vData.Holder, true);
                if (house != null)
                {
                    var garage = Houses.GarageManager.Garages[house.GarageID];
                    garage.DeleteCar(oldNum);
                    garage.SpawnCar(newNum);
                }

                MySQL.Query($"UPDATE vehicles SET number='{newNum}' WHERE number='{oldNum}'");
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"New number for {oldNum} = {newNum}", 3000);
            }
            catch (Exception e) { Log.Write("newvnum: " + e.Message, nLog.Type.Error); }
        }
        
        [Command("redname")] // Красный админский ник (1 лвл)
        public static void CMD_redname(Player player)
        {
            try
            {
                if (!Group.CanUseCmd(player, "redname")) return;

                if (!player.HasSharedData("REDNAME") || !player.GetSharedData<bool>("REDNAME"))
                {
                    player.SendChatMessage("~r~Admin nickname enabled");
                    player.SetSharedData("REDNAME", true);
                }
                else
                {
                    player.SendChatMessage("~r~Admin nickname disabled");
                    player.SetSharedData("REDNAME", false);
                }

            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("hidenick")] // Скрыть ник (7 лвл)
        public static void CMD_hidenick(Player player)
        {
            if (!Group.CanUseCmd(player, "hidenick")) return;
            if (!player.HasSharedData("HideNick") || !player.GetSharedData<bool>("HideNick"))
            {
                player.SendChatMessage("~g~HideNick ON");
                player.SetSharedData("HideNick", true);
            }
            else
            {
                player.SendChatMessage("~g~HideNick OFF");
                player.SetSharedData("HideNick", false);
            }

        }

        [Command("pay")]
        public static void CMD_paytoplayer(Player player, int id, int count)
        {
            if (count < 1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Enter correct data", 3000);
                return;
            }
            if (count > 500000)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"It is forbidden to transfer more 500 000$", 3000);
                return;
            }
            Player target = Main.GetPlayerByID(id);
            if (player == target)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Can't transfer money to yourself", 3000);
                return;
            }
            if (!Main.Players.ContainsKey(target) || player.Position.DistanceTo(target.Position) > 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The player is too far away from you", 3000);
                return;
            }
            if (count > Main.Players[player].Money)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You don't have enough funds", 3000);
                return;
            }
            player.PlayAnimation("mp_common", "givetake2_a", 1);
            NAPI.Task.Run(() =>
            {
                player.StopAnimation();
            }, 2000);
            Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"Player ({player.Value}) gave you {count}$", 3000);
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You gave the player ({target.Value}) {count}$", 3000);
            MoneySystem.Wallet.Change(target, +count);
            MoneySystem.Wallet.Change(player, -count);
            GameLog.Money($"player({Main.Players[player].UUID})", $"player({Main.Players[target].UUID})", count, $"transfer");
            Commands.RPChat("me", player, $"handed over(а) {count}$ " + "{name}", target);
        }

        [Command("dl")] // Читы для игроков
        public static void CMD_DL(Player player)
        {
            if (!player.HasSharedData("PLAYER_DL") || !player.GetSharedData<bool>("PLAYER_DL"))
            {
                player.SetSharedData("PLAYER_DL", true);
            }
            else
            {
                player.SetSharedData("PLAYER_DL", false);
            }
        }

        [Command("promo")]
        public static void SetPromoCommand(Player player, string text)
        {
            text = text.ToLower();
            if (string.IsNullOrEmpty(text))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Enter correct data", 3000);
                return;
            }
            if (Main.Accounts[player].PromoCodes[0] != "noref")
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You have already entered a promo code on your account", 3000);
                return;
            }
            if (!Main.PromoCodes.ContainsKey(text))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "This promo code does not exist.", 3000);
                return;
            }
            if (Main.Players[player].LVL > 3)
            {
                Notify.Error(player, "You already had a level 3 character, so entering a promotional code is not available");
                return;
            }
            Main.Accounts[player].PromoCodes[0] = text;
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(Main.Accounts[player].PromoCodes);
            MySQL.Query($"UPDATE promocodes SET count=count+1 WHERE name='{text}'");
            MySQL.Query($"UPDATE accounts SET promocodes='{json}' WHERE login='{Main.Accounts[player].Login}'");
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You have successfully entered a promo code {text}, the reward will be issued upon reaching level 3", 3000);
        }

        [Command("givereds")] // Issue RedBucks (8 lvl)
        public static void CMD_givereds(Player player, int id, int amount)
        {
            try
            {
                if (!Group.CanUseCmd(player, "givereds")) return;

                var target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player with this ID was not found", 3000);
                    return;
                }
                Admin.sendRedbucks(player, target, amount);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("checkprop")] // Узнать о имуществе игрока (3 лвл)
        public static void CMD_checkProperety(Player player, int id)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "checkprop")) return;

                var target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player with this ID was not found", 3000);
                    return;
                }

                if (!Main.Players.ContainsKey(target)) return;
                var house = Houses.HouseManager.GetHouse(target);
                if (house != null)
                {
                    if (house.Owner == target.Name)
                    {
                        player.SendChatMessage($"The player has a house worth ${house.Price} class '{Houses.HouseManager.HouseTypeList[house.Type].Name}'");
                        var targetVehicles = VehicleManager.getAllPlayerVehicles(target.Name);
                        foreach (var num in targetVehicles)
                            player.SendChatMessage($"The player has a car '{VehicleManager.Vehicles[num].Model}' with number '{num}'");
                    }
                    else
                        player.SendChatMessage($"The player is settled in the house to {house.Owner} cost ${house.Price}");
                }
                else
                    player.SendChatMessage("The player has no home");
            }
            catch (Exception e)
            {
                Log.Write("checkprop: " + e.Message, nLog.Type.Error);
            }
        }
        
        [Command("id", "~y~/id [имя/id]")] // Find a player by personal ID (1 lvl)
        public static void CMD_checkId(Player player, string target)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "id")) return;

                int id;
                if (Int32.TryParse(target, out id))
                {
                    foreach (var p in Main.Players.Keys.ToList())
                    {
                        if (!Main.Players.ContainsKey(p)) continue;
                        if (p.Value == id)
                        {
                            player.SendChatMessage($"ID: {p.Value} | {p.Name}");
                            return;
                        }
                    }
                    player.SendChatMessage("Player with this ID was not found");
                }
                else
                {
                    var players = 0;
                    foreach (var p in Main.Players.Keys.ToList())
                    {
                        if (!Main.Players.ContainsKey(p)) continue;
                        if (p.Name.ToUpper().Contains(target.ToUpper()))
                        {
                            player.SendChatMessage($"ID: {p.Value} | {p.Name}");
                            players++;
                        }
                    }
                    if (players == 0)
                        player.SendChatMessage("No player found with this name");
                }
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\"/id/:\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("setdim")] // Set up a virtual world for yourself (4 lvl)
        public static void CMD_setDim(Player player, int id, int dim)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "setdim")) return;

                var target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player with this ID was not found", 3000);
                    return;
                }

                if (!Main.Players.ContainsKey(target)) return;
                target.Dimension = Convert.ToUInt32(dim);
                GameLog.Admin($"{player.Name}", $"setDim({dim})", $"{target.Name}");
            }
            catch (Exception e)
            {
                Log.Write("setdim: " + e.Message, nLog.Type.Error);
            }
        }
        
        [Command("checkdim")] // Узнать виртуальный мир игрока (4 лвл)
        public static void CMD_checkDim(Player player, int id)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "checkdim")) return;

                var target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player with this ID was not found", 3000);
                    return;
                }

                if (!Main.Players.ContainsKey(target)) return;
                GameLog.Admin($"{player.Name}", $"checkDim", $"{target.Name}");
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Player measurement - {target.Dimension.ToString()}", 4000);
            }
            catch (Exception e)
            {
                Log.Write("checkdim: " + e.Message, nLog.Type.Error);
            }
        }
        
        [Command("setbizmafia")] // Выдать бизнес под контроль мафии (6 лвл)
        public static void CMD_setBizMafia(Player player, int mafia)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "setbizmafia")) return;
                if (player.GetData<int>("BIZ_ID") == -1) return;
                if (mafia < 10 || mafia > 13) return;

                Business biz = BusinessManager.BizList[player.GetData<int>("BIZ_ID")];
                biz.Mafia = mafia;
                biz.UpdateLabel();
                GameLog.Admin($"{player.Name}", $"setBizMafia({biz.ID},{mafia})", $"");
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"{mafia} the mafia now owns the business №{biz.ID}", 3000);
            }
            catch (Exception e) { Log.Write("setbizmafia: " + e.Message, nLog.Type.Error); }
        }
        
        [Command("newsimcard")] // New sim card with a number for the player (8 lvl)
        public static void CMD_newsimcard(Player player, int id, int newnumber)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "newsimcard")) return;

                var target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player with this ID was not found", 3000);
                    return;
                }

                if (!Main.Players.ContainsKey(target)) return;
                if (Main.SimCards.ContainsKey(newnumber))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"This number already exists", 3000);
                    return;
                }

                Main.SimCards.Remove(newnumber);
                Main.SimCards.Add(newnumber, Main.Players[target].UUID);
                Main.Players[target].Sim = newnumber;
                GUI.Dashboard.sendStats(target);
                GameLog.Admin($"{player.Name}", $"newsim({newnumber})", $"{target.Name}");
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"New number for {target.Name} = {newnumber}", 3000);
            }
            catch (Exception e) { Log.Write("newsimcard: " + e.Message, nLog.Type.Error); }
        }
        
        [Command("takeoffbiz")] // Отобрать бизнес у игрока (8 лвл)
        public static void CMD_takeOffBusiness(Player admin, int bizid, bool byaclear = false)
        {
            try
            {
                if (!Main.Players.ContainsKey(admin)) return;
                if (!Group.CanUseCmd(admin, "takeoffbiz")) return;

                var biz = BusinessManager.BizList[bizid];
                var owner = biz.Owner;
                var player = NAPI.Player.GetPlayerFromName(owner);

                if (player != null && Main.Players.ContainsKey(player))
                {
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"The administrator took your business away from you", 3000);
                    MoneySystem.Wallet.Change(player, Convert.ToInt32(biz.SellPrice * 0.8));
                    Main.Players[player].BizIDs.Remove(biz.ID);
                }
                else
                {
                    var split = biz.Owner.Split('_');
                    var data = MySQL.QueryRead($"SELECT biz,money FROM characters WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                    List<int> ownerBizs = new List<int>();
                    var money = 0;

                    foreach (DataRow Row in data.Rows)
                    {
                        ownerBizs = JsonConvert.DeserializeObject<List<int>>(Row["biz"].ToString());
                        money = Convert.ToInt32(Row["money"]);
                    }

                    ownerBizs.Remove(biz.ID);
                    MySQL.Query($"UPDATE characters SET biz='{JsonConvert.SerializeObject(ownerBizs)}',money={money + Convert.ToInt32(biz.SellPrice * 0.8)} WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                }

                MoneySystem.Bank.Accounts[biz.BankID].Balance = 0;
                biz.Owner = "Государство";
                biz.UpdateLabel();
                GameLog.Money($"server", $"player({Main.PlayerUUIDs[owner]})", Convert.ToInt32(biz.SellPrice * 0.8), $"takeoffBiz({biz.ID})");
                Notify.Send(admin, NotifyType.Info, NotifyPosition.BottomCenter, $"You have taken the business from {owner}", 3000);
                if (!byaclear) GameLog.Admin($"{player.Name}", $"takeoffBiz({biz.ID})", $"");
            }
            catch (Exception e) { Log.Write("takeoffbiz: " + e.Message, nLog.Type.Error); }
        }
        
        [Command("paydaymultiplier")] // Set multiplier on PaydayMultiplier (8 lvl)
        public static void CMD_paydaymultiplier(Player player, int multi)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "paydaymultiplier")) return;
                if (multi < 1 || multi > 5)
                {
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Can only be set from 1 to 5", 3000);
                    return;
                }

                Main.oldconfig.PaydayMultiplier = multi;
                GameLog.Admin($"{player.Name}", $"paydayMultiplier({multi})", $"");
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"PaydayMultiplier changed to {multi}", 3000);
            }
            catch (Exception e) { Log.Write("paydaymultiplier: " + e.Message, nLog.Type.Error); }
        }
        
        [Command("expmultiplier")] // Установить множитель на ExpMultiplier (7 лвл)
        public static void CMD_expmultiplier(Player player, int multi)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "expmultiplier")) return;
                if (multi < 1 || multi > 5)
                {
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Can only be set from 1 to 5", 3000);
                    return;
                }

                Main.oldconfig.ExpMultiplier = multi;
                GameLog.Admin($"{player.Name}", $"expMultiplier({multi})", $"");
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"ExpMultiplier changed to {multi}", 3000);
            }
            catch (Exception e) { Log.Write("paydaymultiplier: " + e.Message, nLog.Type.Error); }
        }
        
        [Command("offdelfrac")] // Dismiss a player from the offline faction (lvl 5)
        public static void CMD_offlineDelFraction(Player player, string name)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "offdelfrac")) return;

                var split = name.Split('_');
                MySQL.Query($"UPDATE `characters` SET fraction=0,fractionlvl=0 WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You fired a player {name} from your faction", 3000);

                int index = Fractions.Manager.AllMembers.FindIndex(m => m.Name == name);
                if (index > -1) Fractions.Manager.AllMembers.RemoveAt(index);

                GameLog.Admin($"{player.Name}", $"delfrac", $"{name}");
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"You removed a faction from {name}", 3000);
            }
            catch (Exception e) { Log.Write("offdelfrac: " + e.Message, nLog.Type.Error); }
        }
        
        [Command("removeobj")] // ??? (8 лвл)
        public static void CMD_removeObject(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "removeobj")) return;

                player.SetData("isRemoveObject", true);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"The next item you pick up will be in the bath", 3000);
            }
            catch (Exception e) { Log.Write("removeobj: " + e.Message, nLog.Type.Error); }
        }
        
        [Command("unwarn")] // Снять варн у игрока (3 лвл)
        public static void CMD_unwarn(Player player, int id)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "unwarn")) return;

                var target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player with this ID was not found", 3000);
                    return;
                }

                if (!Main.Players.ContainsKey(target)) return;
                if (Main.Players[target].Warns <= 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The player has no varns", 3000);
                    return;
                }

                Main.Players[target].Warns--;
                GUI.Dashboard.sendStats(target);

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Removed Warn from a player {target.Name}, in him {Main.Players[target].Warns} варнов", 3000);
                Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"You have removed the varn, it remains {Main.Players[target].Warns} варнов", 3000);
                GameLog.Admin($"{player.Name}", $"unwarn", $"{target.Name}");
            }
            catch (Exception e) { Log.Write("unwarn: " + e.Message, nLog.Type.Error); }
        }
        
        [Command("offunwarn")] // Снять варн у игрока в оффлайне (3 лвл)
        public static void CMD_offunwarn(Player player, string target)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "offunwarn")) return;

                if (!Main.PlayerNames.ContainsValue(target))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player not found", 3000);
                    return;
                }
                if (NAPI.Player.GetPlayerFromName(target) != null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player online", 3000);
                    return;
                }

                var split = target.Split('_');
                var data = MySQL.QueryRead($"SELECT warns FROM characters WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                var warns = 0;
                foreach (System.Data.DataRow Row in data.Rows)
                {
                    warns = Convert.ToInt32(Row["warns"]);
                }

                if (warns <= 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The player has no varns", 3000);
                    return;
                }

                warns--;
                GameLog.Admin($"{player.Name}", $"offUnwarn", $"{target}");
                MySQL.Query($"UPDATE characters SET warns={warns} WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Removed Warn from a player {target}, in him {warns} varnov", 3000);
            }
            catch (Exception e) { Log.Write("offunwarn: " + e.Message, nLog.Type.Error); }
        }
        
        [Command("rescar")] // Respawn auto player (3 lvl)
        public static void CMD_respawnCar(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "rescar")) return;
                if (!player.IsInVehicle) return;
                var vehicle = player.Vehicle;

                if (!vehicle.HasData("ACCESS"))
                    return;
                else if (vehicle.GetData<string>("ACCESS") == "PERSONAL")
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "At the moment, the function of restoring a personal car of a player is disabled", 3000);
                else if (vehicle.GetData<string>("ACCESS") == "WORK")
                    Admin.RespawnWorkCar(vehicle);
                else if (vehicle.GetData<string>("ACCESS") == "FRACTION")
                    Admin.RespawnFractionCar(vehicle);
                else if (vehicle.GetData<string>("ACCESS") == "GANGDELIVERY" || vehicle.GetData<string>("ACCESS") == "MAFIADELIVERY")
                    NAPI.Entity.DeleteEntity(vehicle);

                GameLog.Admin($"{player.Name}", $"rescar", $"");
            }
            catch (Exception e) { Log.Write("ResCar: " + e.Message, nLog.Type.Error); }
        }
        
        [Command("bansync")] // Synchronization of bans ??? (BGN 3)
        public static void CMD_banlistSync(Player client)
        {
            try
            {
                if (!Group.CanUseCmd(client, "bansync")) return;
                Notify.Send(client, NotifyType.Warning, NotifyPosition.BottomCenter, "Starting the sync process...", 4000);
                Ban.Sync();
                Notify.Send(client, NotifyType.Success, NotifyPosition.BottomCenter, "The procedure is complete!", 3000);
            }
            catch (Exception e) { Log.Write("bansync: " + e.Message, nLog.Type.Error); }
        }
        
        [Command("setcolour")] // Changing the color of territories for gangs (6 lvl)
        public static void CMD_setTerritoryColor(Player player, int gangid)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "setcolour")) return;

                if (player.GetData<int>("GANGPOINT") == -1)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You are not located in any of the regions", 3000);
                    return;
                }
                var terrid = player.GetData<int>("GANGPOINT");

                if (!Fractions.GangsCapture.gangPointsColor.ContainsKey(gangid))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"There is no gang with this ID", 3000);
                    return;
                }

                Fractions.GangsCapture.gangPoints[terrid].GangOwner = gangid;
                Main.ClientEventToAll("setZoneColor", Fractions.GangsCapture.gangPoints[terrid].ID, Fractions.GangsCapture.gangPointsColor[gangid]);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Now territory №{terrid} owns {Fractions.Manager.FractionNames[gangid]}", 3000);
                GameLog.Admin($"{player.Name}", $"setColour({terrid},{gangid})", $"");
            }
            catch (Exception e) { Log.Write("CMD_SetColour: " + e.Message, nLog.Type.Error); }
        }
        
        [Command("sc")] // Выдать одежду (8 лвл)
        public static void CMD_setClothes(Player player, int id, int draw, int texture)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "sc")) return;
                player.SetClothes(id, draw, texture);
                if (id == 11) player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][draw], 0);
                if (id == 1) Customization.SetMask(player, draw, texture);
            }
            catch { }
        }
        
        [Command("sac")] //Issue Aksy (8 lvl)
        public static void CMD_setAccessories(Player player, int id, int draw, int texture)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (!Group.CanUseCmd(player, "sac")) return;
            if (draw > -1)
                player.SetAccessories(id, draw, texture);
            else
                player.ClearAccessory(id);

        }
        
        [Command("checkwanted")] // Find out the wanted player (8 lvl)
        public static void CMD_checkwanted(Player player, int id)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (!Group.CanUseCmd(player, "checkwanted")) return;
            var target = Main.GetPlayerByID(id);
            if (target == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Person with this ID was not found", 3000);
                return;
            }
            var stars = (Main.Players[target].WantedLVL == null) ? 0 : Main.Players[target].WantedLVL.Level;
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Number of stars - {stars}", 3000);
        }
        
        [Command("fixcar")] // Repair a car (3 lvl)
        public static void CMD_fixcar(Player player)
        {
            try
            {
                if (!Group.CanUseCmd(player, "fixcar")) return;
                if (!player.IsInVehicle) return;
                VehicleManager.RepairCar(player.Vehicle);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"CMD_fixcar\":" + e.ToString(), nLog.Type.Error);
            }
        }
        
        [Command("stats")] // Player statistics (2 lvl)
        public static void CMD_showPlayerStats(Player admin, int id)
        {
            try
            {
                if (!Group.CanUseCmd(admin, "stats")) return;

                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(admin, NotifyType.Error, NotifyPosition.BottomCenter, $"Player with this ID was not found", 3000);
                    return;
                }

                Player player = Main.GetPlayerByID(id);
                if (player == admin) return;
                var acc = Main.Players[player];
                string status =
                    (acc.AdminLVL >= 1) ? "Администратор" :
                    (Main.Accounts[player].VipLvl > 0) ? $"{Group.GroupNames[Main.Accounts[player].VipLvl]} до {Main.Accounts[player].VipDate.ToString("dd.MM.yyyy")}" :
                    $"{Group.GroupNames[Main.Accounts[player].VipLvl]}";

                long bank = (acc.Bank != 0) ? MoneySystem.Bank.Accounts[acc.Bank].Balance : 0;

                var lic = "";
                for (int i = 0; i < acc.Licenses.Count; i++)
                    if (acc.Licenses[i]) lic += $"{Main.LicWords[i]} / ";
                if (lic == "") lic = "Отсутствуют";

                string work = (acc.WorkID > 0) ? Jobs.WorkManager.JobStats[acc.WorkID - 1] : "Unemployed";
                string fraction = (acc.FractionID > 0) ? Fractions.Manager.FractionNames[acc.FractionID] : "Нет";

                var number = (acc.Sim == -1) ? "Нет сим-карты" : Main.Players[player].Sim.ToString();

                List<object> data = new List<object>
                {
                    acc.LVL,
                    $"{acc.EXP}/{3 + acc.LVL * 3}",
                    number,
                    status,
                    0,
                    acc.Warns,
                    lic,
                    acc.CreateDate.ToString("dd.MM.yyyy"),
                    acc.UUID,
                    acc.Bank,
                    work,
                    fraction,
                    acc.FractionLVL,
                };

                string json = JsonConvert.SerializeObject(data);
                Trigger.ClientEvent(admin, "board", 2, json);
                admin.SetData("CHANGE_WITH", player);
                GUI.Dashboard.OpenOut(admin, nInventory.Items[acc.UUID], player.Name, 20);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"CMD_showPlayerStats\":" + e.ToString(), nLog.Type.Error);
            }
        }
        
        [Command("admins")] // List of administrators online (2 lvl)
        public static void CMD_AllAdmins(Player client)
        {
            try
            {
                if (!Group.CanUseCmd(client, "admins")) return;

                client.SendChatMessage("=== ADMINS ONLINE ===");
                foreach (var p in Main.Players)
                {
                    if (p.Value.AdminLVL < 1) continue;
                    client.SendChatMessage($"[{p.Key.Value}] {p.Key.Name} - {p.Value.AdminLVL}");
                }
                client.SendChatMessage("=== ADMINS ONLINE ===");

            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"CMD_AllAdmins\":" + e.ToString(), nLog.Type.Error);
            }
        }
        
        [Command("fixweaponsshops")] // Фикс ??? (8 лвл)
        public static void CMD_fixweaponsshops(Player client)
        {
            try
            {
                if (!Group.CanUseCmd(client, "fixweaponsshops")) return;

                foreach (var biz in BusinessManager.BizList.Values)
                {
                    if (biz.Type != 6) continue;
                    biz.Products = BusinessManager.fillProductList(6);

                    var result = MySQL.QueryRead($"SELECT * FROM `weapons` WHERE id={biz.ID}");
                    if (result != null) continue;
                    MySQL.Query($"INSERT INTO weapons (id,lastserial) VALUES ({biz.ID},0)");
                    Log.Debug($"Insert into weapons new business ({biz.ID})");
                }
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"CMD_fixweaponsshops\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
        
        [Command("fixgovbizprices")] // Фикс ??? (8 лвл)
        public static void CMD_fixgovbizprices(Player client)
        {
            try
            {
                if (!Group.CanUseCmd(client, "fixgovbizprices")) return;

                foreach (var biz in BusinessManager.BizList.Values)
                {
                    foreach (var p in biz.Products)
                    {
                        if (p.Name == "Расходники" || biz.Type == 7 || biz.Type == 11 || biz.Type == 12 || p.Name == "Татуировки" || p.Name == "Парики" || p.Name == "Патроны") continue;
                        p.Price = BusinessManager.ProductsOrderPrice[p.Name];
                    }
                    biz.UpdateLabel();
                }

                foreach (var biz in BusinessManager.BizList.Values)
                {
                    if (biz.Owner != "Государство") continue;
                    foreach (var p in biz.Products)
                    {
                        if (p.Name == "Расходники") continue;
                        double price = (biz.Type == 7 || biz.Type == 11 || biz.Type == 12 || p.Name == "Татуировки" || p.Name == "Парики" || p.Name == "Патроны") ? 125 : (biz.Type == 1) ? 6 : BusinessManager.ProductsOrderPrice[p.Name] * 1.25;
                        p.Price = Convert.ToInt32(price);
                        p.Lefts = Convert.ToInt32(BusinessManager.ProductsCapacity[p.Name] * 0.1);
                    }
                    biz.UpdateLabel();
                }
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"CMD_fixgovbizprices\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
        
        [Command("setproductbyindex")] // Выдать продукты в бизнес (8 лвл)
        public static void CMD_setproductbyindex(Player client, int id, int index, int product)
        {
            try
            {
                if (!Group.CanUseCmd(client, "setproductbyindex")) return;

                if (!BusinessManager.BizList.ContainsKey(id))
                {
                    Notify.Error(client, "The specified business ID was not found", 2000);
                    return;
                }
                var biz = BusinessManager.BizList[id];
                if(index >= biz.Products.Count)
                {
                    Notify.Error(client, "The specified product ID was not found", 2000);
                    return;
                }
                biz.Products[index].Lefts = product;
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"CMD_setproductbyindex\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
        
        [Command("deleteproducts")] // Delete products in business (8 lvl)
        public static void CMD_deleteproducts(Player client, int id)
        {
            try
            {
                if (!Group.CanUseCmd(client, "deleteproducts")) return;

                if (!BusinessManager.BizList.ContainsKey(id))
                {
                    Notify.Error(client, "The specified business ID was not found", 2000);
                    return;
                }
                var biz = BusinessManager.BizList[id];
                foreach (var p in biz.Products)
                    p.Lefts = 0;
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"CMD_setproductbyindex\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
        
        [Command("changebizprice")] // Изменить цену бизнеса (8 лвл)
        public static void CMD_changeBusinessPrice(Player player, int newPrice)
        {
            if (!Group.CanUseCmd(player, "changebizprice")) return;
            if (player.GetData<int>("BIZ_ID") == -1)
            {
                player.SendChatMessage("~r~You must be in one of the businesses");
                return;
            }
            Business biz = BusinessManager.BizList[player.GetData<int>("BIZ_ID")];
            biz.SellPrice = newPrice;
            biz.UpdateLabel();
        }
        
        [Command("pa")] // Perform animation (8 lvl)
        public static void CMD_playAnimation(Player player, string dict, string anim, int flag)
        {
            if (!Group.CanUseCmd(player, "pa")) return;
            player.PlayAnimation(dict, anim, flag);
        }
        
        [Command("sa")] // Stop animation (8 lvl)
        public static void CMD_stopAnimation(Player player)
        {
            if (!Group.CanUseCmd(player, "sa")) return;
            player.StopAnimation();
        }
        
        [Command("changestock")] // Change faction warehouse (lvl 6)
        public static void CMD_changeStock(Player player, int fracID, string item, int amount)
        {
            if (!Group.CanUseCmd(player, "changestock")) return;
            if (!Fractions.Stocks.fracStocks.ContainsKey(fracID))
            {
                player.SendChatMessage("~r~There is no warehouse for this faction");
                return;
            }
            switch (item)
            {
                case "mats":
                    Fractions.Stocks.fracStocks[fracID].Materials += amount;
                    Fractions.Stocks.fracStocks[fracID].UpdateLabel();
                    return;
                case "drugs":
                    Fractions.Stocks.fracStocks[fracID].Drugs += amount;
                    Fractions.Stocks.fracStocks[fracID].UpdateLabel();
                    return;
                case "medkits":
                    Fractions.Stocks.fracStocks[fracID].Medkits += amount;
                    Fractions.Stocks.fracStocks[fracID].UpdateLabel();
                    return;
                case "money":
                    Fractions.Stocks.fracStocks[fracID].Money += amount;
                    Fractions.Stocks.fracStocks[fracID].UpdateLabel();
                    return;
            }
            player.SendChatMessage("~r~mats - materials");
            player.SendChatMessage("~r~drugs - drugs");
            player.SendChatMessage("~r~medkits - honey. first aid kits");
            player.SendChatMessage("~r~money - money");
            GameLog.Admin($"{player.Name}", $"changeStock({item},{amount})", $"");
        }
        
        [Command("tpc")] // Teleport by coordinates (3 lvl)
        public static void CMD_tpCoord(Player player, double x, double y, double z)
        {
            if (!Group.CanUseCmd(player, "tpc")) return;
            NAPI.Entity.SetEntityPosition(player, new Vector3(x, y, z));
        }
        
        [Command("inv")] // Invisibility (2 lvl)
        public static void CMD_ToogleInvisible(Player player)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (!Group.CanUseCmd(player, "inv")) return;

            BasicSync.SetInvisible(player, !BasicSync.GetInvisible(player));
        }
        
        [Command("delfrac")] // Remove player from faction (lvl 3)
        public static void CMD_delFrac(Player player, int id)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (Main.GetPlayerByID(id) == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player with this ID was not found", 3000);
                return;
            }
            Admin.delFrac(player, Main.GetPlayerByID(id));
        }
        
        [Command("sendcreator")] // Send the player to the appearance editor (lvl 5)
        public static void CMD_SendToCreator(Player player, int id)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (!Group.CanUseCmd(player, "sendcreator")) return;
            var target = Main.GetPlayerByID(id);
            if (target == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player with this ID was not found", 3000);
                return;
            }
            Customization.SendToCreator(target);
            GameLog.Admin($"{player.Name}", $"sendCreator", $"{target.Name}");
        }
        
        [Command("afuel")] // Заправить авто (2 лвл)
        public static void CMD_setVehiclePetrol(Player player, int fuel)
        {
            try
            {
                if (!Group.CanUseCmd(player, "afuel")) return;
                if (!player.IsInVehicle) return;
                player.Vehicle.SetSharedData("PETROL", fuel);
                GameLog.Admin($"{player.Name}", $"afuel({fuel})", $"");
            }
            catch (Exception e) { Log.Write("afuel: " + e.Message, nLog.Type.Error); }
        }
        
        [Command("changename", GreedyArg = true)] // Изменить имя игрока (5 лвл)
        public static void CMD_changeName(Player client, string curient, string newName)
        {
            try
            {
                if (!Group.CanUseCmd(client, "changename")) return;
                if (!Main.PlayerNames.ContainsValue(curient)) return;

                try
                {
                    string[] split = newName.Split("_");
                    Log.Debug($"SPLIT: {split[0]} {split[1]}");
                }
                catch (Exception e)
                {
                    Log.Write("ERROR ON CHANGENAME COMMAND\n" + e.ToString());
                }


                if (Main.PlayerNames.ContainsValue(newName))
                {
                    Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, "This name already exists!", 3000);
                    return;
                }

                Player target = NAPI.Player.GetPlayerFromName(curient);
                Character.Character.toChange.Add(curient, newName);

                if (target == null || target.IsNull)
                {
                    Notify.Send(client, NotifyType.Alert, NotifyPosition.BottomCenter, "The player is offline, change...", 3000);
                    Task changeTask = Character.Character.changeName(curient);
                }
                else
                {
                    Notify.Send(client, NotifyType.Alert, NotifyPosition.BottomCenter, "Player online, kick...", 3000);
                    NAPI.Player.KickPlayer(target);
                }

                Notify.Send(client, NotifyType.Success, NotifyPosition.BottomCenter, "Nick changed!", 3000);
                GameLog.Admin($"{client.Name}", $"changeName({newName})", $"{curient}");

            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"CMD_CHANGENAME\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
        
        [Command("startmatwars")] // Start a mating war (lvl 6)
        public static void CMD_startMatWars(Player player)
        {
            try
            {
                if (!Group.CanUseCmd(player, "startmatwars")) return;
                if (Fractions.MatsWar.isWar)
                {
                    player.SendChatMessage("~r~The war for mats is already on");
                    return;
                }
                Fractions.MatsWar.startMatWarTimer();
                player.SendChatMessage("~r~The war for mats has begun");
                GameLog.Admin($"{player.Name}", $"startMatwars", $"");
            }
            catch (Exception e) { Log.Write("startmatwars: " + e.Message, nLog.Type.Error); }
        }
        
        [Command("whitelistdel")] //Remove player from WhiteList (8 lvl)
        public static void CMD_whitelistdel(Player player, string socialClub)
        {
            if (!Group.CanUseCmd(player, "whitelistdel")) return;

            try
            {
                if (CheckSocialClubInWhiteList(socialClub))
                {
                    MySQL.Query("DELETE FROM `whiteList` WHERE `socialclub` = '" + socialClub + "';");
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Social club successfully removed from the white list!", 3000);
                }
                else
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "This social club was not found in the white list!", 3000);
                }
                GameLog.Admin($"{player.Name}", $"whitelistdel", $"");
            }
            catch (Exception e) { Log.Write("whitelistdel: " + e.Message, nLog.Type.Error); }
        }
        public static bool CheckSocialClubInWhiteList(string SocialClub)
        {
            DataTable data = MySQL.QueryRead($"SELECT * FROM `whiteList` WHERE 1");
            foreach (DataRow Row in data.Rows)
            {
                if (Row["socialclub"].ToString() == SocialClub)
                {
                    return true;
                }
            }
            return false;
        }
        
        [Command("whitelistadd")] //Add player from WhiteList (8 lvl)
        public static void CMD_whitelistadd(Player player, string socialClub)
        {
            if (!Group.CanUseCmd(player, "whitelistadd")) return;

            try
            {
                if (CheckSocialClubInAccounts(socialClub))
                {
                    if (!CheckSocialClubInWhiteList(socialClub))
                    {
                        MySQL.Query("INSERT INTO `whiteList` (`socialclub`) VALUES ('" + socialClub + "');");
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Social club successfully added to white list!", 3000);
                    }
                    else
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "This social club is already whitelisted!", 3000);
                    }
                }
                else
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "This social club was not found!", 3000);
                }
                GameLog.Admin($"{player.Name}", $"whitelistadd", $"");
            }
            catch (Exception e) { Log.Write("whitelistadd: " + e.Message, nLog.Type.Error); }
        }
        public static bool CheckSocialClubInAccounts(string SocialClub)
        {
            DataTable data = MySQL.QueryRead($"SELECT * FROM `accounts` WHERE 1");
            foreach (DataRow Row in data.Rows)
            {
                if (Row["socialclub"].ToString() == SocialClub)
                {
                    return true;
                }
            }
            return false;
        }
        
        [Command("giveexp")] //Give EXP to a player (lvl 6)
        public static void CMD_giveExp(Player player, int id, int exp)
        {
            try
            {
                if (!Group.CanUseCmd(player, "giveexp")) return;
                var target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player with this ID was not found", 3000);
                    return;
                }
                Main.Players[target].EXP += exp;
                if (Main.Players[target].EXP >= 3 + Main.Players[target].LVL * 3)
                {
                    Main.Players[target].EXP = Main.Players[target].EXP - (3 + Main.Players[target].LVL * 3);
                    Main.Players[target].LVL += 1;
                    if (Main.Players[target].LVL == 1)
                    {
                        NAPI.Task.Run(() => { try { Trigger.ClientEvent(target, "disabledmg", false); } catch { } }, 5000);
                    }
                }
                Dashboard.sendStats(target);
                GameLog.Admin($"{player.Name}", $"giveExp({exp})", $"{target.Name}");
            }
            catch (Exception e) { Log.Write("giveexp" + e.Message, nLog.Type.Error); }
        }
        
        [Command("housetypeprice")] // ??? (8 lbs)
        public static void CMD_replaceHousePrices(Player player, int type, int newPrice)
        {
            if (!Group.CanUseCmd(player, "housetypeprice")) return;
            foreach (var h in Houses.HouseManager.Houses)
            {
                if (h.Type != type) continue;
                h.Price = newPrice;
                h.UpdateLabel();
                h.Save();
            }
        }
        
        [Command("delhouseowner")] // Remove the owner from the house (8 lvl)
        public static void CMD_deleteHouseOwner(Player player)
        {
            if (!Group.CanUseCmd(player, "delhouseowner")) return;
            if (!player.HasData("HOUSEID"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You must be on the home marker", 3000);
                return;
            }

            Houses.House house = Houses.HouseManager.Houses.FirstOrDefault(h => h.ID == player.GetData<int>("HOUSEID"));
            if (house == null) return;

            house.SetOwner(null);
            house.UpdateLabel();
            house.Save();
            GameLog.Admin($"{player.Name}", $"delHouseOwner({house.ID})", $"");
        }
        
        [Command("stt")] // Boost car / acceleration (7 lvl)
        public static void CMD_SetTurboTorque(Player player, float power, float torque)
        {
            try
            {
                if (!Group.CanUseCmd(player, "stt")) return;
                if (!player.IsInVehicle) return;
                Trigger.ClientEvent(player, "svem", power, torque);
            }
            catch (Exception e)
            {
                Log.Write("Error at \"STT\":" + e.ToString(), nLog.Type.Error);
            }
        }
        
        [Command("svm")] // Car modification / tuning (8 lvl)
        public static void CMD_SetVehicleMod(Player player, int type, int index)
        {
            try
            {
                if (!Group.CanUseCmd(player, "svm")) return;
                if (!player.IsInVehicle) return;
                player.Vehicle.SetMod(type, index);

            }
            catch (Exception e)
            {
                Log.Write("Error at \"SVM\":" + e.ToString(), nLog.Type.Error);
            }
        }
        
        [Command("svn")] // Install neon on car (8 lvl)
        public static void CMD_SetVehicleNeon(Player player, byte r, byte g, byte b, byte alpha)
        {
            try
            {
                if (!Group.CanUseCmd(player, "svn")) return;
                if (!player.IsInVehicle) return;
                Vehicle v = player.Vehicle;
                if (alpha != 0)
                {
                    NAPI.Vehicle.SetVehicleNeonState(v, true);
                    NAPI.Vehicle.SetVehicleNeonColor(v, r, g, b);
                }
                else
                {
                    NAPI.Vehicle.SetVehicleNeonColor(v, 255, 255, 255);
                    NAPI.Vehicle.SetVehicleNeonState(v, false);
                }
            }
            catch (Exception e)
            {
                Log.Write("Error at \"SVN\":" + e.ToString(), nLog.Type.Error);
            }
        }
        
        [Command("svhid")] //Set the color of the headlights on the car (8 lvl)
        public static void CMD_SetVehicleHeadlightColor(Player player, int hlcolor)
        {
            try
            {
                if (!Group.CanUseCmd(player, "svhid")) return;
                if (!player.IsInVehicle) return;
                Vehicle v = player.Vehicle;
                if (hlcolor >= 0 && hlcolor <= 12)
                {
                    v.SetSharedData("hlcolor", hlcolor);
                    Trigger.ClientEventInRange(v.Position, 250f, "VehStream_SetVehicleHeadLightColor", v.Handle, hlcolor);
                }
                else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Headlight color can be from 0 to 12.", 3000);
            }
            catch (Exception e)
            {
                Log.Write("Error at \"SVN\":" + e.ToString(), nLog.Type.Error);
            }
        }
        
        [Command("svh")] // Set vehicle health / repair (lvl 8)
        public static void CMD_SetVehicleHealth(Player player, int health = 100)
        {
            try
            {
                if (!Group.CanUseCmd(player, "svh")) return;
                if (!player.IsInVehicle) return;
                Vehicle v = player.Vehicle;
                v.Repair();
                v.Health = health;

            }
            catch (Exception e)
            {
                Log.Write("Error at \"SVH\":" + e.ToString(), nLog.Type.Error);
            }

        }
        
        [Command("delacars")] // Delete all created admin cars (lvl 5)
        public static void CMD_deleteAdminCars(Player player)
        {
            try
            {
                NAPI.Task.Run(() =>
                {
                    try
                    {
                        if (!Group.CanUseCmd(player, "delacars")) return;
                        foreach (var v in NAPI.Pools.GetAllVehicles())
                        {
                            if (v.HasData("ACCESS") && v.GetData<string>("ACCESS") == "ADMIN")
                                v.Delete();
                        }
                        GameLog.Admin($"{player.Name}", $"delacars", $"");
                    }
                    catch { }
                });
            }
            catch (Exception e) { Log.Write("delacars: " + e.Message, nLog.Type.Error); }
        }
       
        [Command("delacar")] //Delete a specific admin car (lvl 3)
        public static void CMD_deleteThisAdminCar(Player client)
        {
            if (!Group.CanUseCmd(client, "delacar")) return;
            if (!client.IsInVehicle) return;
            Vehicle veh = client.Vehicle;
            if (veh.HasData("ACCESS") && veh.GetData<string>("ACCESS") == "ADMIN")
                veh.Delete();
            GameLog.Admin($"{client.Name}", $"delacar", $"");
        }
        
        [Command("delmycars", "dmcs")] //Delete all your admin cars (4 lvl)
        public static void CMD_delMyCars(Player client)
        {
            try
            {
                NAPI.Task.Run(() =>
                {
                    try
                    {
                        if (!Group.CanUseCmd(client, "delmycars")) return;
                        foreach (var v in NAPI.Pools.GetAllVehicles())
                        {
                            if (v.HasData("ACCESS") && v.GetData<string>("ACCESS") == "ADMIN")
                            {
                                if (v.GetData<string>("BY") == client.Name)
                                    v.Delete();
                            }
                        }
                        GameLog.Admin($"{client.Name}", $"delmycars", $"");
                    }
                    catch { }
                });
            }
            catch (Exception e) { Log.Write("delacars: " + e.Message, nLog.Type.Error); }
        }
        
        [Command("allspawncar")] // Respawn all cars (8 lvl)
        public static void CMD_allSpawnCar(Player player)
        {
            Admin.respawnAllCars(player);
        }
        
        [Command("save")] // Save coordinates (8 lvl)
        public static void CMD_saveCoord(Player player, string name)
        {
            Admin.saveCoords(player, name);
        }

        [Command("savepos")]
        public static void CMD_saveCoords(Player player, string msg = "No name")
        {
            if (!Group.CanUseCmd(player, "savepos")) return;
            Vector3 pos = NAPI.Entity.GetEntityPosition(player);
            Vector3 rot = NAPI.Entity.GetEntityRotation(player);
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                Vehicle vehicle = player.Vehicle;
                pos = NAPI.Entity.GetEntityPosition(vehicle) + new Vector3(0, 0, 0.5);
                rot = NAPI.Entity.GetEntityRotation(vehicle);
            }
            try
            {
                StreamWriter saveCoords = new StreamWriter("logs/savedcoords.txt", true, Encoding.UTF8);
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                saveCoords.Write($"{msg}   Coords: new Vector3({pos.X}, {pos.Y}, {pos.Z}),    JSON: {JsonConvert.SerializeObject(pos)}      \r\n");
                saveCoords.Write($"{msg}   Rotation: new Vector3({rot.X}, {rot.Y}, {rot.Z}),     JSON: {JsonConvert.SerializeObject(rot)}    \r\n");
                saveCoords.Write($"===========================================================================================================================\r\n\n");
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

        [Command("setfractun")]
        public static void ACMD_setfractun(Player player, int cat = -1, int id = -1)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "setvehdirt")) return;
                if (!player.IsInVehicle)
                {
                    player.SendChatMessage("You must be in the vehicle of the faction you wish to change.");
                    return;
                }
                if (player.Vehicle.HasData("ACCESS") && player.Vehicle.GetData<string>("ACCESS") == "FRACTION")
                {
                    if (!Fractions.Configs.FractionVehicles[player.Vehicle.GetData<int>("FRACTION")].ContainsKey(player.Vehicle.NumberPlate)) return;
                    int fractionid = player.Vehicle.GetData<int>("FRACTION");
                    if (cat < 0)
                    {
                        Core.VehicleManager.FracApplyCustomization(player.Vehicle, fractionid);
                        return;
                    }

                    string number = player.Vehicle.NumberPlate;
                    Tuple<string, Vector3, Vector3, int, int, int, VehicleManager.VehicleCustomization> oldtuple = Fractions.Configs.FractionVehicles[fractionid][number];
                    string oldname = oldtuple.Item1;
                    VehicleHash oldvehhash = (VehicleHash)NAPI.Util.GetHashKey(oldname);
                    Vector3 oldvehpos = oldtuple.Item2;
                    Vector3 oldvehrot = oldtuple.Item3;
                    int oldvehrank = oldtuple.Item4;
                    int oldvehc1 = oldtuple.Item5;
                    int oldvehc2 = oldtuple.Item6;
                    VehicleManager.VehicleCustomization oldvehdata = oldtuple.Item7;
                    switch (cat)
                    {
                        case 0:
                            oldvehdata.Spoiler = id;
                            break;
                        case 1:
                            oldvehdata.FrontBumper = id;
                            break;
                        case 2:
                            oldvehdata.RearBumper = id;
                            break;
                        case 3:
                            oldvehdata.SideSkirt = id;
                            break;
                        case 4:
                            oldvehdata.Muffler = id;
                            break;
                        case 5:
                            oldvehdata.Wings = id;
                            break;
                        case 6:
                            oldvehdata.Roof = id;
                            break;
                        case 7:
                            oldvehdata.Hood = id;
                            break;
                        case 8:
                            oldvehdata.Vinyls = id;
                            break;
                        case 9:
                            oldvehdata.Lattice = id;
                            break;
                        case 10:
                            oldvehdata.Engine = id;
                            break;
                        case 11:
                            oldvehdata.Turbo = id;
                            var turbo = (oldvehdata.Turbo == 0);
                            player.Vehicle.SetSharedData("TURBO", turbo);
                            break;
                        case 12:
                            oldvehdata.Horn = id;
                            break;
                        case 13:
                            oldvehdata.Transmission = id;
                            break;
                        case 14:
                            oldvehdata.WindowTint = id;
                            break;
                        case 15:
                            oldvehdata.Suspension = id;
                            break;
                        case 16:
                            oldvehdata.Brakes = id;
                            break;
                        case 17:
                            oldvehdata.Headlights = id;
                            break;
                        case 18:
                            oldvehdata.NumberPlate = id;
                            break;
                        case 19:
                            oldvehdata.NeonColor.Red = id;
                            break;
                        case 20:
                            oldvehdata.NeonColor.Green = id;
                            break;
                        case 21:
                            oldvehdata.NeonColor.Blue = id;
                            break;
                        case 22:
                            oldvehdata.NeonColor.Alpha = id;
                            break;
                        case 23:
                            oldvehdata.WheelsType = id;
                            break;
                        case 24:
                            oldvehdata.Wheels = id;
                            break;
                        case 25:
                            oldvehdata.WheelsColor = id;
                            break;
                    }
                    Fractions.Configs.FractionVehicles[fractionid][number] = new Tuple<string, Vector3, Vector3, int, int, int, VehicleManager.VehicleCustomization>(oldname, oldvehpos, oldvehrot, oldvehrank, oldvehc1, oldvehc2, oldvehdata);
                    MySqlCommand cmd = new MySqlCommand
                    {
                        CommandText = "UPDATE `fractionvehicles` SET `components`=@com WHERE `number`=@num"
                    };
                    cmd.Parameters.AddWithValue("@com", JsonConvert.SerializeObject(oldvehdata));
                    cmd.Parameters.AddWithValue("@num", player.Vehicle.NumberPlate);
                    MySQL.Query(cmd);
                    Core.VehicleManager.FracApplyCustomization(player.Vehicle, fractionid);
                    player.SendChatMessage("You have changed the tuning of this car for the faction.");
                }
                else player.SendChatMessage("You must be in the vehicle of the faction you wish to change.");
            }
            catch { }
        }

        [Command("newrentveh")] //Add car for rent (8 lvl)
        public static void newrentveh(Player player, string model, string number, int price, int c1, int c2)
        {
            try
            {
                if (!Group.CanUseCmd(player, "newrentveh")) return;
                VehicleHash vh = (VehicleHash)NAPI.Util.GetHashKey(model);
                if (vh == 0) throw null;
                var veh = NAPI.Vehicle.CreateVehicle(vh, player.Position, player.Rotation.Z, 0, 0);
                VehicleStreaming.SetEngineState(veh, true);
                veh.Dimension = player.Dimension;
                MySqlCommand cmd = new MySqlCommand
                {
                    CommandText = "INSERT INTO `othervehicles`(`type`, `number`, `model`, `position`, `rotation`, `color1`, `color2`, `price`) VALUES (@type, @number, @model, @pos, @rot, @c1, @c2, @price);"
                };
                cmd.Parameters.AddWithValue("@type", 0);
                cmd.Parameters.AddWithValue("@price", price);
                cmd.Parameters.AddWithValue("@model", model);
                cmd.Parameters.AddWithValue("@number", number);
                cmd.Parameters.AddWithValue("@c1", c1);
                cmd.Parameters.AddWithValue("@c2", c2);
                cmd.Parameters.AddWithValue("@pos", JsonConvert.SerializeObject(player.Position));
                cmd.Parameters.AddWithValue("@rot", JsonConvert.SerializeObject(player.Rotation));
                MySQL.Query(cmd);
                veh.PrimaryColor = c1;
                veh.SecondaryColor = c2;
                veh.NumberPlate = number;
                player.SendChatMessage("You have added a rental car.");
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"newrentveh\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("newjobveh")] // Добавить машину на работу (8 лвл)
        public static void newjobveh(Player player, string typejob, string model, string number, int c1, int c2)
        {
            try
            {
                if (!Group.CanUseCmd(player, "newjobveh")) return;
                VehicleHash vh = (VehicleHash)NAPI.Util.GetHashKey(model);
                if (vh == 0) return;
                int typeIdJob = -1;
                switch (typejob)
                {
                    case "Taxi":
                        typeIdJob = 3;
                        break;
                    case "Bus":
                        typeIdJob = 4;
                        break;
                    case "Lawnmower":
                        typeIdJob = 5;
                        break;
                    case "Collector":
                        typeIdJob = 7;
                        break;
                    case "AutoMechanic":
                        typeIdJob = 8;
                        break;
                    default:
                        player.SendChatMessage("Choose one type of work from: Taxi, Bus, Lawnmower, Collector, AutoMechanic");
                        break;
                }
                if (typeIdJob == -1) return;
                var veh = NAPI.Vehicle.CreateVehicle(vh, player.Position, player.Rotation.Z, 0, 0);
                VehicleStreaming.SetEngineState(veh, true);
                veh.Dimension = player.Dimension;
                MySqlCommand cmd = new MySqlCommand
                {
                    CommandText = "INSERT INTO `othervehicles`(`type`, `number`, `model`, `position`, `rotation`, `color1`, `color2`, `price`) VALUES (@type, @number, @model, @pos, @rot, @c1, @c2, '0');"
                };
                cmd.Parameters.AddWithValue("@type", typeIdJob);
                cmd.Parameters.AddWithValue("@model", model);
                cmd.Parameters.AddWithValue("@number", number);
                cmd.Parameters.AddWithValue("@c1", c1);
                cmd.Parameters.AddWithValue("@c2", c2);
                cmd.Parameters.AddWithValue("@pos", JsonConvert.SerializeObject(player.Position));
                cmd.Parameters.AddWithValue("@rot", JsonConvert.SerializeObject(player.Rotation));
                MySQL.Query(cmd);
                veh.PrimaryColor = c1;
                veh.SecondaryColor = c2;
                veh.NumberPlate = number;
                player.SendChatMessage("You have added a work machine.");
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"newjobveh\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("newfracveh")] // Add a car for the faction (8 lvl)
        public static void ACMD_newfracveh(Player player, string model, int fracid, string number, int c1, int c2) // add rank, number
        {
            try
            {
                if (!Group.CanUseCmd(player, "newfracveh")) return;
                VehicleHash vh = (VehicleHash)NAPI.Util.GetHashKey(model);
                if (vh == 0) throw null;
                var veh = NAPI.Vehicle.CreateVehicle(vh, player.Position, player.Rotation.Z, 0, 0);
                VehicleStreaming.SetEngineState(veh, true);
                veh.Dimension = player.Dimension;
                MySqlCommand cmd = new MySqlCommand
                {
                    CommandText = "INSERT INTO `fractionvehicles`(`fraction`, `number`, `components`, `model`, `position`, `rotation`, `rank`, `colorprim`, `colorsec`) VALUES (@idfrac, @number, '{}', @model, @pos, @rot, '1', @c1, @c2);"
                };
                cmd.Parameters.AddWithValue("@idfrac", fracid);
                cmd.Parameters.AddWithValue("@model", model);
                cmd.Parameters.AddWithValue("@number", number);
                cmd.Parameters.AddWithValue("@c1", c1);
                cmd.Parameters.AddWithValue("@c2", c2);
                cmd.Parameters.AddWithValue("@pos", JsonConvert.SerializeObject(player.Position));
                cmd.Parameters.AddWithValue("@rot", JsonConvert.SerializeObject(player.Rotation));
                MySQL.Query(cmd);
                veh.PrimaryColor = c1;
                veh.SecondaryColor = c2;
                veh.NumberPlate = number;
                VehicleManager.FracApplyCustomization(veh, fracid);
                player.SendChatMessage("You have added a faction vehicle.");
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"ACMD_newfracveh\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("setfracveh")]
        public static void ACMD_setfracveh(Player player, string vehname, int rank, int c1, int c2)
        {
            try
            {
                if (!Group.CanUseCmd(player, "setfracveh")) return;
                if (!player.IsInVehicle)
                {
                    player.SendChatMessage("You must be in the vehicle of the faction you wish to change.");
                    return;
                }
                if (rank <= 0 || c1 < 0 || c1 >= 160 || c2 < 0 || c2 >= 160) return;
                VehicleHash vh = (VehicleHash)NAPI.Util.GetHashKey(vehname);
                if (vh == 0) return;
                Vehicle vehicle = player.Vehicle;
                if (vehicle.HasData("ACCESS") && vehicle.GetData<string>("ACCESS") == "FRACTION")
                {
                    if (!Fractions.Configs.FractionVehicles[vehicle.GetData<int>("FRACTION")].ContainsKey(vehicle.NumberPlate)) return;

                    var canmats = (vh == VehicleHash.Barracks || vh == VehicleHash.Youga || vh == VehicleHash.Burrito3);
                    var candrugs = (vh == VehicleHash.Youga || vh == VehicleHash.Burrito3);
                    var canmeds = (vh == VehicleHash.Ambulance);
                    int fractionid = vehicle.GetData<int>("FRACTION");
                    NAPI.Data.SetEntityData(vehicle, "CANMATS", false);
                    NAPI.Data.SetEntityData(vehicle, "CANDRUGS", false);
                    NAPI.Data.SetEntityData(vehicle, "CANMEDKITS", false);
                    if (canmats) NAPI.Data.SetEntityData(vehicle, "CANMATS", true);
                    if (candrugs) NAPI.Data.SetEntityData(vehicle, "CANDRUGS", true);
                    if (canmeds) NAPI.Data.SetEntityData(vehicle, "CANMEDKITS", true);
                    NAPI.Data.SetEntityData(vehicle, "MINRANK", rank);
                    Vector3 pos = NAPI.Entity.GetEntityPosition(vehicle) + new Vector3(0, 0, 0.5);
                    Vector3 rot = NAPI.Entity.GetEntityRotation(vehicle);
                    VehicleManager.VehicleCustomization data = Fractions.Configs.FractionVehicles[fractionid][vehicle.NumberPlate].Item7;
                    VehicleHash vehhash = (VehicleHash)NAPI.Util.GetHashKey(Fractions.Configs.FractionVehicles[fractionid][vehicle.NumberPlate].Item1);
                    if (vehhash != vh) data = new VehicleManager.VehicleCustomization();
                    Fractions.Configs.FractionVehicles[fractionid][vehicle.NumberPlate] = new Tuple<string, Vector3, Vector3, int, int, int, VehicleManager.VehicleCustomization>(vehname, pos, rot, rank, c1, c2, data);
                    MySqlCommand cmd = new MySqlCommand
                    {
                        CommandText = "UPDATE `fractionvehicles` SET `model`=@mod,`position`=@pos,`rotation`=@rot,`rank`=@ra,`colorprim`=@col,`colorsec`=@sec,`components`=@com WHERE `number`=@num"
                    };
                    cmd.Parameters.AddWithValue("@mod", vehname);
                    cmd.Parameters.AddWithValue("@pos", JsonConvert.SerializeObject(pos));
                    cmd.Parameters.AddWithValue("@rot", JsonConvert.SerializeObject(rot));
                    cmd.Parameters.AddWithValue("@ra", rank);
                    cmd.Parameters.AddWithValue("@col", c1);
                    cmd.Parameters.AddWithValue("@sec", c2);
                    cmd.Parameters.AddWithValue("@com", JsonConvert.SerializeObject(data));
                    cmd.Parameters.AddWithValue("@num", vehicle.NumberPlate);
                    MySQL.Query(cmd);
                    vehicle.PrimaryColor = c1;
                    vehicle.SecondaryColor = c2;
                    NAPI.Entity.SetEntityModel(vehicle, (uint)vh);
                    VehicleManager.FracApplyCustomization(vehicle, fractionid);
                    player.SendChatMessage("You have changed the data of this vehicle for a faction..");
                }
                else player.SendChatMessage("You must be in the vehicle of the faction you wish to change.");
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"ACMD_setfracveh\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("payday")] // Complete PAYDAY (lvl 7)
        public static void payDay(Player player, string text = null)
        {
            if (!Group.CanUseCmd(player, "payday")) return;
            GameLog.Admin($"{player.Name}", $"payDay", "");
            Main.payDayTrigger();
        }
        
        [Command("giveitem")] // Give item (8 lvl)
        public static void CMD_giveItem(Player player, int id, int itemType, int amount, string data)   //        public static void CMD_giveItem(Client player, int id, int itemType, int amount, string data)
        {
            try
            {
                if (!Group.CanUseCmd(player, "giveitem")) return;
                var target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player with this ID was not found!", 3000);
                    return;
                }
                if (itemType == 12)
                {
                    int parsedData = 0;
                    int.TryParse(data, out parsedData);
                    if (parsedData > 100)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"you want too much", 3000);
                        return;
                    }
                }
                nInventory.Add(player, new nItem((ItemType)itemType, amount, data));
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You have {amount} {nInventory.ItemsNames[itemType]} ", 3000);
            }
            catch { }
        }
        
        [Command("setleader")] // Appoint a player as a leader (5 lvl)
        public static void CMD_setLeader(Player player, int id, int fracid)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player with this ID was not found", 3000);
                    return;
                }
                Admin.setFracLeader(player, Main.GetPlayerByID(id), fracid);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("sp")] // Follow the player (2 lvl)
        public static void CMD_spectateMode(Player player, int id)
        {
            if (!Group.CanUseCmd(player, "sp")) return;
            try
            {
                AdminSP.Spectate(player, id);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("usp")] //Finish following the player (2 lvl)
        public static void CMD_unspectateMode(Player player)
        {
            if (!Group.CanUseCmd(player, "sp")) return;
            try
            {
                AdminSP.UnSpectate(player);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("metp")] // Teleport player to you (lvl 2)
        public static void CMD_teleportToMe(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player with this ID was not found", 3000);
                    return;
                }
                Admin.teleportTargetToPlayer(player, Main.GetPlayerByID(id), false);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("gethere")] // Teleport the player to you along with the car (lvl 2)
        public static void CMD_teleportVehToMe(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player with this ID was not found", 3000);
                    return;
                }
                Admin.teleportTargetToPlayer(player, Main.GetPlayerByID(id), true);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("kill")] // Kill player (3 lvl)
        public static void CMD_kill(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player with this ID was not found", 3000);
                    return;
                }
                Admin.killTarget(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("hp")] // Give health to the player (2 lvl)
        public static void CMD_adminHeal(Player player, int id, int hp)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player with this ID was not found", 3000);
                    return;
                }
                Admin.healTarget(player, Main.GetPlayerByID(id), hp);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("ar")] // Выдать броню игроку (8 лвл)
        public static void CMD_adminArmor(Player player, int id, int ar)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.armorTarget(player, Main.GetPlayerByID(id), ar);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("fz")] // Freeze player (3 lvl)
        public static void CMD_adminFreeze(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player with this ID was not found", 3000);
                    return;
                }
                Admin.freezeTarget(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("ufz")] // Unfreeze player (3 lvl)
        public static void CMD_adminUnFreeze(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player with this ID was not found", 3000);
                    return;
                }
                Admin.unFreezeTarget(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("setadmin")] // Appoint a player as an admin (lvl 6)
        public static void CMD_setAdmin(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player with this ID was not found", 3000);
                    return;
                }
                Admin.setPlayerAdminGroup(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("lsn", GreedyArg = true)] // Admin announcement (8 lvl)
        public static void CMD_adminLSnewsChat(Player player, string message)
        {
            try
            {
                Admin.adminLSnews(player, message);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("tpcar")] //Teleport the player to himself (in the car) (2 lvl)
        public static void CMD_teleportToMeWithCar(Player player, int id)
        {
            try
            {
                Player Target = Main.GetPlayerByID(id);

                if (Target == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player with this ID was not found", 3000);
                    return;
                }

                if (!Target.IsInVehicle)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The player is not in the car", 3000);
                    return;
                }

                Admin.teleportTargetToPlayerWithCar(player, Target);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
        
        [Command("deladmin")] // Remove admin powers from a player (lvl 6)
        public static void CMD_delAdmin(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player with this ID was not found", 3000);
                    return;
                }
                Admin.delPlayerAdminGroup(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("setadminrank")] // Set admin rank (lvl 6)
        public static void CMD_setAdminRank(Player player, int id, int rank)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player with this ID was not found", 3000);
                    return;
                }
                Admin.setPlayerAdminRank(player, Main.GetPlayerByID(id), rank);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("guns")] // Give weapons to the player (gives any items) (8 lvl)
        public static void CMD_adminGuns(Player player, int id, string wname, string serial)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player with this ID was not found", 3000);
                    return;
                }
                Admin.giveTargetGun(player, Main.GetPlayerByID(id), wname, serial);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("giveclothes")] //Give clothes to the player (8 lvl)
        public static void CMD_adminClothes(Player player, int id, string wname, string serial)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player with this ID was not found", 3000);
                    return;
                }
                Admin.giveTargetClothes(player, Main.GetPlayerByID(id), wname, serial);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("setskin")] // Set skin to player (4 lvl)
        public static void CMD_adminSetSkin(Player player, int id, string pedModel)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player with this ID was not found", 3000);
                    return;
                }
                Admin.giveTargetSkin(player, Main.GetPlayerByID(id), pedModel);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("oguns")] // Take the weapon from the player (8 lvl)
        public static void CMD_adminOGuns(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player with this ID was not found", 3000);
                    return;
                }
                Admin.takeTargetGun(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("givemoney")] // Print money (8 lvl)
        public static void CMD_adminGiveMoney(Player player, int id, int money)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player with this ID was not found", 3000);
                    return;
                }
                Admin.giveMoney(player, Main.GetPlayerByID(id), money);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("delleader")] // Remove leader (lvl 6)
        public static void CMD_delleader(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player with this ID was not found", 3000);
                    return;
                }
                Admin.delFracLeader(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("deljob")] // Dismiss a player from work (lvl 6)
        public static void CMD_deljob(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player with this ID was not found", 3000);
                    return;
                }
                Admin.delJob(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("vehc")] //Create a car??? (4 lvl)
        public static void CMD_createVehicleCustom(Player player, string name, int r, int g, int b)
        {
            try
            {
                if (player == null || !Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "vehc")) return;
                VehicleHash vh = (VehicleHash)NAPI.Util.GetHashKey(name);
                if (vh == 0) throw null;
                var veh = NAPI.Vehicle.CreateVehicle(vh, player.Position, player.Rotation.Z, 0, 0);
                veh.Dimension = player.Dimension;
                veh.NumberPlate = "ADMIN";
                veh.CustomPrimaryColor = new Color(r, g, b);
                veh.CustomSecondaryColor = new Color(r, g, b);
                veh.SetData("ACCESS", "ADMIN");
                veh.SetData("BY", player.Name);
                VehicleStreaming.SetEngineState(veh, true);
                Log.Debug($"vehc {name} {r} {g} {b}");
                GameLog.Admin($"{player.Name}", $"vehCreate({name})", $"");
            }
            catch { }
        }
	

      [Command("veh")]
        public static void CMD_createVehicle(Player player, string name = "comet6", int a = 0, int b = 0, string number = "Shadow")
        {
            try
            {
                if (player == null || !Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "veh")) return;
                VehicleHash vh = (VehicleHash)NAPI.Util.GetHashKey(name);
                if (vh == 0)
                {
                    player.SendChatMessage("vh return");
                    return;
                }
                var veh = NAPI.Vehicle.CreateVehicle(vh, player.Position, player.Rotation.Z, 0, 0);
                veh.Dimension = player.Dimension;
                veh.PrimaryColor = a;
                veh.SecondaryColor = b;
                veh.NumberPlate = number;
                veh.Health = 1000;
                veh.SetData("ACCESS", "ADMIN");
                veh.SetData("BY", player.Name);
                VehicleStreaming.SetEngineState(veh, true);
                player.SetIntoVehicle(veh, 0);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD_veh\":\n" + e.ToString(), nLog.Type.Error); }
        }
		
        [Command("vehp")]
        public static void CMD_createVehicle(Player player, string name = "buffalo", string plate = "admin", int a = 0, int b = 0)
        {
            try
            {
                if (player == null || !Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "vehp")) return;
                VehicleHash vh = (VehicleHash)NAPI.Util.GetHashKey(name);
                player.SendChatMessage("vh " + vh);
                if (vh == 0)
                {
                    player.SendChatMessage("vh return");
                    return;
                }
                var veh = NAPI.Vehicle.CreateVehicle(vh, player.Position, player.Rotation.Z, 0, 0);
                veh.Dimension = player.Dimension;
                veh.NumberPlate = plate;
                veh.PrimaryColor = a;
                veh.SecondaryColor = b;
                veh.Health = 1000;
                veh.SetData("ACCESS", "ADMIN");
                veh.SetData("BY", player.Name);
                VehicleStreaming.SetEngineState(veh, true);
                GameLog.Admin($"{player.Name}", $"vehCreate({name})", $"");
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD_veh\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("vehhash")] //Create a car ??? (8 lvl)
        public static void CMD_createVehicleHash(Player player, string name, int a, int b)
        {
            try
            {
                if (player == null || !Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "vehhash")) return;
                var veh = NAPI.Vehicle.CreateVehicle(Convert.ToInt32(name, 16), player.Position, player.Rotation.Z, 0, 0);
                veh.Dimension = player.Dimension;
                veh.NumberPlate = "PROJECT";
                veh.PrimaryColor = a;
                veh.SecondaryColor = b;
                veh.SetData("ACCESS", "ADMIN");
                veh.SetData("BY", player.Name);
                VehicleStreaming.SetEngineState(veh, true);
            }
            catch { }
        }
        
        [Command("vehs")] // Create a car ??? (8 lvl)
        public static void CMD_createVehicles(Player player, string name, int a, int b, int count)
        {
            try
            {
                if (player == null || !Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "vehs")) return;
                VehicleHash vh = (VehicleHash)NAPI.Util.GetHashKey(name);
                if (vh == 0) throw null;
                for (int i = count; i > 0; i--)
                {
                    var veh = NAPI.Vehicle.CreateVehicle(vh, player.Position, player.Rotation.Z, 0, 0);
                    veh.Dimension = player.Dimension;
                    veh.NumberPlate = "ADMIN";
                    veh.PrimaryColor = a;
                    veh.SecondaryColor = b;
                    veh.SetData("ACCESS", "ADMIN");
                    veh.SetData("BY", player.Name);
                    VehicleStreaming.SetEngineState(veh, true);
                }
                GameLog.Admin($"{player.Name}", $"vehsCreate({name})", $"");
            }
            catch { }
        }
        
        [Command("vehcs")] // Create a car??? (8 lvl)
        public static void CMD_createVehicleCustoms(Player player, string name, int r, int g, int b, int count)
        {
            try
            {
                if (player == null || !Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "vehcs")) return;
                VehicleHash vh = (VehicleHash)NAPI.Util.GetHashKey(name);
                if (vh == 0) throw null;
                for (int i = count; i > 0; i--)
                {
                    var veh = NAPI.Vehicle.CreateVehicle(vh, player.Position, player.Rotation.Z, 0, 0);
                    veh.Dimension = player.Dimension;
                    veh.NumberPlate = "ADMIN";
                    veh.CustomPrimaryColor = new Color(r, g, b);
                    veh.CustomSecondaryColor = new Color(r, g, b);
                    veh.SetData("ACCESS", "ADMIN");
                    veh.SetData("BY", player.Name);
                    VehicleStreaming.SetEngineState(veh, true);
                    Log.Debug($"vehc {name} {r} {g} {b}");
                }
                GameLog.Admin($"{player.Name}", $"vehsCreate({name})", $"");
            }
            catch { }
        }
        
        [Command("vehcustompcolor")] // Custom painting on a car (8 lvl)
        public static void CMD_ApplyCustomPColor(Player client, int r, int g, int b, int mod = -1)
        {
            try
            {
                if (!Main.Players.ContainsKey(client)) return;
                if (!Group.CanUseCmd(client, "vehcustompcolor")) return;
                Color color = new Color(r, g, b);

                var number = client.Vehicle.NumberPlate;

                VehicleManager.Vehicles[number].Components.PrimColor = color;
                VehicleManager.Vehicles[number].Components.PrimModColor = mod;

                VehicleManager.ApplyCustomization(client.Vehicle);

            }
            catch { }
        }
        [Command("vehcustomscolor")] // Custom car paint (8 lvl)
        public static void CMD_ApplyCustomSColor(Player client, int r, int g, int b, int mod = -1)
        {
            try
            {
                if (!Main.Players.ContainsKey(client)) return;
                if (!Group.CanUseCmd(client, "vehcustomscolor")) return;
                Color color = new Color(r, g, b);

                var number = client.Vehicle.NumberPlate;

                VehicleManager.Vehicles[number].Components.SecColor = color;
                VehicleManager.Vehicles[number].Components.SecModColor = mod;

                VehicleManager.ApplyCustomization(client.Vehicle);

            }
            catch { }
        }
        [Command("vehsetcolor")]
        public static void CMD_SetColorToVehicle(Player player, int a, int b)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (!Group.CanUseCmd(player, "vehsetcolor")) return;
            var veh = player.Vehicle;
            if (veh == null) return;
            veh.PrimaryColor = a;
            veh.SecondaryColor = b;
        }

        [Command("aclear")] // Clear player account (8 lvl)
        public static void ACMD_aclear(Player player, string target)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "aclear")) return;
                if (!Main.PlayerNames.ContainsValue(target))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Player not found", 3000);
                    return;
                }
                if (NAPI.Player.GetPlayerFromName(target) != null)
                {
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Unable to clear a character that is in the game", 3000);
                    return;
                }
                string[] split = target.Split('_');
                int tuuid = 0;
                // CLEAR BIZ
                DataTable result = MySQL.QueryRead($"SELECT uuid,adminlvl,biz FROM `characters` WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                if (result != null && result.Rows.Count != 0)
                {
                    DataRow row = result.Rows[0];
                    if (Convert.ToInt32(row["adminlvl"]) >= Main.Players[player].AdminLVL)
                    {
                        SendToAdmins(3, $"!{{#d35400}}[ACLEAR-DENIED] {player.Name} ({player.Value}) tried to clear {target} (offline), who has a higher administrator level.");
                        return;
                    }
                    tuuid = Convert.ToInt32(row["uuid"]);
                    List<int> TBiz = JsonConvert.DeserializeObject<List<int>>(row["biz"].ToString());
                    if (TBiz.Count >= 1 && TBiz[0] >= 1)
                    {
                        var biz = BusinessManager.BizList[TBiz[0]];
                        var owner = biz.Owner;
                        var ownerplayer = NAPI.Player.GetPlayerFromName(owner);

                        if (ownerplayer != null && Main.Players.ContainsKey(player))
                        {
                            Notify.Send(ownerplayer, NotifyType.Warning, NotifyPosition.BottomCenter, $"The administrator took your business away from you", 3000);
                            MoneySystem.Wallet.Change(ownerplayer, Convert.ToInt32(biz.SellPrice * 0.8));
                            Main.Players[ownerplayer].BizIDs.Remove(biz.ID);
                        }
                        else
                        {
                            var split1 = biz.Owner.Split('_');
                            var data = MySQL.QueryRead($"SELECT biz,money FROM characters WHERE firstname='{split1[0]}' AND lastname='{split1[1]}'");
                            List<int> ownerBizs = new List<int>();
                            var money = 0;

                            foreach (DataRow Row in data.Rows)
                            {
                                ownerBizs = JsonConvert.DeserializeObject<List<int>>(Row["biz"].ToString());
                                money = Convert.ToInt32(Row["money"]);
                            }

                            ownerBizs.Remove(biz.ID);
                            MySQL.Query($"UPDATE characters SET biz='{JsonConvert.SerializeObject(ownerBizs)}',money={money + Convert.ToInt32(biz.SellPrice * 0.8)} WHERE firstname='{split1[0]}' AND lastname='{split1[1]}'");
                        }

                        MoneySystem.Bank.Accounts[biz.BankID].Balance = 0;
                        biz.Owner = "Государство";
                        biz.UpdateLabel();
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"You have taken the business from {owner}", 3000);
                    }
                }
                else
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Unable to find character in database", 3000);
                    return;
                }
                // CLEAR HOUSE
                result = MySQL.QueryRead($"SELECT id FROM `houses` WHERE `owner`='{target}'");
                if (result != null && result.Rows.Count != 0)
                {
                    DataRow row = result.Rows[0];
                    Houses.House house = Houses.HouseManager.Houses.FirstOrDefault(h => h.ID == Convert.ToInt32(row[0]));
                    if (house != null)
                    {
                        house.SetOwner(null);
                        house.UpdateLabel();
                        house.Save();
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"You took the house from {target}", 3000);
                    }
                }
                // CLEAR VEHICLES
                result = MySQL.QueryRead($"SELECT `number` FROM `vehicles` WHERE `holder`='{target}'");
                if (result != null && result.Rows.Count != 0)
                {
                    DataRowCollection rows = result.Rows;
                    foreach (DataRow row in rows)
                    {
                        VehicleManager.Remove(row[0].ToString());
                    }
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"you took from {target} all cars.", 3000);
                }

                // CLEAR MONEY, HOTEL, FRACTION, SIMCARD, PET
                MySQL.Query($"UPDATE `characters` SET `money`=0,`fraction`=0,`fractionlvl`=0,`hotel`=-1,`hotelleft`=0,`sim`=-1,`PetName`='null' WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                // CLEAR BANK MONEY
                Bank.Data bankAcc = Bank.Accounts.FirstOrDefault(a => a.Value.Holder == target).Value;
                if (bankAcc != null)
                {
                    bankAcc.Balance = 0;
                    MySQL.Query($"UPDATE `money` SET `balance`=0 WHERE `holder`='{target}'");
                }
                // CLEAR ITEMS
                if (tuuid != 0) MySQL.Query($"UPDATE `inventory` SET `items`='[]' WHERE `uuid`={tuuid}");
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You took all things from the player, money from the hands and bank account from {target}", 3000);
                GameLog.Admin($"{player.Name}", $"aClear", $"{target}");
            }
            catch (Exception e) { Log.Write("EXCEPTION AT aclear\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("findbyveh")] // Find a car by number (8 lvl)
        public static void CMD_FindByVeh(Player player, string number)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (!Group.CanUseCmd(player, "findbyveh")) return;
            if (number.Length > 8)
            {
                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "The number of characters in the license plate cannot exceed 8.", 3000);
                return;
            }
            if (VehicleManager.Vehicles.ContainsKey(number)) Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Number of the car: {number} | Model: {VehicleManager.Vehicles[number].Model} | Owner: {VehicleManager.Vehicles[number].Holder}", 6000);
            else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "No car found with this license plate.", 3000);
        }

        [Command("vehcustom")] // ??? (8 lbs)
        public static void CMD_ApplyCustom(Player client, int cat = -1, int id = -1)
        {
            try
            {
                if (!Main.Players.ContainsKey(client)) return;
                if (!Group.CanUseCmd(client, "vehcustom")) return;

                if (!client.IsInVehicle) return;

                if (cat < 0)
                {
                    Core.VehicleManager.ApplyCustomization(client.Vehicle);
                    return;
                }

                var number = client.Vehicle.NumberPlate;

                switch (cat)
                {
                    case 0:
                        VehicleManager.Vehicles[number].Components.Muffler = id;
                        break;
                    case 1:
                        VehicleManager.Vehicles[number].Components.SideSkirt = id;
                        break;
                    case 2:
                        VehicleManager.Vehicles[number].Components.Hood = id;
                        break;
                    case 3:
                        VehicleManager.Vehicles[number].Components.Spoiler = id;
                        break;
                    case 4:
                        VehicleManager.Vehicles[number].Components.Lattice = id;
                        break;
                    case 5:
                        VehicleManager.Vehicles[number].Components.Wings = id;
                        break;
                    case 6:
                        VehicleManager.Vehicles[number].Components.Roof = id;
                        break;
                    case 7:
                        VehicleManager.Vehicles[number].Components.Vinyls = id;
                        break;
                    case 8:
                        VehicleManager.Vehicles[number].Components.FrontBumper = id;
                        break;
                    case 9:
                        VehicleManager.Vehicles[number].Components.RearBumper = id;
                        break;
                    case 10:
                        VehicleManager.Vehicles[number].Components.Engine = id;
                        break;
                    case 11:
                        VehicleManager.Vehicles[number].Components.Turbo = id;
                        var turbo = (VehicleManager.Vehicles[number].Components.Turbo == 0);
                        client.Vehicle.SetSharedData("TURBO", turbo);
                        break;
                    case 12:
                        VehicleManager.Vehicles[number].Components.Horn = id;
                        break;
                    case 13:
                        VehicleManager.Vehicles[number].Components.Transmission = id;
                        break;
                    case 14:
                        VehicleManager.Vehicles[number].Components.WindowTint = id;
                        break;
                    case 15:
                        VehicleManager.Vehicles[number].Components.Suspension = id;
                        break;
                    case 16:
                        VehicleManager.Vehicles[number].Components.Brakes = id;
                        break;
                    case 17:
                        VehicleManager.Vehicles[number].Components.Headlights = id;
                        break;
                    case 18:
                        VehicleManager.Vehicles[number].Components.NumberPlate = id;
                        break;
                    case 19:
                        VehicleManager.Vehicles[number].Components.NeonColor.Red = id;
                        break;
                    case 20:
                        VehicleManager.Vehicles[number].Components.NeonColor.Green = id;
                        break;
                    case 21:
                        VehicleManager.Vehicles[number].Components.NeonColor.Blue = id;
                        break;
                    case 22:
                        VehicleManager.Vehicles[number].Components.NeonColor.Alpha = id;
                        break;
                    case 23:
                        VehicleManager.Vehicles[number].Components.WheelsType = id;
                        break;
                    case 24:
                        VehicleManager.Vehicles[number].Components.Wheels = id;
                        break;
                    case 25:
                        VehicleManager.Vehicles[number].Components.WheelsColor = id;
                        break;
                }

                Core.VehicleManager.ApplyCustomization(client.Vehicle);
            }
            catch { }
        }

        [Command("sw")] // Set weather (lvl 6)
        public static void CMD_setWeatherID(Player player, byte weather)
        {
            if (!Group.CanUseCmd(player, "sw")) return;
            Main.changeWeather(weather);
            GameLog.Admin($"{player.Name}", $"setWeather({weather})", $"");
        }

        [Command("st")] // Set time (8 lvl)
        public static void CMD_setTime(Player player, int hours, int minutes, int seconds)
        {
            if (!Group.CanUseCmd(player, "st")) return;
            NAPI.World.SetTime(hours, minutes, seconds);
        }
        
        [Command("tp")] // Teleport to the player (2 lvl)
        public static void CMD_teleport(Player player, int id)
        {
            try
            {
                if (!Group.CanUseCmd(player, "tp")) return;

                var target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player with this ID was not found", 3000);
                    return;
                }
                NAPI.Entity.SetEntityPosition(player, target.Position + new Vector3(1, 0, 1.5));
                NAPI.Entity.SetEntityDimension(player, NAPI.Entity.GetEntityDimension(target));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("goto")] // Teleport to the player along with the car (2 lvl)
        public static void CMD_teleportveh(Player player, int id)
        {
            try
            {
                if (!Group.CanUseCmd(player, "goto")) return;
                if (!player.IsInVehicle) return;
                var target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player with this ID was not found", 3000);
                    return;
                }
                NAPI.Entity.SetEntityDimension(player.Vehicle, NAPI.Entity.GetEntityDimension(target));
                NAPI.Entity.SetEntityPosition(player.Vehicle, target.Position + new Vector3(2, 2, 2));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("flip")] // Flip car (2 lvl)
        public static void CMD_flipveh(Player player, int id)
        {
            try
            {
                if (!Group.CanUseCmd(player, "flip")) return;
                Player target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player with this ID was not found", 3000);
                    return;
                }
                if (!target.IsInVehicle)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The player is not in the car", 3000);
                    return;
                }
                NAPI.Entity.SetEntityPosition(target.Vehicle, target.Vehicle.Position + new Vector3(0, 0, 2.5f));
                NAPI.Entity.SetEntityRotation(target.Vehicle, new Vector3(0, 0, target.Vehicle.Rotation.Z));
                GameLog.Admin($"{player.Name}", $"flipVeh", $"{target.Name}");
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("mtp")] // Очередной телепорт ??? (8 лвл)
        public static void CMD_maskTeleport(Player player, int id)
        {
            try
            {
                if (!Group.CanUseCmd(player, "mtp")) return;

                if (!Main.MaskIds.ContainsKey(id))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Mask with this ID was not found", 3000);
                    return;
                }
                var target = Main.MaskIds[id];

                NAPI.Entity.SetEntityPosition(player, target.Position);
                NAPI.Entity.SetEntityDimension(player, NAPI.Entity.GetEntityDimension(target));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("createbusiness")] // Создать бизнес (8 лвл)
        public static void CMD_createBiz(Player player, int govPrice, int type)
        {
            try
            {
                BusinessManager.createBusinessCommand(player, govPrice, type);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("createunloadpoint")] // Создать точку разгрузки у бизнеса (8 лвл)
        public static void CMD_createUnloadPoint(Player player, int bizid)
        {
            try
            {
                BusinessManager.createBusinessUnloadpoint(player, bizid);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("deletebusiness")] // Удалить бизнес (8 лвл)
        public static void CMD_deleteBiz(Player player, int bizid)
        {
            try
            {
                BusinessManager.deleteBusinessCommand(player, bizid);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("createsafe", GreedyArg = true)] // Создать сейф для ограблений (8 лвл)
        public static void CMD_createSafe(Player player, int id, float distance, int min, int max, string address)
        {
            try
            {
                SafeMain.CMD_CreateSafe(player, id, distance, min, max, address);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("removesafe")] // Удалить сейф для ограблений (8 лвл)
        public static void CMD_removeSafe(Player player)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    SafeMain.CMD_RemoveSafe(player);
                }
                catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
            });
        }
        
        [Command("createstock")] // Создать склад (8 лвл)
        public static void CMD_createStock(Player player, int frac, int drugs, int mats, int medkits, int money)
        {
            if (!Group.CanUseCmd(player, "createstock")) return;

            try
            {
                MySQL.Query($"INSERT INTO fractions (id,drugs,mats,medkits,money) VALUES ({frac},{drugs},{mats},{medkits},{money})");
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("demorgan", GreedyArg = true)] // Посадить игрока в деморган (2 лвл)
        public static void CMD_sendTargetToDemorgan(Player player, int id, int time, string reason)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.sendPlayerToDemorgan(player, Main.GetPlayerByID(id), time, reason);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("loadipl")] // Подгрузить всем игрокам IPL для карты (8 лвл)
        public static void CMD_LoadIPL(Player player, string ipl)
        {
            try
            {
                if (!Group.CanUseCmd(player, "loadipl")) return;
                NAPI.World.RequestIpl(ipl);
                player.SendChatMessage("Вы подгрузили IPL: " + ipl);
            }
            catch
            {
            }
        }
        
        [Command("unloadipl")] // Выгрузить у всех игроков IPL (8 лвл)
        public static void CMD_UnLoadIPL(Player player, string ipl)
        {
            try
            {
                if (!Group.CanUseCmd(player, "unloadipl")) return;
                NAPI.World.RemoveIpl(ipl);
                player.SendChatMessage("Вы выгрузили IPL: " + ipl);
            }
            catch
            {
            }
        }

        [Command("loadprop")] // Подгрузить себе PROP для карты (8 лвл)
        public static void CMD_LoadProp(Player player, double x, double y, double z, string prop)
        {
            try
            {
                if (!Group.CanUseCmd(player, "loadprop")) return;
                Trigger.ClientEvent(player, "loadProp", x, y, z, prop);
                player.SendChatMessage("Вы подгрузили Interior Prop: " + prop);
            }
            catch
            {
            }
        }
        
        [Command("unloadprop")]// Подгрузить у себя PROP (8 лвл)
        public static void CMD_UnLoadProp(Player player, double x, double y, double z, string prop)
        {
            try
            {
                if (!Group.CanUseCmd(player, "unloadprop")) return;
                Trigger.ClientEvent(player, "UnloadProp", x, y, z, prop);
                player.SendChatMessage("Вы выгрузили Interior Prop: " + prop);
            }
            catch
            {
            }
        }

        [Command("starteffect")] // Включить эффект ??? (8 лвл)
        public static void CMD_StartEffect(Player player, string effect, int dur = 0, bool loop = false)
        {
            try
            {
                if (!Group.CanUseCmd(player, "starteffect")) return;
                Trigger.ClientEvent(player, "startScreenEffect", effect, dur, loop);
                player.SendChatMessage("Вы включили Effect: " + effect);
            }
            catch
            {
            }
        }
        
        [Command("stopeffect")] // Выключить эффект ??? (8 лвл)
        public static void CMD_StopEffect(Player player, string effect)
        {
            try
            {
                if (!Group.CanUseCmd(player, "stopeffect")) return;
                Trigger.ClientEvent(player, "stopScreenEffect", effect);
                player.SendChatMessage("Вы выключили Effect: " + effect);
            }
            catch
            {
            }
        }
        
        [Command("udemorgan")] // Выпустить игрока из деморгана (8 лвл)
        public static void CMD_releaseTargetFromDemorgan(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.releasePlayerFromDemorgan(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("offjail", GreedyArg = true)] // Посадить игрока в оффлайне (3 лвл)
        public static void CMD_offlineJailTarget(Player player, string target, int time, string reason)
        {
            try
            {
                if (!Group.CanUseCmd(player, "offjail")) return;
                if (!Main.PlayerNames.ContainsValue(target))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Игрок не найден", 3000);
                    return;
                }
                if (player.Name.Equals(target)) return;
                if (NAPI.Player.GetPlayerFromName(target) != null)
                {
                    Admin.sendPlayerToDemorgan(player, NAPI.Player.GetPlayerFromName(target), time, reason);
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Игрок был онлайн, поэтому offjail заменён на demorgan", 3000);
                    return;
                }

                var firstTime = time * 60;
                var deTimeMsg = "м";
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

                var split = target.Split('_');
                MySQL.QueryRead($"UPDATE `characters` SET `demorgan`={firstTime},`arrest`=0 WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                NAPI.Chat.SendChatMessageToAll($"~r~{player.Name} посадил игрока {target} в спец. тюрьму на {time}{deTimeMsg} ({reason})");
                GameLog.Admin($"{player.Name}", $"demorgan({time}{deTimeMsg},{reason})", $"{target}");
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("offwarn", GreedyArg = true)] // Выдать игроку варн в оффлайне (3 лвл)
        public static void CMD_offlineWarnTarget(Player player, string target, int time, string reason)
        {
            try
            {
                if (!Group.CanUseCmd(player, "offwarn")) return;
                if (!Main.PlayerNames.ContainsValue(target))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок не найден", 3000);
                    return;
                }
                if (player.Name.Equals(target)) return;
                if (NAPI.Player.GetPlayerFromName(target) != null)
                {
                    Admin.warnPlayer(player, NAPI.Player.GetPlayerFromName(target), reason);
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Игрок был онлайн, поэтому offwarn был заменён на warn", 3000);
                    return;
                }
                else
                {
                    string[] split1 = target.Split('_');
                    DataTable result = MySQL.QueryRead($"SELECT adminlvl FROM characters WHERE firstname='{split1[0]}' AND lastname='{split1[1]}'");
                    DataRow row = result.Rows[0];
                    int targetadminlvl = Convert.ToInt32(row[0]);
                    if (targetadminlvl >= Main.Players[player].AdminLVL)
                    {
                        SendToAdmins(3, $"!{{#d35400}}[OFFWARN-DENIED] {player.Name} ({player.Value}) попытался забанить {target} (offline), который имеет выше уровень администратора.");
                        return;
                    }
                }


                var split = target.Split('_');
                var data = MySQL.QueryRead($"SELECT warns FROM characters WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                var warns = Convert.ToInt32(data.Rows[0]["warns"]);
                warns++;

                if (warns >= 3)
                {
                    MySQL.Query($"UPDATE `characters` SET `warns`=0 WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                    Ban.Offline(target, DateTime.Now.AddMinutes(43200), false, "Warns 3/3", "Server_Serverniy");
                }
                else
                    MySQL.Query($"UPDATE `characters` SET `unwarn`='{MySQL.ConvertTime(DateTime.Now.AddDays(14))}',`warns`={warns},`fraction`=0,`fractionlvl`=0 WHERE firstname='{split[0]}' AND lastname='{split[1]}'");

                NAPI.Chat.SendChatMessageToAll($"~r~{player.Name} выдал предупреждение игроку {target} ({warns}/3 | {reason})");
                GameLog.Admin($"{player.Name}", $"warn({time},{reason})", $"{target}");
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("ban", GreedyArg = true)] // Забанить игрока (3 лвл обычный бан) (8 лвл скрытый бан)
        public static void CMD_banTarget(Player player, int id, int time, string reason)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.banPlayer(player, Main.GetPlayerByID(id), time, reason, false);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("hardban", GreedyArg = true)] // Жестко забанить игрока (3 лвл)
        public static void CMD_hardbanTarget(Player player, int id, int time, string reason)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.hardbanPlayer(player, Main.GetPlayerByID(id), time, reason);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("offban", GreedyArg = true)] // Забанить игрока оффлайн (3 лвл)
        public static void CMD_offlineBanTarget(Player player, string name, int time, string reason)
        {
            try
            {
                if (!Main.PlayerNames.ContainsValue(name))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрока с таким именем не найдено", 3000);
                    return;
                }
                Admin.offBanPlayer(player, name, time, reason);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("offhardban", GreedyArg = true)] // Жестко забанить игрока оффлайн (3 лвл)
        public static void CMD_offlineHardbanTarget(Player player, string name, int time, string reason)
        {
            try
            {
                if (!Main.PlayerNames.ContainsValue(name))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрока с таким именем не найдено", 3000);
                    return;
                }
                Admin.offHardBanPlayer(player, name, time, reason);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("unban", GreedyArg = true)] // Разбанить игрока (3 лвл)
        public static void CMD_unbanTarget(Player player, string name)
        {
            if (!Group.CanUseCmd(player, "unban")) return;
            try
            {
                Admin.unbanPlayer(player, name);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("unhardban", GreedyArg = true)] // Снять хар-бан у игрока (3 лвл)
        public static void CMD_unhardbanTarget(Player player, string name)
        {
            if (!Group.CanUseCmd(player, "unhardban")) return;
            try
            {
                Admin.unhardbanPlayer(player, name);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("offgivereds")] // Выдать RedBucks в оффлайне (8 лвл)
        public static void CMD_offredbaks(Player client, string name, long amount)
        {
            if (!Group.CanUseCmd(client, "offgivereds")) return;
            try
            {
                name = name.ToLower();
                KeyValuePair<Player, nAccount.Account> acc = Main.Accounts.FirstOrDefault(x => x.Value.Login == name);
                if (acc.Value != null)
                {
                    Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок онлайн! {acc.Key.Name}:{acc.Key.Value}", 8000);
                    return;
                }
                MySQL.Query($"update `accounts` set `redbucks`=`redbucks`+{amount} where `login`='{name}'");
                GameLog.Admin(client.Name, $"offgivereds({amount})", name);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("mute", GreedyArg = true)] // Выдать игроку мут (2 лвл)
        public static void CMD_muteTarget(Player player, int id, int time, string reason)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.mutePlayer(player, Main.GetPlayerByID(id), time, reason);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("offmute", GreedyArg = true)] // Выдать игроку мут в оффлайне (2 лвл)
        public static void CMD_offlineMuteTarget(Player player, string target, int time, string reason)
        {
            try
            {
                if (!Main.PlayerNames.ContainsValue(target))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Игрок не найден", 3000);
                    return;
                }
                Admin.OffMutePlayer(player, target, time, reason);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("unmute")] // Снять мут у игрока (2 лвл)
        public static void CMD_muteTarget(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.unmutePlayer(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("vmute", GreedyArg = true)] // Замутить игрока в войс-чате (2 лвл)
        public static void CMD_voiceMuteTarget(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }

                if (!Group.CanUseCmd(player, "vmute")) return;
                player.SetSharedData("voice.muted", true);
                Trigger.ClientEvent(player, "voice.mute");
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("vunmute")] // Снять мут у игрока в войс-чате (2 лвл)
        public static void CMD_voiceUnMuteTarget(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }

                if (!Group.CanUseCmd(player, "vunmute")) return;
                player.SetSharedData("voice.muted", false);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("sban", GreedyArg = true)] // Скрыто забанить игрока (8 лвл)
        public static void CMD_silenceBan(Player player, int id, int time)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.banPlayer(player, Main.GetPlayerByID(id), time, "", true);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("kick", GreedyArg = true)] // Кикнуть игрока (2 лвл)
        public static void CMD_kick(Player player, int id, string reason)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.kickPlayer(player, Main.GetPlayerByID(id), reason, false);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("skick")] // Скрытно кикнуть игрока (4 лвл)
        public static void CMD_silenceKick(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.kickPlayer(player, Main.GetPlayerByID(id), "Silence kick", true);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("gm")] // Проверка на ГМ (2 лвл)
        public static void CMD_checkGamemode(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.checkGamemode(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("agm")] // Админское бессмертие (2 лвл)
        public static void CMD_enableGodmode(Player player)
        {
            try
            {
                if (!Group.CanUseCmd(player, "agm")) return;
                if (!player.HasData("AGM") || !player.GetData<bool>("AGM"))
                {
                    Trigger.ClientEvent(player, "AGM", true);
                    player.SetData("AGM", true);
                }
                else
                {
                    Trigger.ClientEvent(player, "AGM", false);
                    player.SetData("AGM", false);
                }
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("warn", GreedyArg = true)] // Выдать варн игроку (3 лвл)
        public static void CMD_warnTarget(Player player, int id, string reason)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.warnPlayer(player, Main.GetPlayerByID(id), reason);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("asms", GreedyArg = true)] // Админское сообщение игроку (1 лвл)
        public static void CMD_adminSMS(Player player, int id, string msg)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.adminSMS(player, Main.GetPlayerByID(id), msg);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("ans", GreedyArg = true)] // Ответ игроку (1 лвл)
        public static void CMD_answer(Player player, int id, string answer)
        {
            try
            {
                var sender = Main.GetPlayerByID(id);
                if (sender == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.answerReport(player, sender, answer);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("global", GreedyArg = true)] // Глобальный чат (4 лвл)
        public static void CMD_adminGlobalChat(Player player, string message)
        {
            try
            {
                Admin.adminGlobal(player, message);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("a", GreedyArg = true)] // Админский чат (1 лвл)
        public static void CMD_adminChat(Player player, string message)
        {
            try
            {
                Admin.adminChat(player, message);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("setvip")] // Выдать VIP игроку (6 лвл)
        public static void CMD_setVip(Player player, int id, int rank)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.setPlayerVipLvl(player, Main.GetPlayerByID(id), rank);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("checkmoney")] // Проверить деньги игрока (3 лвл)
        public static void CMD_checkMoney(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.checkMoney(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        #endregion

        [Command("findtrailer")]
        public static void CMD_findTrailer(Player player)
        {
            try
            {
                if (player.HasData("TRAILER"))
                {
                    Vehicle trailer = player.GetData<Vehicle>("TRAILER");
                    Trigger.ClientEvent(player, "createWaypoint", trailer.Position.X, trailer.Position.Y);
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы успешно установили маркер на карту, там находится Ваш трейлер", 5000);
                }
            }
            catch { }
        }



        #region VipCommands
        [Command("leave")]
        public static void CMD_leaveFraction(Player player)
        {
            try
            {
                if (Main.Accounts[player].VipLvl == 0) return;

                Fractions.Manager.UNLoad(player);

                int index = Fractions.Manager.AllMembers.FindIndex(m => m.Name == player.Name);
                if (index > -1) Fractions.Manager.AllMembers.RemoveAt(index);

                Main.Players[player].FractionID = 0;
                Main.Players[player].FractionLVL = 0;

                Customization.ApplyCharacter(player);
                if (player.HasData("HAND_MONEY")) player.SetClothes(5, 45, 0);
                else if (player.HasData("HEIST_DRILL")) player.SetClothes(5, 41, 0);
                player.SetData("ON_DUTY", false);
                NAPI.Player.RemoveAllPlayerWeapons(player);

                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"Вы покинули организацию", 3000);
                return;
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        #endregion

        [Command("ticket", GreedyArg = true)]
        public static void CMD_govTicket(Player player, int id, int sum, string reason)
        {
            try
            {
                var target = Main.GetPlayerByID(id);
                if (sum < 1) return;
                if (target == null || !Main.Players.ContainsKey(target))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                if (target.Position.DistanceTo(player.Position) > 2)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок слишком далеко", 3000);
                    return;
                }
                Fractions.FractionCommands.ticketToTarget(player, target, sum, reason);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("givemedlic")]
        public static void CMD_givemedlic(Player player, int id)
        {
            try
            {
                var target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                if (target.Position.DistanceTo(player.Position) > 2)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок слишком далеко", 3000);
                    return;
                }
                Fractions.FractionCommands.giveMedicalLic(player, target);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("givearmylic", "~g~/givearmylic [Айди]")]
        public static void CMD_GiveArmyLicense(Player player, int id)
        {
            try
            {
                if (!Manager.canUseCommand(player, "givearmylic")) return;
                var target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                if (player.Position.DistanceTo(target.Position) > 3)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок слишком далеко от Вас", 3000);
                    return;
                }
                if (Main.Players[target].Licenses[8])
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У игрока уже есть военный билет", 3000);
                    return;
                }
                FractionCommands.giveArmyLic(player, player);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("sellbiz")]
        public static void CMD_sellBiz(Player player, int id, int price)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                BusinessManager.sellBusinessCommand(player, Main.GetPlayerByID(id), price);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("password")]
        public static void CMD_ResetPassword(Player player, string new_password)
        {
            if (!Main.Players.ContainsKey(player)) return;
            Main.Accounts[player].changePassword(new_password);
            Notify.Send(player, NotifyType.Alert, NotifyPosition.BottomCenter, "Вы сменили пароль! Перезайдите с новым.", 3000);
        }

        [Command("time")]
        public static void CMD_checkPrisonTime(Player player)
        {
            try
            {
                if (Main.Players[player].ArrestTime != 0)
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вам осталось сидеть {Convert.ToInt32(Main.Players[player].ArrestTime / 60.0)} минут", 3000);
                else if (Main.Players[player].DemorganTime != 0)
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вам осталось сидеть {Convert.ToInt32(Main.Players[player].DemorganTime / 60.0)} минут", 3000);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("ptime")]
        public static void CMD_pcheckPrisonTime(Player player, int id)
        {
            try
            {
                if (!Group.CanUseCmd(player, "ptime")) return;
                Player target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                if (Main.Players[target].ArrestTime != 0)
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Игроку {target.Name} осталось сидеть {Convert.ToInt32(Main.Players[target].ArrestTime / 60.0)} минут", 3000);
                else if (Main.Players[target].DemorganTime != 0)
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Игроку {target.Name} осталось сидеть {Convert.ToInt32(Main.Players[target].DemorganTime / 60.0)} минут", 3000);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("sellcars")]
        public static void CMD_sellCars(Player player)
        {
            try
            {
                if (!Group.CanUseCmd(player, "sellcars")) return;
                Houses.HouseManager.OpenCarsSellMenu(player);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("dep", GreedyArg = true)]
        public static void CMD_govFracChat(Player player, string msg)
        {
            try
            {
                Fractions.Manager.govFractionChat(player, msg);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("gov", GreedyArg = true)]
        public static void CMD_gov(Player player, string msg)
        {
            try
            {
                if (!Fractions.Manager.canUseCommand(player, "gov")) return;
                int frac = Main.Players[player].FractionID;
                int lvl = Main.Players[player].FractionLVL;
                string[] split = player.Name.Split('_');

                NAPI.Chat.SendChatMessageToAll($"~y~[{Fractions.Manager.GovTags[frac]} | {split[0]} {split[1]}] {msg}");
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("call", GreedyArg = true)]
        public static void CMD_gov(Player player, int number, string msg)
        {
            try
            {
                if (number == 112)
                    Fractions.Police.callPolice(player, msg);
                else if (number == 911)
                    Fractions.Ems.callEms(player);
                else
                    player.SendChatMessage("Неправильный номер.");
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("report", GreedyArg = true)]
        public static void CMD_report(Player player, string message)
        {
            try
            {
                if (message.Length > 150)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Слишком длинное сообщение", 3000);
                    return;
                }
                if (Main.Accounts[player].VipLvl == 0 && player.HasData("NEXT_REPORT"))
                {
                    DateTime nextReport = player.GetData<DateTime>("NEXT_REPORT");
                    if (DateTime.Now < nextReport)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Попробуйте отправить жалобу через некоторое время", 3000);
                        return;
                    }
                }
                if (player.HasData("MUTE_TIMER"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Нельзя отправлять репорт во время Mute, дождитесь его окончания", 3000);
                    return;
                }
                ReportSys.AddReport(player, message);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

      

        [Command("ghc")]
        public static void CMD_teleportToMeCar(Player player, ushort vid)
        {
            try
            {
                foreach (var v in NAPI.Pools.GetAllVehicles())
                {
                    if (vid == v.Value)
                    {
                        NAPI.Entity.SetEntityPosition(v, player.Position);
                        NAPI.Entity.SetEntityRotation(v, player.Rotation);
                        NAPI.Entity.SetEntityDimension(v, player.Dimension);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

        [Command("revive")]
        public static void CMD_revive(Player client, int id)
        {
            try
            {
                if (!Group.CanUseCmd(client, "revive")) return;
                Player target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, "Игрок с таким ID не найден", 3000);
                    return;
                }
                target.PlayAnimation("rcmcollect_paperleadinout@", "kneeling_arrest_get_up", 33);
                NAPI.Entity.SetEntityPosition(target, target.Position + new Vector3(0, 0, 0.5));
                target.SetSharedData("InDeath", false);
                Trigger.ClientEvent(target, "DeathTimer", false);
                Trigger.ClientEvent(target, "screenFadeIn", 1000);
                target.Health = 100;
                Trigger.ClientEvent(target, "screenDeath");
                target.ResetData("IS_DYING");
                target.ResetSharedData("IS_DYING");
                Main.Players[target].IsAlive = true;
                Main.OffAntiAnim(target);
                if (target.HasData("DYING_TIMER"))
                {
                    Timers.Stop(target.GetData<string>("DYING_TIMER"));
                    target.ResetData("DYING_TIMER");
                }
                Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"Игрок ({client.Value}) реанимировал Вас", 3000);
                Notify.Send(client, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы реанимировали игрока ({target.Value})", 3000);

                if (target.HasData("CALLEMS_BLIP"))
                {
                    NAPI.Entity.DeleteEntity(target.GetData<Blip>("CALLEMS_BLIP"));
                }
                if (target.HasData("CALLEMS_COL"))
                {
                    NAPI.ColShape.DeleteColShape(target.GetData<ColShape>("CALLEMS_COL"));
                }
            }
            catch
            {

            }

        }
        [Command("takegunlic")]
        public static void CMD_takegunlic(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.takeGunLic(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("givegunlic")]
        public static void CMD_givegunlic(Player player, int id, int price)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.giveGunLic(player, Main.GetPlayerByID(id), price);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("pd")]
        public static void CMD_policeAccept(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.Police.acceptCall(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("inveh")] // Починить авто (3 лвл)
        public static void inveh(Player admin, ushort vid, int id, int pos)
        {
            try
            {
                if (!Group.CanUseCmd(admin, "inveh")) return;
                foreach (var v in NAPI.Pools.GetAllVehicles())
                {
                    if (Main.GetPlayerByID(id) == null)
                    {
                        Notify.Send(admin, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                        return;
                    }
                    Player player = Main.GetPlayerByID(id);
                    if (vid == v.Value)
                    {
                        player.SetIntoVehicle(v, pos);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"CMD_reapir\":" + e.ToString(), nLog.Type.Error);
            }
        }

        [Command("eject")]
        public static void CMD_ejectTarget(Player player, int id)
        {
            try
            {
                var target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                if (!player.IsInVehicle || player.VehicleSeat != 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не находитесь в машине или не на водительском месте", 3000);
                    return;
                }
                if (!target.IsInVehicle || player.Vehicle != target.Vehicle) return;
                VehicleManager.WarpPlayerOutOfVehicle(target);

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы выкинули игрока ({target.Value}) из машины", 3000);
                Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"Игрок ({player.Value}) выкинул Вас из машины", 3000);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("ems")]
        public static void CMD_emsAccept(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.Ems.acceptCall(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("pocket")]
        public static void CMD_pocketTarget(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                if (player.Position.DistanceTo(Main.GetPlayerByID(id).Position) > 2)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок слишком далеко", 3000);
                    return;
                }

                Fractions.FractionCommands.playerChangePocket(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
       /* [Command("buybiz")]
        public static void CMD_buyBiz(Player player)
        {
            try
            {
                if (player == null || !Main.Players.ContainsKey(player)) return;

                BusinessManager.buyBusinessCommand(player);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }*/

        [Command("setrank")]
        public static void CMD_setRank(Player player, int id, int newrank)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрока с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.SetFracRank(player, Main.GetPlayerByID(id), newrank);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("invite")]
        public static void CMD_inviteFrac(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.InviteToFraction(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("uninvite")]
        public static void CMD_uninviteFrac(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.UnInviteFromFraction(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("f", GreedyArg = true)]
        public static void CMD_fracChat(Player player, string msg)
        {
            try
            {
                Fractions.Manager.fractionChat(player, msg);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("fb", GreedyArg = true)]
        public static void CMD_fracOcChat(Player player, string msg)
        {
            try
            {
                Fractions.Manager.fractionOcChat(player, msg);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("arrest")]
        public static void CMD_arrest(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.arrestTarget(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("rfp")]
        public static void CMD_rfp(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.releasePlayerFromPrison(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("follow")]
        public static void CMD_follow(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.targetFollowPlayer(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("unfollow")]
        public static void CMD_unfollow(Player player)
        {
            try
            {
                Fractions.FractionCommands.targetUnFollowPlayer(player);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("su", GreedyArg = true)]
        public static void CMD_suByPassport(Player player, int pass, int stars, string reason)
        {
            try
            {
                Fractions.FractionCommands.suPlayer(player, pass, stars, reason);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("c")]
        public static void CMD_getCoords(Player player)
        {
            try
            {
                if (!Group.CanUseCmd(player, "c")) return;
                NAPI.Chat.SendChatMessageToPlayer(player, "Coords", NAPI.Entity.GetEntityPosition(player).ToString());
                NAPI.Chat.SendChatMessageToPlayer(player, "Rot", NAPI.Entity.GetEntityRotation(player).ToString());
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("incar")]
        public static void CMD_inCar(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.playerInCar(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("pull")]
        public static void CMD_pullOut(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.playerOutCar(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("warg")]
        public static void CMD_warg(Player player)
        {
            try
            {
                Fractions.FractionCommands.setWargPoliceMode(player);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("medkit")]
        public static void CMD_medkit(Player player, int id, int price)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.sellMedKitToTarget(player, Main.GetPlayerByID(id), price);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("accept")]
        public static void CMD_accept(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.acceptEMScall(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("heal")]
        public static void CMD_heal(Player player, int id, int price)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.healTarget(player, Main.GetPlayerByID(id), price);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("capture")]
        public static void CMD_capture(Player player)
        {
            try
            {
                Fractions.GangsCapture.CMD_startCapture(player);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("alpha2")]
        public static void alpha2(Player player, int vaule)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (!Group.CanUseCmd(player, "alpha2")) return;
            player.Transparency = vaule;
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы стали прозрачным на {vaule}", 3000);
            return;
        }
        [Command("delveh")]
        public static void CMD_deleteVidCar(Player player, ushort vid)
        {
            try
            {
                if (!Group.CanUseCmd(player, "delveh")) return;
                foreach (var v in NAPI.Pools.GetAllVehicles())
                {
                    if (vid == v.Value)
                    {
                        if (v.HasData("ACCESS") && v.GetData<string>("ACCESS") == "ADMIN")
                            v.Delete();
                    }
                }
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
        [Command("alpha")] // Невидимость
        public static void CMD_ToogleTranspancy(Player player)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (!Group.CanUseCmd(player, "alpha")) return;
            if (player.Transparency != 255)
            {
                player.Transparency = 255;
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы стали непрозрачным", 3000);
                return;
            }
            else
            {
                player.Transparency = 145;
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы стали прозрачным", 3000);
                return;
            }
        }
        [Command("repair")] // Починить авто (3 лвл)
        public static void repair(Player player, ushort vid)
        {
            try
            {
                if (!Group.CanUseCmd(player, "repair")) return;
                foreach (var v in NAPI.Pools.GetAllVehicles())
                {
                    if (vid == v.Value)
                    {
                        VehicleManager.RepairCar(v);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"CMD_reapir\":" + e.ToString(), nLog.Type.Error);
            }
        }

        /*[Command("frepair")] Работа автомеханика (добавить)
        public static void CMD_mechanicRepairFractions(Player player, int id, int price)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Person mit dieser ID nicht gefunden", 3000);
                    return;
                }
                Fractions.AutoMechanic.mechanicRepair(player, Main.GetPlayerByID(id), price);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }*/

        [Command("sellfuel")]
        public static void CMD_mechanicSellFuel(Player player, int id, int fuel, int pricePerLitr)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Jobs.AutoMechanic.mechanicFuel(player, Main.GetPlayerByID(id), fuel, pricePerLitr);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("buyfuel")]
        public static void CMD_mechanicBuyFuel(Player player, int fuel)
        {
            try
            {
                Jobs.AutoMechanic.buyFuel(player, fuel);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("ma")]
        public static void CMD_acceptMechanic(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Jobs.AutoMechanic.acceptMechanic(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("cmechanic")]
        public static void CMD_cancelMechanic(Player player)
        {
            try
            {
                Jobs.AutoMechanic.cancelMechanic(player);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("tprice")]
        public static void CMD_tprice(Player player, int id, int price)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Jobs.Taxi.offerTaxiPay(player, Main.GetPlayerByID(id), price);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("ta")]
        public static void CMD_taxiAccept(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Jobs.Taxi.acceptTaxi(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("ctaxi")]
        public static void CMD_cancelTaxi(Player player)
        {
            try
            {
                Jobs.Taxi.cancelTaxi(player);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("taxi")]
        public static void CMD_callTaxi(Player player)
        {
            try
            {
                Jobs.Taxi.callTaxi(player);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("me", GreedyArg = true)]
        public static async Task CMD_chatMe(Player player, string msg)
        {
            if (Main.Players[player].Unmute > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы замучены еще на {Main.Players[player].Unmute / 60} минут", 3000);
                return;
            }
            msg = RainbowExploit(player, msg);
            await RPChatAsync("me", player, msg);
        }

        [Command("do", GreedyArg = true)]
        public static async Task CMD_chatDo(Player player, string msg)
        {
            if (Main.Players[player].Unmute > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы замучены еще на {Main.Players[player].Unmute / 60} минут", 3000);
                return;
            }
            msg = RainbowExploit(player, msg);
            await RPChatAsync("do", player, msg);
        }

        [Command("todo", GreedyArg = true)]
        public static async Task CMD_chatToDo(Player player, string msg)
        {
            if (Main.Players[player].Unmute > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы замучены еще на {Main.Players[player].Unmute / 60} минут", 3000);
                return;
            }
            await RPChatAsync("todo", player, msg);
        }

        [Command("s", GreedyArg = true)]
        public static async Task CMD_chatS(Player player, string msg)
        {
            if (Main.Players[player].Unmute > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы замучены еще на {Main.Players[player].Unmute / 60} минут", 3000);
                return;
            }
            await RPChatAsync("s", player, msg);
        }

        [Command("b", GreedyArg = true)]
        public static async Task CMD_chatB(Player player, string msg)
        {
            if (Main.Players[player].Unmute > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы замучены еще на {Main.Players[player].Unmute / 60} минут", 3000);
                return;
            }
            await RPChatAsync("b", player, msg);
        }

        [Command("vh", GreedyArg = true)]
        public static async Task CMD_chatVh(Player player, string msg)
        {
            if (Main.Players[player].Unmute > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы замучены еще на {Main.Players[player].Unmute / 60} минут", 3000);
                return;
            }
            await RPChatAsync("vh", player, msg);
        }

        [Command("m", GreedyArg = true)]
        public static async Task CMD_chatM(Player player, string msg)
        {
            if (Main.Players[player].Unmute > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы замучены еще на {Main.Players[player].Unmute / 60} минут", 3000);
                return;
            }
            await RPChatAsync("m", player, msg);
        }

        [Command("t", GreedyArg = true)]
        public static async Task CMD_chatT(Player player, string msg)
        {
            if (Main.Players[player].Unmute > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы замучены еще на {Main.Players[player].Unmute / 60} минут", 3000);
                return;
            }
            await RPChatAsync("t", player, msg);
        }

        [Command("try", GreedyArg = true)]
        public static void CMD_chatTry(Player player, string msg)
        {
            if (Main.Players[player].Unmute > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы замучены еще на {Main.Players[player].Unmute / 60} минут", 3000);
                return;
            }
            Try(player, msg);
        }

        #region Try command handler
        private static void Try(Player sender, string message)
        {
            try
            {
                //Random
                int result = rnd.Next(5);
                Log.Debug("Random result: " + result.ToString());
                //
                switch (result)
                {
                    case 3:
                        foreach (Player player in Main.GetPlayersInRadiusOfPosition(sender.Position, 10f, sender.Dimension))
                            Trigger.ClientEvent(player, "sendRPMessage", "try", "!{#BF11B7}{name} " + message + " | !{#277C6B}" + " удачно", new int[] { sender.Value });
                        return;
                    default:
                        foreach (Player player in Main.GetPlayersInRadiusOfPosition(sender.Position, 10f, sender.Dimension))
                            Trigger.ClientEvent(player, "sendRPMessage", "try", "!{#BF11B7}{name} " + message + " | !{#FF0707}" + " неудачно", new int[] { sender.Value });
                        return;
                }
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        #endregion Try command handler

        #region RP Chat
        public static void RPChat(string cmd, Player sender, string message, Player target = null)
        {
            try
            {
                if (!Main.Players.ContainsKey(sender)) return;
                var names = new int[] { sender.Value };
                if (target != null) names = new int[] { sender.Value, target.Value };
                switch (cmd)
                {
                    case "me":
                        foreach (var player in Main.GetPlayersInRadiusOfPosition(sender.Position, 10f, sender.Dimension))
                            Trigger.ClientEvent(player, "sendRPMessage", "me", "!{#babaf8}{name} " + message, names);
                        return;
                    case "todo":
                        var args = message.Split('*');
                        var msg = args[0];
                        var action = args[1];
                        var genderCh = (Main.Players[sender].Gender) ? "" : "а";
                        foreach (var player in Main.GetPlayersInRadiusOfPosition(sender.Position, 10f, sender.Dimension))
                            Trigger.ClientEvent(player, "sendRPMessage", "todo", msg + ",!{#ffe666} - сказал" + genderCh + " {name}, " + action, names);
                        return;
                    case "do":
                        foreach (var player in Main.GetPlayersInRadiusOfPosition(sender.Position, 10f, sender.Dimension))
                            Trigger.ClientEvent(player, "sendRPMessage", "do", "!{#ffe666}" + message + " ({name})", names);
                        return;
                    case "s":
                        foreach (var player in Main.GetPlayersInRadiusOfPosition(sender.Position, 30f, sender.Dimension))
                            Trigger.ClientEvent(player, "sendRPMessage", "s", "{name} кричит: " + message, names);
                        return;
                    case "b":
                        foreach (var player in Main.GetPlayersInRadiusOfPosition(sender.Position, 10f, sender.Dimension))
                            Trigger.ClientEvent(player, "sendRPMessage", "b", "(( {name}: " + message + " ))", names);
                        return;
                    case "m":
                        if ((Main.Players[sender].FractionID != 7 && Main.Players[sender].FractionID != 9) || !NAPI.Player.IsPlayerInAnyVehicle(sender)) return;
                        var vehicle = sender.Vehicle;
                        if (vehicle.GetData<string>("ACCESS") != "FRACTION") return;
                        if (vehicle.GetData<int>("FRACTION") != 7 && vehicle.GetData<int>("FRACTION") != 9) return;
                        foreach (var player in Main.GetPlayersInRadiusOfPosition(sender.Position, 120f, sender.Dimension))
                            Trigger.ClientEvent(player, "sendRPMessage", "m", "~r~[Мегафон] {name}: " + message, names);
                        return;
                    case "t":
                        if (!Main.Players.ContainsKey(sender) || Main.Players[sender].WorkID != 6) return;
                        foreach (var p in Main.Players.Keys.ToList())
                        {
                            if (p == null || !Main.Players.ContainsKey(p)) continue;

                            if (Main.Players[p].WorkID == 6)
                            {
                                if (p.HasData("ON_WORK") && p.GetData<bool>("ON_WORK") && p.IsInVehicle)
                                    p.SendChatMessage($"~y~[Radio truckers] [{sender.Name}]: {message}");
                            }
                        }
                        return;
                }
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        public static Task RPChatAsync(string cmd, Player sender, string message)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (!Main.Players.ContainsKey(sender)) return;
                    var names = new int[] { sender.Value };
                    switch (cmd)
                    {
                        case "me":
                            foreach (var player in Main.GetPlayersInRadiusOfPosition(sender.Position, 10f, sender.Dimension))
                                Trigger.ClientEvent(player, "sendRPMessage", "me", "!{#E066FF}{name} " + message, names);
                            return;
                        case "todo":
                            var args = message.Split('*');
                            var msg = args[0];
                            var action = args[1];
                            var genderCh = Main.Players[sender].Gender ? "" : "а";
                            foreach (var player in Main.GetPlayersInRadiusOfPosition(sender.Position, 10f, sender.Dimension))
                                Trigger.ClientEvent(player, "sendRPMessage", "todo", msg + ",!{#E066FF} - сказал" + genderCh + " {name}, " + action, names);
                            return;
                        case "do":
                            foreach (var player in Main.GetPlayersInRadiusOfPosition(sender.Position, 10f, sender.Dimension))
                                Trigger.ClientEvent(player, "sendRPMessage", "do", "!{#E066FF}" + message + " ({name})", names);
                            return;
                        case "s":
                            foreach (var player in Main.GetPlayersInRadiusOfPosition(sender.Position, 30f, sender.Dimension))
                                Trigger.ClientEvent(player, "sendRPMessage", "s", "{name} кричит: " + message, names);
                            return;
                        case "b":
                            foreach (var player in Main.GetPlayersInRadiusOfPosition(sender.Position, 10f, sender.Dimension))
                                Trigger.ClientEvent(player, "sendRPMessage", "b", "(( {name}: " + message + " ))", names);
                            return;
                        case "m":
                            if (Main.Players[sender].FractionID != 7 && Main.Players[sender].FractionID != 9 || !NAPI.Player.IsPlayerInAnyVehicle(sender)) return;
                            var vehicle = sender.Vehicle;
                            if (vehicle.GetData<string>("ACCESS") != "FRACTION") return;
                            if (vehicle.GetData<int>("FRACTION") != 7 && vehicle.GetData<int>("FRACTION") != 9) return;
                            foreach (var player in Main.GetPlayersInRadiusOfPosition(sender.Position, 120f, sender.Dimension))
                                Trigger.ClientEvent(player, "sendRPMessage", "m", "!{#FF4D4D}[Мегафон] {name}: " + message, names);
                            return;
                        case "t":
                            if (!Main.Players.ContainsKey(sender) || Main.Players[sender].WorkID != 6) return;
                            foreach (var p in Main.Players.Keys.ToList())
                            {
                                if (p == null || !Main.Players.ContainsKey(p)) continue;

                                if (Main.Players[p].WorkID == 6)
                                {
                                    if (p.HasData("ON_WORK") && p.GetData<bool>("ON_WORK") && p.IsInVehicle)
                                        p.SendChatMessage($"~y~[Рация дальнобойщиков] [{sender.Name}]: {message}");
                                }
                            }
                            return;
                    }
                }
                catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
            });
            return Task.CompletedTask;
        }
        #endregion RP Chat
        [Command("roll")]
        public static void rollDice(Player player, int id, int money)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Человек с таким ID не найден", 3000);
                    return;
                }

                if (money <= 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Денежная ценность должна быть выше 0", 3000);
                    return;
                }
                if (Main.Players[player].Money < money)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You do not have enough money", 3000);
                    return;
                }


                // Send request for a game
                Player target = Main.GetPlayerByID(id);
                target.SetData("DICE_PLAYER", player);
                target.SetData("DICE_VALUE", money);
                if (player.Position.DistanceTo(target.Position) > 2)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы слишком далеко от игрока", 3000);
                    return;
                }
                if (Main.Players[target].Money < money)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У оппонента недостаточно средств", 3000);
                    return;
                }
                Trigger.ClientEvent(target, "openDialog", "DICE", $"Игрок {player.Name}({player.Value}) хочет сыграть с вами в бросок костей на {money}$. Вы принимаете?");

                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Игровой запрос на кости был отправлен на ({target.Value}) за {money}$.", 3000);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        public static int acceptDiceGame(Player playerTwo)
        {
            try
            {
                Player originPlayer = playerTwo.GetData<Player>("DICE_PLAYER");
                int money = playerTwo.GetData<int>("DICE_VALUE");

                if (money <= 0)
                {
                    Notify.Send(playerTwo, NotifyType.Error, NotifyPosition.BottomCenter, $"Денежная ценность должна быть выше 0", 3000);
                    Notify.Send(originPlayer, NotifyType.Error, NotifyPosition.BottomCenter, $"Денежная ценность должна быть выше 0", 3000);

                    return 0;
                }

                int playerOneResult = new Random().Next(1, 6);
                int playerTwoResult = new Random().Next(1, 6);

                while (playerOneResult == playerTwoResult)
                {
                    Notify.Send(playerTwo, NotifyType.Warning, NotifyPosition.BottomCenter, $"Кидаем кости снова, потому что у вас тот же кубик {playerTwoResult}, что и у противника", 3000);
                    Notify.Send(originPlayer, NotifyType.Warning, NotifyPosition.BottomCenter, $"Кидаем кости снова, потому что у вас тот же кубик {playerTwoResult}, что и у противника", 3000);

                    playerOneResult = new Random().Next(1, 6);
                    playerTwoResult = new Random().Next(1, 6);
                }

                playerTwo.SendChatMessage("!{#BF11B7}" + $"{playerTwo.Name}" + " на кубике выпало число: " + "!{#277C6B}" + $"{playerTwoResult}");
                originPlayer.SendChatMessage("!{#BF11B7}" + $"{originPlayer.Name}" + " на кубике выпало число: " + "!{#277C6B}" + $"{playerOneResult}");

                if (playerOneResult > playerTwoResult)
                {
                    Notify.Send(originPlayer, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы выиграли у соперника ${money}$", 3000);
                    MoneySystem.Wallet.Change(originPlayer, money);
                    MoneySystem.Wallet.Change(playerTwo, -money);
                    return 1;
                }
                else
                {
                    Notify.Send(playerTwo, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы выиграли у соперника ${money}$", 3000);
                    MoneySystem.Wallet.Change(originPlayer, -money);
                    MoneySystem.Wallet.Change(playerTwo, money);
                    return 2;
                }
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }

            return 0;
        }

        public static void rejectDiceGame(Player playerTwo)
        {
            Player originPlayer = playerTwo.GetData<Player>("DICE_PLAYER");

            Notify.Send(originPlayer, NotifyType.Warning, NotifyPosition.BottomCenter, $"Игрок (${playerTwo.Value}) отклонил игру", 3000);

            playerTwo.ResetData("DICE_PLAYER");
            playerTwo.ResetData("DICE_VALUE");
        }
    }
}