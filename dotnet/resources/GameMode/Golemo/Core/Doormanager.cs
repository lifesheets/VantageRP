using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using GolemoSDK;

namespace Golemo.Core
{
    class Doormanager : Script
    {
        private static nLog Log = new nLog("Doormanager");

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                RegisterDoor(961976194, new Vector3(255.2283, 223.976, 102.3932)); // safe door 
                SetDoorLocked(0, true, 0);

                RegisterDoor(631614199, new Vector3(461.8065, -997.6583, 25.06443)); // police prison door
                SetDoorLocked(3, true, 0);

                RegisterDoor(-1663022887, new Vector3(150.8389, -1008.352, -98.85)); // hotel
                SetDoorLocked(4, true, 0);

                RegisterDoor(452874391, new Vector3(827.5342, -2160.493, 29.76884)); // gunshop door
                SetDoorLocked(5, true, 0);

                RegisterDoor(452874391, new Vector3(6.81789, -1098.209, 29.94685)); // gunshop door
                SetDoorLocked(6, true, 0);

                NAPI.World.DeleteWorldProp(NAPI.Util.GetHashKey("tr_prop_tr_gate_r_01a"), new Vector3(-2148.653, 1110.646, -23.5492), 30f);
                NAPI.World.DeleteWorldProp(NAPI.Util.GetHashKey("tr_prop_tr_gate_l_01a"), new Vector3(-2148.653, 1101.464, -23.5492), 30f);

                NAPI.World.DeleteWorldProp(NAPI.Util.GetHashKey("vw_prop_casino_3cardpoker_01"), new Vector3(1146.329, 261.2543, -52.84094), 30f);
                NAPI.World.DeleteWorldProp(NAPI.Util.GetHashKey("vw_prop_casino_3cardpoker_01"), new Vector3(1143.338, 264.2453, -52.84094), 30f);
                NAPI.World.DeleteWorldProp(NAPI.Util.GetHashKey("vw_prop_casino_blckjack_01"), new Vector3(1148.837, 269.747, -52.84095), 30f);
                NAPI.World.DeleteWorldProp(NAPI.Util.GetHashKey("vw_prop_casino_blckjack_01"), new Vector3(1151.84, 266.747, -52.84095), 30f);
                NAPI.World.DeleteWorldProp(NAPI.Util.GetHashKey("vw_prop_casino_blckjack_01b"), new Vector3(1144.429, 247.3352, -52.041), 30f);
                NAPI.World.DeleteWorldProp(NAPI.Util.GetHashKey("vw_prop_casino_3cardpoker_01b"), new Vector3(1148.74, 251.6947, -52.04094), 30f);
                NAPI.World.DeleteWorldProp(NAPI.Util.GetHashKey("vw_prop_casino_3cardpoker_01b"), new Vector3(1133.74, 266.6947, -52.04094), 30f);
                NAPI.World.DeleteWorldProp(NAPI.Util.GetHashKey("vw_prop_casino_blckjack_01b"), new Vector3(1129.406, 262.3578, -52.041), 30f);
                //NAPI.World.CreatObject(NAPI.Util.GetHashKey("h4_airstrip_hanger"), new Vector3(4440.415, -4461.775, 7.763086), 30f);

                NAPI.Object.CreateObject(NAPI.Util.GetHashKey("h4_mph4_air_airstrip_01"), new Vector3(4440.415, -4461.775, 7.763086), new Vector3(0, 0, 0), 255);
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        private static List<Door> allDoors = new List<Door>();
        public static int RegisterDoor(int model, Vector3 Position)
        {
            allDoors.Add(new Door(model, Position));
            var col = NAPI.ColShape.CreateCylinderColShape(Position, 5, 5, 0);
            col.SetData("DoorID", allDoors.Count - 1);
            col.OnEntityEnterColShape += Door_onEntityEnterColShape;
            return allDoors.Count - 1;
        }

        private static void Door_onEntityEnterColShape(ColShape shape, Player entity)
        {
            try
            {
                if (NAPI.Entity.GetEntityType(entity) != EntityType.Player) return;
                var door = allDoors[shape.GetData<int>("DoorID")];
                Trigger.ClientEvent(entity, "setDoorLocked", door.Model, door.Position.X, door.Position.Y, door.Position.Z, door.Locked, door.Angle);
            }
            catch (Exception e) { Log.Write("Door_onEntityEnterColshape: " + e.ToString(), nLog.Type.Error); }
        }

        public static void SetDoorLocked(int id, bool locked, float angle)
        {
            if (allDoors.Count < id + 1) return;
            allDoors[id].Locked = locked;
            allDoors[id].Angle = angle;
            Main.ClientEventToAll("setDoorLocked", allDoors[id].Model, allDoors[id].Position.X, allDoors[id].Position.Y, allDoors[id].Position.Z, allDoors[id].Locked, allDoors[id].Angle);
        }

        public static bool GetDoorLocked(int id)
        {
            if (allDoors.Count < id + 1) return false;
            return allDoors[id].Locked;
        }

        internal class Door
        {
            public Door(int model, Vector3 position)
            {
                Model = model;
                Position = position;
                Locked = false;
                Angle = 50.0f;
            }

            public int Model { get; set; }
            public Vector3 Position { get; set; }
            public bool Locked { get; set; }
            public float Angle { get; set; }
        }
    }
}
