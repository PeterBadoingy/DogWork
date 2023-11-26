// DogScript By Peter Badoingy and Chat GPT.
using System;
using System.Linq;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;
using GTA.UI;
using static DogScript;
using Hash = GTA.Native.Hash;

public class DogScript : Script
{
    public Dog dog;
    public bool isDogInVehicle = false;
    public DogScript()
    {
        dog = new Dog(this);
        Tick += OnTick;
        Interval = (100);
        KeyDown += OnKeyDown;
    }

    public void OnTick(object sender, EventArgs e)
    {
        if (dog.IsSpawned())
        {
            Vehicle playerVehicle = Game.Player.Character.CurrentVehicle;

            if (playerVehicle != null && playerVehicle.Exists())
            {
                // Check if the player's vehicle is not a motorcycle or bike
                if (!IsMotorcycleOrBike(playerVehicle))
                {
                    if (!isDogInVehicle)
                    {
                        // Dog is not in the vehicle, so command it to enter as a passenger
                        dog.EnterVehicle(playerVehicle, VehicleSeat.Passenger);
                        isDogInVehicle = true;
                    }
                }
                else
                {
                   // if (isDogInVehicle)
                    {
                        // Dog is in the vehicle, so command it to exit
                       // dog.ExitVehicle();
                        //isDogInVehicle = false;
                    }
                }
            }
            else
            {
                if (isDogInVehicle)
                {
                    // Dog is in the vehicle, so command it to exit
                    dog.ExitVehicle();
                    isDogInVehicle = false;
                }
            }
        }
        if (dog.HasBeenDamagedByNPC(Game.Player.Character))
        {
            // Player is being attacked by an NPC, make the dog aggressive
            dog.Attack();
        }
        else
        {
            // Handle other logic if needed
        }
    }

    public bool IsMotorcycleOrBike(Vehicle vehicle)
    {
        // List of model names for motorcycles and bicycles
        Model[] motorcycleAndBicycleModels =
        {
        new Model("bati"),
        new Model("hakuchou"),
        new Model("akuma"),
        new Model("lectro"),
        new Model("nemesis"),
        new Model("pcj"),
        new Model("ruffian"),
        new Model("sanchez"),
        new Model("shotaro"),
        new Model("thrust"),
        new Model("vader"),
        new Model("carbonrs"),
        new Model("cliffhanger"),
        new Model("daemon"),
        new Model("defiler"),
        new Model("esskey"),
        new Model("faggio"),
        new Model("fcr"),
        new Model("gargoyle"),
        new Model("innovation"),
        new Model("lurcher"),
        new Model("manchez"),
        new Model("nightblade"),
        new Model("ratbike"),
        new Model("rrocket"),
        new Model("shotaro"),
        new Model("sovereign"),
        new Model("stryder"),
        new Model("thrust"),
        new Model("vader"),
        new Model("vindicator"),
        new Model("wolfsbane"),
        new Model("enduro"),
        new Model("esskey"),
        new Model("faggio"),
        new Model("faggio2"),
        new Model("faggio3"),
        new Model("hakuchou2"),
        new Model("manchez2"),
        new Model("nemesis"),
        new Model("ruffian"),
        new Model("rrocket"),
        new Model("sanchez"),
        new Model("shotaro"),
        new Model("vindicator"),
        new Model("bmx"),
        new Model("cruiser"),
        new Model("fixter"),
        new Model("scorcher"),
        new Model("tribike"),
        new Model("tribike2"),
        new Model("tribike3")
        // Add more models as needed
    };

        // Check if the vehicle's model or class is in the list of motorcycles and bicycles
        return Array.Exists(motorcycleAndBicycleModels, model => model == vehicle.Model) ||
               (vehicle.ClassType == VehicleClass.Cycles || vehicle.ClassType == VehicleClass.Motorcycles);
    }

    public void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Scroll)
        {
            if (!dog.IsSpawned())
            {
                dog.Spawn();
                //UI.Notify("Dog Spawned!", true, true);
            }
            else
            {
                dog.Delete();
                //UI.Notify("Dog Removed!",true, true);
            }
        }
    }

    // Dog.cs
    public class Dog
    {
        public Ped dog;
        public bool isFollowing = false;
        public bool isSitting = false;
        public bool isAttacking = false;
        public bool isLayingDown = false;
        public bool isDogInVehicle = false;

        public DogScript dogScript;
        public Dog(DogScript script)
        {
            dogScript = script;
            dogScript.Tick += OnTick;
        }
        public void OnTick(object sender, EventArgs e)
        {
            Update();

        }
        public bool IsSpawned()
        {
            return dog != null && dog.Exists();
        }
        public void Update()
        {
            CheckGestures();
            // Add more functionality as needed
        }
        public void Spawn()
        {
            // Implement the spawn logic here
            if (dog == null)
            {
                // Example: Create a new dog ped near the player's position
                Vector3 playerPosition = Game.Player.Character.Position;
                Vector3 spawnOffset = Game.Player.Character.ForwardVector * -2.0f; // Adjust the offset forward
                Vector3 spawnPosition = playerPosition + spawnOffset;

                dog = World.CreatePed(PedHash.Rottweiler, spawnPosition);

                // Set dog attributes
                SetDogAttributes();
                ClearDogTasks();

                // Create a blip for the dog
                Blip dogBlip = dog.AddBlip();
                dogBlip.Sprite = BlipSprite.Chop; // You can set a custom sprite for the blip if needed
                dogBlip.Name = "Dog";
                dogBlip.Scale = 0.5f;

                // The dog will not follow or attack initially
                isFollowing = false;
                isSitting = false;
                isAttacking = false;
                isLayingDown = false;
            }
        }
        public void SetDogAttributes()
        {
            if (dog != null)
            {
                // Set health and armor
                dog.Health = 1000;
                dog.Armor = 1000;

                // Set invincibility
                Function.Call(Hash.SET_ENTITY_INVINCIBLE, dog.Handle, true);

                // Set no ragdoll and no player impacts
                Function.Call(Hash.SET_PED_CAN_RAGDOLL, dog.Handle, false);
                Function.Call(Hash.SET_PED_CAN_RAGDOLL_FROM_PLAYER_IMPACT, dog.Handle, false);

                // Set the relationship group hash of the player's character
                int playerRelationshipGroup = Function.Call<int>(Hash.GET_PED_RELATIONSHIP_GROUP_HASH, Game.Player.Character.Handle);

                // Set the dog's relationship group to that of the player
                Function.Call(Hash.SET_PED_RELATIONSHIP_GROUP_HASH, dog.Handle, playerRelationshipGroup);

                // Set flee attributes
                Function.Call(Hash.SET_PED_FLEE_ATTRIBUTES, dog.Handle, 512, false);

                // Set blocking of non-temporary events
                Function.Call(Hash.SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, dog.Handle, true);
                Function.Call(Hash.SET_PED_CAN_PLAY_AMBIENT_ANIMS, dog.Handle, true);
                Function.Call(Hash.SET_PED_CAN_PLAY_GESTURE_ANIMS, dog.Handle, true);
                Function.Call(Hash.SET_PED_SUFFERS_CRITICAL_HITS, dog.Handle, false);
            }
        }
        public void Delete()
        {
            // Implement the delete logic here
            if (dog != null)
            {
                dog.Delete();
                dog = null;
            }
        }
        public void EnterVehicle(Vehicle vehicle, VehicleSeat seat)
        {
            if (dog != null && !dog.IsInVehicle())
            {
                // Warp the dog into the vehicle
                Function.Call(Hash.TASK_WARP_PED_INTO_VEHICLE, dog.Handle, vehicle.Handle, (int)seat);

                //Notification.Show("Dog is in a vehicle!");
                Script.Wait(2000);
                SitInVehicle();
                // Wait for the dog to enter the vehicle
                while (!dog.IsInVehicle() && Function.Call<int>(Hash.GET_SCRIPT_TASK_STATUS, dog.Handle, 0) != 7)
                {
                    Script.Wait(1000);
                }

                // Ensure the dog is in the correct vehicle seat
                if (dog.IsInVehicle() && GetPedVehicleSeatIndex(dog.Handle) == (int)seat)
                {
                    isSitting = true;
                }
            }
        }

        public void ExitVehicle()
        {
            if (dog != null && dog.IsInVehicle())
            {
                // Teleport the dog outside the vehicle
                Vector3 exitPosition = dog.CurrentVehicle.Position + dog.CurrentVehicle.RightVector * 2.0f; // Adjust the exit distance as needed
                dog.Position = exitPosition;

                // Wait for a brief moment
                ClearDogTasks();
                // Reset the isSitting state when the dog exits the vehicle
                isSitting = false;
            }
        }
        public int GetPedVehicleSeatIndex(int pedHandle)
        {
            // Function to get the seat index of a ped in a vehicle
            return Function.Call<int>(Hash.GET_PED_IN_VEHICLE_SEAT, pedHandle);
        }
        public void FollowPlayer()
        {
            if (!isFollowing && dog != null)
            {
                StartFollowing();
                StopSitting();
                StopAttacking();
                StopLayingDown();
            }
        }

        public void SitInVehicle()
        {
            if (!isSitting && dog != null && dog.Exists())
            {
                // Dog is in a vehicle, play the "sit_in_vehicle" animation
                PlayDogAnimation("creatures@rottweiler@in_vehicle@low_car", "sit");

                isSitting = true;
                isFollowing = false;
                isAttacking = false;
                isLayingDown = false;
            }
        }
        public void Sit()
        {
            if (!isSitting && dog != null && dog.Exists())
            {
                if (!dog.IsInVehicle())
                {
                    // Dog is not in a vehicle, play the regular "sit" animation
                    PlayDogAnimation("creatures@rottweiler@amb@world_dog_sitting@base", "base");

                    isSitting = true;
                    isFollowing = false;
                    isAttacking = false;
                    isLayingDown = false;
                }
                else
                {
                    // Dog is in a vehicle, play the "sit_in_vehicle" animation
                    SitInVehicle();
                }
            }
        }
        public void StopSitting()
        {
            // Implement the logic to stop the dog from sitting
            if (isSitting && dog != null && dog.Exists())
            {
                // Make the dog stand
                ClearDogTasks();
                isSitting = false;
            }
        }

        public void LayDown()
        {
            if (!isLayingDown && dog != null)
            {
                StartLayingDown();
                StopFollowing();
                StopSitting();
                StopAttacking();
            }
        }
        public void StartFollowing()
        {
            if (dog != null && dog.Exists())
            {
                // Clear the current tasks to ensure the dog is not performing any other action
                ClearDogTasks();

                // Get the player's position and heading
                Vector3 playerPosition = Game.Player.Character.Position;
                float playerHeading = Game.Player.Character.Heading;

                // Calculate the offset to the left side of the player
                Vector3 offset = Game.Player.Character.ForwardVector * -2.0f; // Adjust the offset to the left

                // Calculate the final position for the dog
                Vector3 dogPosition = playerPosition + offset;

                // Start following the player with the calculated offset and running
                Function.Call(Hash.TASK_FOLLOW_TO_OFFSET_OF_ENTITY, dog.Handle, Game.Player.Character.Handle, offset.X, offset.Y, offset.Z, 3.0f, -1, 8.0f, 1, 1, 1);

                isFollowing = true;
                isSitting = false;
                isAttacking = false;
                isLayingDown = false;
            }
        }
        public void StopFollowing()
        {
            // Implement the logic to stop the dog from following
            if (isFollowing && dog != null && dog.Exists())
            {
                // Stop following
                ClearDogTasks();
                isFollowing = false;
            }
        }

        public void StartLayingDown()
        {
            // Implement the logic to make the dog lay down
            if (dog != null && dog.Exists())
            {
                ClearDogTasks();

                PlayDogAnimation("creatures@rottweiler@amb@sleep_in_kennel@", "sleep_in_kennel");
                isLayingDown = true;
                isSitting = false;
                isFollowing = false;
                isAttacking = false;
            }
        }

        public void StopLayingDown()
        {
            // Implement the logic to stop the dog from laying down
            if (isLayingDown && dog != null && dog.Exists())
            {
                ClearDogTasks();
                isLayingDown = false;
            }
        }
        public void Attack()
        {
            if (!isAttacking)
            {
                // Check if the player commands the attack
                if (CheckForGesture("gesture_bring_it_on", "gestures@m@standing@casual"))
                {
                    StartAttacking();
                    StopFollowing();
                    StopSitting();
                    StopLayingDown();
                }
                else // If no attack gesture, check for automatic attack on aggressive pedestrians
                {
                    if (dog != null && dog.Exists())
                    {
                        Ped currentTarget = GetClosestAggressivePed();

                        if (currentTarget != null)
                        {
                            // Log information about the aggressive NPC
                            // Log($"Found aggressive NPC: {currentTarget.Handle}");

                            // Assign combat task to the dog
                            Function.Call(Hash.TASK_COMBAT_PED, dog.Handle, currentTarget.Handle, 0, 16);
                            isAttacking = true;
                            isFollowing = false;
                            isSitting = false;
                            isLayingDown = false;
                        }
                    }
                }
            }
        }


        private void StartAttacking()
        {
            if (dog != null && dog.Exists())
            {
                ClearDogTasks();

                if (!Function.Call<bool>(Hash.IS_PED_IN_COMBAT, dog.Handle, 0))
                {
                    if (!dog.IsInVehicle() || (dog.IsInVehicle() && !Function.Call<bool>(Hash.GET_PED_STEALTH_MOVEMENT, dog.Handle)))
                    {
                        AttackNearbyPedestrians();
                    }
                }

                isAttacking = true;
                isFollowing = false;
                isSitting = false;
                isLayingDown = false;
            }
        }

        public void AttackNearbyPedestrians()
        {
            Ped closestPed = GetClosestPedestrian();

            if (closestPed != null)
            {
                // Command the dog to attack the closest pedestrian
                Function.Call(Hash.TASK_COMBAT_PED, dog.Handle, closestPed.Handle, 0, 16);
            }
        }

        public Ped GetClosestPedestrian()
        {
            Ped[] nearbyPeds = World.GetNearbyPeds(dog.Position, 75.0f);
            Ped closestPed = null;
            float closestDistance = float.MaxValue;

            foreach (Ped ped in nearbyPeds)
            {
                if (IsValidPedestrianTarget(ped, Game.Player.Character.Handle))
                {
                    float distance = World.GetDistance(dog.Position, ped.Position);

                    if (distance < closestDistance)
                    {
                        closestPed = ped;
                        closestDistance = distance;
                    }
                }
            }

            return closestPed;
        }
        public bool HasBeenDamagedByNPC(Ped player)
        {
            // Check if the player has been damaged by an NPC
            foreach (Ped npc in World.GetNearbyPeds(player.Position, 75.0f))
            {
                if (npc.IsAlive && !npc.IsPlayer && npc.IsHuman)
                {
                    if (player.HasBeenDamagedBy(npc))
                    {
                        //Notification.Show("Got Hit!");
                        return true;
                    }
                }
            }
            return false;
        }
        public bool IsValidPedestrianTarget(Ped ped, int playerHandle)
        {
            return ped.IsAlive && !ped.IsPlayer && ped.IsHuman && ped.Handle != playerHandle;
        }

        public Ped GetClosestAggressivePed()
        {
            Ped[] nearbyPeds = World.GetNearbyPeds(dog.Position, 75.0f);
            Ped closestPed = null;
            float closestDistance = float.MaxValue;

            foreach (Ped ped in nearbyPeds.Where(IsValidAggressivePed))
            {
                float distance = World.GetDistance(dog.Position, ped.Position);
                if (distance < closestDistance)
                {
                    closestPed = ped;
                    closestDistance = distance;
                }
            }

            return closestPed;
        }
        public bool IsPedAggressiveTowardsPlayer(Ped ped)
        {
            // Check if the pedestrian is aggressive towards the player
            return Function.Call<bool>(Hash.IS_PED_IN_COMBAT, ped.Handle, Game.Player.Character.Handle);
        }
        public void AttackAggressivePedestrians()
        {
            if (dog == null || !dog.Exists())
            {
                return;
            }
            Ped[] aggressivePeds = World.GetNearbyPeds(dog.Position, 75.0f);

            foreach (Ped ped in aggressivePeds)
            {
                // Check if the pedestrian is a valid target
                if (ped.IsAlive && !ped.IsPlayer && ped.IsHuman)
                {
                    // Check if the pedestrian is targeting the player
                    bool isAggressive = Function.Call<bool>(Hash.IS_PED_IN_COMBAT, ped.Handle, Game.Player.Character.Handle);

                    if (isAggressive)
                    {
                        // Check if the dog is not already attacking this pedestrian
                        bool isDogNotAttacking = !Function.Call<bool>(Hash.IS_PED_IN_COMBAT, dog.Handle, ped.Handle);

                        if (isDogNotAttacking)
                        {
                            // Log information about the aggressive NPC
                            //Log($"Found aggressive NPC: {ped.Handle}");

                            // Assign combat task to the dog
                            Function.Call(Hash.TASK_COMBAT_PED, dog.Handle, ped.Handle, 0, 16);
                            isAttacking = true;
                            isFollowing = false;
                            isSitting = false;
                            isLayingDown = false;
                        }
                    }
                }
            }

            // Additionally, check if the dog is not attacking any aggressive NPCs and clear tasks if needed
            if (isAttacking)
            {
                Ped currentTarget = GetClosestAggressivePed();

                if (currentTarget == null)
                {
                    // Clear tasks if there are no aggressive NPCs nearby
                    ClearDogTasks();
                    isAttacking = false;
                }
            }
        }
        public bool IsValidAggressivePed(Ped ped)
        {
            return ped.IsAlive && !ped.IsPlayer && ped.IsHuman && IsPedAggressiveTowardsPlayer(ped);
        }
        public void StopAttacking()
        {
            if (isAttacking && dog != null && dog.Exists()) 
            {
                ClearDogTasks();
                isAttacking = false;
            }
        }

        public void ClearDogTasks()
        {
            Function.Call(Hash.CLEAR_PED_TASKS, dog.Handle);
            Script.Wait(1000);  // Adjust the wait time as needed
        }

        public void CheckGestures()
        {
            CheckGesture("gesture_come_here_hard", "gestures@m@standing@casual", FollowPlayer);
            CheckGesture("gesture_hand_down", "gestures@m@standing@casual", Sit);
            CheckGesture("gesture_bring_it_on", "gestures@m@standing@casual", Attack);
            CheckGesture("gesture_bye_soft", "gestures@m@standing@casual", LayDown);
        }
        public void CheckGesture(string gestureName, string animationDictionary, Action action)
        {
            if (CheckForGesture(gestureName, animationDictionary))
            {
                action.Invoke();
            }
        }
        public bool CheckForGesture(string gestureName, string animationDictionary)
        {
            // Implement the logic to check for a specific gesture
            int playerPed = Game.Player.Character.Handle;
            return Function.Call<bool>(Hash.IS_ENTITY_PLAYING_ANIM, playerPed, animationDictionary, gestureName, 3);
        }
        public void PlayDogAnimation(string animationDictionary, string animationName, string taskName = "")
        {
            // Implement the logic to play a dog animation
            if (dog != null)
            {
                PlayAnimationCommon(animationDictionary, animationName, taskName);
            }
        }
        public void PlayAnimationCommon(string animationDictionary, string animationName, string taskName = "")
        {
            RequestAndCheckAnimationDict(animationDictionary);

            // Stop the current task before playing the animation
            Function.Call(Hash.CLEAR_PED_TASKS, dog.Handle);

            Function.Call(Hash.TASK_PLAY_ANIM, dog.Handle, animationDictionary, animationName, 8.0f, -8.0f, -1, 1, 0, false, false, false);

            // If a task name is specified, set the task name for better tracking
            if (!string.IsNullOrEmpty(taskName))
            {
                Function.Call(Hash.TASK_SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, dog.Handle, true);
            }
        }
        public void RequestAndCheckAnimationDict(string animationDictionary)
        {
            RequestAnimationDict(animationDictionary);

            int attempts = 0;
            while (!HasAnimationDictLoaded(animationDictionary) && attempts < 100)
            {
                Function.Call(Hash.WAIT, 50);
                attempts++;
            }

            if (!HasAnimationDictLoaded(animationDictionary))
            {
                // Handle the case where the animation dictionary fails to load
                // GTA.UI.ShowSubtitle($"Failed to load animation dictionary: {animationDictionary}");
            }
        }
        public void RequestAnimationDict(string dict)
        {
            // Implement the logic to request an animation dictionary
            if (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, dict))
            {
                Function.Call(Hash.REQUEST_ANIM_DICT, dict);
            }
        }
        public bool HasAnimationDictLoaded(string dict)
        {
            // Implement the logic to check if an animation dictionary is loaded
            return Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, dict);
        }
    }
}