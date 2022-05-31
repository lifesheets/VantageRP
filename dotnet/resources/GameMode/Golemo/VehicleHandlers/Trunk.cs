using System;
using System.Collections.Generic;
using System.Text;
using GolemoSDK;
using GTANetworkAPI;
using Golemo.Core;

namespace Golemo.VehicleHandlers
{
    public class Trunk : Script
    {
        static int StreamDistance = 500; // Указать вашу дистанцию стрима [ conf.json => stream-distance ]

        [ServerEvent(Event.PlayerDeath)]
        static void PlayerDeath(Player player, Player entityKiller, uint weapon)
        {
            try
            {
                Vehicle vehicle = Trunk.Get(player);
                if (vehicle == null) return;
                Trunk.Exit(vehicle, player);
            }
            catch { }
        }


        [RemoteEvent("trunk.exit")]
        static void RM_exit(Player player)
        {
            try
            {
                Vehicle vehicle = Trunk.Get(player);
                if (vehicle == null) return;
                Trunk.Exit(vehicle, player);
            }
            catch { }
        }

        /// <summary>
        /// Посадить игрока в багажник с проверками
        /// </summary>
        /// <param name="vehicle">Транспорт</param>
        /// <param name="player">Игрок</param>
        public static void Enter(Vehicle vehicle, Player player)
        {
            try
            {
                if (player.HasData("PLY::TRUNK")) return;

                if (player.IsInVehicle)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Невозможно залезть в багажник, пока вы находитесь в транспорте", 3000);
                    return;
                }
                if (vehicle.Class == 21 || vehicle.Class == 5 || vehicle.Class == 1 || vehicle.Class == 10 || vehicle.Class == 8 || vehicle.Class == 14)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Невозможно залезть в багажник", 3000);
                    return;
                }
                if (VehicleStreaming.GetDoorState(vehicle, DoorID.DoorTrunk) == DoorState.DoorClosed)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Багажник закрыт", 3000);
                    return;
                }
                if (Trunk.Get(vehicle) != null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "В багажнике кто-то уже находится!", 3000);
                    return;
                }

                Trunk.Set(vehicle, player);
            }
            catch { }
        }

        /// <summary>
        /// Вытащить игрока из багажника с проверками
        /// </summary>
        /// <param name="vehicle">Транспорт</param>
        /// <param name="player">Игрок</param>
        public static void Exit(Vehicle vehicle, Player player)
        {
            try
            {
                if (!player.HasData("PLY::TRUNK") || player.GetData<Vehicle>("PLY::TRUNK") != vehicle) return;

                if (player.HasData("CUFFED") && player.GetData<bool>("CUFFED"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Ваши руки связаны, или в наручниках!", 3000);
                    return;
                }
                if (VehicleStreaming.GetDoorState(vehicle, DoorID.DoorTrunk) == DoorState.DoorClosed)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Багажник закрыт", 3000);
                    return;
                }

                Trunk.Reset(vehicle);
            }
            catch { }
        }

        /// <summary>
        /// Посадить в багажник игрока
        /// </summary>
        /// <param name="vehicle">Транспорт</param>
        /// <param name="player">Игрок</param>
        public static void Set(Vehicle vehicle, Player player)
        {
            try
            {
                if (Trunk.Get(vehicle) != null) return;

                player.SetData("PLY::TRUNK", vehicle);
                vehicle.SetData("VEH::TRUNK", player);
                vehicle.SetSharedData("VEH::TRUNK", player.Value);

                Trigger.ClientEventInRange(vehicle.Position, StreamDistance, "trunk.enter", vehicle, player);
            }
            catch { }
        }

        /// <summary>
        /// Убрать игрока если в транспорте кто-то есть
        /// </summary>
        /// <param name="vehicle">Транспорт</param>
        public static void Reset(Vehicle vehicle)
        {
            try
            {
                Player player = Trunk.Get(vehicle);
                if (player == null) return;

                player.ResetData("PLY::TRUNK");

                vehicle.ResetData("VEH::TRUNK");
                vehicle.SetSharedData("VEH::TRUNK", "NONE");
                vehicle.ResetSharedData("VEH::TRUNK");

                Trigger.ClientEventInRange(vehicle.Position, StreamDistance, "trunk.exit", player);
            }
            catch { }
        }

        /// <summary>
        /// Получить игрока, который находится в багажнике
        /// </summary>
        /// <returns>Player - Если в багажнике кто-то есть иначе null</returns>
        public static Player Get(Vehicle vehicle)
        {
            try
            {
                return vehicle.HasData("VEH::TRUNK") ? vehicle.GetData<Player>("VEH::TRUNK") : null;
            }
            catch { return null; }
        }

        /// <summary>
        /// Получить транспорт, в котором он находится в багажнике
        /// </summary>
        /// <returns>Vehicle - Если он в нём находится иначе null</returns>
        public static Vehicle Get(Player player)
        {
            try
            {
                return player.HasData("PLY::TRUNK") ? player.GetData<Vehicle>("PLY::TRUNK") : null;
            }
            catch { return null; }
        } 

    }
}
