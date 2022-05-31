using GTANetworkAPI;
using GolemoSDK;
using System;
using Golemo.Houses;
using Golemo.Fractions;
namespace Golemo.Core
{
    class VehChangeNumber : Script
    {


        private static nLog RLog = new nLog("Change Number Place");

        private static ColShape shapeChangeNum;

        private static Vector3 PositionChangeNumber = new Vector3(396.69742, -1165.649, 28.171766);

        public static int Price = 2500; //donate
        public static int Price1 = 5000;
        public static int Price2 = 10000;
        public static int Price3 = 15000;

        [ServerEvent(Event.ResourceStart)]
        public static void OnResourceStart()
        {
            try
            {
                NAPI.Marker.CreateMarker(1, PositionChangeNumber, new Vector3(), new Vector3(), 4f, new Color(225, 217, 0), false, 0);
                NAPI.Blip.CreateBlip(490, PositionChangeNumber, 0.8f, 7, "Смена номеров для ТС", shortRange: true, dimension: 0);

                shapeChangeNum = NAPI.ColShape.CreateCylinderColShape(PositionChangeNumber, 4, 4, 0);
                shapeChangeNum.OnEntityEnterColShape += (s, ent) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 901);
                    }
                    catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); }
                };
                shapeChangeNum.OnEntityExitColShape += (s, ent) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 0);
                    }
                    catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); }
                };

                RLog.Write("Loaded", nLog.Type.Info);
            }
            catch (Exception e) { RLog.Write(e.ToString(), nLog.Type.Error); }
        }

        public static void OpenMenuChangeNumber(Player player)
        {
            if (player.VehicleSeat != 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You must be in the driver's seat", 3000);
                return;
            }
            var veh = player.Vehicle;
            if (!veh.HasData("ACCESS") && (veh.GetData<string>("ACCESS") != "PERSONAL" || veh.GetData<string>("ACCESS") != "GARAGE"))
            {
                var access = VehicleManager.canAccessByNumber(player, veh.NumberPlate);
                if (!access)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Это не ваше авто!", 3000);
                    return;
                }
            }
            Trigger.ClientEvent(player, "openChangeNumber");
            return;
        }

        [RemoteEvent("changeNumber")]
        public void Event_changeNumber(Player player, string number, int type)
        {
            try
            {
                RLog.Write("ХУЕСОС" + Configs.FractionVehicles);
                if (!Main.Players.ContainsKey(player)) return;
                if (player.VehicleSeat != 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You must be in the driver's seat", 3000);
                    return;
                }

                var veh = player.Vehicle;
                if (!veh.HasData("ACCESS") && (veh.GetData<string>("ACCESS") != "PERSONAL" || veh.GetData<string>("ACCESS") != "GARAGE"))
                {
                    var access = VehicleManager.canAccessByNumber(player, veh.NumberPlate);
                    if (!access)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы можете установить номера только на Личном Авто", 3000);
                        return;
                    }
                }

                if (VehicleManager.Vehicles[veh.NumberPlate].Сasco == 1)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "На этом ТС уже установлен номер", 3000);
                    return;
                }

                if (string.IsNullOrEmpty(number) || string.IsNullOrWhiteSpace(number) || number == "0")
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Введите корректные данные", 3000);
                    return;
                }
                if (number.Length < 3)
                {
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Количество символов в не может быть меньше 3", 3000);
                    return;
                }
                if (number.Length > 9)
                {
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Количество символов в номерном знаке не может превышать 8.", 3000);
                    return;
                }

                number = number.ToUpper();
                if (VehicleManager.Vehicles.ContainsKey(number) || number.Contains("TRANSIT") || number.Contains("ADMIN") || number.Contains("PIDAR") || number.Contains("P1DAR") || number.Contains("PIDARAS") || number.Contains("P1D4R") || number.Contains("P1D4RAS") || number.Contains("P1DARAS") || number.Contains("NIGGER") || number.Contains("N1GGER") || number.Contains("PIDOR") || number.Contains("PID0R"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Введенный вами номер уже занят, попробуйте другой", 3000);
                    return;
                }

                var oldNum = player.Vehicle.NumberPlate;
                var vData = VehicleManager.Vehicles[oldNum];

                if (type == 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Выберите услугу", 3000);
                    return;
                }
                if (type == 1)  //5000
                {
                    if (Main.Players[player].Money < Price1)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 3000);
                        return;
                    }

                    MoneySystem.Wallet.Change(player, -Price1);
                    GameLog.Money($"{Main.Players[player].UUID}", "server", -Price1, "VehChangeNumber");

                    veh.NumberPlate = number;
                    VehicleManager.Vehicles.Remove(oldNum);
                    VehicleManager.Vehicles.Add(number, vData);
                    VehicleManager.Vehicles[number].Сasco = 1;
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Новый номер для транспорта {veh.DisplayName} = {number}", 3000);
                    MySQL.Query($"UPDATE vehicles SET number='{number}' WHERE number='{oldNum}'");
                    VehicleManager.Save(number);
                    return;
                }
                else if (type == 3)   //10000
                {
                    if (Main.Players[player].Money < Price2)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 3000);
                        return;
                    }

                    MoneySystem.Wallet.Change(player, -Price2);
                    GameLog.Money($"{Main.Players[player].UUID}", "server", -Price2, "VehChangeNumber");

                    veh.NumberPlate = number;
                    VehicleManager.Vehicles.Remove(oldNum);
                    VehicleManager.Vehicles.Add(number, vData);
                    VehicleManager.Vehicles[number].Сasco = 1;
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Новый номер для транспорта {veh.DisplayName} = {number}", 3000);
                    MySQL.Query($"UPDATE vehicles SET number='{number}' WHERE number='{oldNum}'");
                    VehicleManager.Save(number);
                    return;
                }
                else if (type == 4)       //15000
                {
                    if (Main.Players[player].Money < Price3)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 3000);
                        return;
                    }

                    MoneySystem.Wallet.Change(player, -Price3);
                    GameLog.Money($"{Main.Players[player].UUID}", "server", -Price3, "VehChangeNumber");

                    veh.NumberPlate = number;
                    VehicleManager.Vehicles.Remove(oldNum);
                    VehicleManager.Vehicles.Add(number, vData);
                    VehicleManager.Vehicles[number].Сasco = 1;
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Новый номер для транспорта {veh.DisplayName} = {number}", 3000);
                    MySQL.Query($"UPDATE vehicles SET number='{number}' WHERE number='{oldNum}'");
                    VehicleManager.Save(number);
                    return;
                }
                else if (type == 2)              //donat
                {
                    if (Main.Accounts[player].RedBucks < Price)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно рублей", 3000);
                        return;
                    }

                    Main.Accounts[player].RedBucks -= Price;
                    GameLog.Money($"{Main.Players[player].UUID}", "server", -Price, "VehChangeNumber");
                    MySQL.Query($"update `accounts` set `redbucks`={Main.Accounts[player].RedBucks} where `login`='{Main.Accounts[player].Login}'");

                    veh.NumberPlate = number;
                    VehicleManager.Vehicles.Remove(oldNum);
                    VehicleManager.Vehicles.Add(number, vData);
                    VehicleManager.Vehicles[number].Сasco = 1;
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Новый номер для транспорта {veh.DisplayName} = {number}", 3000);
                    MySQL.Query($"UPDATE vehicles SET number='{number}' WHERE number='{oldNum}'");
                    VehicleManager.Save(number);
                    return;
                }
            }
            catch (Exception e) { RLog.Write("changeNumber: " + e.Message, nLog.Type.Error); }
        }
    }
}