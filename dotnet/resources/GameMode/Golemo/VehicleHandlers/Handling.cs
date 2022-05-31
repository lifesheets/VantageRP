/*using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using Newtonsoft.Json;
using GolemoSDK;
using Golemo.Core;

namespace Golemo.VehicleHandlers
{
    public class Handling
    {
        // Изначальные параметры транспорта, если тут нет транспорта, то невозможно его редактировать
        public static Dictionary<string, Handling> Default = new Dictionary<string, Handling>
        {
            { "Elegy", new Handling(1, 0, 0, 0, 0, 0) },
            { "g63", new Handling(1, 0, 0, 0, 0, 0) },
            { "cullinan", new Handling(1, 0, 0, 0, 0, 0) },
            { "a80", new Handling(1, 0, 0, 0, 0, 0) },
            { "a90", new Handling(1, 0, 0, 0, 0, 0) },
            { "a45", new Handling(1, 0, 0, 0, 0, 0) },
            { "w140", new Handling(1, 0, 0, 0, 0, 0) },
            { "mustang19", new Handling(1, 0, 0, 0, 0, 0) },
            { "2019m5", new Handling(1, 0, 0, 0, 0, 0) },
            { "mbbs20", new Handling(1, 0, 0, 0, 0, 0) }
        };

        static int StreamDistance = 500; // Указать вашу дистанцию стрима [ conf.json => stream-distance ]

        public float Break { get; set; }
        public float Wheels { get; set; }
        public float Steering { get; set; }
        public float Skid { get; set; }
        public float Height { get; set; }
        public float Rotate { get; set; }

        public Handling(float _break, float _wheels, float _steering, float _skid, float _height, float _rotate)
        {
            Break = _break; Wheels = _wheels; Steering = _steering; Skid = _skid; Height = _height; Rotate = _rotate;
        }


        public void Apply(Vehicle vehicle)
        {
            try
            {
                vehicle.SetSharedData("HANDLING", JsonConvert.SerializeObject(this));
                Trigger.ClientEventInRange(vehicle.Position, StreamDistance, "handling.set", vehicle, JsonConvert.SerializeObject(this));
            }
            catch { }
        }

        public void Open(Player player, string model)
        {
            try
            {
                Trigger.ClientEvent(player, "handling.open", JsonConvert.SerializeObject(this), JsonConvert.SerializeObject(Default[model]));
            }
            catch { }
        }

        public class Eventer : Script
        {
            [RemoteEvent("handling.send")]
            static void SendHandling(Player player, float _break, float _wheels, float _steering, float _skid, float _height, float _rotate)
            {
                try
                {
                    if (!player.IsInVehicle || !player.Vehicle.HasData("ACCESS") || player.Vehicle.GetData<string>("ACCESS") != "PERSONAL")
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы должны находиться в личном транспорте", 3000);
                        return;
                    }

                    VehicleManager.VehicleData data = VehicleManager.Vehicles[player.Vehicle.NumberPlate];

                    if (data == null) return;

                    if (data.Holder != player.Name)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы не являетесь владельцем этого транспорта", 3000);
                        return;
                    }
                    if (!Handling.Default.ContainsKey(data.Model))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Транспорт не поддерживает настройку", 3000);
                        return;
                    }


                    handling.Break = _break;
                    handling.Wheels = _wheels;
                    handling.Steering = _steering;
                    handling.Skid = _skid;
                    handling.Height = _height;
                    handling.Rotate = _rotate;

                    handling.Apply(player.Vehicle);
                }
                catch { }
            }

            [RemoteEvent("handling.open")]
            static void OpenHandling(Player player)
            {
                try
                {
                    if (!player.IsInVehicle || !player.Vehicle.HasData("ACCESS") || player.Vehicle.GetData<string>("ACCESS") != "PERSONAL")
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы должны находиться в личном транспорте", 3000);
                        return;
                    }

                    VehicleManager.VehicleData data = VehicleManager.Vehicles[player.Vehicle.NumberPlate];

                    if (data == null)
                        return;

                    if (data.Holder != player.Name)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы не являетесь владельцем этого транспорта", 3000);
                        return;
                    }
                    if (!Handling.Default.ContainsKey(data.Model))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Транспорт не поддерживает настройку", 3000);
                        return;
                    }

                    data.Handling.Open(player, data.Model);
                }
                catch { }
            }

        }

    }
}*/
