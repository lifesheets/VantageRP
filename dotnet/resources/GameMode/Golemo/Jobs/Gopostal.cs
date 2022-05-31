using GTANetworkAPI;
using System.Collections.Generic;
using System;
using Golemo.GUI;
using Golemo.Core;
using GolemoSDK;
using Golemo.Houses;

namespace Golemo.Jobs
{
    class Gopostal : Script
    {
        private static nLog Log = new nLog("GoPostal");

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                Cols.Add(0, NAPI.ColShape.CreateCylinderColShape(Coords[0], 1, 2, 0)); // start work
                Cols[0].OnEntityEnterColShape += gp_onEntityEnterColShape;
                Cols[0].OnEntityExitColShape += gp_onEntityExitColShape;
                Cols[0].SetData("INTERACT", 28);
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        private static int checkpointPayment = 1000;

        public static List<Vector3> Coords = new List<Vector3>()
        {
            new Vector3(-260.96762, -904.54315, 31.190844), // start work
        };
        private static Dictionary<int, ColShape> Cols = new Dictionary<int, ColShape>();
        private static Dictionary<int, ColShape> gCols = new Dictionary<int, ColShape>();
        // Postal items (objects) //
        public static List<uint> GoPostalObjects = new List<uint>
        {
            NAPI.Util.GetHashKey("prop_drug_package_02"),
        };

        [ServerEvent(Event.PlayerExitVehicle)]
        public void onPlayerExitVehicle(Player player, Vehicle vehicle)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].WorkID != 2) return;
                if (NAPI.Data.GetEntityData(player, "ON_WORK") && NAPI.Data.GetEntityData(player, "PACKAGES") != 0)
                {
                    int x = WorkManager.rnd.Next(0, GoPostalObjects.Count);
                    BasicSync.AttachObjectToPlayer(player, GoPostalObjects[x], 60309, new Vector3(0.03, 0, 0.02), new Vector3(0, 0, 50));
                }
            }
            catch (Exception e) { Log.Write("PlayerExitVehicle: " + e.Message, nLog.Type.Error); }
        }

        public static void Event_PlayerDeath(Player player, Player entityKiller, uint weapon)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].WorkID == 2 && NAPI.Data.GetEntityData(player, "ON_WORK"))
                {
                    player.SetData("ON_WORK", false);
                    player.SetData("PAYMENT", 0);
                    player.SetData("PACKAGES", 0);

                    Customization.ApplyCharacter(player);
                    Trigger.ClientEvent(player, "deleteWorkBlip");
                    Trigger.ClientEvent(player, "deleteCheckpoint", 1, 0);
                    NAPI.Task.Run(() => { BasicSync.DetachObject(player); }, 1000);
                    if (player.GetData<Vehicle>("WORK") != null)
                    {
                        NAPI.Entity.DeleteEntity(player.GetData<Vehicle>("WORK"));
                        player.SetData<Vehicle>("WORK", null);
                    }
                    Notify.Info(player, $"Вы закончили рабочий день", 3000);
                }
            }
            catch (Exception e) { Log.Write("PlayerDeath: " + e.Message, nLog.Type.Error); }
        }

        public static void GoPostal_onEntityEnterColShape(ColShape shape, Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (HouseManager.Houses.Count == 0) return;
                if (Main.Players[player].WorkID != 2 || !NAPI.Data.GetEntityData(player, "ON_WORK")) return;
                if (player.HasData("NEXTHOUSE") && player.HasData("HOUSEID") && NAPI.Data.GetEntityData(player, "NEXTHOUSE") == player.GetData<int>("HOUSEID"))
                {
                    if (NAPI.Player.IsPlayerInAnyVehicle(player))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Leave the vehicle", 3000);
                        return;
                    }
                    if (player.GetData<int>("PACKAGES") == 0) return;
                    else if (player.GetData<int>("PACKAGES") > 1)
                    {
                        var coef = Convert.ToInt32(player.Position.DistanceTo2D(player.GetData<Vector3>("W_LASTPOS")) / 100);
                        DateTime lastTime = player.GetData<DateTime>("W_LASTTIME");
                        if (DateTime.Now < lastTime.AddSeconds(coef * 2))
                        {
                            Notify.Error(player, "The owner is not at home, please try again later.", 3000);
                            return;
                        }
                        player.SetData("PACKAGES", player.GetData<int>("PACKAGES") - 1);
                        Notify.Info(player, $"У Вас осталось {player.GetData<int>("PACKAGES")} посылок", 3000);

                        var payment = Convert.ToInt32(coef * checkpointPayment * Group.GroupPayAdd[Main.Accounts[player].VipLvl] * Main.oldconfig.PaydayMultiplier);
                        //надбавка с учетом уровня игрока на данной работе
                        //добавление опыта
                        if (Main.Players[player].AddExpForWork(Main.oldconfig.PaydayMultiplier))
                            Notify.Alert(player, $"Congratulations on leveling up! current level now: {Main.Players[player].GetLevelAtThisWork()}");
                        MoneySystem.Wallet.Change(player, payment);
                        GameLog.Money($"server", $"player({Main.Players[player].UUID})", payment, $"postalCheck");

                        BasicSync.DetachObject(player);

                        var nextHouse = player.GetData<object>("NEXTHOUSE");
                        var next = -1;
                        do
                        {
                            next = WorkManager.rnd.Next(0, HouseManager.Houses.Count - 1);
                        }
                        while (Houses.HouseManager.Houses[next].Position.DistanceTo2D(player.Position) < 200);
                        player.SetData("W_LASTPOS", player.Position);
                        player.SetData("W_LASTTIME", DateTime.Now);
                        player.SetData("NEXTHOUSE", HouseManager.Houses[next].ID);

                        Trigger.ClientEvent(player, "createCheckpoint", 1, 1, HouseManager.Houses[next].Position, 1, 0, 255, 0, 0);
                        Trigger.ClientEvent(player, "createWaypoint", HouseManager.Houses[next].Position.X, HouseManager.Houses[next].Position.Y);
                        Trigger.ClientEvent(player, "createWorkBlip", HouseManager.Houses[next].Position);
                        NAPI.Player.PlayPlayerAnimation(player, -1, "anim@heists@narcotics@trash", "drop_side");
                    }
                    else
                    {
                        var coef = Convert.ToInt32(player.Position.DistanceTo2D(player.GetData<Vector3>("W_LASTPOS")) / 100);
                        DateTime lastTime = player.GetData<DateTime>("W_LASTTIME");
                        if (DateTime.Now < lastTime.AddSeconds(coef * 2))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Хозяина нет дома, попробуйте позже", 3000);
                            return;
                        }

                        var payment = Convert.ToInt32(coef * checkpointPayment * Group.GroupPayAdd[Main.Accounts[player].VipLvl]);
                        //надбавка с учетом уровня игрока на данной работе
                        payment += Jobs.WorkManager.PaymentIncreaseAmount[Main.Players[player].WorkID] * Main.Players[player].GetLevelAtThisWork();
                        //добавление опыта
                        if (Main.Players[player].AddExpForWork(Main.oldconfig.PaydayMultiplier))
                            Notify.Alert(player, $"Поздравляем с повышением уровня! Текущий уровень теперь: {Main.Players[player].GetLevelAtThisWork()}");
                        MoneySystem.Wallet.Change(player, payment);
                        GameLog.Money($"server", $"player({Main.Players[player].UUID})", payment, $"postalCheck");

                        Trigger.ClientEvent(player, "deleteWorkBlip");
                        Trigger.ClientEvent(player, "createWaypoint", 105.4633f, -1568.843f);

                        BasicSync.DetachObject(player);

                        Trigger.ClientEvent(player, "deleteCheckpoint", 1, 0);
                        NAPI.Player.PlayPlayerAnimation(player, -1, "anim@heists@narcotics@trash", "drop_side");
                        player.SetData("PACKAGES", 0);
                        Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"У Вас не осталось посылок, возьмите новые", 3000);
                    }
                }
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"GoPostal\":\n" + e.ToString(), nLog.Type.Error); }
        }
        private void gp_onEntityEnterColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", shape.GetData<int>("INTERACT"));
            }
            catch (Exception ex) { Log.Write("gp_onEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
        }
        private void gp_onEntityExitColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
            }
            catch (Exception ex) { Log.Write("gp_onEntityExitColShape: " + ex.Message, nLog.Type.Error); }
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void onPlayerEnterVehicle(Player player, Vehicle vehicle, sbyte seatid)
        {
            try
            {
                BasicSync.DetachObject(player);
            }
            catch (Exception e) { Log.Write("PlayerEnterVehicle: " + e.Message, nLog.Type.Error); }
        }
        public static Vector3[] GoPostalPosition = new Vector3[]
        {
            new Vector3(-282.88535, -914.90173, 31.480614),  //1
            new Vector3(-283.97055, -918.5027, 31.482304),     //2
            new Vector3(-308.2229, -909.6656, 31.480274),          //3
            new Vector3(-315.28146, -908.04346, 31.478937),            //4
            new Vector3(-322.68716, -906.3424, 31.479263),               //6
            new Vector3(-329.7898, -904.611, 31.480503),          //7
            new Vector3(-337.24588, -903.0805, 31.476934),          //8
            new Vector3(-327.63263, -924.5121, 31.481627),               //9
            new Vector3(-328.71387, -928.05273, 31.483341),                 //10
            new Vector3(-329.7941, -931.58215, 31.482365),                     //11
            new Vector3(-331.2554, -934.9551, 31.482347),                     //11
            new Vector3(-332.61777, -938.2651, 31.48429),                     //11
        };
        public static Vector3[] GoPostalRostation = new Vector3[]
        {
            new Vector3(-0.035715405, 0.0013870909, 69.592125),  //1
            new Vector3(-0.061156623, 0.006720415, 70.03513),     //2
            new Vector3(0.17549357, -0.07101518, 168.39957),          //3
            new Vector3(-0.07284627, -0.071367405, 167.31075),            //4
            new Vector3(-0.10462221, -0.050929017, 167.8245),               //6
            new Vector3(-0.009691902, 0.025734754, 168.85394),          //7
            new Vector3(-0.048233803, 0.010193029, 163.16072),          //8
            new Vector3(-0.043792084, -0.027216814, 69.352196),               //9
            new Vector3(-0.11702437, 0.013116606, 70.33562),                 //10
            new Vector3(-0.07020408, -0.007268885, 70.273926),                     //11
            new Vector3(-0.15972129, -0.021549193, 71.46376),                     //11
            new Vector3(-0.36198166, 0.025422568, 71.39994),                     //11
       };
        public static void getGoPostalCar(Player player)
        {
            if (Main.Players[player].WorkID != 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не работаете почтальоном", 3000);
                return;
            }
            if (!player.GetData<bool>("ON_WORK"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны начать рабочий день", 3000);
                return;
            }
            if (player.GetData<Vehicle>("WORK") != null)
            {
                NAPI.Entity.DeleteEntity(player.GetData<Vehicle>("WORK"));
                player.SetData<Vehicle>("WORK", null);
                return;
            }
            var rnd = new Random().Next(0, GoPostalPosition.Length);
            var veh = API.Shared.CreateVehicle(VehicleHash.Boxville2, GoPostalPosition[rnd], GoPostalRostation[rnd], 134, 77, "GOPOSTAL");
            player.SetData("WORK", veh);
            player.SetIntoVehicle(veh, 0);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы получили рабочий транспорт", 3000);
            veh.SetData("ACCESS", "WORK");
            Core.VehicleStreaming.SetEngineState(veh, true);
        }
    }
}
