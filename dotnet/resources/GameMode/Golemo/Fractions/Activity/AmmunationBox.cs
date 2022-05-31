using GolemoSDK;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Golemo.Fractions.Activity
{
    class AmmunationBox : Script
    {

        #region Инициализация
        private static nLog RLog = new nLog("AmmunationBox");
        private static Random rnd = new Random();
        //
        private static GTANetworkAPI.TextLabel _processPercentText;
        private static GTANetworkAPI.Marker _zoneMarker;
        private static GTANetworkAPI.Blip _boxBlip;
        private static GTANetworkAPI.Object _ammunationBox;
        //
        public static bool _isLoading = false;
        public static bool _isStart = false;
        private static int _rndAmmoType;
        private static int _rndcords;
        private static int _percentLoading = 0;
        //
        private static List<Vector3> SpawnPositions = new List<Vector3>() 
        {
             new Vector3(-1112.0697, 2693.9429, 17.43415),
             new Vector3(1699.3103, 3755.459, 33.58531),
             new Vector3(-661.3205, -942.5471, 20.709215),
             new Vector3(244.80771, -48.50754, 68.821),
             new Vector3(-1313.1881, -393.4646, 35.575775),
             new Vector3(-324.57916, 6078.9346, 30.334764)
        };
        private static List<string> AmmoType = new List<string>()
        {
            "",
            "",
            "",
            "",
        };
        private static List<nItem> PistolAmmo = new List<nItem>()
        {
            new nItem(ItemType.Revolver, 5, "123456789"),
            new nItem(ItemType.MarksmanPistol, 3, "123456789"),
            new nItem(ItemType.Pistol50, 3, "123456789"),
            new nItem(ItemType.MG, 3, "123456789"),
            new nItem(ItemType.CarbineRifle, 3, "123456789"),
            new nItem(ItemType.SniperRifle, 1, "123456789"),
        };
        private static List<nItem> RandomAmmo = new List<nItem>()
        {
            new nItem(ItemType.Musket, 5, "123456789"),
            new nItem(ItemType.BullpupRifle, 3, "123456789"),
            new nItem(ItemType.PumpShotgun, 3, "123456789"),
        };
        private static List<nItem> AssaultAmmo = new List<nItem>()
        {
            new nItem(ItemType.CarbineRifle, 5, "123456789"),
            new nItem(ItemType.CompactRifle, 3, "123456789"),
            new nItem(ItemType.SpecialCarbine, 3, "123456789"),
        };
        private static List<nItem> BullapAmmo = new List<nItem>()
        {
           new nItem(ItemType.Revolver, 5, "123456789"),
            new nItem(ItemType.MarksmanPistol, 3, "123456789"),
            new nItem(ItemType.Pistol50, 3, "123456789"),
            new nItem(ItemType.MG, 3, "123456789"),
            new nItem(ItemType.CarbineRifle, 3, "123456789"),
            new nItem(ItemType.SniperRifle, 1, "123456789"),
        };

        private static List<List<nItem>> _ammunationBoxLoot = new List<List<nItem>>()
        {
            PistolAmmo, RandomAmmo, AssaultAmmo, BullapAmmo
        };
        //
        #endregion

        #region Methods
        [ServerEvent(Event.ResourceStart)]
        public static void onResourceStart()
        {
            for (int i = 0; i < SpawnPositions.Count; i++)
            {
                ColShape _ShapeBox = NAPI.ColShape.CreateCylinderColShape(SpawnPositions[i], 8, 5, 0);
                _ShapeBox.OnEntityEnterColShape += (s, e) =>
                {
                    if (_isStart)
                    {
                        if (!_isLoading)
                        {
                            if (e.VehicleSeat == 0)
                            {
                                if (e.Vehicle.Model != 1176811760)
{
                                    if (!e.Vehicle.HasData("CANMATS"))
                                    {
                                        Notify.Error(e, $"На этой машине нельзя перевозить оружие.", 3000);
                                        return;
                                    }
                                }
                                StartLoadingAmmunation(e);
                                e.SetData("isLootingAmmoBox", true);
                            }
                        }
                        else
                        {
                            Notify.Error(e, $"Кто-то уже загружает оружие, сначала нейтралезуйте его.", 5000);
                        }
                    }
                };
                _ShapeBox.OnEntityExitColShape += (s, e) =>
                {
                    if (e.HasData("isLootingAmmoBox"))
                    {
                        if (e.VehicleSeat == 0)
                        {
                            _isLoading = false;
                            _percentLoading = 0;
                            UpdateLabel();
                            Timers.Stop($"LoadingBox{e.Value}");
                            Notify.Alert(e, "Вы выехали из зоны погрузка. Загрузка боеприпасов отменена");
                        }
                    }

                };
            }
            RLog.Write("AmmunationBox Loaded", nLog.Type.Info);
        }
        public static void UpdateLabel()
        {
            string text = $"~w~{_percentLoading * 1} %";
            _processPercentText.Text = Main.StringToU16(text);
        }
        public static void SpawnAnAmmoBox()
        {
            _rndcords = rnd.Next(0, SpawnPositions.Count);
            _rndAmmoType = rnd.Next(0, AmmoType.Count);
            _zoneMarker = NAPI.Marker.CreateMarker(1, SpawnPositions[_rndcords] - new Vector3(0, 0, 1), new Vector3(), new Vector3(), 16f, new Color(255, 255, 255, 150), false, 0);
            _ammunationBox = NAPI.Object.CreateObject(NAPI.Util.GetHashKey("p_secret_weapon_02"), SpawnPositions[_rndcords], new Vector3(0, 0, 0), 255, 0);
            _processPercentText = NAPI.TextLabel.CreateTextLabel(Main.StringToU16($"~w~{_percentLoading * 1} %"), SpawnPositions[_rndcords] + new Vector3(0, 0, 1.7), 8f, 4f, 4, new Color(0, 0, 255), false, 0);
            _boxBlip = NAPI.Blip.CreateBlip(556, SpawnPositions[_rndcords], 1.1f, 1, Main.StringToU16("Ограбление"), 255, 1, true, 0, 0);
            NAPI.Notification.SendNotificationToAll($"Кто то начал ограбление аммунации", false);
            _isStart = true;
        }
        public static void LoadAmmunitionIntoCar(Player player)
        {
            var vehicle = player.Vehicle;
            for (int i = 0; i < _ammunationBoxLoot[_rndAmmoType].Count; i++)
            {
                for (int j = 0; j < _ammunationBoxLoot[_rndAmmoType][i].Count; j++)
                {
                    Core.VehicleInventory.Add(vehicle, new nItem(_ammunationBoxLoot[_rndAmmoType][i].Type, 1, _ammunationBoxLoot[_rndAmmoType][i].Data));
                }
            }
            Notify.Succ(player, "Машина загружена, поторопись отвезти оружие на склад");
            DestroyBox();

        }
        public static void DestroyBox()
        {
            NAPI.Task.Run(() =>
            {
                _ammunationBox.Delete();
                _processPercentText.Delete();
                _zoneMarker.Delete();
                _boxBlip.Delete();
            });
            _isStart = false;
        }
        public static void StartLoadingAmmunation(Player player)
        {
            Notify.Info(player, "Вы начали загрузку оружия, не выходите с машины!", 6000);
            _isLoading = true;
            NAPI.Task.Run(() =>
            {
                Timers.Start($"LoadingBox{player.Value}", 1000, () =>
                {
                    ++_percentLoading;
                    UpdateLabel();
                    if (_percentLoading >= 150) // кол-во процентов 
                    {
                        _percentLoading = 0;
                        _isLoading = false;
                        player.ResetData("isLootingAmmoBox");
                        Timers.Stop($"LoadingBox{player.Value}");
                        LoadAmmunitionIntoCar(player);
                    }
                });
            });
        }
        #endregion

        #region Events
        [ServerEvent(Event.PlayerDisconnected)]
        public void OnPlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            if (player.HasData("isLootingAmmoBox"))
            {
                Timers.Stop($"LoadingBox{player.Value}");
                _percentLoading = 0;
                _isLoading = false;
                player.ResetData("isLootingAmmoBox");
            }
        }

        [ServerEvent(Event.PlayerDeath)]
        public void OnPlayerDeath(Player player, Player killer, uint reason)
        {
            if (player.HasData("isLootingAmmoBox"))
            {
                Timers.Stop($"LoadingBox{player.Value}");
                _percentLoading = 0;
                _isLoading = false;
                player.ResetData("isLootingAmmoBox");
            }
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void onPlayerExitVehicleHandler(Player player, Vehicle vehicle)
        {
            try
            {
                if (player.HasData("isLootingAmmoBox"))
                {
                    Timers.Stop($"LoadingBox{player.Value}");
                    _percentLoading = 0;
                    _isLoading = false;
                    player.ResetData("isLootingAmmoBox");
                }
            }
            catch (Exception e) { RLog.Write("PlayerExitVehicle: " + e.Message, nLog.Type.Error); }
        }
        #endregion

        [Command("spawnammobox")]
        public static void CMD_SpawnBox(Player player)
        {
            try
            {
                if (!Core.Group.CanUseCmd(player, "spawnammobox")) return;
                if (_isStart || _isLoading)
                {
                    Notify.Error(player, "Ящик с боеприпасами уже заспавнен");
                    return;
                }
                SpawnAnAmmoBox();
            }
            catch (Exception e) { RLog.Write("AmmoBox: " + e.Message, nLog.Type.Error); }
        }
    }
}
