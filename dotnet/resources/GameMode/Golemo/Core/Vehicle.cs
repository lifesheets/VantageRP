using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using Golemo.GUI;
using Newtonsoft.Json;
using System.Linq;
using GolemoSDK;
using MySqlConnector;
using Golemo.Fractions;

namespace Golemo.Core
{
    class VehicleManager : Script
    {
        private static nLog Log = new nLog("Vehicle");
        private static Random Rnd = new Random();

        public static SortedDictionary<string, VehicleData> Vehicles = new SortedDictionary<string, VehicleData>();
        public static SortedDictionary<int, int> VehicleTank = new SortedDictionary<int, int>()
        {
            { -1, 100 },
            { 0, 120 }, // compacts
            { 1, 150 }, // Sedans
            { 2, 200 }, // SUVs
            { 3, 100 }, // Coupes
            { 4, 130 }, // Muscle
            { 5, 150 }, // Sports
            { 6, 100 }, // Sports (classic?)
            { 7, 150 }, // Super
            { 8, 100 }, // Motorcycles
            { 9, 200 }, // Off-Road
            { 10, 150 }, // Industrial
            { 11, 150 }, // Utility
            { 12, 150 }, // Vans
            { 13, 1   }, // cycles
            { 14, 300 }, // Boats
            { 15, 400 }, // Helicopters
            { 16, 500 }, // Planes
            { 17, 130 }, // Service
            { 18, 200 }, // Emergency
            { 19, 150 }, // Military
            { 20, 150 }, // Commercial
            // 21 trains
        };
        public static SortedDictionary<int, int> VehicleRepairPrice = new SortedDictionary<int, int>()
        {
            { -1, 100 }, // compacts
            { 0, 100 }, // compacts
            { 1, 100 }, // Sedans
            { 2, 100 }, // SUVs
            { 3, 100 }, // Coupes
            { 4, 100 }, // Muscle
            { 5, 100 }, // Sports
            { 6, 100 }, // Sports (classic?)
            { 7, 100 }, // Super
            { 8, 100 }, // Motorcycles
            { 9, 100 }, // Off-Road
            { 10, 100 }, // Industrial
            { 11, 100 }, // Utility
            { 12, 100 }, // Vans
            { 13, 100 }, // 13 cycles
            { 14, 100 }, // Boats
            { 15, 100 }, // Helicopters
            { 16, 100 }, // Planes
            { 17, 100 }, // Service
            { 18, 100 }, // Emergency
            { 19, 100 }, // Military
            { 20, 100 }, // Commercial
            // 21 trains
        };
        private static SortedDictionary<int, int> PetrolRate = new SortedDictionary<int, int>()
        {
            { -1, 0 },
            { 0, 1 }, // compacts
            { 1, 1 }, // Sedans
            { 2, 1 }, // SUVs
            { 3, 1 }, // Coupes
            { 4, 1 }, // Muscle
            { 5, 1 }, // Sports
            { 6, 1 }, // Sports (classic?)
            { 7, 1 }, // Super
            { 8, 1 }, // Motorcycles
            { 9, 1 }, // Off-Road
            { 10, 1 }, // Industrial
            { 11, 1 }, // Utility
            { 12, 1 }, // Vans
            { 13, 0 }, // Cycles
            { 14, 1 }, // Boats
            { 15, 1 }, // Helicopters
            { 16, 1 }, // Planes
            { 17, 1 }, // Service
            { 18, 1 }, // Emergency
            { 19, 1 }, // Military
            { 20, 1 }, // Commercial
            // 21 trains
        };
        public static List<VehicleHash> ElectroVehicles = new List<VehicleHash>()
        {
              (VehicleHash)NAPI.Util.GetHashKey("Neon"),
              (VehicleHash)NAPI.Util.GetHashKey("Raiden"),
              (VehicleHash)NAPI.Util.GetHashKey("Cyclone"),
              (VehicleHash)NAPI.Util.GetHashKey("Surge"),
              (VehicleHash)NAPI.Util.GetHashKey("Dilettante"),
              (VehicleHash)NAPI.Util.GetHashKey("Tezeract"),
              (VehicleHash)NAPI.Util.GetHashKey("Imorgon"),
              (VehicleHash)NAPI.Util.GetHashKey("cyber"),
              (VehicleHash)NAPI.Util.GetHashKey("avtr"),
              (VehicleHash)NAPI.Util.GetHashKey("Modelx"),
              (VehicleHash)NAPI.Util.GetHashKey("I8"),
              (VehicleHash)NAPI.Util.GetHashKey("Teslaroad"),
              (VehicleHash)NAPI.Util.GetHashKey("Eqg"),
              (VehicleHash)NAPI.Util.GetHashKey("Cybertruck"),
              (VehicleHash)NAPI.Util.GetHashKey("Taycan21"),
        };
        public static List<VehicleHash> DieselVehicles = new List<VehicleHash>()
        {
              (VehicleHash)NAPI.Util.GetHashKey("Mule"),
              (VehicleHash)NAPI.Util.GetHashKey("Pounder"),
              (VehicleHash)NAPI.Util.GetHashKey("nspeedo"),
              (VehicleHash)NAPI.Util.GetHashKey("srt8"),
              (VehicleHash)NAPI.Util.GetHashKey("g63"),
              (VehicleHash)NAPI.Util.GetHashKey("gle63"),
              (VehicleHash)NAPI.Util.GetHashKey("63gls"),
              (VehicleHash)NAPI.Util.GetHashKey("63gls2"),
              (VehicleHash)NAPI.Util.GetHashKey("msprinter"),
              (VehicleHash)NAPI.Util.GetHashKey("vclass"),
              (VehicleHash)NAPI.Util.GetHashKey("g65amg"),
              (VehicleHash)NAPI.Util.GetHashKey("tahoe2"),
        };
        public static List<VehicleHash> PremiumVehicles = new List<VehicleHash>()
        {
              (VehicleHash)NAPI.Util.GetHashKey("18rs7"),
              (VehicleHash)NAPI.Util.GetHashKey("18velar"),
              (VehicleHash)NAPI.Util.GetHashKey("2019m5"),
              (VehicleHash)NAPI.Util.GetHashKey("sto21"),
              (VehicleHash)NAPI.Util.GetHashKey("m760i"),
              (VehicleHash)NAPI.Util.GetHashKey("gtam21"),
              (VehicleHash)NAPI.Util.GetHashKey("senna"),
              (VehicleHash)NAPI.Util.GetHashKey("f812c21"),
              (VehicleHash)NAPI.Util.GetHashKey("ikx3mso21"),
              (VehicleHash)NAPI.Util.GetHashKey("agerars"),
              (VehicleHash)NAPI.Util.GetHashKey("amggt16"),
              (VehicleHash)NAPI.Util.GetHashKey("astondb11"),
              (VehicleHash)NAPI.Util.GetHashKey("bentayga"),
              (VehicleHash)NAPI.Util.GetHashKey("bentaygast"),
              (VehicleHash)NAPI.Util.GetHashKey("bentley20"),
              (VehicleHash)NAPI.Util.GetHashKey("bmw730"),
              (VehicleHash)NAPI.Util.GetHashKey("bmwm7"),
              (VehicleHash)NAPI.Util.GetHashKey("bmwx7"),
              (VehicleHash)NAPI.Util.GetHashKey("chiron19"),
              (VehicleHash)NAPI.Util.GetHashKey("cls63s"),
              (VehicleHash)NAPI.Util.GetHashKey("cullinan"),
              (VehicleHash)NAPI.Util.GetHashKey("e63s"),
              (VehicleHash)NAPI.Util.GetHashKey("ghost"),
              (VehicleHash)NAPI.Util.GetHashKey("gle63"),
              (VehicleHash)NAPI.Util.GetHashKey("gt63s"),
              (VehicleHash)NAPI.Util.GetHashKey("huracan"),
              (VehicleHash)NAPI.Util.GetHashKey("jesko20"),
              (VehicleHash)NAPI.Util.GetHashKey("kiastinger"),
              (VehicleHash)NAPI.Util.GetHashKey("lc200"),
              (VehicleHash)NAPI.Util.GetHashKey("lex570"),
              (VehicleHash)NAPI.Util.GetHashKey("m4comp"),
              (VehicleHash)NAPI.Util.GetHashKey("m5comp"),
              (VehicleHash)NAPI.Util.GetHashKey("panamera17turbo"),
              (VehicleHash)NAPI.Util.GetHashKey("rs72"),
              (VehicleHash)NAPI.Util.GetHashKey("s63cab"),
              (VehicleHash)NAPI.Util.GetHashKey("starone"),
              (VehicleHash)NAPI.Util.GetHashKey("urus"),
              (VehicleHash)NAPI.Util.GetHashKey("x6m2"),
        };
        private const int MIN_ENGINE_HEALTH_AFTER_WHICH_ENGINE_NOT_START = 300;

        public VehicleManager()
        {
            try
            {
                Timers.StartTask("mile", 10, () => Mile());
                Timers.StartTask("fuel", 30000, () => FuelControl());

                Log.Write("Loading Vehicles...");
                DataTable result = MySQL.QueryRead("SELECT * FROM `vehicles`");
                if (result == null || result.Rows.Count == 0)
                {
                    Log.Write("DB return null result.", nLog.Type.Warn);
                    return;
                }
                int count = 0;
                foreach (DataRow Row in result.Rows)
                {
                    count++;
                    VehicleData data = new VehicleData();
                    data.Holder = Convert.ToString(Row["holder"]);
                    data.Model = Convert.ToString(Row["model"]);
                    data.Health = Convert.ToInt32(Row["health"]);
                    data.Fuel = Convert.ToInt32(Row["fuel"]);
                    data.Price = Convert.ToInt32(Row["price"]);
                    data.Components = JsonConvert.DeserializeObject<VehicleCustomization>(Row["components"].ToString());
                    data.Items = JsonConvert.DeserializeObject<List<nItem>>(Row["items"].ToString());
                    data.Position = Convert.ToString(Row["position"]);
                    data.Rotation = Convert.ToString(Row["rotation"]);
                    data.KeyNum = Convert.ToInt32(Row["keynum"]);
                    data.Dirt = Convert.ToSingle(Row["dirt"]);
                    data.Сasco = Convert.ToInt32(Row["casco"]);
                    data.Sell = Convert.ToInt32(Row["sell"]);
                    Vehicles.Add(Convert.ToString(Row["number"]), data);

                }
                Log.Write($"Vehicles are loaded ({count})", nLog.Type.Success);
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }
        static DateTime Now = DateTime.Now;
        static void Mile()
        {
            NAPI.Task.Run(() => {
                Vehicle localveh = null;
                try
                {
                    foreach (Vehicle veh in NAPI.Pools.GetAllVehicles())
                    {
                        localveh = veh;
                        if (!veh.HasSharedData("MILE") || !veh.EngineStatus) continue;
                        Vector3 velocity = NAPI.Entity.GetEntityVelocity(veh.Handle);
                        double speeds = Math.Sqrt((velocity.X * velocity.X) + (velocity.Y * velocity.Y) + (velocity.Z * velocity.Z)) * 3.6;
                        float trip = (float)((float)speeds * ((DateTime.Now - Now).TotalSeconds / 1000) * 1000);
                        if (Vehicles.ContainsKey(veh.NumberPlate))
                        {
                            Vehicles[veh.NumberPlate].Sell += trip / 5;
                            float distance = Vehicles[veh.NumberPlate].Sell;
                            veh.SetSharedData("MILE", distance);
                            if (distance > 100000)
                            {
                                //NAPI.Vehicle.SetVehicleEnginePowerMultiplier(veh, -4);
                                //NAPI.Vehicle.SetVehicleEngineTorqueMultiplier(veh, -4);
                            }

                            Save(veh.NumberPlate);
                        }
                        else
                        {
                            float distance = veh.GetSharedData<float>("MILE") + trip / 5;
                        }

                    }
                    Now = DateTime.Now;
                }
                catch (Exception e) { Log.Write($"MILE ({localveh.NumberPlate}): " + e.ToString(), nLog.Type.Error); }
            });
        }
        private static void FuelControl()
        {
            NAPI.Task.Run(() =>
            {
                List<Vehicle> allVehicles = NAPI.Pools.GetAllVehicles();
                if (allVehicles.Count == 0) return;
                foreach (Vehicle veh in allVehicles)
                {
                    int fuel = 0;
                    float engineHealth;
                    try
                    {
                        if (!veh.HasSharedData("PETROL")) continue;
                        if (!Core.VehicleStreaming.GetEngineState(veh)) continue;

                        fuel = veh.GetSharedData<int>("PETROL");
                        engineHealth = NAPI.Vehicle.GetVehicleEngineHealth(veh);

                        if (engineHealth <= MIN_ENGINE_HEALTH_AFTER_WHICH_ENGINE_NOT_START)
                        {
                            VehicleStreaming.SetEngineState(veh, false);
                        }

                        if (fuel == 0) continue;
                        if (fuel - PetrolRate[veh.Class] <= 0)
                        {
                            fuel = 0;
                            Core.VehicleStreaming.SetEngineState(veh, false);
                        }
                        else if (!BusinessManager.ElectroCar.Contains((VehicleHash)veh.Model))
                        {

                            fuel -= PetrolRate[veh.Class];
                        }
                        else
                        {
                            fuel -= 3;
                        }
                        if (NAPI.Data.GetEntityData(veh, "ACCESS") == "PERSONAL")
                        {
                            Vehicles[veh.NumberPlate].Fuel = fuel;
                        }
                        veh.SetSharedData("PETROL", fuel);
                    }
                    catch (Exception e)
                    {
                        Log.Write($"FUELCONTROL_TIMER: {veh.NumberPlate} {fuel}\n{e.Message}", nLog.Type.Error);
                    }
                }
            });
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void onPlayerEnterVehicleHandler(Player player, Vehicle vehicle, sbyte seatid)
        {
            try
            {
                if (!vehicle.HasData("OCCUPANTS"))
                {
                    List<Player> occupantsList = new List<Player>();
                    occupantsList.Add(player);
                    vehicle.SetData("OCCUPANTS", occupantsList);
                }
                else
                {
                    if (!vehicle.GetData<List<Player>>("OCCUPANTS").Contains(player)) vehicle.GetData<List<Player>>("OCCUPANTS").Add(player);
                }

                if (player.VehicleSeat == 0)
                {
                    if (VehicleHandlers.AutoPilot.HasAccessToAutopilot((VehicleHash)player.Vehicle.Model))
                    {
                        player.SetSharedData("isAutouPilot", true);
                    }
                    if (NAPI.Data.GetEntityData(vehicle, "ACCESS") == "FRACTION")
                    {
                        if (NAPI.Data.GetEntityData(vehicle, "FRACTION") == 14 && vehicle.DisplayName == "BARRACKS")
                        {
                            int fracid = Main.Players[player].FractionID;
                            if ((fracid >= 1 && fracid <= 5) || (fracid >= 10 && fracid <= 13))
                            {
                                if (DateTime.Now.Hour < 10)
                                {
                                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Невозможно сесть в машину с 00:00 до 10:00", 3000);
                                    return;
                                }
                                return;
                            }
                            else if (fracid == 14)
                            {
                                if (Main.Players[player].FractionLVL < NAPI.Data.GetEntityData(vehicle, "MINRANK"))
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому транспорту", 3000);
                                    VehicleManager.WarpPlayerOutOfVehicle(player);
                                    return;
                                }
                                return;
                            }
                            else
                                VehicleManager.WarpPlayerOutOfVehicle(player);
                        }
                        if (NAPI.Data.GetEntityData(vehicle, "FRACTION") == Main.Players[player].FractionID)
                        {
                            if (Main.Players[player].FractionLVL < NAPI.Data.GetEntityData(vehicle, "MINRANK"))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому транспорту", 3000);
                                VehicleManager.WarpPlayerOutOfVehicle(player);
                                return;
                            }
                        }
                        else
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому транспорту", 3000);
                            VehicleManager.WarpPlayerOutOfVehicle(player);
                            return;
                        }
                    }
                    else if (NAPI.Data.GetEntityData(vehicle, "ACCESS") == "WORK" && player.GetData<Vehicle>("WORK") == vehicle)
                        return;
                }
            }
            catch (Exception e) { Log.Write("PlayerEnterVehicle: " + e.Message, nLog.Type.Error); }
        }

        [ServerEvent(Event.PlayerExitVehicleAttempt)]
        public void onPlayerExitVehicleHandler(Player player, Vehicle vehicle)
        {
            try
            {
                if (!vehicle.HasData("OCCUPANTS"))
                {
                    List<Player> occupantsList = new List<Player>();
                    vehicle.SetData("OCCUPANTS", occupantsList);
                }
                else
                {
                    if (vehicle.GetData<List<Player>>("OCCUPANTS").Contains(player)) vehicle.GetData<List<Player>>("OCCUPANTS").Remove(player);
                }
                if (player.HasSharedData("isAutouPilot"))
                    player.ResetSharedData("isAutouPilot");
            }
            catch (Exception e) { Log.Write("PlayerExitVehicleAttempt: " + e.Message, nLog.Type.Error); }
        }

        public static void API_onPlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            try
            {
                if (player.IsInVehicle)
                {
                    Vehicle vehicle = player.Vehicle;
                    if (!vehicle.HasData("OCCUPANTS"))
                    {
                        List<Player> occupantsList = new List<Player>();
                        vehicle.SetData("OCCUPANTS", occupantsList);
                    }
                    else
                    {
                        if (vehicle.GetData<List<Player>>("OCCUPANTS").Contains(player)) vehicle.GetData<List<Player>>("OCCUPANTS").Remove(player);
                    }
                }

                if (NAPI.Data.HasEntityData(player, "WORK_CAR_EXIT_TIMER"))
                    //Main.StopT(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"), "WORK_CAR_EXIT_TIMER_vehicle");
                    Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }

        public static void WarpPlayerOutOfVehicle(Player player)
        {
            Vehicle vehicle = player.Vehicle;
            if (vehicle == null) return;
            if (!vehicle.HasData("OCCUPANTS"))
            {
                List<Player> occupantsList = new List<Player>();
                vehicle.SetData("OCCUPANTS", occupantsList);
            }
            else
            {
                if (vehicle.GetData<List<Player>>("OCCUPANTS").Contains(player)) vehicle.GetData<List<Player>>("OCCUPANTS").Remove(player);
            }
            Trigger.ClientEvent(player, "PLAYER::TASK_LEAVE_ON_VEHICLE");
            //player.WarpOutOfVehicle();
        }

        public static List<Player> GetVehicleOccupants(Vehicle vehicle)
        {
            if (!vehicle.HasData("OCCUPANTS"))
                return new List<Player>();
            else
                return vehicle.GetData<List<Player>>("OCCUPANTS");
        }

        public static void RepairCar(Vehicle vehicle)
        {
            vehicle.Repair();
            VehicleStreaming.VehicleSyncData data = VehicleStreaming.GetVehicleSyncData(vehicle);
            if (data == default(VehicleStreaming.VehicleSyncData))
                data = new VehicleStreaming.VehicleSyncData();
            data.BodyHealth = 1000.0f;
            data.EngineHealth = 1000.0f;
            data.Dirt = 0.0f;
            VehicleStreaming.UpdateVehicleSyncData(vehicle, data);
        }

        public static string Create(string Holder, string Model, Color Color1, Color Color2, Color Color3, int Health = 1000, int Fuel = 100, int Price = 0)
        {
            VehicleData data = new VehicleData();
            data.Holder = Holder;
            data.Model = Model;
            data.Health = Health;
            data.Fuel = Fuel;
            data.Price = Price;
            data.Components = new VehicleCustomization();
            data.Components.PrimColor = Color1;
            data.Components.SecColor = Color2;
            data.Components.NeonColor = Color3;
            data.Items = new List<nItem>();
            data.KeyNum = 0;
            data.Dirt = 0.0F;
            data.Сasco = 0;

            string Number = GenerateNumber2();
            Vehicles.Add(Number, data);
            MySQL.Query("INSERT INTO `vehicles`(`number`, `holder`, `model`, `health`, `fuel`, `price`, `components`, `items`, `keynum`, `dirt`, `sell`)" +
                $" VALUES ('{Number}','{Holder}','{Model}',{Health},{Fuel},{Price},'{JsonConvert.SerializeObject(data.Components)}','{JsonConvert.SerializeObject(data.Items)}',{data.KeyNum},{(byte)data.Dirt},'0')");

            return Number;
        }
        public static void Remove(string Number, Player player = null)
        {
            if (!Vehicles.ContainsKey(Number)) return;
            try
            {
                Houses.House house = Houses.HouseManager.GetHouse(Vehicles[Number].Holder, true);
                if (house != null)
                {
                    Houses.Garage garage = Houses.GarageManager.Garages[house.GarageID];
                    garage.DeleteCar(Number);
                }
                else
                {
                    foreach (var item in NAPI.Pools.GetAllVehicles())
                    {
                        if(item.NumberPlate == Number)
                        {
                            NAPI.Task.Run(() => { NAPI.Entity.DeleteEntity(item); });
                            break;
                        }
                    }
                }
            }
            catch { }
            Vehicles.Remove(Number);
            MySQL.Query($"DELETE FROM `vehicles` WHERE number='{Number}'");
        }
        public static void Spawn(string Number, Vector3 Pos, float Rot, Player owner)
        {
            if (!Vehicles.ContainsKey(Number))
            {
                Log.Write(owner.Name + " failed to spawn vehicle " + Number);
                return;
            }

            VehicleData data = Vehicles[Number];
            // VehicleHash model = (VehicleHash)NAPI.Util.GetHashKey(data.Model);
            VehicleHash model = (VehicleHash)NAPI.Util.GetHashKey(data.Model);
            Vehicle veh = NAPI.Vehicle.CreateVehicle(model, Pos, Rot, 0, 0);

            veh.Health = data.Health;
            veh.NumberPlate = Number;
            veh.SetSharedData("PETROL", data.Fuel);
            veh.SetData("ACCESS", "PERSONAL");
            veh.SetData("OWNER", owner);
            veh.SetData("ITEMS", data.Items);

            if (VehicleManager.Vehicles[veh.NumberPlate].Model == "msprinter") veh.SetData("MSPRINTER", true);
            NAPI.Vehicle.SetVehicleNumberPlate(veh, Number);
            VehicleStreaming.SetEngineState(veh, false);
            VehicleStreaming.SetLockStatus(veh, true);
            VehicleManager.ApplyCustomization(veh);
            owner.SetIntoVehicle(veh, 0);
        }
        public static bool Save(string Number)
        {
            if (!Vehicles.ContainsKey(Number)) return false;
            VehicleData data = Vehicles[Number];
            string items = JsonConvert.SerializeObject(data.Items);
            if (string.IsNullOrEmpty(items) || items == null) items = "[]";
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = "UPDATE `vehicles` SET holder=@hold, model=@model, health=@hp, fuel=@fuel, components=@comp, items=@it,position=@pos,rotation=@rot,keynum=@keyn,dirt=@dirt,sell=@sell WHERE number=@numb";
            cmd.Parameters.AddWithValue("@hold", data.Holder);
            cmd.Parameters.AddWithValue("@model", data.Model);
            cmd.Parameters.AddWithValue("@hp", data.Health);
            cmd.Parameters.AddWithValue("@fuel", data.Fuel);
            cmd.Parameters.AddWithValue("@comp", JsonConvert.SerializeObject(data.Components));
            cmd.Parameters.AddWithValue("@it", items);
            cmd.Parameters.AddWithValue("@pos", data.Position);
            cmd.Parameters.AddWithValue("@rot", data.Rotation);
            cmd.Parameters.AddWithValue("@keyn", data.KeyNum);
            cmd.Parameters.AddWithValue("@dirt", (byte)data.Dirt);
            cmd.Parameters.AddWithValue("@numb", Number);
            cmd.Parameters.AddWithValue("@casc", data.Сasco);
            cmd.Parameters.AddWithValue("@sell", data.Sell);
            MySQL.Query(cmd);

            return true;
        }
        public static bool isHaveAccess(Player Player, Vehicle Vehicle)
        {
            if (NAPI.Data.GetEntityData(Vehicle, "ACCESS") == "WORK")
            {
                if (NAPI.Data.GetEntityData(Player, "WORK") != Vehicle)
                    return false;
                else
                    return true;
            }
            else if (NAPI.Data.GetEntityData(Vehicle, "ACCESS") == "FRACTION")
            {
                if (Main.Players[Player].FractionID != NAPI.Data.GetEntityData(Vehicle, "FRACTION"))
                    return false;
                else
                    return true;
            }
            else if (NAPI.Data.GetEntityData(Vehicle, "ACCESS") == "PERSONAL")
            {
                bool access = canAccessByNumber(Player, Vehicle.NumberPlate);
                if (access)
                    return true;
                else
                    return false;
            }
            else if (NAPI.Data.GetEntityData(Vehicle, "ACCESS") == "GARAGE")
            {
                bool access = canAccessByNumber(Player, Vehicle.NumberPlate);
                if (access)
                    return true;
                else
                    return false;
            }
            else if (NAPI.Data.GetEntityData(Vehicle, "ACCESS") == "HOTEL")
            {
                if (NAPI.Data.HasEntityData(Player, "HOTELCAR") && NAPI.Data.GetEntityData(Player, "HOTELCAR") == Vehicle)
                {
                    return true;
                }
                else
                    return false;
            }
            else if (NAPI.Data.GetEntityData(Vehicle, "ACCESS") == "RENT")
            {
                if (NAPI.Data.GetEntityData(Vehicle, "DRIVER") == Player)
                {
                    return true;
                }
                else
                    return false;
            }
            return true;
        }
        public static Vehicle getNearestVehicle(Player player, int radius)
        {
            List<Vehicle> all_vehicles = NAPI.Pools.GetAllVehicles();
            Vehicle nearest_vehicle = null;
            foreach (Vehicle v in all_vehicles)
            {
                if (v.Dimension != player.Dimension) continue;
                if (nearest_vehicle == null && player.Position.DistanceTo(v.Position) < radius)
                {
                    nearest_vehicle = v;
                    continue;
                }
                else if (nearest_vehicle != null)
                {
                    if (player.Position.DistanceTo(v.Position) < player.Position.DistanceTo(nearest_vehicle.Position))
                    {
                        nearest_vehicle = v;
                        continue;
                    }
                }
            }
            return nearest_vehicle;
        }
        public static List<string> getAllPlayerVehicles(string playername)
        {
            List<string> all_number = new List<string>();
            foreach (KeyValuePair<string, VehicleData> accVehicle in Vehicles)
                if (accVehicle.Value.Holder == playername)
                {
                    all_number.Add(accVehicle.Key);
                }
            return all_number;
        }

        public static void sellCar(Player player, Player target)
        {
            player.SetData("SELLCARFOR", target);
            OpenSellCarMenu(player);
        }

        #region Selling Menu
        public static void OpenSellCarMenu(Player player)
        {
            Menu menu = new Menu("sellcar", false, true);
            menu.Callback = callback_sellcar;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Продажа машины";
            menu.Add(menuItem);

            foreach (string number in getAllPlayerVehicles(player.Name))
            {
                menuItem = new Menu.Item(number, Menu.MenuItem.Button);
                menuItem.Text = Vehicles[number].Model + " - " + number;
                menu.Add(menuItem);
            }

            menuItem = new Menu.Item("close", Menu.MenuItem.Button);
            menuItem.Text = "Закрыть";
            menu.Add(menuItem);

            menu.Open(player);
        }

        private static void callback_sellcar(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            MenuManager.Close(player);
            if (item.ID == "close") return;
            player.SetData("SELLCARNUMBER", item.ID);
            Trigger.ClientEvent(player, "openInput", "Продать машину", "Введите цену", 7, "sellcar");
        }
        #endregion

        public static void FracApplyCustomization(Vehicle veh, int fraction)
        {
            try
            {
                if (veh != null)
                {
                    if (!Configs.FractionVehicles[fraction].ContainsKey(veh.NumberPlate)) return;

                    VehicleCustomization data = Configs.FractionVehicles[fraction][veh.NumberPlate].Item7;

                    if (data.NeonColor.Alpha != 0)
                    {
                        NAPI.Vehicle.SetVehicleNeonState(veh, true);
                        NAPI.Vehicle.SetVehicleNeonColor(veh, data.NeonColor.Red, data.NeonColor.Green, data.NeonColor.Blue);
                    }

                    veh.SetMod(4, data.Muffler);
                    veh.SetMod(3, data.SideSkirt);
                    veh.SetMod(7, data.Hood);
                    veh.SetMod(0, data.Spoiler);
                    veh.SetMod(6, data.Lattice);
                    veh.SetMod(8, data.Wings);
                    veh.SetMod(10, data.Roof);
                    veh.SetMod(48, data.Vinyls);
                    veh.SetMod(1, data.FrontBumper);
                    veh.SetMod(2, data.RearBumper);

                    veh.SetMod(11, data.Engine);
                    veh.SetMod(18, data.Turbo);
                    veh.SetMod(13, data.Transmission);
                    veh.SetMod(15, data.Suspension);
                    veh.SetMod(12, data.Brakes);
                    veh.SetMod(14, data.Horn);

                    veh.WindowTint = data.WindowTint;
                    veh.NumberPlateStyle = data.NumberPlate;

                    if (data.Headlights >= 0)
                    {
                        veh.SetMod(22, 0);
                        veh.SetSharedData("hlcolor", data.Headlights);
                        Trigger.ClientEventInRange(veh.Position, 250f, "VehStream_SetVehicleHeadLightColor", veh.Handle, data.Headlights);
                    }
                    else
                    {
                        veh.SetMod(22, -1);
                        veh.SetSharedData("hlcolor", 0);
                    }

                    veh.WheelType = data.WheelsType;
                    veh.SetMod(23, data.Wheels);
                }
            }
            catch (Exception e) { Log.Write("ApplyCustomization: " + e.Message, nLog.Type.Error); }
        }

        public static void ApplyCustomization(Vehicle veh)
        {
            try
            {
                if (veh != null)
                {
                    if (!Vehicles.ContainsKey(veh.NumberPlate)) return;

                    VehicleCustomization data = Vehicles[veh.NumberPlate].Components;

                    if (data.NeonColor.Alpha != 0)
                    {
                        NAPI.Vehicle.SetVehicleNeonState(veh, true);
                        NAPI.Vehicle.SetVehicleNeonColor(veh, data.NeonColor.Red, data.NeonColor.Green, data.NeonColor.Blue);
                    }

                    veh.SetMod(4, data.Muffler);
                    veh.SetMod(3, data.SideSkirt);
                    veh.SetMod(7, data.Hood);
                    veh.SetMod(0, data.Spoiler);
                    veh.SetMod(6, data.Lattice);
                    veh.SetMod(8, data.Wings);
                    veh.SetMod(10, data.Roof);
                    veh.SetMod(48, data.Vinyls);
                    veh.SetMod(1, data.FrontBumper);
                    veh.SetMod(2, data.RearBumper);

                    veh.SetMod(11, data.Engine);
                    veh.SetMod(18, data.Turbo);
                    veh.SetMod(13, data.Transmission);
                    veh.SetMod(15, data.Suspension);
                    veh.SetMod(12, data.Brakes);
                    veh.SetMod(14, data.Horn);
                    veh.SetMod(16, data.Armor);

                    veh.WindowTint = data.WindowTint;
                    veh.NumberPlateStyle = data.NumberPlate;

                    if (data.Headlights >= 0)
                    {
                        veh.SetMod(22, 0);
                        veh.SetSharedData("hlcolor", data.Headlights);
                        Trigger.ClientEventInRange(veh.Position, 250f, "VehStream_SetVehicleHeadLightColor", veh.Handle, data.Headlights);
                    }
                    else
                    {
                        veh.SetMod(22, -1);
                        veh.SetSharedData("hlcolor", 0);
                    }


                    if (data.PrimModColor != -1)
                    {
                        NAPI.Vehicle.SetVehiclePrimaryColor(veh, data.PrimModColor);
                        NAPI.Vehicle.SetVehicleSecondaryColor(veh, data.SecModColor);

                        VehicleStreaming.SetVehicleColors(veh, data.PrimModColor, data.SecModColor);
                    }

                    NAPI.Vehicle.SetVehicleCustomPrimaryColor(veh, data.PrimColor.Red, data.PrimColor.Green, data.PrimColor.Blue);
                    NAPI.Vehicle.SetVehicleCustomSecondaryColor(veh, data.SecColor.Red, data.SecColor.Green, data.SecColor.Blue);

                    veh.WheelType = data.WheelsType;
                    veh.SetMod(23, data.Wheels);

                    VehicleStreaming.SetVehicleDirt(veh, Vehicles[veh.NumberPlate].Dirt);
                }
            }
            catch (Exception e) { Log.Write("ApplyCustomization: " + e.Message, nLog.Type.Error); }
        }

        public static void ChangeVehicleDoors(Player player, Vehicle vehicle)
        {
            switch (NAPI.Data.GetEntityData(vehicle, "ACCESS"))
            {
                case "HOTEL":
                    if (NAPI.Data.GetEntityData(vehicle, "OWNER") != player && Main.Players[player].AdminLVL < 3)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You do not have keys for this vehicle", 3000);
                        return;
                    }
                    if (Core.VehicleStreaming.GetLockState(vehicle))
                    {
                        Core.VehicleStreaming.SetLockStatus(vehicle, false);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You opened the car doors", 3000);
                    }
                    else
                    {
                        Core.VehicleStreaming.SetLockStatus(vehicle, true);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You closed the car doors", 3000);
                    }
                    break;
                case "RENT":
                    if (NAPI.Data.GetEntityData(vehicle, "DRIVER") != player && Main.Players[player].AdminLVL < 3)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You do not have keys for this vehicle", 3000);
                        return;
                    }
                    if (Core.VehicleStreaming.GetLockState(vehicle))
                    {
                        Core.VehicleStreaming.SetLockStatus(vehicle, false);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You opened the car doors", 3000);
                    }
                    else
                    {
                        Core.VehicleStreaming.SetLockStatus(vehicle, true);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You closed the car doors", 3000);
                    }
                    break;
                case "WORK":
                    if (NAPI.Data.GetEntityData(player, "WORK") != vehicle && Main.Players[player].AdminLVL < 3)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You do not have keys for this vehicle", 3000);
                        return;
                    }
                    if (Core.VehicleStreaming.GetLockState(vehicle))
                    {
                        Core.VehicleStreaming.SetLockStatus(vehicle, false);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You opened the car doors", 3000);
                    }
                    else
                    {
                        Core.VehicleStreaming.SetLockStatus(vehicle, true);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You closed the car doors", 3000);
                    }
                    break;
                case "PERSONAL":

                    bool access = canAccessByNumber(player, vehicle.NumberPlate);
                    if (!access && Main.Players[player].AdminLVL < 3)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You do not have keys for this vehicle", 3000);
                        return;
                    }

                    if (Core.VehicleStreaming.GetLockState(vehicle))
                    {
                        Core.VehicleStreaming.SetLockStatus(vehicle, false);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You opened the car doors", 3000);
                        return;
                    }
                    else
                    {
                        Core.VehicleStreaming.SetLockStatus(vehicle, true);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You closed the car doors", 3000);
                        return;
                    }
                case "GARAGE":

                    access = canAccessByNumber(player, vehicle.NumberPlate);
                    if (!access && Main.Players[player].AdminLVL < 3)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You do not have keys for this vehicle", 3000);
                        return;
                    }

                    if (Core.VehicleStreaming.GetLockState(vehicle))
                    {
                        Core.VehicleStreaming.SetLockStatus(vehicle, false);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You opened the car doors", 3000);
                        return;
                    }
                    else
                    {
                        Core.VehicleStreaming.SetLockStatus(vehicle, true);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You closed the car doors", 3000);
                        return;
                    }
                case "ADMIN":
                    if (Main.Players[player].AdminLVL == 0)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You do not have keys for this vehicle", 3000);
                        return;
                    }

                    if (Core.VehicleStreaming.GetLockState(vehicle))
                    {
                        Core.VehicleStreaming.SetLockStatus(vehicle, false);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You opened the car doors", 3000);
                        return;
                    }
                    else
                    {
                        Core.VehicleStreaming.SetLockStatus(vehicle, true);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You closed the car doors", 3000);
                        return;
                    }
                default:
                    return;
            }
            return;
        }
        public static bool canAccessByNumber(Player player, string number)
        {
            List<nItem> items = nInventory.Items[Main.Players[player].UUID];
            string needData = $"{number}_{Vehicles[number].KeyNum}";
            bool access = (items.FindIndex(i => i.Type == ItemType.CarKey && i.Data == needData) != -1);

            if (!access)
            {
                int index = items.FindIndex(i => i.Type == ItemType.KeyRing && new List<string>(Convert.ToString(i.Data).Split('/')).Contains(needData));
                if (index != -1) access = true;
            }

            return access;
        }
        // ///// need refactoring //// //
        public static void onClientEvent(Player sender, string eventName, params object[] args)
        {
            switch (eventName)
            {

                case "engineCarPressed":
                    #region Engine button
                    if (!NAPI.Player.IsPlayerInAnyVehicle(sender)) return;
                    if (sender.VehicleSeat != 0)
                    {
                        Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"You must be in the driver's seat", 3000);
                        return;
                    }
                    Vehicle vehicle = sender.Vehicle;
                    if (vehicle.Class == 13 && Main.Players[sender].InsideGarageID == -1) return;

                    if (vehicle.GetSharedData<string>("ACCESS") == "DUMMY") return;

                    int fuel = vehicle.GetSharedData<int>("PETROL");
                    if (fuel <= 0)
                    {
                        Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"Fuel tank is empty, car cannot be started", 3000);
                        return;
                    }
                    if (NAPI.Vehicle.GetVehicleEngineHealth(vehicle) <= MIN_ENGINE_HEALTH_AFTER_WHICH_ENGINE_NOT_START)
                    {
                        Notify.Error(sender, "Двигатель сломан, нужно починить");
                        return;
                    }
            switch (NAPI.Data.GetEntityData(vehicle, "ACCESS"))
                    {
                        case "HOTEL":
                            if (NAPI.Data.GetEntityData(vehicle, "OWNER") != sender && Main.Players[sender].AdminLVL < 3)
                            {
                                Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"You do not have keys for this vehicle", 3000);
                                return;
                            }
                            if (Core.VehicleStreaming.GetEngineState(vehicle))
                            {
                                Core.VehicleStreaming.SetEngineState(vehicle, false);
                                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"You turned off the car engine", 3000);
                            }
                            else
                            {
                                Core.VehicleStreaming.SetEngineState(vehicle, true);
                                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"you started the car", 3000);
                            }
                            break;
                        case "SCHOOL":
                            if (NAPI.Data.GetEntityData(vehicle, "DRIVER") != sender && Main.Players[sender].AdminLVL < 3)
                            {
                                Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"You do not have keys for this vehicle", 3000);
                                return;
                            }
                            if (Core.VehicleStreaming.GetEngineState(vehicle))
                            {
                                Core.VehicleStreaming.SetEngineState(vehicle, false);
                                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"You turned off the car engine", 3000);
                            }
                            else
                            {
                                Core.VehicleStreaming.SetEngineState(vehicle, true);
                                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"you started the car", 3000);
                            }
                            break;
                        case "beltCarPressed":
                            if (!NAPI.Player.IsPlayerInAnyVehicle(sender)) return;

                            bool beltstate = Convert.ToBoolean(args[0]);

                            if (!beltstate) Commands.RPChat("me", sender, "пристегнул(а) ремень безопасности");
                            else Commands.RPChat("me", sender, "отслегнул(а) ремень безопасности");

                            break;
                        case "RENT":
                            if (NAPI.Data.GetEntityData(vehicle, "DRIVER") != sender && Main.Players[sender].AdminLVL < 3)
                            {
                                Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"You do not have keys for this vehicle", 3000);
                                return;
                            }
                            if (Core.VehicleStreaming.GetEngineState(vehicle))
                            {
                                Core.VehicleStreaming.SetEngineState(vehicle, false);
                                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"You turned off the car engine", 3000);
                            }
                            else
                            {
                                Core.VehicleStreaming.SetEngineState(vehicle, true);
                                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"you started the car", 3000);
                            }
                            break;
                        case "WORK":
                            if (NAPI.Data.GetEntityData(sender, "WORK") != vehicle && Main.Players[sender].AdminLVL < 3)
                            {
                                Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"You do not have keys for this vehicle", 3000);
                                return;
                            }
                            if (Core.VehicleStreaming.GetEngineState(vehicle))
                            {
                                Core.VehicleStreaming.SetEngineState(vehicle, false);
                                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"You turned off the car engine", 3000);
                            }
                            else
                            {
                                Core.VehicleStreaming.SetEngineState(vehicle, true);
                                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"you started the car", 3000);
                            }
                            break;
                        case "FRACTION":
                            if (Main.Players[sender].FractionID != NAPI.Data.GetEntityData(vehicle, "FRACTION"))
                            {
                                Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"You do not have keys for this vehicle", 3000);
                                return;
                            }
                            if (Core.VehicleStreaming.GetEngineState(vehicle))
                            {
                                Core.VehicleStreaming.SetEngineState(vehicle, false);
                                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"You turned off the car engine", 3000);
                            }
                            else
                            {
                                Core.VehicleStreaming.SetEngineState(vehicle, true);
                                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"you started the car", 3000);
                            }
                            break;
                        case "PERSONAL":

                            bool access = canAccessByNumber(sender, vehicle.NumberPlate);
                            if (!access && Main.Players[sender].AdminLVL < 3)
                            {
                                Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"You do not have keys for this vehicle", 3000);
                                return;
                            }

                            if (Core.VehicleStreaming.GetEngineState(vehicle))
                            {
                                Core.VehicleStreaming.SetEngineState(vehicle, false);
                                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"You turned off the car engine", 3000);
                            }
                            else
                            {
                                Core.VehicleStreaming.SetEngineState(vehicle, true);
                                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"you started the car", 3000);
                            }
                            break;
                        case "GARAGE":
                            if (Main.Players[sender].InsideGarageID == -1) return;
                            string number = NAPI.Vehicle.GetVehicleNumberPlate(vehicle);

                            Houses.Garage garage = Houses.GarageManager.Garages[Main.Players[sender].InsideGarageID];
                            garage.RemovePlayer(sender);

                            garage.GetVehicleFromGarage(sender, number);
                            break;
                        case "MAFIADELIVERY":
                        case "GANGDELIVERY":
                            if (Core.VehicleStreaming.GetEngineState(vehicle))
                            {
                                Core.VehicleStreaming.SetEngineState(vehicle, false);
                                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"You turned off the car engine", 3000);
                            }
                            else
                            {
                                Core.VehicleStreaming.SetEngineState(vehicle, true);
                                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"you started the car", 3000);
                            }
                            break;
                        case "ADMIN":
                            if (Main.Players[sender].AdminLVL == 0)
                            {
                                Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"You do not have keys for this vehicle", 3000);
                                return;
                            }
                            if (Core.VehicleStreaming.GetEngineState(vehicle))
                            {
                                Core.VehicleStreaming.SetEngineState(vehicle, false);
                                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"You turned off the car engine", 3000);
                            }
                            else
                            {
                                Core.VehicleStreaming.SetEngineState(vehicle, true);
                                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"you started the car", 3000);
                            }
                            break;
                    }
                    if (Core.VehicleStreaming.GetEngineState(vehicle)) Commands.RPChat("me", sender, "started the vehicle");
                    else Commands.RPChat("me", sender, "turned off the vehicle");
                    return;
                #endregion Engine button
                case "lockCarPressed":
                    #region inVehicle
                    if (NAPI.Player.IsPlayerInAnyVehicle(sender) && sender.VehicleSeat == 0)
                    {
                        vehicle = sender.Vehicle;
                        ChangeVehicleDoors(sender, vehicle);
                        return;
                    }
                    #endregion
                    #region outVehicle
                    vehicle = getNearestVehicle(sender, 10);
                    if (vehicle != null)
                        ChangeVehicleDoors(sender, vehicle);
                    #endregion
                    break;
            }
        }
        // ////////////////////////// //

        [ServerEvent(Event.VehicleDeath)]
        public void Event_vehicleDeath(Vehicle vehicle)
        {
            try
            {
                if (!vehicle.HasData("ACCESS") || vehicle.GetData<string>("ACCESS") == "ADMIN") return;
                string access = vehicle.GetData<string>("ACCESS");
                switch (access)
                {
                    case "PERSONAL":
                        {
                            Player owner = vehicle.GetData<Player>("OWNER");
                            string number = vehicle.NumberPlate;

                            Notify.Send(owner, NotifyType.Alert, NotifyPosition.BottomCenter, "Your car has been destroyed", 3000);

                            VehicleData vData = Vehicles[number];
                            vData.Items = new List<nItem>();
                            vData.Health = 0;

                            vehicle.Delete();
                        }
                        return;
                    case "WORK":
                        Player player = vehicle.GetData<Player>("DRIVER");
                        if (player != null)
                        {
                            string paymentMsg = (player.GetData<int>("PAYMENT") == 0) ? "" : $"Вы получили зарплату в {player.GetData<int>("PAYMENT")}$";
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Your work vehicle has been destroyed. " + paymentMsg, 3000);
                            player.SetData("ON_WORK", false);
                            Customization.ApplyCharacter(player);
                        }
                        string work = vehicle.GetData<string>("TYPE");
                        switch (work)
                        {
                            case "BUS":
                                Jobs.Bus.respawnBusCar(vehicle);
                                return;
                            case "MOWER":
                                Jobs.Lawnmower.respawnCar(vehicle);
                                return;
                            case "TAXI":
                                Jobs.Lawnmower.respawnCar(vehicle);
                                return;
                            case "COLLECTOR":
                                Jobs.Collector.respawnCar(vehicle);
                                return;
                        }
                        return;
                }
            }
            catch (Exception e) { Log.Write("VehicleDeath: " + e.Message, nLog.Type.Error); }
        }
        private static List<string> whitelist = new List<string>
        {
          "A","B","E","K","M","H","O","P","C","T","Y","X"
        };
        public static string GenerateNumber()
        {
            string number;
            do
            {
                number = "";

                number += whitelist[Rnd.Next(0, whitelist.Count)];
                number += whitelist[Rnd.Next(0, whitelist.Count)];
                number += whitelist[Rnd.Next(0, whitelist.Count)];
                number += Rnd.Next(0, 9);
                number += Rnd.Next(0, 9);
                number += Rnd.Next(0, 9);
                number += whitelist[Rnd.Next(0, whitelist.Count)];
                number += whitelist[Rnd.Next(0, whitelist.Count)];

            } while (Vehicles.ContainsKey(number));
            return number;
        }
        private static List<string> whitelist2 = new List<string>
        {
          " "," "," "," "," "," "," "," "," "," "," "," "
        };

        public static string GenerateNumber2()
        {
            string number;
            do
            {
                number = "";
                number += (char)Rnd.Next(0x0041, 0x005A);
                number += (char)Rnd.Next(0x0030, 0x0039);
                number += (char)Rnd.Next(0x0030, 0x0039);
                number += (char)Rnd.Next(0x0030, 0x0039);
                number += (char)Rnd.Next(0x0041, 0x005A);
                number += (char)Rnd.Next(0x0041, 0x005A);
                number += 2;
                number += 6;

            } while (Vehicles.ContainsKey(number));
            return number;
        }

        internal class VehicleData
        {
            public string Holder { get; set; }
            public string Model { get; set; }
            public int Health { get; set; }
            public int Fuel { get; set; }
            public int Price { get; set; }
            public VehicleCustomization Components { get; set; }
            public List<nItem> Items { get; set; }
            public string Position { get; set; }
            public string Rotation { get; set; }
            public int KeyNum { get; set; }
            public float Dirt { get; set; }
            public int Сasco { get; set; }
            public float Sell { get; set; }

            public string GetVehicleDataToJson(string vehnumber = null)
            {
                Dictionary<string, object> data = new Dictionary<string, object>()
                {
                    { "model", this.Model },
                    { "number", vehnumber },
                    { "price", this.Sell },
                    { "fuel", this.Fuel }
                };

                return JsonConvert.SerializeObject(data);
            }
        }

        internal class VehicleCustomization
        {
            public Color PrimColor = new Color(0, 0, 0);
            public Color SecColor = new Color(0, 0, 0);
            public Color NeonColor = new Color(0, 0, 0, 0);

            public int PrimModColor = -1;
            public int SecModColor = -1;

            public int Muffler = -1;
            public int SideSkirt = -1;
            public int Hood = -1;
            public int Spoiler = -1;
            public int Lattice = -1;
            public int Wings = -1;
            public int Roof = -1;
            public int Vinyls = -1;
            public int FrontBumper = -1;
            public int RearBumper = -1;

            public int Engine = -1;
            public int Turbo = -1;
            public int Horn = -1;
            public int Transmission = -1;
            public int WindowTint = 0;
            public int Suspension = -1;
            public int Brakes = -1;
            public int Headlights = -1;
            //public int HeadlightColor = 0;
            public int NumberPlate = 0;

            public int Wheels = -1;
            public int WheelsType = 0;
            public int WheelsColor = 0;


            public int Armor = -1;
        }

        public static void changeOwner(string oldName, string newName)
        {
            List<string> toChange = new List<string>();
            lock (Vehicles)
            {
                foreach (KeyValuePair<string, VehicleData> vd in Vehicles)
                {
                    if (vd.Value.Holder != oldName) continue;
                    Log.Write($"The car was found! [{vd.Key}]");
                    toChange.Add(vd.Key);
                }
                foreach (string num in toChange)
                {
                    if (Vehicles.ContainsKey(num)) Vehicles[num].Holder = newName;
                }
                // // //
                MySQL.Query($"UPDATE `vehicles` SET `holder`='{newName}' WHERE `holder`='{oldName}'");
            }
        }
    }

    class VehicleInventory : Script
    {
        public static void Add(Vehicle vehicle, nItem item)
        {
            if (!vehicle.HasData("ITEMS")) return;
            List<nItem> items = vehicle.GetData<List<nItem>>("ITEMS");

            if (nInventory.ClothesItems.Contains(item.Type) || nInventory.WeaponsItems.Contains(item.Type)
                || nInventory.MeleeWeaponsItems.Contains(item.Type) || item.Type == ItemType.CarKey || item.Type == ItemType.KeyRing)
            {
                items.Add(item);
            }
            else
            {
                int count = item.Count;
                for (int i = 0; i < items.Count; i++)
                {
                    if (i >= items.Count) break;
                    if (items[i].Type == item.Type && items[i].Count < nInventory.ItemsStacks[item.Type])
                    {
                        int temp = nInventory.ItemsStacks[item.Type] - items[i].Count;
                        if (count < temp) temp = count;
                        items[i].Count += temp;
                        count -= temp;
                    }
                }

                while (count > 0)
                {
                    if (count >= nInventory.ItemsStacks[item.Type])
                    {
                        items.Add(new nItem(item.Type, nInventory.ItemsStacks[item.Type], item.Data));
                        count -= nInventory.ItemsStacks[item.Type];
                    }
                    else
                    {
                        items.Add(new nItem(item.Type, count, item.Data));
                        count = 0;
                    }
                }
            }

            vehicle.SetData("ITEMS", items);

            if (vehicle.GetData<string>("ACCESS") == "PERSONAL" || vehicle.GetData<string>("ACCESS") == "GARAGE")
                VehicleManager.Vehicles[vehicle.NumberPlate].Items = items;

            foreach (Player p in Main.Players.Keys.ToList())
            {
                if (p == null || !Main.Players.ContainsKey(p)) continue;
                if (p.HasData("OPENOUT_TYPE") && p.GetData<int>("OPENOUT_TYPE") == 2 && p.HasData("SELECTEDVEH") && p.GetData<Vehicle>("SELECTEDVEH") == vehicle) GUI.Dashboard.OpenOut(p, vehicle.GetData<List<nItem>>("ITEMS"), "Багажник", 2);
            }
        }

        public static Dictionary<string, int> WeightCapacity = new Dictionary<string, int>
            {
            {"18rs7", 70 },
            {"63gls", 150 },
            {"2019m5", 30 },
            {"sto21", 30 },
            {"m760i", 30 },
            {"gtam21", 30 },
            {"senna", 30 },
            {"f812c21", 30},
            {"ikx3mso21",30},
            {"agerars", 20 },
            {"amggt16", 20 },
            {"astondb11", 20 },
            {"bentayga", 100 },
            {"bentaygast", 100 },
            {"bmw730", 30 },
            {"bmwm7", 30 },
            {"bmwx7", 60 },
            {"cls63s", 30 },
            {"cyber", 50 },
            {"e63s", 40 },
            {"g63", 180 },
            {"g63amg6x6", 150 },
            {"gle63", 120 },
            {"gt63s", 40 },
            {"huracan", 20 },
            {"i8", 30 },
            {"jesko20", 20 },
            {"kiastinger", 40 },
            {"lc200", 130 },
            {"lex570", 130 },
            {"m5comp", 30 },
            {"modelx", 65 },
            {"mp1", 20 },
            {"rs6", 30 },
            {"s63cab", 20 },
            {"urus", 150 },
            {"x6m", 130 },
            {"bmwe28", 30 },
            {"bmwe34", 30 },
            {"bmwe36", 30 },
            {"bmwe38", 30 },
            {"bmwe39", 30 },
            {"bmwe46", 30 },
            {"bmwe70", 30 },
            {"bmwg05", 50 },
            {"bmwg07", 60 },
            {"bmwm2", 30 },
            {"bmwz4", 30 },
            {"camry70", 35 },
            {"a45", 40 },
            {"a90", 20 },
            {"350z", 20 },
            {"m4comp", 40 },
            {"mark2", 20 },
            {"rx7", 30 },
            {"s600", 30 },
            {"skyline", 20 },
            {"x5me70", 50 },
            {"x5g05", 60 },
            {"w210", 30 },
            {"tahoe2", 140 },
            {"vclass", 210 },

            {"chiron19", 30 },
            {"cullinan", 140 },
            {"msprinter", 250 },
            {"starone", 30 },
            {"eqg", 80 },
            {"bentley20", 90 },
            {"taycan21", 30 },
            {"x6m2", 70 },
            {"panamera17turbo", 40 },
            {"rs72", 40 },
            {"ghost", 40 },
            };
        // { МОДЕЛЬ МАШИНЫ, ВЕС МАКСИМАЛЬНЫЙ }

        public static int GetVehicleWeight(Vehicle vehicle)
        {
            try
            {
                if (vehicle == null || !VehicleManager.Vehicles.ContainsKey(vehicle.NumberPlate) || !VehicleInventory.WeightCapacity.ContainsKey(VehicleManager.Vehicles[vehicle.NumberPlate].Model))
                    return 100;
                return VehicleInventory.WeightCapacity[VehicleManager.Vehicles[vehicle.NumberPlate].Model];
            }
            catch { return 100; }
        }

        public static int TryAdd(Vehicle vehicle, nItem item)
        {
            if (!vehicle.HasData("ITEMS")) return -1;
            List<nItem> items = vehicle.GetData<List<nItem>>("ITEMS");

            if (nInventory.IsFullWeight(items, GetVehicleWeight(vehicle), item))
                return -1;

            int tail = 0;
            if (nInventory.ClothesItems.Contains(item.Type) || nInventory.WeaponsItems.Contains(item.Type) || nInventory.MeleeWeaponsItems.Contains(item.Type) ||
                item.Type == ItemType.CarKey || item.Type == ItemType.KeyRing || item.Type == ItemType.Number)
            {
                if (items.Count >= 25) return -1;
            }
            else
            {
                int count = 0;
                foreach (nItem i in items)
                    if (i.Type == item.Type) count += nInventory.ItemsStacks[i.Type] - i.Count;

                int slots = 30;
                int maxCapacity = (slots - items.Count) * nInventory.ItemsStacks[item.Type] + count;
                if (item.Count > maxCapacity) tail = item.Count - maxCapacity;
            }
            return tail;
        }

        public static int GetCountOfType(Vehicle vehicle, ItemType type)
        {
            if (!vehicle.HasData("ITEMS")) return 0;
            List<nItem> items = vehicle.GetData<List<nItem>>("ITEMS");
            int count = 0;

            for (int i = 0; i < items.Count; i++)
            {
                if (i >= items.Count) break;
                if (items[i].Type == type) count += items[i].Count;
            }

            return count;
        }

        public static void Remove(Vehicle vehicle, ItemType type, int amount)
        {
            if (!vehicle.HasData("ITEMS")) return;
            List<nItem> items = vehicle.GetData<List<nItem>>("ITEMS");

            for (int i = items.Count - 1; i >= 0; i--)
            {
                if (i >= items.Count) continue;
                if (items[i].Type != type) continue;
                if (items[i].Count <= amount)
                {
                    amount -= items[i].Count;
                    items.RemoveAt(i);
                }
                else
                {
                    items[i].Count -= amount;
                    amount = 0;
                    break;
                }
            }

            if (vehicle.GetData<string>("ACCESS") == "PERSONAL" || vehicle.GetData<string>("ACCESS") == "GARAGE")
                VehicleManager.Vehicles[vehicle.NumberPlate].Items = items;

            foreach (Player p in Main.Players.Keys.ToList())
            {
                if (p == null || !Main.Players.ContainsKey(p)) continue;
                if (p.HasData("OPENOUT_TYPE") && p.GetData<int>("OPENOUT_TYPE") == 2 && p.HasData("SELECTEDVEH") && p.GetData<Vehicle>("SELECTEDVEH") == vehicle) GUI.Dashboard.OpenOut(p, vehicle.GetData<List<nItem>>("ITEMS"), "Багажник", 2);
            }
        }

        public static void Remove(Vehicle vehicle, nItem item)
        {
            if (!vehicle.HasData("ITEMS")) return;
            List<nItem> items = vehicle.GetData<List<nItem>>("ITEMS");

            if (nInventory.ClothesItems.Contains(item.Type) || nInventory.WeaponsItems.Contains(item.Type) || nInventory.MeleeWeaponsItems.Contains(item.Type) ||
                item.Type == ItemType.BagWithDrill || item.Type == ItemType.BagWithMoney || item.Type == ItemType.CarKey || item.Type == ItemType.KeyRing)
            {
                items.Remove(item);
            }
            else
            {
                for (int i = items.Count - 1; i >= 0; i--)
                {
                    if (i >= items.Count) continue;
                    if (items[i].Type != item.Type) continue;
                    if (items[i].Count <= item.Count)
                    {
                        item.Count -= items[i].Count;
                        items.RemoveAt(i);
                    }
                    else
                    {
                        items[i].Count -= item.Count;
                        item.Count = 0;
                        break;
                    }
                }
            }

            if (vehicle.GetData<string>("ACCESS") == "PERSONAL" || vehicle.GetData<string>("ACCESS") == "GARAGE")
                VehicleManager.Vehicles[vehicle.NumberPlate].Items = items;

            foreach (Player p in Main.Players.Keys.ToList())
            {
                if (p == null || !Main.Players.ContainsKey(p)) continue;
                if (p.HasData("OPENOUT_TYPE") && p.GetData<int>("OPENOUT_TYPE") == 2 && p.HasData("SELECTEDVEH") && p.GetData<Vehicle>("SELECTEDVEH") == vehicle) GUI.Dashboard.OpenOut(p, vehicle.GetData<List<nItem>>("ITEMS"), "Багажник", 2);
            }
        }
    }
}
