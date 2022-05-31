using GTANetworkAPI;
using Newtonsoft.Json;
using GolemoSDK;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Golemo.Core
{
    public class ColShapeType
    {
        public Vector3 Position { get; }

        public ColShapeType(Vector3 position)
        {
            Position = position;
        }
    }
    public class BusinessColShape
    {
        public int ID { get; }
        public int BizID { get; }
        public Vector3 Position { get; }
        public Vector3 Rotation { get; }
        [JsonIgnore] 
        public int Dimension { get; set; }

        public static int DimensionID = 0;

        [JsonIgnore]
        private ColShape shape;

        [JsonIgnore]
        private ColShape intShape;
        [JsonIgnore]
        private Marker intMarker;
        [JsonIgnore]
        private TextLabel label;


        public BusinessColShape(int id, Vector3 position, Vector3 rotation, int bizid)
        {
            ID = id;
            Position = position;
            Rotation = rotation;
            BizID = bizid;

            shape = NAPI.ColShape.CreateCylinderColShape(position - new Vector3(0, 0, 1), 1, 3, 0);
            shape.OnEntityEnterColShape += (s, ent) =>
            {
                try
                {
                    NAPI.Data.SetEntityData(ent, "BIZ_ID", bizid);
                    NAPI.Data.SetEntityData(ent, "COL_ID", id);
                    NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 507);
                    //Business biz = BusinessManager.BizList[ent.GetData<int>("BIZ_ID")];
                }
                catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); }
            };
            shape.OnEntityExitColShape += (s, ent) =>
            {
                try
                {
                    if (NAPI.Entity.GetEntityType(ent) != EntityType.Player) return;
                    NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 0);
                    NAPI.Data.ResetEntityData(ent, "BIZ_ID");
                    NAPI.Data.ResetEntityData(ent, "COL_ID");
                }
                catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); }
            };

           // label = NAPI.TextLabel.CreateTextLabel($"{BusinessManager.BusinessTypeNames[BizID]}", new Vector3(Position.X, Position.Y, Position.Z + 1), 5F, 0.5F, 0, new Color(255, 255, 255, 120), true, 0);

        }


        public void Create()
        {
            MySQL.Query($"INSERT INTO `business_colshape`(`id`,`position`,`rotation`,`bizid`) VALUES ({ID},'{JsonConvert.SerializeObject(Position)}','{JsonConvert.SerializeObject(Rotation)}','{BizID}')");
        }
        public void Save()
        {

        }
        public void Destroy()
        {
            shape.Delete();
            intShape.Delete();
            intMarker.Delete();
            label.Delete();
        }

        public void CreateColShape()
        {
            intMarker = NAPI.Marker.CreateMarker(29, new Vector3(Position.X, Position.Y, Position.Z + 0.5) - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1f, new Color(255, 255, 255, 120), false, (uint)Dimension);

           // label = NAPI.TextLabel.CreateTextLabel($"{BusinessManager.BusinessTypeNames[BizID]}", new Vector3(Position.X, Position.Y, Position.Z + 1), 5F, 0.5F, 0, new Color(255, 255, 255, 120), true, 0);
            label = NAPI.TextLabel.CreateTextLabel($"", new Vector3(Position.X, Position.Y, Position.Z + 1), 5F, 0.5F, 0, new Color(255, 255, 255, 120), true, 0);

            intShape = NAPI.ColShape.CreateCylinderColShape(Position - new Vector3(0, 0, 1.12), 1f, 4f, (uint)Dimension);
            intShape.OnEntityEnterColShape += (s, ent) =>
            {
                try
                {
                    NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 507);
                }
                catch (Exception ex) { Console.WriteLine("intShape.OnEntityEnterColShape: " + ex.Message); }
            };
            intShape.OnEntityExitColShape += (s, ent) =>
            {
                try
                {
                    NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 0);
                }
                catch (Exception ex) { Console.WriteLine("intShape.OnEntityExitColShape: " + ex.Message); }
            };

        }

        public class BusinessColShape_Manager : Script
        {
            private static nLog Log = new nLog("BusinessColShapes");
            public static Dictionary<int, BusinessColShape> ColShapes = new Dictionary<int, BusinessColShape>();
            public static Dictionary<int, ColShapeType> ColShapeTypes = new Dictionary<int, ColShapeType>()
            {

            };
            [ServerEvent(Event.ResourceStart)]
            public void onResourceStart()
            {
                try
                {
                    var result = MySQL.QueryRead($"SELECT * FROM `business_colshape`");
                    if (result == null || result.Rows.Count == 0)
                    {
                        Log.Write("DB return null result.", nLog.Type.Warn);
                        return;
                    }
                    foreach (DataRow Row in result.Rows)
                    {
                        var id = Convert.ToInt32(Row["id"]);
                        var position = JsonConvert.DeserializeObject<Vector3>(Row["position"].ToString());
                        var rotation = JsonConvert.DeserializeObject<Vector3>(Row["rotation"].ToString());
                        var bizid = Convert.ToInt32(Row["bizid"]);
                                       
                        var colshape = new BusinessColShape(id, position, rotation, bizid);
                        colshape.Dimension = DimensionID;
                        colshape.CreateColShape();

                        ColShapes.Add(id, colshape);
                    }
                    Log.Write($"Loaded {ColShapes.Count} ColShapes.", nLog.Type.Success);
                }
                catch (Exception e) { Log.Write($"ResourceStart: " + e.Message, nLog.Type.Error); }
            }

            public static void interactionPressed(Player player, int id)
            {
                try
                {
                    switch (id)
                    {
                        case 506:
                            if (!player.HasData("BIZ_ID")) return;
                            BusinessColShape colshape = ColShapes[player.GetData<int>("BIZ_ID")];
                            if (colshape == null) return;

                            return;
                        case 507:
                            int bizid_ = player.GetData<int>("BIZ_ID");
                            Business biz = BusinessManager.BizList[bizid_];

                            CharacterData acc = Main.Players[player];
                            if (acc.AdminLVL > 0)
                            {
                                NAPI.ClientEvent.TriggerClientEvent(player, "OpenBuyBusinessMenu", BusinessManager.BusinessTypeNames[biz.Type], biz.Owner, biz.SellPrice, Fractions.Manager.getName(biz.Mafia), biz.ID);
                            }
                            else
                            {
                                NAPI.ClientEvent.TriggerClientEvent(player, "OpenBuyBusinessMenu", BusinessManager.BusinessTypeNames[biz.Type], biz.Owner, biz.SellPrice, Fractions.Manager.getName(biz.Mafia), "");
                            }
                            
                            return;
                    }
                }
                catch (Exception e)
                {
                    Log.Write("EXCEPTION AT \"COLSHAPE_INTERACTION\":\n" + e.ToString(), nLog.Type.Error);
                }
            }

            public static void Event_PlayerDisconnected(Player player)
            {

            }

            #region Commands

            [Command("createbizcol")]
            public static void CMD_CreateBizCol(Player player, int bizid)
            {
                if (!Group.CanUseCmd(player, "allspawncar")) return;
                int id = 0;
                do
                {
                    id++;

                } while (ColShapes.ContainsKey(id));

                BusinessColShape colshape = new BusinessColShape(id, player.Position, player.Rotation, bizid);
                colshape.Dimension = DimensionID;
                colshape.Create();
                colshape.CreateColShape();

                ColShapes.Add(colshape.ID, colshape);
                if (!ColShapes.ContainsKey(bizid)) return;
            }

            [Command("removebizcol")]
            public static void CMD_RemoveBizCol(Player player)
            {
                if (!Group.CanUseCmd(player, "allspawncar")) return;
                if (!player.HasData("COL_ID"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $" removebizcol ", 3000);
                    return;
                }
                if (!ColShapes.ContainsKey(player.GetData<int>("COL_ID"))) return;
                BusinessColShape colshape = ColShapes[player.GetData<int>("COL_ID")];

                colshape.Destroy();
                ColShapes.Remove(player.GetData<int>("COL_ID"));
                MySQL.Query($"DELETE FROM `business_colshape` WHERE `id`='{colshape.ID}'");
            }

            #endregion
        }
    }
}
