using GTANetworkAPI;
using GolemoSDK;
using Golemo.Core;
using System;
using System.Collections.Generic;

namespace Golemo.Core
{
    class Hijacking : Script
    {
        public static Random rnd = new Random();
        private static Dictionary<int, ColShape> Cols = new Dictionary<int, ColShape>();
        private static Vehicle _veh;
        private static Marker _marker;
        private static string _number;
        private static ColShape _cols;
        private static nLog Log = new nLog("HIJACKING");
        private static Blip _blip;

        [RemoteEvent("StartHijackingOnBand")]
        public static void StartHijacking(Player player)
        {
            try
            {
                if (player.HasData("HijackingPlayer"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы уже взяли заказ на угон авто.", 3000);
                    return;
                }
                if (Fractions.Manager.FractionTypes[Main.Players[player].FractionID] == 2 || Main.Players[player].FractionID == 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You are not in CRIM. Organization.", 3000);
                    return;
                }
                var rand = rnd.Next(0, 36);
                var rand2 = rnd.Next(0, 4);
                var rand3 = rnd.Next(150, 200);
                var rand4 = rnd.Next(150, 200);
                var rand5 = rnd.Next(0, 8);
                var vehs = vehicles[rand2];
                _number = VehicleManager.GenerateNumber();
                var pos = SpawnPosition[rand];
                _veh = NAPI.Vehicle.CreateVehicle(vehs, pos, SpawnRotation[rand], Colors[rand5], Colors[rand5], _number, 255, false, false, 0);
                Trigger.ClientEvent(player, "BlipsHijacking", true, pos + new Vector3(rand3, rand4, rand2));
                _veh.SetData("Hijacking", true);
                VehicleStreaming.SetEngineState(_veh, false);
                player.SetData("HijackingPlayer", true);
                NAPI.Data.SetEntityData(player, "VEHICLE_ONEMSTIMER", 0);
                NAPI.Data.SetEntityData(player, "VEHICLE_ONEMS", Timers.StartTask(1000, () => timer_playerStartHikacking(player)));
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы взяли заказ на угон авто. Теперь едьте и найдите его.", 3000);
                player.SendChatMessage("Сейчас тебе придется угнать авто марки - !{#9898e6}" + $"{VehicleHandlers.VehiclesName.GetRealVehicleNameHash(model: vehs)}" + " ~w~с номером - !{#9898e6}" + $"{_number}.~w~ Цвет - {ColorsName[rand5]}.");
                return;
            }
            catch (Exception e) { Log.Write("StartHijacking : " + e.Message); }
        }
        private static void timer_playerStartHikacking(Player player)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (!player.HasData("VEHICLE_ONEMS")) return;
                    if (NAPI.Data.GetEntityData(player, "VEHICLE_ONEMSTIMER") > 1800)
                    {
                        if (player.HasData("HijackingPlayer"))
                        {
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы не успели сдать авто вовремя.", 3000);
                            if (_cols != null) _cols.Delete();
                            if (_veh != null) _veh.Delete();
                            if (_marker != null) _marker.Delete();
                            if (_blip != null) _blip.Delete();
                            Trigger.ClientEvent(player, "BlipsHijacking", false, new Vector3());
                            player.ResetData("HijackingPlayer");
                            player.ResetData("VEHICLE_ONEMSTIMER");
                            player.ResetData("VEHICLE_ONEMS");
                            return;
                        }
                    }
                    NAPI.Data.SetEntityData(player, "VEHICLE_ONEMSTIMER", NAPI.Data.GetEntityData(player, "VEHICLE_ONEMSTIMER") + 1);
                }
                catch (Exception e) { Log.Write("exitVehicleTimer: " + e.Message); }
            });
        }
        [ServerEvent(Event.PlayerDisconnected)]
        public void Event_OnPlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            try
            {
                if (player.HasData("HijackingPlayer"))
                {
                    NAPI.Task.Run(() =>
                    {
                        if (_cols != null) _cols.Delete();
                        if (_veh != null) _veh.Delete();
                        if (_marker != null) _marker.Delete();
                        if (_blip != null) _blip.Delete();
                        player.ResetData("HijackingPlayer");
                        Trigger.ClientEvent(player, "BlipsHijacking", false, new Vector3());
                    });
                }
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }
        [ServerEvent(Event.PlayerExitVehicle)]
        public void Event_onPlayerExitVehicleHandler(Player player, Vehicle vehicle)
        {
            try
            {
                if (vehicle.HasData("Hijacking"))
                {
                    if (player.HasData("HijackingPlayer"))
                    {
                        if (_cols != null) _cols.Delete();
                        if (_marker != null) _marker.Delete();
                        if (_blip != null) _blip.Delete();
                        Trigger.ClientEvent(player, "BlipsHijacking", false, new Vector3());
                    }
                }
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }
        [ServerEvent(Event.PlayerEnterVehicle)]
        public static void OnPlayerEnterVehicleHandler(Player player, Vehicle vehicle, sbyte seatid)
        {
            try
            {
                if (vehicle.HasData("Hijacking"))
                {
                    if (Fractions.Manager.FractionTypes[Main.Players[player].FractionID] == 0 || Fractions.Manager.FractionTypes[Main.Players[player].FractionID] == 1)
                    {
                        var rand = rnd.Next(0, 3);
                        var pos = new Vector3(1540.4772, 6336.3423, 22.955172);
                        _cols = NAPI.ColShape.CreateCylinderColShape(pos, 7f, 3f, 0);
                        _marker = NAPI.Marker.CreateMarker(1, pos, new Vector3(), new Vector3(), 5f, new Color(163, 131, 188));
                        _cols.OnEntityEnterColShape += EnterCheckpoint;
                        Trigger.ClientEvent(player, "createWaypoint", pos.X, pos.Y);
                        Trigger.ClientEvent(player, "BlipsHijacking", false, new Vector3());
                    }
                    foreach (var p in NAPI.Pools.GetAllPlayers())
                    {
                        if (Main.Players[p].FractionID == 7 || Main.Players[p].FractionID == 9 || Main.Players[p].FractionID == 14 || Main.Players[p].FractionID == 18)
                        {
                            p.SendChatMessage($"~g~[Фракция] Только что было угнано авто - {VehicleHandlers.VehiclesName.GetRealVehicleName(_veh.DisplayName)}. C регистрационным номером - {_number}. Цвет - черый.");
                            NAPI.Notification.SendNotificationToPlayer(p, "Поступил угон авто.");
                            _blip = NAPI.Blip.CreateBlip(468, player.Vehicle.Position, 1, 38, "Угон авто", 255, 0, true, 0, 0);
                        }
                    }
                }
            }
            catch (Exception e) { Log.Write("PlayerEnterVehicle: " + e.Message, nLog.Type.Error); }
        }
        [RemoteEvent("stophijackingengine")]
        public static void HijackingEngine(Player player)
        {
            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас не получилось завести машину", 3000);
            player.SetIntoVehicle(player.Vehicle, 0);
            nInventory.Remove(player, ItemType.Programmer, 1);
            return;
        }
        [RemoteEvent("succeshijackingengine")]
        public static void HijackingEngineSucces(Player player)
        {
            VehicleStreaming.SetEngineState(player.Vehicle, true);
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Доставьте автомобиль в гараж отмеченный на GPS", 3000);
            player.SetIntoVehicle(player.Vehicle, 0);
            //GUI.Dashboard.Close(player);
            return;
        }
        public static void EnterCheckpoint(ColShape shape, Player player)
        {
            try
            {
                if (!player.IsInVehicle) return;
                if (player.IsInVehicle && player.Vehicle.HasData("Hijacking"))
                {
                    NAPI.Task.Run(() => {
                        try
                        {
                            if (player != null)
                            {
                                Trigger.ClientEvent(player, "showHUD", false);
                                Trigger.ClientEvent(player, "screenFadeOut", 1000);
                            }
                        }
                        catch { }
                    }, 1000);
                    NAPI.Task.Run(() => {
                        try
                        {
                            if (player != null)
                            {
                                Trigger.ClientEvent(player, "screenFadeIn", 1000);
                                Trigger.ClientEvent(player, "camerhijacking");
                                Trigger.ClientEvent(player, "OpenMenuHij");
                            }
                        }
                        catch { }
                    }, 2000);
                    NAPI.Task.Run(() => {
                        try
                        {
                            if (player != null)
                            {
                                Trigger.ClientEvent(player, "exitcamerahij");
                                Trigger.ClientEvent(player, "CloseMenuHij");
                                player.Vehicle.Delete();
                                var count = 5000 * rnd.Next(0, 12) + rnd.Next(100, 400);
                                MoneySystem.Wallet.Change(player, +count);
                                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы успешно довезли авто и получили {count}$.", 3000);
                                VehicleManager.WarpPlayerOutOfVehicle(player);
                                Trigger.ClientEvent(player, "BlipsHijacking", false, new Vector3());
                                player.ResetData("HijackingPlayer");
                                if (_cols != null) _cols.Delete();
                                if (_marker != null) _marker.Delete();
                                if (_blip != null) _blip.Delete();
                            }
                        }
                        catch { }
                    }, 10000);
                }
                return;
            }
            catch (Exception e) { Log.Write("EnterCheckpoint: " + e.Message, nLog.Type.Error); }
        }
        #region Lists
        private static List<VehicleHash> vehicles = new List<VehicleHash>()
        {
             (VehicleHash)NAPI.Util.GetHashKey("a6"),
             (VehicleHash)NAPI.Util.GetHashKey("a45"),
             (VehicleHash)NAPI.Util.GetHashKey("c32"),
             (VehicleHash)NAPI.Util.GetHashKey("s600w220"),
        };
        private static List<int> Colors = new List<int>()
        {
            0,
            134,
            44,
            125,
            70,
            123,
            20,
            89,
        };
        private static List<string> ColorsName = new List<string>()
        {
            "Черный",
            "Белый",
            "Красный",
            "Зеленый",
            "Голубой",
            "Оранжевый",
            "Серый",
            "Желтый",
        };
        private static List<Vector3> SpawnPosition = new List<Vector3>()
        {
             new Vector3(286.05154, 150.3633, 104.24346),
             new Vector3(330.34653, 163.18925, 103.52409),
             new Vector3(232.6546, 116.872345, 102.778946),
             new Vector3(220.87799, 174.18686, 105.44652),
             new Vector3(91.448, 183.54625, 104.74335),
             new Vector3(371.45947, 266.818, 103.20544),
             new Vector3(394.11493, 111.21666, 102.04925),
             new Vector3(453.72995, 118.20008, 99.1958),
             new Vector3(1143.2651, -478.33295, 66.345535),
             new Vector3(1162.377, -464.15692, 66.8199),
             new Vector3(1133.3479, -423.77744, 66.668236),
             new Vector3(1135.971, -403.5186, 67.22695),
             new Vector3(1145.1654, -392.61932, 67.29094),
             new Vector3(1060.662, -775.9393, 58.4231),
             new Vector3(1016.9659, -760.40546, 58.146816),
             new Vector3(-1180.8954, -368.5742, 36.74158),
             new Vector3(-1255.1271, -389.60645, 37.46727),
             new Vector3(-1254.5638, -389.105, 42.97005),
             new Vector3(-1212.7767, -376.84772, 48.46921),
             new Vector3(-1255.7703, -385.4172, 48.469948),
             new Vector3(-1230.3757, -386.29276, 53.969997),
             new Vector3(-1223.2935, -382.49347, 59.46494),
             new Vector3(-1201.4923, -361.35278, 59.464905),
             new Vector3(-1376.3784, -449.17993, 34.65544),
             new Vector3(-1389.9889, -483.21893, 31.770359),
             new Vector3(-1521.7712, -419.1343, 35.619625),
             new Vector3(-1528.6445, -426.84088, 35.619717),
             new Vector3(-1455.7185, -385.45804, 38.500748),
             new Vector3(-1277.4507, -1356.7175, 4.482589),
             new Vector3(-1274.2908, -1364.6027, 4.4804626),
             new Vector3(-1182.4229, -1483.8151, 4.5577383),
             new Vector3(-1186.6853, -1490.6162, 4.5573874),
             new Vector3(-1190.4403, -1503.9343, 4.5510426),
             new Vector3(-1129.54, -1609.3094, 4.576397),
             new Vector3(-1125.4401, -1614.0461, 4.58424),
             new Vector3(-1108.3134, -1633.2437, 4.7936516),
        };
        private static List<Vector3> SpawnRotation = new List<Vector3>()
        {
            new Vector3(-0.7785859, 0.010205129, -110.00212),
            new Vector3(1.3986064, 0.05890426, 70.33218),
            new Vector3(0.28896704, -0.08269749, -20.097967),
            new Vector3(-0.670204, -0.24243982, -114.55817),
            new Vector3(2.581168, -0.81041247, 160.24596),
            new Vector3(0.6689477, 0.28677288, -20.07758),
            new Vector3(-2.8246896, -0.0073503503, -110.85293),
            new Vector3(3.2780187, -0.093480885, 70.418915),
            new Vector3(0.17576467, 4.322993, -105.39578),
            new Vector3(-0.3770248, -2.3164847, 166.11703),
            new Vector3(0.2723467, 0.0010786763, -12.972105),
            new Vector3(0.22374266, -0.009463847, -81.37421),
            new Vector3(-1.681152, -0.6623257, 56.484688),
            new Vector3(-0.46088597, 0.04107991, -4.3239517),
            new Vector3(0.3122588, 1.3114654, -137.47433),
            new Vector3(-3.0044188, -0.67643714, 94.60416),
            new Vector3(0.22024153, 0.013342908, -59.59641),
            new Vector3(0.21109882, 0.017326994, -59.8096),
            new Vector3(0.2917024, 0.00070767157, 30.018764),
            new Vector3(0.251202, 0.022468926, -60.387352),
            new Vector3(0.3121692, 0.0011496327, 29.630611),
            new Vector3(0.2957245, 0.0020774712, 28.633995),
            new Vector3(0.28412458, 0.0059589054, 91.791626),
            new Vector3(0.19809473, -0.0010329528, -142.94626),
            new Vector3(0.2129345, -0.0013305064, 8.473756),
            new Vector3(0.272394, -0.017957807, -132.0792),
            new Vector3(0.26339287, 0.019783981, -127.95651),
            new Vector3(0.15271392, 6.2903, -55.42914),
            new Vector3(0.24015626, 0.0146592725, -70.41437),
            new Vector3(0.26142162, 0.004809133, -72.67287),
            new Vector3(0.20912634, 0.017892322, 123.368034),
            new Vector3(0.24573226, 0.012875013, 124.844635),
            new Vector3(0.4428138, -0.012769597, -53.71847),
            new Vector3(0.2664922, 0.022358516, -54.408436),
            new Vector3(-0.07327878, 0.520644, -53.451378),
            new Vector3(-1108.3134, -1633.2437, 4.7936516),
        };
        #endregion
    }
}