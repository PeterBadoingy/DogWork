using System;
using System.Linq;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;
using GTA.UI;

public class DogScript : Script
{
    public Dog dog;
    public bool isDogInVehicle = false;
    public const float NearbyPedestrianRadius = 35.0f;

    public DogScript()
    {
        dog = new Dog(this);
        Tick += OnTick;
        Interval = 1000;
        KeyDown += OnKeyDown;
    }

    public void OnTick(object sender, EventArgs e)
    {
        if (dog.IsSpawned())
        {
            Vehicle playerVehicle = Game.Player.Character.CurrentVehicle;

            if (playerVehicle != null && playerVehicle.Exists())
            {
                if (!IsMotorcycleOrBike(playerVehicle) && !isDogInVehicle)
                {
                    dog.EnterVehicle(playerVehicle, VehicleSeat.Passenger);
                    isDogInVehicle = true;
                }
            }
            else if (isDogInVehicle)
            {
                dog.ExitVehicle();
                dog.StartFollowing();
                isDogInVehicle = false;
            }
            else if (dog.HasBeenDamagedByNPC(Game.Player.Character))
            {
                dog.Attack();
            }
        }
    }

    public bool IsMotorcycleOrBike(Vehicle vehicle)
    {
        Model[] motorcycleAndBicycleModels =
        {
            new Model("bati"), new Model("hakuchou"), new Model("akuma"), new Model("lectro"),
            new Model("nemesis"), new Model("pcj"), new Model("ruffian"), new Model("sanchez"),
            new Model("shotaro"), new Model("thrust"), new Model("vader"), new Model("carbonrs"),
            new Model("cliffhanger"), new Model("daemon"), new Model("defiler"), new Model("esskey"),
            new Model("faggio"), new Model("fcr"), new Model("gargoyle"), new Model("innovation"),
            new Model("lurcher"), new Model("manchez"), new Model("nightblade"), new Model("ratbike"),
            new Model("rrocket"), new Model("sovereign"), new Model("stryder"), new Model("vindicator"),
            new Model("wolfsbane"), new Model("enduro"), new Model("faggio2"), new Model("faggio3"),
            new Model("hakuchou2"), new Model("manchez2"), new Model("bmx"), new Model("cruiser"),
            new Model("fixter"), new Model("scorcher"), new Model("tribike"), new Model("tribike2"),
            new Model("tribike3")
        };

        return Array.Exists(motorcycleAndBicycleModels, model => model == vehicle.Model) ||
               vehicle.ClassType == VehicleClass.Cycles || vehicle.ClassType == VehicleClass.Motorcycles;
    }

    public void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Scroll)
        {
            if (!dog.IsSpawned())
            {
                dog.Spawn();
            }
            else
            {
                dog.Delete();
            }
        }
    }
}

public class Dog
{
    public enum DogState { None, Following, Sitting, Attacking, LayingDown }

    public Ped dog;
    private DogState currentState = DogState.None;
    public DogScript dogScript;

    public Dog(DogScript script)
    {
        dogScript = script;
        dogScript.Tick += OnTick;
    }

    public void OnTick(object sender, EventArgs e)
    {
        if (dog != null && dog.Exists())
        {
            Update();
        }
    }

    public bool IsSpawned()
    {
        return dog != null && dog.Exists();
    }

    public void Update()
    {
        CheckGestures();
        if (currentState == DogState.Attacking && !Function.Call<bool>(Hash.IS_PED_IN_COMBAT, dog.Handle, 0))
        {
            StartFollowing(); // Reset to following when combat ends
        }
    }

    public void Spawn()
    {
        if (dog == null || !dog.Exists())
        {
            Vector3 playerPosition = Game.Player.Character.Position;
            Vector3 spawnOffset = Game.Player.Character.ForwardVector * -2.0f;
            Vector3 spawnPosition = playerPosition + spawnOffset;

            dog = World.CreatePed(PedHash.Rottweiler2, spawnPosition);
            if (dog != null && dog.Exists())
            {
                SetDogAttributes();

                Blip dogBlip = dog.AddBlip();
                dogBlip.Sprite = BlipSprite.Chop;
                dogBlip.Name = "Dog";
                dogBlip.Scale = 0.5f;

                StartFollowing();
            }
        }
    }

    public void SetDogAttributes()
    {
        if (dog != null && dog.Exists())
        {
            dog.Health = 1000;
            dog.Armor = 1000;
            Function.Call(Hash.SET_ENTITY_INVINCIBLE, dog.Handle, true);
            Function.Call(Hash.SET_PED_CAN_RAGDOLL, dog.Handle, false);
            Function.Call(Hash.SET_PED_CAN_RAGDOLL_FROM_PLAYER_IMPACT, dog.Handle, false);

            int playerRelationshipGroup = Function.Call<int>(Hash.GET_PED_RELATIONSHIP_GROUP_HASH, Game.Player.Character.Handle);
            Function.Call(Hash.SET_PED_RELATIONSHIP_GROUP_HASH, dog.Handle, playerRelationshipGroup);
            Function.Call(Hash.SET_PED_FLEE_ATTRIBUTES, dog.Handle, 512, false);
            Function.Call(Hash.SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, dog.Handle, true);
            Function.Call(Hash.SET_PED_CAN_PLAY_AMBIENT_ANIMS, dog.Handle, true);
            Function.Call(Hash.SET_PED_CAN_PLAY_GESTURE_ANIMS, dog.Handle, true);
            Function.Call(Hash.SET_PED_SUFFERS_CRITICAL_HITS, dog.Handle, false);
        }
    }

    public void Delete()
    {
        if (dog != null && dog.Exists())
        {
            dog.Delete();
            dog = null;
            currentState = DogState.None;
        }
    }

    public void EnterVehicle(Vehicle vehicle, VehicleSeat seat)
    {
        if (dog != null && dog.Exists() && vehicle != null && vehicle.Exists() && !dog.IsInVehicle())
        {
            Function.Call(Hash.TASK_WARP_PED_INTO_VEHICLE, dog.Handle, vehicle.Handle, (int)seat);
            dogScript.isDogInVehicle = true;
            Script.Wait(800);
            Sit();

            int maxAttempts = 10;
            int attempts = 0;
            while (!dog.IsInVehicle() && Function.Call<int>(Hash.GET_SCRIPT_TASK_STATUS, dog.Handle, 0x4B1F1C8F) != 7 && attempts < maxAttempts)
            {
                Script.Wait(800);
                attempts++;
            }

            if (dog.IsInVehicle() && dog.CurrentVehicle == vehicle)
            {
                currentState = DogState.Sitting;
            }
            else
            {
                dogScript.isDogInVehicle = false;
                ClearDogTasks();
                StartFollowing();
            }
        }
    }

    public void ExitVehicle()
    {
        if (dog != null && dog.Exists() && dog.IsInVehicle())
        {
            Vector3 exitPosition = dog.CurrentVehicle.Position + dog.CurrentVehicle.RightVector * 2.0f;
            dog.Position = exitPosition;
            ClearDogTasks();
            dogScript.isDogInVehicle = false;
            StartFollowing();
        }
    }

    public void StartFollowing()
    {
        if (dog != null && dog.Exists() && currentState != DogState.Following)
        {
            ClearDogTasks();
            Vector3 offset = Game.Player.Character.RightVector * 2.0f;
            Function.Call(Hash.TASK_FOLLOW_TO_OFFSET_OF_ENTITY, dog.Handle, Game.Player.Character.Handle, offset.X, offset.Y, offset.Z, 3.0f, -1, 3.0f, true);
            Function.Call(Hash.SET_PED_KEEP_TASK, dog.Handle, true);
            currentState = DogState.Following;
        }
    }

    public void Sit()
    {
        if (dog != null && dog.Exists() && currentState != DogState.Sitting)
        {
            if (!dog.IsInVehicle())
            {
                PlayDogAnimation("creatures@rottweiler@amb@world_dog_sitting@base", "base");
            }
            else
            {
                PlayDogAnimation("creatures@rottweiler@in_vehicle@low_car", "sit");
            }
            currentState = DogState.Sitting;
        }
    }

    public void LayDown()
    {
        if (dog != null && dog.Exists() && currentState != DogState.LayingDown)
        {
            ClearDogTasks();
            PlayDogAnimation("creatures@rottweiler@amb@sleep_in_kennel@", "sleep_in_kennel");
            currentState = DogState.LayingDown;
        }
    }

    public void Attack()
    {
        if (dog != null && dog.Exists() && currentState != DogState.Attacking && !Function.Call<bool>(Hash.IS_PED_IN_COMBAT, dog.Handle, 0))
        {
            if (dog.IsInVehicle())
            {
                ExitVehicle();
            }

            // Prioritize attacking aggressive NPCs (defensive behavior)
            Ped aggressivePed = GetClosestAggressivePed();
            if (aggressivePed != null)
            {
                Function.Call(Hash.TASK_COMBAT_PED, dog.Handle, aggressivePed.Handle, 0, 16);
                Function.Call(Hash.SET_PED_KEEP_TASK, dog.Handle, true);
                currentState = DogState.Attacking;
            }
            // If no aggressive NPCs and gesture is performed, attack nearby pedestrian
            else if (CheckForGesture("gesture_bring_it_on", "gestures@m@standing@casual"))
            {
                Ped closestPed = GetClosestPedestrian();
                if (closestPed != null)
                {
                    Function.Call(Hash.TASK_COMBAT_PED, dog.Handle, closestPed.Handle, 0, 16);
                    Function.Call(Hash.SET_PED_KEEP_TASK, dog.Handle, true);
                    currentState = DogState.Attacking;
                }
            }
        }
    }

    public Ped GetClosestPedestrian()
    {
        if (dog == null || !dog.Exists())
            return null;

        Ped[] nearbyPeds = World.GetNearbyPeds(dog.Position, 55.0f);
        Ped closestPed = null;
        float closestDistance = float.MaxValue;

        foreach (Ped ped in nearbyPeds)
        {
            if (ped != null && ped.Exists() && IsValidPedestrianTarget(ped, Game.Player.Character.Handle))
            {
                float distance = Vector3.Distance(dog.Position, ped.Position);
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
        if (player == null || !player.Exists())
            return false;

        foreach (Ped npc in World.GetNearbyPeds(player.Position, 55.0f))
        {
            if (npc != null && npc.Exists() && IsValidPedestrianTarget(npc, player.Handle) && player.HasBeenDamagedBy(npc))
            {
                return true;
            }
        }
        return false;
    }

    public bool IsValidPedestrianTarget(Ped ped, int playerHandle)
    {
        return ped != null && ped.Exists() && ped.IsAlive && !ped.IsPlayer && ped.IsHuman && ped.Handle != playerHandle;
    }

    public Ped GetClosestAggressivePed()
    {
        if (dog == null || !dog.Exists())
            return null;

        Ped[] nearbyPeds = World.GetNearbyPeds(dog.Position, DogScript.NearbyPedestrianRadius)
            .Where(ped => ped != null && ped.Exists() && IsValidAggressivePed(ped))
            .ToArray();

        if (nearbyPeds.Length > 0)
        {
            Array.Sort(nearbyPeds, (ped1, ped2) =>
            {
                float distance1 = Vector3.Distance(dog.Position, ped1.Position);
                float distance2 = Vector3.Distance(dog.Position, ped2.Position);
                return distance1.CompareTo(distance2);
            });
            return nearbyPeds[0];
        }

        return null;
    }

    public bool IsPedAggressiveTowardsPlayer(Ped ped)
    {
        return ped != null && ped.Exists() && Function.Call<bool>(Hash.IS_PED_IN_COMBAT, ped.Handle, Game.Player.Character.Handle);
    }

    public bool IsValidAggressivePed(Ped ped)
    {
        return ped != null && ped.Exists() && ped.IsAlive && !ped.IsPlayer && ped.IsHuman && IsPedAggressiveTowardsPlayer(ped);
    }

    public void CheckGestures()
    {
        if (dog == null || !dog.Exists())
            return;

        if (CheckForGesture("gesture_come_here_hard", "gestures@m@standing@casual"))
        {
            StartFollowing();
        }
        else if (CheckForGesture("gesture_hand_down", "gestures@m@standing@casual"))
        {
            Sit();
        }
        else if (CheckForGesture("gesture_bring_it_on", "gestures@m@standing@casual"))
        {
            Attack();
        }
        else if (CheckForGesture("gesture_bye_soft", "gestures@m@standing@casual"))
        {
            LayDown();
        }
    }

    public bool CheckForGesture(string gestureName, string animationDictionary)
    {
        if (Game.Player.Character == null || !Game.Player.Character.Exists())
            return false;

        return Function.Call<bool>(Hash.IS_ENTITY_PLAYING_ANIM, Game.Player.Character.Handle, animationDictionary, gestureName, 3);
    }

    public void PlayDogAnimation(string animationDictionary, string animationName, string taskName = "")
    {
        if (dog != null && dog.Exists())
        {
            PlayAnimationCommon(animationDictionary, animationName, taskName);
        }
    }

    public void PlayAnimationCommon(string animationDictionary, string animationName, string taskName = "")
    {
        RequestAndCheckAnimationDict(animationDictionary);
        ClearDogTasks();
        Function.Call(Hash.TASK_PLAY_ANIM, dog.Handle, animationDictionary, animationName, 8.0f, -8.0f, -1, 1, 0.0f, false, false, false);

        if (!string.IsNullOrEmpty(taskName))
        {
            Function.Call(Hash.SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, dog.Handle, true);
        }
    }

    public void RequestAndCheckAnimationDict(string animationDictionary)
    {
        if (string.IsNullOrEmpty(animationDictionary))
            return;

        RequestAnimationDict(animationDictionary);
        int attempts = 0;
        while (!HasAnimationDictLoaded(animationDictionary) && attempts < 100)
        {
            Script.Wait(50);
            attempts++;
        }
    }

    public void RequestAnimationDict(string dict)
    {
        if (!string.IsNullOrEmpty(dict) && !Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, dict))
        {
            Function.Call(Hash.REQUEST_ANIM_DICT, dict);
        }
    }

    public bool HasAnimationDictLoaded(string dict)
    {
        return !string.IsNullOrEmpty(dict) && Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, dict);
    }

    public void ClearDogTasks()
    {
        if (dog != null && dog.Exists())
        {
            Function.Call(Hash.CLEAR_PED_TASKS, dog.Handle);
        }
    }
}