using GTANetworkAPI;
using Newtonsoft.Json;
using Golemo.Core;
using GolemoSDK;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;

namespace Golemo.Roulette
{
    class CaseController : Script
    {
        public static Dictionary<int, List<CaseItem>> CasesPrizeList = new Dictionary<int, List<CaseItem>>();
        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                DataTable result = MySQL.QueryRead($"SELECT * FROM `caseprize`");
                if (result == null || result.Rows.Count == 0) return;
                foreach (DataRow Row in result.Rows)
                {
                    int UUID = Convert.ToInt32(Row["UUID"]);
                    string Cases = Row["Prize"].ToString();
                    List<CaseItem> ListCases = JsonConvert.DeserializeObject<List<CaseItem>>(Cases);
                    CasesPrizeList.Add(UUID, ListCases);
                }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        public static Dictionary<String, Case> Cases = new Dictionary<String, Case>
        {

            {"DEFCASE",new Case(new Dictionary<int, CaseItem>
                {
                    { 1, new CaseItem(0,"Car",4,1,5000,"cullinan","Rolls-Royca Cullinan","rphantom.png",0.4,0.4) },
                    { 2, new CaseItem(1,"Car",4,1,5000,"huracan","Lamborgini Huracan","huracan.png",0.4,0.4) },
                    { 3, new CaseItem(2,"Car",4,1,4500,"urus","Lamborgini Urus","urus.png",0.4,0.4) },
                    { 5, new CaseItem(3,"Car",4,1,4000,"mp1","McLaren P1","mp1.png",0.4,0.4) },
                    { 6, new CaseItem(4,"Car",4,1,3000,"63gls","Mercedes-Benz GLS 63","63gls.png",0.4,0.4) },
                    { 7, new CaseItem(5,"Car",4,1,3000,"e63s2020","Mercedes-Benz E63S 2020","e63s2020.png",0.4,0.4) },
                    { 8, new CaseItem(6,"Car",4,1,2500,"bmwx7","BMW X7","bmwx7.png",0.4,0.4) },
                    { 10, new CaseItem(7,"Car",3,1,2000,"16charger","Dodge Charger 16","16charger.png",0.4,0.4) },
                    { 11, new CaseItem(8,"Car",3,1,2000,"2020mustang","Ford Mustang GT 2020","2020mustang.png",0.4,0.4) },
                    { 12, new CaseItem(9,"Car",4,1,2000,"x6m","BMW X6M","x6m.png",0.4,0.4) },
                    { 13, new CaseItem(10,"Car",4,1,1500,"starone","Polestar One","starone.png",0.4,0.4) },
                    { 14, new CaseItem(11,"Car",3,1,1750,"camry70","Toyota Camry V70","camry70.png",0.4,0.4) },
                    { 15, new CaseItem(12,"Car",3,1,750,"a45","Mercedes-Benz A45","a45.png",0.4,0.4) },

                    { 16, new CaseItem(13,"Vip",3,1,600,"3","VIP Diamond","vip3.png",0.4,0.4) },
                    { 17, new CaseItem(14,"DonatePoint",2,1,0,"560","560 Коинов","donate.png",0.4,0.4) },
                    { 20, new CaseItem(15,"Vip",2,55,300,"2","VIP Ruby","vip2.png",0.4,0.4) },
                    { 190, new CaseItem(16,"Money",2,2,0,"1500000","1 500 000$","money3.png",0.4,0.4) },
                    { 200, new CaseItem(17,"Vip",2,70,100,"1","VIP Saphire","vip1.png",0.4,0.4) },
                    { 25, new CaseItem(18,"DonatePoint",2,1,0,"250","250 Коинов","donate.png",0.4,0.4) },
                    { 220, new CaseItem(19,"Money",2,65,0,"1000000","1 000 000$","money2.png",0.4,0.4) },
                    { 230, new CaseItem(20,"DonatePoint",1,70,0,"100","100 Коинов","donate.png",0.4,0.4) },
                    { 240, new CaseItem(21,"DonatePoint",1,75,0,"30","30 Коинов","donate.png",0.4,0.4) },
                    { 280, new CaseItem(22,"DonatePoint",1,75,0,"15","15 Коинов","donate.png",0.4,0.4) },
                    { 250, new CaseItem(23,"Money",1,80,0,"250000","250 000$","money1.png",0.4,0.4) },    
                    { 260, new CaseItem(24,"Money",1,80,0,"100000","100 000$","money1.png",0.4,0.4) },     
                })
            }
        };
        [RemoteEvent("r:GetCase")]
        public void GetCase(Player player, int id)
        {
            try
            {
                List<CaseItem> Case = new List<CaseItem>();
                switch (id)
                {
                    case 1:
                        {
                            Case = Cases["DEFCASE"].Citems;
                        }
                        break;
                }
                Trigger.ClientEvent(player, "r:setcase", JsonConvert.SerializeObject(Case));
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        [RemoteEvent("r:getWinId")]
        public void GetWinIdCase(Player player, string type, int caseid)
        {
            try
            {
                string name = GetCaseName(player, caseid);
                int id = -1;
                switch (name)
                {
                    case "DEFCASE":
                        if (Main.Accounts[player].RedBucks < 200)
                        {
                            Notify.Error(player, "Недостаточно средств");
                            return;
                        }
                        Main.Accounts[player].RedBucks -= 200;
                        id = Cases["DEFCASE"].GetRandom().id;
                        Trigger.ClientEvent(player, "r:getWinIdCallback", id, type);
                        return;
                }
                Trigger.ClientEvent(player, "redset", Main.Accounts[player].RedBucks);
                MySQL.Query($"update `accounts` set `redbucks`={Main.Accounts[player].RedBucks} where `login`='{Main.Accounts[player].Login}'");
                Trigger.ClientEvent(player, "r:getWinIdCallback", id, type);
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        [RemoteEvent("r:getPrize")]
        public void getPrize(Player player, string type, int id, int caseid)
        {
            try
            {
                string casename = GetCaseName(player, caseid);
                if (type != "sellList" && type != "take" && Cases[casename].Citems[id].rarity == 4)
                {
                    NAPI.Chat.SendChatMessageToAll($"Игрок {player.Name} ~r~Выбил с Кейса: {Cases[casename].Citems[id].title}");
                }
                switch (type)
                {
                    case "get":
                        {
                            GetCaseItem(player, id, casename);
                        }
                        break;
                    case "sell":
                        {
                            Main.Accounts[player].RedBucks += Cases[casename].Citems[id].cost;
                            Trigger.ClientEvent(player, "redset", Main.Accounts[player].RedBucks);
                            MySQL.Query($"update `accounts` set `redbucks`={Main.Accounts[player].RedBucks} where `login`='{Main.Accounts[player].Login}'");
                        }
                        break;
                    case "sellList":
                        {
                            Main.Accounts[player].RedBucks += CasesPrizeList[Main.Players[player].UUID][id].cost;
                            CasesPrizeList[Main.Players[player].UUID].RemoveAt(id);
                            SendCaseList(player);
                        }
                        break;
                    case "take":
                        {
                            if (!CasesPrizeList.ContainsKey(Main.Players[player].UUID) || CasesPrizeList[Main.Players[player].UUID].ElementAt(id) == null) return;
                            if (CasesPrizeList[Main.Players[player].UUID][id].Type == "Vip")
                            {
                                if (Main.Accounts[player].VipLvl >= 1)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У вас уже есть VIP статус!", 3000);
                                    return;
                                }
                            }
                            RouletteTakeItem(player, CasesPrizeList[Main.Players[player].UUID][id]);
                            CasesPrizeList[Main.Players[player].UUID].RemoveAt(id);
                            SendCaseList(player);
                        }
                        break;
                }
                Trigger.ClientEvent(player, "redset", Main.Accounts[player].RedBucks);
                MySQL.Query($"update `accounts` set `redbucks`={Main.Accounts[player].RedBucks} where `login`='{Main.Accounts[player].Login}'");
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        public void RouletteTakeItem(Player player, CaseItem itemtype)
        {
            try
            {
                switch (itemtype.Type)
                {
                    case "DonatePoint":
                        Main.Accounts[player].RedBucks += Convert.ToInt32(itemtype.carname);
                        Trigger.ClientEvent(player, "redset", Main.Accounts[player].RedBucks);
                        MySQL.Query($"update `accounts` set `redbucks`={Main.Accounts[player].RedBucks} where `login`='{Main.Accounts[player].Login}'");
                        break;
                    case "Car":
                        var house = Houses.HouseManager.GetHouse(player, true);
                        if (house == null)
                        {
                            var vNumber = VehicleManager.Create(player.Name, itemtype.carname, new Color(0, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0));
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы выйграли {itemtype.title} с номером {vNumber}", 5000);
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"В скором времени она будет доставлена на стоянку", 5000);
                        }
                        else
                        {
                            var garage = Houses.GarageManager.Garages[house.GarageID];
                            var vNumber = VehicleManager.Create(player.Name, itemtype.carname, new Color(0, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0));
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы выйграли {itemtype.title} с номером {vNumber}", 5000);
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"В скором времени она будет доставлена в Ваш гараж", 5000);
                            garage.SpawnCar(vNumber);
                        }
                        break;
                    case "Clothes":
                        Customization.AddClothes(player, (ItemType)Convert.ToInt32(itemtype.carname.Split("_")[0]), Convert.ToInt32(itemtype.carname.Split("_")[1]), Convert.ToInt32(itemtype.carname.Split("_")[2]));
                        break;
                    case "EXP":
                        Main.Players[player].EXP += Convert.ToInt32(itemtype.carname) * Group.GroupEXP[Main.Accounts[player].VipLvl] * Main.oldconfig.ExpMultiplier;
                        break;
                    case "Money":
                        MoneySystem.Wallet.Change(player, Convert.ToInt32(itemtype.carname));
                        break;
                    case "Vip":
                        Main.Accounts[player].VipLvl = Convert.ToInt32(itemtype.carname);
                        Main.Accounts[player].VipDate = DateTime.Now.AddDays(30);
                        GUI.Dashboard.sendStats(player);
                        break;

                }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        public void GetCaseItem(Player player, int id, string casename)
        {
            try
            {
                if (!CasesPrizeList.ContainsKey(Main.Players[player].UUID))
                {
                    List<CaseItem> item = new List<CaseItem>();
                    item.Add(Cases[casename].Citems[id]);
                    CasesPrizeList.Add(Main.Players[player].UUID, item);
                }
                else
                {
                    CasesPrizeList[Main.Players[player].UUID].Add(Cases[casename].Citems[id]);
                }
                MySQL.Query($"UPDATE `caseprize` SET `Prize`='{JsonConvert.SerializeObject(CasesPrizeList[Main.Players[player].UUID])}' WHERE `uuid`={Main.Players[player].UUID}");
                SendCaseList(player);
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        public string GetCaseName(Player player, int idcase)
        {

            string casename = null;
            switch (idcase)
            {
                case 1:
                    {
                        casename = "DEFCASE";
                    }
                    break;
            }
            return casename;
        }
        [RemoteEvent("r:SendCasePrize")]
        public static void SendCaseList(Player player)
        {
            try
            {
                if (!CasesPrizeList.ContainsKey(Main.Players[player].UUID))
                {
                    List<CaseItem> item = new List<CaseItem>();
                    CasesPrizeList.Add(Main.Players[player].UUID, item);
                }
                Trigger.ClientEvent(player, "r:SendCasePrize", JsonConvert.SerializeObject(CasesPrizeList[Main.Players[player].UUID]), Main.Accounts[player].RedBucks);
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        public class CaseItem
        {
            public Int32 id { get; private set; }
            public string Type { get; private set; }
            public Int32 rarity { get; private set; }
            public Int32 chance { get; private set; }
            public Int32 cost { get; private set; }
            public String title { get; private set; }
            public String carname { get; private set; }
            public String background { get; private set; }
            public Double DefaultChance { get; private set; }
            public Double StreamerChance { get; private set; }
            public CaseItem(Int32 ID, String Type, Int32 rarity = 1, Int32 chance = 1, Int32 cost = 1, String carname = "", String Title = "Default", String background = "Default", Double DefaultChance = 0.0001, Double StreamerChance = 0.0001)
            {
                this.id = ID;
                this.Type = Type;
                this.rarity = rarity;
                this.chance = chance;
                this.cost = cost;
                this.title = Title;
                this.carname = carname;
                this.background = background;
                this.DefaultChance = DefaultChance;
                this.StreamerChance = StreamerChance;
            }
        }
        public class Case
        {
            private Dictionary<(Int32 Min, Int32 Max), CaseItem> Items { get; set; }
            private Int32 ChanceLenght { get; set; }
            public List<CaseItem> Citems { get; set; }
            public Case(Dictionary<Int32, CaseItem> Items)
            {
                this.Citems = new List<CaseItem>();
                this.ChanceLenght = 0;
                this.Items = new Dictionary<(Int32 Min, Int32 Max), CaseItem>();
                foreach (KeyValuePair<Int32, CaseItem> Item in Items)
                {
                    this.Citems.Add(Item.Value);
                    this.Items.Add((this.ChanceLenght, this.ChanceLenght + Item.Key), Item.Value);
                    this.ChanceLenght += Item.Key;
                }
            }
            public CaseItem GetRandom()
            {
                Int32 Random = new Random().Next(1, this.ChanceLenght);
                foreach (KeyValuePair<(Int32 Min, Int32 Max), CaseItem> Item in this.Items) if (Item.Key.Min <= Random && Random <= Item.Key.Max) return Item.Value;
                return null;
            }
        }
    }
}
