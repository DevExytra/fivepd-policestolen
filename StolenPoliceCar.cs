using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FivePD.API.Utils;
using FivePD.API;

namespace TestCallout
{
    [CalloutProperties("Stolen Police Car", "ERLS Team", "1.0")]
    public class StolenPoliceCar : Callout
    {
        private Vehicle car;
        private Ped suspect;
        private Ped suspect2;

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
            suspect = await SpawnPed(RandomUtils.GetRandomPed(), Location + 1);
            suspect.AlwaysKeepTask = true;
            suspect.BlockPermanentEvents = true;
            
            if (SpawnChance())
            {
                suspect2 = await SpawnPed(RandomUtils.GetRandomPed(), Location + 1);
                suspect2.AlwaysKeepTask = true;
                suspect2.BlockPermanentEvents = true;
                
                // Spawns the car with bot suspects
                car = await SpawnVehicle(VehicleHash.Police, Location);
                suspect.SetIntoVehicle(car, VehicleSeat.Driver);
                suspect2.SetIntoVehicle(car, VehicleSeat.Passenger);
                
                // Gives suspect 2 the weapon
                API.GiveWeaponToPed(suspect2.GetHashCode(), 0x22D8FE39, 100, false, true);
                suspect2.Task.VehicleShootAtPed(player);
                // Makes the Driver flee from the player
                suspect.Task.FleeFrom(player);
                
                // Registers the suspect if spawned
                Pursuit.RegisterPursuit(suspect2);
            }
            else
            {
                
                car = await SpawnVehicle(VehicleHash.Police, Location);
                suspect.SetIntoVehicle(car, VehicleSeat.Driver);
                
                // Sets the suspect to flee
                API.SetDriveTaskDrivingStyle(suspect.GetHashCode(), 524852);
                suspect.Task.FleeFrom(player);

                // Registers suspect 1
                Pursuit.RegisterPursuit(suspect);
            }

            // Attach blips to suspects and the police car
            suspect.AttachBlip();
            car.AttachBlip();
        }

        public async override Task OnAccept()
        {
            InitBlip();
            UpdateData();
        }

        private bool SpawnChance()
        {
            double spawnChance = 0.5;

            // Initialize random number generator
            Random random = new Random();

            // Generate random number between 0 and 1
            double randomValue = random.NextDouble();

            // Check if random number is less than or equal to spawn chance
            return randomValue < spawnChance; // Use < instead of <=
        }
        
    }
}