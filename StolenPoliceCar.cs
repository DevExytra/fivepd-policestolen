using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FivePD.API.Utils;
using FivePD.API;

namespace StolenPoliceVehicle
{
    [CalloutProperties("Stolen Emergency Vehicle", "ERLS Team", "1.3")]
    public class StolenPoliceCar : Callout
    {
        private Vehicle _car;
        private Ped _driver;
        private Ped _shooter;
        private readonly Random _rnd = new Random();

        public StolenPoliceCar()
        {
            Random random = new Random();
            float offsetX = random.Next(100, 700);
            float offsetY = random.Next(100, 700);
            InitInfo(World.GetNextPositionOnStreet(Game.PlayerPed.GetOffsetPosition(new Vector3(offsetX, offsetY, 0))));
            ShortName = "Stolen Police Car";
            CalloutDescription = "A Police Car is gone";
            ResponseCode = 3;
            StartDistance = 150f;
        }

        // Gets a random weapon out of this list
        private WeaponHash GetRandomWeapon()
        {
            List<WeaponHash> weapons = new List<WeaponHash>
            {
                WeaponHash.APPistol,
                WeaponHash.Pistol,
                WeaponHash.Pistol50,
                WeaponHash.Revolver
            };
            return weapons[_rnd.Next(weapons.Count)];
        }

        private VehicleHash GetRandomEmergencyVehicleHash()
        {
            List<VehicleHash> vehicle = new List<VehicleHash>
            {
                VehicleHash.Police,
                VehicleHash.Police2,
                VehicleHash.Police3,
                VehicleHash.Police4,
                VehicleHash.Policeb,
                VehicleHash.PoliceOld1,
                VehicleHash.FireTruk,
                VehicleHash.Ambulance
            };
            return vehicle[_rnd.Next(vehicle.Count)];
        }

        // Gets a random driver out of this list
        private PedHash GetRandomDriver()
        {
            List<PedHash> ped = new List<PedHash>
            {
                PedHash.Abigail,
                PedHash.Clay,
                PedHash.Andreas,
                PedHash.Bankman01,
                PedHash.Brad,
                PedHash.Hao,
                PedHash.Barry,
                PedHash.Nigel
            };
            return ped[_rnd.Next(ped.Count)];
        }

        // Gets a random shooter out of this list
        private PedHash GetRandomShooter()
        {
            List<PedHash> ped = new List<PedHash>
            {
                PedHash.AfriAmer01AMM,
                PedHash.Cletus,
                PedHash.Dale,
                PedHash.Franklin,
                PedHash.Floyd,
                PedHash.DoaMan, 
                PedHash.TaoCheng,
                PedHash.Trevor
            };
            return ped[_rnd.Next(ped.Count)];
        }


        public async override void OnStart(Ped player)
        {
            double chance = 0.75;
            bool spawn = SpawnChance(chance);

            if (spawn)
            {
                // Creates The Driver and seats him into the vehicle
                _driver = await World.CreatePed(GetRandomDriver(), Location + 1);
                _driver.AlwaysKeepTask = true;
                _driver.BlockPermanentEvents = true;
                
                // Creates the Shooter
                _shooter = await World.CreatePed(GetRandomShooter(), Location + 1);
                _shooter.Weapons.Give(GetRandomWeapon(), 100, true, true);
                _shooter.Accuracy = 98;
                _shooter.AlwaysKeepTask = true;
                _shooter.BlockPermanentEvents = true;
                
                // Creates the Emergency Vehicle and seats both Suspects in!
                _car = await World.CreateVehicle(GetRandomEmergencyVehicleHash(), Location);
                _driver.SetIntoVehicle(_car, VehicleSeat.Driver);
                _shooter.SetIntoVehicle(_car, VehicleSeat.Passenger);
                _car.IsSirenActive = true;
                
                // Tasks Both Suspects do to something
                API.SetDriveTaskDrivingStyle(_driver.GetHashCode(), 524852);
                _driver.Task.FleeFrom(player);
                _shooter.Task.VehicleShootAtPed(player);
                
                // Attaches the blip to the vehicle
                _car.AttachBlip();
            }
            else
            {
                
                // Creates the Driver
                _driver = await World.CreatePed(GetRandomDriver(), Location + 1);
                _driver.AlwaysKeepTask = true;
                _driver.BlockPermanentEvents = true;
                
                // Creates the Emergency Vehicle and seats the Suspects in!
                _car = await World.CreateVehicle(GetRandomEmergencyVehicleHash(), Location);
                _driver.SetIntoVehicle(_car, VehicleSeat.Driver);
                _car.IsSirenActive = true;
                
                // Tasks the Suspect to flee
                API.SetDriveTaskDrivingStyle(_driver.GetHashCode(), 524852);
                _driver.Task.FleeFrom(player);
                
                // Attaches the Blip to the vehicle
                _car.AttachBlip();
            }
        }
        
        public async override Task OnAccept()
        {
            InitBlip();
            UpdateData();
        }
        
        // Thanks to Grandpa Rex!
        public override void OnCancelAfter()
        {
            base.OnCancelAfter();
            try
            {
                // Clears the driver
                if (!_driver.IsAlive && !_shooter.IsAlive || _driver.IsCuffed && _shooter.IsCuffed) return;
                _driver.Task.WanderAround();
                _driver.AlwaysKeepTask = false;
                _driver.BlockPermanentEvents = false;
                
                // Clears the Shooter
                _shooter.Task.WanderAround();
                _shooter.AlwaysKeepTask = false;
                _shooter.BlockPermanentEvents = false;
            }
            catch
            {
                EndCallout();
            }
        }

        // Thanks to Grandpa Rex!
        public override void OnCancelBefore()
        {
            // Clears the Driver
            base.OnCancelBefore();
            if (!_driver.IsAlive && !_shooter.IsAlive || _driver.IsCuffed && _shooter.IsCuffed) return;
            _driver.Task.WanderAround();
            _driver.AlwaysKeepTask = false;
            _driver.BlockPermanentEvents = false;
            
            // Clears the Shooter
            _shooter.Task.WanderAround();
            _shooter.AlwaysKeepTask = false;
            _shooter.BlockPermanentEvents = false;
        }        

        static bool SpawnChance(double chance)
        {
            Random random = new Random();
            double randomNumber = random.NextDouble();
            return randomNumber < chance;
        }
    }
}