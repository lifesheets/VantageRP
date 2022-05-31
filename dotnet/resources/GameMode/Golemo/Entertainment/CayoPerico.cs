using System;
using GTANetworkAPI;
using GolemoSDK;

namespace Golemo.CayoPerico
{
    class CayoPerico : Script
    {
        private static nLog Log = new nLog("Cayo Perico");
        private static int _priceForAdmission = 500;
        private static Vector3 _entrancePosition = new Vector3(-1058.5121, -2538.0662, 13.94454);
        private static Vector3 _exitPosition = new Vector3(4494.155, -4525.5806, 4.4123641);
        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                var colShapeEnter = NAPI.ColShape.CreateCylinderColShape(_entrancePosition, 1f, 2, 0);
                var colShapeExit = NAPI.ColShape.CreateCylinderColShape(_exitPosition, 1f, 2, 0);

                NAPI.Marker.CreateMarker(30, _entrancePosition - new Vector3(0, 0, 0), new Vector3(), new Vector3(), 0.8f, new Color(163, 131, 188), false, 0);
                NAPI.Marker.CreateMarker(30, _exitPosition - new Vector3(0, 0, 0), new Vector3(), new Vector3(), 0.8f, new Color(163, 131, 188), false, 0);

                NAPI.Blip.CreateBlip(307, _entrancePosition, 0.9f, 4, "Аэропорт", 255, 0, true, dimension: 0);
                NAPI.Blip.CreateBlip(307, _exitPosition, 0.9f, 4, "Аэропорт", 255, 0, true, dimension: 0);

                colShapeEnter.OnEntityEnterColShape += (s, e) =>
                {
                    try
                    {
                        if (!e.IsInVehicle)
                        {
                            NAPI.Data.SetEntityData(e, "INTERACTIONCHECK", 778);
                            NAPI.Data.SetEntityData(e, "CASINO_MAIN_SHAPE", "ENTER");
                        }
                    }
                    catch (Exception ex) { Log.Write("EnterCayoPerico_OnEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
                };
                colShapeEnter.OnEntityExitColShape += OnEntityExitCasinoMainShape;

                colShapeExit.OnEntityEnterColShape += (s, e) =>
                {
                    try
                    {
                        if (!e.IsInVehicle)
                        {
                            NAPI.Data.SetEntityData(e, "INTERACTIONCHECK", 778);
                            NAPI.Data.SetEntityData(e, "CASINO_MAIN_SHAPE", "EXIT");
                        }
                    }
                    catch (Exception ex) { Log.Write("ExitCayoPerico_OnEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
                };
                colShapeExit.OnEntityExitColShape += OnEntityExitCasinoMainShape;
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }
        public static void OnEntityExitCasinoMainShape(ColShape shape, Player player)
        {
            NAPI.Data.SetEntityData(player, "INTERACTIONCHECK", 0);
            NAPI.Data.ResetEntityData(player, "CASINO_MAIN_SHAPE");
        }
        public static void CallBackShape(Player player)
        {
            if (!player.HasData("CASINO_MAIN_SHAPE")) return;
            string data = player.GetData<string>("CASINO_MAIN_SHAPE");
            if (data == "ENTER")
            {
                Trigger.ClientEvent(player, "showHUD", false);
                NAPI.Task.Run(() => {
                    try
                    {
                        if (player != null)
                        {
                            Trigger.ClientEvent(player, "screenFadeOut", 1000);
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
                                NAPI.Entity.SetEntityPosition(player, _exitPosition);
                                NAPI.Entity.SetEntityRotation(player, new Vector3(0, 0, -27.5));
                                Trigger.ClientEvent(player, "screenFadeIn", 1000);
                                Trigger.ClientEvent(player, "showHUD", true);
                                MoneySystem.Wallet.Change(player, -_priceForAdmission);
                            }
                        }
                    }
                    catch { }
                }, 1600);
                return;
            }
            if (data == "EXIT")
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
                                NAPI.Entity.SetEntityPosition(player, _entrancePosition);
                                NAPI.Entity.SetEntityRotation(player, new Vector3(0, 0, -27.5));
                                Trigger.ClientEvent(player, "screenFadeIn", 1000);
                                Trigger.ClientEvent(player, "showHUD", true);
                            }
                        }
                    }
                    catch { }
                }, 1600);
            }
        }
    }
}
