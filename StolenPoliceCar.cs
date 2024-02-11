using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FivePD.API.Utils;
using FivePD.API;

namespace StolenPoliceVehicle
{
    [CalloutProperties("Stolen Police Car", "ERLS Team", "1.2")]
    public class StolenPoliceCar : Callout
    {
        private Vehicle car;
        private Ped suspect;
        private Ped suspect2;

        private readonly Random rnd = new Random();

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

        public async override void OnStart(Ped player)
        {
            suspect = await World.CreatePed(RandomUtils.GetRandomPed(), Location + 1);
            suspect.AlwaysKeepTask = true;
            suspect.BlockPermanentEvents = true;
            
            if (SpawnChance())
            {
                suspect2 = await World.CreatePed(RandomUtils.GetRandomPed(), Location + 1);
                suspect2.AlwaysKeepTask = true;
                suspect2.BlockPermanentEvents = true;
                
                // Spawns the car with bot suspects
                car = await World.CreateVehicle(VehicleHash.Police, Location);
                suspect.SetIntoVehicle(car, VehicleSeat.Driver);
                suspect2.SetIntoVehicle(car, VehicleSeat.Passenger);
                
                // Gives suspect 2 the weapon
                suspect2.Weapons.Give(getRandomWeapon(), 100, true, true);
                suspect2.Task.VehicleShootAtPed(player);
                // Makes the Driver flee from the player
                suspect.Task.FleeFrom(player);
                car.IsSirenActive = true;
                
                // Registers the suspect if spawned
                Pursuit.RegisterPursuit(suspect2);
            }
            else
            {
                
                car = await World.CreateVehicle(VehicleHash.Police, Location);
                suspect.SetIntoVehicle(car, VehicleSeat.Driver);
                
                // Sets the suspect to flee 
                API.SetDriveTaskDrivingStyle(suspect.GetHashCode(), 524852);
                car.IsSirenActive = true;
                suspect.Task.FleeFrom(player);

                // Registers suspect 1
                Pursuit.RegisterPursuit(suspect);
            }

            // Attach blips to suspects and the police car
            suspect.AttachBlip();
            car.AttachBlip();
        }

        // Grandpa Rex gave me this <3
        public override void OnCancelAfter()
        {
            base.OnCancelAfter();
            
            try
            {
                if (!suspect.IsAlive || suspect.IsCuffed) return;
                suspect.Task.WanderAround(); suspect.AlwaysKeepTask = false; suspect.BlockPermanentEvents = false;
            }
            catch { EndCallout(); }

            try
            {
                if (!suspect2.IsAlive || suspect2.IsCuffed) return;
                suspect2.Task.WanderAround(); suspect2.AlwaysKeepTask = false; suspect2.BlockPermanentEvents = false;
            }
            catch { EndCallout(); }
            
        }

        // Grandpa Rex gave me this <3
        public override void OnCancelBefore()
        {
            base.OnCancelBefore();

            try
            {
                if (!suspect.IsAlive || suspect.IsCuffed) return;
                suspect.Task.WanderAround(); suspect.AlwaysKeepTask = false; suspect.BlockPermanentEvents = false;
            }
            catch { EndCallout(); }

            try
            {
                if (!suspect2.IsAlive || suspect2.IsCuffed) return;
                suspect2.Task.WanderAround(); suspect2.AlwaysKeepTask = false; suspect2.BlockPermanentEvents = false;
            }
            catch { EndCallout(); }
        }
        
        // Nice Hashing
        private WeaponHash getRandomWeapon()
        {
            List<WeaponHash> weapons = new List<WeaponHash>
            {
                WeaponHash.APPistol
            };
            return weapons[rnd.Next(weapons.Count)];
        }

        public async override Task OnAccept()
        {
            InitBlip();
            UpdateData();
        }
        
        private bool SpawnChance()
        {
            // This is the randomizing amount
            double spawnChance = 0.3;

            // Initialize random number generator
            Random random = new Random();

            // Generate random number between 0 and 1
            double randomValue = random.NextDouble();

            // Check if random number is less than or equal to spawn chance
            return randomValue < spawnChance;
         }
     }
}