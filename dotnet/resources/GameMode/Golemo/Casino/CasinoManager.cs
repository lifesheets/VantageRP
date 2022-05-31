using System;
using GTANetworkAPI;
using GolemoSDK;

namespace Golemo.Casino
{
    class CasinoManager : Script
    {
        private static nLog Log = new nLog("Casino");
        private static Vector3 _entrancePosition = new Vector3(924.93176, 46.50941, 81.2);
        private static Vector3 _exitPosition = new Vector3(1089.695, 206.015, -49);
        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {

                var colShapeEnter = NAPI.ColShape.CreateCylinderColShape(_entrancePosition, 1f, 2, 0);
                var colShapeExit = NAPI.ColShape.CreateCylinderColShape(_exitPosition, 1f, 2, 0);

                NAPI.Marker.CreateMarker(30, _entrancePosition - new Vector3(0, 0, 0), new Vector3(), new Vector3(), 0.8f, new Color(13, 230, 70), false, 0);
                NAPI.Marker.CreateMarker(30, _exitPosition - new Vector3(0, 0, 0), new Vector3(), new Vector3(), 0.8f, new Color(13, 230, 70), false, 0);

                NAPI.Blip.CreateBlip(617, _entrancePosition, 1, 4, "Diamond Casino", 255, 0, true, dimension: 0);

                colShapeEnter.OnEntityEnterColShape += (s, e) =>
                {
                    try
                    {
                        if (!e.IsInVehicle)
                        {
                            NAPI.Data.SetEntityData(e, "INTERACTIONCHECK", 805);
                            NAPI.Data.SetEntityData(e, "CASINO_MAIN_SHAPE", "ENTER");
                        }
                    }
                    catch (Exception ex) { Log.Write("EnterCasino_OnEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
                };
                colShapeEnter.OnEntityExitColShape += OnEntityExitCasinoMainShape;

                colShapeExit.OnEntityEnterColShape += (s, e) =>
                {
                    try
                    {
                        if (!e.IsInVehicle)
                        {
                            NAPI.Data.SetEntityData(e, "INTERACTIONCHECK", 805);
                            NAPI.Data.SetEntityData(e, "CASINO_MAIN_SHAPE", "EXIT");
                        }
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
            if (data == "ENTER")
            {
                Trigger.ClientEvent(player, "openDialog", "ENTER_CASINO", $"You are sure that you want to go to Kazinо?");
                return;
            }
            if (data == "EXIT")
            {
                NAPI.Entity.SetEntityPosition(player, _entrancePosition);
                NAPI.Entity.SetEntityRotation(player, new Vector3(0, 0, 113.5));
                Trigger.ClientEvent(player, "exitCasinoWall");
                player.SetSharedData("PLAYER_IN_CASINO", false);
                player.SetData("PLAYER_IN_CASINO", false);
                Trigger.ClientEvent(player, "client::destroyblips", 1);
                Trigger.ClientEvent(player, "client::destroyblips", 2);
                Trigger.ClientEvent(player, "client::destroyblips", 3);
                Trigger.ClientEvent(player, "client::destroyblips", 4);
                Trigger.ClientEvent(player, "client::destroyblips", 5);
                Trigger.ClientEvent(player, "client::destroyblips", 6);
                Trigger.ClientEvent(player, "client::destroyblips", 7);
            }
        }
        public static void EnterCasino(Player player)
        {
            Trigger.ClientEvent(player, "cameracasinoenter", true);
            NAPI.Task.Run(() => {
                try
                {
                    if (player != null)
                    {
                        Trigger.ClientEvent(player, "screenFadeOut", 2000);
                        Trigger.ClientEvent(player, "client::createblip", 1, 681, "Колесо удачи", 4, new Vector3(1111.052, 229.8579, -49.133), true);
                        Trigger.ClientEvent(player, "client::createblip", 2, 679, "Столы", 4, new Vector3(1138.4813, 256.3838, -53.67249), true);
                        Trigger.ClientEvent(player, "client::createblip", 3, 684, "Скачки", 4, new Vector3(1096.9711, 259.95172, -53.54053), true);
                        Trigger.ClientEvent(player, "client::createblip", 4, 683, "Фишки", 4, new Vector3(1115.912, 219.99, -50.55512), true);
                        Trigger.ClientEvent(player, "client::createblip", 5, 682, "Стойка с информацией", 4, CarLottery._mainShapePosition, true);
                        Trigger.ClientEvent(player, "client::createblip", 6, 93, "Бар", 4, CasinBar.shape[0], true);
                        Trigger.ClientEvent(player, "client::createblip", 7, 743, "Лифт", 4, new Vector3(1085.1727, 214.28719, -49.22043), true);
                    }
                }
                catch { }
            }, 8500);
            NAPI.Task.Run(() => {
                try
                {
                    if (player != null)
                    {
                        player.SetSharedData("PLAYER_IN_CASINO", true);
                        player.SetData("PLAYER_IN_CASINO", true);
                        Trigger.ClientEvent(player, "cameracasinoexit", true);
                        Trigger.ClientEvent(player, "screenFadeIn", 1000);
                        NAPI.Entity.SetEntityPosition(player, _exitPosition);
                        Trigger.ClientEvent(player, "enterCasinoWall");
                        NAPI.Entity.SetEntityRotation(player, new Vector3(0, 0, -27.5));
                    }
                }
                catch { }
            }, 11500);
        }
    }
}
