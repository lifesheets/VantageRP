using System;
using System.Collections.Generic;
using System.Data;
using GTANetworkAPI;
using Golemo.Core;
using GolemoSDK;

namespace Golemo.Jobs.FarmerJob
{
    public class Farmer : Script
    {
        #region Settings
        private static readonly nLog Log = new nLog("Mushroomer");
        private static List<CharacterData> Farmers = new List<CharacterData>();
        private static ColShape checkpoint;
        private static readonly Random rnd = new Random();
        private static int minsec = 200;
        private static int maxsec = 300;
        private static int maxlvl = 20;
        #endregion

        #region Инициализация Работы Фермера
        [ServerEvent(Event.ResourceStart)]
        public void Event_FarmerStart()
        {
            try
            {
                #region Создание блипа, текста, колшейпа
                NAPI.Blip.CreateBlip(468, new Vector3(-607.50726, 5794.2153, 28.98518), 0.9f, 10, "Грибы", 255, 0, true, 0, 0); // Блип на карте
                for (int i = 0; i < Checkpoints.Count; i++)
                {
                    NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_mush1"), Checkpoints[i], new Vector3(0, 0, 92.086174), 255, 0);
                }
                // NAPI.TextLabel.CreateTextLabel("~w~Грибник", new Vector3(438.3554, 6510.949, 29.6), 10f, 0.1f, 0, new Color(255, 255, 255), true, NAPI.GlobalDimension); // Над головой у Ped
                List<Vector3> shapes = new List<Vector3>()
                {
                    new Vector3(-607.50726, 5794.2153, 28.98518), //Голем
                };

                var golemShape = NAPI.ColShape.CreateCylinderColShape(shapes[0], 2f, 2, 0);
                golemShape.OnEntityEnterColShape += (shape, player) =>
                {
                    try
                    {
                        player.SetData("INTERACTIONCHECK", 801);
                    }
                    catch (Exception e)
                    {
                        Log.Write(e.ToString(), nLog.Type.Error);
                    }
                };
                golemShape.OnEntityExitColShape += (shape, player) =>
                {
                    try
                    {
                        player.SetData("INTERACTIONCHECK", 0);
                    }
                    catch (Exception e)
                    {
                        Log.Write(e.ToString(), nLog.Type.Error);
                    }
                };

                #endregion
                for (int i = 0; i < Checkpoints.Count; i++)
                {
                    checkpoint = NAPI.ColShape.CreateCylinderColShape(Checkpoints[i], 1, 2, 0);
                    checkpoint.SetData($"plantID", i);
                    checkpoint.OnEntityEnterColShape += PlayerEnterCheckpoint;
                }
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
        }
        #endregion

        #region Открыть меню Фермера
        public static void OpenFarmerMenu(Player player)
        {
            try
            {
                if (player.IsInVehicle) return;
                var item = nInventory.Find(Main.Players[player].UUID, ItemType.Seed);
                int itemcount = item != null ? item.Count : 0;
                LoadLvl(player, "farmer");
                int[] jobinfo = player.GetData<int[]>("job_farmer"); //данные игрока
                List<object> data = new List<object>()
                {
                    jobinfo[0],
                    jobinfo[1],
                    jobinfo[2],
                    itemcount,
                    Farmers.Count,
                    player.GetData<bool>("ON_WORK"),
                    maxlvl
                };
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                Trigger.ClientEvent(player, "openJobsMenu", json);
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
        }
        #endregion

        #region Грибочки
        public static Dictionary<int, ItemType> mushrooms1 = new Dictionary<int, ItemType>
        {
            {1, ItemType.Mush1 },
            {2, ItemType.Mush2 },
            {3, ItemType.Mush3 },
            {4, ItemType.Mush4 },
            {5, ItemType.Mush5 },
            {6, ItemType.Mush6 },
            {7, ItemType.MushGold },
            {8, ItemType.Mush1 },
            {9, ItemType.Mush1 },
            {10, ItemType.Mush1 },
            {11, ItemType.Mush1 },
            {12, ItemType.Mush1 },
            {13, ItemType.Mush1 },
            {14, ItemType.Mush1 },
            {15, ItemType.Mush2 },
            {16, ItemType.Mush2 },
            {17, ItemType.Mush2 },
            {18, ItemType.Mush2 },
            {19, ItemType.Mush2 },
            {20, ItemType.Mush2 },
            {21, ItemType.Mush2 },
            {22, ItemType.Mush3 },
            {23, ItemType.Mush3 },
            {24, ItemType.Mush3 },
            {25, ItemType.Mush3 },
            {26, ItemType.Mush3 },
            {27, ItemType.Mush4 },
            {28, ItemType.Mush4 },
            {29, ItemType.Mush4 },
            {30, ItemType.Mush4 },
            {31, ItemType.Mush4 },
            {32, ItemType.Mush4 },
            {33, ItemType.Mush5 },
            {34, ItemType.Mush5 },
            {35, ItemType.Mush5 },
            {36, ItemType.Mush5 },
            {37, ItemType.Mush5 },
            {38, ItemType.Mush6 },
            {39, ItemType.Mush6 },
            {40, ItemType.Mush6 },
        };
        #endregion
        #region Начать или Закончить работу фермера
        [RemoteEvent("workstate")]
        public static void StartWork(Player player, bool state, string workname = null)
        {
            if (state)
            {
                var item = nInventory.Find(Main.Players[player].UUID, ItemType.Knife);
                if (item == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы не можете собирать грибы без ножа", 2000);
                    return;
                }
                for (int i = 0; i < Checkpoints.Count; i++)
                {
                    //Trigger.ClientEvent(player, "createPlant", Convert.ToInt32($"10{i}"), "Гриб", 1, Checkpoints[i], 1, 0, 0, 0, 0);
                    player.SetData($"seedplant{i}", false);
                    player.ResetData($"regenplant{i}");
                }
                Farmers.Add(Main.Players[player]);
                player.SetData("jobname", "farmer");
                player.SetData("ON_WORK", true);
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы взяли задание у деда на сбор грибов", 2000);
            }
            else
            {
                try
                {
                    Farmers.Remove(Main.Players[player]);
                    for (int i = 0; i < Checkpoints.Count; i++)
                    {
                        Trigger.ClientEvent(player, "deletePlant", Convert.ToInt32($"10{i}"));
                        Timers.Stop($"{player.Name}farmer{i}"); //остановка всех таймеров
                        player.SetData($"seedplant{i}", false); //можно Reset, надо чекнуть
                        player.ResetData($"regenplant{i}"); //сброс счетчика
                    }
                    SaveLvl(player, "farmer"); //сохранение данных
                    player.ResetData("job_farmer");
                    player.ResetData("jobname");
                    player.SetData("ON_WORK", false);
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы отменили задание у деда", 2000);
                }
                catch (Exception e)
                {
                    Log.Write(e.ToString(), nLog.Type.Error);
                }
            }
        }

        #endregion

        #region Игрок зашел в чекпоинт
        private static void PlayerEnterCheckpoint(ColShape colShape, Player player)
        {
            try
            {
                int colID = colShape.GetData<int>("plantID"); //номер колшейпа
                if (player.IsInVehicle) return; //если игрок в машине
                if (player.GetData<string>("jobname") != "farmer") return; //если игрок не работает фермером
                int[] jobinfo = player.GetData<int[]>("job_farmer");
                int lvl = jobinfo[0], exp = jobinfo[1], allpoints = jobinfo[2], sec = Convert.ToInt32(rnd.Next(minsec, maxsec) - lvl * 2);
                if (player.HasData($"regenplant{colID}")) //если колшейп регенерируется
                {
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"Осталось: {player.GetData<int>($"regenplant{colID}")} секунд", 2000);
                    return;
                }
                var item = nInventory.Find(Main.Players[player].UUID, ItemType.Knife); //ищем семена у игрока

                if (!player.GetData<bool>($"seedplant{colID}")) //если семена не были посажены сажаем
                {
                    if (!item.IsActive) //если у игрока нет семян
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У вас нет ножа в руках", 2000);
                        return; //не срабатывает
                    }

                    NAPI.Task.Run(() => { player.SetData($"regenplant{colID}", sec); UpdateCheckpointState(colShape, player); }, 15000); //заводится таймер
                    var rnd = new Random();
                    int mush = rnd.Next(1, Farmer.mushrooms1.Count);
                    nInventory.Add(player, new nItem(Farmer.mushrooms1[mush], 1));
                    NAPI.Task.Run(() => { Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Гриб собран", 2000); }, 5000);
                }
                Trigger.ClientEvent(player, "deletePlant", Convert.ToInt32($"10{colID}")); //удаляется маркер
                PlayFarmerAnimation(player);
                NAPI.Task.Run(() => { player.SetData($"regenplant{colID}", sec); UpdateCheckpointState(colShape, player); }, 15000); //заводится таймер
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
        }
        #endregion

        #region Таймер обновления
        private static void UpdateCheckpointState(ColShape colShape, Player player)
        {
            int colID = colShape.GetData<int>("plantID"); //номер колшейпа
            if (player.HasData($"regenplant{colID}")) //если колшейп имеет таймер
            {
                Timers.Start($"{player.Name}farmer{colID}", 5000, () =>
                {
                    if (player.HasData($"regenplant{colID}")) player.SetData($"regenplant{colID}", player.GetData<int>($"regenplant{colID}") - 5);
                    if (player.GetData<int>($"regenplant{colID}") < 1) //если таймер меньше 1
                    {
                        if (!player.GetData<bool>($"seedplant{colID}")) //если семена не посажены и процесс начат для регенарации земли
                        {
                            player.ResetData($"regenplant{colID}"); //сбрасываем таймер
                                                                    // Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Грибы выросли", 2000);
                                                                    //Trigger.ClientEvent(player, "createPlant", Convert.ToInt32($"10{colID}"), "Гриб", 1, Checkpoints[colID], 1, 0, 255, 0, 0);
                            Timers.Stop($"{player.Name}farmer{colID}"); //дубляж
                        }
                        else
                        {
                            player.ResetData($"regenplant{colID}"); //сброс таймера
                            player.SetData($"seedplant{colID}", true); //семена взросли
                                                                       // Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Соберите урожай", 2000);
                                                                       // Trigger.ClientEvent(player, "createPlant", Convert.ToInt32($"10{colID}"), "Гриб", 1, Checkpoints[colID], 1, 0, 0, 255, 0);
                            Timers.Stop($"{player.Name}farmer{colID}"); //дубляж
                        }
                        //Timers.Stop($"{player.Name}farmer{colID}");
                    }
                });
            }
        }
        #endregion

        #region Play Animation
        private static void PlayFarmerAnimation(Player player)
        {
            //перенести реализацию анимации на клиент
            Main.OnAntiAnim(player);
            player.PlayAnimation("amb@world_human_gardener_plant@male@base", "base", 39);
            NAPI.Task.Run(() =>
            {
                player.StopAnimation();
                player.Position += new Vector3(0, 0, 0.2);
                Main.OffAntiAnim(player);

            }, 6000);
        }
        #endregion

        #region Подгрузка уровня игрока
        public static void LoadLvl(Player player, string workname)
        {
            try
            {
                if (player.HasData($"job_{workname}")) return;
                int lvl = 1, exp = 0, allpoints = 0;
                CharacterData acc = Main.Players[player];
                DataTable result = MySQL.QueryRead($"SELECT * FROM `{workname}` WHERE uuid={acc.UUID}");
                if (result == null || result.Rows.Count == 0)
                {
                    MySQL.Query($"INSERT INTO `{workname}`(`uuid`, `level`, `exp`, `allpoints`) VALUES({acc.UUID}, {lvl}, {exp}, {allpoints})");
                    Log.Write($"Я зарегал игрока {player.Name}", nLog.Type.Warn);
                }
                else
                {
                    foreach (DataRow Row in result.Rows)
                    {
                        lvl = Convert.ToInt32(Row["level"]);
                        exp = Convert.ToInt32(Row["exp"]);
                        allpoints = Convert.ToInt32(Row["allpoints"]);
                    }
                    Log.Write($"Я загрузил игрока {player.Name}", nLog.Type.Warn);
                }
                player.SetData($"job_{workname}", new int[] { lvl, exp, allpoints });
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
        }
        #endregion

        #region Сохранение уровня игрока
        public static void SaveLvl(Player player, string workname)
        {
            try
            {
                int[] data = player.GetData<int[]>($"job_{workname}");
                if (data == null) return;
                CharacterData acc = Main.Players[player];
                DataTable result = MySQL.QueryRead($"SELECT * FROM `{workname}` WHERE uuid={acc.UUID}");
                if (result == null || result.Rows.Count == 0)
                {
                    MySQL.Query($"INSERT INTO `{workname}`(`uuid`, `level`, `exp`, `allpoints`) VALUES({acc.UUID}, {data[0]}, {data[1]}, {data[2]})");
                    Log.Write("Я зарегистрировал нового пользователя", nLog.Type.Warn);
                }
                else
                {
                    MySQL.Query($"UPDATE `{workname}` SET `level`={data[0]}, `exp`={data[1]}, `allpoints`={data[2]} WHERE uuid={acc.UUID}");
                    Log.Write($"Я обновил данные игрока {player.Name}", nLog.Type.Warn);
                }
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
        }
        #endregion

        #region Checkpoints
        private static List<Vector3> Checkpoints = new List<Vector3>()
        {
            new Vector3(-497.8794, 5940.881, 32.95699),
            new Vector3(-513.7657, 5949.9863, 34.814117),
            new Vector3(-560.182, 5993.643, 29.780622),
            new Vector3(-595.4567, 5951.719, 20.748014),
            new Vector3(-583.9242, 5901.6426, 27.30363),
            new Vector3(-581.3796, 5889.0967, 27.372414),
            new Vector3(-566.6963, 5882.358, 29.528416),
            new Vector3(-536.54877, 5903.899, 30.174746),
            new Vector3(-515.3322, 5920.233, 31.34397),
            new Vector3(-516.633, 5875.146, 31.7395),
            new Vector3(-556.5287, 5817.542, 35.725395),
            new Vector3(-604.0314, 5769.6006, 31.81936),
            new Vector3(-622.544, 5754.72, 30.417358),
            new Vector3(-643.93365, 5789.4443, 22.790462),
            new Vector3(-638.6498, 5820.0303, 21.837831),
            new Vector3(-597.66595, 5835.9907, 26.18203),
            new Vector3(-604.97906, 5899.796, 24.135624),
            new Vector3(-607.8583, 5929.7812, 23.311817),
            new Vector3(-595.2485, 5982.6, 16.39111),

            new Vector3(-623.92114, 5749.6226, 30.214207),
            new Vector3(-630.4431, 5732.187, 28.006386),
            new Vector3(-682.47015, 5734.459, 17.155357),
            new Vector3(-696.4003, 5714.5254, 20.627272),
            new Vector3(-692.5301, 5689.8726, 26.86021),
            new Vector3(-690.3135, 5670.19, 30.022827),
            new Vector3(-696.0153, 5663.673, 29.794142),
            new Vector3(-668.93976, 5636.367, 32.496487),
            new Vector3(-673.1972, 5624.421, 32.517666),
            new Vector3(-670.72363, 5614.9023, 33.742496),
            new Vector3(-690.4553, 5600.387, 30.32046),
            new Vector3(-696.45667, 5599.172, 29.22115),
            //new Vector3(-700.88574, 5589.0015, 30.846739),
            new Vector3(-700.4914, 5584.6245, 31.813238),
            new Vector3(-712.0137, 5615.586, 28.438805),
            new Vector3(-743.1314, 5625.8965, 27.871975),
            new Vector3(-747.3226, 5661.8696, 23.594765),
            new Vector3(-744.97504, 5690.03, 21.515915),
            new Vector3(-731.17334, 5733.2153, 14.755583),
            new Vector3(-707.9427, 5751.018, 17.134197),
            new Vector3(-659.4278, 5778.0723, 19.080515),
            new Vector3(-640.9955, 5820.551, 21.69515),
            new Vector3(-629.7339, 5881.401, 18.89496),
            new Vector3(-655.9108, 5904.8438, 15.98957),
            new Vector3(-692.5208, 5904.091, 16.843317),
            new Vector3(-695.1079, 5889.8057, 16.970171),
            new Vector3(-729.4695, 5904.7515, 15.753062),
            new Vector3(-709.177, 5925.8184, 15.129188),
            new Vector3(-696.44775, 5958.8604, 13.363238),
            new Vector3(-690.6112, 5987.651, 11.22951),
            new Vector3(-683.6509, 5997.1343, 11.101251),
            new Vector3(-716.5311, 5994.2856, 12.887409),
            new Vector3(-741.18884, 5986.336, 13.703304),
            new Vector3(-735.12286, 5931.1074, 15.650872),
            new Vector3(-759.3062, 5918.742, 18.013159),
            new Vector3(-787.2128, 5919.763, 19.870335),
            new Vector3(-811.9773, 5916.726, 17.224651),
            new Vector3(-861.3918, 5976.7095, 18.655973),
            new Vector3(-867.9875, 5968.987, 8.19727),
            new Vector3(-883.1722, 5999.9805, 29.70179),
            new Vector3(-889.0528, 5997.983, 28.818903),
            new Vector3(-884.7406, 6004.749, 32.150993),
            new Vector3(-892.1359, 6044.0723, 41.825275),
            new Vector3(-901.86584, 6030.9897, 39.64846),
            new Vector3(-843.4777, 6013.4604, 20.366648),
            new Vector3(-596.4123, 6027.749, 18.32277),
            new Vector3(-636.0051, 6074.2993, 7.2238073),
            new Vector3(-607.66, 6119.114, 6.8911247),
        };
        #endregion

    }
}
