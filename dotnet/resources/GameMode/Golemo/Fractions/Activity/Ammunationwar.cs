using GTANetworkAPI;
using GolemoSDK;
using System;

namespace Golemo.Fractions
{
    class Crime : Script
    {
        private static nLog RLog = new nLog("CrimeMissionsnew");

        private static GTANetworkAPI.ColShape shapeGunshop;

        private static Vector3 Gunshop = new Vector3(2439.2727, 4969.5, -500.444828);

        private static int PriceGunshop = 5000000; // цена на ограбление амунашки

        [ServerEvent(Event.ResourceStart)]
        public static void EnterCrimeShapeRealtor()
        {
            try
            {
                #region Creating Marker & Colshape
                //заказ ограбления ганшопа
                shapeGunshop = NAPI.ColShape.CreateCylinderColShape(Gunshop + new Vector3(0, 0, 0.65), 1, 1, 0);
                
                shapeGunshop.OnEntityEnterColShape += (s, ent) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 328);
                    }
                    catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); }
                };
                shapeGunshop.OnEntityExitColShape += (s, ent) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 0);
                    }
                    catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); }
                };
            }
            catch (Exception e) { RLog.Write(e.ToString(), nLog.Type.Error); }
            #endregion
        }

        public static void Gunshopbuy(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].FractionID > 5 && Main.Players[player].FractionID < 10)
                    {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You are not in CREMOM OFFICE!", 3000);
                    return;
                }
                if (Main.Players[player].FractionLVL < 8)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас маленький ранг", 3000);
                    return;
                }
                if (!MoneySystem.Wallet.Change(player, -PriceGunshop))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You do not have enough money.", 3000);
                    return;
                }
                if (Fractions.Activity.AmmunationBox._isStart == true)
                {
                    Notify.Error(player, "Ящик с боеприпасами уже заспавнен");
                    return;
                }
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы заказали ограбление ганшопа", 3000);
                Fractions.Activity.AmmunationBox.SpawnAnAmmoBox();
            }
            catch (Exception e) { RLog.Write("GunShopOrder: " + e.Message, nLog.Type.Error); }
        }
    }
}