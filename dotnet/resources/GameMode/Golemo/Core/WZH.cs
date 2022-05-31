using GTANetworkAPI;
using GolemoSDK;
using Golemo.Core;
using System;
using System.Collections.Generic;

namespace Golemo.Randomchick
{
    class WZH : Script
    {
        private static nLog RLog = new nLog("WZH");
        public static Random rnd = new Random();
        public static bool _isStart = false;
        public static Vehicle _veh;
        public static Vehicle _veh1;
        private static Blip blip;
        private static TextLabel _text;
        private static Marker _marker;
        private static ColShape _cols;
        [ServerEvent(Event.ResourceStart)]
        public static void OnResourceStart()
        { }
        #region Spawn
        private static List<Vector3> SpawnPosition = new List<Vector3>()
        {
             new Vector3(2863.3303, 1515.699, 23.44753),
             new Vector3(2743.5884, 1348.1758, 23.403996),
             new Vector3(2652.389, 1437.0834, 23.401022),
             new Vector3(2702.9004, 1569.5549, 23.377705),
             new Vector3(2879.1755, 1568.9574, 23.44749),
             new Vector3(2672.5564, 1697.2798, 23.36822),
             new Vector3(2858.5515, 1444.4585, 23.44753)
        };
        private static List<Vector3> SpawnRotation = new List<Vector3>()
        {
             new Vector3(0, 0, 71.67744),
             new Vector3(0, 0, -178.2181),
             new Vector3(0, 0, -8.364768),
             new Vector3(0, 0, -10.123747),
             new Vector3(0, 0, 70.17627),
             new Vector3(0, 0, 90.13812),
             new Vector3(0, 0, -17.870136)
        };
        #endregion
        public static void SpawnVehicles()
        {
            try
            {
                var rand = rnd.Next(0, 3);
                _veh = NAPI.Vehicle.CreateVehicle(VehicleHash.Speedo4, SpawnPosition[rand], SpawnRotation[rand], 0, 0, "WZH", 255, false, false, 0);
                _veh.SetData("WZH", true);
                var rand1 = rnd.Next(3, 6);
                _veh1 = NAPI.Vehicle.CreateVehicle(VehicleHash.Speedo4, SpawnPosition[rand1], SpawnRotation[rand1], 0, 0, "WZH", 255, false, false, 0);
                _veh1.SetData("WZH", true);
                blip = NAPI.Blip.CreateBlip(587, new Vector3(2689.7925, 1642.409, 29.722664), 0.8f, 1, "Война за хамеры", shortRange: true);
                foreach (var p in NAPI.Pools.GetAllPlayers())
                    if (Fractions.Manager.FractionTypes[Main.Players[p].FractionID] == 0 || Fractions.Manager.FractionTypes[Main.Players[p].FractionID] == 1)
                    {
                        p.SendChatMessage("Началась война за хамеры.");
                        NAPI.Notification.SendNotificationToPlayer(p, "Началась война за хамеры.");
                    }
                _isStart = true;
            }
            catch (Exception e) { RLog.Write("SpawnVehicles: " + e.Message, nLog.Type.Error); }
        }
        #region CheckFrac
        private static Dictionary<int, Vector3> Markers = new Dictionary<int, Vector3>()
        {
            {1, new Vector3(-238.68332, -1587.6147, 32.621796)},
            {2, new Vector3(102.845314, -1956.1198, 19.629417)},
            {3, new Vector3(466.05264, -1542.1353, 28.162691)},
            {4, new Vector3(1394.063, -1522.3331, 56.61154)},
            {5, new Vector3(868.0217, -2191.9995, 29.305378)},
            {10, new Vector3(1425.5674, 1117.222, 113.32523)},
            {11, new Vector3(-125.17264, 1002.61584, 234.61217)},
            {12, new Vector3(-1568.0859, -82.15858, 53.014442)},
            {13, new Vector3(-1801.1328, 395.96225, 111.609695)},
        };
        #endregion
        [ServerEvent(Event.PlayerEnterVehicle)]
        public static void OnPlayerEnterVehicleHandler(Player player, Vehicle vehicle, sbyte seatid)
        {
            try
            {
                if (vehicle.HasData("WZH") && vehicle.NumberPlate == "WZH")
                {
                    if (Fractions.Manager.FractionTypes[Main.Players[player].FractionID] == 2)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы не можете перевозить хаммер, т.к. вы состоите в гос.структуре.", 3000);
                        VehicleManager.WarpPlayerOutOfVehicle(player);
                        return;
                    }
                    if (Main.Players[player].FractionID == 0)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы не можете перевозить хаммер, т.к. вы не состоите в крайм.организации.", 3000);
                        VehicleManager.WarpPlayerOutOfVehicle(player);
                        return;
                    }
                    VehicleStreaming.SetEngineState(vehicle, true);
                    var fracid = Main.Players[player].FractionID;
                    var pos = Markers[fracid];
                    player.SetData("WZHVeh", true);
                    _cols = NAPI.ColShape.CreateCylinderColShape(pos, 7f, 3f, 0);
                    _marker = NAPI.Marker.CreateMarker(1, pos, new Vector3(), new Vector3(), 7f, new Color(0, 0, 255));
                    _cols.OnEntityEnterColShape += EnterCheckpoint;
                    _text = NAPI.TextLabel.CreateTextLabel("Выгрузка здесь!", pos + new Vector3(0, 0, 2.5), 7f, 3f, 4, new Color(0, 0, 255), false, 0);
                    Trigger.ClientEvent(player, "createWaypoint", pos.X, pos.Y);
                }
            }
            catch (Exception e) { RLog.Write("PlayerEnterVehicle: " + e.Message, nLog.Type.Error); }
        }
        public static void EnterCheckpoint(ColShape shape, Player player)
        {
            try
            {
                if (!player.IsInVehicle) return;
                if (player.IsInVehicle && player.Vehicle.HasData("WZH"))
                {
                    player.Vehicle.Delete();
                    var fracid = Main.Players[player].FractionID;
                    Fractions.Stocks.fracStocks[fracid].Materials += 7500;
                    Fractions.Stocks.fracStocks[fracid].UpdateLabel();
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы привезли на склад материалы", 3000);
                    VehicleManager.WarpPlayerOutOfVehicle(player);
                    NAPI.Task.Run(() =>
                    {
                        _cols.Delete();
                        _text.Delete();
                        _marker.Delete();
                    });
                }
                return;
            }
            catch (Exception e) { RLog.Write("EnterCheckpoint: " + e.Message, nLog.Type.Error); }
        }
        [ServerEvent(Event.PlayerExitVehicle)]
        public void Event_onPlayerExitVehicleHandler(Player player, Vehicle vehicle)
        {
            try
            {
                if (player.HasData("WZHVeh"))
                {
                    if (_cols != null) _cols.Delete();
                    if (_text != null) _text.Delete();
                    if (_marker != null) _marker.Delete();
                    player.ResetData("WZHVeh");
                    VehicleStreaming.SetEngineState(vehicle, false);
                }
            }
            catch (Exception e) { RLog.Write("PlayerExitVehicle: " + e.Message, nLog.Type.Error); }
        }
        public static void StopWzhs()
        {
            try
            {
                NAPI.Task.Run(() =>
                {
                    if (_cols != null) _cols.Delete();
                    if (_text != null) _text.Delete();
                    if (_veh != null) _veh.Delete();
                    if (_veh1 != null) _veh1.Delete();
                    if (_marker != null) _marker.Delete();
                    blip.Delete();
                });
                _isStart = false;
                foreach (var p in NAPI.Pools.GetAllPlayers())
                    if (Fractions.Manager.FractionTypes[Main.Players[p].FractionID] == 0 || Fractions.Manager.FractionTypes[Main.Players[p].FractionID] == 1)
                    {
                        p.SendChatMessage("Война за хамеры закончена.");
                        NAPI.Notification.SendNotificationToPlayer(p, "Война за хамеры закончена.");
                    }
            }
            catch (Exception e) { RLog.Write("StopWZHS: " + e.Message, nLog.Type.Error); }
        }
        #region CMD
        [Command("startwzh")]
        public static void CMD_StartWZH(Player player)
        {
            try
            {
                if (!Core.Group.CanUseCmd(player, "startwzh")) return;
                if (_isStart == true)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Война за хамеры только что началась.", 3000);
                    return;
                }
                SpawnVehicles();
            }
            catch (Exception e) { RLog.Write("StartWZH: " + e.Message, nLog.Type.Error); }
        }
        [Command("stopwzh")]
        public static void CMD_StopWZH(Player player)
        {
            try
            {
                if (!Core.Group.CanUseCmd(player, "stopwzh")) return;
                if (_isStart == false)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Война за хамеры только что закончилась.", 3000);
                    return;
                }
                StopWzhs();
            }
            catch (Exception e) { RLog.Write("StopWZH: " + e.Message, nLog.Type.Error); }
        }
        #endregion
    }
}