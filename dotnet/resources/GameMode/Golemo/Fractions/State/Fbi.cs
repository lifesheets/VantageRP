using System.Collections.Generic;
using GTANetworkAPI;
using Golemo.Core;
using GolemoSDK;
using Golemo.GUI;
using System;
using System.Linq;

namespace Golemo.Fractions
{
    class Fbi : Script
    {
        private static Dictionary<int, ColShape> Cols = new Dictionary<int, ColShape>();
        public static Vector3 EnterFBI = new Vector3(104.56081, -744.52924, -1044.634758);
        private static Vector3 ExitFBI = new Vector3(107.1693, -744.6852, -1044.634758);
        private static List<Vector3> fbiCheckpoints = new List<Vector3>()
        {
            new Vector3(2518.5847, -340.4426, 100.773346), // duty          0
            new Vector3(-10000,-1000,-1000), // 49 floor       1
            new Vector3(-10000,-1000,-1000), // 49 floor to 53          2
            new Vector3(-10000,-1000,-1000), // 53 floor          3
            new Vector3(-10000,-1000,-1000), // roof          4
            new Vector3(2524.9958, -333.06476, 100.77326), // gun menu           5
            new Vector3(2504.315, -433.08472, 105.79175), // elevator 1.1        6
            new Vector3(2504.301, -433.17325, 97.992294), // elevator 1.2       7
            new Vector3(-10000,-1000,-1000),  // warg mode    8
            new Vector3(2525.711, -342.75302, 100.773346), // fbi stock     9
            new Vector3(2504.3083, -342.10947, 92.97236), // elevator 2.1     10
            new Vector3(2504.3083, -342.10794, 100.766945), // elevator 2.2     11
            new Vector3(2504.3083, -342.11108, 104.55668), // elevator 2.3    12
        };
        public static bool warg_mode = false;

        private static nLog Log = new nLog("FIB");

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                foreach (Vector3 vec in fbiCheckpoints)
                {
                    NAPI.Marker.CreateMarker(1, vec + new Vector3(0, 0, 0.0), new Vector3(), new Vector3(), 0.8f, new Color(67, 140, 239), false, 0);
                }

                var fibped = NAPI.ColShape.CreateCylinderColShape(new Vector3(2511.3296, -428.29663, 93.46218), 1, 2, 0);
                fibped.OnEntityEnterColShape += (shape, player) =>
                {
                    try
                    {
                        player.SetData("INTERACTIONCHECK", 915);
                    }
                    catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); }
                };
                fibped.OnEntityExitColShape += (shape, player) =>
                {
                    try
                    {
                        player.SetData("INTERACTIONCHECK", 0);
                    }
                    catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); }
                };

                #region cols
                Cols.Add(0, NAPI.ColShape.CreateCylinderColShape(fbiCheckpoints[0], 1, 2, 0)); // duty fbi
                Cols[0].SetData("INTERACT", 20);
                Cols[0].OnEntityEnterColShape += fbiShape_onEntityEnterColShape;
                Cols[0].OnEntityExitColShape += fbiShape_onEntityExitColShape;

                Cols.Add(1, NAPI.ColShape.CreateCylinderColShape(EnterFBI, 1, 2, 0)); // fbi enter
                Cols[1].SetData("INTERACT", 21);
                Cols[1].OnEntityEnterColShape += fbiShape_onEntityEnterColShape;
                Cols[1].OnEntityExitColShape += fbiShape_onEntityExitColShape;
                NAPI.Marker.CreateMarker(21, EnterFBI + new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 0.8f, new Color(255, 255, 255, 60));

                Cols.Add(2, NAPI.ColShape.CreateCylinderColShape(ExitFBI, 1, 2, 0)); // fbi exit
                Cols[2].SetData("INTERACT", 22);
                Cols[2].OnEntityEnterColShape += fbiShape_onEntityEnterColShape;
                Cols[2].OnEntityExitColShape += fbiShape_onEntityExitColShape;
                NAPI.Marker.CreateMarker(21, ExitFBI + new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 0.8f, new Color(255, 255, 255, 60));

                Cols.Add(3, NAPI.ColShape.CreateCylinderColShape(fbiCheckpoints[1], 1, 2, 0)); // 49 floor to 53
                Cols[3].SetData("INTERACT", 23);
                Cols[3].OnEntityEnterColShape += fbiShape_onEntityEnterColShape;
                Cols[3].OnEntityExitColShape += fbiShape_onEntityExitColShape;

                Cols.Add(5, NAPI.ColShape.CreateCylinderColShape(fbiCheckpoints[3], 1, 2, 0)); // 53 floor to 49
                Cols[5].SetData("INTERACT", 27);
                Cols[5].OnEntityEnterColShape += fbiShape_onEntityEnterColShape;
                Cols[5].OnEntityExitColShape += fbiShape_onEntityExitColShape;

                Cols.Add(6, NAPI.ColShape.CreateCylinderColShape(fbiCheckpoints[4], 1, 2, 0)); // roof
                Cols[6].SetData("INTERACT", 23);
                Cols[6].OnEntityEnterColShape += fbiShape_onEntityEnterColShape;
                Cols[6].OnEntityExitColShape += fbiShape_onEntityExitColShape;

                Cols.Add(7, NAPI.ColShape.CreateCylinderColShape(fbiCheckpoints[5], 1, 2, 0)); // gun menu
                Cols[7].SetData("INTERACT", 24);
                Cols[7].OnEntityEnterColShape += fbiShape_onEntityEnterColShape;
                Cols[7].OnEntityExitColShape += fbiShape_onEntityExitColShape;

                Cols.Add(8, NAPI.ColShape.CreateCylinderColShape(fbiCheckpoints[6], 1, 2, 0)); // 1 floor
                Cols[8].SetData("INTERACT", 23);
                Cols[8].OnEntityEnterColShape += fbiShape_onEntityEnterColShape;
                Cols[8].OnEntityExitColShape += fbiShape_onEntityExitColShape;

                Cols.Add(9, NAPI.ColShape.CreateCylinderColShape(fbiCheckpoints[7], 1, 2, 0)); // garage
                Cols[9].SetData("INTERACT", 23);
                Cols[9].OnEntityEnterColShape += fbiShape_onEntityEnterColShape;
                Cols[9].OnEntityExitColShape += fbiShape_onEntityExitColShape;

                Cols.Add(10, NAPI.ColShape.CreateCylinderColShape(fbiCheckpoints[8], 1, 2, 0)); // warg
                Cols[10].SetData("INTERACT", 46);
                Cols[10].OnEntityEnterColShape += fbiShape_onEntityEnterColShape;
                Cols[10].OnEntityExitColShape += fbiShape_onEntityExitColShape;

                Cols.Add(11, NAPI.ColShape.CreateCylinderColShape(fbiCheckpoints[9], 1, 2, 0)); // stock
                Cols[11].SetData("INTERACT", 61);
                Cols[11].OnEntityEnterColShape += fbiShape_onEntityEnterColShape;
                Cols[11].OnEntityExitColShape += fbiShape_onEntityExitColShape;


                //=================================ELEVATOR
                Cols.Add(12, NAPI.ColShape.CreateCylinderColShape(fbiCheckpoints[10], 1, 2, 0)); // elevator
                Cols[12].SetData("INTERACT", 26);
                Cols[12].OnEntityEnterColShape += fbiShape_onEntityEnterColShape;
                Cols[12].OnEntityExitColShape += fbiShape_onEntityExitColShape;

                Cols.Add(13, NAPI.ColShape.CreateCylinderColShape(fbiCheckpoints[11], 1, 2, 0)); //elevator
                Cols[13].SetData("INTERACT", 26);
                Cols[13].OnEntityEnterColShape += fbiShape_onEntityEnterColShape;
                Cols[13].OnEntityExitColShape += fbiShape_onEntityExitColShape;

                Cols.Add(14, NAPI.ColShape.CreateCylinderColShape(fbiCheckpoints[12], 1, 2, 0)); // elevator
                Cols[14].SetData("INTERACT", 26);
                Cols[14].OnEntityEnterColShape += fbiShape_onEntityEnterColShape;
                Cols[14].OnEntityExitColShape += fbiShape_onEntityExitColShape;
                //========================================
                #endregion
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        public static void openpedmenuFIB(Player player)
        {
            Trigger.ClientEvent(player, "NPC.cameraOn", "FIB", 1500);
            Trigger.ClientEvent(player, "open_FibPedMenu", Main.Players[player].FractionID, Main.Players[player].FirstName, Main.Players[player].LastName, NAPI.Data.GetEntityData(player, "ON_DUTY"));
        }

        public static void interactPressed(Player player, int interact)
        {
            switch (interact)
            {
                case 20:
                    if (Main.Players[player].FractionID == 9)
                    {
                        if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                        {
                            Notify.Error(player, "Вы должны начать рабочий день у стойки с информацией");
                            return;
                        }
                        else
                        {
                            Trigger.ClientEvent(player, "OpenClothesChange", ("Копмлект FIB #1", "Копмлект FIB #2", "Копмлект FIB #3", "Копмлект FIB #4", "Копмлект FIB #5", "Копмлект FIB #6", "Копмлект FIB #7"), "FIB", 9);
                            NAPI.Entity.SetEntityRotation(player, new Vector3(0, 0, -45));
                            NAPI.Entity.SetEntityDimension(player, (uint)(2000 + player.Value));
                        }
                    }
                    else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не сотрудник FIB", 3000);
                    return;
                case 21:
                    if (player.IsInVehicle) return;
                    if (player.HasData("FOLLOWING"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вас кто-то тащит за собой", 3000);
                        return;
                    }
                    NAPI.Entity.SetEntityPosition(player, ExitFBI + new Vector3(0, 0, 1.12));
                    Main.PlayerEnterInterior(player, ExitFBI + new Vector3(0, 0, 1.12));
                    return;
                case 22:
                    if (player.IsInVehicle) return;
                    if (player.HasData("FOLLOWING"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вас кто-то тащит за собой", 3000);
                        return;
                    }
                    NAPI.Entity.SetEntityPosition(player, EnterFBI + new Vector3(0, 0, 1.12));
                    Main.PlayerEnterInterior(player, EnterFBI + new Vector3(0, 0, 1.12));
                    return;
                case 23:
                    if (player.IsInVehicle) return;
                    if (player.HasData("FOLLOWING"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вас кто-то тащит за собой", 3000);
                        return;
                    }
                    OpenFbiLiftMenu(player);
                    return;
                case 24:
                    if (Main.Players[player].FractionID != 9)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не сотрудник FBI", 3000);
                        return;
                    }
                    if (!Stocks.fracStocks[9].IsOpen)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Склад закрыт", 3000);
                        return;
                    }
                    if (!player.GetData<bool>("ON_DUTY"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны начать рабочий день", 3000);
                        return;
                    }
                    OpenFbiGunMenu(player);
                    return;
                case 26:
                    OpenFbiLiftMenu2(player);
                    return;
                case 27:
                    NAPI.Entity.SetEntityPosition(player, fbiCheckpoints[2] + new Vector3(0, 0, 1.12));
                    return;
                case 46:
                    if (Main.Players[player].FractionID != 9)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не сотрудник FBI", 3000);
                        return;
                    }
                    if (!player.GetData<bool>("ON_DUTY"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны начать рабочий день", 3000);
                        return;
                    }
                    if (player.GetData<bool>("IN_CP_MODE"))
                    {
                        Manager.setSkin(player, Main.Players[player].FractionID, Main.Players[player].FractionLVL);
                        player.SetData("IN_CP_MODE", false);
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы переоделись в рабочую форму", 3000);
                    }
                    else
                    {
                        if (!warg_mode)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Не включен режим ЧП", 3000);
                            return;
                        }
                        if (Main.Players[player].Gender)
                        {
                            Customization.SetHat(player, 39, 2);
                            player.SetClothes(11, 53, 1);
                            player.SetClothes(4, 31, 2);
                            player.SetClothes(6, 25, 0);
                            player.SetClothes(9, 28, 9);
                            player.SetClothes(8, 130, 0);
                            player.SetClothes(3, 49, 0);
                        }
                        else
                        {
                            Customization.SetHat(player, 38, 2);
                            player.SetClothes(11, 46, 1);
                            player.SetClothes(4, 30, 2);
                            player.SetClothes(6, 25, 0);
                            player.SetClothes(9, 31, 9);
                            player.SetClothes(8, 160, 0);
                            player.SetClothes(3, 49, 0);
                        }
                        player.SetData("IN_CP_MODE", true);
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы переоделись в спец. форму", 3000);
                    }
                    return;
                case 61:
                    if (Main.Players[player].FractionID != 9)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не сотрудник полиции", 3000);
                        return;
                    }
                    if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны начать рабочий день", 3000);
                        return;
                    }
                    if (!Stocks.fracStocks[9].IsOpen)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Склад закрыт", 3000);
                        return;
                    }
                    if (!Manager.canUseCommand(player, "openweaponstock")) return;
                    player.SetData("ONFRACSTOCK", 9);
                    GUI.Dashboard.OpenOut(player, Stocks.fracStocks[9].Weapons, "Склад оружия", 6);
                    return;
            }
        }

        private void fbiShape_onEntityEnterColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", shape.GetData<int>("INTERACT"));
            }
            catch (Exception ex) { Log.Write("fbiShape_onEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
        }

        private void fbiShape_onEntityExitColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
            }
            catch (Exception ex) { Log.Write("fbiShape_onEntityExitColShape: " + ex.Message, nLog.Type.Error); }
        }

        #region menus
        public static void OpenFbiLiftMenu(Player player)
        {
            Trigger.ClientEvent(player, "openlift", 0, "fbilift");
        }

        public static void OpenFbiLiftMenu2(Player player)
        {
            Trigger.ClientEvent(player, "openlift2", 0, "fbilift2");
        }
        [RemoteEvent("fbilift")]
        public static void callback_fbilift(Player client, int floor)
        {
            try
            {
                if (client.IsInVehicle) return;
                if (client.HasData("FOLLOWING"))
                {
                    Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, $"Вас кто-то тащит за собой", 3000);
                    return;
                }
                switch (floor)
                {
                    case 0: //garage
                        NAPI.Entity.SetEntityPosition(client, fbiCheckpoints[7] + new Vector3(0, 0, 1.12));
                        Main.PlayerEnterInterior(client, fbiCheckpoints[7] + new Vector3(0, 0, 1.12));
                        return;
                    case 1: //floor1
                        NAPI.Entity.SetEntityPosition(client, fbiCheckpoints[6] + new Vector3(0, 0, 1.12));
                        Main.PlayerEnterInterior(client, fbiCheckpoints[6] + new Vector3(0, 0, 1.12));
                        return;
                }
            }
            catch (Exception e) { Log.Write("fbilift: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("fbilift2")]
        public static void callback_fbilift2(Player client, int floor)
        {
            try
            {
                if (client.IsInVehicle) return;
                if (client.HasData("FOLLOWING"))
                {
                    Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, $"Вас кто-то тащит за собой", 3000);
                    return;
                }
                switch (floor)
                {
                    case 0: //garage
                        NAPI.Entity.SetEntityPosition(client, fbiCheckpoints[10] + new Vector3(0, 0, 1.12));
                        Main.PlayerEnterInterior(client, fbiCheckpoints[10] + new Vector3(0, 0, 1.12));
                        return;
                    case 1: //floor1
                        NAPI.Entity.SetEntityPosition(client, fbiCheckpoints[11] + new Vector3(0, 0, 1.12));
                        Main.PlayerEnterInterior(client, fbiCheckpoints[11] + new Vector3(0, 0, 1.12));
                        return;
                    case 2: //floor49
                        NAPI.Entity.SetEntityPosition(client, fbiCheckpoints[12] + new Vector3(0, 0, 1.12));
                        Main.PlayerEnterInterior(client, fbiCheckpoints[12] + new Vector3(0, 0, 1.12));
                        return;
                }
            }
            catch (Exception e) { Log.Write("fbilift2: " + e.Message, nLog.Type.Error); }
        }

        public static void OpenFbiGunMenu(Player player)
        {
            Trigger.ClientEvent(player, "OpenStock_FIB");
            Trigger.ClientEvent(player, "NPC.cameraOn", "FIBStock", 1500);
        }
        [RemoteEvent("FIBGunBuy")]
        public static void callback_fbiguns(Player player, int index)
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
                        if (Fractions.Stocks.fracStocks[9].Materials < 120)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно материалов на складе", 3000);
                            return;
                        }
                        var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.Pistol, 1));
                        if (tryAdd == -1 || tryAdd > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Not enough space in the inventory", 2000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[9].Materials -= 120;
                        Fractions.Stocks.fracStocks[9].UpdateLabel();
                        Weapons.GiveWeapon(player, ItemType.Pistol, Weapons.GetSerialFrac(true, 9));
                        Trigger.ClientEvent(player, "acguns");
                        Notify.Succ(player, "Вы взяли Пистолет");
                        return;
                    case 1:
                        if (Main.Players[player].FractionLVL < 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому предмету", 3000);
                            return;
                        }
                        if (Fractions.Stocks.fracStocks[9].Materials < 150)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно материалов на складе", 3000);
                            return;
                        }
                        var tryAdd1 = nInventory.TryAdd(player, new nItem(ItemType.SMG, 1));
                        if (tryAdd1 == -1 || tryAdd1 > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Not enough space in the inventory", 2000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[9].Materials -= 150;
                        Fractions.Stocks.fracStocks[9].UpdateLabel();
                        Weapons.GiveWeapon(player, ItemType.SMG, Weapons.GetSerialFrac(true, 9));
                        Trigger.ClientEvent(player, "acguns");
                        Notify.Succ(player, "Вы взяли Пистолет-Пулемет");
                        return;
                    case 2:
                        if (Main.Players[player].FractionLVL < 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому предмету", 3000);
                            return;
                        }
                        if (Fractions.Stocks.fracStocks[9].Materials < 200)
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
                        Fractions.Stocks.fracStocks[9].Materials -= 200;
                        Fractions.Stocks.fracStocks[9].UpdateLabel();
                        Weapons.GiveWeapon(player, ItemType.CarbineRifle, Weapons.GetSerialFrac(true, 9));
                        Trigger.ClientEvent(player, "acguns");
                        Notify.Succ(player, "Вы взяли Карабиновую Винтовку");
                        return;
                    case 3:
                        if (Main.Players[player].FractionLVL < 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому предмету", 3000);
                            return;
                        }
                        if (Fractions.Stocks.fracStocks[9].Materials < 250)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно материалов на складе", 3000);
                            return;
                        }
                        var tryAdd3 = nInventory.TryAdd(player, new nItem(ItemType.PumpShotgun, 1));
                        if (tryAdd3 == -1 || tryAdd3 > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Not enough space in the inventory", 2000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[9].Materials -= 250;
                        Fractions.Stocks.fracStocks[9].UpdateLabel();
                        Weapons.GiveWeapon(player, ItemType.PumpShotgun, Weapons.GetSerialFrac(true, 9));
                        Trigger.ClientEvent(player, "acguns");
                        Notify.Succ(player, "Вы взяли Помповый дробовик");
                        return;
                    case 4:
                        if (Main.Players[player].FractionLVL < 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому предмету", 3000);
                            return;
                        }
                        if (Fractions.Stocks.fracStocks[9].Materials < 50)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно материалов на складе", 3000);
                            return;
                        }
                        var tryAdd4 = nInventory.TryAdd(player, new nItem(ItemType.PistolAmmo2, 50));
                        if (tryAdd4 == -1 || tryAdd4 > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Not enough space in the inventory", 2000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[9].Materials -= 50;
                        Fractions.Stocks.fracStocks[9].UpdateLabel();
                        nInventory.Add(player, new nItem(ItemType.PistolAmmo2, 50));
                        Trigger.ClientEvent(player, "acguns");
                        Notify.Succ(player, "Вы взяли Патроны 9x19mm");
                        return;
                    case 5:
                        if (Main.Players[player].FractionLVL < 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому предмету", 3000);
                            return;
                        }
                        if (Fractions.Stocks.fracStocks[9].Materials < 50)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно материалов на складе", 3000);
                            return;
                        }
                        var tryAdd5 = nInventory.TryAdd(player, new nItem(ItemType.CarabineAmmo, 50));
                        if (tryAdd5 == -1 || tryAdd5 > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Not enough space in the inventory", 2000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[9].Materials -= 50;
                        Fractions.Stocks.fracStocks[9].UpdateLabel();
                        nInventory.Add(player, new nItem(ItemType.CarabineAmmo, 50));
                        Trigger.ClientEvent(player, "acguns");
                        Notify.Succ(player, "Вы взяли Патроны 56x45mm");
                        return;
                    case 6:
                        if (Main.Players[player].FractionLVL < 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому предмету", 3000);
                            return;
                        }
                        if (Fractions.Stocks.fracStocks[9].Materials < 50)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно материалов на складе", 3000);
                            return;
                        }
                        var tryAdd6 = nInventory.TryAdd(player, new nItem(ItemType.ShotgunsAmmo, 50));
                        if (tryAdd6 == -1 || tryAdd6 > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Not enough space in the inventory", 2000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[9].Materials -= 50;
                        Fractions.Stocks.fracStocks[9].UpdateLabel();
                        nInventory.Add(player, new nItem(ItemType.ShotgunsAmmo, 50));
                        Trigger.ClientEvent(player, "acguns");
                        Notify.Succ(player, "Вы взяли Патроны 12ga Buckshots");
                        return;
                    case 7:
                        if (Main.Players[player].FractionLVL < 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому предмету", 3000);
                            return;
                        }
                        if (Fractions.Stocks.fracStocks[9].Materials < 50)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно материалов на складе", 3000);
                            return;
                        }
                        var tryAdd7 = nInventory.TryAdd(player, new nItem(ItemType.HeavyRiflesAmmo, 50));
                        if (tryAdd7 == -1 || tryAdd7 > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Not enough space in the inventory", 2000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[9].Materials -= 50;
                        Fractions.Stocks.fracStocks[9].UpdateLabel();
                        nInventory.Add(player, new nItem(ItemType.HeavyRiflesAmmo, 50));
                        Trigger.ClientEvent(player, "acguns");
                        Notify.Succ(player, "Вы взяли Патроны 12ga Rifles");
                        return;
                    case 8:
                        if (Main.Players[player].FractionLVL < 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому предмету", 3000);
                            return;
                        }
                        if (Fractions.Stocks.fracStocks[9].Materials < 400)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно материалов на складе", 3000);
                            return;
                        }
                        var tryAdd8 = nInventory.TryAdd(player, new nItem(ItemType.HeavyShotgun, 1));
                        if (tryAdd8 == -1 || tryAdd8 > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Not enough space in the inventory", 2000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[9].Materials -= 400;
                        Fractions.Stocks.fracStocks[9].UpdateLabel();
                        Weapons.GiveWeapon(player, ItemType.HeavyShotgun, Weapons.GetSerialFrac(true, 9));
                        Trigger.ClientEvent(player, "acguns");
                        Notify.Succ(player, "Вы взяли Сайгу");
                        return;
                    case 9:
                        if (Main.Players[player].FractionLVL < 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому предмету", 3000);
                            return;
                        }
                        if (Fractions.Stocks.fracStocks[9].Materials < 100)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно материалов на складе", 3000);
                            return;
                        }
                        var tryAdd9 = nInventory.TryAdd(player, new nItem(ItemType.BodyArmor, 1));
                        if (tryAdd9 == -1 || tryAdd9 > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Not enough space in the inventory", 2000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[9].Materials -= 100;
                        Fractions.Stocks.fracStocks[9].UpdateLabel();
                        nInventory.Add(player, new nItem(ItemType.BodyArmor, 1, 100.ToString()));
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили бронежилет", 3000);
                        return;
                    case 10:
                        if (Main.Players[player].FractionLVL < 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому предмету", 3000);
                            return;
                        }
                        if (Fractions.Stocks.fracStocks[9].Materials < 5000)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно материалов на складе", 3000);
                            return;
                        }
                        var tryAdd10 = nInventory.TryAdd(player, new nItem(ItemType.LSPDDrone, 1));
                        if (tryAdd10 == -1 || tryAdd10 > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Not enough space in the inventory", 2000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[9].Materials -= 5000;
                        Fractions.Stocks.fracStocks[9].UpdateLabel();
                        nInventory.Add(player, new nItem(ItemType.LSPDDrone, 1));
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили Дрон", 3000);
                        return;
                    case 11:
                        if (Main.Players[player].FractionLVL < 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому предмету", 3000);
                            return;
                        }
                        if (Fractions.Stocks.fracStocks[9].Materials < 200)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно материалов на складе", 3000);
                            return;
                        }
                        var tryAdd11 = nInventory.TryAdd(player, new nItem(ItemType.StunGun, 1));
                        if (tryAdd11 == -1 || tryAdd11 > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Not enough space in the inventory", 2000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[9].Materials -= 200;
                        Fractions.Stocks.fracStocks[9].UpdateLabel();
                        Weapons.GiveWeapon(player, ItemType.StunGun, Weapons.GetSerialFrac(true, 9));
                        Trigger.ClientEvent(player, "acguns");
                        Notify.Succ(player, "Вы взяли Тайзер");
                        return;
                    case 12:
                        if (Main.Players[player].FractionLVL < 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому предмету", 3000);
                            return;
                        }
                        if (Fractions.Stocks.fracStocks[9].Materials < 200)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно материалов на складе", 3000);
                            return;
                        }
                        var tryAdd12 = nInventory.TryAdd(player, new nItem(ItemType.HeavySniper, 1));
                        if (tryAdd12 == -1 || tryAdd12 > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Not enough space in the inventory", 2000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[9].Materials -= 200;
                        Fractions.Stocks.fracStocks[9].UpdateLabel();
                        Weapons.GiveWeapon(player, ItemType.HeavySniper, Weapons.GetSerialFrac(true, 9));
                        Trigger.ClientEvent(player, "acguns");
                        Notify.Succ(player, "Вы взяли Тяжелую Снайперскую винтовку");
                        return;
                    case 13:
                        if (Main.Players[player].FractionLVL < 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому предмету", 3000);
                            return;
                        }
                        if (Fractions.Stocks.fracStocks[9].Materials < 200)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно материалов на складе", 3000);
                            return;
                        }
                        var tryAdd13 = nInventory.TryAdd(player, new nItem(ItemType.SniperAmmo, 50));
                        if (tryAdd13 == -1 || tryAdd13 > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Not enough space in the inventory", 2000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[9].Materials -= 200;
                        Fractions.Stocks.fracStocks[9].UpdateLabel();
                        nInventory.Add(player, new nItem(ItemType.SniperAmmo, 10));
                        Trigger.ClientEvent(player, "acguns");
                        Notify.Succ(player, "Вы взяли Патроны 50BMG");
                        return;
                }
            }
            catch (Exception e) { Log.Write("Fbigun: " + e.Message, nLog.Type.Error); }
        }
        #endregion
    }
}
