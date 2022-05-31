using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using GolemoSDK;

namespace Golemo.Buildings
{
    public static class Dummies
    {
        private static nLog Log = new nLog("Dummies");

        private static Dictionary<string, Tuple<int, int, Vector3, Vector3>> _vehicleDummies = new Dictionary<string, Tuple<int, int, Vector3, Vector3>>();
        private static void VehicleDummy(string vehicleName, Vector3 position, Vector3 rotation, int a = 0, int b = 0)
        {
            if(vehicleName != null && position != null && rotation != null)
            {
                _vehicleDummies.Add(vehicleName, new Tuple<int, int, Vector3, Vector3>(a, b, position, rotation));
            }
        }

        #region Init VehicleList
        public static void OnResourceStart()
        {                //model    //position                  //rotation                        color color
            VehicleDummy("chiron", new Vector3(-789.60864, -239.84534, 37.105966), new Vector3(0, 0, -89.62737), 25, 83);
            VehicleDummy("avtr", new Vector3(-791.80664, -235.15756, 36.105966), new Vector3(0, 0, -94.01049), 83, 21);
            VehicleDummy("g63", new Vector3(-784.04767, -223.88043, 37.301527), new Vector3(0, 0, 137.84833), 120, 120);

            foreach (var item in _vehicleDummies)
            {
                VehicleHash vh = (VehicleHash)NAPI.Util.GetHashKey(item.Key);
                var vehicle = NAPI.Vehicle.CreateVehicle(vh, item.Value.Item3, item.Value.Item4, item.Value.Item1, item.Value.Item2, "DUMMY", 255, true, false, 0);
                vehicle.SetSharedData("ACCESS", "DUMMY");
                Core.SafeZones.CreateSafeZone(vehicle.Position, 10, 10, false); //если вам не нужно Зеленая зона у машины, то удалите эту строку
            }

            Log.Write("Fell asleep " + _vehicleDummies.Count + " exhibition transport", nLog.Type.Info);
        }
        #endregion
    }
}
