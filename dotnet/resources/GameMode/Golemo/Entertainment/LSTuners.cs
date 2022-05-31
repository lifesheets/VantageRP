using System;
using GTANetworkAPI;
using GolemoSDK;

namespace Golemo.LSTuners
{
    class LSTuners : Script
    {
        private static nLog Log = new nLog("LSTuners");
        private static Vector3 _entrancePosition = new Vector3(783.0346, -1867.8917, 29.325284);
        private static Vector3 _exitPosition = new Vector3(-2220.0928, 1156.5823, -23.379158);
        
        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                var colShapeEnter = NAPI.ColShape.CreateCylinderColShape(_entrancePosition, 5f, 2, 0);
                var colShapeExit = NAPI.ColShape.CreateCylinderColShape(_exitPosition, 5f, 2, 0);

                NAPI.Blip.CreateBlip(777, new Vector3(783.0346, -1867.8917, 28.125284), 0.9f, 4, Main.StringToU16("Los Santos Tuners"), 255, 0, true, 0, 0);
                NAPI.Marker.CreateMarker(1, _entrancePosition - new Vector3(0, 0, 1.5), new Vector3(), new Vector3(), 5, new Color(67, 140, 239), false, 0);
                NAPI.Marker.CreateMarker(1, _exitPosition - new Vector3(0, 0, 1.5), new Vector3(), new Vector3(), 5, new Color(67, 140, 239), false, 0);
                colShapeEnter.OnEntityEnterColShape += (s, e) =>
                {
                    try
                    {
                            NAPI.Data.SetEntityData(e, "INTERACTIONCHECK", 904);
                            NAPI.Data.SetEntityData(e, "CASINO_MAIN_SHAPE", "ENTER");
                    }
                    catch (Exception ex) { Log.Write("EnterCasino_OnEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
                };
                colShapeEnter.OnEntityExitColShape += OnEntityExitCasinoMainShape;

                colShapeExit.OnEntityEnterColShape += (s, e) =>
                {
                    try
                    {
                            NAPI.Data.SetEntityData(e, "INTERACTIONCHECK", 904);
                            NAPI.Data.SetEntityData(e, "CASINO_MAIN_SHAPE", "EXIT");
                    }
                    catch (Exception ex) { Log.Write("ExitCasino_OnEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
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
            Trigger.ClientEvent(player, "showHUD", false);
            if (data == "ENTER")
            {
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
                                NAPI.Entity.SetEntityPosition(player.Vehicle, _exitPosition);
                                NAPI.Entity.SetEntityRotation(player.Vehicle, new Vector3(0, 0, -27.5));
                                player.SetIntoVehicle(player.Vehicle, 0);
                                Trigger.ClientEvent(player, "screenFadeIn", 1000);
                                Trigger.ClientEvent(player, "showHUD", true);
                            }
                            else
                            {
                                NAPI.Entity.SetEntityPosition(player, _exitPosition);
                                NAPI.Entity.SetEntityRotation(player, new Vector3(0, 0, -27.5));
                                Trigger.ClientEvent(player, "screenFadeIn", 1000);
                                Trigger.ClientEvent(player, "showHUD", true);
                            }
                        }
                    }
                    catch { }
                }, 1600);
                return;
            }
            if(data == "EXIT")
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
                                NAPI.Entity.SetEntityPosition(player.Vehicle, _entrancePosition);
                                NAPI.Entity.SetEntityRotation(player.Vehicle, new Vector3(0, 0, -27.5));
                                player.SetIntoVehicle(player.Vehicle, 0);
                                Trigger.ClientEvent(player, "screenFadeIn", 1000);
                                Trigger.ClientEvent(player, "showHUD", true);
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
        public static void EnterLSTuners(Player player)
        {
            Trigger.ClientEvent(player, "ShowHUD", false);
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
                        if (player.IsInVehicle) { 
                            NAPI.Entity.SetEntityPosition(player.Vehicle, _exitPosition);
                            NAPI.Entity.SetEntityRotation(player.Vehicle, new Vector3(0, 0, -27.5));
                            player.SetIntoVehicle(player.Vehicle, 0);
                            Trigger.ClientEvent(player, "screenFadeIn", 1000);
                            Trigger.ClientEvent(player, "ShowHUD", true);
                        }
                        else
                        {
                            NAPI.Entity.SetEntityPosition(player, _exitPosition);
                            NAPI.Entity.SetEntityRotation(player, new Vector3(0, 0, -27.5));
                            Trigger.ClientEvent(player, "screenFadeIn", 1000);
                            Trigger.ClientEvent(player, "ShowHUD", true);
                        }
                    }   
                }
                catch { }
            }, 1600);
        }
    }
}
