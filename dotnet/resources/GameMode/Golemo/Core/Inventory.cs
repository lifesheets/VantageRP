using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Linq;
using System.Threading;
using GTANetworkAPI;
using Newtonsoft.Json;
using GolemoSDK;
using Golemo;
using Golemo.Core.Character;
using Golemo.Houses;
using Golemo.Fractions.Activity;

namespace Golemo.Core
{
    class nInventory : Script
    {
        public static Dictionary<int, string> ItemsNames = new Dictionary<int, string>
        {
            {-1, "Маска" },
            {-3, "Перчатки" },
            {-4, "Штаны"},
            {-5, "Рюкзак"},
            {-6, "Обувь"},
            {-7, "Аксессуар"},
            {-8, "Нижняя одежда"},
            {-9, "Бронежилет"},
            {-10, "Украшения"},
            {-11, "Верхняя одежда" },
            {-12, "Головной убор" },
            {-13, "Очки" },
            {-14, "Аксессуар" },
            {1, "Аптечка"},
            {2, "Канистра бензина"},
            {3, "Чипсы"},
            {4, "Пиво"},
            {5, "Пицца"},
            {6, "Бургер"},
            {7, "Хот-Дог"},
            {8, "Сэндвич"},
            {9, "eCola"},
            {10, "Sprunk"},
            {11, "Отмычка для замков"},
            {12, "Сумка с деньгами"},
            {13, "Материалы"},
            {14, "Наркотики"},
            {15, "Сумка с дрелью"},
            {16, "Военная отмычка"},
            {17, "Мешок"},
            {18, "Стяжки"},
            {19, "Ключи от машины"},
            {40, "Подарок"},
            {41, "Связка ключей"},
            {42, "Рем. Комплект"},

            {20, "«На корке лимона»"},
            {21, "«На бруснике»"},
            {22, "«Русский стандарт»"},
            {23, "«Asahi»"},
            {24, "«Midori»"},
            {25, "«Yamazaki»"},
            {26, "«Martini Asti»"},
            {27, "«Sambuca»"},
            {28, "«Campari»"},
            {29, "«Дживан»"},
            {30, "«Арарат»"},
            {31, "«Noyan Tapan»"},

            {100, "Taurus PT92" },
            {101, "HK p2000" },
            {102, "Desert Eagle" },
            {103, "HK P7M10" },
            {104, "EWB 1911" },
            {105, "FN Model 1922" },
            {106, "Contender G2" },
            {107, "Taurus Raging Bull" },
            {108, "Colt SCAMP" },
            {109, "Stinger S-200" },
            {110, "Orion Flare Gun" },
            {111, "Colt New Service" },
            {112, "Glock 18" },
            {113, "AMT Backup" },
            {114, "Taurus Raging Bull MK2" },

            {115, "Mini UZI" },
            {116, "Tec-9" },
            {117, "MP5A3" },
            {118, "P-90" },
            {119, "SIG MPX-SD" },
            {120, "«Печенег»" },
            {121, "MK-48" },
            {122, "Thompson M1918A1" },
            {123, "Scorpion vz.61" },
            {124, "MP5K" },
            {125, "MG42" },

            {126, "Type 56-2" },
            {127, "HK-416" },
            {128, "Tavor CTar-21" },
            {129, "G36C" },
            {130, "QBZ-97" },
            {131, "Draco Pistol" },
            {132, "АК-103" },
            {133, "AR-15" },
            {134, "G36KV" },
            {135, "QBZ-95" },

            {136, "L115A3" },
            {137, "Barrett M82A1" },
            {138, "M14 EBR" },
            {139, "Barrett XM109" },
            {140, "SOCOM 16" },

            {141, "Mossberg 590 A1" },
            {142, "Mossberg 500" },
            {143, "KSG 12" },
            {144, "UTS-15" },
            {145, "Land Pattern Musket" },
            {146, "Сайга-12К" },
            {147, "Обрез" },
            {148, "Protecta" },
            {149, "Benelli M3" },

            {180, "Нож" },
            {181, "Дубинка" },
            {182, "Молоток" },
            {183, "Бита" },
            {184, "Монтировка" },
            {185, "Гольф клюшка" },
            {186, "Бутылка" },
            {187, "Кинжал" },
            {188, "Топор" },
            {189, "Кастет" },
            {190, "Мачете" },
            {191, "Фонарик" },
            {192, "Швейцарский нож" },
            {193, "Кий" },
            {194, "Гаечный ключ" },
            {195, "Боевой топор" },

            {200, "Патроны 45ACP" },
            {201, "Патроны 9x19mm" },
            {2201, "Патроны 357 Magnum 9x32" },
            {202, "Патроны 62x39mm" },
            {203, "Патроны 12ga Rifles" },
            {204, "Патроны 50BMG" },
            {244, "Патроны 12ga Buckshots" },
            {245, "Патроны 56x45mm" },

            {205, "Удочка" },
            {206, "Улучшенная удочка" },
            {207, "Удочка MK2" },
            {208, "Наживка" },
            {209, "Корюшка" },
            {210, "Кунджа" },
            {211, "Лосось" },
            {212, "Окунь" },
            {213, "Осётр" },
            {214, "Скат" },
            {215, "Тунец" },
            {216, "Угорь" },
            {217, "Чёрный амур" },
            {218, "Щука" },

            //Farmer Job Items
            {219, "Урожай" },
            {220, "Семена" },

            {199, "GiveBox" },


            //Merumond items
            {227, "Бинокль" },
            {500, "LSPD Дрон" },
            {501, "Дрон" },
            {224, "Фишки казино" },
            {562, "Номерной знак" },
            {563, "Документы" },
            {564, "Лицензии" },
            {565, "Рация" },
            {566, "Таблетка" },
            {567, "Использованная таблетка" },
            {568, "Розовое дилдо" },
            {571, "Черное дилдо" },
            {572, "Красное дилдо" },
            {569, "Аптека" },
            {570, "Бинты" },
            {573, "Бонг" },
            {574, "Зажигалка" },
            {575, "Разноцветный бонг" },
            {576, "Программатор" },

            {1000, "Гриб" },
            {1001, "Гриб" },
            {1002, "Гриб" },
            {1003, "Гриб" },
            {1004, "Гриб" },
            {1005, "Гриб" },
            {1006, "Золотой Гриб" },

            {580, "Сноуборд #1" },
            {581, "Сноуборд #2" },
            {582, "Сноуборд #3" },
            {583, "Сноуборд #4" },
            {584, "Сноуборд #5" },
            {585, "Сноуборд #6" },
            {586, "Сноуборд #7" },
            {587, "Сноуборд #8" },
            {588, "Парашют" },

            {603, "Леденец" },
            {604, "Пончик" },
            {605, "Коктель" },
            {606, "Милк шейк" },
            {607, "Салат цезарь" },
            {608, "Шашлык" },

            {258, "Снежок" },
            {43, "Балончик" },
            {609, "Награбленные деньги" },
            {610, "Бумбокс" },
        };
        public static Dictionary<ItemType, float> ItemsWeight = new Dictionary<ItemType, float>
            {
                { ItemType.BagWithDrill, 3f },
                { ItemType.BagWithMoney, 3f },
                { ItemType.Hat, 0.2f },
                { ItemType.Mask, 0.2f },
                { ItemType.Gloves, 0.2f },
                { ItemType.Leg, 0.2f },
                { ItemType.Bag, 0.2f },
                { ItemType.Feet, 0.2f },
                { ItemType.Jewelry, 0.2f },
                { ItemType.Undershit, 0.2f },
                { ItemType.BodyArmor, 0.2f },
                { ItemType.Decals, 0.2f },
                { ItemType.Top, 0.2f },
                { ItemType.Glasses, 0.2f },
                { ItemType.Accessories, 0.2f },
                { ItemType.CasinoChips, 0.00001f },
              //  { ItemType.Backs, 0.2f },

                { ItemType.Drugs, 0.1f },
                { ItemType.Material, 0.05f },
                { ItemType.Debug, 0000000 },
                { ItemType.HealthKit, 0.1f },
               // { ItemType.Apteka, 0.1f },
              //  { ItemType.Bint, 0.1f },
                { ItemType.RepairKit, 0.5f },
                { ItemType.GasCan, 0.5f },
                { ItemType.Сrisps, 0.5f },
                { ItemType.Beer, 0.5f },
                { ItemType.Pizza, 0.5f },
                { ItemType.Burger, 0.5f },
                { ItemType.HotDog, 0.5f },
                { ItemType.Sandwich, 0.5f },
                { ItemType.eCola, 0.5f },
                { ItemType.Sprunk, 0.5f },
                { ItemType.Lockpick, 0.5f },
                { ItemType.ArmyLockpick, 0.5f },
                { ItemType.Pocket, 0.5f },
                { ItemType.Cuffs, 0.5f },
                { ItemType.CarKey, 0.5f },
                { ItemType.Present, 0.5f },
                { ItemType.KeyRing, 0.5f },
                { ItemType.Balon, 0.05f },

                { ItemType.RusDrink1, 0.5f },
                { ItemType.RusDrink2, 0.5f },
                { ItemType.RusDrink3, 0.5f },
                { ItemType.YakDrink1, 0.5f },
                { ItemType.YakDrink2, 0.5f },
                { ItemType.YakDrink3, 0.5f },
                { ItemType.LcnDrink1, 0.5f },
                { ItemType.LcnDrink2, 0.5f },
                { ItemType.LcnDrink3, 0.5f },
                { ItemType.ArmDrink1, 0.5f },
                { ItemType.ArmDrink2, 0.5f },
                { ItemType.ArmDrink3, 0.5f },

              //  { ItemType.Phone, 0.5f },

                { ItemType.Pistol, 1.5f },
                { ItemType.CombatPistol, 1.5f },
                { ItemType.Pistol50, 1.5f },
                { ItemType.SNSPistol, 1.5f },
                { ItemType.HeavyPistol, 1.5f },
                { ItemType.VintagePistol, 1.5f },
                { ItemType.MarksmanPistol, 1.5f },
                { ItemType.Revolver, 1.5f },
                { ItemType.APPistol, 1.5f },
                { ItemType.StunGun, 1.5f },
                { ItemType.FlareGun, 1.5f },
                { ItemType.DoubleAction, 1.5f },
                { ItemType.PistolMk2, 1.5f },
                { ItemType.SNSPistolMk2, 1.5f },
                { ItemType.RevolverMk2, 1.5f },

                { ItemType.MicroSMG, 1.5f },
                { ItemType.MachinePistol, 1.5f },
                { ItemType.SMG, 1.5f },
                { ItemType.AssaultSMG, 1.5f },
                { ItemType.CombatPDW, 1.5f },
                { ItemType.Programmer, 1f },
                { ItemType.MG, 1.5f },
                { ItemType.CombatMG, 1.5f },
                { ItemType.Gusenberg, 1.5f },
                { ItemType.MiniSMG, 1.5f },
                { ItemType.SMGMk2, 1.5f },
                { ItemType.CombatMGMk2, 1.5f },

                { ItemType.AssaultRifle, 1.5f },
                { ItemType.CarbineRifle, 1.5f },
                { ItemType.AdvancedRifle, 1.5f },
                { ItemType.SpecialCarbine, 1.5f },
                { ItemType.BullpupRifle, 1.5f },
                { ItemType.CompactRifle, 1.5f },
                { ItemType.AssaultRifleMk2, 1.5f },
                { ItemType.CarbineRifleMk2, 1.5f },
                { ItemType.SpecialCarbineMk2, 1.5f },
                { ItemType.BullpupRifleMk2, 1.5f },

                { ItemType.SniperRifle, 1.5f },
                { ItemType.HeavySniper, 1.5f },
                { ItemType.MarksmanRifle, 1.5f },
                { ItemType.HeavySniperMk2, 1.5f },
                { ItemType.MarksmanRifleMk2, 1.5f },

                { ItemType.PumpShotgun, 1.5f },
                { ItemType.SawnOffShotgun, 1.5f },
                { ItemType.BullpupShotgun, 1.5f },
                { ItemType.AssaultShotgun, 1.5f },
                { ItemType.Musket, 1.5f },
                { ItemType.HeavyShotgun, 1.5f },
                { ItemType.DoubleBarrelShotgun, 1.5f },
                { ItemType.SweeperShotgun, 1.5f },
                { ItemType.PumpShotgunMk2, 1.5f },

                { ItemType.Knife, 1f },
                { ItemType.Nightstick, 1f },
                { ItemType.Hammer, 1f },
                { ItemType.Bat, 1f },
                { ItemType.Crowbar, 1f },
                { ItemType.GolfClub, 1f },
                { ItemType.Bottle, 1f },
                { ItemType.Dagger, 1f },
                { ItemType.Hatchet, 1f },
                { ItemType.KnuckleDuster, 1f },
                { ItemType.Machete, 1f },
                { ItemType.Flashlight, 1f },
                { ItemType.SwitchBlade, 1f },
                { ItemType.PoolCue, 1f },
                { ItemType.Wrench, 1f },
                { ItemType.BattleAxe, 1f },

                { ItemType.PistolAmmo, 0.005f },
                { ItemType.PistolAmmo2, 0.005f },
                { ItemType.PistolAmmo3, 0.005f },
                { ItemType.RiflesAmmo, 0.005f },
                { ItemType.HeavyRiflesAmmo, 0.005f },
                { ItemType.SniperAmmo, 0.005f },
                { ItemType.ShotgunsAmmo, 0.005f },
                { ItemType.CarabineAmmo, 0.005f },

                /* Fishing */
                { ItemType.Rod, 0.75f },
                { ItemType.RodUpgrade, 0.75f },
                { ItemType.RodMK2, 0.75f },
                { ItemType.Naz, 0.005f },
                { ItemType.Koroska, 0.25f },
                { ItemType.Kyndja, 0.25f },
                { ItemType.Lococ, 0.25f },
                { ItemType.Okyn, 0.25f },
                { ItemType.Ocetr, 0.25f },
                { ItemType.Skat, 0.25f },
                { ItemType.Tunec, 0.25f },
                { ItemType.Ygol, 0.25f },
                { ItemType.Amyr, 0.25f },
               // { ItemType.Orange, 0.5f },
                { ItemType.Chyka, 0.25f },

               // { ItemType.Listokcocs, 0.05f },
               // { ItemType.Redled, 0.05f },
               // { ItemType.Cocs, 0.05f },
               // { ItemType.Geroin, 0.05f },

                //Farmer Job Items
                { ItemType.Hay, 0.02f },
                { ItemType.Seed, 0.02f },

                { ItemType.LSPDDrone, 1f },
                { ItemType.GiveBox, 0.02f },
                { ItemType.Number, 0.02f },

                { ItemType.Documents, 0.5f },
                { ItemType.License, 0.5f },

                { ItemType.Walkie, 0.5f },
                { ItemType.Pill, 0.05f },
                { ItemType.UsePill, 0.05f },
                { ItemType.Binocular, 1f },
                { ItemType.Dildo, 0.8f },
                { ItemType.DildoBl, 0.8f },
                { ItemType.DildoRed, 0.8f },
                { ItemType.Apteka, 0.1f },
                { ItemType.Bint, 0.1f },
                { ItemType.Bong, 0.8f },
                { ItemType.RGBBong, 0.8f },
                { ItemType.Lighter, 0.2f },

                { ItemType.Mush1, 0.05f },
                { ItemType.Mush2, 0.05f },
                { ItemType.Mush3, 0.05f },
                { ItemType.Mush4, 0.05f },
                { ItemType.Mush5, 0.05f },
                { ItemType.Mush6, 0.05f },
                { ItemType.MushGold, 0.05f },

                { ItemType.Snowboard1, 1f },
                { ItemType.Snowboard2, 1f },
                { ItemType.Snowboard3, 1f },
                { ItemType.Snowboard4, 1f },
                { ItemType.Snowboard5, 1f },
                { ItemType.Snowboard6, 1f },
                { ItemType.Snowboard7, 1f },
                { ItemType.Snowboard8, 1f },
                { ItemType.Parachute, 0.4f },

                { ItemType.CookieSnow, 0.05f },
                { ItemType.Donut, 0.05f },
                { ItemType.Cocktail, 0.05f },
                { ItemType.Milkshake, 0.05f },
                { ItemType.Salad, 0.05f },
                { ItemType.Shashlik, 0.05f },

                { ItemType.SnowBall, 0.01f },
                { ItemType.MoneyHeist, 0.000000001f },
                { ItemType.Boombox, 1f },
            };
        public static Dictionary<int, string> ItemsDescriptions = new Dictionary<int, string>();
        public static Dictionary<ItemType, uint> ItemModels = new Dictionary<ItemType, uint>()
        {
            { ItemType.Hat, NAPI.Util.GetHashKey("q_box_hat") },
            { ItemType.Mask, NAPI.Util.GetHashKey("q_box_m") },
            { ItemType.Gloves, NAPI.Util.GetHashKey("prop_gloves") },
            { ItemType.Leg, NAPI.Util.GetHashKey("q_box_pt") },
            { ItemType.Bag, 0000000 },
            { ItemType.Feet, NAPI.Util.GetHashKey("q_box_sh") },
            { ItemType.Jewelry, NAPI.Util.GetHashKey("prop_earr") },
            { ItemType.Undershit, NAPI.Util.GetHashKey("q_box_cl") },
            { ItemType.BodyArmor, 701173564 },
            { ItemType.Decals, 0000000 },
            { ItemType.Top, NAPI.Util.GetHashKey("q_box_cur") },
            { ItemType.Glasses, NAPI.Util.GetHashKey("prop_glass") },
            { ItemType.Accessories, NAPI.Util.GetHashKey("prop_acs") },

            { ItemType.Drugs, 4293279169 },
            { ItemType.Material, 3045218749 },
            { ItemType.Debug, 0000000 },
            { ItemType.HealthKit, 678958360 },
            { ItemType.RepairKit, NAPI.Util.GetHashKey("prop_box_ammo07a") },
            { ItemType.GasCan, 786272259 },
            { ItemType.Сrisps, 2564432314 },
            { ItemType.Beer, 1940235411 },
            { ItemType.Pizza, 604847691 },
            { ItemType.Burger, 2240524752 },
            { ItemType.HotDog, 2565741261 },
            { ItemType.Sandwich, 987331897 },
            { ItemType.eCola, 144995201 },
            { ItemType.Sprunk, 2973713592 },
            { ItemType.Lockpick, 977923025 },
            { ItemType.ArmyLockpick, 977923025 },
            { ItemType.Pocket, 3887136870 },
            { ItemType.Cuffs, 3887136870 },
            { ItemType.CarKey, 977923025 },
            { ItemType.Present, NAPI.Util.GetHashKey("prop_box_ammo07a") },
            { ItemType.KeyRing, 977923025 },

            { ItemType.RusDrink1, NAPI.Util.GetHashKey("prop_vodka_bottle") },
            { ItemType.RusDrink2, NAPI.Util.GetHashKey("prop_vodka_bottle") },
            { ItemType.RusDrink3, NAPI.Util.GetHashKey("prop_vodka_bottle") },
            { ItemType.YakDrink1, NAPI.Util.GetHashKey("prop_cs_beer_bot_02") },
            { ItemType.YakDrink2, NAPI.Util.GetHashKey("prop_wine_red") },
            { ItemType.YakDrink3, NAPI.Util.GetHashKey("p_whiskey_bottle_s") },
            { ItemType.LcnDrink1, NAPI.Util.GetHashKey("prop_wine_white") },
            { ItemType.LcnDrink2, NAPI.Util.GetHashKey("prop_vodka_bottle") },
            { ItemType.LcnDrink3, NAPI.Util.GetHashKey("prop_wine_red") },
            { ItemType.ArmDrink1, NAPI.Util.GetHashKey("prop_bottle_cognac") },
            { ItemType.ArmDrink2, NAPI.Util.GetHashKey("prop_bottle_cognac") },
            { ItemType.ArmDrink3, NAPI.Util.GetHashKey("prop_bottle_cognac") },

            { ItemType.Pistol, NAPI.Util.GetHashKey("w_pi_pistol") },
            { ItemType.CombatPistol, NAPI.Util.GetHashKey("w_pi_combatpistol") },
            { ItemType.Pistol50, NAPI.Util.GetHashKey("w_pi_pistol50") },
            { ItemType.SNSPistol, NAPI.Util.GetHashKey("w_pi_sns_pistol") },
            { ItemType.HeavyPistol, NAPI.Util.GetHashKey("w_pi_heavypistol") },
            { ItemType.VintagePistol, NAPI.Util.GetHashKey("w_pi_vintage_pistol") },
            { ItemType.MarksmanPistol, NAPI.Util.GetHashKey("w_pi_singleshot") },
            { ItemType.Revolver, NAPI.Util.GetHashKey("w_pi_revolver") },
            { ItemType.APPistol, NAPI.Util.GetHashKey("w_pi_appistol") },
            { ItemType.StunGun, NAPI.Util.GetHashKey("w_pi_stungun") },
            { ItemType.FlareGun, NAPI.Util.GetHashKey("w_pi_flaregun") },
            { ItemType.DoubleAction, NAPI.Util.GetHashKey("mk2") },
            { ItemType.PistolMk2, NAPI.Util.GetHashKey("w_pi_pistolmk2") },
            { ItemType.SNSPistolMk2, NAPI.Util.GetHashKey("w_pi_sns_pistolmk2") },
            { ItemType.RevolverMk2, NAPI.Util.GetHashKey("w_pi_revolvermk2") },

            { ItemType.MicroSMG, NAPI.Util.GetHashKey("w_sb_microsmg") },
            { ItemType.MachinePistol, NAPI.Util.GetHashKey("w_sb_compactsmg") },
            { ItemType.SMG, NAPI.Util.GetHashKey("w_sb_smg") },
            { ItemType.AssaultSMG, NAPI.Util.GetHashKey("w_sb_assaultsmg") },
            { ItemType.CombatPDW, NAPI.Util.GetHashKey("w_sb_pdw") },
            { ItemType.MG, NAPI.Util.GetHashKey("w_mg_mg") },
            { ItemType.CombatMG, NAPI.Util.GetHashKey("w_mg_combatmg") },
            { ItemType.Gusenberg, NAPI.Util.GetHashKey("w_sb_gusenberg") },
            { ItemType.MiniSMG, NAPI.Util.GetHashKey("w_sb_minismg") },
            { ItemType.SMGMk2, NAPI.Util.GetHashKey("w_sb_smgmk2") },
            { ItemType.CombatMGMk2, NAPI.Util.GetHashKey("w_mg_combatmgmk2") },

            { ItemType.AssaultRifle, NAPI.Util.GetHashKey("w_ar_assaultrifle") },
            { ItemType.CarbineRifle, NAPI.Util.GetHashKey("w_ar_carbinerifle") },
            { ItemType.AdvancedRifle, NAPI.Util.GetHashKey("w_ar_advancedrifle") },
            { ItemType.SpecialCarbine, NAPI.Util.GetHashKey("w_ar_specialcarbine") },
            { ItemType.BullpupRifle, NAPI.Util.GetHashKey("w_ar_bullpuprifle") },
            { ItemType.CompactRifle, NAPI.Util.GetHashKey("w_ar_assaultrifle_smg") },
            { ItemType.AssaultRifleMk2, NAPI.Util.GetHashKey("w_ar_assaultriflemk2") },
            { ItemType.CarbineRifleMk2, NAPI.Util.GetHashKey("w_ar_carbineriflemk2") },
            { ItemType.SpecialCarbineMk2, NAPI.Util.GetHashKey("w_ar_specialcarbinemk2") },
            { ItemType.BullpupRifleMk2, NAPI.Util.GetHashKey("w_ar_bullpupriflemk2") },

            { ItemType.SniperRifle, NAPI.Util.GetHashKey("w_sr_sniperrifle") },
            { ItemType.HeavySniper, NAPI.Util.GetHashKey("w_sr_heavysniper") },
            { ItemType.MarksmanRifle, NAPI.Util.GetHashKey("w_sr_marksmanrifle") },
            { ItemType.HeavySniperMk2, NAPI.Util.GetHashKey("w_sr_heavysnipermk2") },
            { ItemType.MarksmanRifleMk2, NAPI.Util.GetHashKey("w_sr_marksmanriflemk2") },

            { ItemType.PumpShotgun, NAPI.Util.GetHashKey("w_sg_pumpshotgun") },
            { ItemType.SawnOffShotgun, NAPI.Util.GetHashKey("w_sg_sawnoff") },
            { ItemType.BullpupShotgun, NAPI.Util.GetHashKey("w_sg_bullpupshotgun") },
            { ItemType.AssaultShotgun, NAPI.Util.GetHashKey("w_sg_assaultshotgun") },
            { ItemType.Musket, NAPI.Util.GetHashKey("w_ar_musket") },
            { ItemType.HeavyShotgun, NAPI.Util.GetHashKey("w_sg_heavyshotgun") },
            { ItemType.DoubleBarrelShotgun, NAPI.Util.GetHashKey("w_sg_doublebarrel") },
            { ItemType.SweeperShotgun, NAPI.Util.GetHashKey("mk2") },
            { ItemType.PumpShotgunMk2, NAPI.Util.GetHashKey("w_sg_pumpshotgunmk2") },

            { ItemType.Knife, NAPI.Util.GetHashKey("w_me_knife_01") },
            { ItemType.Nightstick, NAPI.Util.GetHashKey("w_me_nightstick") },
            { ItemType.Hammer, NAPI.Util.GetHashKey("w_me_hammer") },
            { ItemType.Bat, NAPI.Util.GetHashKey("w_me_bat") },
            { ItemType.Crowbar, NAPI.Util.GetHashKey("w_me_crowbar") },
            { ItemType.GolfClub, NAPI.Util.GetHashKey("w_me_gclub") },
            { ItemType.Bottle, NAPI.Util.GetHashKey("w_me_bottle") },
            { ItemType.Dagger, NAPI.Util.GetHashKey("w_me_dagger") },
            { ItemType.Hatchet, NAPI.Util.GetHashKey("w_me_hatchet") },
            { ItemType.KnuckleDuster, NAPI.Util.GetHashKey("w_me_knuckle") },
            { ItemType.Machete, NAPI.Util.GetHashKey("prop_ld_w_me_machette") },
            { ItemType.Flashlight, NAPI.Util.GetHashKey("w_me_flashlight") },
            { ItemType.SwitchBlade, NAPI.Util.GetHashKey("w_me_switchblade") },
            { ItemType.PoolCue, NAPI.Util.GetHashKey("prop_pool_cue") },
            { ItemType.Wrench, NAPI.Util.GetHashKey("prop_cs_wrench") },
            { ItemType.BattleAxe, NAPI.Util.GetHashKey("w_me_battleaxe") },

            { ItemType.PistolAmmo, NAPI.Util.GetHashKey("tor_ammo_05_045ACP") },
            { ItemType.PistolAmmo2, NAPI.Util.GetHashKey("tor_ammo_06_9x19mm") },
            { ItemType.PistolAmmo3, NAPI.Util.GetHashKey("tor_ammo_01_357Magnum") },
            { ItemType.RiflesAmmo, NAPI.Util.GetHashKey("tor_ammo_04_7_62x39mm") },
            { ItemType.HeavyRiflesAmmo, NAPI.Util.GetHashKey("tor_ammo_02_12gaRifled") },
            { ItemType.SniperAmmo, NAPI.Util.GetHashKey("tor_ammo_08_50BMG") },
            { ItemType.ShotgunsAmmo, NAPI.Util.GetHashKey("tor_ammo_07_12gaBuckshots") },
            { ItemType.CarabineAmmo, NAPI.Util.GetHashKey("tor_ammo_03_5_56x45mm") },

            /* Fishing */
            { ItemType.Rod, NAPI.Util.GetHashKey("prop_fishing_rod_01") },
            { ItemType.RodUpgrade, NAPI.Util.GetHashKey("prop_fishing_rod_01") },
            { ItemType.RodMK2, NAPI.Util.GetHashKey("prop_fishing_rod_01") },
            { ItemType.Naz, NAPI.Util.GetHashKey("ng_proc_paintcan02a") },
            { ItemType.Koroska, NAPI.Util.GetHashKey("ribka_tor1") },
            { ItemType.Kyndja, NAPI.Util.GetHashKey("ribka_tor1") },
            { ItemType.Lococ, NAPI.Util.GetHashKey("ribka_tor1") },
            { ItemType.Okyn, NAPI.Util.GetHashKey("ribka_tor7") },
            { ItemType.Ocetr, NAPI.Util.GetHashKey("ribka_tor1") },
            { ItemType.Skat, NAPI.Util.GetHashKey("ribka_tor6") },
            { ItemType.Tunec, NAPI.Util.GetHashKey("ribka_tor5") },
            { ItemType.Ygol, NAPI.Util.GetHashKey("ribka_tor1") },
            { ItemType.Amyr, NAPI.Util.GetHashKey("ribka_tor1") },
            { ItemType.Chyka, NAPI.Util.GetHashKey("ribka_tor4") },

            //Farmer Job Items
            { ItemType.Hay, NAPI.Util.GetHashKey("prop_haybale_01") },
            { ItemType.Seed, NAPI.Util.GetHashKey("ch_prop_ch_moneybag_01a") },

            { ItemType.GiveBox,NAPI.Util.GetHashKey("prop_cs_clothes_box") },

            //Merumond Add-on
            { ItemType.Binocular, NAPI.Util.GetHashKey("prop_binoc_01") },
            { ItemType.LSPDDrone, NAPI.Util.GetHashKey("ch_prop_casino_drone_02a") },
            { ItemType.Drone, NAPI.Util.GetHashKey("ch_prop_casino_drone_01a") },
            { ItemType.CasinoChips, 3045218749 },
            { ItemType.Number, NAPI.Util.GetHashKey("p_num_plate_01") },

            { ItemType.Documents, NAPI.Util.GetHashKey("p_ld_id_card_002") },
            { ItemType.License, NAPI.Util.GetHashKey("prop_cs_documents_01") },

            { ItemType.Walkie, NAPI.Util.GetHashKey("prop_cs_walkie_talkie") },
            { ItemType.Pill, NAPI.Util.GetHashKey("prop_cs_pills") },
            { ItemType.UsePill, NAPI.Util.GetHashKey("prop_cs_pills") },
            { ItemType.Dildo, NAPI.Util.GetHashKey("prop_pinkdildo") },
            { ItemType.DildoBl, NAPI.Util.GetHashKey("prop_blackdildo") },
            { ItemType.DildoRed, NAPI.Util.GetHashKey("prop_reddildo") },
            { ItemType.Apteka, 678958360 },
            { ItemType.Bint, NAPI.Util.GetHashKey("bandage_shell") },
            { ItemType.Bong, NAPI.Util.GetHashKey("prop_bong_01") },
            { ItemType.RGBBong, NAPI.Util.GetHashKey("radmir_bong_rgb") },
            { ItemType.Lighter, NAPI.Util.GetHashKey("p_cs_lighter_01") },
            { ItemType.Programmer, NAPI.Util.GetHashKey("prop_cs_server_drive") },

            { ItemType.Mush1,NAPI.Util.GetHashKey("prop_mush1") },
            { ItemType.Mush2,NAPI.Util.GetHashKey("prop_mush2") },
            { ItemType.Mush3,NAPI.Util.GetHashKey("prop_mush3") },
            { ItemType.Mush4,NAPI.Util.GetHashKey("prop_mush4") },
            { ItemType.Mush5,NAPI.Util.GetHashKey("prop_mush5") },
            { ItemType.Mush6,NAPI.Util.GetHashKey("prop_mush6") },
            { ItemType.MushGold,NAPI.Util.GetHashKey("prop_mush_gold") },

            { ItemType.Snowboard1,NAPI.Util.GetHashKey("snowboard1") },
            { ItemType.Snowboard2,NAPI.Util.GetHashKey("snowboard2") },
            { ItemType.Snowboard3,NAPI.Util.GetHashKey("snowboard3") },
            { ItemType.Snowboard4,NAPI.Util.GetHashKey("snowboard4") },
            { ItemType.Snowboard5,NAPI.Util.GetHashKey("snowboard5") },
            { ItemType.Snowboard6,NAPI.Util.GetHashKey("snowboard6") },
            { ItemType.Snowboard7,NAPI.Util.GetHashKey("snowboard7") },
            { ItemType.Snowboard8,NAPI.Util.GetHashKey("snowboard8") },

            { ItemType.Parachute,NAPI.Util.GetHashKey("p_parachute_s") },

            { ItemType.CookieSnow, NAPI.Util.GetHashKey("mj_xm_candy") },
            { ItemType.Donut, NAPI.Util.GetHashKey("prop_amb_donut") },
            { ItemType.Cocktail, NAPI.Util.GetHashKey("prop_cocktail") },
            { ItemType.Milkshake, NAPI.Util.GetHashKey("prop_cocktail") },
            { ItemType.Salad, NAPI.Util.GetHashKey("ng_proc_food_burg01a") },
            { ItemType.Shashlik, NAPI.Util.GetHashKey("prop_cs_steak") },

            { ItemType.SnowBall, NAPI.Util.GetHashKey("w_ex_snowball") },
            { ItemType.Balon, NAPI.Util.GetHashKey("prop_cs_spray_can") },
            { ItemType.MoneyHeist, NAPI.Util.GetHashKey("bkr_prop_moneypack_01a") },
            { ItemType.Boombox, NAPI.Util.GetHashKey("prop_boombox_01") },
        };

        public static Dictionary<ItemType, Vector3> ItemsPosOffset = new Dictionary<ItemType, Vector3>()
        {
            { ItemType.Hat, new Vector3(0, 0, -0.93) },
            { ItemType.Mask, new Vector3(0, 0, -1) },
            { ItemType.Gloves, new Vector3(0, 0, -1) },
            { ItemType.Leg, new Vector3(0, 0, -0.85) },
            { ItemType.Bag, new Vector3() },
            { ItemType.Feet, new Vector3(0, 0, -0.95) },
            { ItemType.Jewelry, new Vector3(0, 0, -0.98) },
            { ItemType.Undershit, new Vector3(0, 0, -0.98) },
            { ItemType.BodyArmor, new Vector3(0, 0, -0.88) },
            { ItemType.Decals, new Vector3() },
            { ItemType.Top, new Vector3(0, 0, -0.96) },
            { ItemType.Glasses, new Vector3(0, 0, -0.98) },
            { ItemType.Accessories, new Vector3(0, 0, -0.98) },

            { ItemType.Drugs, new Vector3(0, 0, -0.95) },
            { ItemType.Material, new Vector3(0, 0, -0.6) },
            { ItemType.Debug, new Vector3() },
            { ItemType.HealthKit, new Vector3(0, 0, -0.9) },
            { ItemType.RepairKit, new Vector3(0, 0, -1) },
            { ItemType.GasCan, new Vector3(0, 0, -1) },
            { ItemType.Сrisps, new Vector3(0, 0, -1) },
            { ItemType.Beer, new Vector3(0, 0, -1) },
            { ItemType.Pizza, new Vector3(0, 0, -1) },
            { ItemType.Burger, new Vector3(0, 0, -0.97) },
            { ItemType.HotDog, new Vector3(0, 0, -0.97) },
            { ItemType.Sandwich, new Vector3(0, 0, -0.99) },
            { ItemType.eCola, new Vector3(0, 0, -1) },
            { ItemType.Sprunk, new Vector3(0, 0, -1) },
            { ItemType.Lockpick, new Vector3(0, 0, -0.98) },
            { ItemType.ArmyLockpick, new Vector3(0, 0, -0.98) },
            { ItemType.Pocket, new Vector3(0, 0, -0.98) },
            { ItemType.Cuffs, new Vector3(0, 0, -0.98) },
            { ItemType.CarKey, new Vector3(0, 0, -0.98) },
            { ItemType.Present, new Vector3(0, 0, -0.98) },
            { ItemType.KeyRing, new Vector3(0, 0, -0.98) },

            { ItemType.RusDrink1, new Vector3(0, 0, -1) },
            { ItemType.RusDrink2, new Vector3(0, 0, -1) },
            { ItemType.RusDrink3, new Vector3(0, 0, -1) },
            { ItemType.YakDrink1, new Vector3(0, 0, -0.87) },
            { ItemType.YakDrink2, new Vector3(0, 0, -1) },
            { ItemType.YakDrink3, new Vector3(0, 0, -0.87) },
            { ItemType.LcnDrink1, new Vector3(0, 0, -1) },
            { ItemType.LcnDrink2, new Vector3(0, 0, -1) },
            { ItemType.LcnDrink3, new Vector3(0, 0, -1) },
            { ItemType.ArmDrink1, new Vector3(0, 0, -1) },
            { ItemType.ArmDrink2, new Vector3(0, 0, -1) },
            { ItemType.ArmDrink3, new Vector3(0, 0, -1) },

            { ItemType.Pistol, new Vector3(0, 0, -0.99) },
            { ItemType.CombatPistol, new Vector3(0, 0, -0.99) },
            { ItemType.Pistol50, new Vector3(0, 0, -0.99) },
            { ItemType.SNSPistol, new Vector3(0, 0, -0.99) },
            { ItemType.HeavyPistol, new Vector3(0, 0, -0.99) },
            { ItemType.VintagePistol, new Vector3(0, 0, -0.99) },
            { ItemType.MarksmanPistol, new Vector3(0, 0, -0.99) },
            { ItemType.Revolver, new Vector3(0, 0, -0.99) },
            { ItemType.APPistol, new Vector3(0, 0, -0.99) },
            { ItemType.StunGun, new Vector3(0, 0, -0.99) },
            { ItemType.FlareGun, new Vector3(0, 0, -0.99) },
            { ItemType.DoubleAction, new Vector3(0, 0, -0.99) },
            { ItemType.PistolMk2, new Vector3(0, 0, -0.99) },
            { ItemType.SNSPistolMk2, new Vector3(0, 0, -0.99) },
            { ItemType.RevolverMk2, new Vector3(0, 0, -0.99) },

            { ItemType.MicroSMG, new Vector3(0, 0, -0.99) },
            { ItemType.MachinePistol, new Vector3(0, 0, -0.99) },
            { ItemType.SMG, new Vector3(0, 0, -0.99) },
            { ItemType.AssaultSMG, new Vector3(0, 0, -0.99) },
            { ItemType.CombatPDW, new Vector3(0, 0, -0.99) },
            { ItemType.MG, new Vector3(0, 0, -0.99) },
            { ItemType.CombatMG, new Vector3(0, 0, -0.99) },
            { ItemType.Gusenberg, new Vector3(0, 0, -0.99) },
            { ItemType.MiniSMG, new Vector3(0, 0, -0.99) },
            { ItemType.SMGMk2, new Vector3(0, 0, -0.99) },
            { ItemType.CombatMGMk2, new Vector3(0, 0, -0.99) },

            { ItemType.AssaultRifle, new Vector3(0, 0, -0.99) },
            { ItemType.CarbineRifle, new Vector3(0, 0, -0.99) },
            { ItemType.AdvancedRifle, new Vector3(0, 0, -0.99) },
            { ItemType.SpecialCarbine, new Vector3(0, 0, -0.99) },
            { ItemType.BullpupRifle, new Vector3(0, 0, -0.99) },
            { ItemType.CompactRifle, new Vector3(0, 0, -0.99) },
            { ItemType.AssaultRifleMk2, new Vector3(0, 0, -0.99) },
            { ItemType.CarbineRifleMk2, new Vector3(0, 0, -0.99) },
            { ItemType.SpecialCarbineMk2, new Vector3(0, 0, -0.99) },
            { ItemType.BullpupRifleMk2, new Vector3(0, 0, -0.99) },

            { ItemType.SniperRifle, new Vector3(0, 0, -0.99) },
            { ItemType.HeavySniper, new Vector3(0, 0, -0.99) },
            { ItemType.MarksmanRifle, new Vector3(0, 0, -0.99) },
            { ItemType.HeavySniperMk2, new Vector3(0, 0, -0.99) },
            { ItemType.MarksmanRifleMk2, new Vector3(0, 0, -0.99) },

            { ItemType.PumpShotgun, new Vector3(0, 0, -0.99) },
            { ItemType.SawnOffShotgun, new Vector3(0, 0, -0.99) },
            { ItemType.BullpupShotgun, new Vector3(0, 0, -0.99) },
            { ItemType.AssaultShotgun, new Vector3(0, 0, -0.99) },
            { ItemType.Musket, new Vector3(0, 0, -0.99) },
            { ItemType.HeavyShotgun, new Vector3(0, 0, -0.99) },
            { ItemType.DoubleBarrelShotgun, new Vector3(0, 0, -0.99) },
            { ItemType.SweeperShotgun, new Vector3(0, 0, -0.99) },
            { ItemType.PumpShotgunMk2, new Vector3(0, 0, -0.99) },

            { ItemType.Knife, new Vector3(0, 0, -0.99) },
            { ItemType.Nightstick, new Vector3(0, 0, -0.99) },
            { ItemType.Hammer, new Vector3(0, 0, -0.99) },
            { ItemType.Bat, new Vector3(0, 0, -0.99) },
            { ItemType.Crowbar, new Vector3(0, 0, -0.99) },
            { ItemType.GolfClub, new Vector3(0, 0, -0.99) },
            { ItemType.Bottle, new Vector3(0, 0, -0.99) },
            { ItemType.Dagger, new Vector3(0, 0, -0.99) },
            { ItemType.Hatchet, new Vector3(0, 0, -0.99) },
            { ItemType.KnuckleDuster, new Vector3(0, 0, -0.99) },
            { ItemType.Machete, new Vector3(0, 0, -0.99) },
            { ItemType.Flashlight, new Vector3(0, 0, -0.99) },
            { ItemType.SwitchBlade, new Vector3(0, 0, -0.99) },
            { ItemType.PoolCue, new Vector3(0, 0, -0.99) },
            { ItemType.Wrench, new Vector3(0, 0, -0.985) },
            { ItemType.BattleAxe, new Vector3(0, 0, -0.99) },

            { ItemType.PistolAmmo, new Vector3(0, 0, -1) },
            { ItemType.PistolAmmo2, new Vector3(0, 0, -1) },
            { ItemType.PistolAmmo3, new Vector3(0, 0, -1) },
            { ItemType.RiflesAmmo, new Vector3(0, 0, -1) },
            { ItemType.HeavyRiflesAmmo, new Vector3(0, 0, -1) },
            { ItemType.SniperAmmo, new Vector3(0, 0, -1) },
            { ItemType.ShotgunsAmmo, new Vector3(0, 0, -1) },
            { ItemType.CarabineAmmo, new Vector3(0, 0, -1) },

            /* Fishing */
            { ItemType.Rod, new Vector3(0, 0, -0.99) },
            { ItemType.RodUpgrade, new Vector3(0, 0, -0.99) },
            { ItemType.RodMK2, new Vector3(0, 0, -0.99) },
            { ItemType.Naz, new Vector3(0, 0, -0.99) },
            { ItemType.Koroska, new Vector3(0, 0, -0.99) },
            { ItemType.Kyndja, new Vector3(0, 0, -0.99) },
            { ItemType.Lococ, new Vector3(0, 0, -0.99) },
            { ItemType.Okyn, new Vector3(0, 0, -0.99) },
            { ItemType.Ocetr, new Vector3(0, 0, -0.99) },
            { ItemType.Skat, new Vector3(0, 0, -0.99) },
            { ItemType.Tunec, new Vector3(0, 0, -0.99) },
            { ItemType.Ygol, new Vector3(0, 0, -0.99) },
            { ItemType.Amyr, new Vector3(0, 0, -0.99) },
            { ItemType.Chyka, new Vector3(0, 0, -0.99) },

            //Farmer Job Items
            { ItemType.Hay, new Vector3(0, 0, -0.99) },
            { ItemType.Seed, new Vector3(0, 0, -0.99) },

            { ItemType.GiveBox, new Vector3(0, 0, 0) },


            //Merumond Add-on
            { ItemType.LSPDDrone, new Vector3(0, 0, 0) },
            { ItemType.Drone, new Vector3(0, 0, 0) },
            { ItemType.CasinoChips, new Vector3(0, 0, -0.96) },
            { ItemType.Number, new Vector3(0, 0, -0.99) },
            { ItemType.Documents, new Vector3(0, 0, -0.99) },
            { ItemType.License, new Vector3(0, 0, -0.99) },

            { ItemType.Walkie, new Vector3(0, 0, -0.95) },
            { ItemType.Binocular, new Vector3(0, 0, -0.99) },
            { ItemType.Pill, new Vector3(0, 0, -0.99) },
            { ItemType.UsePill, new Vector3(0, 0, -0.99) },
            { ItemType.Dildo, new Vector3(0, 0, -0.99) },
            { ItemType.DildoBl, new Vector3(0, 0, -0.99) },
            { ItemType.DildoRed, new Vector3(0, 0, -0.99) },
            { ItemType.Apteka, new Vector3(0, 0, -0.9) },
            { ItemType.Bint, new Vector3(0, 0, -0.9) },
            { ItemType.Bong, new Vector3(0, 0, -0.99) },
            { ItemType.RGBBong, new Vector3(0, 0, -0.99) },
            { ItemType.Lighter, new Vector3(0, 0, -0.99) },
            { ItemType.Programmer, new Vector3(0, 0, -0.96) },

            { ItemType.Mush1, new Vector3(0, 0, -0.99) },
            { ItemType.Mush2, new Vector3(0, 0, -0.99) },
            { ItemType.Mush3, new Vector3(0, 0, -0.99) },
            { ItemType.Mush4, new Vector3(0, 0, -0.99) },
            { ItemType.Mush5, new Vector3(0, 0, -0.99) },
            { ItemType.Mush6, new Vector3(0, 0, -0.99) },
            { ItemType.MushGold, new Vector3(0, 0, -0.99) },

            { ItemType.Snowboard1, new Vector3(0, 0, -0.99) },
            { ItemType.Snowboard2, new Vector3(0, 0, -0.99) },
            { ItemType.Snowboard3, new Vector3(0, 0, -0.99) },
            { ItemType.Snowboard4, new Vector3(0, 0, -0.99) },
            { ItemType.Snowboard5, new Vector3(0, 0, -0.99) },
            { ItemType.Snowboard6, new Vector3(0, 0, -0.99) },
            { ItemType.Snowboard7, new Vector3(0, 0, -0.99) },
            { ItemType.Snowboard8, new Vector3(0, 0, -0.99) },

            { ItemType.Parachute, new Vector3(0, 0, -0.99) },

            { ItemType.CookieSnow, new Vector3(0, 0, -0.99) },
            { ItemType.Donut, new Vector3(0, 0, -0.99) },
            { ItemType.Cocktail, new Vector3(0, 0, -0.99) },
            { ItemType.Milkshake, new Vector3(0, 0, -0.99) },
            { ItemType.Salad, new Vector3(0, 0, -0.99) },
            { ItemType.Shashlik, new Vector3(0, 0, -0.99) },

            { ItemType.SnowBall, new Vector3(0, 0, -0.9) },
            { ItemType.MoneyHeist, new Vector3(0, 0, -0.9) },
        };
        public static Dictionary<ItemType, Vector3> ItemsRotOffset = new Dictionary<ItemType, Vector3>()
        {
            { ItemType.Hat, new Vector3() },
            { ItemType.Mask, new Vector3() },
            { ItemType.Gloves, new Vector3(90, 0, 0) },
            { ItemType.Leg, new Vector3() },
            { ItemType.Bag, new Vector3() },
            { ItemType.Feet, new Vector3() },
            { ItemType.Jewelry, new Vector3() },
            { ItemType.Undershit, new Vector3() },
            { ItemType.BodyArmor, new Vector3(90, 90, 0) },
            { ItemType.Decals, new Vector3() },
            { ItemType.Top, new Vector3() },
            { ItemType.Glasses, new Vector3() },
            { ItemType.Accessories, new Vector3() },

            { ItemType.Drugs, new Vector3() },
            { ItemType.Material, new Vector3() },
            { ItemType.Debug, new Vector3() },
            { ItemType.HealthKit, new Vector3() },
            { ItemType.RepairKit, new Vector3() },
            { ItemType.GasCan, new Vector3() },
            { ItemType.Сrisps, new Vector3(90, 90, 0) },
            { ItemType.Beer, new Vector3() },
            { ItemType.Pizza, new Vector3() },
            { ItemType.Burger, new Vector3() },
            { ItemType.HotDog, new Vector3() },
            { ItemType.Sandwich, new Vector3() },
            { ItemType.eCola, new Vector3() },
            { ItemType.Sprunk, new Vector3() },
            { ItemType.Lockpick, new Vector3() },
            { ItemType.ArmyLockpick, new Vector3() },
            { ItemType.Pocket, new Vector3() },
            { ItemType.Cuffs, new Vector3() },
            { ItemType.CarKey, new Vector3() },
            { ItemType.Present, new Vector3() },
            { ItemType.KeyRing, new Vector3() },

            { ItemType.RusDrink1, new Vector3() },
            { ItemType.RusDrink2, new Vector3() },
            { ItemType.RusDrink3, new Vector3() },
            { ItemType.YakDrink1, new Vector3() },
            { ItemType.YakDrink2, new Vector3() },
            { ItemType.YakDrink3, new Vector3() },
            { ItemType.LcnDrink1, new Vector3() },
            { ItemType.LcnDrink2, new Vector3() },
            { ItemType.LcnDrink3, new Vector3() },
            { ItemType.ArmDrink1, new Vector3() },
            { ItemType.ArmDrink2, new Vector3() },
            { ItemType.ArmDrink3, new Vector3() },

            { ItemType.Pistol, new Vector3(90, 0, 0) },
            { ItemType.CombatPistol, new Vector3(90, 0, 0) },
            { ItemType.Pistol50, new Vector3(90, 0, 0) },
            { ItemType.SNSPistol, new Vector3(90, 0, 0) },
            { ItemType.HeavyPistol, new Vector3(90, 0, 0) },
            { ItemType.VintagePistol, new Vector3(90, 0, 0) },
            { ItemType.MarksmanPistol, new Vector3(90, 0, 0) },
            { ItemType.Revolver, new Vector3(90, 0, 0) },
            { ItemType.APPistol, new Vector3(90, 0, 0) },
            { ItemType.StunGun, new Vector3(90, 0, 0) },
            { ItemType.FlareGun, new Vector3(90, 0, 0) },
            { ItemType.DoubleAction, new Vector3(90, 0, 0) },
            { ItemType.PistolMk2, new Vector3(90, 0, 0) },
            { ItemType.SNSPistolMk2, new Vector3(90, 0, 0) },
            { ItemType.RevolverMk2, new Vector3(90, 0, 0) },

            { ItemType.MicroSMG, new Vector3(90, 0, 0) },
            { ItemType.MachinePistol, new Vector3(90, 0, 0) },
            { ItemType.SMG, new Vector3(90, 0, 0) },
            { ItemType.AssaultSMG, new Vector3(90, 0, 0) },
            { ItemType.CombatPDW, new Vector3(90, 0, 0) },
            { ItemType.MG, new Vector3(90, 0, 0) },
            { ItemType.CombatMG, new Vector3(90, 0, 0) },
            { ItemType.Gusenberg, new Vector3(90, 0, 0) },
            { ItemType.MiniSMG, new Vector3(90, 0, 0) },
            { ItemType.SMGMk2, new Vector3(90, 0, 0) },
            { ItemType.CombatMGMk2, new Vector3(90, 0, 0) },

            { ItemType.AssaultRifle, new Vector3(90, 0, 0) },
            { ItemType.CarbineRifle, new Vector3(90, 0, 0) },
            { ItemType.AdvancedRifle, new Vector3(90, 0, 0) },
            { ItemType.SpecialCarbine, new Vector3(90, 0, 0) },
            { ItemType.BullpupRifle, new Vector3(90, 0, 0) },
            { ItemType.CompactRifle, new Vector3(90, 0, 0) },
            { ItemType.AssaultRifleMk2, new Vector3(90, 0, 0) },
            { ItemType.CarbineRifleMk2, new Vector3(90, 0, 0) },
            { ItemType.SpecialCarbineMk2, new Vector3(90, 0, 0) },
            { ItemType.BullpupRifleMk2, new Vector3(90, 0, 0) },

            { ItemType.SniperRifle, new Vector3(90, 0, 0) },
            { ItemType.HeavySniper, new Vector3(90, 0, 0) },
            { ItemType.MarksmanRifle, new Vector3(90, 0, 0) },
            { ItemType.HeavySniperMk2, new Vector3(90, 0, 0) },
            { ItemType.MarksmanRifleMk2, new Vector3(90, 0, 0) },

            { ItemType.PumpShotgun, new Vector3(90, 0, 0) },
            { ItemType.SawnOffShotgun, new Vector3(90, 0, 0) },
            { ItemType.BullpupShotgun, new Vector3(90, 0, 0) },
            { ItemType.AssaultShotgun, new Vector3(90, 0, 0) },
            { ItemType.Musket, new Vector3(90, 0, 0) },
            { ItemType.HeavyShotgun, new Vector3(90, 0, 0) },
            { ItemType.DoubleBarrelShotgun, new Vector3(90, 0, 0) },
            { ItemType.SweeperShotgun, new Vector3(90, 0, 0) },
            { ItemType.PumpShotgunMk2, new Vector3(90, 0, 0) },

            { ItemType.Knife, new Vector3(90, 0, 0) },
            { ItemType.Nightstick, new Vector3(90, 0, 0) },
            { ItemType.Hammer, new Vector3(90, 0, 0) },
            { ItemType.Bat, new Vector3(90, 0, 0) },
            { ItemType.Crowbar, new Vector3(90, 0, 0) },
            { ItemType.GolfClub, new Vector3(90, 0, 0) },
            { ItemType.Bottle, new Vector3(90, 0, 0) },
            { ItemType.Dagger, new Vector3(90, 0, 0) },
            { ItemType.Hatchet, new Vector3(90, 0, 0) },
            { ItemType.KnuckleDuster, new Vector3(90, 0, 0) },
            { ItemType.Machete, new Vector3(90, 0, 0) },
            { ItemType.Flashlight, new Vector3(90, 0, 0) },
            { ItemType.SwitchBlade, new Vector3(90, 0, 0) },
            { ItemType.PoolCue, new Vector3(90, 0, 0) },
            { ItemType.Wrench, new Vector3(-12, 0, 0) },
            { ItemType.BattleAxe, new Vector3(90, 0, 0) },

            { ItemType.PistolAmmo, new Vector3(0, 0, 0) },
            { ItemType.PistolAmmo2, new Vector3(0, 0, 0) },
            { ItemType.PistolAmmo3, new Vector3(0, 0, 0) },
            { ItemType.RiflesAmmo, new Vector3(0, 0, 0) },
            { ItemType.HeavyRiflesAmmo, new Vector3(0, 0, 0) },
            { ItemType.SniperAmmo, new Vector3(0, 0, 0) },
            { ItemType.ShotgunsAmmo, new Vector3(0, 0, 0) },
            { ItemType.CarabineAmmo, new Vector3(0, 0, 0) },

            /* Fishing */
            { ItemType.Rod, new Vector3(90, 0, 0) },
            { ItemType.RodUpgrade, new Vector3(90, 0, 0) },
            { ItemType.RodMK2, new Vector3(90, 0, 0) },
            { ItemType.Naz, new Vector3(90, 0, 0) },
            { ItemType.Koroska, new Vector3(90, 0, 0) },
            { ItemType.Kyndja, new Vector3(90, 0, 0) },
            { ItemType.Lococ, new Vector3(90, 0, 0) },
            { ItemType.Okyn, new Vector3(90, 0, 0) },
            { ItemType.Ocetr, new Vector3(90, 0, 0) },
            { ItemType.Skat, new Vector3(90, 0, 0) },
            { ItemType.Tunec, new Vector3(90, 0, 0) },
            { ItemType.Ygol, new Vector3(90, 0, 0) },
            { ItemType.Amyr, new Vector3(90, 0, 0) },
            { ItemType.Chyka, new Vector3(90, 0, 0) },

            //Farmer Job Items
            { ItemType.Hay, new Vector3(0, 0, 0) },
            { ItemType.Seed, new Vector3(0, 0, 0) },

            { ItemType.GiveBox, new Vector3(0, 0, 0) },

            //merumond add-on
            { ItemType.LSPDDrone, new Vector3(0, 0, 0) },
            { ItemType.Drone, new Vector3(0, 0, 0) },
            { ItemType.CasinoChips,  new Vector3() },
            { ItemType.Number, new Vector3() },
            { ItemType.Documents, new Vector3() },
            { ItemType.License, new Vector3() },

            { ItemType.Walkie, new Vector3(90, 0, 0) },
            { ItemType.Pill, new Vector3(0, 0, 0) },
            { ItemType.UsePill, new Vector3(0, 0, 0) },
            { ItemType.Binocular, new Vector3(0,0,0) },
            { ItemType.Dildo, new Vector3(90,0,0) },
            { ItemType.DildoBl, new Vector3(90,0,0) },
            { ItemType.DildoRed, new Vector3(90,0,0) },
            { ItemType.Apteka, new Vector3() },
            { ItemType.Bint, new Vector3() },
            { ItemType.Bong, new Vector3(90, 0, 0) },
            { ItemType.RGBBong, new Vector3(90, 0, 0) },
            { ItemType.Lighter, new Vector3(90, 0, 0) },
            { ItemType.Programmer, new Vector3() },

            { ItemType.Mush1, new Vector3(0, 0, 0) },
            { ItemType.Mush2, new Vector3(0, 0, 0) },
            { ItemType.Mush3, new Vector3(0, 0, 0) },
            { ItemType.Mush4, new Vector3(0, 0, 0) },
            { ItemType.Mush5, new Vector3(0, 0, 0) },
            { ItemType.Mush6, new Vector3(0, 0, 0) },
            { ItemType.MushGold, new Vector3(0, 0, 0) },

            { ItemType.Snowboard1, new Vector3(0, 0, 0) },
            { ItemType.Snowboard2, new Vector3(0, 0, 0) },
            { ItemType.Snowboard3, new Vector3(0, 0, 0) },
            { ItemType.Snowboard4, new Vector3(0, 0, 0) },
            { ItemType.Snowboard5, new Vector3(0, 0, 0) },
            { ItemType.Snowboard6, new Vector3(0, 0, 0) },
            { ItemType.Snowboard7, new Vector3(0, 0, 0) },
            { ItemType.Snowboard8, new Vector3(0, 0, 0) },

            { ItemType.Parachute, new Vector3(0, 0, 0) },

            { ItemType.CookieSnow, new Vector3(0, 0, 0) },
            { ItemType.Donut, new Vector3(0, 0, 0) },
            { ItemType.Cocktail, new Vector3(0, 0, 0) },
            { ItemType.Milkshake, new Vector3(0, 0, 0) },
            { ItemType.Salad, new Vector3(0, 0, 0) },
            { ItemType.Shashlik, new Vector3(0, 0, 0) },

            { ItemType.SnowBall, new Vector3(0, 0, 0) },
            { ItemType.MoneyHeist, new Vector3(0, 0, 0) },

        };

        public static Dictionary<ItemType, int> ItemsStacks = new Dictionary<ItemType, int>()
        {
            { ItemType.BagWithMoney, 1 },
            { ItemType.Material, 300 },
            { ItemType.Drugs, 50 },
            { ItemType.BagWithDrill, 1 },
            { ItemType.Debug, 10000 },
            { ItemType.HealthKit, 5 },
            { ItemType.RepairKit, 5 },
            { ItemType.GasCan, 2 },
            { ItemType.Сrisps, 4 },
            { ItemType.Beer, 5 },
            { ItemType.Pizza, 3 },
            { ItemType.Burger, 4 },
            { ItemType.HotDog, 5 },
            { ItemType.Sandwich, 7 },
            { ItemType.eCola, 5 },
            { ItemType.Sprunk, 5 },
            { ItemType.Lockpick, 10 },
            { ItemType.ArmyLockpick, 10 },
            { ItemType.Pocket, 5 },
            { ItemType.Cuffs, 5 },
            { ItemType.CarKey, 1 },
            { ItemType.Present, 1 },
            { ItemType.KeyRing, 1 },

            { ItemType.Mask, 1 },
            { ItemType.Gloves, 1 },
            { ItemType.Leg, 1 },
            { ItemType.Bag, 1 },
            { ItemType.Feet, 1 },
            { ItemType.Jewelry, 1 },
            { ItemType.Undershit, 1 },
            { ItemType.BodyArmor, 1 },
            { ItemType.Decals, 1 },
            { ItemType.Top, 1 },
            { ItemType.Hat, 1 },
            { ItemType.Glasses, 1 },
            { ItemType.Accessories, 1 },

            { ItemType.RusDrink1, 5 },
            { ItemType.RusDrink2, 5 },
            { ItemType.RusDrink3, 5 },

            { ItemType.YakDrink1, 5 },
            { ItemType.YakDrink2, 5 },
            { ItemType.YakDrink3, 5 },

            { ItemType.LcnDrink1, 5 },
            { ItemType.LcnDrink2, 5 },
            { ItemType.LcnDrink3, 5 },

            { ItemType.ArmDrink1, 5 },
            { ItemType.ArmDrink2, 5 },
            { ItemType.ArmDrink3, 5 },

            { ItemType.Pistol, 1 },
            { ItemType.CombatPistol, 1 },
            { ItemType.Pistol50, 1 },
            { ItemType.SNSPistol, 1 },
            { ItemType.HeavyPistol, 1 },
            { ItemType.VintagePistol, 1 },
            { ItemType.MarksmanPistol, 1 },
            { ItemType.Revolver, 1 },
            { ItemType.APPistol, 1 },
            { ItemType.StunGun, 1 },
            { ItemType.FlareGun, 1 },
            { ItemType.DoubleAction, 1 },
            { ItemType.PistolMk2, 1 },
            { ItemType.SNSPistolMk2, 1 },
            { ItemType.RevolverMk2, 1 },

            { ItemType.MicroSMG, 1 },
            { ItemType.MachinePistol, 1 },
            { ItemType.SMG, 1 },
            { ItemType.AssaultSMG, 1 },
            { ItemType.CombatPDW, 1 },
            { ItemType.MG, 1 },
            { ItemType.CombatMG, 1 },
            { ItemType.Gusenberg, 1 },
            { ItemType.MiniSMG, 1 },
            { ItemType.SMGMk2, 1 },
            { ItemType.CombatMGMk2, 1 },

            { ItemType.AssaultRifle, 1 },
            { ItemType.CarbineRifle, 1 },
            { ItemType.AdvancedRifle, 1 },
            { ItemType.SpecialCarbine, 1 },
            { ItemType.BullpupRifle, 1 },
            { ItemType.CompactRifle, 1 },
            { ItemType.AssaultRifleMk2, 1 },
            { ItemType.CarbineRifleMk2, 1 },
            { ItemType.SpecialCarbineMk2, 1 },
            { ItemType.BullpupRifleMk2, 1 },

            { ItemType.SniperRifle, 1 },
            { ItemType.HeavySniper, 1 },
            { ItemType.MarksmanRifle, 1 },
            { ItemType.HeavySniperMk2, 1 },
            { ItemType.MarksmanRifleMk2, 1 },

            { ItemType.PumpShotgun, 1 },
            { ItemType.SawnOffShotgun, 1 },
            { ItemType.BullpupShotgun, 1 },
            { ItemType.AssaultShotgun, 1 },
            { ItemType.Musket, 1 },
            { ItemType.HeavyShotgun, 1 },
            { ItemType.DoubleBarrelShotgun, 1 },
            { ItemType.SweeperShotgun, 1 },
            { ItemType.PumpShotgunMk2, 1 },

            { ItemType.Knife, 1 },
            { ItemType.Nightstick, 1 },
            { ItemType.Hammer, 1 },
            { ItemType.Bat, 1 },
            { ItemType.Crowbar, 1 },
            { ItemType.GolfClub, 1 },
            { ItemType.Bottle, 1 },
            { ItemType.Dagger, 1 },
            { ItemType.Hatchet, 1 },
            { ItemType.KnuckleDuster, 1 },
            { ItemType.Machete, 1 },
            { ItemType.Flashlight, 1 },
            { ItemType.SwitchBlade, 1 },
            { ItemType.PoolCue, 1 },
            { ItemType.Wrench, 1 },
            { ItemType.BattleAxe, 1 },

            { ItemType.PistolAmmo, 350 },
            { ItemType.PistolAmmo2, 350 },
            { ItemType.PistolAmmo3, 350 },
            { ItemType.RiflesAmmo, 200 },
            { ItemType.HeavyRiflesAmmo, 200 },
            { ItemType.SniperAmmo, 16 },
            { ItemType.ShotgunsAmmo, 100 },
            { ItemType.CarabineAmmo, 200 },

            /* Fishing */
            { ItemType.Rod, 1 },
            { ItemType.RodUpgrade, 1 },
            { ItemType.RodMK2, 1 },
            { ItemType.Naz, 100 },
            { ItemType.Koroska, 30 },
            { ItemType.Kyndja, 30 },
            { ItemType.Lococ, 30 },
            { ItemType.Okyn, 30 },
            { ItemType.Ocetr, 30 },
            { ItemType.Skat, 30 },
            { ItemType.Tunec, 30 },
            { ItemType.Ygol, 30 },
            { ItemType.Amyr, 30 },
            { ItemType.Chyka, 30 },

            //Farmer Job Items
            { ItemType.Hay, 60 }, //60 урожая всего в инвентаре
            { ItemType.Seed, 100 }, //100 семян всего в инвентаре (максимум)

            { ItemType.GiveBox, 100 },


            //merumond add-on
            { ItemType.LSPDDrone, 3 },
            { ItemType.Drone, 3 },
            { ItemType.CasinoChips, 100000 },
            { ItemType.Number, 1 },
            { ItemType.Documents, 1 },
            { ItemType.License, 1 },
            { ItemType.Pill, 100 },
            { ItemType.UsePill, 100 },
            { ItemType.Binocular, 1 },
            { ItemType.Dildo, 1 },
            { ItemType.DildoBl, 1 },
            { ItemType.DildoRed, 1 },
            { ItemType.Apteka, 5 },
            { ItemType.Bint, 16 },
            { ItemType.Bong, 1 },
            { ItemType.RGBBong, 1 },
            { ItemType.Lighter, 10 },
            { ItemType.Programmer, 1},

            { ItemType.Mush1, 25 },
            { ItemType.Mush2, 25 },
            { ItemType.Mush3, 25 },
            { ItemType.Mush4, 25 },
            { ItemType.Mush5, 25 },
            { ItemType.Mush6, 25 },
            { ItemType.MushGold, 25 },

            { ItemType.Snowboard1, 1 },
            { ItemType.Snowboard2, 1 },
            { ItemType.Snowboard3, 1 },
            { ItemType.Snowboard4, 1 },
            { ItemType.Snowboard5, 1 },
            { ItemType.Snowboard6, 1 },
            { ItemType.Snowboard7, 1 },
            { ItemType.Snowboard8, 1 },

            { ItemType.Parachute, 1 },

            { ItemType.CookieSnow, 10 },
            { ItemType.Donut, 10 },
            { ItemType.Cocktail, 10 },
            { ItemType.Milkshake, 10 },
            { ItemType.Salad, 10 },
            { ItemType.Shashlik, 10 },

            { ItemType.SnowBall, 50 },
            { ItemType.MoneyHeist, 1 },
        };

        public static List<ItemType> ClothesItems = new List<ItemType>()
        {
            ItemType.Mask,
            ItemType.Gloves,
            ItemType.Leg,
            ItemType.Bag,
            ItemType.Feet,
            ItemType.Jewelry,
            ItemType.Undershit,
            ItemType.BodyArmor,
            ItemType.Decals,
            ItemType.Top,
            ItemType.Hat,
            ItemType.Glasses,
            ItemType.Accessories,
        };
        public static List<ItemType> WeaponsItems = new List<ItemType>()
        {
            ItemType.Pistol,
            ItemType.CombatPistol,
            ItemType.Pistol50,
            ItemType.SNSPistol,
            ItemType.HeavyPistol,
            ItemType.VintagePistol,
            ItemType.MarksmanPistol,
            ItemType.Revolver,
            ItemType.APPistol,
            ItemType.FlareGun,
            ItemType.DoubleAction,
            ItemType.PistolMk2,
            ItemType.SNSPistolMk2,
            ItemType.RevolverMk2,

            ItemType.MicroSMG,
            ItemType.MachinePistol,
            ItemType.SMG,
            ItemType.AssaultSMG,
            ItemType.CombatPDW,
            ItemType.MG,
            ItemType.CombatMG,
            ItemType.Gusenberg,
            ItemType.MiniSMG,
            ItemType.SMGMk2,
            ItemType.CombatMGMk2,

            ItemType.AssaultRifle,
            ItemType.CarbineRifle,
            ItemType.AdvancedRifle,
            ItemType.SpecialCarbine,
            ItemType.BullpupRifle,
            ItemType.CompactRifle,
            ItemType.AssaultRifleMk2,
            ItemType.CarbineRifleMk2,
            ItemType.SpecialCarbineMk2,
            ItemType.BullpupRifleMk2,

            ItemType.SniperRifle,
            ItemType.HeavySniper,
            ItemType.MarksmanRifle,
            ItemType.HeavySniperMk2,
            ItemType.MarksmanRifleMk2,

            ItemType.PumpShotgun,
            ItemType.SawnOffShotgun,
            ItemType.BullpupShotgun,
            ItemType.AssaultShotgun,
            ItemType.Musket,
            ItemType.HeavyShotgun,
            ItemType.DoubleBarrelShotgun,
            ItemType.SweeperShotgun,
            ItemType.PumpShotgunMk2,
        };
        public static List<ItemType> MeleeWeaponsItems = new List<ItemType>()
        {
            ItemType.Knife,
            ItemType.Nightstick,
            ItemType.Hammer,
            ItemType.Bat,
            ItemType.Crowbar,
            ItemType.GolfClub,
            ItemType.Bottle,
            ItemType.Dagger,
            ItemType.Hatchet,
            ItemType.KnuckleDuster,
            ItemType.Machete,
            ItemType.Flashlight,
            ItemType.SwitchBlade,
            ItemType.PoolCue,
            ItemType.Wrench,
            ItemType.BattleAxe,
            ItemType.StunGun,

            ItemType.SnowBall,
        };
        public static List<ItemType> ParachuteItem = new List<ItemType>()
        {
            ItemType.Parachute,
        };
        public static List<ItemType> AmmoItems = new List<ItemType>()
        {
           ItemType.PistolAmmo,
            ItemType.PistolAmmo2,
            ItemType.PistolAmmo3,
            ItemType.RiflesAmmo,
            ItemType.HeavyRiflesAmmo,
            ItemType.SniperAmmo,
            ItemType.ShotgunsAmmo,
            ItemType.CarabineAmmo,
        };
        public static List<ItemType> AlcoItems = new List<ItemType>()
        {
            ItemType.LcnDrink1,
            ItemType.LcnDrink2,
            ItemType.LcnDrink3,
            ItemType.RusDrink1,
            ItemType.RusDrink2,
            ItemType.RusDrink3,
            ItemType.YakDrink1,
            ItemType.YakDrink2,
            ItemType.YakDrink3,
            ItemType.ArmDrink1,
            ItemType.ArmDrink2,
            ItemType.ArmDrink3,
        };
        // UUID, Items by index
        public static Dictionary<int, List<nItem>> Items = new Dictionary<int, List<nItem>>();
        private static nLog Log = new nLog("nInventory");
        private static Timer SaveTimer;

        #region Constructor
        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                Log.Write("Loading player items...", nLog.Type.Info);
                // // //
                var result = MySQL.QueryRead($"SELECT * FROM `inventory`");
                if (result == null || result.Rows.Count == 0)
                {
                    Log.Write("DB return null result", nLog.Type.Warn);
                    return;
                }
                foreach (DataRow Row in result.Rows)
                {
                    int UUID = Convert.ToInt32(Row["uuid"]);
                    string json = Convert.ToString(Row["items"]);
                    List<nItem> items = JsonConvert.DeserializeObject<List<nItem>>(json);
                    Items.Add(UUID, items);
                }
                SaveTimer = new Timer(new TimerCallback(SaveAll), null, 0, 1800000);
                Log.Write("Items loaded.", nLog.Type.Success);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"INVENTORY_CONSTRUCT\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
        #endregion

        #region Add/Remove item
        public static void Add(Player player, nItem item)
        {
            try
            {
                int UUID = Main.Players[player].UUID;
                int index = FindIndex(UUID, item.Type);
                if (ClothesItems.Contains(item.Type) || WeaponsItems.Contains(item.Type) || item.Type == ItemType.CarKey || item.Type == ItemType.KeyRing || item.Type == ItemType.Number)
                {
                    Items[UUID].Add(item);
                    GUI.Dashboard.Update(player, item, Items[UUID].IndexOf(item));
                }
                else
                {
                    if (index != -1)
                    {
                        int count = Items[UUID][index].Count;
                        Items[UUID][index].Count = count + item.Count;
                        GUI.Dashboard.Update(player, Items[UUID][index], index);
                        Log.Debug($"Added existing item! {UUID.ToString()}:{index.ToString()}");
                    }
                    else
                    {
                        Items[UUID].Add(item);
                        GUI.Dashboard.Update(player, item, Items[UUID].IndexOf(item));
                    }
                }
                Log.Debug($"Item added. {UUID.ToString()}:{index.ToString()}");
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"INVENTORY_ADD\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

        public static float GetWeight(List<nItem> items)
        {
            try
            {
                float total = 0;
                foreach (nItem item in items)
                    total += item.Count * ItemsWeight[item.Type];
                return total;
            }
            catch { return 0f; }
        }

        public static bool IsFullWeight(List<nItem> items, int max, nItem toadd)
        {
            try
            {
                float total = GetWeight(items) + toadd.Count * ItemsWeight[toadd.Type];

                return total > max;
            }
            catch { return false; }
        }

        public static int TryAdd(Player client, nItem item)
        {
            try
            {
                int UUID = Main.Players[client].UUID;
                int index = FindIndex(UUID, item.Type);
                int tail = 0;

                if (IsFullWeight(nInventory.Items[UUID], 20, item))
                    return -1;

                /* if (ClothesItems.Contains(item.Type) || item.Type == ItemType.CarKey || item.Type == ItemType.KeyRing)
                 {
                     if (isFull(client,UUID))
                         return -1;
                 }
                 else if (MeleeWeaponsItems.Contains(item.Type))
                 {
                     if (isFull(client, UUID))
                         return -1;

                     var sameWeapon = Items[UUID].FirstOrDefault(i => i.Type == item.Type);
                     if (sameWeapon != null) return -1;
                 }
                 else if (ParachuteItem.Contains(item.Type))
                 {
                     if (isFull(client, UUID))
                         return -1;

                     var sameWeapon = Items[UUID].FirstOrDefault(i => i.Type == item.Type);
                     if (sameWeapon != null) return -1;
                 }
                 else
                 {

                 }*/
                return tail;
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"INVENTORY_ADD\":\n" + e.ToString(), nLog.Type.Error);
                return 0;
            }
        }
        public static void Remove(Player player, ItemType type, int count)
        {
            try
            {
                int UUID = Main.Players[player].UUID;
                int Index = FindIndex(UUID, type);
                if (Index != -1)
                {
                    int temp = Items[UUID][Index].Count - count;
                    if (temp > 0)
                    {
                        Items[UUID][Index].Count = temp;
                        GUI.Dashboard.Update(player, Items[UUID][Index], Index);
                    }
                    else
                    {
                        Items[UUID].RemoveAt(Index);
                        GUI.Dashboard.sendItems(player);
                    }
                }
                Log.Debug($"Item removed. {UUID.ToString()}:{Index.ToString()}");
                return;
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"INVENTORY_REMOVE\":\n" + e.ToString(), nLog.Type.Error);
            }

        }

        public static void Remove(Player player, nItem item)
        {
            try
            {
                int UUID = Main.Players[player].UUID;

                if (ClothesItems.Contains(item.Type) || WeaponsItems.Contains(item.Type) || MeleeWeaponsItems.Contains(item.Type) || ParachuteItem.Contains(item.Type) || item.Type == ItemType.BagWithDrill
                    || item.Type == ItemType.BagWithMoney || item.Type == ItemType.CarKey || item.Type == ItemType.KeyRing || item.Type == ItemType.Number)
                {
                    Items[UUID].Remove(item);
                    GUI.Dashboard.sendItems(player);
                    Log.Debug($"Item removed. {UUID.ToString()}:TYPE {(int)item.Type}");
                }
                else
                {
                    int Index = FindIndex(UUID, item.Type);
                    if (Index != -1)
                    {
                        int temp = Items[UUID][Index].Count - item.Count;
                        if (temp > 0)
                        {
                            Items[UUID][Index].Count = temp;
                            GUI.Dashboard.Update(player, Items[UUID][Index], Index);
                        }
                        else
                        {
                            Items[UUID].RemoveAt(Index);
                            GUI.Dashboard.sendItems(player);
                        }
                    }
                    Log.Debug($"Item removed. {UUID.ToString()}:{Index.ToString()}");
                }
                return;
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"INVENTORY_REMOVE\":\n" + e.ToString(), nLog.Type.Error);
            }

        }
        #endregion

        #region Save items to db
        public static void SaveAll(object state = null)
        {
            try
            {
                Log.Write("Saving items...", nLog.Type.Info);
                if (Items.Count == 0) return;
                Dictionary<int, List<nItem>> cItems = new Dictionary<int, List<nItem>>(Items);

                foreach (KeyValuePair<int, List<nItem>> kvp in cItems)
                {
                    int UUID = kvp.Key;
                    string json = JsonConvert.SerializeObject(kvp.Value);
                    MySQL.Query($"UPDATE `inventory` SET items='{json}' WHERE uuid={UUID}");
                }
                Log.Write("Items has been saved to DB.", nLog.Type.Success);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"INVENTORY_SAVEALL\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
        public static void Save(int UUID)
        {
            try
            {
                if (!Items.ContainsKey(UUID)) return;
                Log.Write($"Saving items for {UUID}", nLog.Type.Info);
                string json = JsonConvert.SerializeObject(Items[UUID]);
                MySQL.Query($"UPDATE `inventory` SET items='{json}' WHERE uuid={UUID}");
                Log.Write("Items has been saved to DB.", nLog.Type.Success);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"INVENTORY_SAVE\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
        #endregion

        #region SPECIAL
        public static nItem Find(int UUID, ItemType type)
        {
            List<nItem> items = Items[UUID];
            nItem result = items.Find(i => i.Type == type);
            return result;
        }
        public static int FindIndex(int UUID, ItemType type)
        {
            List<nItem> items = Items[UUID];
            int result = items.FindIndex(i => i.Type == type);
            return result;
        }

        public static int FindData(int UUID, ItemType data)
        {
            List<nItem> items = Items[UUID];
            int result = items.FindIndex(i => i.Data == data);
            return result;
        }

        public static bool isFull(Player player, int UUID) //todo Увеличение Maxinventory-player
        {
            if (Find(Main.Players[player].UUID, ItemType.Bag) != null)
            {
                if (Items[UUID].Count >= 70) return true;
                else return false;
            }
            else if (Items[UUID].Count >= 54) return true;
            else return false;
        }
        public static bool isFullNumber(int UUID) //todo Увеличение 
        {
            if (Items[UUID].Count > 1) return true;
            else return false;
        }

        public static void Check(int uuid)
        { //if items dict does not contains account uuid, then add him
            if (!Items.ContainsKey(uuid))
            {
                Items.Add(uuid, new List<nItem>());
                MySQL.Query($"INSERT INTO `inventory`(`uuid`,`items`) VALUES ({uuid},'{JsonConvert.SerializeObject(new List<nItem>())}')");
                Log.Debug("Player added");
            }
        }

        public static void UnActiveItem(Player player, ItemType type)
        {
            var items = Items[Main.Players[player].UUID];
            foreach (var i in items)
                if (i.Type == type && i.IsActive)
                {
                    i.IsActive = false;
                    GUI.Dashboard.Update(player, i, items.IndexOf(i));
                }
            Items[Main.Players[player].UUID] = items;
        }
        public static void ClearWithoutClothes(Player player)
        {
            try
            {
                int uuid = Main.Players[player].UUID;
                List<nItem> items = Items[uuid];
                List<nItem> upd = new List<nItem>();
                foreach (nItem item in items)
                    if (ClothesItems.Contains(item.Type) || item.Type == ItemType.CarKey || item.Type == ItemType.KeyRing || item.Type == ItemType.Number) upd.Add(item);

                Items[uuid] = upd;
                GUI.Dashboard.sendItems(player);
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
        }
        public static void ClearAllClothes(Player client)
        {
            try
            {
                int uuid = Main.Players[client].UUID;
                List<nItem> items = Items[uuid];
                List<nItem> upd = new List<nItem>();
                foreach (nItem item in items)
                    if (!ClothesItems.Contains(item.Type)) upd.Add(item);

                Items[uuid] = upd;
                GUI.Dashboard.sendItems(client);
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
        }
        #endregion
    }
    class Items : Script
    {
        private static nLog Log = new nLog("Items");

        private static Random rnd = new Random();
        public static List<int> ItemsDropped = new List<int>();
        public static List<int> InProcessering = new List<int>();
        [ServerEvent(Event.EntityDeleted)]
        public void Event_OnEntityDeleted(Entity entity)
        {
            try
            {
                if (NAPI.Entity.GetEntityType(entity) == EntityType.Object && NAPI.Data.HasEntityData(entity, "DELETETIMER"))
                {
                    Timers.Stop(NAPI.Data.GetEntityData(entity, "DELETETIMER"));
                    ItemsDropped.Remove(NAPI.Data.GetEntityData(entity, "ID"));
                    InProcessering.Remove(NAPI.Data.GetEntityData(entity, "ID"));
                }
            }
            catch (Exception e)
            {
                Log.Write("Event_OnEntityDeleted: " + e.Message, nLog.Type.Error);
            }
        }

        public static void deleteObject(GTANetworkAPI.Object obj)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    //Main.StopT(obj.GetData<object>("DELETETIMER"), "timer_33");
                    obj.ResetData("DELETETIMER");
                    ItemsDropped.Remove(obj.GetData<int>("ID"));
                    InProcessering.Remove(obj.GetData<int>("ID"));
                    obj.Delete();
                }
                catch (Exception e)
                {
                    Log.Write("UpdateObject: " + e.Message, nLog.Type.Error);
                }
            }, 0);
        }

        public static void onUse(Player player, nItem item, int index)
        {
            try
            {
                var UUID = Main.Players[player].UUID;
                if (nInventory.ClothesItems.Contains(item.Type) && item.Type != ItemType.BodyArmor && item.Type != ItemType.Mask)
                {
                    var data = (string)item.Data;
                    var clothesGender = Convert.ToBoolean(data.Split('_')[2]);
                    if (clothesGender != Main.Players[player].Gender)
                    {
                        var error_gender = (clothesGender) ? "мужская" : "женская";
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"This is {error_gender} clothes", 3000);
                        GUI.Dashboard.Close(player);
                        return;
                    }
                    if ((player.GetData<bool>("ON_DUTY") && Fractions.Manager.FractionTypes[Main.Players[player].FractionID] == 2 && Main.Players[player].FractionID != 9) || player.GetData<bool>("ON_WORK"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You can't use it now", 3000);
                        GUI.Dashboard.Close(player);
                        return;
                    }
                }

                if (nInventory.WeaponsItems.Contains(item.Type) || nInventory.MeleeWeaponsItems.Contains(item.Type))
                {
                    if (item.Type == ItemType.SnowBall)
                    {
                        nInventory.Remove(player, item.Type, 1);
                        Trigger.ClientEvent(player, "wgive", (int)Weapons.GetHash(item.Type.ToString()), 1, false, true);
                        return;
                    }
                    if (item.IsActive)
                    {
                        var wHash = Weapons.GetHash(item.Type.ToString());
                        Trigger.ClientEvent(player, "takeOffWeapon", (int)wHash);
                        player.ResetData("LastActiveWeap");
                        player.SetData("ActiveWeaponRobbey", false);
                        Commands.RPChat("me", player, $"removed {nInventory.ItemsNames[(int)item.Type]}");
                    }
                    else
                    {
                        if (player.GetData<bool>("Walkie_open") == true) return;
                        var oldwItem = nInventory.Items[UUID].FirstOrDefault(i => (nInventory.WeaponsItems.Contains(i.Type) || nInventory.MeleeWeaponsItems.Contains(i.Type)) && i.IsActive);
                        if (oldwItem != null)
                        {
                            var oldwHash = Weapons.GetHash(oldwItem.Type.ToString());
                            Trigger.ClientEvent(player, "serverTakeOffWeapon", (int)oldwHash);
                            oldwItem.IsActive = false;
                            GUI.Dashboard.Update(player, oldwItem, nInventory.Items[UUID].IndexOf(oldwItem));
                            Commands.RPChat("me", player, $"removed {nInventory.ItemsNames[(int)oldwItem.Type]}");
                        }

                        var wHash = Weapons.GetHash(item.Type.ToString());
                        if (Weapons.WeaponsAmmoTypes.ContainsKey(item.Type))
                        {
                            var ammoItem = nInventory.Find(UUID, Weapons.WeaponsAmmoTypes[item.Type]);
                            var ammo = (ammoItem == null) ? 0 : ammoItem.Count;
                            if (ammo > Weapons.WeaponsClipsMax[item.Type]) ammo = Weapons.WeaponsClipsMax[item.Type];
                            if (ammoItem != null) nInventory.Remove(player, ammoItem.Type, ammo);
                            Trigger.ClientEvent(player, "wgive", (int)wHash, ammo, false, true);
                        }
                        else
                        {
                            Trigger.ClientEvent(player, "wgive", (int)wHash, 1, false, true);
                        }

                        Commands.RPChat("me", player, $"got (a) {nInventory.ItemsNames[(int)item.Type]}");
                        item.IsActive = true;
                        player.SetData("LastActiveWeap", item.Type);
                        player.SetData("ActiveWeaponRobbey", true);
                        GUI.Dashboard.Update(player, item, index);
                        GUI.Dashboard.Close(player);
                    }
                    return;
                }

                if (nInventory.ParachuteItem.Contains(item.Type))
                {
                    if (item.IsActive)
                    {
                        var wHash = Weapons.GetHash(item.Type.ToString());
                        Trigger.ClientEvent(player, "takeOffWeapon", (int)wHash);
                        player.ResetData("LastActiveWeap");
                        Commands.RPChat("me", player, $"убрал(а) {nInventory.ItemsNames[(int)item.Type]}");
                    }
                    else
                    {
                        if (player.GetData<bool>("Walkie_open") == true) return;
                        var oldwItem = nInventory.Items[UUID].FirstOrDefault(i => (nInventory.WeaponsItems.Contains(i.Type) || nInventory.MeleeWeaponsItems.Contains(i.Type)) && i.IsActive);
                        if (oldwItem != null)
                        {
                            var oldwHash = Weapons.GetHash(oldwItem.Type.ToString());
                            Trigger.ClientEvent(player, "serverTakeOffWeapon", (int)oldwHash);
                            oldwItem.IsActive = false;
                            GUI.Dashboard.Update(player, oldwItem, nInventory.Items[UUID].IndexOf(oldwItem));
                            Commands.RPChat("me", player, $"убрал(а) {nInventory.ItemsNames[(int)oldwItem.Type]}");
                        }

                        var wHash = Weapons.GetHash(item.Type.ToString());
                        if (Weapons.WeaponsAmmoTypes.ContainsKey(item.Type))
                        {
                            var ammoItem = nInventory.Find(UUID, Weapons.WeaponsAmmoTypes[item.Type]);
                            var ammo = (ammoItem == null) ? 0 : ammoItem.Count;
                            if (ammo > Weapons.WeaponsClipsMax[item.Type]) ammo = Weapons.WeaponsClipsMax[item.Type];
                            if (ammoItem != null) nInventory.Remove(player, ammoItem.Type, ammo);
                            Trigger.ClientEvent(player, "wgive", (int)wHash, ammo, false, true);
                        }
                        else
                        {
                            Trigger.ClientEvent(player, "wgive", (int)wHash, 1, false, true);
                        }

                        Commands.RPChat("me", player, $"достал(а) {nInventory.ItemsNames[(int)item.Type]}");
                        item.IsActive = true;
                        nInventory.Remove(player, item);
                        player.SetData("LastActiveWeap", item.Type);
                        GUI.Dashboard.Update(player, item, index);
                        GUI.Dashboard.Close(player);
                    }
                    return;
                }

                if (nInventory.AmmoItems.Contains(item.Type)) return;

                if (nInventory.AlcoItems.Contains(item.Type))
                {
                    int stage = Convert.ToInt32(item.Type.ToString().Split("Drink")[1]);
                    int curStage = player.GetData<int>("RESIST_STAGE");

                    if (player.HasData("RESIST_BAN"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You are drunk to the point that you cannot open a bottle", 3000);
                        return;
                    }

                    var stageTimes = new List<int>() { 0, 300, 420, 600 };

                    if (curStage == 0 || curStage == stage)
                    {
                        player.SetData("RESIST_STAGE", stage);
                        player.SetData("RESIST_TIME", player.GetData<int>("RESIST_TIME") + stageTimes[stage]);
                    }
                    else if (curStage < stage)
                    {
                        player.SetData("RESIST_STAGE", stage);
                    }
                    else if (curStage > stage)
                    {
                        player.SetData("RESIST_TIME", player.GetData<int>("RESIST_TIME") + stageTimes[stage]);
                    }

                    if (player.GetData<int>("RESIST_TIME") >= 1500)
                        player.SetData("RESIST_BAN", true);

                    Trigger.ClientEvent(player, "setResistStage", player.GetData<int>("RESIST_STAGE"));
                    BasicSync.AttachObjectToPlayer(player, nInventory.ItemModels[item.Type], 57005, Fractions.AlcoFabrication.AlcoPosOffset[item.Type], Fractions.AlcoFabrication.AlcoRotOffset[item.Type]);

                    Main.OnAntiAnim(player);
                    player.PlayAnimation("amb@world_human_drinking@beer@male@idle_a", "idle_c", 49);
                    NAPI.Task.Run(() => {
                        try
                        {
                            if (player != null)
                            {
                                if (!player.IsInVehicle) player.PlayAnimation("rcmcollect_paperleadinout@", "kneeling_arrest_get_up", 33);
                                else player.SetData("ToResetAnimPhone", true);
                                Main.OffAntiAnim(player);
                                //Trigger.ClientEvent(player, "startScreenEffect", "PPFilter", player.GetData<int>("RESIST_TIME") * 1000, false);
                                BasicSync.DetachObject(player);
                            }
                        }
                        catch { }
                    }, 5000);

                    /*if (!player.HasData("RESIST_TIMER"))
                        player.SetData("RESIST_TIMER", Timers.Start(1000, () => Fractions.AlcoFabrication.ResistTimer(player.Name)));*/

                    Commands.RPChat("me", player, "выпил бутылку " + nInventory.ItemsNames[(int)item.Type]);
                    GameLog.Items($"player({Main.Players[player].UUID})", "use", Convert.ToInt32(item.Type), 1, $"{item.Data}");
                }

                var gender = Main.Players[player].Gender;
                Log.Debug("item used");
                switch (item.Type)
                {
                    #region Clothes
                    case ItemType.GiveBox:
                        {

                            if (nInventory.isFull(player, UUID))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Insufficient inventory space", 3000);
                                return;
                            }
                            int gift = rnd.Next(1, 101); // рандом от 0 до 100
                            if (gift <= 90) // шанс на выпадение денег
                            {
                                int pay = 20000 * gift;
                                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You received   {pay}$ from a Christmas present!", 3000);
                                MoneySystem.Wallet.Change(player, pay);
                            }
                            else if (gift <= 94) // шанс на выпадение предмета
                            {
                                var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.Musket));
                                if (tryAdd == -1 || tryAdd > 0)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"There is not enough space, you have been given compensation of $ 80,000", 3000);
                                    MoneySystem.Wallet.Change(player, 50000);
                                    return;
                                }
                                Weapons.GiveWeapon(player, ItemType.Musket, "112233445");
                                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "You received a unique weapon - Musket", 3000);
                            }
                            else if (gift <= 96) // шанс на выпадение предмета
                            {
                                Main.Accounts[player].RedBucks += 100;
                                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "You received 100 Coins from the New Year's gift!", 3000);
                            }
                            else
                            {
                                if (VehicleManager.getAllPlayerVehicles(player.Name).Count >= 10)
                                {
                                    MoneySystem.Wallet.Change(player, 120000); // сумма компенсации
                                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "You received $120,000 in compensation because you have the maximum number of cars", 3000);
                                }
                                else
                                {
                                    var vNumber = VehicleManager.Create(player.Name, "mark2", new Color(0, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0)); // название машины менять тут
                                    var house = Houses.HouseManager.GetHouse(player, false);
                                    if (house != null)
                                    {
                                        if (house.GarageID != 0)
                                        {
                                            var garage2 = Houses.GarageManager.Garages[house.GarageID];
                                            if (VehicleManager.getAllPlayerVehicles(player.Name).Count < Houses.GarageManager.GarageTypes[garage2.Type].MaxCars)
                                            {
                                                garage2.SpawnCar(vNumber);
                                            }
                                        }
                                    }
                                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "You have received a unique Toyota Mark 2", 3000);
                                }
                            }



                            nInventory.Remove(player, ItemType.GiveBox, 1);
                            GUI.Dashboard.sendItems(player);

                            return;
                        }
                    case ItemType.Glasses:
                        {
                            if (item.IsActive)
                            {
                                Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Glasses.Variation = -1;
                                player.ClearAccessory(1);
                                nInventory.Items[UUID][index].IsActive = false;
                                GUI.Dashboard.Update(player, item, index);
                            }
                            else
                            {
                                var mask = Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Mask.Variation;
                                if (Customization.MaskTypes.ContainsKey(mask) && Customization.MaskTypes[mask].Item3)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You can't wear those goggles with a mask", 3000);
                                    return;
                                }
                                var itemData = (string)item.Data;
                                var variation = Convert.ToInt32(itemData.Split('_')[0]);
                                var texture = Convert.ToInt32(itemData.Split('_')[1]);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Glasses = new ComponentItem(variation, texture);
                                player.SetAccessories(1, variation, texture);

                                nInventory.UnActiveItem(player, item.Type);
                                nInventory.Items[UUID][index].IsActive = true;
                                GUI.Dashboard.Update(player, item, index);
                            }
                            return;
                        }
                    case ItemType.Hat:
                        {
                            if (item.IsActive)
                            {
                                Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Hat.Variation = -1;

                                nInventory.Items[UUID][index].IsActive = false;
                                GUI.Dashboard.Update(player, item, index);
                            }
                            else
                            {
                                var mask = Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Mask.Variation;
                                if (Customization.MaskTypes.ContainsKey(mask) && Customization.MaskTypes[mask].Item2)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You can't wear this headgear with a mask", 3000);
                                    return;
                                }
                                var itemData = (string)item.Data;
                                var variation = Convert.ToInt32(itemData.Split('_')[0]);
                                var texture = Convert.ToInt32(itemData.Split('_')[1]);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Hat = new ComponentItem(variation, texture);

                                nInventory.UnActiveItem(player, item.Type);
                                nInventory.Items[UUID][index].IsActive = true;
                                GUI.Dashboard.Update(player, item, index);
                            }
                            Customization.SetHat(player, Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Hat.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Hat.Texture);
                            return;
                        }
                    case ItemType.Mask:
                        {
                            if (item.IsActive)
                            {
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Mask = new ComponentItem(Customization.EmtptySlots[gender][1], 0);

                                nInventory.Items[UUID][index].IsActive = false;
                                GUI.Dashboard.Update(player, item, index);
                            }
                            else
                            {
                                var itemData = (string)item.Data;
                                var variation = Convert.ToInt32(itemData.Split('_')[0]);
                                var texture = Convert.ToInt32(itemData.Split('_')[1]);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Mask = new ComponentItem(variation, texture);

                                if (Customization.MaskTypes.ContainsKey(variation))
                                {
                                    if (Customization.MaskTypes[variation].Item1)
                                    {
                                        player.SetClothes(2, 0, 0);
                                    }
                                    if (Customization.MaskTypes[variation].Item2)
                                    {
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Hat.Variation = -1;
                                        nInventory.UnActiveItem(player, ItemType.Hat);
                                        Customization.SetHat(player, -1, 0);
                                    }
                                    if (Customization.MaskTypes[variation].Item3)
                                    {
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Glasses.Variation = -1;
                                        nInventory.UnActiveItem(player, ItemType.Glasses);
                                        player.ClearAccessory(1);
                                    }
                                }

                                nInventory.UnActiveItem(player, item.Type);
                                nInventory.Items[UUID][index].IsActive = true;
                                GUI.Dashboard.Update(player, item, index);
                            }
                            Customization.SetMask(player, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Mask.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Mask.Texture);
                            return;
                        }
                    case ItemType.Gloves:
                        {
                            if (item.IsActive)
                            {
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Gloves = new ComponentItem(0, 0);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Torso = new ComponentItem(Customization.CorrectTorso[gender][Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation], 0);

                                nInventory.Items[UUID][index].IsActive = false;
                                GUI.Dashboard.Update(player, item, index);
                            }
                            else
                            {
                                var itemData = (string)item.Data;
                                var variation = Convert.ToInt32(itemData.Split('_')[0]);
                                var texture = Convert.ToInt32(itemData.Split('_')[1]);
                                if (!Customization.CorrectGloves[gender][variation].ContainsKey(Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Torso.Variation)) return;
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Gloves = new ComponentItem(variation, texture);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Torso = new ComponentItem(Customization.CorrectGloves[gender][variation][Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Torso.Variation], texture);

                                nInventory.UnActiveItem(player, item.Type);
                                nInventory.Items[UUID][index].IsActive = true;
                                GUI.Dashboard.Update(player, item, index);
                            }
                            player.SetClothes(3, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Torso.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Torso.Texture);
                            return;
                        }
                    case ItemType.Leg:
                        {
                            if (item.IsActive)
                            {
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Leg = new ComponentItem(Customization.EmtptySlots[gender][4], 4);

                                nInventory.Items[UUID][index].IsActive = false;
                                GUI.Dashboard.Update(player, item, index);
                            }
                            else
                            {
                                var itemData = (string)item.Data;
                                var variation = Convert.ToInt32(itemData.Split('_')[0]);
                                var texture = Convert.ToInt32(itemData.Split('_')[1]);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Leg = new ComponentItem(variation, texture);

                                nInventory.UnActiveItem(player, item.Type);
                                nInventory.Items[UUID][index].IsActive = true;
                                GUI.Dashboard.Update(player, item, index);
                            }
                            player.SetClothes(4, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Leg.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Leg.Texture);
                            return;
                        }
                    case ItemType.Bag:
                        {
                            if (item.IsActive)
                            {
                                if (nInventory.Items[Main.Players[player].UUID].Count > 20)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "First, remove the extra things from the backpack!", 3000);
                                    return;
                                }

                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bag = new ComponentItem(Customization.EmtptySlots[gender][5], 0);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bag.Texture = 1;
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bag.Variation = 0;
                                player.ResetData("BAG_UP");
                                nInventory.Items[UUID][index].IsActive = false;
                                GUI.Dashboard.Update(player, item, index);
                            }
                            else
                            {
                                var itemData = (string)item.Data;
                                var variation = Convert.ToInt32(itemData.Split('_')[0]);
                                var texture = Convert.ToInt32(itemData.Split('_')[1]);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bag = new ComponentItem(variation, texture);

                                nInventory.UnActiveItem(player, item.Type);
                                nInventory.Items[UUID][index].IsActive = true;
                                GUI.Dashboard.Update(player, item, index);
                                player.SetData("BAG_UP", true);
                            }
                            player.SetClothes(5, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bag.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bag.Texture);
                            return;
                        }
                    case ItemType.Feet:
                        {
                            if (item.IsActive)
                            {
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Feet = new ComponentItem(Customization.EmtptySlots[gender][6], 0);

                                nInventory.Items[UUID][index].IsActive = false;
                                GUI.Dashboard.Update(player, item, index);
                            }
                            else
                            {
                                var itemData = (string)item.Data;
                                var variation = Convert.ToInt32(itemData.Split('_')[0]);
                                var texture = Convert.ToInt32(itemData.Split('_')[1]);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Feet = new ComponentItem(variation, texture);

                                nInventory.UnActiveItem(player, item.Type);
                                nInventory.Items[UUID][index].IsActive = true;
                                GUI.Dashboard.Update(player, item, index);
                            }
                            player.SetClothes(6, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Feet.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Feet.Texture);
                            return;
                        }
                    case ItemType.Jewelry:
                        {
                            if (item.IsActive)
                            {
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Accessory = new ComponentItem(Customization.EmtptySlots[gender][7], 0);

                                nInventory.Items[UUID][index].IsActive = false;
                                GUI.Dashboard.Update(player, item, index);
                            }
                            else
                            {
                                var itemData = (string)item.Data;
                                var variation = Convert.ToInt32(itemData.Split('_')[0]);
                                var texture = Convert.ToInt32(itemData.Split('_')[1]);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Accessory = new ComponentItem(variation, texture);

                                nInventory.UnActiveItem(player, item.Type);
                                nInventory.Items[UUID][index].IsActive = true;
                                GUI.Dashboard.Update(player, item, index);
                            }
                            player.SetClothes(7, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Accessory.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Accessory.Texture);
                            return;
                        }
                    case ItemType.Accessories:
                        {
                            var itemData = (string)item.Data;
                            var variation = Convert.ToInt32(itemData.Split('_')[0]);
                            var texture = Convert.ToInt32(itemData.Split('_')[1]);

                            if (item.IsActive)
                            {
                                var watchesSlot = Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Watches;
                                if (watchesSlot.Variation == variation && watchesSlot.Texture == texture)
                                {
                                    Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Watches = new ComponentItem(-1, 0);
                                    player.ClearAccessory(6);
                                }
                                else
                                {
                                    Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Bracelets = new ComponentItem(-1, 0);
                                    player.ClearAccessory(7);
                                }

                                nInventory.Items[UUID][index].IsActive = false;
                                GUI.Dashboard.Update(player, item, index);
                            }
                            else
                            {
                                if (Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Watches.Variation == -1)
                                {
                                    Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Watches = new ComponentItem(variation, texture);
                                    player.SetAccessories(6, variation, texture);

                                    nInventory.Items[UUID][index].IsActive = true;
                                    GUI.Dashboard.Update(player, item, index);
                                }
                                else if (Customization.AccessoryRHand[gender].ContainsKey(variation))
                                {
                                    if (Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Bracelets.Variation == -1)
                                    {
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Bracelets = new ComponentItem(Customization.AccessoryRHand[gender][variation], texture);
                                        player.SetAccessories(7, Customization.AccessoryRHand[gender][variation], texture);

                                        nInventory.Items[UUID][index].IsActive = true;
                                        GUI.Dashboard.Update(player, item, index);
                                    }
                                    else
                                    {
                                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Both hands are occupied", 3000);
                                        return;
                                    }
                                }
                                else
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "The left hand is busy, but no one wears a watch on the right", 3000);
                                    return;
                                }
                            }
                            return;
                        }
                    case ItemType.Undershit:
                        {
                            var itemData = (string)item.Data;
                            var underwearID = Convert.ToInt32(itemData.Split('_')[0]);
                            var underwear = Customization.Underwears[gender][underwearID];
                            var texture = Convert.ToInt32(itemData.Split('_')[1]);
                            if (item.IsActive)
                            {
                                if (underwear.Top == Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation)
                                    Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top = new ComponentItem(Customization.EmtptySlots[gender][11], 0);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit = new ComponentItem(Customization.EmtptySlots[gender][8], 0);

                                nInventory.Items[UUID][index].IsActive = false;
                                GUI.Dashboard.Update(player, item, index);
                            }
                            else
                            {
                                if (Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation == Customization.EmtptySlots[gender][11])
                                {
                                    if (underwear.Top == -1)
                                    {
                                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "This garment can only be worn under the bottom of the top", 3000);
                                        return;
                                    }
                                    Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top = new ComponentItem(underwear.Top, texture);

                                    nInventory.UnActiveItem(player, item.Type);
                                    nInventory.Items[UUID][index].IsActive = true;
                                    GUI.Dashboard.Update(player, item, index);
                                }
                                else
                                {
                                    var nowTop = Customization.Tops[gender].FirstOrDefault(t => t.Variation == Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation);
                                    if (nowTop != null)
                                    {
                                        var topType = nowTop.Type;
                                        if (!underwear.UndershirtIDs.ContainsKey(topType))
                                        {
                                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "This garment is not compatible with your outerwear.", 3000);
                                            return;
                                        }
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit = new ComponentItem(underwear.UndershirtIDs[topType], texture);

                                        nInventory.UnActiveItem(player, item.Type);
                                        nInventory.Items[UUID][index].IsActive = true;
                                        GUI.Dashboard.Update(player, item, index);
                                    }
                                    else
                                    {
                                        if (underwear.Top == -1)
                                        {
                                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "This garment can only be worn under the bottom of the top", 3000);
                                            return;
                                        }
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top = new ComponentItem(underwear.Top, texture);

                                        nInventory.UnActiveItem(player, item.Type);
                                        nInventory.Items[UUID][index].IsActive = true;
                                        GUI.Dashboard.Update(player, item, index);
                                    }
                                }
                            }

                            var gloves = Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Gloves.Variation;
                            if (gloves != 0 &&
                                !Customization.CorrectGloves[gender][gloves].ContainsKey(Customization.CorrectTorso[gender][Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation]))
                            {
                                nInventory.UnActiveItem(player, ItemType.Gloves);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Gloves = new ComponentItem(0, 0);
                            }

                            player.SetClothes(8, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit.Texture);
                            player.SetClothes(11, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Texture);
                            var noneGloves = Customization.CorrectTorso[gender][Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation];
                            if (Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Gloves.Variation == 0)
                            {
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Torso = new ComponentItem(noneGloves, 0);
                                player.SetClothes(3, noneGloves, 0);
                            }
                            else
                                player.SetClothes(3, Customization.CorrectGloves[gender][Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Gloves.Variation][noneGloves], Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Gloves.Texture);
                            return;
                        }
                    case ItemType.BodyArmor:
                        {
                            if (item.IsActive)
                            {
                                item.Data = player.Armor.ToString();
                                player.Armor = 0;
                                player.ResetSharedData("HASARMOR");
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 0;
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture = 0;
                                player.SetClothes(9, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture);
                                nInventory.Items[UUID][index].IsActive = false;
                                GUI.Dashboard.Update(player, item, index);
                            }
                            else
                            {
                                var armor = Convert.ToInt32((string)item.Data);
                                player.Armor = armor;
                                player.SetSharedData("HASARMOR", true);
                                switch (Main.Players[player].FractionID)
                                {
                                    case 0:
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture = 6;
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 28;
                                        break;
                                    case 1:
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture = 0;
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 28;
                                        break;
                                    case 2:
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture = 2;
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 28;
                                        break;
                                    case 3:
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture = 1;
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 28;
                                        break;
                                    case 4:
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture = 5;
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 28;
                                        break;
                                    case 5:
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture = 4;
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 28;
                                        break;
                                    case 6:
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture = 8;
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 28;
                                        break;
                                    case 7:
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture = 2;
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 16;
                                        break;
                                    case 8:
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture = 7; //EMS
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 28;
                                        break;
                                    case 9:
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture = 9; //FBI 
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 28;
                                        break;
                                    case 10:
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture = 7; //Мафия 1
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 28;
                                        break;
                                    case 11:
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture = 7; //Мафия 2
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 28;
                                        break;
                                    case 12:
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture = 8; //Мафия 3
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 28;
                                        break;
                                    case 13:
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture = 4; //Мафия 4
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 28;
                                        break;
                                    case 14:
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture = 0;
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 16;
                                        break;
                                    case 15:
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture = 6;
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 28;
                                        break;
                                    case 18:
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture = 9; // Групп6
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 28;
                                        break;

                                }
                                //Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 12;
                                player.SetClothes(9, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture);
                                nInventory.UnActiveItem(player, item.Type);
                                nInventory.Items[UUID][index].IsActive = true;
                                GUI.Dashboard.Update(player, item, index);
                            }
                            return;
                        }
                    case ItemType.Decals:
                        {
                            if (item.IsActive)
                            {
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Decals = new ComponentItem(Customization.EmtptySlots[gender][10], 0);
                            }
                            else
                            {
                                var itemData = (string)item.Data;
                                var variation = Convert.ToInt32(itemData.Split('_')[0]);
                                var texture = Convert.ToInt32(itemData.Split('_')[1]);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Decals = new ComponentItem(variation, texture);
                            }
                            player.SetClothes(10, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Decals.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Decals.Texture);
                            return;
                        }
                    case ItemType.Top:
                        {
                            if (item.IsActive)
                            {
                                if (Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit.Variation == Customization.EmtptySlots[gender][8] || (!gender && Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit.Variation == 15))
                                    Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top = new ComponentItem(Customization.EmtptySlots[gender][11], 0);
                                else
                                {
                                    var underwearID = Customization.Undershirts[gender][Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit.Variation];
                                    var underwear = Customization.Underwears[gender][underwearID];
                                    if (underwear.Top == -1)
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top = new ComponentItem(Customization.EmtptySlots[gender][11], 0);
                                    else
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top = new ComponentItem(underwear.Top, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit.Texture);
                                    Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit = new ComponentItem(Customization.EmtptySlots[gender][8], 0);
                                }

                                nInventory.Items[UUID][index].IsActive = false;
                                GUI.Dashboard.Update(player, item, index);
                            }
                            else
                            {
                                var itemData = (string)item.Data;
                                var variation = Convert.ToInt32(itemData.Split('_')[0]);
                                var texture = Convert.ToInt32(itemData.Split('_')[1]);

                                if (Customization.Tops[gender].FirstOrDefault(t => t.Variation == Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation) != null || Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation == Customization.EmtptySlots[gender][11])
                                {
                                    if (Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit.Variation == Customization.EmtptySlots[gender][8] || (!gender && Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit.Variation == 15))
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top = new ComponentItem(variation, texture);
                                    else
                                    {
                                        var underwearID = Customization.Undershirts[gender][Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit.Variation];
                                        var underwear = Customization.Underwears[gender][underwearID];
                                        var underwearTexture = Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit.Texture;
                                        var topType = Customization.Tops[gender].FirstOrDefault(t => t.Variation == variation).Type;
                                        Log.Debug($"UnderwearID: {underwearID} | TopType: {topType}");
                                        if (!underwear.UndershirtIDs.ContainsKey(topType))
                                        {
                                            Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit = new ComponentItem(Customization.EmtptySlots[gender][8], 0);
                                            nInventory.UnActiveItem(player, ItemType.Undershit);
                                        }
                                        else
                                            Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit = new ComponentItem(underwear.UndershirtIDs[topType], underwearTexture);
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top = new ComponentItem(variation, texture);
                                    }
                                }
                                else
                                {
                                    var underwearID = 0;
                                    var underwear = Customization.Underwears[gender].Values.FirstOrDefault(u => u.Top == Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation);
                                    var underwearTexture = Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Texture;
                                    if (underwear != null)
                                    {
                                        var topType = Customization.Tops[gender].FirstOrDefault(t => t.Variation == variation).Type;
                                        Log.Debug($"UnderwearID: {underwearID} | TopType: {topType}");
                                        if (!underwear.UndershirtIDs.ContainsKey(topType))
                                        {
                                            Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit = new ComponentItem(Customization.EmtptySlots[gender][8], 0);
                                            nInventory.UnActiveItem(player, ItemType.Undershit);
                                        }
                                        else
                                            Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit = new ComponentItem(underwear.UndershirtIDs[topType], underwearTexture);
                                    }
                                    Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top = new ComponentItem(variation, texture);
                                }

                                nInventory.UnActiveItem(player, item.Type);
                                nInventory.Items[UUID][index].IsActive = true;
                                GUI.Dashboard.Update(player, item, index);
                            }

                            var gloves = Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Gloves.Variation;
                            if (gloves != 0 &&
                                !Customization.CorrectGloves[gender][gloves].ContainsKey(Customization.CorrectTorso[gender][Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation]))
                            {
                                nInventory.UnActiveItem(player, ItemType.Gloves);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Gloves = new ComponentItem(0, 0);
                            }

                            player.SetClothes(8, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit.Texture);
                            player.SetClothes(11, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Texture);
                            var noneGloves = Customization.CorrectTorso[gender][Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation];
                            if (Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Gloves.Variation == 0)
                            {
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Torso = new ComponentItem(noneGloves, 0);
                                player.SetClothes(3, noneGloves, 0);
                            }
                            else
                                player.SetClothes(3, Customization.CorrectGloves[gender][Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Gloves.Variation][noneGloves], Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Gloves.Texture);
                            return;
                        }
                    #endregion
                    case ItemType.BagWithDrill:
                    case ItemType.BagWithMoney:
                    case ItemType.Pocket:
                    case ItemType.Cuffs:
                    case ItemType.CarKey:
                    case ItemType.Hay:
                    case ItemType.Seed:
                    case ItemType.MoneyHeist:
                        return;
                    case ItemType.KeyRing:
                        List<nItem> items = new List<nItem>();
                        string data = item.Data;
                        List<string> keys = (data.Length == 0) ? new List<string>() : new List<string>(data.Split('/'));
                        if (keys.Count > 0 && string.IsNullOrEmpty(keys[keys.Count - 1]))
                            keys.RemoveAt(keys.Count - 1);

                        foreach (var key in keys)
                            items.Add(new nItem(ItemType.CarKey, 1, key));
                        player.SetData("KEYRING", nInventory.Items[Main.Players[player].UUID].IndexOf(item));
                        GUI.Dashboard.OpenOut(player, items, "Связка ключей", 7);
                        return;
                    case ItemType.Material:
                        Trigger.ClientEvent(player, "board", "close");
                        GUI.Dashboard.isopen[player] = false;
                        GUI.Dashboard.Close(player);
                        Fractions.Manager.OpenGunCraftMenu(player);
                        return;
                    case ItemType.Beer:
                        EatManager.AddWater(player, 12);
                        EatManager.AddEat(player, 2);
                        Commands.RPChat("me", player, $"drank {nInventory.ItemsNames[(int)item.Type]}");
                        break;
                    case ItemType.Burger:
                        player.Health = (player.Health + 30 > 100) ? 100 : player.Health + 30;
                        EatManager.AddEat(player, 15);
                        if (player.GetData<int>("RESIST_TIME") < 600) Trigger.ClientEvent(player, "stopScreenEffect", "PPFilter");
                        Commands.RPChat("me", player, $"ate {nInventory.ItemsNames[(int)item.Type]}");
                        break;
                    case ItemType.eCola:
                        EatManager.AddWater(player, 15);
                        EatManager.AddEat(player, 2);
                        Commands.RPChat("me", player, $"drank {nInventory.ItemsNames[(int)item.Type]}");
                        break;
                    case ItemType.HotDog:
                        EatManager.AddWater(player, -10);
                        EatManager.AddEat(player, 14);
                        if (player.GetData<int>("RESIST_TIME") < 600) Trigger.ClientEvent(player, "stopScreenEffect", "PPFilter");
                        Commands.RPChat("me", player, $"ate {nInventory.ItemsNames[(int)item.Type]}");
                        break;
                    case ItemType.Pizza:
                        EatManager.AddWater(player, -10);
                        EatManager.AddEat(player, 30);
                        if (player.GetData<int>("RESIST_TIME") < 600) Trigger.ClientEvent(player, "stopScreenEffect", "PPFilter");
                        Commands.RPChat("me", player, $"ate {nInventory.ItemsNames[(int)item.Type]}");
                        break;
                    case ItemType.Sandwich:
                        EatManager.AddWater(player, -5);
                        EatManager.AddEat(player, 8);
                        if (player.GetData<int>("RESIST_TIME") < 600) Trigger.ClientEvent(player, "stopScreenEffect", "PPFilter");
                        Commands.RPChat("me", player, $"ate {nInventory.ItemsNames[(int)item.Type]}");
                        break;
                    case ItemType.Sprunk:
                        EatManager.AddWater(player, 25);
                        EatManager.AddEat(player, 2);
                        if (player.GetData<int>("RESIST_TIME") < 600) Trigger.ClientEvent(player, "stopScreenEffect", "PPFilter");
                        Commands.RPChat("me", player, $"drank {nInventory.ItemsNames[(int)item.Type]}");
                        break;
                    case ItemType.Сrisps:
                        EatManager.AddWater(player, -10);
                        EatManager.AddEat(player, 15);
                        if (player.GetData<int>("RESIST_TIME") < 600) Trigger.ClientEvent(player, "stopScreenEffect", "PPFilter");
                        Commands.RPChat("me", player, $"ate {nInventory.ItemsNames[(int)item.Type]}");
                        break;
                    case ItemType.Boombox:
                        Notify.Info(player, "To put a boombox, press ALT");
                        return;
                    case ItemType.Rod:
                        RodManager.useInventory(player, 1);
                        return;
                    case ItemType.RodUpgrade:
                        RodManager.useInventory(player, 2);
                        return;
                    case ItemType.RodMK2:
                        RodManager.useInventory(player, 3);
                        return;
                    case ItemType.Drugs:
                        return;
                    case ItemType.HealthKit:
                        if (!player.HasData("USE_MEDKIT") || DateTime.Now > player.GetData<DateTime>("USE_MEDKIT"))
                        {
                            player.Health = 100;
                            player.SetData("USE_MEDKIT", DateTime.Now.AddMinutes(5));
                            Main.OnAntiAnim(player);
                            player.PlayAnimation("amb@code_human_wander_texting_fat@female@enter", "enter", 49);
                            NAPI.Task.Run(() => {
                                try
                                {
                                    if (player == null) return;
                                    if (!player.IsInVehicle) player.PlayAnimation("rcmcollect_paperleadinout@", "kneeling_arrest_get_up", 33);
                                    else player.SetData("ToResetAnimPhone", true);
                                    Main.OffAntiAnim(player);
                                    Trigger.ClientEvent(player, "stopScreenEffect", "PPFilter");
                                }
                                catch { }
                            }, 5000);
                            Commands.RPChat("me", player, $"used a first aid kit");
                        }
                        else
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Try to use later", 3000);
                            return;
                        }
                        break;
                    case ItemType.RepairKit:
                        var vehicle = VehicleManager.getNearestVehicle(player, 3);
                        if (player.IsInVehicle)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You need to get out of the car", 3000);
                            return;
                        }
                        if (vehicle == null)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You must be near the car", 3000);
                            return;
                        }
                        nInventory.Remove(player, ItemType.RepairKit, 1);
                        GUI.Dashboard.Close(player);
                        Main.OnAntiAnim(player);
                        player.PlayAnimation("amb@world_human_vehicle_mechanic@male@idle_a", "idle_a", 39);
                        NAPI.Task.Run(() => {
                            try
                            {
                                if (player != null && Main.Players.ContainsKey(player))
                                {
                                    NAPI.Vehicle.SetVehicleEngineHealth(vehicle, 1000);
                                    VehicleManager.RepairCar(vehicle);
                                    player.PlayAnimation("rcmcollect_paperleadinout@", "kneeling_arrest_get_up", 33);
                                    Main.OffAntiAnim(player);
                                    //NAPI.Entity.SetEntityPosition(player, player.Position + new Vector3(0, 0, 0.2));
                                    player.Position += new Vector3(0, 0, 0.2);
                                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "You have successfully repaired the car", 3000);
                                }
                            }
                            catch { }
                        }, 20000);
                        return;
                    case ItemType.Lockpick:
                        if (player.GetData<int>("INTERACTIONCHECK") != 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Unable to use at the moment", 3000);
                            GUI.Dashboard.Close(player);
                            return;
                        }
                        //player.SetData("LOCK_TIMER", Main.StartT(10000, 999999, (o) => SafeMain.lockCrack(player, player.Name), "LOCK_TIMER"));
                        player.SetData("LOCK_TIMER", Timers.StartOnce(10000, () => SafeMain.lockCrack(player, player.Name)));
                        //player.FreezePosition = true;
                        Trigger.ClientEvent(player, "showLoader", "Идёт взлом", 1);
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"You started to break down the door", 3000);
                        break;
                    case ItemType.ArmyLockpick:
                        if (player.HasData("HOUSEID"))
                        {
                            Houses.House house = Houses.HouseManager.Houses.FirstOrDefault(h => h.ID == player.GetData<int>("HOUSEID"));
                            if (house == null || !Houses.HouseManager.Houses.Contains(house)) return;
                            Scripts.HijackingHouse.HijackingHouseData dataHij = Scripts.HijackingHouse.HijackingHouseData.HijackingHouseDic.FirstOrDefault(i => i.Player == player);
                            if (dataHij != null)
                            {
                                if (dataHij.House == house.ID && dataHij.TypeHouse == house.Type)
                                {
                                    if (!house.Locked)
                                    {
                                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "The house is open", 3000);
                                        return;
                                    }
                                    else
                                    {
                                        int random = rnd.Next(0, 12);
                                        if (random == 8)
                                        {
                                            house.SetLock(false);
                                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "you opened a house", 3000);
                                        }
                                        else
                                        {
                                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "try again", 3000);
                                            nInventory.Remove(player, new nItem(ItemType.ArmyLockpick));
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (!player.IsInVehicle || player.Vehicle.DisplayName != "BARRACKS" || player.VehicleSeat != 0)
                            {
                                Notify.Error(player, $"You must be in a military material carrier");
                                return;
                            }
                            if (VehicleStreaming.GetEngineState(player.Vehicle))
                            {
                                player.SetIntoVehicle(player.Vehicle, player.VehicleSeat);
                                Notify.Error(player, $"The car is already running");
                                return;
                            }
                            player.SetIntoVehicle(player.Vehicle, player.VehicleSeat);
                            int lucky = rnd.Next(0, 6);
                            Log.Debug(lucky.ToString());
                            if (lucky == 5 || lucky == 1)
                            {
                                Notify.Error(player, $"You failed to start the car. Try again");
                                if (player.IsInVehicle) player.SetIntoVehicle(player.Vehicle, player.VehicleSeat);
                                return;
                            }
                            else
                            {
                                VehicleStreaming.SetEngineState(player.Vehicle, true);
                                Notify.Succ(player, $"Did you manage to start the car?");
                            }
                            if (player.IsInVehicle) player.SetIntoVehicle(player.Vehicle, player.VehicleSeat);
                        }
                        break;
                    case ItemType.Present:
                        player.Health = (player.Health + 10 > 100) ? 100 : player.Health + 10;
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You opened the gift, it contained:", 3000);

                        Tuple<int, int> types = PresentsTypes[Convert.ToInt32(item.Data)];
                        if (types.Item1 <= 2)
                        {
                            Main.Players[player].EXP += TypesCounts[types.Item1];
                            if (Main.Players[player].EXP >= 3 + Main.Players[player].LVL * 3)
                            {
                                Main.Players[player].EXP = Main.Players[player].EXP - (3 + Main.Players[player].LVL * 3);
                                Main.Players[player].LVL += 1;
                            }

                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"{TypesCounts[types.Item1]} EXP", 3000);

                            MoneySystem.Wallet.Change(player, TypesCounts[types.Item2]);

                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"$ {TypesCounts[types.Item2]}", 3000);
                        }
                        else
                        {
                            MoneySystem.Wallet.Change(player, TypesCounts[types.Item1]);

                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"$ {TypesCounts[types.Item1]}", 3000);

                            Main.Players[player].EXP += TypesCounts[types.Item2];
                            if (Main.Players[player].EXP >= 3 + Main.Players[player].LVL * 3)
                            {
                                Main.Players[player].EXP = Main.Players[player].EXP - (3 + Main.Players[player].LVL * 3);
                                Main.Players[player].LVL += 1;
                            }

                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"{TypesCounts[types.Item2]} EXP", 3000);
                        }

                        Commands.RPChat("me", player, $"opened a gift");
                        break;
                    case ItemType.LSPDDrone:
                        if (Main.Players[player].FractionID != 7 && Main.Players[player].FractionID != 9 && Main.Players[player].FractionID != 14 && Main.Players[player].FractionID != 6 && Main.Players[player].FractionID != 8)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Drones can only be used by government employees", 3000);
                            return;
                        }
                        if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"You must start the work day", 3000);
                            return;
                        }

                        Trigger.ClientEvent(player, "client:StartLSPDDrone");
                        break;
                    case ItemType.Walkie:
                        /*if (Main.Players[player].FractionID != 6 & Main.Players[player].FractionID != 7 & Main.Players[player].FractionID != 8 & Main.Players[player].FractionID != 9 & Main.Players[player].FractionID != 14)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не умеете пользоваться рациями", 3000);
                            return;
                        } */
                        if (player.GetData<bool>("Walkie_open") == true)
                        {
                            GUI.Dashboard.Close(player);
                            player.ResetData("Walkie_open");
                            player.SetData("Walkie_open", false);
                        }
                        else
                        {
                            GUI.Dashboard.Close(player);
                            player.ResetData("Walkie_open");
                            player.SetData("Walkie_open", true);
                        }
                        return;
                    case ItemType.Programmer:
                        if (!player.IsInVehicle || !player.Vehicle.HasData("Hijacking")) return;
                        if (player.VehicleSeat == 0)
                        {
                            if (VehicleStreaming.GetEngineState(player.Vehicle))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"The car is already running", 3000);
                                player.SetIntoVehicle(player.Vehicle, 0);
                                return;
                            }
                            Trigger.ClientEvent(player, "hijackingBaitTaken");
                            GUI.Dashboard.Close(player);
                            /* var rand = Jobs.WorkManager.rnd.Next(1, 3);
                             Log.Debug($"{rand}");
                             if (rand == 1)
                             {
                                 Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас не получилось завести машину. Попробуйте ещё раз", 3000);
                                 player.SetIntoVehicle(player.Vehicle, 0);
                                 return;
                             }
                             else
                             {
                                 VehicleStreaming.SetEngineState(player.Vehicle, true);
                                 Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Доставьте автомобиль в гараж отмеченный на GPS", 3000);
                                 player.SetIntoVehicle(player.Vehicle, 0);
                                 GUI.Dashboard.Close(player);
                                 return;
                             }*/
                        }
                        break;
                    case ItemType.CasinoChips:
                        return;
                    case ItemType.Lighter:
                        return;
                    case ItemType.Bong:
                        if (!player.HasData("USE_DRUGS") || DateTime.Now > player.GetData<DateTime>("USE_DRUGS"))
                        {
                            var lighter = nInventory.Find(Main.Players[player].UUID, ItemType.Lighter);
                            if (lighter == null)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You don't have a lighter", 2000);
                                return;
                            }
                            var narko = nInventory.Find(Main.Players[player].UUID, ItemType.Drugs);
                            if (narko == null)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You don't have drugs", 2000);
                                return;
                            }
                            nInventory.Remove(player, ItemType.Drugs, 1);
                            Main.OnAntiAnim(player);
                            player.PlayAnimation("anim@safehouse@bong", "bong_stage1", 49);
                            Commands.RPChat("me", player, $"закурил(а) кальян");
                            player.SetData("USE_DRUGS", DateTime.Now.AddSeconds(20));
                            BasicSync.AttachObjectToPlayer(player, nInventory.ItemModels[ItemType.Bong], 57005, new Vector3(0.25, 0, -0.03), new Vector3(-40, -70, 15));
                            player.SetData("HEAL_TIMER_BONG", Timers.Start(3750, () => healTimer(player)));
                            Trigger.ClientEvent(player, "sound.bong");
                            NAPI.Task.Run(() => {
                                try
                                {
                                    if (player != null)
                                    {
                                        if (!player.IsInVehicle) player.PlayAnimation("rcmcollect_paperleadinout@", "kneeling_arrest_get_up", 33);
                                        else player.SetData("ToResetAnimPhone", true);
                                        Main.OffAntiAnim(player);
                                        BasicSync.DetachObject(player);
                                        //Trigger.ClientEvent(player, "startScreenEffect", "DrugsTrevorClownsFight", 300000, false);
                                    }
                                }
                                catch { }
                            }, 7000);
                        }
                        else
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Try to use later", 3000);
                            return;
                        }
                        return;
                    case ItemType.RGBBong:
                        if (!player.HasData("USE_DRUGS") || DateTime.Now > player.GetData<DateTime>("USE_DRUGS"))
                        {
                            var lighter = nInventory.Find(Main.Players[player].UUID, ItemType.Lighter);
                            if (lighter == null)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You don't have a lighter", 2000);
                                return;
                            }
                            var narko = nInventory.Find(Main.Players[player].UUID, ItemType.Drugs);
                            if (narko == null)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You don't have drugs", 2000);
                                return;
                            }
                            nInventory.Remove(player, ItemType.Drugs, 1);
                            Main.OnAntiAnim(player);
                            player.PlayAnimation("anim@safehouse@bong", "bong_stage1", 49);
                            Commands.RPChat("me", player, $"smoked a hookah");
                            player.SetData("USE_DRUGS", DateTime.Now.AddSeconds(20));
                            BasicSync.AttachObjectToPlayer(player, nInventory.ItemModels[ItemType.RGBBong], 57005, new Vector3(0.25, 0, -0.03), new Vector3(-40, -70, 15));
                            player.SetData("HEAL_TIMER_BONG", Timers.Start(3750, () => healTimer(player)));
                            Trigger.ClientEvent(player, "sound.bong");
                            NAPI.Task.Run(() => {
                                try
                                {
                                    if (player != null)
                                    {
                                        if (!player.IsInVehicle) player.PlayAnimation("rcmcollect_paperleadinout@", "kneeling_arrest_get_up", 33);
                                        else player.SetData("ToResetAnimPhone", true);
                                        Main.OffAntiAnim(player);
                                        BasicSync.DetachObject(player);
                                        //Trigger.ClientEvent(player, "startScreenEffect", "DrugsTrevorClownsFight", 300000, false);
                                    }
                                }
                                catch { }
                            }, 7000);
                        }
                        else
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Try to use later", 3000);
                            return;
                        }
                        return;
                    case ItemType.Documents:
                        return;
                    case ItemType.License:
                        return;
                    case ItemType.Binocular:
                        if (player.IsInVehicle) return;
                        GUI.Dashboard.Close(player);
                        if (!player.HasData("BINOCULAR"))
                        {
                            player.SetData("BINOCULAR", true);
                            Trigger.ClientEvent(player, "toggleBinocular");
                        }
                        else
                        {
                            player.ResetData("BINOCULAR");
                            Trigger.ClientEvent(player, "toggleBinocular");
                        }
                        return;
                    case ItemType.Apteka:
                        if (!player.HasData("USE_MEDKIT") || DateTime.Now > player.GetData<DateTime>("USE_MEDKIT"))
                        {

                            if (player.Health > 99)
                            {
                                Notify.Error(player, "You don't need a first aid kit");
                                return;
                            }
                            if (player.Health > 50)
                            {
                                var healthco = 100 - player.Health;
                                player.Health += healthco;
                            }
                            else
                            {
                                player.Health += 50;
                            }
                            player.SetData("USE_MEDKIT", DateTime.Now.AddSeconds(10));
                            Main.OnAntiAnim(player);
                            player.PlayAnimation("amb@code_human_wander_texting_fat@female@enter", "enter", 49);
                            NAPI.Task.Run(() => {
                                try
                                {
                                    if (player == null) return;
                                    if (!player.IsInVehicle) player.PlayAnimation("rcmcollect_paperleadinout@", "kneeling_arrest_get_up", 33);
                                    else player.SetData("ToResetAnimPhone", true);
                                    Main.OffAntiAnim(player);
                                    Trigger.ClientEvent(player, "stopScreenEffect", "PPFilter");
                                }
                                catch { }
                            }, 5000);
                            Commands.RPChat("me", player, $"used first aid kit");
                        }
                        else
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Try to use later", 3000);
                            return;
                        }
                        break;
                    case ItemType.Bint:
                        if (!player.HasData("USE_MEDKIT") || DateTime.Now > player.GetData<DateTime>("USE_MEDKIT"))
                        {
                            if (player.Health > 75)
                            {
                                Notify.Error(player, "You cannot use a bandage");
                                return;
                            }
                            player.Health += 25;
                            player.SetData("USE_MEDKIT", DateTime.Now.AddSeconds(10));
                            Main.OnAntiAnim(player);
                            player.PlayAnimation("amb@code_human_wander_texting_fat@female@enter", "enter", 49);
                            NAPI.Task.Run(() => {
                                try
                                {
                                    if (player == null) return;
                                    if (!player.IsInVehicle) player.PlayAnimation("rcmcollect_paperleadinout@", "kneeling_arrest_get_up", 33);
                                    else player.SetData("ToResetAnimPhone", true);
                                    Main.OffAntiAnim(player);
                                    Trigger.ClientEvent(player, "stopScreenEffect", "PPFilter");
                                }
                                catch { }
                            }, 5000);
                            Commands.RPChat("me", player, $"использовал(а) бинт");
                        }
                        else
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Please try later", 3000);
                            return;
                        }
                        break;
                    case ItemType.GasCan:
                        if (player.IsInVehicle) return;
                        //if (item.IsActive == true) return;
                        var GasCanItem = nInventory.Find(Main.Players[player].UUID, ItemType.GasCan);
                        if (GasCanItem.IsActive)
                        {
                            GasCanItem.IsActive = false;
                            BasicSync.DetachObject(player);
                        }
                        else
                        {
                            GasCanItem.IsActive = true;
                            BasicSync.AttachObjectToPlayer(player, 786272259, 57005, new Vector3(0.55, -0.1, 0), new Vector3(-90, -10, 80));
                        }
                        return;
                    case ItemType.Dildo:
                        if (player.IsInVehicle) return;
                        //if (item.IsActive == true) return;
                        if (player.GetData<bool>("PLAYER_HAS_REDDILDO") == true || player.GetData<bool>("PLAYER_HAS_BLACKDILDO") == true) return;
                        var Dildo = nInventory.Find(Main.Players[player].UUID, ItemType.Dildo);
                        if (Dildo.IsActive)
                        {
                            Dildo.IsActive = false;
                            player.SetSharedData("PLAYER_HAS_DILDO", false);
                            player.SetData("PLAYER_HAS_DILDO", false);
                            player.SetData("PLAYER_HAS_PINKDILDO", false);
                            BasicSync.DetachObject(player);
                        }
                        else
                        {
                            Dildo.IsActive = true;
                            player.SetSharedData("PLAYER_HAS_DILDO", true);
                            player.SetData("PLAYER_HAS_DILDO", true);
                            player.SetData("PLAYER_HAS_PINKDILDO", true);
                            BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_pinkdildo"), 57005, new Vector3(0.15, 0.09, 0), new Vector3(-70, 0, 0));
                        }
                        return;
                    case ItemType.DildoBl:
                        if (player.IsInVehicle) return;
                        //if (item.IsActive == true) return;
                        if (player.GetData<bool>("PLAYER_HAS_REDDILDO") == true || player.GetData<bool>("PLAYER_HAS_PINKDILDO") == true) return;
                        var DildoBl = nInventory.Find(Main.Players[player].UUID, ItemType.DildoBl);
                        if (DildoBl.IsActive)
                        {
                            DildoBl.IsActive = false;
                            player.SetSharedData("PLAYER_HAS_DILDO", false);
                            player.SetData("PLAYER_HAS_DILDO", false);
                            player.SetData("PLAYER_HAS_BLACKDILDO", false);
                            BasicSync.DetachObject(player);
                        }
                        else
                        {
                            DildoBl.IsActive = true;
                            player.SetSharedData("PLAYER_HAS_DILDO", true);
                            player.SetData("PLAYER_HAS_DILDO", true);
                            player.SetData("PLAYER_HAS_BLACKDILDO", true);
                            BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_darkdildo"), 57005, new Vector3(0.15, 0.09, 0), new Vector3(-70, 0, 0));
                        }
                        return;
                    case ItemType.DildoRed:
                        if (player.IsInVehicle) return;
                        if (player.GetData<bool>("PLAYER_HAS_BLACKDILDO") == true || player.GetData<bool>("PLAYER_HAS_PINKDILDO") == true) return;
                        var DildoRed = nInventory.Find(Main.Players[player].UUID, ItemType.DildoRed);
                        if (DildoRed.IsActive)
                        {
                            DildoRed.IsActive = false;
                            player.SetSharedData("PLAYER_HAS_DILDO", false);
                            player.SetData("PLAYER_HAS_DILDO", false);
                            player.SetData("PLAYER_HAS_REDDILDO", false);
                            BasicSync.DetachObject(player);
                        }
                        else
                        {
                            DildoRed.IsActive = true;
                            player.SetSharedData("PLAYER_HAS_DILDO", true);
                            player.SetData("PLAYER_HAS_DILDO", true);
                            player.SetData("PLAYER_HAS_REDDILDO", true);
                            BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_reddildo"), 57005, new Vector3(0.15, 0.09, 0), new Vector3(-70, 0, 0));
                        }
                        return;
                    case ItemType.Snowboard1:
                        if (player.IsInVehicle) return;
                        if (Entertainment.NewYear.ActiveXMAS == false)
                        {
                            Notify.Error(player, "Snowboarding is only allowed in winter");
                            return;
                        }
                        var itemsnowboard1 = nInventory.Find(Main.Players[player].UUID, ItemType.Snowboard1);
                        if (itemsnowboard1.IsActive)
                        {
                            if (player.HasSharedData("SnowboardAttached") && player.GetSharedData<bool>("SnowboardAttached") == true)
                            {
                                BasicSync.DetachObject(player);
                                player.SetSharedData("SnowboardAttached", false);
                                player.SetData("SnowboardAttached", false);
                                itemsnowboard1.IsActive = false;
                                player.PlayAnimation("rcmcollect_paperleadinout@", "kneeling_arrest_get_up", 33);
                                return;
                            }
                            itemsnowboard1.IsActive = false;
                            player.ResetData("snowboard_type");
                            player.SetSharedData("SnowboardAttached", false);
                            player.SetData("SnowboardAttached", false);
                            Trigger.ClientEvent(player, "client:stopSnowBoard");
                        }
                        else
                        {
                            itemsnowboard1.IsActive = true;
                            player.SetSharedData("SnowboardAttached", true);
                            player.SetData("SnowboardAttached", true);
                            player.PlayAnimation("weapons@heavy@minigun", "action_walk", 49);
                            player.SetData("snowboard_type", 1);
                            BasicSync.AttachObjectToPlayer(player, nInventory.ItemModels[ItemType.Snowboard1], 57005, new Vector3(0.2f, 0.1f, 0.05f), new Vector3(205f, 90f, 0.0f));
                        }
                        return;
                    case ItemType.Balon:
                        if (Main.Players[player].FractionID == 1 || Main.Players[player].FractionID == 2 || Main.Players[player].FractionID == 3 || Main.Players[player].FractionID == 4 || Main.Players[player].FractionID == 5)
                        {
                            if (GraffitiWar.isWar)
                            {
                                if (player.HasData("graffiti"))
                                {
                                    player.PlayAnimation("anim@amb@business@weed@weed_inspecting_lo_med_hi@", "weed_spraybottle_stand_spraying_02_inspectorfemale", 1);
                                    Core.BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_cs_spray_can"), 6286, new Vector3(0.06, 0.01, -0.02), new Vector3(80, -10, 110));
                                    Main.OnAntiAnim(player);
                                    NAPI.Task.Run(() => { try { Main.OffAntiAnim(player); player.StopAnimation(); Core.BasicSync.DetachObject(player); Notify.Succ(player, "Вы успешно перекрасили граффити.", 3000); player.GetData<Graffiti>("graffiti").SetGang(Main.Players[player].FractionID); } catch { } }, 9000);
                                    nInventory.Remove(player, new nItem(ItemType.Balon, 1));

                                }
                                else
                                {
                                    Notify.Error(player, "Рядом нет граффити.", 3000);
                                    return;
                                }
                            }
                            else
                            {
                                Notify.Error(player, "Война за граффити еще не началась.", 3000);
                                return;
                            }
                        }
                        else
                        {
                            Notify.Error(player, "You are not in gangs.", 3000);
                            return;
                        }
                        return;
                    case ItemType.Number:
                        var vehicle2 = VehicleManager.getNearestVehicle(player, 3);
                        if (player.IsInVehicle)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вам нужно выйти из автомобиля", 3000);
                            return;
                        }
                        if (vehicle2 == null)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы должны быть рядом с машиной", 3000);
                            return;
                        }
                        if (vehicle2.HasData("ACCESS") && vehicle2.GetData<string>("ACCESS") == "FRACTION" || vehicle2.NumberPlate == "ADMIN")
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Это не ваша машина.", 3000);
                            return;
                        }
                        if (VehicleManager.Vehicles[vehicle2.NumberPlate].Сasco == 1)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "На этом ТС уже установлен номер", 3000);
                            return;
                        }
                        var numbers = vehicle2.NumberPlate;
                        var holder = VehicleManager.Vehicles[numbers].Holder;
                        if (player.Name != holder)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Это не ваша машина.", 3000);
                            return;
                        }
                        var Number = item.Data;
                        vehicle2.NumberPlate = Number;
                        var vData = VehicleManager.Vehicles[numbers];
                        VehicleManager.Vehicles.Remove(numbers);
                        VehicleManager.Vehicles.Add(Number, vData);
                        VehicleManager.Vehicles[Number].Сasco = 1;
                        MySQL.Query($"UPDATE vehicles SET number='{Number}' WHERE number='{numbers}'");
                        VehicleManager.Save(Number);
                        // VehicleManager.Vehicles[numbers].Сasco = 1;
                        nInventory.Remove(player, ItemType.Number, 1);
                        GUI.Dashboard.Close(player);
                        return;
                }
                nInventory.Remove(player, item.Type, 1);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы использовали {nInventory.ItemsNames[item.ID]}", 3000);
                GameLog.Items($"player({Main.Players[player].UUID})", "use", Convert.ToInt32(item.Type), 1, $"{item.Data}");
                //GUI.Dashboard.Close(player);
            }
            catch (Exception e)
            {
                Log.Write($"EXCEPTION AT\"ITEM_USE\"/{item.Type}/{index}/{player.Name}/:\n" + e.ToString(), nLog.Type.Error);
            }
        }
        [RemoteEvent("Unactivesnowboarditem")]
        public static void Unactivesnowboarditem(Player player)
        {
            var itemsnowboard1 = nInventory.Find(Main.Players[player].UUID, ItemType.Snowboard1);
            itemsnowboard1.IsActive = false;
            player.SetSharedData("SnowboardAttached", false);
            player.SetData("SnowboardAttached", false);
        }

        private static void healTimer(Player player)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (player.Health == 100)
                    {
                        //Main.StopT(player.GetData("HEAL_TIMER"), "timer_10");
                        Timers.Stop(player.GetData<string>("HEAL_TIMER_BONG"));
                        player.ResetData("HEAL_TIMER_BONG");
                        Trigger.ClientEvent(player, "stopScreenEffect", "PPFilter");
                        return;
                    }
                    player.Health = player.Health + 1;
                }
                catch { }
            });
        }

        // TO DELETE
        private static List<int> TypesCounts = new List<int>()
        {
            5, 10, 15, 3000, 5000, 10000
        };
        private static List<Tuple<int, int>> PresentsTypes = new List<Tuple<int, int>>()
        {
            new Tuple<int, int>(0, 5),
            new Tuple<int, int>(1, 4),
            new Tuple<int, int>(2, 3),
            new Tuple<int, int>(5, 0),
            new Tuple<int, int>(4, 1),
            new Tuple<int, int>(3, 2),
        };
        //
        public static void onDrop(Player player, nItem item, dynamic data)
        {
            try
            {
                if (data != null && (int)data != 1)
                    Commands.RPChat("me", player, $"выбросил(а) {nInventory.ItemsNames[(int)item.Type]}");

                GameLog.Items($"player({Main.Players[player].UUID})", "ground", Convert.ToInt32(item.Type), 1, $"{item.Data}");

                if (!nInventory.ClothesItems.Contains(item.Type) && !nInventory.WeaponsItems.Contains(item.Type) && item.Type != ItemType.CarKey && item.Type != ItemType.KeyRing)
                {
                    foreach (var o in NAPI.Pools.GetAllObjects())
                    {
                        if (player.Position.DistanceTo(o.Position) > 2) continue;
                        if (!o.HasSharedData("TYPE") || o.GetSharedData<string>("TYPE") != "DROPPED" || !o.HasData("ITEM")) continue;
                        nItem oItem = o.GetData<nItem>("ITEM");
                        if (oItem.Type == item.Type)
                        {
                            oItem.Count += item.Count;
                            o.SetData("ITEM", oItem);
                            o.SetData("WILL_DELETE", DateTime.Now.AddMinutes(2));
                            return;
                        }
                    }
                }
                item.IsActive = false;


                var xrnd = rnd.NextDouble();
                var yrnd = rnd.NextDouble();
                var obj = NAPI.Object.CreateObject(nInventory.ItemModels[item.Type], player.Position + nInventory.ItemsPosOffset[item.Type] + new Vector3(xrnd, yrnd, 0), player.Rotation + nInventory.ItemsRotOffset[item.Type], 255, player.Dimension);
                obj.SetSharedData("TYPE", "DROPPED");
                obj.SetSharedData("PICKEDT", false);
                obj.SetData("ITEM", item);
                var id = rnd.Next(100000, 999999);
                while (ItemsDropped.Contains(id)) id = rnd.Next(100000, 999999);
                obj.SetData("ID", id);
                //obj.SetData("DELETETIMER", Main.StartT(14400000, 99999999, (o) => deleteObject(obj), "ODELETE_TIMER"));
                obj.SetData("DELETETIMER", Timers.StartOnce(14400000, () => deleteObject(obj)));
            }
            catch (Exception e) { Log.Write("onDrop: " + e.Message, nLog.Type.Error); }
        }
        public static void onTransfer(Player player, nItem item, dynamic data)
        {
            //
        }
    }
}
