using System.Collections.Generic;
using GTANetworkAPI;
using Golemo.Core;
using GolemoSDK;
using System;

namespace Golemo.Fractions
{
    class Gangs : Script
    {
        private static nLog Log = new nLog("Gangs");

        public static Dictionary<int, Vector3> ExitPoints = new Dictionary<int, Vector3>()
        {
            /*{ 1, new Vector3(-201.7147, -1627.962, -138.664788) },
            { 2, new Vector3(82.57095, -1958.607, -123.41236) },
            { 3, new Vector3(1420.487, -1497.264, -207.8639) },
            { 4, new Vector3(892.4592, -2168.068, -100.921189) },
            { 5, new Vector3(484.9963, -1536.083, -133.22089) },*/
        };

        public static List<Vector3> DrugPoints = new List<Vector3>()
        {
            /*new Vector3(8.621573, 3701.914, 39.51624),
            new Vector3(3804.169, 4444.753, 3.977164),*/
        };

        [ServerEvent(Event.ResourceStart)]
        public void Event_OnResourceStart()
        {
            try
            {
                var BallasCraft = NAPI.ColShape.CreateCylinderColShape(new Vector3(-76.67501, -1812.6608, 19.68730), 1, 2, 0);
                BallasCraft.OnEntityEnterColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 941); }  catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); } };
                BallasCraft.OnEntityExitColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 0); } catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); } };

                var GrooveCraft = NAPI.ColShape.CreateCylinderColShape(new Vector3(-240.0167, -1496.9425, 26.852406), 1, 2, 0);
                GrooveCraft.OnEntityEnterColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 940); } catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); } };
                GrooveCraft.OnEntityExitColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 0); } catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); } };

                var VagosCraft = NAPI.ColShape.CreateCylinderColShape(new Vector3(961.9523, -1841.8513, 30.159239), 1, 2, 0);
                VagosCraft.OnEntityEnterColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 942); } catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); } };
                VagosCraft.OnEntityExitColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 0); } catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); } };

                var CripsCraft = NAPI.ColShape.CreateCylinderColShape(new Vector3(1443.818, -1492.9783, 66.53294), 1, 2, 0);
                CripsCraft.OnEntityEnterColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 943); } catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); } };
                CripsCraft.OnEntityExitColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 0); } catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); } };

                var BloodsCraft = NAPI.ColShape.CreateCylinderColShape(new Vector3(515.3959, -1340.065, 28.253294), 1, 2, 0);
                BloodsCraft.OnEntityEnterColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 944); } catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); } };
                BloodsCraft.OnEntityExitColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 0); } catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); } };

                var Ballasped = NAPI.ColShape.CreateCylinderColShape(new Vector3(-96.057335, -1797.3448, 27.87421), 1, 2, 0);
                Ballasped.OnEntityEnterColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 934); } catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); } };
                Ballasped.OnEntityExitColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 0); } catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); } };

                var Groove = NAPI.ColShape.CreateCylinderColShape(new Vector3(-255.18102, -1520.1024, 30.440317), 1, 2, 0);
                Groove.OnEntityEnterColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 935); } catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); } };
                Groove.OnEntityExitColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 0); } catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); } };

                var Vagos = NAPI.ColShape.CreateCylinderColShape(new Vector3(976.22015, -1834.1323, 34.987488), 1, 2, 0);
                Vagos.OnEntityEnterColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 936); } catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); } };
                Vagos.OnEntityExitColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 0); } catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); } };

                var Crips = NAPI.ColShape.CreateCylinderColShape(new Vector3(1435.455, -1491.4196, 62.4917), 1, 2, 0);
                Crips.OnEntityEnterColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 937); } catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); } };
                Crips.OnEntityExitColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 0); } catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); } };

                var Bloods = NAPI.ColShape.CreateCylinderColShape(new Vector3(472.02426, -1308.6912, 28.115564), 1, 2, 0);
                Bloods.OnEntityEnterColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 938); } catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); } };
                Bloods.OnEntityExitColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 0); } catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); } };

                var RMafia = NAPI.ColShape.CreateCylinderColShape(new Vector3(-1368.6532, 83.98657, 48.511635), 1, 2, 0);
                RMafia.OnEntityEnterColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 953); } catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); } };
                RMafia.OnEntityExitColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 0); } catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); } };

                var RMafiaCraft = NAPI.ColShape.CreateCylinderColShape(new Vector3(-1351.5812, 67.17415, 54.12777), 1, 2, 0);
                RMafiaCraft.OnEntityEnterColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 954); } catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); } };
                RMafiaCraft.OnEntityExitColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 0); } catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); } };

                var LCN = NAPI.ColShape.CreateCylinderColShape(new Vector3(-1884.871, 2048.0312, 139.84718), 1, 2, 0);
                LCN.OnEntityEnterColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 955); } catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); } };
                LCN.OnEntityExitColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 0); } catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); } };

                var LCNCraft = NAPI.ColShape.CreateCylinderColShape(new Vector3(-1876.8888, 2060.102, 144.39247), 1, 2, 0);
                LCNCraft.OnEntityEnterColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 956); } catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); } };
                LCNCraft.OnEntityExitColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 0); } catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); } };
                
                var KillersCraft = NAPI.ColShape.CreateCylinderColShape(new Vector3(905.77386, -3230.835, -99.41437), 1, 2, 0);
                KillersCraft.OnEntityEnterColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 957); } catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); } };
                KillersCraft.OnEntityExitColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 0); } catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); } };
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }
        public static void OpenCraftGang(Player player, int id)
        {
            if (Main.Players[player].FractionID != id)
            {
                Notify.Error(player, "You are not in this faction");
                return;
            }
            if (!Stocks.fracStocks[Main.Players[player].FractionID].IsOpen)
            {
                Notify.Error(player, "The warehouse is closed");
                return;
            }
            Trigger.ClientEvent(player, "OpenStock_GANG");
                
        }
        public static void OpenBallasPed(Player player)
        {
            Trigger.ClientEvent(player, "NPC.cameraOn", "Ballas", 1500);
            Trigger.ClientEvent(player, "open_GangPedMenu", 2, Main.Players[player].FractionID, "Дарнелл Стивенс");
        }
        public static void OpenGroovePed(Player player)
        {
            Trigger.ClientEvent(player, "NPC.cameraOn", "Groove", 1500);
            Trigger.ClientEvent(player, "open_GangPedMenu", 1, Main.Players[player].FractionID, "Фрэнк Грув");
        }
        public static void OpenVagosPed(Player player)
        {
            Trigger.ClientEvent(player, "NPC.cameraOn", "Vagos", 1500);
            Trigger.ClientEvent(player, "open_GangPedMenu", 3, Main.Players[player].FractionID, "Эмма Вагос");
        }
        public static void OpenCripsPed(Player player)
        {
            Trigger.ClientEvent(player, "NPC.cameraOn", "Crips", 1500);
            Trigger.ClientEvent(player, "open_GangPedMenu", 4, Main.Players[player].FractionID, "Адам Крипс");
        }
        public static void OpenBloodsPed(Player player)
        {
            Trigger.ClientEvent(player, "NPC.cameraOn", "Bloods", 1500);
            Trigger.ClientEvent(player, "open_GangPedMenu", 5, Main.Players[player].FractionID, "Мигель Бладс");
        }
        public static void OpenRMPed(Player player)
        {
            Trigger.ClientEvent(player, "NPC.cameraOn", "RMafia", 1500);
            Trigger.ClientEvent(player, "open_GangPedMenu", 11, Main.Players[player].FractionID, "Дима Медведь");
        }
        public static void OpenLCNPed(Player player)
        {
            Trigger.ClientEvent(player, "NPC.cameraOn", "LCN", 1500);
            Trigger.ClientEvent(player, "open_GangPedMenu", 10, Main.Players[player].FractionID, "Изабелла ЛаКоста");
        }
        [RemoteEvent("GangBuyGuns")]
        public static void callback_gangguns(Player player, int index)
        {
            try
            {
                switch (index)
                {
                    case 0:
                        if (Main.Players[player].FractionLVL < Main.Players[player].FractionID)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому предмету", 3000);
                            return;
                        }
                        if (Fractions.Stocks.fracStocks[Main.Players[player].FractionID].Materials < 120)
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
                        Fractions.Stocks.fracStocks[Main.Players[player].FractionID].Materials -= 120;
                        Fractions.Stocks.fracStocks[Main.Players[player].FractionID].UpdateLabel();
                        Weapons.GiveWeapon(player, ItemType.Pistol, Weapons.GetSerialFrac(true, Main.Players[player].FractionID));
                        Trigger.ClientEvent(player, "acguns");
                        Notify.Succ(player, "Вы взяли Пистолет");
                        return;
                    case 1:
                        if (Main.Players[player].FractionLVL < 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому предмету", 3000);
                            return;
                        }
                        if (Fractions.Stocks.fracStocks[Main.Players[player].FractionID].Materials < 150)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно материалов на складе", 3000);
                            return;
                        }
                        var tryAdd1 = nInventory.TryAdd(player, new nItem(ItemType.CompactRifle, 1));
                        if (tryAdd1 == -1 || tryAdd1 > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Not enough space in the inventory", 2000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[Main.Players[player].FractionID].Materials -= 150;
                        Fractions.Stocks.fracStocks[Main.Players[player].FractionID].UpdateLabel();
                        Weapons.GiveWeapon(player, ItemType.CompactRifle, Weapons.GetSerialFrac(true, Main.Players[player].FractionID));
                        Trigger.ClientEvent(player, "acguns");
                        Notify.Succ(player, "Вы взяли Компактную винтовку");
                        return;
                    case 2:
                        if (Main.Players[player].FractionLVL < 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому предмету", 3000);
                            return;
                        }
                        if (Fractions.Stocks.fracStocks[Main.Players[player].FractionID].Materials < 200)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно материалов на складе", 3000);
                            return;
                        }
                        var tryAdd2 = nInventory.TryAdd(player, new nItem(ItemType.AssaultRifle, 1));
                        if (tryAdd2 == -1 || tryAdd2 > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Not enough space in the inventory", 2000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[Main.Players[player].FractionID].Materials -= 200;
                        Fractions.Stocks.fracStocks[Main.Players[player].FractionID].UpdateLabel();
                        Weapons.GiveWeapon(player, ItemType.AssaultRifle, Weapons.GetSerialFrac(true, Main.Players[player].FractionID));
                        Trigger.ClientEvent(player, "acguns");
                        Notify.Succ(player, "Вы взяли Штурмовую Винтовку");
                        return;
                    case 3:
                        if (Main.Players[player].FractionLVL < 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому предмету", 3000);
                            return;
                        }
                        if (Fractions.Stocks.fracStocks[Main.Players[player].FractionID].Materials < 50)
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
                        Fractions.Stocks.fracStocks[Main.Players[player].FractionID].Materials -= 50;
                        Fractions.Stocks.fracStocks[Main.Players[player].FractionID].UpdateLabel();
                        nInventory.Add(player, new nItem(ItemType.PistolAmmo2, 50));
                        Trigger.ClientEvent(player, "acguns");
                        Notify.Succ(player, "Вы взяли Патроны 9x19mm");
                        return;
                    case 4:
                        if (Main.Players[player].FractionLVL < 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому предмету", 3000);
                            return;
                        }
                        if (Fractions.Stocks.fracStocks[Main.Players[player].FractionID].Materials < 50)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно материалов на складе", 3000);
                            return;
                        }
                        var tryAdd5 = nInventory.TryAdd(player, new nItem(ItemType.RiflesAmmo, 50));
                        if (tryAdd5 == -1 || tryAdd5 > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Not enough space in the inventory", 2000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[Main.Players[player].FractionID].Materials -= 50;
                        Fractions.Stocks.fracStocks[Main.Players[player].FractionID].UpdateLabel();
                        nInventory.Add(player, new nItem(ItemType.RiflesAmmo, 50));
                        Trigger.ClientEvent(player, "acguns");
                        Notify.Succ(player, "Вы взяли Патроны 62x39mm");
                        return;
                    case 5:
                        if (Main.Players[player].FractionLVL < 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому предмету", 3000);
                            return;
                        }
                        if (Fractions.Stocks.fracStocks[Main.Players[player].FractionID].Materials < 50)
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
                        Fractions.Stocks.fracStocks[Main.Players[player].FractionID].Materials -= 50;
                        Fractions.Stocks.fracStocks[Main.Players[player].FractionID].UpdateLabel();
                        nInventory.Add(player, new nItem(ItemType.HeavyRiflesAmmo, 50));
                        Trigger.ClientEvent(player, "acguns");
                        Notify.Succ(player, "Вы взяли Патроны 12ga Rifles");
                        return;
                    case 6:
                        if (Main.Players[player].FractionLVL < 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому предмету", 3000);
                            return;
                        }
                        if (Fractions.Stocks.fracStocks[Main.Players[player].FractionID].Materials < 100)
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
                        Fractions.Stocks.fracStocks[Main.Players[player].FractionID].Materials -= 100;
                        Fractions.Stocks.fracStocks[Main.Players[player].FractionID].UpdateLabel();
                        nInventory.Add(player, new nItem(ItemType.BodyArmor, 1, 50.ToString()));
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили бронежилет", 3000);
                        return;
                    case 7:
                        if (Main.Players[player].FractionLVL < 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому предмету", 3000);
                            return;
                        }
                        if (Fractions.Stocks.fracStocks[Main.Players[player].FractionID].Materials < 200)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно материалов на складе", 3000);
                            return;
                        }
                        var tryAdd10 = nInventory.TryAdd(player, new nItem(ItemType.Gusenberg, 1));
                        if (tryAdd10 == -1 || tryAdd10 > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Not enough space in the inventory", 2000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[Main.Players[player].FractionID].Materials -= 200;
                        Fractions.Stocks.fracStocks[Main.Players[player].FractionID].UpdateLabel();
                        Weapons.GiveWeapon(player, ItemType.Gusenberg, Weapons.GetSerialFrac(true, Main.Players[player].FractionID));
                        Trigger.ClientEvent(player, "acguns");
                        Notify.Succ(player, "Вы взяли Автомат Томпсона");
                        return;
                    case 8:
                        if (Main.Players[player].FractionLVL < 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому предмету", 3000);
                            return;
                        }
                        if (Fractions.Stocks.fracStocks[Main.Players[player].FractionID].Materials < 130)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно материалов на складе", 3000);
                            return;
                        }
                        var tryAdd11 = nInventory.TryAdd(player, new nItem(ItemType.Revolver, 1));
                        if (tryAdd11 == -1 || tryAdd11 > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Not enough space in the inventory", 2000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[Main.Players[player].FractionID].Materials -= 130;
                        Fractions.Stocks.fracStocks[Main.Players[player].FractionID].UpdateLabel();
                        Weapons.GiveWeapon(player, ItemType.Revolver, Weapons.GetSerialFrac(true, Main.Players[player].FractionID));
                        Trigger.ClientEvent(player, "acguns");
                        Notify.Succ(player, "Вы взяли Револьвер");
                        return;
                }
            }
            catch (Exception e) { Log.Write("Fbigun: " + e.Message, nLog.Type.Error); }
        }
    }
}
