using GTANetworkAPI;
using System;
using GolemoSDK;
using Golemo.Core;

namespace Golemo.Entertainment
{
    class NewYear : Script
    {
        public static bool ActiveXMAS = false;
        private static nLog Log = new nLog("New Year");

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                if (!ActiveXMAS) return;
                NAPI.Blip.CreateBlip(304, new Vector3(225.6532, -891.8182, 29.57199), 1f, 5, Main.StringToU16("Новогодняя Ёлка"), 255, 0, true, 0, 0);
                ColShape shape = NAPI.ColShape.CreateSphereColShape(new Vector3(191.9436, -930.8194, 30.7338), 5.5f);
                ColShape shape2 = NAPI.ColShape.CreateSphereColShape(new Vector3(204.1075, -939.4277, 30.6458), 5.5f);
                shape.OnEntityEnterColShape += OnEnter;
                shape2.OnEntityEnterColShape += OnEnter;
                shape.OnEntityExitColShape += OnExit;
                shape2.OnEntityExitColShape += OnExit;

                ColShape elka = NAPI.ColShape.CreateSphereColShape(new Vector3(225.6532, -891.8182, 29.57199), 3f);
                elka.OnEntityEnterColShape += (s, player) =>
                {
                    try
                    {
                        player.SetData("INTERACTIONCHECK", 159);
                    }
                    catch (Exception e) { Log.Write("EXCEPTION AT \"NEWYEAR\":\n" + e.ToString(), nLog.Type.Error); }
                };
                elka.OnEntityExitColShape += (s, player) =>
                {
                    try
                    {
                        player.SetData("INTERACTIONCHECK", 0);
                    }
                    catch (Exception e) { Log.Write("EXCEPTION AT \"NEWYEAR\":\n" + e.ToString(), nLog.Type.Error); }
                };

                Log.Write("Actived!", nLog.Type.Success);
            } 
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"NEWYEAR\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

        private static void OnEnter(ColShape shape, Player player)
        {
            try {
                player.SetData("INTERACTIONCHECK", 158);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"NEWYEAR\":\n" + e.ToString(), nLog.Type.Error); }
        }
        public static void TakeSnowball(Player player)
        {
            var find = nInventory.Find(Main.Players[player].UUID, ItemType.SnowBall);
            if (find != null && find.Count >= 16)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Нет места для снежков!", 3000);
                return;
            }
            nInventory.Add(player, new nItem(ItemType.SnowBall));
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Вы взяли снежок", 3000);
        }
        public static void TakeGiftINTree(Player player)
        {
            if (Main.Players[player].Cooldown > DateTime.Now.AddHours(-1))
            {
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Приходите за подарком позже", 3000);
                return;
            }
            var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.Present));
            if (tryAdd == -1 || tryAdd > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Not enough space in the inventory", 3000);
                return;
            }
            Main.Players[player].Cooldown = DateTime.Now;
            nInventory.Add(player, new nItem(ItemType.Present));
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы получили подарок, приходите еще раз через час", 3000);
        }
        private static void OnExit(ColShape shape, Player player)
        {
            try
            {
                player.SetData("INTERACTIONCHECK", 0);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"NEWYEAR\":\n" + e.ToString(), nLog.Type.Error); }
        }


    }
}
