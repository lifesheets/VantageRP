using GTANetworkAPI;
using System;
using GolemoSDK;
using Golemo.Core;
using Golemo.Core.Character;

namespace Golemo.Houses
{
    class TransportDump : Script
    {
        private static ColShape _shape;
        private static Vector3 _position = new Vector3(-469.10852, -1717.8014, 17.569134);
        private static nLog Log = new nLog("Dump Transport");

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                NAPI.Marker.CreateMarker(1, _position - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 3, new Color(255, 0, 0));
                NAPI.Blip.CreateBlip(163, _position, 0.9f, 1, "Сдача авто на свалку", 255, 0, true, 0, 0);
                _shape = NAPI.ColShape.CreateCylinderColShape(_position, 3, 3, 0);
                _shape.OnEntityEnterColShape += svalkaShape_onEntityEnterColShape;
                _shape.OnEntityExitColShape += svalkaShape_onEntityExitColShape;
                NAPI.TextLabel.CreateTextLabel("~r~Сдача авто на свалку", _position + new Vector3(0, 0, 1.5), 5F, 0.3F, 0, new Color(255, 255, 255));
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        private void svalkaShape_onEntityEnterColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 101);
            }
            catch (Exception ex) { Log.Write("svalkaShape_onEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
        }

        private void svalkaShape_onEntityExitColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
            }
            catch (Exception ex) { Log.Write("svalkaShape_onEntityExitColShape: " + ex.Message, nLog.Type.Error); }
        }
        public static void OpenCarsSellMenu(Player player)
        {
            if (player.IsInVehicle)
            {
                Character acc = Main.Players[player];
                var pl = acc.FirstName + "_" + acc.LastName;
                var holder = VehicleManager.Vehicles[player.Vehicle.NumberPlate].Holder;
                if (pl != holder)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"This is not your car.", 3000);
                    return;
                }
                var vData = VehicleManager.Vehicles[player.Vehicle.NumberPlate];
                var price = (BusinessManager.ProductsOrderPrice.ContainsKey(vData.Model)) ? Convert.ToInt32(BusinessManager.ProductsOrderPrice[vData.Model] * 0.5) : 0;
                Trigger.ClientEvent(player, "openDialog", "SvalkaSell", $"Вы хотите продать данное авто за {price}$ ");
                return;
            }
            else
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You must be in the car.", 3000);
                return;
            }
        } 
    }
}