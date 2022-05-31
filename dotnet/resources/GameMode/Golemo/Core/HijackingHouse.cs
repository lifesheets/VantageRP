using GTANetworkAPI;
using Golemo.Core;
using Golemo.Houses;
using GolemoSDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Golemo.Scripts
{
    class HijackingHouse : Script
    {
        private static nLog Log = new nLog("HijackingHouse");
        private static Vector3 TakeOrderHijacking = new Vector3(1417.9823, 6344.0547, 22.879662);
        private static Vector3 SurrenderOfTheLoot = new Vector3(1540.2452, 6335.979, 22.954458);
        private static Dictionary<string, List<int>> VehicleHijackingItems = new Dictionary<string, List<int>>();
        private static Random rnd = new Random();
        #region Event`s
        #region ServerEvents
        [ServerEvent(Event.ResourceStart)]
        public static void ResourceStart()
        {
            try
            {
                #region #Creating Marker && Blip
                // NAPI.Marker.CreateMarker(1, TakeOrderHijacking, new Vector3(), new Vector3(), 1f, new Color(255, 225, 64), false);
                ColShape shape = NAPI.ColShape.CreateCylinderColShape(SurrenderOfTheLoot, 7, 3);
                shape.OnEntityEnterColShape += (s, entity) =>
                {
                    try
                    {
                        entity.SetData("INTERACTIONCHECK", 7000);
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
                #endregion
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.ToString(), nLog.Type.Error); }
        }
        [ServerEvent(Event.PlayerDisconnected)]
        public static void Event_PlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                StopWorkHijackingHouse(player, true);
            }
            catch (Exception e) { Log.Write("Event_PlayerDisconnected: " + e.ToString(), nLog.Type.Error); }
        }
        [ServerEvent(Event.PlayerDeath)]
        public static void OnPlayerDeath(Player player, Player killer, uint reason)
        {
            if (Main.Players.ContainsKey(player))
            {
                HijackingHouseData dataHij = HijackingHouseData.HijackingHouseDic.FirstOrDefault(i => i.Player == player);
                if (dataHij != null)
                {
                    if (player.HasSharedData("TakeHijackingItem"))
                    {
                        BasicSync.DetachObject(player);
                        player.StopAnimation();
                        player.ResetSharedData("TakeHijackingItem");
                    }
                }
            }
        }
        #endregion
        [RemoteEvent("PutHijackingItemHouseInVehicle")]
        public static void PutHijackingItemHouseInVehicle(Player player, Vehicle vehicle)
        {
            try
            {
                if (!Main.Players.ContainsKey(player) || !player.HasSharedData("TakeHijackingItem") || !player.HasData("ON_WORK_HIJACKING_HOUSE") || !player.HasData("TakeHijackingItem:Server")) return;
                if (vehicle.HasData("ACCESS") && vehicle.GetData<string>("ACCESS") == "PERSONAL" && VehicleManager.Vehicles.ContainsKey(vehicle.NumberPlate))
                {
                    string holder = VehicleManager.Vehicles[vehicle.NumberPlate].Holder;
                    if (player.Name != holder)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не владелец этого авто.", 3000);
                        return;
                    }
                    BasicSync.DetachObject(player);
                    player.StopAnimation();
                    if (VehicleHijackingItems.ContainsKey(vehicle.NumberPlate))
                    {
                        if (VehicleHijackingItems[vehicle.NumberPlate].Count < 3)
                        {
                            VehicleHijackingItems[vehicle.NumberPlate].Add(player.GetSharedData<int>("TakeHijackingItem"));
                            if (VehicleHijackingItems[vehicle.NumberPlate].Count == 3)
                            {
                                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы успешно загрузили всю мебель, теперь отвезите ее на пункт сдачи.", 3000);
                                Trigger.ClientEvent(player, "createWaypoint", SurrenderOfTheLoot.X, SurrenderOfTheLoot.Y);
                                Trigger.ClientEvent(player, "createCheckpoint", 500, 1, SurrenderOfTheLoot, 7, 0, 220, 220, 0);
                                Trigger.ClientEvent(player, "deleteHijackingHouseBlip");
                                return;
                            }
                        }
                    }
                    else
                        VehicleHijackingItems.Add(vehicle.NumberPlate, new List<int> { player.GetSharedData<int>("TakeHijackingItem") });
                    player.ResetData("TakeHijackingItem:Server");
                    player.ResetSharedData("TakeHijackingItem");
                }
            }
            catch (Exception e) { Log.Write("PutHijackingItemHouseInVehicle: " + e.ToString(), nLog.Type.Error); }
        }
        #endregion
        #region Start && Stop && Sell
        public static void StopWorkHijackingHouse(Player player, bool DelAuto)
        {
            try
            {
                if (!Main.Players.ContainsKey(player) || !player.HasData("ON_WORK_HIJACKING_HOUSE")) return;
                HijackingHouseData dataHij = HijackingHouseData.HijackingHouseDic.FirstOrDefault(i => i.Player == player);
                if (dataHij != null)
                {
                    Trigger.ClientEvent(player, "deleteHijackingHouseBlip");
                    HijackingHouseData.HijackingHouseDic.Remove(dataHij);
                    if (player.HasData("TakeHijackingItem:Server"))
                        player.ResetData("TakeHijackingItem:Server");
                    if (player.HasData("ON_WORK_HIJACKING_HOUSE"))
                        player.ResetData("ON_WORK_HIJACKING_HOUSE");
                    for (int i = 0; i < 4; i++)
                        Trigger.ClientEvent(player, "deleteCheckpoint", 500 + i);
                    BasicSync.DetachObject(player);
                    if (player.HasData("ColShapeHijackingHouse"))
                    {
                        foreach (ColShape shape in player.GetData<List<ColShape>>("ColShapeHijackingHouse"))
                            shape.Delete();
                        player.ResetData("ColShapeHijackingHouse");
                    }
                    if (DelAuto)
                    {
                        List<string> numbers = VehicleManager.getAllPlayerVehicles(player.Name);
                        if (numbers.Count > 0)
                            foreach (string number in numbers)
                                if (VehicleHijackingItems.ContainsKey(number))
                                    VehicleHijackingItems.Remove(number);
                    }
                }
            }
            catch (Exception e) { Log.Write("StopWorkHijackingHouse: " + e.ToString(), nLog.Type.Error); }
        }
        [RemoteEvent("StarHijackingHouse")]
        public static void TakeHouseHijacking(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (player.HasData("ColShapeHijackingHouse") || HijackingHouseData.HijackingHouseDic.Find(i => i.Player == player) != null || player.HasData("ON_WORK_HIJACKING_HOUSE"))
                {
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы уже взяли контракт на ограбление домов", 3000);
                    return;
                }
                if (player.HasData("HijackingPlayer") && player.GetData<bool>("HijackingPlayer") == true)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас уже есть Угон Авто, закончите сначало его.", 3000);
                    return;
                }
                if (Main.Players[player].FractionID == 0 || Fractions.Manager.FractionTypes[Main.Players[player].FractionID] == 2)

                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы не состоите в крайм организации", 3000);
                    return;
                }
                int IDHouses = HouseManager.Houses.ElementAt(rnd.Next(0, HouseManager.Houses.Count)).ID;
                while (HouseManager.Houses[IDHouses].Type == 0 || HouseManager.Houses[IDHouses].Type == 1 || HouseManager.Houses[IDHouses].Type == 7)
                    IDHouses = HouseManager.Houses.ElementAt(rnd.Next(0, HouseManager.Houses.Count)).ID;
                new HijackingHouseData(player, HouseManager.Houses[IDHouses].Type, IDHouses);
            }
            catch (Exception e) { Log.Write("TakeHouseHijacking: " + e.ToString(), nLog.Type.Error); }
        }
        public static void SellHouseHijackingItems(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player) || !player.HasData("ON_WORK_HIJACKING_HOUSE") || !player.IsInVehicle || player.Vehicle == null || !VehicleManager.Vehicles.ContainsKey(player.Vehicle.NumberPlate)) return;
                if (HijackingHouseData.HijackingHouseDic.Find(i => i.Player == player) != null)
                {
                    Vehicle vehicle = player.Vehicle;
                    if (VehicleHijackingItems.ContainsKey(vehicle.NumberPlate))
                    {
                        if (VehicleHijackingItems[vehicle.NumberPlate].Count > 1)
                        {
                            int ToiletPrice = rnd.Next(40000, 80000);
                            int CupBoardPrice = rnd.Next(40000, 80000);
                            int LampPrice = rnd.Next(40000, 80000);
                            int allPrice = 0;
                            if (VehicleHijackingItems[vehicle.NumberPlate].Contains(0))
                                allPrice += ToiletPrice;
                            if (VehicleHijackingItems[vehicle.NumberPlate].Contains(1))
                                allPrice += LampPrice;
                            if (VehicleHijackingItems[vehicle.NumberPlate].Contains(2))
                                allPrice += CupBoardPrice;
                            MoneySystem.Wallet.Change(player, +allPrice);
                            VehicleHijackingItems.Remove(vehicle.NumberPlate);
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы успешно сбыли мебель за {allPrice}$", 3000);
                            StopWorkHijackingHouse(player, false);
                        }
                        else
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У вас нету не одной мебели", 3000);
                            return;
                        }
                    }
                }
            }
            catch (Exception e) { Log.Write("TakeHouseHijacking: " + e.ToString(), nLog.Type.Error); }
        }
        #endregion
        public class HijackingHouseData
        {
            public static List<HijackingHouseData> HijackingHouseDic = new List<HijackingHouseData>();
            public Player Player { get; set; }
            public int TypeHouse { get; set; }
            public int House { get; set; }
            public HijackingHouseData(Player player, int typeHouse, int HouseId)
            {
                Player = player;
                TypeHouse = typeHouse;
                House = HouseId;
                CreateColShapeHouses(player, typeHouse, HouseId);
                HijackingHouseDic.Add(this);
            }
            private void CreateColShapeHouses(Player player, int type, int houseID)
            {
                try
                {
                    Vector3 Toilet = null;
                    Vector3 Lamp = null;
                    Vector3 Cupboard = null;
                    switch (type)
                    {
                        case 2:
                            Toilet = new Vector3(256.38434, -1000.63916, -100.129555);
                            Lamp = new Vector3(260.1199, -995.69885, -100.12866);
                            Cupboard = new Vector3(259.98648, -1004.0469, -100.1286);
                            break;
                        case 3:
                            Toilet = new Vector3(347.10635, -993.60724, -100.316185);
                            Lamp = new Vector3(344.2364, -994.2733, -100.31628);
                            Cupboard = new Vector3(350.64938, -993.5129, -100.316154);
                            break;
                        case 4:
                            Toilet = new Vector3(-43.017193, -583.84467, 77.71288);
                            Lamp = new Vector3(-35.153973, -586.32324, 77.71023);
                            Cupboard = new Vector3(-9.232028, -599.3999, 78.31021);
                            break;
                        case 5:
                            Toilet = new Vector3(-28.264793, -585.38715, 82.78744);
                            Lamp = new Vector3(-35.95627, -574.58746, 82.78744);
                            Cupboard = new Vector3(-28.387394, -590.7647, 88.99833);
                            break;
                        case 6:
                            Toilet = new Vector3(-164.94418, 494.4341, 132.72363);
                            Lamp = new Vector3(-171.12411, 485.7119, 136.32343);
                            Cupboard = new Vector3(-172.81432, 500.57434, 128.91965);
                            break;
                    }
                    Dictionary<int, Vector3> InteriorHijacking = new Dictionary<int, Vector3>
                    {
                        { 0, Toilet },
                        { 1, Lamp },
                        { 2, Cupboard },
                    };
                    Trigger.ClientEvent(player, "createHijackingHouseBlip", HouseManager.Houses[houseID].Position);
                    Trigger.ClientEvent(player, "createWaypoint", HouseManager.Houses[houseID].Position.X, HouseManager.Houses[houseID].Position.Y);
                    List<ColShape> shapes = new List<ColShape>();
                    foreach (KeyValuePair<int, Vector3> Interior in InteriorHijacking)
                    {
                        Trigger.ClientEvent(player, "createCheckpoint", 501 + Interior.Key, 1, Interior.Value, 1, (uint)HouseManager.GetDimension(HouseManager.Houses[houseID]), 220, 220, 0);
                        ColShape shape = NAPI.ColShape.CreateCylinderColShape(Interior.Value, 1f, 2f, (uint)HouseManager.GetDimension(HouseManager.Houses[houseID]));
                        shape.OnEntityEnterColShape += (s, e) =>
                        {
                            e.SetData("HijackingInteriorItem", Interior.Key);
                            EnterHijackingColShape(s, e);
                        };
                        shape.OnEntityExitColShape += (s, e) =>
                        {
                            e.ResetData("HijackingInteriorItem");
                            e.ResetData("HijackingHouseMarker");
                        };
                        shapes.Add(shape);
                        shape.SetData("ColShapePlHijackingHouse", player);
                        shape.SetData("ColShapeTypeHijackingHouse", Interior.Key);
                    }
                    player.SetData("ColShapeHijackingHouse", shapes);
                    player.SetData("ON_WORK_HIJACKING_HOUSE", true);
                    Notify.Info(player, $"You took a contract to rob house number #{houseID}", 3000);
                    player.SendChatMessage("You took a contract to rob house number #" + houseID);
                }
                catch (Exception e) { Log.Write("CreateColShapeHouses: " + e.ToString(), nLog.Type.Error); }
            }
            private static void EnterHijackingColShape(ColShape shape, Player player)
            {
                try
                {
                    if (!Main.Players.ContainsKey(player) || !player.HasData("ON_WORK_HIJACKING_HOUSE") || !shape.HasData("ColShapePlHijackingHouse") || !player.HasData("HijackingInteriorItem") || !player.HasData("ColShapeHijackingHouse") || player.HasData("TakeHijackingItem:Server")) return;
                    if (player.GetData<List<ColShape>>("ColShapeHijackingHouse").Find(i => i.GetData<int>("ColShapeTypeHijackingHouse") == shape.GetData<int>("ColShapeTypeHijackingHouse")) == shape)
                    {
                        if (shape.GetData<Player>("ColShapePlHijackingHouse") == player)
                        {
                            switch (player.GetData<int>("HijackingInteriorItem"))
                            {
                                case 0:
                                    BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_ld_toilet_01"), 6286, new Vector3(-0.02f, 0.063f, 0), new Vector3(210, -10, 30));
                                    break;
                                case 1:
                                    BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_cd_lamp"), 6286, new Vector3(-0.02f, 0.063f, 0), new Vector3(0, 167, 90));
                                    break;
                                case 2:
                                    BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("p_yacht_chair_01_s"), 6286, new Vector3(-0.02f, 0.063f, 0), new Vector3(-90, 50, 120));
                                    break;
                            }
                            player.PlayAnimation("anim@heists@box_carry@", "idle", 49);
                            player.SetSharedData("TakeHijackingItem", player.GetData<int>("HijackingInteriorItem"));
                            player.SetData("TakeHijackingItem:Server", true);
                            List<ColShape> shapes = player.GetData<List<ColShape>>("ColShapeHijackingHouse");
                            shapes.Remove(shape);
                            shape.Delete();
                            Trigger.ClientEvent(player, "deleteCheckpoint", 501 + player.GetData<int>("HijackingInteriorItem"));
                            player.ResetData("ColShapeHijackingHouse");
                            if (player.HasData("HijackingHouseMarker"))
                                player.GetData<Marker>("HijackingHouseMarker").Delete();
                            if (shapes.Count > 0)
                                player.SetData("ColShapeHijackingHouse", shapes);
                        }
                    }
                }
                catch (Exception e) { Log.Write("EnterHijackingColShape: " + e.ToString(), nLog.Type.Error); }
            }
        }
    }
}
