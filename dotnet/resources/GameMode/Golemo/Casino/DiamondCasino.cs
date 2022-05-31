using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Golemo.Core;
using Golemo.GUI;
using GolemoSDK;
using Golemo;
using Trigger = GolemoSDK.Trigger;
using Golemo.MoneySystem;
using Newtonsoft.Json;
using System.Data;
using Golemo.Houses;

namespace Golemo
{
    class DiamondCasino : Script
    {
        private static nLog Log = new nLog("Casino");

        private static readonly Random random = new Random();
        private static readonly byte[] webnums = new byte[37] { 0, 14, 31, 2, 33, 18, 27, 6, 21, 10, 19, 23, 4, 25, 12, 35, 16, 29, 8, 34, 13, 32, 9, 20, 17, 30, 1, 26, 5, 7, 22, 11, 36, 15, 28, 3, 24 }; // cell number for each number 0, 1, 2 и т.д.
        private static int[] rednums = new int[18] { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 }; // Все красные числа на рулетке

        private static Dictionary<Player, (ushort, ushort, ushort)> PlayersList = new Dictionary<Player, (ushort, ushort, ushort)>();
        //RED. ZERO, BLACK
        private static Vector3 colPosition = new Vector3
        (
            927.6365,
            44.86219,
            80.9
        );
        private static Vector3 blipPosition = new Vector3
        (
            927.6365,
            44.86219,
            80.9
        );

        public static Dictionary<int, int> WheelPrizes = new Dictionary<int, int>(){
            { 1, 3 },
            { 2, 1 },
            { 3, 2 },
            { 4, 4 },
            { 5, 2 },
            { 6, 1 },
            { 7, 2 },
            { 8, 4 },
            { 9, 3 },
            { 10, 1 },
            { 11, 4 },
            { 12, 6 },
            { 13, 3 },
            { 14, 1 },
            { 15, 2 },
            { 16, 4 },
            { 17, 3 },
            { 18,1 },
            { 19, 5},
            { 20, 2 },
        };

        public static Dictionary<int, List<int>> PrizesNums = new Dictionary<int, List<int>>()
        {
            {1, new List<int>(){ 2, 6, 10, 14, 18} },
            {2, new List<int>(){ 3, 5, 7, 15, 20} },
            {3, new List<int>(){ 1, 9, 13, 17} },
            {4, new List<int>(){ 4, 8, 11, 16} },
            {5, new List<int>(){ 19 } },
            {6, new List<int>(){ 12} }
        };

        public static List<string> PrizesType = new List<string>()
        {
            "-",
            "еду",
            "деньги",
            "одежду",
            "донат",
            "машину",
            "неизвестное",
        };

        public static List<string> PrizesCar = new List<string>()
        {
            "adder",
            "bf400",
            "zentorno",
            "ruston",
        };


        public static List<int> RouletteTableBetMin = new List<int>()
        {
        };


        public static List<int> RouletteTableBetMax = new List<int>()
        {
        };

        public static List<List<Bet>> RouletteBets = new List<List<Bet>>()
        {
            new List<Bet>(){},
            new List<Bet>(){},
        };

        public static List<Vector3> RouletteTables = new List<Vector3>()
        {
             new Vector3(1144.814, 268.2634, -52.8409),
             new Vector3(1150.355, 262.7224, -52.8409),
            // new Vector3(1144.814, 268.2634, -52.8409), 
        };

        public static List<List<Vector3>> RouletteSeats = new List<List<Vector3>>()
        {
            new List<Vector3>(){
                new Vector3(1144.717, 267.277, -52.840),
                new Vector3(1143.748, 267.377, -52.840),
                new Vector3(1143.655, 268.335, -52.840),
                new Vector3(1144.342, 269.022, -52.840),
            },
            new List<Vector3>(){
                new Vector3(1150.45, 263.708, -52.84),
                new Vector3(1151.42, 263.608, -52.84),
                new Vector3(1151.51, 262.650, -52.84),
                new Vector3(1150.82, 261.963, -52.84),
            },
        };
        private static ColShape colShape;
        //private static Blip blip;

        //public static GTANetworkAPI.Object bWheel;
        //public static GTANetworkAPI.Object Wheel;

        public static bool isRoll = false;
        //public static Player rollPlayer;

        public static List<Thread> CasinoThreads = new List<Thread>();

        public static int ChipPrice = 100;
        public static Dictionary<int, List<int>> Spots = new Dictionary<int, List<int>>()
        {
            { 0, new List<int>{ 0 } },
            { 1, new List<int>{ -1 } },
            { 2, new List<int>{ 1 } },
            { 3, new List<int>{ 2 } },
            { 4, new List<int>{ 3 } },
            { 5, new List<int>{ 4 } },
            { 6, new List<int>{ 5 } },
            { 7, new List<int>{ 6 } },
            { 8, new List<int>{ 7 } },
            { 9, new List<int>{ 8 } },
            { 10, new List<int>{ 9 } },
            { 11, new List<int>{ 10 } },
            { 12, new List<int>{ 11 } },
            { 13, new List<int>{ 12 } },
            { 14, new List<int>{ 13 } },
            { 15, new List<int>{ 14 } },
            { 16, new List<int>{ 15 } },
            { 17, new List<int>{ 16 } },
            { 18, new List<int>{ 17 } },
            { 19, new List<int>{ 18 } },
            { 20, new List<int>{ 19 } },
            { 21, new List<int>{ 20 } },
            { 22, new List<int>{ 21 } },
            { 23, new List<int>{ 22 } },
            { 24, new List<int>{ 23 } },
            { 25, new List<int>{ 24 } },
            { 26, new List<int>{ 25 } },
            { 27, new List<int>{ 26 } },
            { 28, new List<int>{ 27 } },
            { 29, new List<int>{ 28 } },
            { 30, new List<int>{ 29 } },
            { 31, new List<int>{ 30 } },
            { 32, new List<int>{ 31 } },
            { 33, new List<int>{ 32 } },
            { 34, new List<int>{ 33 } },
            { 35, new List<int>{ 34 } },
            { 36, new List<int>{ 35 } },
            { 37, new List<int>{ 36 } },
            { 38, new List<int>{ 0, -1 } },
            { 39, new List<int>{ 1, 2 } },
            { 40, new List<int>{ 2, 3 } },
            { 41, new List<int>{ 4, 5 } },
            { 42, new List<int>{ 5, 6 } },
            { 43, new List<int>{ 7, 8 } },
            { 44, new List<int>{ 8, 9 } },
            { 45, new List<int>{ 10, 11 } },
            { 46, new List<int>{ 11, 12 } },
            { 47, new List<int>{ 13, 14 } },
            { 48, new List<int>{ 14, 15 } },
            { 49, new List<int>{ 16, 17 } },
            { 50, new List<int>{ 17, 18 } },
            { 51, new List<int>{ 19, 20 } },
            { 52, new List<int>{ 20, 21 } },
            { 53, new List<int>{ 22, 23 } },
            { 54, new List<int>{ 23, 24 } },
            { 55, new List<int>{ 25, 26 } },
            { 56, new List<int>{ 26, 27 } },
            { 57, new List<int>{ 28, 29 } },
            { 58, new List<int>{ 29, 30 } },
            { 59, new List<int>{ 31, 32 } },
            { 60, new List<int>{ 32, 33 } },
            { 61, new List<int>{ 34, 35 } },
            { 62, new List<int>{ 35, 36 } },
            { 63, new List<int>{ 1, 4 } },
            { 64, new List<int>{ 2, 5 } },
            { 65, new List<int>{ 3, 6 } },
            { 66, new List<int>{ 4, 7 } },
            { 67, new List<int>{ 5, 8 } },
            { 68, new List<int>{ 6, 9 } },
            { 69, new List<int>{ 7, 10 } },
            { 70, new List<int>{ 8, 11 } },
            { 71, new List<int>{ 9, 12 } },
            { 72, new List<int>{ 10, 13 } },
            { 73, new List<int>{ 11, 14 } },
            { 74, new List<int>{ 12, 15 } },
            { 75, new List<int>{ 13, 16 } },
            { 76, new List<int>{ 14, 17 } },
            { 77, new List<int>{ 15, 18 } },
            { 78, new List<int>{ 16, 19 } },
            { 79, new List<int>{ 17, 20 } },
            { 80, new List<int>{ 18, 21 } },
            { 81, new List<int>{ 19, 22 } },
            { 82, new List<int>{ 20, 23 } },
            { 83, new List<int>{ 21, 24 } },
            { 84, new List<int>{ 22, 25 } },
            { 85, new List<int>{ 23, 26 } },
            { 86, new List<int>{ 24, 27 } },
            { 87, new List<int>{ 25, 28 } },
            { 88, new List<int>{ 26, 29 } },
            { 89, new List<int>{ 27, 30 } },
            { 90, new List<int>{ 28, 31 } },
            { 91, new List<int>{ 29, 32 } },
            { 92, new List<int>{ 30, 33 } },
            { 93, new List<int>{ 31, 34 } },
            { 94, new List<int>{ 32, 35 } },
            { 95, new List<int>{ 33, 36 } },
            { 96, new List<int>{ 1, 2, 3 } },
            { 97, new List<int>{ 4, 5, 6 } },
            { 98, new List<int>{ 7, 8, 9 } },
            { 99, new List<int>{ 10, 11, 12 } },
            { 100, new List<int>{ 13, 14, 15 } },
            { 101, new List<int>{ 16, 17, 18 } },
            { 102, new List<int>{ 19, 20, 21 } },
            { 103, new List<int>{ 22, 23, 24 } },
            { 104, new List<int>{ 25, 26, 27 } },
            { 105, new List<int>{ 28, 29, 30 } },
            { 106, new List<int>{ 31, 32, 33 } },
            { 107, new List<int>{ 34, 35, 36 } },
            { 108, new List<int>{ 1, 2, 4, 5 } },
            { 109, new List<int>{ 2, 3, 5, 6 } },
            { 110, new List<int>{ 4, 5, 7, 8 } },
            { 111, new List<int>{ 5, 6, 8, 9 } },
            { 112, new List<int>{ 7, 8, 10, 11 } },
            { 113, new List<int>{ 8, 9, 11, 12 } },
            { 114, new List<int>{ 10, 11, 13, 14 } },
            { 115, new List<int>{ 11, 12, 14, 15 } },
            { 116, new List<int>{ 13, 14, 16, 17 } },
            { 117, new List<int>{ 14, 15, 17, 18 } },
            { 118, new List<int>{ 16, 17, 19, 20 } },
            { 119, new List<int>{ 17, 18, 20, 21 } },
            { 120, new List<int>{ 19, 20, 22, 23 } },
            { 121, new List<int>{ 20, 21, 23, 24 } },
            { 122, new List<int>{ 22, 23, 25, 26 } },
            { 123, new List<int>{ 23, 24, 26, 27 } },
            { 124, new List<int>{ 25, 26, 28, 29 } },
            { 125, new List<int>{ 26, 27, 29, 30 } },
            { 126, new List<int>{ 28, 29, 31, 32 } },
            { 127, new List<int>{ 29, 30, 32, 33 } },
            { 128, new List<int>{ 31, 32, 34, 35 } },
            { 129, new List<int>{ 32, 33, 35, 36 } },
            { 130, new List<int>{ 0, 00, 1, 2, 3 } },
            { 131, new List<int>{ 1, 2, 3, 4, 5, 6 } },
            { 132, new List<int>{ 4, 5, 6, 7, 8, 9 } },
            { 133, new List<int>{ 7, 8, 9, 10, 11, 12 } },
            { 134, new List<int>{ 10, 11, 12, 13, 14, 15 } },
            { 135, new List<int>{ 13, 14, 15, 16, 17, 18 } },
            { 136, new List<int>{ 16, 17, 18, 19, 20, 21 } },
            { 137, new List<int>{ 19, 20, 21, 22, 23, 24 } },
            { 138, new List<int>{ 22, 23, 24, 25, 26, 27 } },
            { 139, new List<int>{ 25, 26, 27, 28, 29, 30 } },
            { 140, new List<int>{ 28, 29, 30, 31, 32, 33 } },
            { 141, new List<int>{ 31, 32, 33, 34, 35, 36 } },
            { 142, new List<int>{ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 } },
            { 143, new List<int>{ 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 } },
            { 144, new List<int>{ 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36 } },
            { 145, new List<int>{ 1, 4, 7, 10, 13, 16, 19, 22, 25, 28, 31, 34} },
            { 146, new List<int>{ 2, 5, 8, 11, 14, 17, 20, 23, 26, 29, 32, 35} },
            { 147, new List<int>{ 3, 6, 9, 12, 15, 18, 21, 24, 27, 30, 33, 36} },
            { 148, new List<int>{ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18} },
            { 149, new List<int>{ 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34, 36} },
            { 150, new List<int>{ 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36} },
            { 151, new List<int>{ 2, 4, 6, 8, 10, 11, 13, 15, 17, 20, 22, 24, 26, 28, 29, 31, 33, 35} },
            { 152, new List<int>{ 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25, 27, 29, 31, 33, 35} },
            { 153, new List<int>{ 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36} },
        };

        public static List<int> RouletteColors = new List<int>()
        {
            -1,
            1,
            0,
            1,
            0,
            1,
            0,
            1,
            0,
            1,
            0,
            1,
            0,
            1,
            0,
            1,
            0,
            1,
            0,
            -1,
            0,
            1,
            0,
            1,
            0,
            1,
            0,
            1,
            0,
            1,
            0,
            1,
            0,
            1,
            0,
            1,
            0,
            1
        };

        public static Dictionary<int, int> RouletteNumbers = new Dictionary<int, int>()
        {
            {-1, 1 },
            {0, 20 },
            {1, 38 },
            {2, 19 },
            {3, 34 },
            {4, 15 },
            {5, 30 },
            {6, 11 },
            {7, 26 },
            {8, 7 },
            {9, 22 },
            {10, 3 },
            {11, 25 },
            {12, 6 },
            {13, 37 },
            {14, 18 },
            {15, 33 },
            {16, 14 },
            {17, 29 },
            {18, 10 },
            {19, 8 },
            {20, 27 },
            {21, 12 },
            {22, 31 },
            {23, 16 },
            {24, 35 },
            {25, 4 },
            {26, 23 },
            {27, 2},
            {28, 21 },
            {29, 5 },
            {30, 24 },
            {31, 9 },
            {32, 28 },
            {33, 13 },
            {34, 32 },
            {35, 17 },
            {36, 36 },
        };


        public static float LastY = 0f;
        //public static int j = 0;

        public static List<int> RouletteBetTime = new List<int>()
        {
            0, 0
        };
        public static List<List<Player>> RoulettePlayers = new List<List<Player>>() {
            new List<Player>(){ },
            new List<Player>(){ }
        };

        public static List<int> RouletteStatus = new List<int>() {
            0,
            0
        };

        public static List<int> RouletteNum = new List<int>() {
            0,
            0
        };


        public static List<List<Player>> RouletteSeatsPlayers = new List<List<Player>>() {
            new List<Player>(){ },
            new List<Player>(){ }
        };

        [Command("addchips")]
        public static void AddChips(Player player)
        {
            nInventory.Add(player, new nItem(ItemType.CasinoChips, 10000));
        }

        [ServerEvent(Event.ResourceStart)]
        public static void OnResourceStart()
        {

            try
            {

                var result = MySQL.QueryRead($"SELECT * FROM `casinoroulette`");
                if (result == null || result.Rows.Count == 0)
                {
                    Log.Write("DB return null result.", nLog.Type.Warn);
                    return;
                }
                foreach (DataRow Row in result.Rows)
                {
                    try
                    {
                        var id = Convert.ToInt32(Row["id"].ToString());
                        var minbet = Convert.ToInt32(Row["minbet"]);
                        var maxbet = Convert.ToInt32(Row["maxbet"]);

                        RouletteTableBetMin.Add(minbet);
                        RouletteTableBetMax.Add(maxbet);
                 
                    }
                    catch (Exception e)
                    {
                        Log.Write(Row["id"].ToString() + e.ToString(), nLog.Type.Error);
                    }

                }
            }
            catch
            {

            }
            colShape = NAPI.ColShape.CreateCylinderColShape(colPosition, 2, 2);
            colShape.OnEntityEnterColShape += (ColShape shape, Player Player) =>
            {
                NAPI.Data.SetEntityData(Player, "INTERACTIONCHECK", 70);
            };
            colShape.OnEntityExitColShape += (ColShape shape, Player Player) =>
            {
                NAPI.Data.SetEntityData(Player, "INTERACTIONCHECK", 0);
            };
            colShape.Position = colPosition;

            var prizeCol = NAPI.ColShape.CreateCylinderColShape(new Vector3(-100000,-100000,-100000), 2, 2);
            prizeCol.OnEntityEnterColShape += (ColShape shape, Player Player) =>
            {
                NAPI.Data.SetEntityData(Player, "INTERACTIONCHECK", 655);
                Trigger.ClientEvent(Player, "client_press_key_to", "open", JsonConvert.SerializeObject(new List<object>() { "E", "получить приз" }));
            };
            prizeCol.OnEntityExitColShape += (ColShape shape, Player Player) =>
            {
                NAPI.Data.SetEntityData(Player, "INTERACTIONCHECK", 0);
                Trigger.ClientEvent(Player, "client_press_key_to", "close");
            };

            var spinWheel = NAPI.ColShape.CreateCylinderColShape(new Vector3(-10000, -10000,-10000), 1, 2);
            spinWheel.OnEntityEnterColShape += (ColShape shape, Player Player) =>
            {
                NAPI.Data.SetEntityData(Player, "INTERACTIONCHECK", 656);
                Trigger.ClientEvent(Player, "client_press_key_to", "open", JsonConvert.SerializeObject(new List<object>() { "E", "крутить колесо фортуны"  }));
            };
            spinWheel.OnEntityExitColShape += (ColShape shape, Player Player) =>
            {
                NAPI.Data.SetEntityData(Player, "INTERACTIONCHECK", 0);
                Trigger.ClientEvent(Player, "client_press_key_to", "close");
            };


            //Console.WriteLine(colShape.Position);
            //Console.WriteLine(blip.Position);
            // NAPI.Marker.CreateMarker(21, colPosition, new Vector3(), new Vector3(), 0.8f, new Color(255, 255, 255, 60));
            // NAPI.TextLabel.CreateTextLabel("~y~Нажмите E чтобы начать играть", colPosition + new Vector3(0, 0, 0.3), 5F, 0.3F, 0, new Color(255, 255, 255));

            //Vector3 _wheelPos = new Vector3(1109.76, 227.89, -49.64);
            //Vector3 _baseWheelPos = new Vector3(1111.05, 229.81, -50.38);

            //uint model = NAPI.Util.GetHashKey("vw_prop_vw_luckywheel_02a");
            //uint baseWheelModel = NAPI.Util.GetHashKey("vw_prop_vw_luckywheel_01a");

            // bWheel = NAPI.Object.CreateObject(baseWheelModel, _baseWheelPos, new Vector3());
            //Wheel = NAPI.Object.CreateObject(model, _baseWheelPos + new Vector3(0, 0, 1.25), new Vector3());

            //NAPI.Entity.SetEntityRotation(Wheel, new Vector3(0, 0, 0));

            var col = NAPI.ColShape.CreateCylinderColShape(new Vector3(1115.912, 219.99, -60.55512), 2f, 2f);
            col.OnEntityEnterColShape += (sahpe, player) =>
            {
                player.SetData("INTERACTIONCHECK", 662);
                Trigger.ClientEvent(player, "client_press_key_to", "open", JsonConvert.SerializeObject(new List<object>() { "E", "купить/продать фишки" }));
            };
            col.OnEntityExitColShape += (sahpe, player) =>
            {
                player.SetData("INTERACTIONCHECK", 0);
                Trigger.ClientEvent(player, "client_press_key_to", "close");
            };

            for (int i = 0; i < RouletteTables.Count; i++)
            {
                var colshape = NAPI.ColShape.CreateCylinderColShape(RouletteTables[i], 2, 2);
                colshape.SetData("TABLE", i);
                colshape.OnEntityEnterColShape += (shape, player) =>
                {
                    player.SetData("INTERACTIONCHECK", 664);
                    player.SetData("TABLE", shape.GetData<int>("TABLE"));
                    Trigger.ClientEvent(player, "client_press_key_to", "open", JsonConvert.SerializeObject(new List<object>() { "E", "сесть за стол" }));
                };
                colshape.OnEntityExitColShape += (shape, player) =>
                {
                    player.SetData("INTERACTIONCHECK", 0);
                    player.ResetData("TABLE");
                    Trigger.ClientEvent(player, "client_press_key_to", "close");
                };
            }


            for (int i = 0; i < 2; i++)
            {
                
                Thread thd = new Thread(new ParameterizedThreadStart(StartRoulette));
                thd.Start(i);
                CasinoThreads.Add(thd);
                
            }
        }

        [ServerEvent(Event.ResourceStop)]
        public static void Stop()
        {
            for (int i = 0; i < CasinoThreads.Count; i++)
                CasinoThreads[i].Abort();
        }

        // [Command("spin")]
        public static int NumberCasino = -2;
        public static void Spin(int table)
        {
            if (NumberCasino != -2)
            {
                RouletteNum[table] = NumberCasino;
                Trigger.ClientEventInRange(RouletteTables[table], 50, "spin_wheel", table, 3, $"exit_{RouletteNumbers[NumberCasino]}_wheel", $"exit_{RouletteNumbers[NumberCasino]}_ball");
            }
            else
            {
                Random rand = new Random();

                int num = rand.Next(-1, 37);
                RouletteNum[table] = num;
                Trigger.ClientEventInRange(RouletteTables[table], 50, "spin_wheel", table, 3, $"exit_{RouletteNumbers[num]}_wheel", $"exit_{RouletteNumbers[num]}_ball");
            }
            NumberCasino = -2;
        }

        [Command("spintestcasinowheelnextnum")]
        public static void SpinTest(Player player, int count)
        {
            NumberCasino = count;
        }

        public static void Roulette(Player player, int tb)
        {
            player.SetData("RL_TABLE", tb);
            player.SetData("RL_BET", 0);
            player.SetData("RL_SETBET", RouletteTableBetMin[tb]);

            RouletteSeatsPlayers[tb].Add(player);
        }

        [RemoteEvent("server_make_roulette_bet")]
        public static void MakeRouletteBet(Player player, int spot)
        {
            if (RouletteStatus[player.GetData<int>("RL_TABLE")] != 0)
            {
                return;
            }

            RlBet(player, spot, player.GetData<int>("RL_SETBET"));
        }
        

        public static void RlBet(Player player, int spot, int chips)
        {
            if (!player.HasData("RL_TABLE"))
                return;

            int table = player.GetData<int>("RL_TABLE");

            if (RouletteStatus[table] != 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Can't bet at the moment", 3000);
                return;
            }

            player.SetData("RL_WIN", 0);

            var item = nInventory.Find(Main.Players[player].UUID, ItemType.CasinoChips);

            if(item == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You don't have enough chips", 3000);
                return;
            }
            if (item.Count < chips)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You don't have enough chips", 3000);
                return;
            }

            if (!RoulettePlayers[player.GetData<int>("RL_TABLE")].Contains(player))
                RoulettePlayers[player.GetData<int>("RL_TABLE")].Add(player);

            nInventory.Remove(player, ItemType.CasinoChips, chips);
            RouletteBets[player.GetData<int>("RL_TABLE")].Add(new Bet(player, chips, spot));
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You have placed a bet {chips} chips", 3000);
            UpdateCasinoChipsGUI(player);
            Trigger.ClientEvent(player, "bet_roulette", player.GetData<int>("RL_TABLE"), spot);
            //Trigger.ClientEvent(player, "seat_to_blackjack_table", 1, 0);
        }

        [RemoteEvent("serverSetRouletteBet")]
        public static void SetBet(Player player, int val)
        {
            if (!player.HasData("RL_TABLE"))
                return;

            player.SetData("RL_SETBET", val);

            //Trigger.ClientEvent(player, "client_casino_bet", "close");
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"You have set the rate at {val} chips", 3000);

        }

        [RemoteEvent("serverChangeRouletteBet")]
        public static void CahngeBet(Player player)
        {
            if (!player.HasData("RL_TABLE"))
                return;


            int table = player.GetData<int>("RL_TABLE");

          

            if (RouletteStatus[table] != 0)
                return;

            //Trigger.ClientEvent(player, "client_casino_bet", "open", JsonConvert.SerializeObject(new List<object>() { RouletteTableBetMin[table], player.GetData<int>("RL_SETBET"), RouletteTableBetMax[table] }));

        }

        public static int GetAllChips(Player player)
        {

            var item = nInventory.Find(Main.Players[player].UUID, ItemType.CasinoChips);

            int count = 0;

            if (item != null)
            {
                count = item.Count;
            }

            return count;

        }

        public static void SeatAtRoulette(Player player, int table)
        {
            if (player.HasData("RL_TABLE"))
            {
                ExitRoulette(player);
                return;
            }

            Trigger.ClientEvent(player, "client_press_key_to", "close");

            Roulette(player, table);

            Trigger.ClientEvent(player, "seat_to_roulette_table", table);
            Trigger.ClientEvent(player, "client_casino_bet", "open", JsonConvert.SerializeObject(new List<object>() { RouletteTableBetMin[table], RouletteTableBetMin[table], RouletteTableBetMax[table], GetAllChips(player)}));
            //Trigger.ClientEvent(player, "seat_to_blackjack_table", 1, 0);
        }

        public static void UpdateCasinoTimeGUI(Player player)
        {
            if (!player.HasData("RL_TABLE"))
            {
               
                return;
            }
            int tb = player.GetData<int>("TABLE");

            Trigger.ClientEvent(player, "updateCasinoTime", RouletteBetTime[tb]);
        }

        public static void UpdateCasinoChipsGUI(Player player)
        {
            if (!player.HasData("RL_TABLE"))
            {

                return;
            }
            int tb = player.GetData<int>("TABLE");

            Trigger.ClientEvent(player, "updateCasinoChips", GetAllChips(player));
        }
        
        [RemoteEvent("ExitMenu_casino")]
        public static void ExitRoulette(Player player)
        {

            if (player.HasData("RL_TABLE"))
            {
                if (RoulettePlayers[player.GetData<int>("RL_TABLE")].Contains(player))
                    return;

                RouletteSeatsPlayers[player.GetData<int>("RL_TABLE")].Remove(player);
                Trigger.ClientEvent(player , "exit_roulette");
                Trigger.ClientEvent(player , "closecasino");
                player.ResetData("RL_TABLE");
                return;
            }
        }

        public static void StartRoulette(object table)
        {
            int tb = Convert.ToInt32(table);
           
            while (true)
            {
                if (RouletteBetTime[tb] != 0)
                {
                    try
                    {
                        if (RouletteSeatsPlayers[tb].Count != 0)
                        {
                            for (int i = 0; i < RouletteSeatsPlayers[tb].Count; i++)
                            {
                                UpdateCasinoTimeGUI(RouletteSeatsPlayers[tb][i]);
                            }
                        }

                        RouletteBetTime[tb] -= 1;

                        Thread.Sleep(1000);

                        if (RouletteBetTime[tb] != 0)
                            continue;
                    }
                    catch (Exception ex)
                    {
                        Log.Debug(ex.Message);
                    }
                }

                if (RoulettePlayers[tb].Count == 0 && RouletteBetTime[tb] == 0)
                {
                    RouletteBetTime[tb] = 30;
                    Thread.Sleep(1000);
                    continue;
                }

                for (int i = 0; i < RouletteSeatsPlayers[tb].Count; i++)
                {
                    UpdateCasinoTimeGUI(RouletteSeatsPlayers[tb][i]);
                }

                RouletteStatus[tb] = 1;

                Spin(tb);

                for (int i = 0; i < RoulettePlayers[tb].Count; i++)
                {
                    try
                    {
                        Trigger.ClientEvent(RoulettePlayers[tb][i], "start_roulette");
                        //Notify.Send(RoulettePlayers[tb][i], NotifyType.Info, NotifyPosition.BottomCenter, "Ставки приняты", 3000);
                    }
                    catch (Exception e)
                    {
                        Log.Debug(e.Message);
                    }
                }

                Thread.Sleep(25000);

                for (int i = 0; i < RoulettePlayers[tb].Count; i++)
                {

                    try
                    {
                        Trigger.ClientEvent(RoulettePlayers[tb][i], "stop_roulette");
                    }
                    catch (Exception e)
                    {
                        Log.Debug(e.Message);
                    }
                }

                Trigger.ClientEventInRange(RouletteTables[tb], 40, "clean_chips", tb);

                foreach (Bet bet in RouletteBets[tb])
                {
                    if (Spots[bet.Spot].Contains(RouletteNum[tb]))
                    {
                        int mnoj = 2;

                        if (bet.Spot <= 0)
                            mnoj = 36;
                        else if (bet.Spot <= 37)
                            mnoj = 36;
                        else if (bet.Spot >= 38 && bet.Spot <= 95)
                            mnoj = 15;
                        else if (bet.Spot >= 96 && bet.Spot <= 107)
                            mnoj = 7;
                        else if (bet.Spot >= 108 && bet.Spot <= 129)
                            mnoj = 3;
                        else if (bet.Spot >= 130 && bet.Spot <= 142)
                            mnoj = 2;
                        else if (bet.Spot >= 142 && bet.Spot <= 147)
                            mnoj = 3;

                        bet.Player.SetData("RL_WIN", bet.Player.GetData<int>("RL_WIN") + (bet.BetAmount * mnoj));
                    }
                }

                for (int i = 0; i < RoulettePlayers[tb].Count; i++)
                {

                    try
                    {

                        string winStr = "";

                        if (RouletteNum[tb] == -1)
                            winStr = "00";
                        else
                            winStr = $"{RouletteNum[tb]}";

                        if (RoulettePlayers[tb][i].GetData<int>("RL_WIN") <= 0)
                        {
                            Notify.Send(RoulettePlayers[tb][i], NotifyType.Info, NotifyPosition.BottomCenter, $"Dropped: {winStr}. Your bets are lost", 3000);
                        }
                        else
                        {
                            int prize = RoulettePlayers[tb][i].GetData<int>("RL_WIN");
                            Notify.Send(RoulettePlayers[tb][i], NotifyType.Info, NotifyPosition.BottomCenter, $"dropped out: {winStr}. you won {prize} chips!", 3000);
                            nInventory.Add(RoulettePlayers[tb][i], new nItem(ItemType.CasinoChips, prize));
                            //GameLog.Money($"player({Main.Players[RoulettePlayers[tb][i]].UUID})", $"CasinoRoulleteWin", prize, $"");
                        }
                        UpdateCasinoChipsGUI(RoulettePlayers[tb][i]);
                        RoulettePlayers[tb][i].SetData("RL_WIN", 0);

                    }
                    catch (Exception e)
                    {
                        Log.Debug(e.Message);
                    }
                }

                RouletteBets[tb].Clear();
                RoulettePlayers[tb].Clear();
                RouletteStatus[tb] = 0;

                Thread.Sleep(2000);
            }
        }

        [ServerEvent(Event.PlayerDeath)]
        public void OnPlayerDeath(Player player, Player killer, uint reason)
        {
            if (PlayersList.ContainsKey(player))
            {
                NAPI.ClientEvent.TriggerClientEvent(player, "rouletecfg", 5, 0, 0); // Обычно CaEnd работает после того как клиент его закроет изнутри, но при смерти нужно закрыть со стороны сервера
            }
        }
    }



    class Bet
    {
        public Player Player;
        public int BetAmount = 0;
        public int Spot;

        public Bet(Player player, int money, int spot)
        {
            Player = player;
            BetAmount = money;
            Spot = spot;
        }
    }
}
