using GTANetworkAPI;
using GolemoSDK;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Golemo.Core
{
    public class AutoSchool : Script
    {
        private static nLog Log = new nLog("AutoSchool");
        private static Vector3 enterSchool = new Vector3(-711.3695, -1305.2975, 3.992632);
        private static Vector3 SpawnVehiclePos = new Vector3(-707.0867, -1275.8596, 4.881519);
        private static Vector3 SpawnVehicleRot = new Vector3(0.104611255, 0.019487236, 141.83179);
        private static Dictionary<int, int> PlayersCheck = new Dictionary<int, int>();
        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                //NAPI.Marker.CreateMarker(1, enterSchool - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1.4f, new Color(0, 255, 255));
                NAPI.Blip.CreateBlip(498, new Vector3(-711.3695, -1305.2975, 3.99263), 0.7f, 4, "Центр лицензирования", 255, 0, true, 0, 0);
                ColShape shape = NAPI.ColShape.CreateCylinderColShape(enterSchool, 1.4f, 3);
                shape.OnEntityEnterColShape += (s, entity) =>
                {
                    try
                    {
                        entity.SetData("INTERACTIONCHECK", 6001);
                    }
                    catch (Exception e) { Log.Write("shape.OnEntityEnterColshape: " + e.ToString(), nLog.Type.Error); }
                };
                shape.OnEntityExitColShape += (s, entity) =>
                {
                    try
                    {
                        entity.SetData("INTERACTIONCHECK", 0);
                    }
                    catch (Exception e) { Log.Write("shape.OnEntityEnterColshape: " + e.ToString(), nLog.Type.Error); }
                };
                for (int i = 0; i < DriveExamPos.Count; i++)
                {
                    shape = NAPI.ColShape.CreateCylinderColShape(DriveExamPos[i], 12, 3);
                    shape.OnEntityEnterColShape += enterDriveColShape;
                    shape.SetData("NUMBER", i);
                }
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.ToString(), nLog.Type.Error); }
        }
        [RemoteEvent("StartExam:Server")]
        public static void StartExam_Server(Player player, int id)
        {
            try
            {
                if (!Main.Players.ContainsKey(player) || !CostLic.ContainsKey(id)) return;
                Vehicle vehicle = null;
                if (Main.Players[player].Licenses[id])
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You already have this license", 3000);
                    return;
                }
                if (Main.Players[player].Money < CostLic[id])
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You don't have enough money to buy this license", 3000);
                    return;
                }
                switch (id)
                {
                    case 0:
                        vehicle = NAPI.Vehicle.CreateVehicle((VehicleHash)NAPI.Util.GetHashKey("manchez"), SpawnVehiclePos, SpawnVehicleRot, 30, 30);
                        player.SetData("LICENSE", 0);
                        break;
                    case 1:
                        vehicle = NAPI.Vehicle.CreateVehicle((VehicleHash)NAPI.Util.GetHashKey("s600"), SpawnVehiclePos, SpawnVehicleRot, 30, 30);
                        player.SetData("LICENSE", 1);
                        break;
                    case 2:
                        vehicle = NAPI.Vehicle.CreateVehicle((VehicleHash)NAPI.Util.GetHashKey("bison"), SpawnVehiclePos, SpawnVehicleRot, 30, 30);
                        player.SetData("LICENSE", 2);
                        break;
                    case 3:
                        Main.Players[player].Licenses[4] = true;
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "You have successfully purchased a helicopter license", 3000);
                        break;
                    case 4:
                        Main.Players[player].Licenses[5] = true;
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "You have successfully purchased an aircraft license", 3000);
                        break;
                }
                MoneySystem.Wallet.Change(player, -CostLic[id]);
                if (vehicle != null)
                {
                    player.SetData("SCHOOLVEH", vehicle);
                    vehicle.SetData("ACCESS", "SCHOOL");
                    vehicle.NumberPlate = "SCHOOL";
                    vehicle.SetData("DRIVER", player);
                    Trigger.ClientEvent(player, "createCheckpoint", 12, 1, DriveExamPos[0] - new Vector3(0, 0, 2), 4, 0, 255, 0, 0);
                    Trigger.ClientEvent(player, "createWaypoint", DriveExamPos[0].X, DriveExamPos[0].Y);
                    VehicleStreaming.SetEngineState(vehicle, false);
                    PlayersCheck.Add(Main.Players[player].UUID, 0);
                    NAPI.Task.Run(() => player.SetIntoVehicle(vehicle, 0));
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Drive to the points", 3000);
                }
            }
            catch (Exception e) { Log.Write("StartExam_Server: " + e.ToString(), nLog.Type.Error); }
        }
        private static void enterDriveColShape(ColShape shape, Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player) || !player.IsInVehicle || !player.HasData("SCHOOLVEH") || !player.Vehicle.HasData("DRIVER")) return;
                if (player.GetData<Vehicle>("SCHOOLVEH") == player.Vehicle && player.Vehicle.GetData<Player>("DRIVER") == player && shape.GetData<int>("NUMBER") == PlayersCheck[Main.Players[player].UUID])
                {
                    if (PlayersCheck[Main.Players[player].UUID] == DriveExamPos.Count - 1)
                    {
                        PlayersCheck.Remove(Main.Players[player].UUID);
                        float vehHP = player.Vehicle.Health;
                        if (player.Vehicle != null)
                            player.Vehicle.Delete();
                        if (vehHP < 500)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "you failed the exam", 3000);
                            return;
                        }
                        Main.Players[player].Licenses[player.GetData<int>("LICENSE")] = true;
                        player.ResetData("SCHOOLVEH");
                        player.ResetData("LICENSE");
                        Trigger.ClientEvent(player, "deleteCheckpoint", 12, 0);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "You have successfully passed the exam", 3000);
                        return;
                    }
                    PlayersCheck[Main.Players[player].UUID] += 1;
                    Trigger.ClientEvent(player, "createCheckpoint", 12, 1, DriveExamPos[PlayersCheck[Main.Players[player].UUID]] - new Vector3(0, 0, 2), 4, 0, 255, 0, 0);
                    Trigger.ClientEvent(player, "createWaypoint", DriveExamPos[PlayersCheck[Main.Players[player].UUID]].X, DriveExamPos[PlayersCheck[Main.Players[player].UUID]].Y);
                }
            }
            catch (Exception e) { Log.Write("enterDriveColShape: " + e.ToString(), nLog.Type.Error); }
        }
        #region List
        private static Dictionary<int, int> CostLic = new Dictionary<int, int>
        {
            { 0, 5000 },
            { 1, 10000 },
            { 2, 15000 },
            { 3, 250000 },
            { 4, 500000 },
            { 5, 2000000 }
        };
        private static List<Vector3> DriveExamPos = new List<Vector3>()
        {
            new Vector3(-743.29065, -1297.7557, 4.8826632),     //as1
            new Vector3(-721.6567, -1264.4321, 8.120333),     //as2
            new Vector3(-694.88525, -1240.9398, 10.445287),     //as3
            new Vector3(-659.7931, -1314.9128, 10.4506035),     //as4
            new Vector3(-659.5859, -1438.2676, 10.464868),     //as5
            new Vector3(-707.8045, -1509.1527, 12.126915),     //as6
            new Vector3(-751.8379, -1589.1432, 14.294757),     //as7
            new Vector3(-814.85455, -1668.3447, 17.078615),     //as8
            new Vector3(-972.41785, -1832.2412, 19.573656),     //as9
            new Vector3(-1089.5142, -2020.5852, 12.996085),     //as10
            new Vector3(-967.31323, -2150.9453, 8.670086),     //as11
            new Vector3(-947.7292, -2144.3386, 8.845933),     //as12
            new Vector3(-942.781, -2122.7502, 9.210661),     //as13
            new Vector3(-918.0939, -2067.7847, 9.181835),     //as14
            new Vector3(-891.56586, -2051.6838, 9.180723),     //as15
            new Vector3(-944.291, -2105.3604, 9.181939),     //as16
            new Vector3(-952.09595, -2140.8408, 8.829953),     //as17
            new Vector3(-1049.8688, -2058.6707, 13.078414),     //as18
            new Vector3(-973.9227, -1841.0247, 19.36009),     //as19
            new Vector3(-833.73773, -1696.0359, 18.403006),     //as20
            new Vector3(-728.14795, -1574.0854, 14.234413),     //as21
            new Vector3(-673.41345, -1470.7544, 10.417674),     //as21
            new Vector3(-645.2232, -1400.5048, 10.539662),     //as21
            new Vector3(-632.62177, -1314.5555, 10.537052),     //as21
            new Vector3(-640.1948, -1275.1984, 10.47842),     //as21
            new Vector3(-679.41455, -1231.6116, 10.54893),     //as21
            new Vector3(-707.18567, -1240.5889, 10.269665),     //as21
            new Vector3(-762.9907, -1287.5219, 4.8821507),     //as21
            new Vector3(-724.15, -1286.1101, 4.8817043),     //as21
        };
        #endregion
        #region Timer
        [ServerEvent(Event.PlayerExitVehicle)]
        public void Event_OnPlayerExitVehicle(Player player, Vehicle vehicle)
        {
            try
            {
                if (player.HasData("SCHOOLVEH") && player.GetData<Vehicle>("SCHOOLVEH") == vehicle)
                {
                    player.SetData("SCHOOL_TIMER", Timers.StartOnce(60000, () => timer_exitVehicle(player)));
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "If you don't get in the car within 60 seconds, you will fail the exam.", 3000);
                    return;
                }
            }
            catch (Exception e) { Log.Write("Event_OnPlayerExitVehicle: " + e.ToString(), nLog.Type.Error); }
        }
        private void timer_exitVehicle(Player player)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (!Main.Players.ContainsKey(player) || !player.HasData("SCHOOLVEH")) return;
                    if (player.IsInVehicle && player.Vehicle == player.GetData<Vehicle>("SCHOOLVEH")) return;
                    if (player.GetData<Vehicle>("SCHOOLVEH") != null)
                        player.GetData<Vehicle>("SCHOOLVEH").Delete();
                    Trigger.ClientEvent(player, "deleteCheckpoint", 12, 0);
                    player.ResetData("IS_DRIVING");
                    player.ResetData("SCHOOLVEH");
                    player.ResetData("LICENSE");
                    Timers.Stop(player.GetData<string>("SCHOOL_TIMER"));
                    player.ResetData("SCHOOL_TIMER");
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "you failed the exam", 3000);
                }
                catch (Exception e) { Log.Write("timer_exitVehicle: " + e.ToString(), nLog.Type.Error); }
            });
        }
        #endregion
    }
}