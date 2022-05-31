using System;
using GTANetworkAPI;
using GolemoSDK;
using Newtonsoft.Json;

namespace Golemo.Core
{
    class Poligon : Script
    {
        private static nLog Log = new nLog("Poligon");
        public static Vector3 startpoligon = new Vector3(897.6991, -3178.2913, -97.123576);

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                var colShapeStart = NAPI.ColShape.CreateCylinderColShape(startpoligon, 1f, 2, 0);

                NAPI.Ped.CreatePed(0x9E08633D, startpoligon, 80, false, true, true, true, 0);
                //NAPI.TextLabel.CreateTextLabel("~r~Начать задание", new Vector3(816.46344, -2161.8843, 30.619012), 5f, 0.3f, 4, new Color(39, 174, 96), true, 0);
                NAPI.Marker.CreateMarker(1, startpoligon - new Vector3(0, 0, 1), new Vector3(), new Vector3(), 0.8f, new Color(13, 230, 70, 200), false, 0);

                colShapeStart.OnEntityEnterColShape += (s, e) =>
                {
                    try
                    {
                        if (!e.IsInVehicle)
                        {
                            NAPI.Data.SetEntityData(e, "INTERACTIONCHECK", 670);
                        }
                    }
                    catch (Exception ex) { Log.Write("StartPoligon_OnEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
                };
                colShapeStart.OnEntityExitColShape += OnEntityExitCasinoMainShape;
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }
        public static void OnEntityExitCasinoMainShape(ColShape shape, Player player)
        {
            NAPI.Data.SetEntityData(player, "INTERACTIONCHECK", 0);
        }
        public static void OpenPedPoligonMenu(Player player)
        {
            Trigger.ClientEvent(player, "OpenPedPoligon", Main.Players[player].LVL);
        }
        [RemoteEvent("SelectWeaponAndStart")]
        public static void SetSelectWeaponAndStart(Player player, int weaponid)
        {
            if (weaponid == 0)
            {
                //NAPI.ClientEvent.TriggerClientEvent(player, "client::setweapon", 453432689);
                Trigger.ClientEvent(player, "wgive", 453432689, 200, false, true);
                player.SetData("weaponHashPoligon", 453432689);
                StartPoligon(player);
                return;
            }
            if (weaponid == 1)
            {
                //NAPI.ClientEvent.TriggerClientEvent(player, "client::setweapon", 984333226);
                Trigger.ClientEvent(player, "wgive", 984333226, 200, false, true);
                player.SetData("weaponHashPoligon", 984333226);
                StartPoligon(player);
                return;
            }
            if (weaponid == 2)
            {
                //NAPI.ClientEvent.TriggerClientEvent(player, "client::setweapon", -1045183535);
                Trigger.ClientEvent(player, "wgive", -1045183535, 200, false, true);
                player.SetData("weaponHashPoligon", -1045183535);
                StartPoligon(player);
                return;
            }
        } 
        public static void StartPoligon(Player player)
        {
            if (!player.HasData("START_PLAYER_POLIGON"))
            {
                Trigger.ClientEvent(player, "showHUD", false);
                NAPI.Task.Run(() =>
                {
                    try
                    {
                        if (player != null)
                        {
                            Trigger.ClientEvent(player, "screenFadeOut", 1000);
                        }
                    }
                    catch { }
                }, 100);
                NAPI.Task.Run(() =>
                {
                    try
                    {
                        if (player != null)
                        {
                            if (player.IsInVehicle)
                            {
                                return;
                            }
                            else
                            {
                                Trigger.ClientEvent(player, "StartPoligon");
                                NAPI.Entity.SetEntityPosition(player, new Vector3(896.09973, -3170.9475, -97.12364));
                                NAPI.Entity.SetEntityRotation(player, new Vector3(0, 0, -8));
                                Notify.Succ(player, "Вы начали стрельбище, стреляйте по мишеням получайте очки и прокачивайте навык стрельбы");
                                Trigger.ClientEvent(player, "screenFadeIn", 1000);
                                Trigger.ClientEvent(player, "showHUD", true);
                                player.SetData("ON_PLAYER_POLIGON", true);
                                player.SetSharedData("ON_PLAYER_POLIGON", true);
                                NAPI.Entity.SetEntityDimension(player, (uint)(5000 + player.Value));
                            }
                        }
                    }
                    catch { }
                }, 1600);
                return;
            }
            else
            {
                Notify.Succ(player, "Вы уже начали задание на стрельбище");
                return;
            }
        } 
        [RemoteEvent("FinishedPoligon")]
        public static void FinishedPoligon(Player player, int points)
        {
            Trigger.ClientEvent(player, "showHUD", false);
            NAPI.Task.Run(() => {
                try
                {
                    if (player != null)
                    {
                        Trigger.ClientEvent(player, "screenFadeOut", 1000);
                        Trigger.ClientEvent(player, "showHUD", false);
                    }
                }
                catch { }
            }, 100);
            NAPI.Task.Run(() => {
                try
                {
                    if (player != null)
                    {
                        if (player.IsInVehicle)
                        {
                            return;
                        }
                        else
                        {
                            Trigger.ClientEvent(player, "screenFadeIn", 1000);
                            Trigger.ClientEvent(player, "showHUD", true);
                            player.ResetData("ON_PLAYER_POLIGON");
                            player.ResetSharedData("ON_PLAYER_POLIGON");
                            NAPI.Entity.SetEntityPosition(player, startpoligon);
                            NAPI.Entity.SetEntityDimension(player, 0);
                            Trigger.ClientEvent(player, "removeAllWeapons");
                            if (points >= 10)
                            {
                                var payment = (points * 25000);
                                Notify.Succ(player, $"Вы набрали {points} поинтов и получили {payment}$", 3000);
                                MoneySystem.Wallet.Change(player, payment);
                                return;
                            }
                            else
                            {
                                Notify.Succ(player, $"Вы набрали {points} поинтов и закончили стрельбу. Чтобы начать еще раз подойдите NPC", 3000);
                            }
                        }
                    }
                }
                catch { }
            }, 1600);
        }
        [RemoteEvent("StopMissionPoligon")]
        public static void StopMissionPoligon(Player player, int points)
        {
            if (player.HasData("ON_PLAYER_POLIGON"))
            {
                Trigger.ClientEvent(player, "showHUD", false);
                NAPI.Task.Run(() => {
                    try
                    {
                        if (player != null)
                        {
                            Trigger.ClientEvent(player, "screenFadeOut", 1000);
                            Trigger.ClientEvent(player, "showHUD", false);
                        }
                    }
                    catch { }
                }, 100);
                NAPI.Task.Run(() => {
                    try
                    {
                        if (player != null)
                        {
                            if (player.IsInVehicle)
                            {
                                return;
                            }
                            else
                            {
                                Trigger.ClientEvent(player, "screenFadeIn", 1000);
                                Trigger.ClientEvent(player, "StopPoligon");
                                Trigger.ClientEvent(player, "showHUD", true);
                                player.ResetData("ON_PLAYER_POLIGON");
                                player.ResetSharedData("ON_PLAYER_POLIGON");
                                NAPI.Entity.SetEntityPosition(player, startpoligon);
                                NAPI.Entity.SetEntityDimension(player, 0);
                                Trigger.ClientEvent(player, "removeAllWeapons");
                                if (points >= 10)
                                {
                                    var payment = (points * 25000);
                                    Notify.Succ(player, $"Вы закончили стрельбу и набрали {points} поинтов и получили {payment}$", 3000);
                                    MoneySystem.Wallet.Change(player, payment);
                                    return;
                                }
                                else
                                {
                                    Notify.Succ(player, $"Вы закончили стрельбу набрав {points} поинтов", 3000);
                                }
                            }
                        }
                    }
                    catch { }
                }, 1600);
            }
            else
            {
                return;
            }
        }
    }
}
