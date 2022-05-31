using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using Golemo.Core;
using GolemoSDK;

namespace Golemo.Markets
{

    class MarketFish : Script
    {
        #region Settings
        private static Random rnd = new Random();

        private static nLog Log = new nLog("MarketFish");

        public static int marketMultiplier;
        private static int _minMultiplier = 2;
        private static int _maxMultiplier = 5;

        public static void UpdateMultiplier()
        {
            marketMultiplier = rnd.Next(_minMultiplier, _maxMultiplier);
            Log.Write($"Updated coefficient on: {marketMultiplier}");
        }

        private static List<Vector3> shape = new List<Vector3>()
        {
            new Vector3(-1229.4137, -1507.4995, 3.049357),
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
                        player.SetData("INTERACTIONCHECK", 810);
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
        //price, subject number, name, item for purchase or for sale (если true, то коэффициент будет умножаться на выставленную сумму)
        private static List<Product> SellItems = new List<Product>()
        {
            new Product(5600, 209, "Корюшка", true, "A trophy copy that every fisherman dreams of getting.The fish is very beautiful and capricious"),
            new Product(4500, 210, "Кунджа", true, "Type of radiant fish, Asian endemic from the genus of the losos family of the family.Kunja large fish "),
            new Product(4750, 211, "Лосось", true, "The family of radiant fish from the salmon unit.The family contains both anadromic and freshwater fish species."),                                                      
            new Product(3000, 212, "Окунь", true, "The type of radiant fish of the family of freshwater perches of the Obunov family.Very often comes across fishermen"),
            new Product(4200, 213, "Осётр", true, "The family of freshwater, semi -passage and passing fish from the sturgeon family."),
            new Product(8000, 214, "Скат", true, "A real treasure, very rarely comes across fishermen"),
            new Product(3200, 215, "Тунец", true, "Sea predatory fish"),
            new Product(5200, 216, "Угорь", true, "Very long fish, you can say the most ugly in appearance, but catching it is an honor for a fisherman"),
            new Product(6300, 217, "Чёрный амур", true, "Very large fish and not to find her fisherman sin!"),
            new Product(5720, 218, "Щука", true, "Long river fish, comes in accurately often"),
        };

        private static List<Product> BuyItems = new List<Product>()
        {
            new Product(2, 209, "Наживка2", false, "Ordinary mushroom, very often comes across"),
        };
        #endregion

        #region Открыть меню Маркета
        [RemoteEvent("changePage3")]
        public static void OpenMarketMenu3(Player player, int page)
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
            LoadPage3(player, page, data);
        }
        #endregion

        #region Взаимодействие с менюшкой Маркета
        public static void LoadPage3(Player player, int page, object data)
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
                    Trigger.ClientEvent(player, "loadPage3", 0, json, json2, json3);
                    break;
            }
        }
        #endregion

        #region BuyItem
        [RemoteEvent("buyFarmerItem3")]
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
                Notify.Error(player, "Not", 2500);
                return;
            }
            int price = item.Ordered ? item.Price * marketMultiplier * count : item.Price * count;
            if (Main.Players[player].Money < price)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Not enough money", 2000);
                return;
            }
            MoneySystem.Wallet.Change(player, -price);
            nInventory.Add(player, new nItem(aItem.Type, count));
            Trigger.ClientEvent(player, "sellgreat3");
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You bought {count} {item.Name} for ${price}", 2000);
        }
        #endregion

        #region SellItems
        [RemoteEvent("sellFarmerItem3")]
        public static void SellFarmerItem3(Player player, int id, int count)
        {
            if (Main.Players[player].Licenses[8] == false)
            {
                Notify.Error(player, "You have no fishing license");
                return;
            }
            var aItem = nInventory.Find(Main.Players[player].UUID, (ItemType)id);
            if (aItem == null || aItem.Count < count)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Not enough item in the inventory", 2000);
                return;
            }
            var item = SellItems.Find(x => x.ID == id);
            if(item == null)
            {
                Notify.Error(player, "Not", 2500);
                return;
            }
            int price = item.Ordered ? item.Price * marketMultiplier * count : item.Price * count;
            MoneySystem.Wallet.Change(player, price);
            nInventory.Remove(player, new nItem(aItem.Type, count));
            Trigger.ClientEvent(player, "sellgreat3");
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You sold {count} {item.Name} for ${price}", 2000);
        }
        #endregion

        #region MarketProduct
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
