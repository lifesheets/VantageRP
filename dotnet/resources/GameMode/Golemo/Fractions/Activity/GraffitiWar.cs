using GTANetworkAPI;
using GTANetworkMethods;
using GolemoSDK;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Golemo.Fractions.Activity
{
    class GraffitiWar : Script
    {
        public static bool isWar = false;
        public static Dictionary<int, uint> Name = new Dictionary<int, uint>()
        {
            {1, 2144338576},
            {2, 2534642577},
            {3, 2515665739},
            {4, 1950380453},
            {5, 1219215027},
        };
        public static uint GetModel(int nom)
        {
            if (!Name.ContainsKey(nom))
            {
                return 0;
            }
            else
            {
                return Name[nom];
            }
        }
        private static nLog Log = new nLog("GraffitiWar");
        [ServerEvent(Event.ResourceStart)]
        public void ResSX()
        {
            try
            {
                var result = MySQL.QueryRead($"SELECT * FROM graf");
                if (result == null || result.Rows.Count == 0)
                {
                    Log.Write("DB GW return null result.", nLog.Type.Warn);
                    return;
                }
                foreach (DataRow Row in result.Rows)
                {
                    int id = Convert.ToInt32(Row["id"].ToString());
                    Vector3 pos = JsonConvert.DeserializeObject<Vector3>(Row["pos"].ToString());
                    Vector3 rot = JsonConvert.DeserializeObject<Vector3>(Row["rot"].ToString());
                    int gang = Convert.ToInt32(Row["band"].ToString());
                    new Graffiti(id, pos, rot, gang);
                }
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }

        } 
    }

    internal class Graffiti 
    {
        public static Dictionary<int, Graffiti> List = new Dictionary<int, Graffiti>();
        public int ID { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public int Gang { get; set; } = 0;
        [JsonIgnore]
        public GTANetworkAPI.Object Handle { get; set;}
        [JsonIgnore]
        public GTANetworkAPI.ColShape Shape { get; set;}

        public Graffiti(int id, Vector3 pos, Vector3 rot, int gang)
        {
            ID = id; Position = pos; Rotation = rot; Gang = gang;

            Handle = NAPI.Object.CreateObject(GraffitiWar.GetModel(Gang), Position, Rotation);
            Shape = NAPI.ColShape.CreateCylinderColShape(Position, 8, 5, 0);
            Shape.OnEntityEnterColShape += (s, entity) =>
            {
                try
                {
                    entity.SetData("graffiti", this);
                }
                catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
            };
            Shape.OnEntityExitColShape += (s, entity) =>
            {
                try
                {
                    entity.ResetData("graffiti");
                }
                catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
            };
            List.Add(ID, this);
        }

        public void SetGang(int gang)
        {
            try
            {
                Graffiti parent = List[ID];
                Handle.Delete();
                Gang = gang;
                Handle = NAPI.Object.CreateObject(GraffitiWar.GetModel(Gang), Position, Rotation);
                parent.Save();
            }
            catch {}
        }

        public void Save() 
        {
            try 
            {
                MySQL.Query($"UPDATE graf SET band='{Gang}' WHERE id={ID}");
            }
            catch{}
        }

    }

}
    
