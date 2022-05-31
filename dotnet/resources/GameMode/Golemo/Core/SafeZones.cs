using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using GolemoSDK;

namespace Golemo.Core
{
    class SafeZones : Script
    {
        private static nLog Log = new nLog("SafeZones");
        public static void CreateSafeZone(Vector3 position, int height, int width, bool showNotify = true)
        {
            var colShape = NAPI.ColShape.CreateCylinderColShape(position, height, width, 0);
            colShape.OnEntityEnterColShape += (shape, player) =>
            {
                try
                {
                    Trigger.ClientEvent(player, "safeZone", true);
                    Trigger.ClientEvent(player, "UpdateGreenZone", true);
                }
                catch (Exception e) { Log.Write($"SafeZoneEnter: {e.Message}", nLog.Type.Error); }

            };
            colShape.OnEntityExitColShape += (shape, player) =>
            {
                try
                {
                    Trigger.ClientEvent(player, "safeZone", false);
                    Trigger.ClientEvent(player, "UpdateGreenZone", false);
                }
                catch (Exception e) { Log.Write($"SafeZoneExit: {e.Message}", nLog.Type.Error); }
            };
        }

        [ServerEvent(Event.ResourceStart)]
        public void Event_onResourceStart()
        {
            CreateSafeZone(new Vector3(-251.72365, -2022.4434, 29.02538), 100, 20); //mazebank
            CreateSafeZone(new Vector3(-1036.0176, -1385.097, 4.437826), 100, 20); //realtor
            CreateSafeZone(new Vector3(-1322.4526, 308.39078, 64.37179), 100, 20); //hotel
            CreateSafeZone(new Vector3(693.4206, 129.92444, 78.9936), 100, 20); //elecctric         
            CreateSafeZone(new Vector3(-6.427995, -2535.1062, 2.2772188), 80, 20); //drugs
            CreateSafeZone(new Vector3(-924.0111, -2052.3843, 8.558694), 110, 20); //buy license
            CreateSafeZone(new Vector3(-325.6188, 6153.7837, 31.03628), 80, 20); //church
            CreateSafeZone(new Vector3(-1714.3624, -242.03223, 51.718357), 120, 20); //church2
            CreateSafeZone(new Vector3(-312.71414, 2810.3735, 58.743404), 80, 20); //church3
            CreateSafeZone(new Vector3(1120.477, 237.97615, -50.941776), 150, 10); //casinoint

            CreateSafeZone(new Vector3(300.99234, -588.9717, 41.0318), 120, 20); //ems
            CreateSafeZone(new Vector3(-561.2012, -194.0378, 37.10239), 150, 20); //gov
            CreateSafeZone(new Vector3(934.07214, 17.082233, 80.539925), 180, 35); //casino
            CreateSafeZone(new Vector3(-208.82649, -1017.73334, 28.909218), 120, 20); //spawn
            CreateSafeZone(new Vector3(2343.619, 5001.0396, 43.523197), 200, 20); //farm orange
            CreateSafeZone(new Vector3(-754.98755, 5908.102, 16.250412), 520, 40); //Mush
            CreateSafeZone(new Vector3(2132.5024, 4925.28, 39.485737), 300, 30); //Farm
            CreateSafeZone(new Vector3(-7.0249476, -2523.6055, -11.183561), 120, 30); //Farm
            CreateSafeZone(new Vector3(457.93304, -982.5159, 29.569586), 90, 40); //LSPD
        }

        [ServerEvent(Event.PlayerEnterColshape)]
        public static void SetEnterInteractionCheck(ColShape shape, Player player)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (player.HasData("INTERACTIONCHECK") && player.GetData<int>("INTERACTIONCHECK") <= 0) return;
            if (player.HasData("CUFFED") && player.GetData<bool>("CUFFED")) return;
            if (player.HasData("IS_DYING") || player.HasData("FOLLOWING")) return;

            if (player.HasData("GARAGEID"))
            {
                Houses.House house = Houses.HouseManager.GetHouse(player);
                if (house == null) return;
                if (player.GetData<int>("GARAGEID") != house.GarageID) return;
            }
            Trigger.ClientEvent(player, "playerInteractionCheck", true);
        }

        [ServerEvent(Event.PlayerExitColshape)]
        public static void SetExitInteractionCheck(ColShape shape, Player player)
        {
            if (!Main.Players.ContainsKey(player)) return;
            Trigger.ClientEvent(player, "playerInteractionCheck", false);
        }
    }
}
