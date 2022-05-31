using GTANetworkAPI;
using System.Collections.Generic;
using Golemo.GUI;
using System;
using Golemo.Core;
using GolemoSDK;

namespace Golemo.Jobs
{
    class AutoMechanic : Script
    {
        public static List<CarInfo> CarInfos = new List<CarInfo>();

        private static Dictionary<int, ColShape> Cols = new Dictionary<int, ColShape>();
        // добавляем в этот лист координаты починок
        public static List<Vector3> Coords = new List<Vector3>()
        {
            /*new Vector3(-357.53308, -115.3788, 37.575314),
            new Vector3(-155.62201, -1302.867, 30.182077),
            new Vector3(-155.62201, -1302.867, 30.182077),
            new Vector3(-2426.179, 3334.5247, 31.664808)*/
        };
        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                for (int i = 0; i < Coords.Count; i++)
                {
                    Cols.Add(i, NAPI.ColShape.CreateCylinderColShape(Coords[i], 2, 2, 0));
                    Cols[i].OnEntityEnterColShape += auto_onEntityEnterColshape;
                    Cols[i].OnEntityExitColShape += auto_onEntityExitColshape;
                    NAPI.TextLabel.CreateTextLabel(Main.StringToU16($"~b~Починка авто. \n ~r~Стоимость: {price}$"), Coords[i] + new Vector3(0, 0, 1.3), 10F, 0.6F, 0, new Color(0, 180, 0));
                    NAPI.Marker.CreateMarker(1, Coords[i] - new Vector3(0, 0, 2), new Vector3(), new Vector3(), 4, new Color(255, 0, 0, 120));
                    NAPI.Blip.CreateBlip(544, Coords[i], 1, 2, Main.StringToU16("Починка авто"), 255, 0, true, 0, 0);
                }

            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }
        public static void mechanicCarsSpawner()
        {
            for (int a = 0; a < CarInfos.Count; a++)
            {
                var veh = NAPI.Vehicle.CreateVehicle(CarInfos[a].Model, CarInfos[a].Position, CarInfos[a].Rotation.Z, CarInfos[a].Color1, CarInfos[a].Color2, CarInfos[a].Number);
                NAPI.Data.SetEntityData(veh, "ACCESS", "WORK");
                NAPI.Data.SetEntityData(veh, "WORK", 8);
                NAPI.Data.SetEntityData(veh, "TYPE", "MECHANIC");
                NAPI.Data.SetEntityData(veh, "NUMBER", a);
                NAPI.Data.SetEntityData(veh, "ON_WORK", false);
                NAPI.Data.SetEntityData(veh, "DRIVER", null);
                NAPI.Data.SetEntitySharedData(veh, "FUELTANK", 0);
                veh.SetSharedData("PETROL", VehicleManager.VehicleTank[veh.Class]);
                Core.VehicleStreaming.SetEngineState(veh, false);
                Core.VehicleStreaming.SetLockStatus(veh, false);
            }
        }
        private static nLog Log = new nLog("Mechanic");

        private static int mechanicRentCost = 100;
        private static int price = 500; // цену на починку редактировать тут
        private static Dictionary<Player, ColShape> orderCols = new Dictionary<Player, ColShape>();

        private void auto_onEntityEnterColshape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 135);
            }
            catch (Exception ex) { Log.Write("onEntityEnterColshape: " + ex.Message, nLog.Type.Error); }
        }
        private void auto_onEntityExitColshape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
            }
            catch (Exception ex) { Log.Write("onEntityExitColshape: " + ex.Message, nLog.Type.Error); }
        }
        public static void mechanicRepair(Player player, Player target, int price)
        {
            if (Main.Players[player].WorkID != 8 || !player.GetData<bool>("ON_WORK"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You do not work as an auto mechanic", 3000);
                return;
            }
            if (!player.IsInVehicle || !player.Vehicle.HasData("TYPE") || player.Vehicle.GetData<string>("TYPE") != "MECHANIC")
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You must be in a working vehicle", 3000);
                return;
            }
            if (!target.IsInVehicle)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The player must be in the vehicle", 3000);
                return;
            }
            if (player.Vehicle.Position.DistanceTo(target.Vehicle.Position) > 5)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The player is too far away from you", 3000);
                return;
            }
            if (price < 50 || price > 300)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You can set the price from $50 to $300", 3000);
                return;
            }
            if (Main.Players[target].Money < price)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player doesn't have enough money", 3000);
                return;
            }

            target.SetData("MECHANIC", player);
            target.SetData("MECHANIC_PRICE", price);
            Trigger.ClientEvent(target, "openDialog", "REPAIR_CAR", $"Игрок ({player.Value}) предложил отремонтировать Ваш транспорт за ${price}");

            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"You offered the player ({target.Value}) repair vehicles for {price}$", 3000);
        }

        public static void mechanicRent(Player player)
        {
            if (!NAPI.Player.IsPlayerInAnyVehicle(player) || player.VehicleSeat != 0 || player.Vehicle.GetData<string>("TYPE") != "MECHANIC") return;

            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"You rented a work vehicle. Expect an order", 3000);
            MoneySystem.Wallet.Change(player, -mechanicRentCost);
            GameLog.Money($"player({Main.Players[player].UUID})", $"server", mechanicRentCost, $"mechanicRent");
            var vehicle = player.Vehicle;
            NAPI.Data.SetEntityData(player, "WORK", vehicle);
            Core.VehicleStreaming.SetEngineState(vehicle, false);
            NAPI.Data.SetEntityData(player, "IN_WORK_CAR", true);
            NAPI.Data.SetEntityData(player, "ON_WORK", true);
            NAPI.Data.SetEntityData(vehicle, "DRIVER", player);
        }


        [RemoteEvent("server::lscustomsrepair")]
        public static void mechanicfixcar(Player player)
        {
            try
            {
                if (!player.IsInVehicle)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You must be in a vehicle", 3000);
                    return;
                }
                if (Main.Players[player].Money < price)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You don't have enough money", 3000);
                    return;
                }
                VehicleManager.RepairCar(player.Vehicle);
                MoneySystem.Wallet.Change(player, -price);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"You have successfully completed the transport for {price}$", 3000);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"AutoMechanic\":\n" + e.ToString(), nLog.Type.Error); }
        }


        public static void mechanicPay(Player player)
        {
            if (!player.IsInVehicle)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You must be in a vehicle", 3000);
                return;
            }

            var price = NAPI.Data.GetEntityData(player, "MECHANIC_PRICE");
            if (Main.Players[player].Money < price)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You don't have enough funds", 3000);
                return;
            }

            VehicleManager.RepairCar(player.Vehicle);
            var driver = NAPI.Data.GetEntityData(player, "MECHANIC");
            MoneySystem.Wallet.Change(player, -price);
            MoneySystem.Wallet.Change(driver, price);
            GameLog.Money($"player({Main.Players[player].UUID})", $"player({Main.Players[driver].UUID})", price, $"mechanicRepair");
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You paid for the repair of your vehicle", 3000);
            Notify.Send(driver, NotifyType.Info, NotifyPosition.BottomCenter, $"Player ({player.Value}) paid for the repair", 3000);
            Commands.RPChat("me", driver, $"починил автомобиль");

            player.ResetData("MECHANIC_DRIVER");
            driver.ResetData("MECHANIC_CLIENT");
            try
            {
                NAPI.ColShape.DeleteColShape(orderCols[player]);
                orderCols.Remove(player);
            }
            catch { }
        }

        private static void order_onEntityExit(ColShape shape, Player player)
        {
            if (shape.GetData<Player>("MECHANIC_CLIENT") != player) return;

            if (player.HasData("MECHANIC_DRIVER"))
            {
                Player driver = player.GetData<Player>("MECHANIC_DRIVER");
                driver.ResetData("MECHANIC_CLIENT");
                player.ResetData("MECHANIC_DRIVER");
                player.SetData("IS_CALL_MECHANIC", false);
                Notify.Send(driver, NotifyType.Warning, NotifyPosition.BottomCenter, $"The customer canceled the order", 3000);
                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"You left the mechanic's call", 3000);
                try
                {
                    NAPI.ColShape.DeleteColShape(orderCols[player]);
                    orderCols.Remove(player);
                }
                catch { }
            }
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void Event_onPlayerEnterVehicleHandler(Player player, Vehicle vehicle, sbyte seatid)
        {
            try
            {
                if (NAPI.Data.GetEntityData(vehicle, "TYPE") != "MECHANIC") return;
                if (seatid == 0)
                {
                    if (!Main.Players[player].Licenses[1])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You do not have a category B license", 3000);
                        VehicleManager.WarpPlayerOutOfVehicle(player);
                        return;
                    }
                    if (Main.Players[player].WorkID == 8)
                    {
                        if (NAPI.Data.GetEntityData(player, "WORK") == null)
                        {
                            if (vehicle.GetData<Player>("DRIVER") != null)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"This work transport is already taken", 3000);
                                return;
                            }
                            if (Main.Players[player].Money >= mechanicRentCost)
                            {
                                Trigger.ClientEvent(player, "openDialog", "MECHANIC_RENT", $"Rent a working vehicle for ${mechanicRentCost}?");
                            }
                            else
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You are missing " + (mechanicRentCost - Main.Players[player].Money) + "$ for rental of working vehicles", 3000);
                                VehicleManager.WarpPlayerOutOfVehicle(player);
                            }
                        }
                        else if (NAPI.Data.GetEntityData(player, "WORK") == vehicle) NAPI.Data.SetEntityData(player, "IN_WORK_CAR", true);
                        else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы уже работаете", 3000);
                    }
                    else
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You are not an auto mechanic. You can get a job at City Hall", 3000);
                        VehicleManager.WarpPlayerOutOfVehicle(player);
                    }
                }
            }
            catch (Exception e) { Log.Write("PlayerEnterVehicle: " + e.Message, nLog.Type.Error); }
        }

        public static void respawnCar(Vehicle veh)
        {
            try
            {
                int i = NAPI.Data.GetEntityData(veh, "NUMBER");
                NAPI.Entity.SetEntityPosition(veh, CarInfos[i].Position);
                NAPI.Entity.SetEntityRotation(veh, CarInfos[i].Rotation);
                VehicleManager.RepairCar(veh);
                NAPI.Data.SetEntityData(veh, "ACCESS", "WORK");
                NAPI.Data.SetEntityData(veh, "WORK", 8);
                NAPI.Data.SetEntityData(veh, "TYPE", "MECHANIC");
                NAPI.Data.SetEntityData(veh, "NUMBER", i);
                NAPI.Data.SetEntityData(veh, "ON_WORK", false);
                NAPI.Data.SetEntityData(veh, "DRIVER", null);
                NAPI.Data.SetEntitySharedData(veh, "FUELTANK", 0);
                Core.VehicleStreaming.SetEngineState(veh, false);
                Core.VehicleStreaming.SetLockStatus(veh, false);
                veh.SetSharedData("PETROL", VehicleManager.VehicleTank[veh.Class]);
            }
            catch (Exception e) { Log.Write("RespawnCar: " + e.Message, nLog.Type.Error); }
        }

        public static void onPlayerDissconnectedHandler(Player player, DisconnectionType type, string reason)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (player.HasData("MECHANIC_DRIVER"))
                {
                    Player driver = player.GetData<Player>("MECHANIC_DRIVER");
                    driver.ResetData("MECHANIC_CLIENT");
                    Notify.Send(driver, NotifyType.Warning, NotifyPosition.BottomCenter, $"The customer canceled the order", 3000);
                    try
                    {
                        NAPI.ColShape.DeleteColShape(orderCols[player]);
                        orderCols.Remove(player);
                    }
                    catch { }
                }
                if ((Main.Players[player].WorkID == 8 && NAPI.Data.GetEntityData(player, "ON_WORK") && NAPI.Data.GetEntityData(player, "WORK") != null))
                {
                    var vehicle = NAPI.Data.GetEntityData(player, "WORK");
                    respawnCar(vehicle);
                    if (player.HasData("MECHANIC_CLIENT"))
                    {
                        Player client = player.GetData<Player>("MECHANIC_CLIENT");
                        client.ResetData("MECHANIC_DRIVER");
                        client.SetData("IS_CALL_MECHANIC", false);
                        Notify.Send(client, NotifyType.Warning, NotifyPosition.BottomCenter, $"Auto mechanic left the workplace, make a new order", 3000);
                        try
                        {
                            NAPI.ColShape.DeleteColShape(orderCols[client]);
                            orderCols.Remove(client);
                        }
                        catch { }
                    }
                }
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void Event_onPlayerExitVehicleHandler(Player player, Vehicle vehicle)
        {
            try
            {
                if (NAPI.Data.GetEntityData(vehicle, "ACCESS") == "WORK" &&
                Main.Players[player].WorkID == 8 &&
                NAPI.Data.GetEntityData(player, "WORK") == vehicle)
                {
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"If you do not get on the transport in 5 minutes, then the working day will end", 3000);
                    NAPI.Data.SetEntityData(player, "IN_WORK_CAR", false);
                    if (player.HasData("WORK_CAR_EXIT_TIMER"))
                        //Main.StopT(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"), "timer_1");
                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                    NAPI.Data.SetEntityData(player, "CAR_EXIT_TIMER_COUNT", 0);
                    //NAPI.Data.SetEntityData(player, "WORK_CAR_EXIT_TIMER", Main.StartT(1000, 1000, (o) => timer_playerExitWorkVehicle(player, vehicle), "AUM_EXIT_CAR_TIMER"));
                    NAPI.Data.SetEntityData(player, "WORK_CAR_EXIT_TIMER", Timers.Start(1000, () => timer_playerExitWorkVehicle(player, vehicle)));
                }
            }
            catch (Exception e) { Log.Write("PlayerExit: " + e.Message, nLog.Type.Error); }
        }

        private void timer_playerExitWorkVehicle(Player player, Vehicle vehicle)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (!player.HasData("WORK_CAR_EXIT_TIMER")) return;
                    if (NAPI.Data.GetEntityData(player, "IN_WORK_CAR"))
                    {
                        //Main.StopT(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"), "timer_2");
                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                        NAPI.Data.ResetEntityData(player, "WORK_CAR_EXIT_TIMER");
                        return;
                    }
                    if (NAPI.Data.GetEntityData(player, "CAR_EXIT_TIMER_COUNT") > 300)
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы закончили рабочий день", 3000);
                        respawnCar(vehicle);
                        player.SetData<bool>("ON_WORK", false);
                        player.SetData<Vehicle>("WORK", null);
                        //Main.StopT(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"), "timer_3");
                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                        NAPI.Data.ResetEntityData(player, "WORK_CAR_EXIT_TIMER");
                        if (player.HasData("MECHANIC_CLIENT"))
                        {
                            Player client = player.GetData<Player>("MECHANIC_CLIENT");
                            client.ResetData("MECHANIC_DRIVER");
                            client.SetData("IS_CALL_MECHANIC", false);
                            Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"Auto mechanic left the workplace, make a new order", 3000);
                            player.ResetData("MECHANIC_CLIENT");
                            try
                            {
                                NAPI.ColShape.DeleteColShape(orderCols[client]);
                                orderCols.Remove(client);
                            }
                            catch { }
                        }
                        return;
                    }
                    NAPI.Data.SetEntityData(player, "CAR_EXIT_TIMER_COUNT", NAPI.Data.GetEntityData(player, "CAR_EXIT_TIMER_COUNT") + 1);

                }
                catch (Exception e)
                {
                    Log.Write("Timer_PlayerExitWorkVehicle:\n" + e.ToString(), nLog.Type.Error);
                }
            });
        }

        public static void acceptMechanic(Player player, Player target)
        {
            if (Main.Players[player].WorkID == 8 && player.GetData<bool>("ON_WORK"))
            {
                if (player.HasData("MECHANIC_CLIENT"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You have already taken an order", 3000);
                    return;
                }
                if (NAPI.Data.GetEntityData(target, "IS_CALL_MECHANIC") && !target.HasData("MECHANIC_DRIVER"))
                {
                    Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"Player ({player.Value}) accepted your challenge. Stay where you are", 3000);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"You accepted the player's challenge ({target.Value})", 3000);
                    Trigger.ClientEvent(player, "createWaypoint", NAPI.Entity.GetEntityPosition(target).X, NAPI.Entity.GetEntityPosition(target).Y);

                    target.SetData("MECHANIC_DRIVER", player);
                    player.SetData("MECHANIC_CLIENT", target);

                    orderCols.Add(target, NAPI.ColShape.CreateCylinderColShape(target.Position, 10F, 10F, 0));
                    orderCols[target].SetData("MECHANIC_CLIENT", target);
                    orderCols[target].OnEntityExitColShape += order_onEntityExit;
                }
                else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The player did not call the mechanic", 3000);
            }
            else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You are not currently working as an auto mechanic", 3000);
        }

        public static void cancelMechanic(Player player)
        {
            if (player.HasData("MECHANIC_CLIENT"))
            {
                Player client = player.GetData<Player>("MECHANIC_CLIENT");
                client.ResetData("MECHANIC_DRIVER");
                client.SetData("IS_CALL_MECHANIC", false);
                player.ResetData("MECHANIC_CLIENT");
                Notify.Send(client, NotifyType.Warning, NotifyPosition.BottomCenter, $"Auto mechanic left the workplace, make a new order", 3000);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"You canceled a visit to a client", 3000);
                try
                {
                    NAPI.ColShape.DeleteColShape(orderCols[client]);
                    orderCols.Remove(client);
                }
                catch { }
                return;
            }
            if (NAPI.Data.GetEntityData(player, "IS_CALL_MECHANIC"))
            {
                NAPI.Data.SetEntityData(player, "IS_CALL_MECHANIC", false);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"You canceled the auto mechanic's call", 3000);
                if (player.HasData("MECHANIC_DRIVER"))
                {
                    Player driver = player.GetData<Player>("MECHANIC_DRIVER");
                    driver.ResetData("MECHANIC_CLIENT");
                    player.ResetData("MECHANIC_DRIVER");
                    Notify.Send(driver, NotifyType.Warning, NotifyPosition.BottomCenter, $"The customer canceled the order", 3000);
                    try
                    {
                        NAPI.ColShape.DeleteColShape(orderCols[player]);
                        orderCols.Remove(player);
                    }
                    catch { }
                }
            }
            else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You didn't call the auto mechanic.", 3000);
        }

        public static void callMechanic(Player player)
        {
            if (!NAPI.Data.GetEntityData(player, "IS_CALL_MECHANIC"))
            {
                List<Player> players = NAPI.Pools.GetAllPlayers();
                var i = 0;
                foreach (var p in players)
                {
                    if (p == null || !Main.Players.ContainsKey(p)) continue;
                    if (Main.Players[p].WorkID == 8 && NAPI.Data.GetEntityData(p, "ON_WORK"))
                    {
                        i++;
                        NAPI.Chat.SendChatMessageToPlayer(p, $"~g~[ДИСПЕТЧЕР]: ~w~Player ({player.Value}) called the auto mechanic ~y~({player.Position.DistanceTo(p.Position)}м)~w~. Write ~y~/ma ~b~[ID]~w~, to accept the challenge");
                    }
                }
                if (i > 0)
                {
                    NAPI.Data.SetEntityData(player, "IS_CALL_MECHANIC", true);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Wait for the call to be accepted. in your area now {i} auto mechanics. To cancel the call, use /cmechanic", 3000);
                }
                else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"There are currently no auto mechanics in your area. Try another time", 3000);
            }
            else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You've already called the auto mechanic. To cancel write /cmechanic", 3000);
        }

        public static void buyFuel(Player player, int fuel)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (Main.Players[player].WorkID != 8 || !player.GetData<bool>("ON_WORK") || !player.IsInVehicle || player.GetData<Vehicle>("WORK") != player.Vehicle)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны работать автомехаником и находиться в рабочей машине", 3000);
                return;
            }
            if (player.GetData<int>("BIZ_ID") == -1 || BusinessManager.BizList[player.GetData<int>("BIZ_ID")].Type != 1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You must be at the gas station", 3000);
                return;
            }
            if (fuel <= 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Enter correct data", 3000);
                return;
            }
            Business biz = BusinessManager.BizList[player.GetData<int>("BIZ_ID")];
            if (Main.Players[player].Money < biz.Products[0].Price * fuel)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Insufficient funds", 3000);
                return;
            }
            if (player.Vehicle.GetSharedData<int>("FUELTANK") + fuel > 1000)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Gasoline tank is full", 3000);
                return;
            }
            if (!BusinessManager.takeProd(biz.ID, fuel, biz.Products[0].Name, biz.Products[0].Price * fuel))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Not enough fuel at the gas station", 3000);
                return;
            }
            MoneySystem.Wallet.Change(player, -biz.Products[0].Price * fuel);
            GameLog.Money($"player({Main.Players[player].UUID})", $"biz({biz.ID})", biz.Products[0].Price * fuel, $"mechanicBuyFuel");
            player.Vehicle.SetSharedData("FUELTANK", player.Vehicle.GetSharedData<int>("FUELTANK") + fuel);
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You have topped up the tank in your work car to {player.Vehicle.GetSharedData<int>("FUELTANK")}л", 3000);
        }

        public static void mechanicFuel(Player player, Player target, int fuel, int pricePerLitr)
        {
            if (Main.Players[player].WorkID != 8 || !player.GetData<bool>("ON_WORK"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You do not work as an auto mechanic", 3000);
                return;
            }
            if (!player.IsInVehicle || !player.Vehicle.HasData("TYPE") || player.Vehicle.GetData<string>("TYPE") != "MECHANIC")
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You must be in a working vehicle", 3000);
                return;
            }
            if (!target.IsInVehicle)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The player must be in the vehicle", 3000);
                return;
            }
            if (player.Vehicle.Position.DistanceTo(target.Vehicle.Position) > 5)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The player is too far away from you", 3000);
                return;
            }
            if (fuel < 1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You cannot sell less than a liter", 3000);
                return;
            }
            if (pricePerLitr < 2 || pricePerLitr > 10)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You can set the price from $2 to $10 per liter", 3000);
                return;
            }
            if (Main.Players[target].Money < pricePerLitr * fuel)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Player doesn't have enough money", 3000);
                return;
            }

            target.SetData("MECHANIC", player);
            target.SetData("MECHANIC_PRICE", pricePerLitr);
            target.SetData("MECHANIC_FEUL", fuel);
            Trigger.ClientEvent(target, "openDialog", "FUEL_CAR", $"Player ({player.Value}) offered to refuel your vehicle for {fuel}l for ${fuel * pricePerLitr}");

            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"You offered the player ({target.Value}) refuel the transport {fuel}l for {fuel * pricePerLitr}$.", 3000);
        }

        public static void mechanicPayFuel(Player player)

        {
            if (!BusinessManager.ElectroCar.Contains((VehicleHash)player.Vehicle.Model)) return;
            if (!player.IsInVehicle)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You must be in a vehicle", 3000);
                return;
            }

            var price = NAPI.Data.GetEntityData(player, "MECHANIC_PRICE");
            var fuel = NAPI.Data.GetEntityData(player, "MECHANIC_FEUL");
            if (Main.Players[player].Money < price * fuel)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You don't have enough funds", 3000);
                return;
            }

            Player driver = NAPI.Data.GetEntityData(player, "MECHANIC");

            if (!driver.IsInVehicle || !driver.Vehicle.HasData("TYPE") || driver.Vehicle.GetData<string>("TYPE") != "MECHANIC")
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The mechanic must be in the vehicle", 3000);
                return;
            }

            if (driver.Vehicle.GetSharedData<int>("FUELTANK") < fuel)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The mechanic doesn't have enough fuel to fill you up.", 3000);
                return;
            }

            MoneySystem.Wallet.Change(player, -price * fuel);
            MoneySystem.Wallet.Change(driver, price * fuel);
            GameLog.Money($"player({Main.Players[player].UUID})", $"player({Main.Players[driver].UUID})", price * fuel, $"mechanicFuel");
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You paid for the repair of refueling the vehicle", 3000);
            Notify.Send(driver, NotifyType.Info, NotifyPosition.BottomCenter, $"Player ({player.Value}) paid for refueling", 3000);
            Commands.RPChat("me", driver, $"refueled the vehicle");

            var carFuel = (player.Vehicle.GetSharedData<int>("PETROL") + fuel > player.Vehicle.GetSharedData<int>("MAXPETROL")) ? player.Vehicle.GetSharedData<int>("MAXPETROL") : player.Vehicle.GetSharedData<int>("PETROL") + fuel;
            player.Vehicle.SetSharedData("PETROL", carFuel);
            driver.Vehicle.SetSharedData("FUELTANK", driver.Vehicle.GetSharedData<int>("FUELTANK") - fuel);
            player.ResetData("MECHANIC_DRIVER");
            driver.ResetData("MECHANIC_CLIENT");
            try
            {
                NAPI.ColShape.DeleteColShape(orderCols[player]);
                orderCols.Remove(player);
            }
            catch { }
        }
    }
}
