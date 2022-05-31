using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using Golemo.Core;
using GolemoSDK;

namespace Golemo.Markets
{

    class MarketBlack : Script
    {
        #region Settings
        private static Random rnd = new Random();

        private static nLog Log = new nLog("Black Market");

        public static int marketMultiplier;
        private static int _minMultiplier = 1;
        private static int _maxMultiplier = 1;

        public static void UpdateMultiplier()
        {
            marketMultiplier = rnd.Next(_minMultiplier, _maxMultiplier);
            Log.Write($"Updated coefficient on: {marketMultiplier}");
        }

        private static List<Vector3> shape = new List<Vector3>()
        {
            new Vector3(-1265.422, -1469.5344, 3.0804386),
        };
        #endregion

        #region Инициализация Работы Фермера
        [ServerEvent(Event.ResourceStart)]
        public void Event_MarketStart()
        {
            try
            {
                #region Создание блипа, текста, колшейпа
               // NAPI.Blip.CreateBlip(501, new Vector3(2367.39, 4881.526, 41.3), 1, 81, "H", 255, 0, true, 0, 0); // Блип на карте
              //  NAPI.TextLabel.CreateTextLabel("~w~Скупщик", new Vector3(2367.39, 4881.526, 43.2), 10f, 0.2f, 4, new Color(255, 255, 255), true, NAPI.GlobalDimension); // Над головой у Ped

                var melogShape = NAPI.ColShape.CreateCylinderColShape(shape[0], 2f, 2, 0);
                melogShape.OnEntityEnterColShape += (shape, player) =>
                {
                    try
                    {
                        player.SetData("INTERACTIONCHECK", 812);
                    }
                    catch (Exception e)
                    {
                        Log.Write(e.ToString(), nLog.Type.Error);
                    }
                };
                melogShape.OnEntityExitColShape += (shape, player) =>
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
                UpdateMultiplier();
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
        }
        #endregion

        #region Предметы в маркете
        //цена, номер предмета, название, предмет для покупки или для продажи (если true, то коэффициент будет умножаться на выставленную сумму)
        private static List<Product> SellItems = new List<Product>()
        {
            new Product(10, 1000, "Гриб", true, "Обычный гриб, очень часто попадается, есть можно"),
        };

        private static List<Product> BuyItems = new List<Product>()
        {
            new Product(11500, 576, "Программатор", false, "Вещь очень сильная, нужна для взлома тачек"),
            new Product(2500, 11, "Отмычка для замков", false, "Можно вскрыть двери любой тачки"),
            new Product(2500, 12, "Сумка с деньгами", false, "Нужна для переноса денег после черных делишек"),
            new Product(2500, 15, "Сумка с дрелью", false, "Дрель высшего класса, сломает любую дверь сейфа"),
            new Product(2500, 16, "Военная отмычка", false, "Взломает любой военный замок"),
        };
        #endregion

        #region Открыть меню Маркета
        [RemoteEvent("changePage5")]
        public static void OpenMarketMenu(Player player, int page)
        {
            if (player.IsInVehicle) return;
            var hitem = nInventory.Find(Main.Players[player].UUID, ItemType.Hay);
            var shitem = nInventory.Find(Main.Players[player].UUID, ItemType.Seed);
            int hayscount = hitem != null ? hitem.Count : 0;
            int seedscount = shitem != null ? shitem.Count : 0;
            List<object> data = new List<object>()
            {
                marketMultiplier,
                hayscount,
                seedscount,
            };
            LoadPage(player, page, data);
        }
        #endregion

        #region Взаимодействие с менюшкой Маркета
        public static void LoadPage(Player player, int page, object data)
        {
            string json;
            string json2;
            string json3;
            switch (page)
            {
                case 0:
                    json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                    json2 = Newtonsoft.Json.JsonConvert.SerializeObject(BuyItems);
                    json3 = Newtonsoft.Json.JsonConvert.SerializeObject(SellItems);
                    Trigger.ClientEvent(player, "loadPage5", 0, json, json2, json3);
                    break;
            }
        }
        #endregion

        #region BuyFarmerItem
        [RemoteEvent("buyFarmerItem5")]
        public static void ButFarmerItem(Player player, int id, int count)
        {
            nItem aItem = new nItem((ItemType)id);
            var tryAdd = nInventory.TryAdd(player, new nItem(aItem.Type, count));
            if (tryAdd == -1 || tryAdd > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Not enough space in the inventory", 2000);
                return;
            }
            var item = BuyItems.Find(x => x.ID == id);
            if (item == null)
            {
                Notify.Error(player, "Предмет не найден", 2500);
                return;
            }
            int price = item.Ordered ? item.Price * marketMultiplier * count : item.Price * count;
            if (Main.Players[player].Money < price)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Not enough money", 2000);
                return;
            }
            MoneySystem.Wallet.Change(player, -price);
            Trigger.ClientEvent(player, "sellgreat5");
            nInventory.Add(player, new nItem(aItem.Type, count));
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы купили {count} {item.Name} за ${price}", 2000);
        }
        #endregion

        #region SellFarmerItem
        [RemoteEvent("sellFarmerItem5")]
        public static void SellFarmerItem(Player player, int id, int count)
        {
            var aItem = nInventory.Find(Main.Players[player].UUID, (ItemType)id);
            if (aItem == null || aItem.Count < count)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно предмета в инвентаре", 2000);
                return;
            }
            var item = SellItems.Find(x => x.ID == id);
            if(item == null)
            {
                Notify.Error(player, "Предмет не найден", 2500);
                return;
            }
            int price = item.Ordered ? item.Price * marketMultiplier * count : item.Price * count;
            MoneySystem.Wallet.Change(player, price);
            Trigger.ClientEvent(player, "sellgreat5");
            nInventory.Remove(player, new nItem(aItem.Type, count));
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы продали {count} {item.Name} за ${price}", 2000);
        }
        #endregion

        #region FarmerProduct
        private class Product
        {
            public int Price { get; set; }
            public int ID { get; set; }
            public string Name { get; set; }
            public string Desc { get; set; }
            public bool Ordered { get; set; }

            public Product(int price, int id, string name, bool ordered, string desc)
            {
                Price = price;
                ID = id;
                Name = name;
                Ordered = ordered;
                Desc = desc;
            }
        }
        #endregion
    }
}
