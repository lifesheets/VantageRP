using System;
using System.Collections.Generic;
using GTANetworkAPI;
using GolemoSDK;
using System.Data;
using Newtonsoft.Json;
using System.Linq;

namespace Golemo.Core
{
    class ContainerSystem : Script
    {
        private static nLog Log = new nLog("SysContainers");
        public static Dictionary<string, string> ClassList = new Dictionary<string, string>()
        {
                {"premium","Premium" },
                {"medium","Medium" },
                {"premium+","Premium+" },
                {"vip","VIP" },
                {"low","Low" },
                {"Premium","Premium" },
                {"Medium","Medium" },
                {"Premium+","Premium+" },
                {"VIP","VIP" },
                {"Low","Low" },
        };

        public static List<Container> containers = new List<Container>();

        #region MainLoad
        [ServerEvent(Event.ResourceStart)]
        public static void OnResourceStart()
        {
            try
            {
                Blip portblip = NAPI.Blip.CreateBlip(50, new Vector3(1210, -2987, 0), 0.91f, 6, Main.StringToU16("Контейнеры"), 254, 0, true, 0, 0);

                var table = MySQL.QueryRead($"SELECT * FROM `containers`");
                if (table == null || table.Rows.Count == 0)
                {
                    Log.Write("Containers return null result.", nLog.Type.Warn);
                    return;
                }
                foreach (DataRow Row in table.Rows)
                {
                    Container data = new Container(
                    Convert.ToInt32(Row["id"]),
                    Convert.ToString(Row["name"]),
                    JsonConvert.DeserializeObject<Vector3>(Row["position"].ToString()),
                    JsonConvert.DeserializeObject<Vector3>(Row["rotation"].ToString()),
                    Convert.ToInt32(Row["price"]),
                    Convert.ToString(Row["type"]),
                    Convert.ToBoolean(Row["donate"])
                    );

                    Dictionary<string, int> autos = JsonConvert.DeserializeObject<Dictionary<string, int>>(Row["loot"].ToString());

                    foreach (var caritem in autos)
                    {
                        for (int i = 0; i < caritem.Value; i++)
                        {
                            data.Loots.Add(caritem.Key);
                        }
                    }
                    containers.Add(data);
                }

                Log.Write($"Loaded {containers.Count} container");
            }
            catch (Exception e)
            {
                Log.Write($"Containers: {e.Message}", nLog.Type.Error);
            }
        }
        #endregion

        #region Change state containers
        [Command("boxstate")] //команда для вручной активации контейнеров
        public void ChangeStateContainers(Player player, bool state)
        {
            try
            {
                if (!Group.CanUseCmd(player, "boxstate")) return;
                foreach (var item in containers)
                {
                    int contPrice = item.MinBet;
                    item.Visible(state);
                    item.Name = item.Name2;
                    item.ID = item.ID2;
                    item.MinBet = item.MinBet2;
                    item.Loots = item.Loots2;
                    item.NamePrize = "null";
                    item.NumberPrize = "null";
                    item.SellPricecPrize = 0;
                    item.NameBetPlayer2 = "null";
                    item.NamePlayerBet = "null";
                    item.DoorState = false;
                    item.Type = item.Type2;
                    item.Price = item.Price2;
                    Timers.Stop($"Container_Timer_{item.ID}");
                }
            }
            catch { }
        }

        public static void ChangeStateContainers2()
        {
            try
            {
                bool state = true;
                foreach (var item in containers)
                {
                    int contPrice = item.MinBet;
                    item.Visible(state);
                    item.Name = item.Name2;
                    item.ID = item.ID2;
                    item.MinBet = item.MinBet2;
                    item.Loots = item.Loots2;
                    item.NamePrize = "null";
                    item.NumberPrize = "null";
                    item.SellPricecPrize = 0;
                    item.NameBetPlayer2 = "null";
                    item.NamePlayerBet = "null";
                    item.DoorState = false;
                    item.Type = item.Type2;
                    item.Price = item.Price2;
                    Timers.Stop($"Container_Timer_{item.ID}");
                }
                NAPI.Chat.SendChatMessageToAll("[Порт] В штат завезли новую партию контейнеров.");
            }
            catch { }
        }

        [Command("contspawn")]
        public static void ChangeBox(Player player, int id, string container_class)
        {
            if (!Group.CanUseCmd(player, "boxstate")) return;
            if (!ClassList.ContainsKey(container_class))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Такого класса не существует.", 3000);
                return;
            }
            var result = MySQL.QueryRead($"SELECT * FROM `containers` WHERE `name`='{ClassList[container_class]}'");
            foreach (DataRow Row in result.Rows)
            {
                int price = Convert.ToInt32(Row["price"]);
                containers[id].Price = price;
                Dictionary<string, int> autos = JsonConvert.DeserializeObject<Dictionary<string, int>>(Row["loot"].ToString());
                containers[id].Loots.Clear();
                foreach (var caritem in autos)
                {
                    for (int i = 0; i < caritem.Value; i++)
                    {
                        containers[id].Loots.Add(caritem.Key);
                    }
                }
            }
            containers[id].Name = ClassList[container_class];
            containers[id].Visible(true);
            containers[id].NamePrize = "null";
            containers[id].NumberPrize = "null";
            containers[id].SellPricecPrize = 0;
            containers[id].NameBetPlayer2 = "null";
            containers[id].NamePlayerBet = "null";
            containers[id].DoorState = false;
        }
        #endregion

        #region Open menu container
        public static void OpenMenuContainer(Player player)
        {
            if (!player.HasData("ContainerID")) return;
            var acc = Main.Players[player];
            Container container = containers[player.GetData<int>("ContainerID")];
            if (container.label.Text == $"Container №{container.ID} \n ~r~The auction is not carried out") return;
            if (!player.HasData($"ASDsadas_{container.ID}") && !player.HasData($"CONTAINER_PRIZE_{container.ID}"))
            {
                Trigger.ClientEvent(player, "openContainerMenu", container.ID, container.Type, container.Name, container.Price, container.Price + 10000, container.NamePlayerBet);
                return;
            }
            else if (player.HasData($"ASDsadas_{container.ID}"))
            {
                if (player.GetData<bool>($"ASDsadas_{container.ID}") == false) return;
                Trigger.ClientEvent(player, "openMenuContainerOpener", container.ID, container.Type, container.Name, container.Price, container.Price, container.NamePlayerBet);
                return;
            }
            else if (player.HasData($"CONTAINER_PRIZE_{container.ID}"))
            {
                if (player.GetData<bool>($"CONTAINER_PRIZE_{container.ID}") == false) return;
                Trigger.ClientEvent(player, "openPrizMenu", VehicleHandlers.VehiclesName.GetRealVehicleName(container.NamePrize), container.SellPricecPrize, container.ID, container.Type, container.Name, container.Price, container.Price + 10000, container.NamePlayerBet);
                return;
            }
        }
        #endregion

        #region Scripts
        [RemoteEvent("setNewBet")]
        public static void SetNewBet(Player player, int bet)
        {
            if (!player.HasData("ContainerID")) return;
            var acc = Main.Players[player];
            int contID = player.GetData<int>("ContainerID");
            Container container = containers[contID];
            if (acc.Money < bet)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Insufficient funds.", 3000);
                return;
            }
            if (container.NameBetPlayer2 != "null" && bet < container.Price + 10000)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Minimum rate {container.Price + 10000}.", 3000);
                return;
            }
            else if (bet < container.Price)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Minimum rate {container.Price}.", 3000);
                return;
            }
            if (container.NameBetPlayer2 == player.Name.Replace('_', ' '))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You have already bet on this container.", 3000);
                return;
            }
            container.Price = bet;
            MoneySystem.Wallet.Change(player, -bet);
            player.SetData<int>("CONTAINER_PRICE_RETURN", bet);
            foreach (Player p in Main.Players.Keys.ToList())
            {
                if (p.HasData("CONTAINER_PRICE_RETURN"))
                    if (container.NameBetPlayer2 == p.Name.Replace('_', ' '))
                    {
                        MoneySystem.Wallet.Change(p, p.GetData<int>("CONTAINER_PRICE_RETURN"));
                        Notify.Send(p, NotifyType.Info, NotifyPosition.BottomCenter, "Your bet was killed", 3000);
                    }
            }
            container.NameBetPlayer2 = player.Name.Replace('_', ' ');
            int timer = 30;
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You have successfully staged a bet {bet}$", 3000);
            Timers.Stop($"Container_Timer_{contID}");
            Timers.StartTask($"Container_Timer_{contID}", 1000, () =>
            {
                timer--;
                if (timer < 10)
                {
                    container.label.Text = $"Container №{container.ID} \n" + $"Ставка: ~g~{container.Price}$ \n" + $"~w~Поставил: ~r~{container.NameBetPlayer2} \n" + $"~w~0:0{timer}";
                }
                else
                {
                    container.label.Text = $"Container №{container.ID} \n" + $"Ставка: ~g~{container.Price}$ \n" + $"~w~Поставил: ~r~{container.NameBetPlayer2} \n" + $"~w~0:{timer}";
                }
                if (timer == 0)
                {
                    NAPI.Task.Run(() =>
                    {
                        container.label.Text = $"Container №{container.ID} \n" + $"Победитель: ~r~{container.NameBetPlayer2}";
                        player.SetData<bool>($"ASDsadas_{container.ID}", true);
                        container.GenerateLoot(player, container);
                        Timers.Stop($"Container_Timer_{contID}");
                    }, 10);
                }
            });
        }

        [RemoteEvent("ReadBet")]
        public static void QueryReadIdCont(Player player)
        {
            if (!player.HasData("ContainerID")) return;
            Container container = containers[player.GetData<int>("ContainerID")];
            if (container.NameBetPlayer2 != "null")
            {
                Trigger.ClientEvent(player, "SetMaxBet", container.Price + 10000);
            }
            else
            {
                Trigger.ClientEvent(player, "SetMaxBet", container.Price);
            }
        }

        [RemoteEvent("SellPriz")]
        public static void SellPriz(Player player)
        {
            try
            {
                if (!player.HasData("ContainerID")) return;
                Container cont = containers[player.GetData<int>("ContainerID")];
                string vNumber = cont.NumberPrize;
                var price = cont.SellPricecPrize;
                MoneySystem.Wallet.Change(player, price);
                VehicleManager.Remove(vNumber);
                cont.Visible(false);
                cont.CloseDoor();
                var veh = cont.LootVehicle;
                NAPI.Entity.DeleteEntity(veh);
                ResetSettings(cont);
                player.ResetData($"CONTAINER_PRIZE_{cont.ID}");
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы продали машину государству", 3000);
            }
            catch { }
        }

        public static void ResetSettings(Container cont)
        {
            var result = MySQL.QueryRead($"SELECT * FROM `containers` WHERE `name`='{cont.Name}'");
            foreach (DataRow Row in result.Rows)
            {
                var price = Convert.ToInt32(Row["price"]);
                cont.Price = price;
                cont.NameBetPlayer2 = "null";
                cont.NamePrize = "null";
                cont.NumberPrize = "null";
                cont.Visible(false);
                cont.NamePrize = "null";
                cont.NumberPrize = "null";
                cont.SellPricecPrize = 0;
                cont.NameBetPlayer2 = "null";
                cont.NamePlayerBet = "null";
                cont.DoorState = false;
            }
        }

        [RemoteEvent("GetPriz")]
        public static void GetPriz(Player player)
        {
            try
            {
                if (!player.HasData("ContainerID")) return;
                Container cont = containers[player.GetData<int>("ContainerID")];
                if (player.Name.Replace('_', ' ') != cont.NamePlayerBet) return;
                cont.Visible(false);
                cont.CloseDoor();
                var veh = cont.LootVehicle;
                NAPI.Entity.DeleteEntity(veh);
                string vName = cont.NamePrize;
                string vNumber = cont.NumberPrize;
                var house = Houses.HouseManager.GetHouse(player, true);
                ResetSettings(cont);
                player.ResetData($"CONTAINER_PRIZE_{cont.ID}");
                if (house == null || house.GarageID == 0)
                {
                    CreateVeh(player.Name, vNumber, vName, new Color(0, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0));
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Ваш приз - {VehicleHandlers.VehiclesName.GetRealVehicleName(vName)}", 2500);
                    return;
                }
                else
                {
                    var garage = Houses.GarageManager.Garages[house.GarageID];
                    if (vNumber != "none")
                    {
                        garage.SpawnCar(vNumber);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Ваш приз - {VehicleHandlers.VehiclesName.GetRealVehicleName(vName)} будет доставлен в гараж", 2500);
                    }
                }
            }
            catch { }
        }

        public static void CreateVeh(string Holder, string number, string Model, Color Color1, Color Color2, Color Color3, int Health = 1000, int Fuel = 100, int Price = 0)
        {
            VehicleManager.VehicleData data = new VehicleManager.VehicleData();
            data.Holder = Holder;
            data.Model = Model;
            data.Health = Health;
            data.Fuel = Fuel;
            data.Price = Price;
            data.Components = new VehicleManager.VehicleCustomization();
            data.Components.PrimColor = Color1;
            data.Components.SecColor = Color2;
            data.Components.NeonColor = Color3;
            data.Items = new List<nItem>();
            data.Dirt = 0.0F;

            VehicleManager.Vehicles.Add(number, data);
            MySQL.Query("INSERT INTO `vehicles`(`number`, `holder`, `model`, `health`, `fuel`, `price`, `components`, `items`, `keynum`, `dirt`)" +
                $" VALUES ('{number}','{Holder}','{Model}',{Health},{Fuel},{Price},'{JsonConvert.SerializeObject(data.Components)}','{JsonConvert.SerializeObject(data.Items)}',{data.KeyNum},{(byte)data.Dirt})");
            Log.Write("Created new vehicle with number: " + number);
            return;
        }

        [RemoteEvent("OpenContainer")]
        public static void OpenContainer(Player player)
        {
            try
            {
                if (!player.HasData("ContainerID")) return;
                Container cont = containers[player.GetData<int>("ContainerID")];
                cont.label.Text = $"Container №{cont.ID} \n ~r~Победил: {player.Name.Replace('_', ' ')}";
                cont.marker.Color = new Color(255, 0, 0, 0);
                player.ResetData($"ASDsadas_{cont.ID}");
                player.SetData<bool>($"CONTAINER_PRIZE_{cont.ID}", true);
                cont.OpenDoor();
            }
            catch { }
        }
        #endregion
    }

    public class Container
    {
        public static Random rnd = new Random();
        public int ID { get; set; } //номер контейнера
        public string Name { get; set; }
        public int Price { get; set; } //цена
        public bool Donate { get; set; } //за донат или нет
        public bool State { get; set; } = false; //активен или нет
        public bool DoorState { get; set; } = false; //открыты или закрыты
        public string Type { get; set; }
        public int MinBet { get; set; }
        public string NamePlayerBet { get; set; } = "null";
        public string NamePrize { get; set; } = "null";
        public int SellPricecPrize { get; set; } = 0;
        public string NumberPrize { get; set; } = "null";
        public string NameBetPlayer2 { get; set; } = "null";
        public Vehicle LootVehicle { get; set; }

        public List<string> Loots = new List<string>();

        public int ID2 { get; set; } //номер контейнера
        public string Name2 { get; set; }
        public int Price2 { get; set; } //цена
        public string Type2 { get; set; }
        public int MinBet2 { get; set; }

        public List<string> Loots2 = new List<string>();

        public GTANetworkAPI.Object Model;
        public GTANetworkAPI.Object Door_l;
        public GTANetworkAPI.Object Door_R;
        public GTANetworkAPI.Object Fence; //стена

        public GTANetworkAPI.ColShape shape;
        public GTANetworkAPI.Marker marker;
        public GTANetworkAPI.TextLabel label;

        public Container(int id, string name, Vector3 pos, Vector3 rot, int price, string type, bool donate = false, string model = "prop_container_02a", string door_l = "prop_cntrdoor_ld_l", string door_r = "prop_cntrdoor_ld_r", string fence = "prop_fncsec_01b")
        {
            ID = id;
            Name = name;
            Price = price;
            Donate = donate;
            Type = type;
            MinBet = price;

            ID2 = id;
            Name2 = name;
            Price2 = price;
            Type2 = type;
            MinBet2 = price;
            Loots2 = Loots;

            Model = NAPI.Object.CreateObject(NAPI.Util.GetHashKey(model), pos, rot, 255, 0);
            Door_l = NAPI.Object.CreateObject(NAPI.Util.GetHashKey(door_l), pos + new Vector3(1.3, 6.08, 1.4), rot, 255, 0);
            Door_R = NAPI.Object.CreateObject(NAPI.Util.GetHashKey(door_r), pos + new Vector3(-1.3, 6.08, 1.4), rot, 255, 0);
            Fence = NAPI.Object.CreateObject(NAPI.Util.GetHashKey(fence), pos + new Vector3(-1.25, 6.05, 0.5), rot, 0, 0);
            label = NAPI.TextLabel.CreateTextLabel($"Container №{ID} \n ~r~The auction is not carried out", pos + new Vector3(-2, 6.7, 1), 10f, 0.2f, 0, new Color(255, 255, 255), true, NAPI.GlobalDimension);
            marker = NAPI.Marker.CreateMarker(36, pos + new Vector3(-2, 6.7, 1), new Vector3(), new Vector3(), 1.3f, new Color(26, 79, 175, 0));
            shape = NAPI.ColShape.CreateCylinderColShape(pos + new Vector3(-2, 6.7, 0), 1f, 2f, 0);

            shape.OnEntityEnterColShape += (s, player) =>
            {
                if (!State) return;
                NAPI.Data.SetEntityData(player, "INTERACTIONCHECK", 814);
                NAPI.Data.SetEntityData(player, "ContainerID", ID);
            };
            shape.OnEntityExitColShape += (s, player) =>
            {
                if (!State) return;
                NAPI.Data.SetEntityData(player, "INTERACTIONCHECK", 0);
                NAPI.Data.ResetEntityData(player, "ContainerID");
            };
        }

        #region active or diActive
        public void Visible(bool state)
        {
            if (state)
            {
                label.Text = $"Container №{ID} \n ~w~Тип: ~o~{Name} \n ~w~The initial rate: ~g~{Price}$";
                marker.Color = new Color(72, 163, 215, 180);
            }
            else
            {
                label.Text = $"Container №{ID} \n ~r~The auction is not carried out";
                marker.Color = new Color(255, 25, 0, 0);
            }
            State = state;
        }
        #endregion

        #region Open door
        public void OpenDoor()
        {
            int i = 0;
            DoorState = true;
            NAPI.Task.Run(() => {
                Timers.Start($"openDoorContainer{ID}", 1, () =>
                {
                    ++i;
                    if (i >= 120)
                    {
                        Timers.Stop($"openDoorContainer{ID}");
                    }
                    NAPI.Task.Run(() => { Door_l.Rotation -= new Vector3(0, 0, 1); });
                    NAPI.Task.Run(() => { Door_R.Rotation -= new Vector3(0, 0, -1); });
                });
            });
        }
        #endregion

        #region Close door
        public void CloseDoor()
        {
            int i = 0;
            Timers.Stop($"openDoorContainer{ID}");
            NAPI.Task.Run(() => {
                Timers.Start($"closeDoorContainer{ID}", 1, () =>
                {
                    ++i;
                    if (Door_l.Rotation == new Vector3(0, 0, -180)) return;
                    if (Door_R.Rotation == new Vector3(0, 0, 180)) return;
                    if (i >= 120)
                    {
                        DoorState = false;
                        Timers.Stop($"closeDoorContainer{ID}");
                    }
                    NAPI.Task.Run(() => { Door_l.Rotation += new Vector3(0, 0, 1); });
                    NAPI.Task.Run(() => { Door_R.Rotation += new Vector3(0, 0, -1); });
                });
            });
        }
        #endregion

        #region Moving items
        public void Moveloots()
        {
            for (int i = 0; i < Loots.Count; i++)
            {
                Random rnd = new Random();
                int index = rnd.Next(0, Loots.Count);
                string elem = Loots[index];
                Loots.RemoveAt(index);
                Loots.Add(elem);
            }
        }
        #endregion

        #region Generate loot
        public void GenerateLoot(Player player, Container cont)
        {
            Moveloots();
            string vName = Loots[rnd.Next(0, Loots.Count)];
            Vehicle veh = NAPI.Vehicle.CreateVehicle((VehicleHash)NAPI.Util.GetHashKey(vName), Model.Position, Model.Rotation.Z + 180, 0, 0);
            string vNumber = VehicleManager.GenerateNumber2();
            veh.Dimension = 0;
            veh.NumberPlate = vNumber;
            veh.PrimaryColor = 0;
            veh.SecondaryColor = 0;
            veh.Health = 1000;
            veh.Locked = true;
            VehicleStreaming.SetEngineState(veh, true);
            var price = Convert.ToInt32(BusinessManager.ProductsOrderPrice[vName] / 2);
            cont.NumberPrize = vNumber;
            cont.NamePrize = vName;
            cont.SellPricecPrize = price;
            cont.NamePlayerBet = player.Name.Replace('_', ' ');
            cont.LootVehicle = veh;
        }
        #endregion
    }
}