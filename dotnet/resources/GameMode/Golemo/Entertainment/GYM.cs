using System;
using GTANetworkAPI;
using GolemoSDK;
using Golemo.Core;
using System.Collections.Generic;

namespace Golemo.GYM
{
    class GYM : Script
    {
        private static nLog Log = new nLog("Gym");
        private static List<bool> states = new List<bool>() //todo Licences Names
        {
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
        };
        public static Vector3[] seatmusculebench = new Vector3[]
        {
            new Vector3(1640.52, 2522.35, 45.06),
            new Vector3(1635.75, 2526.75, 45.06),
            new Vector3(1638.15, 2529.75, 45.06),
            new Vector3(1640.75, 2532.75, 45.06),
            new Vector3(1640.75, 2532.75, 45.06),
            new Vector3(1643.05, 2535.35, 45.06),
        };
        public static Vector3[] CHINUP = new Vector3[]
        {
            new Vector3(1643.39, 2527.75, 45.56),
            new Vector3(1649.16, 2529.57, 45.56),
        };
        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                var CHINUPS = NAPI.ColShape.CreateCylinderColShape(CHINUP[0], 1f, 2, 0);
                CHINUPS.OnEntityEnterColShape += (s, e) => { try { if (!e.IsInVehicle) { NAPI.Data.SetEntityData(e, "INTERACTIONCHECK", 945); } } catch (Exception ex) { Log.Write("ExitCayoPerico_OnEntityEnterColShape: " + ex.Message, nLog.Type.Error); } }; CHINUPS.OnEntityExitColShape += OnEntityExitCasinoMainShape;

                var CHINUPS2 = NAPI.ColShape.CreateCylinderColShape(CHINUP[1], 1f, 2, 0);
                CHINUPS2.OnEntityEnterColShape += (s, e) => { try { if (!e.IsInVehicle) { NAPI.Data.SetEntityData(e, "INTERACTIONCHECK", 952); } } catch (Exception ex) { Log.Write("ExitCayoPerico_OnEntityEnterColShape: " + ex.Message, nLog.Type.Error); } }; CHINUPS2.OnEntityExitColShape += OnEntityExitCasinoMainShape;


                var BENCH = NAPI.ColShape.CreateCylinderColShape(seatmusculebench[0], 1f, 2, 0);
                BENCH.OnEntityEnterColShape += (s, e) => { try { if (!e.IsInVehicle) { NAPI.Data.SetEntityData(e, "INTERACTIONCHECK", 946); } } catch (Exception ex) { Log.Write("ExitCayoPerico_OnEntityEnterColShape: " + ex.Message, nLog.Type.Error); } }; BENCH.OnEntityExitColShape += OnEntityExitCasinoMainShape;

                var BENCH2 = NAPI.ColShape.CreateCylinderColShape(seatmusculebench[1], 1f, 2, 0);
                BENCH2.OnEntityEnterColShape += (s, e) => { try { if (!e.IsInVehicle) { NAPI.Data.SetEntityData(e, "INTERACTIONCHECK", 947); } } catch (Exception ex) { Log.Write("ExitCayoPerico_OnEntityEnterColShape: " + ex.Message, nLog.Type.Error); } }; BENCH2.OnEntityExitColShape += OnEntityExitCasinoMainShape;

                var BENCH3 = NAPI.ColShape.CreateCylinderColShape(seatmusculebench[2], 1f, 2, 0);
                BENCH3.OnEntityEnterColShape += (s, e) => { try { if (!e.IsInVehicle) { NAPI.Data.SetEntityData(e, "INTERACTIONCHECK", 948); } } catch (Exception ex) { Log.Write("ExitCayoPerico_OnEntityEnterColShape: " + ex.Message, nLog.Type.Error); } }; BENCH3.OnEntityExitColShape += OnEntityExitCasinoMainShape;

                var BENCH4 = NAPI.ColShape.CreateCylinderColShape(seatmusculebench[3], 1f, 2, 0);
                BENCH4.OnEntityEnterColShape += (s, e) => { try { if (!e.IsInVehicle) { NAPI.Data.SetEntityData(e, "INTERACTIONCHECK", 949); } } catch (Exception ex) { Log.Write("ExitCayoPerico_OnEntityEnterColShape: " + ex.Message, nLog.Type.Error); } }; BENCH4.OnEntityExitColShape += OnEntityExitCasinoMainShape;

                var BENCH5 = NAPI.ColShape.CreateCylinderColShape(seatmusculebench[4], 1f, 2, 0);
                BENCH.OnEntityEnterColShape += (s, e) => { try { if (!e.IsInVehicle) { NAPI.Data.SetEntityData(e, "INTERACTIONCHECK", 950); } } catch (Exception ex) { Log.Write("ExitCayoPerico_OnEntityEnterColShape: " + ex.Message, nLog.Type.Error); } }; BENCH5.OnEntityExitColShape += OnEntityExitCasinoMainShape;

                var BENCH6 = NAPI.ColShape.CreateCylinderColShape(seatmusculebench[5], 1f, 2, 0);
                BENCH6.OnEntityEnterColShape += (s, e) => { try { if (!e.IsInVehicle) { NAPI.Data.SetEntityData(e, "INTERACTIONCHECK", 951); } } catch (Exception ex) { Log.Write("ExitCayoPerico_OnEntityEnterColShape: " + ex.Message, nLog.Type.Error); } }; BENCH6.OnEntityExitColShape += OnEntityExitCasinoMainShape;

                
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }
        public static void OnEntityExitCasinoMainShape(ColShape shape, Player player)
        {
            NAPI.Data.SetEntityData(player, "INTERACTIONCHECK", 0);
        }
        public static void CallBackShape(Player player, int id)
        {
            if (player.HasData("CHINUP") && player.GetData<bool>("CHINUP") == true) return;
            if (states[id + 6] == true)
            {
                Notify.Error(player, "Это место занято");
                return;
            }
            states[id + 6] = true;
            player.SetData("CHINUP", true);
            NAPI.Entity.SetEntityPosition(player, CHINUP[id]);
            NAPI.Entity.SetEntityRotation(player, new Vector3(0, 0, 50));
            player.PlayAnimation("amb@prop_human_muscle_chin_ups@male@base", "base", 1);
            Trigger.ClientEvent(player, "freeze", true);
            NAPI.Task.Run(() =>
            {
                player.StopAnimation();
                Trigger.ClientEvent(player, "freeze", false);
                player.SetData("CHINUP", true);
                states[id + 6] = false;
            }, 10000);
        }
        public static void CallBackShapeBench(Player player, int id)
        {
            if (player.HasData("BENCHSEAT") && player.GetData<bool>("BENCHSEAT") == true) return;
            if (states[id] == true)
            {
                Notify.Error(player, "Это место занято");
                return;
            }
            states[id] = true;
            player.SetData("BENCHSEAT", true);
            NAPI.Entity.SetEntityPosition(player, player.GetData<Vector3>("GYM_POSITION"));
            NAPI.Entity.SetEntityRotation(player, seatmusculebench[id]);
            Trigger.ClientEvent(player, "freeze", true);
            player.PlayAnimation("amb@prop_human_seat_muscle_bench_press@idle_a", "idle_a", 1);
            BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_barbell_20kg"), 28422, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
            NAPI.Task.Run(() =>
            {
                player.StopAnimation();
                BasicSync.DetachObject(player);
                states[id] = false;
                Trigger.ClientEvent(player, "freeze", false);
                player.SetData("BENCHSEAT", true);
            }, 10000);
        }
    }
}
