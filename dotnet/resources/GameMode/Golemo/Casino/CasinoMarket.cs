using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using Golemo.Core;
using GolemoSDK;

namespace Golemo.Casino
{

    class CasinoMarket : Script
    {
        #region Settings
        private static Random rnd = new Random();

        private static nLog Log = new nLog("CasinoMarket");

        public static int marketMultiplier;
        private static int _minMultiplier = 1;
        private static int _maxMultiplier = 1;

        public static void UpdateMultiplier()
        {
            marketMultiplier = rnd.Next(_minMultiplier, _maxMultiplier);
            Log.Write($"The chip shop is fully operational");
        }

        private static List<Vector3> shape = new List<Vector3>()
        {
            new Vector3(1115.912, 219.99, -50.55512),
           // new Vector3(-3006.155, 76.58138, 15.109628),
        };
        #endregion

        #region Инициализация Работы Фермера
        [ServerEvent(Event.ResourceStart)]
        public void Event_MarketStart()
        {
            try
            {
                #region Создание блипа, текста, колшейпа
                foreach (Vector3 vec in shape)
                {
                    var melogShape = NAPI.ColShape.CreateCylinderColShape(vec, 2f, 2, 0);
                    NAPI.Marker.CreateMarker(1, vec, new Vector3(), new Vector3(), 0.6f, new Color(13, 230, 70));
               
                melogShape.OnEntityEnterColShape += (shape, player) =>
                {
                    try
                    {
                        player.SetData("INTERACTIONCHECK", 666);
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
            new Product(90, 224, "Фишки", false),
        };

        private static List<Product> BuyItems = new List<Product>()
        {
            new Product(100, 224, "Фишки", true),
        };
        #endregion

        #region Открыть меню Маркета
        [RemoteEvent("changePage2")]
        public static void OpenMarketMenu(Player player, int page)
        {
            if (player.IsInVehicle) return;
            var hitem = nInventory.Find(Main.Players[player].UUID, ItemType.CasinoChips);
            var shitem = nInventory.Find(Main.Players[player].UUID, ItemType.CasinoChips);
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
            string json3;
            string json2;
            switch (page)
            {
                case 0:
                    json3 = Newtonsoft.Json.JsonConvert.SerializeObject(BuyItems);
                    json2 = Newtonsoft.Json.JsonConvert.SerializeObject(SellItems);
                    Trigger.ClientEvent(player, "loadPage2", 0, json3, json2);
                    break;
            }
        }
        #endregion

        #region BuyFarmerItem
        [RemoteEvent("buyFarmerItem2")]
        public static void ButFarmerItem(Player player, int id, int count)
        {
            nItem aItem = new nItem((ItemType)id);
            var tryAdd = nInventory.TryAdd(player, new nItem(aItem.Type, count));
            if (tryAdd == -1 || tryAdd > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Insufficient inventory space", 2000);
                return;
            }
            var item = BuyItems.Find(x => x.ID == id);
            if (item == null)
            {
                Notify.Error(player, "Item not found", 2500);
                return;
            }
            int price = item.Ordered ? item.Price * marketMultiplier * count : item.Price * count;
            if (Main.Players[player].Money < price)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Not enough money", 2000);
                return;
            }
            MoneySystem.Wallet.Change(player, -price);
            nInventory.Add(player, new nItem(aItem.Type, count));
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomLeft, $"You bought {count} {item.Name} for ${price}", 2000);
        }
        #endregion

        #region SellFarmerItem
        [RemoteEvent("sellFarmerItem2")]
        public static void SellFarmerItem(Player player, int id, int count)
        {
            var aItem = nInventory.Find(Main.Players[player].UUID, (ItemType)id);
            if (aItem == null || aItem.Count < count)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, "Insufficient item in inventory", 2000);
                return;
            }
            var item = SellItems.Find(x => x.ID == id);
            if(item == null)
            {
                Notify.Error(player, "Item not found", 2500);
                return;
            }
            int price = item.Ordered ? item.Price * marketMultiplier * count : item.Price * count;
            MoneySystem.Wallet.Change(player, price);
            nInventory.Remove(player, new nItem(aItem.Type, count));
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomLeft, $"you sold {count} {item.Name} for ${price}", 2000);
        }
        #endregion

        #region FarmerProduct
        private class Product
        {
            public int Price { get; set; }
            public int ID { get; set; }
            public string Name { get; set; }
            public bool Ordered { get; set; }

            public Product(int price, int id, string name, bool ordered)
            {
                Price = price;
                ID = id;
                Name = name;
                Ordered = ordered;
            }
        }
        #endregion
    }
}
