using System;
using System.Collections.Generic;
using System.Text;

namespace GolemoSDK
{
    public enum ItemType
    {
        Mask = -1, // Mask
        Gloves = -3, // Gloves
        Leg = -4, // Panties
        Bag = -5, // Backpack
        Feet = -6, // shoes 
        Jewelry = -7, // Accessory everyone there
        Undershit = -8, // shirts
        BodyArmor = -9, // Armor
        Decals = -10, // Actually, you know what it is
        Top = -11, // Top
        Hat = -12, // hats
        Glasses = -13, // glasses
        Accessories = -14, // Watches/Bracelets

        Debug = 0,
        BagWithMoney = 12,// Сумка с деньгами
        Material = 13,    // Материалы
        Drugs = 14,       // Наркота
        BagWithDrill = 15,// Сумка с дрелью
        HealthKit = 1,    // Аптечка
        GasCan = 2,       // Канистра
        Сrisps = 3,       // Чипсы
        Beer = 4,         // Пиво
        Pizza = 5,        // Пицца
        Burger = 6,       // Бургер
        HotDog = 7,       // Хот-Дог
        Sandwich = 8,     // Сэндвич
        eCola = 9,        // Кока-Кола
        Sprunk = 10,      // Спрайт
        Lockpick = 11,    // Отмычка для замка
        ArmyLockpick = 16,// Военная отмычка
        Pocket = 17,      // Мешок
        Cuffs = 18,       // Стяжки
        CarKey = 19,      // Ключи от личной машины
        Present = 40,     // Подарок
        KeyRing = 41,     // Связка ключей
        RepairKit = 42,     // починка

        /* Drinks */
        RusDrink1 = 20,
        RusDrink2 = 21,
        RusDrink3 = 22,

        YakDrink1 = 23,
        YakDrink2 = 24,
        YakDrink3 = 25,

        LcnDrink1 = 26,
        LcnDrink2 = 27,
        LcnDrink3 = 28,

        ArmDrink1 = 29,
        ArmDrink2 = 30,
        ArmDrink3 = 31,

        /* Weapons */
        /* Pistols */
        Pistol = 100,
        CombatPistol = 101,
        Pistol50 = 102,
        SNSPistol = 103,
        HeavyPistol = 104,
        VintagePistol = 105,
        MarksmanPistol = 106,
        Revolver = 107,
        APPistol = 108,
        FlareGun = 110,
        DoubleAction = 111,
        PistolMk2 = 112,
        SNSPistolMk2 = 113,
        RevolverMk2 = 114,
        /* SMG */
        MicroSMG = 115,
        MachinePistol = 116,
        SMG = 117,
        AssaultSMG = 118,
        CombatPDW = 119,
        MG = 120,
        CombatMG = 121,
        Gusenberg = 122,
        MiniSMG = 123,
        SMGMk2 = 124,
        CombatMGMk2 = 125,
        /* Rifles */
        AssaultRifle = 126,
        CarbineRifle = 127,
        AdvancedRifle = 128,
        SpecialCarbine = 129,
        BullpupRifle = 130,
        CompactRifle = 131,
        AssaultRifleMk2 = 132,
        CarbineRifleMk2 = 133,
        SpecialCarbineMk2 = 134,
        BullpupRifleMk2 = 135,
        /* Sniper */
        SniperRifle = 136,
        HeavySniper = 137,
        MarksmanRifle = 138,
        HeavySniperMk2 = 139,
        MarksmanRifleMk2 = 140,
        /* Shotguns */
        PumpShotgun = 141,
        SawnOffShotgun = 142,
        BullpupShotgun = 143,
        AssaultShotgun = 144,
        Musket = 145,
        HeavyShotgun = 146,
        DoubleBarrelShotgun = 147,
        SweeperShotgun = 148,
        PumpShotgunMk2 = 149,
        /* MELEE WEAPONS */
        StunGun = 109,
        Knife = 180,
        Nightstick = 181,
        Hammer = 182,
        Bat = 183,
        Crowbar = 184,
        GolfClub = 185,
        Bottle = 186,
        Dagger = 187,
        Hatchet = 188,
        KnuckleDuster = 189,
        Machete = 190,
        Flashlight = 191,
        SwitchBlade = 192,
        PoolCue = 193,
        Wrench = 194,
        BattleAxe = 195,
        /* Ammo */
        PistolAmmo = 200,
        PistolAmmo2 = 201,
        PistolAmmo3 = 2201,
        RiflesAmmo = 202,
        SniperAmmo = 204,
        ShotgunsAmmo = 203,
        HeavyRiflesAmmo = 244,
        CarabineAmmo = 245,

        /* Fishing */
        Rod = 205, // Удочка
        RodUpgrade = 206, // Улучшенная удочка
        RodMK2 = 207, // Удочка MK2
        Naz = 208, // Наживка
        Koroska = 209, // Корюшка
        Kyndja = 210, // Кунджа
        Lococ = 211, // Лосось
        Okyn = 212, // Окунь
        Ocetr = 213, // Осётр
        Skat = 214, // Скат
        Tunec = 215, // Тунец
        Ygol = 216, // Угорь
        Amyr = 217, // Чёрный амур
        Chyka = 218, // Щука

        //Farmer Jobs Items
        Hay = 219,
        Seed = 220,

        Binocular = 227,
        GiveBox = 199,
        LSPDDrone = 500,
        Drone = 501,
        CasinoChips = 224,
        Number = 562,
        Balon = 43,
        Documents = 563,
        License = 564,
        Walkie = 565,
        Apteka = 569,
        Bint = 570,

        Pill = 566,
        UsePill = 567,

        Dildo = 568,
        DildoBl = 571,
        DildoRed = 572,
        Bong = 573,
        Lighter = 574,
        RGBBong = 575,
        Programmer = 576,

        Mush1 = 1000,
        Mush2 = 1001,
        Mush3 = 1002,
        Mush4 = 1003,
        Mush5 = 1004,
        Mush6 = 1005,
        MushGold = 1006,

        Snowboard1 = 580,
        Snowboard2 = 581,
        Snowboard3 = 582,
        Snowboard4 = 583,
        Snowboard5 = 584,
        Snowboard6 = 585,
        Snowboard7 = 586,
        Snowboard8 = 587,

        Parachute = 588,
        SnowBall = 258,

        CookieSnow = 603,
        Donut = 604,
        Cocktail = 605,
        Milkshake = 606,
        Salad = 607,
        Shashlik = 608,

        MoneyHeist = 609,
        Boombox = 610,
    }

    public class nItem
    {
        public int ID { get; internal set; }
        public ItemType Type { get; internal set; }
        public int Count { get; set; }
        public bool IsActive { get; set; }
        public dynamic Data;

        public nItem(ItemType type, int count = 1, dynamic data = null, bool isActive = false)
        {
            ID = Convert.ToInt32(type);
            Type = type;
            Count = count;
            Data = data;
            IsActive = isActive;
        }
    }
}
