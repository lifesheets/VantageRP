using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using Golemo.Core;
using GolemoSDK;

namespace Golemo.Markets
{

    class MarketOther : Script
    {
        #region Settings
        private static Random rnd = new Random();

        private static nLog Log = new nLog("Other Market");

        public static int marketMultiplier;
        private static int _minMultiplier = 2;
        private static int _maxMultiplier = 5;

        public static void UpdateMultiplier()
        {
            marketMultiplier = rnd.Next(_minMultiplier, _maxMultiplier);
            Log.Write($"Updated coefficient for: {marketMultiplier}");
        }

        private static List<Vector3> shape = new List<Vector3>()
        {
            new Vector3(-1261.4259, -1464.436, 3.2746143),
        };
        #endregion

        #region Инициализация 
        [ServerEvent(Event.ResourceStart)]
        public void Event_MarketStart()
        {
            try
            {
                #region Создание блипа, текста, колшейпа

                var melogShape = NAPI.ColShape.CreateCylinderColShape(shape[0], 2f, 2, 0);
                melogShape.OnEntityEnterColShape += (shape, player) =>
                {
                    try
                    {
                        player.SetData("INTERACTIONCHECK", 813);
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
            new Product(10, 17, "Мешок", true, "Впринцепе этот мешок может и использовался не так как нужно, но продав его ты получишь небольшой куш"),
            new Product(10, 18, "Стяжки", true, "Отдам трудовику, пускай вместо гвоздей использует когда табуретки делает"),
            new Product(10, 19, "Ключи от машины", true, "Возьму твой ключик если он уже не подходит к тачке"),
            new Product(10, 182, "Молоток", true, "Такой же сильный как рука твоей тещи"),
            new Product(10, 184, "Монтировка", true, "Может разбить стелко любой тачки даже кибертрака. Впринцепе мне продай я сделаю из него айфон"),
            new Product(10, 194, "Гаечный ключ", true, "Смотрели же фильм трансформеры? Так вот продаешь мне я сделаю нового оптимуса парйма!"),
            new Product(10, 41, "Связка ключей", true, "Возьму связки и тебя свяжу, шутка!"),
            new Product(10, 43, "Балончик", true, "Ох уж эти вандалы оставили мусор свой, вообще их называют по разному: граффомэны, граффитеры. Бабушки называют их хулиганами."),
        };

        private static List<Product> BuyItems = new List<Product>()
        {
            new Product(2, 208, "Наживка", true, "Обычный гриб, очень часто попадается"),
            new Product(2, 209, "Наживка2", false, "Обычный гриб, очень часто попадается"),
        };
        #endregion

        #region Открыть меню Маркета
        [RemoteEvent("changePage6")]
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
                    Trigger.ClientEvent(player, "loadPage6", 0, json, json2, json3);
                    break;
            }
        }
        #endregion

        #region BuyItem
        [RemoteEvent("buyFarmerItem6")]
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
            Trigger.ClientEvent(player, "sellgreat6");
            nInventory.Add(player, new nItem(aItem.Type, count));
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы купили {count} {item.Name} за ${price}", 2000);
        }
        #endregion

        #region SellItem
        [RemoteEvent("sellFarmerItem6")]
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
            Trigger.ClientEvent(player, "sellgreat6");
            nInventory.Remove(player, new nItem(aItem.Type, count));
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы продали {count} {item.Name} за ${price}", 2000);
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
