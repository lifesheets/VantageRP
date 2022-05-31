using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using GTANetworkAPI;
using GolemoSDK;

namespace Golemo.Fractions
{
    class KillersBeru : Script
    {
        private static nLog Log = new nLog("KillersBeru");
        private static Vector3 _entrancePosition = new Vector3(-1096.729, 4947.6616, 217.23425);
        private static Vector3 _exitPosition = new Vector3(892.1448, -3245.8206, -99.3863);
        private static Vector3 _exitPositionVeh = new Vector3(-1095.7719, 4947.4497, 218.27644);
        private static Vector3 ComputerBunker = new Vector3(891.6717, -3211.4407, -99.31772);
        private static Vector3 CreateContractPos = new Vector3(2193.4214, 5594.003, 52.639023);
        private static Vector3 StockPos = new Vector3(891.5632, -3227.1907, -99.352875);
        private static Player PlayerContract; 
        private static int TakedCont; 
        private static KillerContract TakedContracts; 
        private static List<KillerContract> Contracts = new List<KillerContract>() {};
        public class KillerContract
        {
            public string Name;
            public string Kill;
            public int Price;
        }
        private static void ContractsKillersSync()
        {
            Contracts.Clear();
            DataTable result = MySQL.QueryRead($"SELECT `name`,`kill`,`price` FROM `killerslist`");
            if (result == null || result.Rows.Count == 0) return;
            foreach (DataRow row in result.Rows)
            {
                Contracts.Add(new KillerContract()
                {
                    Name = Convert.ToString(row["name"]),
                    Kill = Convert.ToString(row["kill"]),
                    Price = Convert.ToInt32(row["price"]),
                });
            }
        }
        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                ContractsKillersSync();
                var colShapeEnter = NAPI.ColShape.CreateCylinderColShape(_entrancePosition, 1f, 2, 0);
                var colShapeExit = NAPI.ColShape.CreateCylinderColShape(_exitPosition, 2.2f, 2, 0);
                var TakeContract = NAPI.ColShape.CreateCylinderColShape(ComputerBunker, 1f, 2, 0);
                var CreateContract = NAPI.ColShape.CreateCylinderColShape(CreateContractPos, 1f, 2, 0);
                var StockPosition = NAPI.ColShape.CreateCylinderColShape(StockPos, 1f, 2, 0);

                NAPI.Marker.CreateMarker(1, _entrancePosition - new Vector3(0, 0, 0), new Vector3(), new Vector3(), 1f, new Color(13, 230, 70), false, 0);
                NAPI.Marker.CreateMarker(1, _exitPosition - new Vector3(0, 0, 1), new Vector3(), new Vector3(), 2.2f, new Color(13, 230, 70), false, 0);

                colShapeEnter.OnEntityEnterColShape += (s, e) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(e, "INTERACTIONCHECK", 700);
                        NAPI.Data.SetEntityData(e, "BUNKER_MAIN_SHAPE", "ENTER");
                    }
                    catch (Exception ex) { Log.Write("EnterCasino_OnEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
                };
                colShapeEnter.OnEntityExitColShape += OnEntityExitCasinoMainShape;

                colShapeExit.OnEntityEnterColShape += (s, e) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(e, "INTERACTIONCHECK", 700);
                        NAPI.Data.SetEntityData(e, "BUNKER_MAIN_SHAPE", "EXIT");
                    }
                    catch (Exception ex) { Log.Write("ExitCasino_OnEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
                };
                colShapeExit.OnEntityExitColShape += OnEntityExitCasinoMainShape;

                TakeContract.OnEntityEnterColShape += (s, e) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(e, "INTERACTIONCHECK", 960);
                        NAPI.Data.SetEntityData(e, "BUNKER_MAIN_SHAPE", "CONTRACT");
                    }
                    catch (Exception ex) { Log.Write("EnterCasino_OnEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
                };
                TakeContract.OnEntityExitColShape += OnEntityExitCasinoMainShape;

                CreateContract.OnEntityEnterColShape += (s, e) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(e, "INTERACTIONCHECK", 961);
                        NAPI.Data.SetEntityData(e, "BUNKER_MAIN_SHAPE", "CONTRACT");
                    }
                    catch (Exception ex) { Log.Write("EnterCasino_OnEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
                };
                CreateContract.OnEntityExitColShape += OnEntityExitCasinoMainShape;

                StockPosition.OnEntityEnterColShape += (s, e) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(e, "INTERACTIONCHECK", 962);
                        NAPI.Data.SetEntityData(e, "BUNKER_MAIN_SHAPE", "CONTRACT");
                    }
                    catch (Exception ex) { Log.Write("EnterCasino_OnEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
                };
                StockPosition.OnEntityExitColShape += OnEntityExitCasinoMainShape;
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }
        public static void OnEntityExitCasinoMainShape(ColShape shape, Player player)
        {
            NAPI.Data.SetEntityData(player, "INTERACTIONCHECK", 0);
            NAPI.Data.ResetEntityData(player, "BUNKER_MAIN_SHAPE");
        }
        public static void OpenStockKillersBuro(Player player)
        {
            if (Main.Players[player].FractionID != 17)
            {
                return;
            }
            if (!Stocks.fracStocks[17].IsOpen)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Склад закрыт", 3000);
                return;
            }
            player.SetData("ONFRACSTOCK", 17);
            GUI.Dashboard.OpenOut(player, Stocks.fracStocks[17].Weapons, "Склад оружия", 6);
            return;
        }
        public static void TakeKillerContract(Player player)
        {
            /*var rnd = new Random().Next(0, Main.AllPlayers.Count);
            var playerkill = Main.AllPlayers[rnd];
            var FName = Main.Players[playerkill].FirstName;
            Notify.Info(player, $"{FName}");*/
            if (Main.Players[player].FractionID != 17)
            {
                Notify.Error(player, "У вас нет доступа к компьютеру");
                return;
            }
            Trigger.ClientEvent(player, "client::killerscomputer", Newtonsoft.Json.JsonConvert.SerializeObject(Contracts));
        }
        [ServerEvent(Event.PlayerDeath)]
        public void onPlayerDeathHandler(Player player, Player entityKiller, uint weapon)
        {
            try
            {
                if (TakedContracts == null)
                {
                    return;
                }
                if (player == PlayerContract && Main.Players[entityKiller].FractionID == 17)
                {
                    foreach (var killers in API.Shared.GetAllPlayers())
                    {
                        if (Main.Players[killers].FractionID == 17)
                        {
                            var thiscontract = Contracts[TakedCont];
                            Notify.Succ(killers, $"Вы выполнили контракт на убийство {thiscontract.Kill.Replace("_"," ")}. Деньги были получены на счет фракции в размере {thiscontract.Price}$");
                            var data = Fractions.Stocks.fracStocks[17];
                            Trigger.ClientEvent(player, "BlipsHijacking", false, new Vector3());
                            data.Money += thiscontract.Price;
                            data.Materials += 5000;
                            Contracts.Remove(TakedContracts);
                            TakedCont = -1;
                            PlayerContract = null;
                            TakedContracts = null;
                            MySQL.Query($"DELETE FROM `killerslist` WHERE `name`='{thiscontract.Name}'");
                            ContractsKillersSync();
                        }
                    }
                } 
            }
            catch (Exception e) { Log.Write("PlayerDeath: " + e.Message, nLog.Type.Error); }
        }
        [ServerEvent(Event.PlayerDisconnected)]
        public void Event_OnPlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            try
            {
                if (TakedContracts == null)
                {
                    return;
                }
                if (player == PlayerContract)
                {
                    foreach (var killers in API.Shared.GetAllPlayers())
                    {
                        if (Main.Players[killers].FractionID == 17)
                        {
                            Notify.Succ(killers, $"Игрок, на котороко вы взяли контракт покинул игру. Возьмите новый контракт");
                            TakedCont = -1;
                            PlayerContract = null;
                            TakedContracts = null;
                        }
                    }
                }
            }
            catch (Exception e) { Log.Write($"PlayerDisconnected (value: {player.Value}): " + e.Message, nLog.Type.Error); }
        }
        [RemoteEvent("server::createcontractonkill")]
        public static void createcontractonkill(Player player, string name, string surname, int price)
        {
            DataTable surnameDB = MySQL.QueryRead($"SELECT * FROM `characters` WHERE `lastname`='{surname}' AND `firstname`='{name}'");
            if (surnameDB == null || surnameDB.Rows.Count == 0 || Main.Players[player].LastName == surname && Main.Players[player].FirstName == name)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок не найден", 5000);
                return;
            }
            if (Main.Players[player].Money < price)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Not enough money", 2000);
                return;
            }
            if (price < 1500000)
            {
                Notify.Error(player, "Слишком маленькая цена");
                return;
            }
            var killname = name + "_" + surname;
            MySQL.Query($"INSERT INTO `killerslist` (`name`,`kill`,`price`) VALUES ('{player.Name}','{killname}','{price}')");
            Notify.Succ(player, $"Вы заказали убийство на {name} {surname}. Вскором его выполнят");
            MoneySystem.Wallet.Change(player, -price);
            ContractsKillersSync();
        }
        [RemoteEvent("server::takecontractkillers")]
        public static void TakeContractKillers(Player player, int id)
        {
            var thiscont = Contracts[id];
            ContractsKillersSync();
            if (PlayerContract != null) 
            {
                Notify.Error(player, "У вас уже есть активный заказ на игрока. Метка на него установлена");
                Trigger.ClientEvent(player, "BlipsHijacking", true, PlayerContract.Position);
                return;
            }
            foreach (var target in API.Shared.GetAllPlayers())
            {
                var namekiller = Main.Players[target].FirstName + "_" + Main.Players[target].LastName;
                if (namekiller == thiscont.Kill)
                {
                    Notify.Succ(player, $"Вы взяли контракт #{id + 1}. Метка на цель установлена");
                    PlayerContract = target;
                    TakedCont = id;
                    TakedContracts = thiscont;
                    Trigger.ClientEvent(player, "BlipsHijacking", true, target.Position);
                    return;
                }
                else {
                    Notify.Error(player, $"Цель не в сети");
                }
            }
            Notify.Info(player, $"{thiscont.Name} {thiscont.Kill} {thiscont.Price}");
        }
        public static void CallBackShape(Player player)
        {
            if (!player.HasData("BUNKER_MAIN_SHAPE")) return;
            if (Main.Players[player].FractionID != 17) return;
            string data = player.GetData<string>("BUNKER_MAIN_SHAPE");
            if (data == "ENTER")
            {
                Trigger.ClientEvent(player, "showHUD", false);
                NAPI.Task.Run(() => {
                    try
                    {
                        if (player != null)
                        {
                            Trigger.ClientEvent(player, "client::createblip", 20, 567, "Крафт оружия", 4, new Vector3(905.77386, -3230.835, -99.41437), true);
                            Trigger.ClientEvent(player, "client::createblip", 21, 606, "Компьютер", 4, ComputerBunker, true);
                            Trigger.ClientEvent(player, "client::createblip", 22, 110, "Стрельбище", 4, Core.Poligon.startpoligon, true);
                            Trigger.ClientEvent(player, "screenFadeOut", 1000);
                            player.SetSharedData("PLAYER_IN_BUNKER", true);
                            player.SetData("PLAYER_IN_BUNKER", true);
                        }
                    }
                    catch { }
                }, 100);
                NAPI.Task.Run(() => {
                    try
                    {
                        if (player != null)
                        {
                            if (player.IsInVehicle)
                            {
                                NAPI.Entity.SetEntityPosition(player.Vehicle, _exitPosition);
                                NAPI.Entity.SetEntityRotation(player.Vehicle, new Vector3(0, 0, 270.5));
                                player.SetIntoVehicle(player.Vehicle, 0);
                                Trigger.ClientEvent(player, "screenFadeIn", 1000);
                                Trigger.ClientEvent(player, "showHUD", true);
                            }
                            else
                            {
                                NAPI.Entity.SetEntityPosition(player, _exitPosition + new Vector3(0, 0, 1.2));
                                NAPI.Entity.SetEntityRotation(player, new Vector3(0, 0, 270.5));
                                Trigger.ClientEvent(player, "screenFadeIn", 1000);
                                Trigger.ClientEvent(player, "showHUD", true);
                            }
                        }
                    }
                    catch { }
                }, 1600);
                return;
            }
            if (data == "EXIT")
            {
                Trigger.ClientEvent(player, "showHUD", false);
                if (player.IsInVehicle)
                {
                    NAPI.Task.Run(() =>
                    {
                        if (player != null)
                        {
                            Trigger.ClientEvent(player, "screenFadeOut", 1000);
                            Trigger.ClientEvent(player, "showHUD", false);
                            Trigger.ClientEvent(player, "client::destroyblips", 20);
                            Trigger.ClientEvent(player, "client::destroyblips", 21);
                            Trigger.ClientEvent(player, "client::destroyblips", 22);
                            player.SetSharedData("PLAYER_IN_BUNKER", false);
                            player.SetData("PLAYER_IN_BUNKER", false);
                        }
                    }, 100);
                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            if (player != null)
                            {
                                NAPI.Entity.SetEntityPosition(player.Vehicle, _exitPositionVeh);
                                NAPI.Entity.SetEntityRotation(player.Vehicle, new Vector3(0, 0, -110.5));
                                player.SetIntoVehicle(player.Vehicle, 0);
                                Trigger.ClientEvent(player, "screenFadeIn", 1000);
                                Trigger.ClientEvent(player, "showHUD", true);
                            }
                        }
                        catch { }
                    }, 1600);
                }
                else
                {
                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            if (player != null)
                            {
                                Trigger.ClientEvent(player, "screenFadeOut", 1000);
                                Trigger.ClientEvent(player, "showHUD", false);
                                Trigger.ClientEvent(player, "client::destroyblips", 20);
                                Trigger.ClientEvent(player, "client::destroyblips", 21);
                                Trigger.ClientEvent(player, "client::destroyblips", 22);
                                player.SetSharedData("PLAYER_IN_BUNKER", false);
                                player.SetData("PLAYER_IN_BUNKER", false);
                            }
                        }
                        catch { }
                    }, 100);
                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            if (player != null)
                            {
                                if (player.IsInVehicle)
                                {
                                    return;
                                }
                                else
                                {
                                    NAPI.Entity.SetEntityPosition(player, _entrancePosition + new Vector3(0, 0, 1.2));
                                    NAPI.Entity.SetEntityRotation(player, new Vector3(0, 0, -110.5));
                                    Trigger.ClientEvent(player, "screenFadeIn", 1000);
                                    Trigger.ClientEvent(player, "showHUD", true);
                                }
                            }
                        }
                        catch { }
                    }, 1600);

                }
            }
            }
    }
}
