using Golemo.Core;
using GolemoSDK;
using GTANetworkAPI;
using System;
using System.Collections.Generic;

namespace Golemo.Scripts
{
    #region Data
    class SprinterData
    {
        public ColShape Cols { get; set; }
        public TextLabel Text { get; set; }
        public SprinterData(ColShape col, TextLabel text)
        {
            Cols = col;
            Text = text;
        }
    }
    #endregion
    class Sprinter : Script
    {
        private static nLog RLog = new nLog("Sprinter");
        private static Random rnd = new Random();
        private static Dictionary<int, SprinterData> SprintersData = new Dictionary<int, SprinterData>();
        #region Checkpoints
        private static List<Vector3> Checkpoints = new List<Vector3>()
        {
             new Vector3(-617.0033, -1599.3397, 25.630264),
             new Vector3(-963.14294, -1282.4154, 4.037139),
             new Vector3(-1975.6167, -300.60104, 42.986095),
             new Vector3(-1843.7434, -322.44266, 48.025726),
             new Vector3(-1489.3601, -201.57156, 49.27749),
             new Vector3(-698.8425, 313.49368, 81.85817),
             new Vector3(-452.06226, 299.51324, 82.07714),
             new Vector3(251.90828, 378.7582, 104.41033),
             new Vector3(538.6052, -39.488316, 69.64222),
             new Vector3(1268.3446, -364.48376, 67.881905),
             new Vector3(1334.688, -756.81647, 67.04299),
             new Vector3(2661.5906, 1698.028, 23.340117),
             new Vector3(2770.1833, 2805.6038, 40.322353),
             new Vector3(2671.0479, 3525.0964, 51.384464),
             new Vector3(3578.6118, 3663.9114, 32.78228),
             new Vector3(2931.4211, 4309.7686, 49.701492),
             new Vector3(2307.3044, 4887.5117, 40.68578),
             new Vector3(-689.3224, -2456.669, 12.755113),
             new Vector3(-1272.9027, -1360.0321, 3.1205363),
             new Vector3(-1452.8916, -381.2956, 37.359768),
             new Vector3(-679.1697, 5797.5825, 16.210945),
             new Vector3(-80.81878, 6493.6216, 30.37089),
             new Vector3(1685.3204, 6437.812, 31.217184),
             new Vector3(2552.719, 4674.665, 32.8113),
             new Vector3(1982.3381, 3780.3752, 31.06079),
             new Vector3(1702.2665, 3596.0247, 34.317146),
             new Vector3(1362.0955, 3620.0232, 33.770638),
             new Vector3(450.81027, 3560.9602, 32.048054),
             new Vector3(-1133.099, 2696.5176, 17.68042),
             new Vector3(1243.7809, 1863.5703, 78.14359),
             new Vector3(1204.7982, 2641.138, 36.63915),
             new Vector3(2660.9294, 3276.8828, 54.120518),
             new Vector3(2548.7542, 342.1873, 107.34316),
             new Vector3(748.061, 1301.9269, 359.1765),
             new Vector3(1093.0692, -791.21466, 57.142696),
             new Vector3(822.34735, -2144.5447, 27.630606),
             new Vector3(921.5283, -1556.3578, 29.65599),
             new Vector3(1204.7228, -3117.7385, 4.420327),
             new Vector3(1190.3407, -3329.35, 4.4797826),
             new Vector3(134.78249, -3106.2124, 4.720085),
             new Vector3(-407.6967, -2799.6357, 4.8077035),
             new Vector3(495.4179, -1969.9838, 23.796118)
        };
        #endregion
        #region Events
        [ServerEvent(Event.PlayerEnterVehicle)]
        public static void OnPlayerEnterVehicleHandler(Player player, Vehicle vehicle, sbyte seatid)
        {
            try
            {
                if (vehicle == null || !VehicleManager.Vehicles.ContainsKey(vehicle.NumberPlate) || player == null || !Main.Players.ContainsKey(player) || !vehicle.HasData("MSPRINTER") || VehicleManager.Vehicles[vehicle.NumberPlate].Model != "msprinter" || vehicle.GetData<string>("ACCESS") != "PERSONAL") return;
                if (Main.Players[player].FirstName + "_" + Main.Players[player].LastName == VehicleManager.Vehicles[player.Vehicle.NumberPlate].Holder)
                {
                    int check = rnd.Next(0, Checkpoints.Count - 1);
                    player.SetData("WORKCHECKS", check);
                    int i = 0;
                    ColShape _cols = NAPI.ColShape.CreateCylinderColShape(Checkpoints[check], 7f, 3f);
                    _cols.OnEntityEnterColShape += EnterCheckpoint;
                    _cols.SetData("NUMBERS", i);
                    TextLabel _text = NAPI.TextLabel.CreateTextLabel("Выгрузка здесь!", Checkpoints[check], 8f, 3f, 4, new Color(0, 0, 255), false, 0);
                    SprinterData data = new SprinterData(_cols, _text);
                    SprintersData.Add(Main.Players[player].UUID, data);
                    Trigger.ClientEvent(player, "createCheckpoint", Main.Players[player].UUID, 1, Checkpoints[check], 6, 0, 0, 0, 255);
                    Trigger.ClientEvent(player, "createWaypoint", Checkpoints[check].X, Checkpoints[check].Y);
                    Trigger.ClientEvent(player, "ARREA:BLIPS:MSPRINTER", true, Checkpoints[check]);
                    player.SetData("WORK:MSSPRINTER", data);
                    i++;
                }
            }
            catch (Exception e) { RLog.Write("PlayerEnterVehicle: " + e.ToString(), nLog.Type.Error); }
        }
        [ServerEvent(Event.PlayerExitVehicle)]
        public static void Event_onPlayerExitVehicleHandler(Player player, Vehicle vehicle)
        {
            try
            {
                if (vehicle == null || !VehicleManager.Vehicles.ContainsKey(vehicle.NumberPlate) || !player.HasData("WORK:MSSPRINTER") || player == null || !Main.Players.ContainsKey(player) || !SprintersData.ContainsKey(Main.Players[player].UUID) || !vehicle.HasData("MSPRINTER") || VehicleManager.Vehicles[vehicle.NumberPlate].Model != "msprinter" || vehicle.GetData<string>("ACCESS") != "PERSONAL") return;
                {
                    if (Main.Players[player].FirstName + "_" + Main.Players[player].LastName == VehicleManager.Vehicles[player.Vehicle.NumberPlate].Holder)
                    {
                        Trigger.ClientEvent(player, "deleteCheckpoint", Main.Players[player].UUID);
                        Trigger.ClientEvent(player, "deleteWaypoint");
                        Trigger.ClientEvent(player, "ARREA:BLIPS:MSPRINTER", false, new Vector3());
                        SprintersData.Remove(Main.Players[player].UUID);
                        SprinterData data = player.GetData<SprinterData>("WORK:MSSPRINTER");
                        data.Cols.Delete();
                        data.Text.Delete();
                        player.ResetData("WORK:MSSPRINTER");
                    }
                }
            }
            catch (Exception e) { RLog.Write("PlayerExitVehicle: " + e.ToString(), nLog.Type.Error); }
        }
        #endregion
        #region Timer
        private static void timer_playerEnterCheck(Player player)
        {
            try
            {
                if (player == null || !Main.Players.ContainsKey(player) || !SprintersData.ContainsKey(Main.Players[player].UUID) || !player.HasData("WORK:MSSPRINTER")) return;
                int price = rnd.Next(2, 10) * 5000 + Main.pluscost + rnd.Next(100, 200);
                MoneySystem.Wallet.Change(player, +price);
                VehicleStreaming.SetEngineState(player.Vehicle, true);
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили за доставку груза {price}$.", 3000);
            }
            catch (Exception e) { RLog.Write("timer_playerEnterCheck: " + e.ToString(), nLog.Type.Error); }
        }
        #endregion
        #region EnterCheckPoint
        private static void EnterCheckpoint(ColShape shape, Player player)
        {
            try
            {
                if (player.Vehicle == null || !VehicleManager.Vehicles.ContainsKey(player.Vehicle.NumberPlate) || player == null || !Main.Players.ContainsKey(player) || !SprintersData.ContainsKey(Main.Players[player].UUID) || !player.HasData("WORK:MSSPRINTER")) return;
                if (VehicleManager.Vehicles[player.Vehicle.NumberPlate].Model == "msprinter" && Main.Players[player].FirstName + "_" + Main.Players[player].LastName == VehicleManager.Vehicles[player.Vehicle.NumberPlate].Holder && player.IsInVehicle && player.VehicleSeat == 0 && player.Vehicle.NumberPlate != "CARSHARE" && player.Vehicle.NumberPlate != "TESTDRIVE")
                {
                    Timers.StartTask(5000, () => timer_playerEnterCheck(player));
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы доехали до точки. Подождите 5 секунд для разгрузки.", 3000);
                    VehicleStreaming.SetEngineState(player.Vehicle, false);
                    SprinterData data = player.GetData<SprinterData>("WORK:MSSPRINTER");
                    data.Cols.Delete();
                    data.Text.Delete();
                    player.ResetData("WORK:MSSPRINTER");
                    SprintersData.Remove(Main.Players[player].UUID);
                    Trigger.ClientEvent(player, "ARREA:BLIPS:MSPRINTER", false, new Vector3());
                    Trigger.ClientEvent(player, "deleteCheckpoint", Main.Players[player].UUID);
                    Trigger.ClientEvent(player, "deleteWaypoint");
                    int nextCheck = rnd.Next(0, Checkpoints.Count - 1);
                    while (nextCheck == shape.GetData<int>("NUMBERS")) nextCheck = rnd.Next(0, Checkpoints.Count - 1);
                    ColShape _cols = NAPI.ColShape.CreateCylinderColShape(Checkpoints[nextCheck], 7f, 3f);
                    _cols.OnEntityEnterColShape += EnterCheckpoint;
                    TextLabel _text = NAPI.TextLabel.CreateTextLabel("Выгрузка здесь!", Checkpoints[nextCheck], 8f, 3f, 4, new Color(0, 0, 255), false, 0);
                    SprinterData sprinterdata = new SprinterData(_cols, _text);
                    SprintersData.Add(Main.Players[player].UUID, sprinterdata);
                    Trigger.ClientEvent(player, "createCheckpoint", Main.Players[player].UUID, 1, Checkpoints[nextCheck], 6, 0, 0, 0, 255);
                    Trigger.ClientEvent(player, "createWaypoint", Checkpoints[nextCheck].X, Checkpoints[nextCheck].Y);
                    Trigger.ClientEvent(player, "ARREA:BLIPS:MSPRINTER", true, Checkpoints[nextCheck]);
                }
            }
            catch (Exception e) { RLog.Write("EnterCheckpoint: " + e.ToString(), nLog.Type.Error); }
        }
        #endregion
    }
}