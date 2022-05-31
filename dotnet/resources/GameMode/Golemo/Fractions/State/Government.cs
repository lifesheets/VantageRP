using System;
using System.Collections.Generic;
using GTANetworkAPI;
using Golemo.Core;
using GolemoSDK;
using Golemo.GUI;

namespace Golemo.Fractions
{
    class Cityhall : Script
    {
        private static nLog Log = new nLog("Cityhall");
        public static int lastHourTax = 0;
        public static int PriceForFishLic = 5000;
        public static int canGetMoney = 999999;

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStartHandler()
        {
            try
            {
                var govped = NAPI.ColShape.CreateCylinderColShape(new Vector3(-555.069, -186.90717, 37.10113), 1, 2, 0);
                govped.OnEntityEnterColShape += (shape, player) =>
                {
                    try
                    {
                        player.SetData("INTERACTIONCHECK", 913);
                    }
                    catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); }
                };
                govped.OnEntityExitColShape += (shape, player) =>
                {
                    try
                    {
                        player.SetData("INTERACTIONCHECK", 0);
                    }
                    catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); }
                };

                Cols.Add(0, NAPI.ColShape.CreateCylinderColShape(CityhallChecksCoords[0], 1f, 2, 0)); // Оружейка
                Cols[0].OnEntityEnterColShape += city_OnEntityEnterColShape;
                Cols[0].OnEntityExitColShape += city_OnEntityExitColShape;
                Cols[0].SetData("INTERACT", 9);

                Cols.Add(1, NAPI.ColShape.CreateCylinderColShape(CityhallChecksCoords[1], 1f, 2, 0)); // Раздевалка
                Cols[1].OnEntityEnterColShape += city_OnEntityEnterColShape;
                Cols[1].OnEntityExitColShape += city_OnEntityExitColShape;
                Cols[1].SetData("INTERACT", 1);
                //NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~w~Переодеться"), new Vector3(CityhallChecksCoords[1].X, CityhallChecksCoords[1].Y, CityhallChecksCoords[1].Z + 0.7), 5F, 0.4F, 0, new Color(255, 255, 255), false, 0);
                /*int door = 0;
                for (int i = 4; i < 6; i++)
                {
                    Cols.Add(i, NAPI.ColShape.CreateCylinderColShape(CityhallChecksCoords[i], 1, 2, 0));
                    Cols[i].OnEntityEnterColShape += city_OnEntityEnterColShape;
                    Cols[i].OnEntityExitColShape += city_OnEntityExitColShape;
                    Cols[i].SetData("INTERACT", 3);
                    Cols[i].SetData("DOOR", door);
                    door++;
                }*/

                Cols.Add(6, NAPI.ColShape.CreateCylinderColShape(new Vector3(255.2283, 223.976, 102.3932), 3, 2, 0));
                Cols[6].OnEntityEnterColShape += city_OnEntityEnterColShape;
                Cols[6].OnEntityExitColShape += city_OnEntityExitColShape;
                Cols[6].SetData("INTERACT", 4);

                NAPI.Marker.CreateMarker(1, CityhallChecksCoords[0] + new Vector3(0, 0, 0.0), new Vector3(), new Vector3(), 0.8f, new Color(67, 140, 239), false, 0);
                NAPI.Marker.CreateMarker(1, CityhallChecksCoords[1] + new Vector3(0, 0, 0.0), new Vector3(), new Vector3(), 0.8f, new Color(67, 140, 239), false, 0);
                NAPI.Marker.CreateMarker(1, CityhallChecksCoords[6] + new Vector3(0, 0, 0.0), new Vector3(), new Vector3(), 0.8f, new Color(67, 140, 239), false, 0);

                Cols.Add(7, NAPI.ColShape.CreateCylinderColShape(CityhallChecksCoords[6], 1f, 2, 0)); // Оружейка
                Cols[7].OnEntityEnterColShape += city_OnEntityEnterColShape;
                Cols[7].OnEntityExitColShape += city_OnEntityExitColShape;
                Cols[7].SetData("INTERACT", 62);
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~w~Склад Government"), new Vector3(CityhallChecksCoords[6].X, CityhallChecksCoords[6].Y, CityhallChecksCoords[6].Z + 0.7), 5F, 0.4F, 0, new Color(255, 255, 255));

                NAPI.Object.CreateObject(0x4f97336b, new Vector3(260.651764, 203.230209, 106.432785), new Vector3(0, 0, 160.003571), 255, 0);
                NAPI.Object.CreateObject(0x4f97336b, new Vector3(258.209259, 204.120041, 106.432785), new Vector3(0, 0, -20.0684872), 255, 0);

                NAPI.Object.CreateObject(0x4f97336b, new Vector3(259.09613, 212.803894, 106.432793), new Vector3(0, 0, 70.0000153), 255, 0);
                NAPI.Object.CreateObject(0x4f97336b, new Vector3(259.985962, 215.246399, 106.432793), new Vector3(0, 0, -109.999962), 255, 0);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT\"FRACTIONS_CITYHALL\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

        public static void openpedmenuGov(Player player)
        {
            Trigger.ClientEvent(player, "NPC.cameraOn", "Government", 1500);
            Trigger.ClientEvent(player, "open_GovPedMenu", Main.Players[player].FractionID, Main.Players[player].FirstName, Main.Players[player].LastName, PriceForFishLic, NAPI.Data.GetEntityData(player, "ON_DUTY"));
        }

        private static Dictionary<int, ColShape> Cols = new Dictionary<int, ColShape>();
        public static List<Vector3> CityhallChecksCoords = new List<Vector3>
        {
            new Vector3(-570.3558, -202.66745, 37.048862), // Выдача пушук
            new Vector3(-571.0057, -195.54567, 37.04884), // раздевалка 
            new Vector3(-545.0524, -204.0801, 37.09514), // main door enter  БЛИП
            new Vector3(233.312, 216.0169, 105.1667), // main door exit          Нон юзед
            new Vector3(256.9124, 220.4567, 105.2864), // door 1
            new Vector3(265.8495, 218.1592, 109.283), // door 2
            new Vector3(-568.80365, -206.08519, 37.04885), // Склад инвентарь
        };

        private void city_OnEntityEnterColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", shape.GetData<int>("INTERACT"));
                if (shape.HasData("DOOR")) NAPI.Data.SetEntityData(entity, "DOOR", shape.GetData<int>("DOOR"));
            }
            catch (Exception e) { Log.Write("city_OnEntityEnterColShape: " + e.Message, nLog.Type.Error); }
        }

        private void city_OnEntityExitColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
            }
            catch (Exception e) { Log.Write("city_OnEntityExitColShape: " + e.Message, nLog.Type.Error); }
        }

        public static void interactPressed(Player player, int interact)
        {
            switch (interact)
            {
                case 3:
                    if (Main.Players[player].FractionID == 6 && Main.Players[player].FractionLVL > 1)
                    {
                        Doormanager.SetDoorLocked(player.GetData<int>("DOOR"), !Doormanager.GetDoorLocked(player.GetData<int>("DOOR")), 0);
                        string msg = "Вы открыли дверь";
                        if (Doormanager.GetDoorLocked(player.GetData<int>("DOOR"))) msg = "Вы закрыли дверь";
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, msg, 3000);
                    }
                    return;
                case 4:
                    SafeMain.OpenSafedoorMenu(player);
                    return;
                case 5:
                    if (player.IsInVehicle) return;
                    if (player.HasData("FOLLOWING"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Someone drags you after him", 3000);
                        return;
                    }
                    if (player.Position.Z < 50)
                    {
                        NAPI.Entity.SetEntityPosition(player, CityhallChecksCoords[3] + new Vector3(0, 0, 1.12));
                        Main.PlayerEnterInterior(player, CityhallChecksCoords[3] + new Vector3(0, 0, 1.12));
                    }
                    else
                    {
                        NAPI.Entity.SetEntityPosition(player, CityhallChecksCoords[2] + new Vector3(0, 0, 1.12));
                        Main.PlayerEnterInterior(player, CityhallChecksCoords[2] + new Vector3(0, 0, 1.12));
                    }
                    return;
                case 62:
                    if (Main.Players[player].FractionID != 6)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You are not an employee of the city hall", 3000);
                        return;
                    }
                    if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You must start a working day", 3000);
                        return;
                    }
                    if (!Stocks.fracStocks[6].IsOpen)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The warehouse is closed", 3000);
                        return;
                    }
                    if (!Manager.canUseCommand(player, "openweaponstock")) return;
                    player.SetData("ONFRACSTOCK", 6);
                    GUI.Dashboard.OpenOut(player, Stocks.fracStocks[6].Weapons, "Склад оружия", 6);
                    return;
            }
        }

        [RemoteEvent("SetEntityDimensionOnClient")]
        public static void SetEntityDimensionOnClient(Player player, uint dim)
        {
            NAPI.Entity.SetEntityDimension(player, dim);
        }

        [RemoteEvent("StartWorkDayFraction")]
        public static void StartWorkDayFraction(Player player, uint dim)
        {
            NAPI.Entity.SetEntityDimension(player, dim);
        }

        [RemoteEvent("StartWorkDayGov")]
        public static void StartWorkDayGov(Player player)
        {
            if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
            {
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You started a working day", 3000);
                NAPI.Data.SetEntityData(player, "ON_DUTY", true);
                return;
            }
            else
            {
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You finished the working day", 3000);
                Customization.ApplyCharacter(player);
                NAPI.Data.SetEntityData(player, "ON_DUTY", false);
                return;
            }
        }

        [RemoteEvent("BuyFishLic")]
        public static void BuyFishLic(Player player)
        {
            MoneySystem.Wallet.Change(player, -PriceForFishLic);
            Main.Players[player].Licenses[8] = true;
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You bought a fishing license", 3000);
            return;
        }
        public static void beginWorkDay(Player player)
        {
            if (Main.Players[player].FractionID == 6)
            {
                if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                {
                    Notify.Error(player, "You should start a working day at the counter with information");
                    return;
                }
                else
                {
                    Trigger.ClientEvent(player, "OpenClothesChange", ("Копмлект GOV #1", "Копмлект GOV #2", "Копмлект GOV #3", "Копмлект GOV #4", "Копмлект GOV #5", "Копмлект GOV #6", "Копмлект GOV #7", "Копмлект GOV #8", "Копмлект GOV #9"), "Government", 6);
                    NAPI.Entity.SetEntityRotation(player, new Vector3(0, 0, 117));
                    NAPI.Entity.SetEntityDimension(player, (uint)(2000 + player.Value));
                }
            }
            else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You are not a Government employee", 3000);
        }

        #region menu
        public static void OpenCityhallGunMenu(Player player)
        {

            if (Main.Players[player].FractionID != 6)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You are not an employee of the city hall", 3000);
                return;
            }
            if (!Stocks.fracStocks[6].IsOpen)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The warehouse is closed", 3000);
                return;
            }
            if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You must start a working day", 3000);
                return;
            }
            Trigger.ClientEvent(player, "OpenStock_GOV");
            Trigger.ClientEvent(player, "NPC.cameraOn", "GOVStock", 1500);
        }
        [RemoteEvent("TakeGovGunStock")]
        public static void callback_cityhallGuns(Player player, int index)
        {
            try
            {
                switch (index)
                {
                    case 0:
                        if (Main.Players[player].FractionLVL < 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому предмету", 3000);
                            return;
                        }
                        if (Fractions.Stocks.fracStocks[6].Materials < 200)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно материалов на складе", 3000);
                            return;
                        }
                        var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.StunGun, 1));
                        if (tryAdd == -1 || tryAdd > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Not enough space in the inventory", 2000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[6].Materials -= 200;
                        Fractions.Stocks.fracStocks[6].UpdateLabel();
                        Weapons.GiveWeapon(player, ItemType.StunGun, Weapons.GetSerialFrac(true, 6));
                        Trigger.ClientEvent(player, "acguns");
                        Notify.Succ(player, "Вы взяли тайзер");
                        return;
                    case 1:
                        if (Main.Players[player].FractionLVL < 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому предмету", 3000);
                            return;
                        }
                        if (Fractions.Stocks.fracStocks[6].Materials < 120)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно материалов на складе", 3000);
                            return;
                        }
                        var tryAdd1 = nInventory.TryAdd(player, new nItem(ItemType.Pistol, 1));
                        if (tryAdd1 == -1 || tryAdd1 > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Not enough space in the inventory", 2000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[6].Materials -= 120;
                        Fractions.Stocks.fracStocks[6].UpdateLabel();
                        Weapons.GiveWeapon(player, ItemType.Pistol, Weapons.GetSerialFrac(true, 6));
                        Trigger.ClientEvent(player, "acguns");
                        Notify.Succ(player, "Вы взяли Пистолет");
                        return;
                    case 2:
                        if (Main.Players[player].FractionLVL < 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому предмету", 3000);
                            return;
                        }
                        if (Fractions.Stocks.fracStocks[6].Materials < 200)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно материалов на складе", 3000);
                            return;
                        }
                        var tryAdd2 = nInventory.TryAdd(player, new nItem(ItemType.CarbineRifle, 1));
                        if (tryAdd2 == -1 || tryAdd2 > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Not enough space in the inventory", 2000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[6].Materials -= 200;
                        Fractions.Stocks.fracStocks[6].UpdateLabel();
                        Weapons.GiveWeapon(player, ItemType.CarbineRifle, Weapons.GetSerialFrac(true, 6));
                        Trigger.ClientEvent(player, "acguns");
                        Notify.Succ(player, "Вы взяли Карабиновую винтовку");
                        return;
                    case 3:
                        if (Main.Players[player].FractionLVL < 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому предмету", 3000);
                            return;
                        }
                        if (Fractions.Stocks.fracStocks[6].Materials < 50)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно материалов на складе", 3000);
                            return;
                        }
                        var tryAdd3 = nInventory.TryAdd(player, new nItem(ItemType.PistolAmmo2, 50));
                        if (tryAdd3 == -1 || tryAdd3 > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Not enough space in the inventory", 2000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[6].Materials -= 50;
                        Fractions.Stocks.fracStocks[6].UpdateLabel();
                        nInventory.Add(player, new nItem(ItemType.PistolAmmo2, 50));
                        Trigger.ClientEvent(player, "acguns");
                        Notify.Succ(player, "Вы взяли патроны 9x19mm");
                        return;
                    case 4:
                        if (Main.Players[player].FractionLVL < 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому предмету", 3000);
                            return;
                        }
                        if (Fractions.Stocks.fracStocks[6].Materials < 50)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно материалов на складе", 3000);
                            return;
                        }
                        var tryAdd4 = nInventory.TryAdd(player, new nItem(ItemType.CarabineAmmo, 50));
                        if (tryAdd4 == -1 || tryAdd4 > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Not enough space in the inventory", 2000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[6].Materials -= 50;
                        Fractions.Stocks.fracStocks[6].UpdateLabel();
                        nInventory.Add(player, new nItem(ItemType.CarabineAmmo, 50));
                        Trigger.ClientEvent(player, "acguns");
                        Notify.Succ(player, "Вы взяли патроны 56x45mm");
                        return;
                    case 5:
                        if (Main.Players[player].FractionLVL < 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому предмету", 3000);
                            return;
                        }
                        if (Fractions.Stocks.fracStocks[6].Materials < 100)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно материалов на складе", 3000);
                            return;
                        }
                        var tryAdd5 = nInventory.TryAdd(player, new nItem(ItemType.BodyArmor, 1));
                        if (tryAdd5 == -1 || tryAdd5 > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Not enough space in the inventory", 2000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[6].Materials -= 100;
                        Fractions.Stocks.fracStocks[6].UpdateLabel();
                        nInventory.Add(player, new nItem(ItemType.BodyArmor, 1, 100.ToString()));
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили бронежилет", 3000);
                        return;
                    case 6:
                        if (Main.Players[player].FractionLVL < 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому предмету", 3000);
                            return;
                        }
                        if (Fractions.Stocks.fracStocks[6].Materials < 5000)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно материалов на складе", 3000);
                            return;
                        }
                        var tryAdd6 = nInventory.TryAdd(player, new nItem(ItemType.LSPDDrone, 1));
                        if (tryAdd6 == -1 || tryAdd6 > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Not enough space in the inventory", 2000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[6].Materials -= 5000;
                        Fractions.Stocks.fracStocks[6].UpdateLabel();
                        nInventory.Add(player, new nItem(ItemType.LSPDDrone, 1));
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили Дрон", 3000);
                        return;
                }
            }
            catch (Exception e) { Log.Write("Govgun: " + e.Message, nLog.Type.Error); }
        }
        #endregion
    }
}
