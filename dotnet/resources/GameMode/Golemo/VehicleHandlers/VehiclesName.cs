using System.Collections.Generic;

using System.Linq;
using GTANetworkAPI;

namespace Golemo.VehicleHandlers
{
    public static class VehiclesName
    {
        //реальные названия авто
        public static Dictionary<string, string> ModelList = new Dictionary<string, string>()
        {
            { "18rs7", "Audi RS7 2018" },
            { "63gls", "Mercedes-Benz GLS 63" },
            { "2019m5", "BMW M5 F90 2019" },
            { "sto21", "Lamborgini Huracan"},
            { "m760i", "BMW M760I"},
            { "gtam21", "AMG GT 2021"},
            { "senna", "McLaren Senna"},
            { "f812c21", "Ferrari 812 Competizione 2021"},
            { "ikx3mso21", "Elva" },
            { "agerars", "Koenigsegg Agera RS" },
            { "amggt16", "Mercedes-Benz AMG GT" },
            { "astondb11", "Aston Martin DB11" },
            { "bentayga", "Bentley Bentayga" },
            { "bentaygast", "Bentley Bentayga ST" },
            { "bmw730", "BMW Series 7" },
            { "bmwm7", "BMW M7" },
            { "bmwx7", "BMW X7" },
            { "cls63s", "Mercedes-Benz CLS 63" },
            { "cyber", "Tesla Cybertruck" },
            { "e63s", "Mercedes-Benz E63S" },
            { "g63", "Mercedes-Benz G63" },
            { "g63amg6x6", "Mercedes-Benz G63 6x6" },
            { "gle63", "Mercedes-Benz GLE 63" },
            { "gt63s", "Mercedes-Benz GT63S" },
            { "huracan", "Lamborghini Huracan Perfomante" },
            { "i8", "BMW I8" },
            { "jesko20", "Koenigsegg Jesko" },
            { "kiastinger", "Kia Stinger" },
            { "lc200", "Toyota Land Cruiser 200" },
            { "lex570", "Lexus LX570" },
            { "m5comp", "BMW M5 Competition" },
            { "modelx", "Tesla Model X" },
            { "mp1", "McLaren P1" },
            { "rs6", "Audi RS6" },
            { "s63cab", "Mercedes-Benz S63 Cabriolet" },
            { "urus", "Lamborghini Urus" },
            { "x6m", "BMW X6M" },
            { "bmwe28", "BMW E28" },
            { "bmwe34", "BMW E34" },
            { "bmwe36", "BMW E36" },
            { "bmwe38", "BMW E38" },
            { "bmwe39", "BMW E39" },
            { "bmwe46", "BMW E46" },
            { "bmwe70", "BMW E70" },
            { "bmwg05", "BMW G05" },
            { "bmwg07", "BMW G07" },
            { "bmwm2", "BMW M2" },
            { "bmwz4", "BMW Z4" },
            { "camry70", "Toyota Camry V70" },
            { "a45", "Mercedes-Benz A45" },
            { "a90", "Toyota Supra A90" },
            { "350z", "Nissan 350Z" },
            { "m4comp", "BMW M4" },
            { "mark2", "Toyota Mark II" },
            { "rx7", "Mazda RX7" },
            { "s600", "Mercedes-Benz S600" },
            { "skyline", "Nissan Skyline GT-R" },
            { "x5me70", "BMW X5M E70" },
            { "x5g05", "BMW X5 G05" },
            { "w210", "Mercedes-Benz W210" },
            { "tahoe2", "Chevrolet Tahoe Max" },
            { "vclass", "Mercedes-Benz V-Classs" },
            { "avtr", "Mercedes-Benz AVTR"},
            { "bentley20", "Bentley Continental 2020"},
            { "chiron19", "Bugatti Chiron"},
            { "cullinan", "Rolls-Royce Cullinan"},
            { "ghost", "Rolls-Royce Ghost"},
            { "eqg", "Mercedes-Benz EQG"},
            { "msprinter", "Mercedes-Benz Sprinter"},
            { "rs72", "Audi RS72"},
            { "starone", "Polestar One"},
            { "taycan21", "Porsche Taycan Litvin Edition"},
            { "x6m2", "BMW X6M 2021"},
            { "panamera17turbo", "Porsche Panamera Turbo" },
            { "harvinoiidlpi8004", "Countach 2020" },
            { "lhuracant", "Huracan HT" }
              //modelname  //Realname
        };

        public static string GetRealVehicleName(string model)
        {
            if (ModelList.ContainsKey(model))
            {
                return ModelList[model];
            }
            else
            {
                return model;
            }
        }

        public static string GetVehicleModelName(string name)
        {
            if (ModelList.ContainsValue(name))
            {
                return ModelList.FirstOrDefault(x => x.Value == name).Key;
            }
            else
            {
                return name;
            }
        }
        public static string GetRealVehicleNameHash(VehicleHash model)
        {
            if (ModelList2.ContainsKey(model))
            {
                return ModelList2[model];
            }
            else
            {
                return "null";
            }
        }
        public static Dictionary<VehicleHash, string> ModelList2 = new Dictionary<VehicleHash, string>()
        {

              { (VehicleHash)NAPI.Util.GetHashKey("a6"), "Audi A6" },
              { (VehicleHash)NAPI.Util.GetHashKey("a45"), "Mercedes-Benz A45" },
              { (VehicleHash)NAPI.Util.GetHashKey("c32"), "Mercedes-Benz C32" },
              { (VehicleHash)NAPI.Util.GetHashKey("s600w220"), "Mercedes-Benz s600 w220" },
              //modelname  //Realname
        };
    }
}
