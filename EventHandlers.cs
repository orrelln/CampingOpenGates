using System;
using System.Collections.Generic;
using System.Linq;

using Exiled.API.Features;
using MEC;
using PlayerRoles;
using Exiled.Events.EventArgs.Server;
using Scanner.Structures;
using Exiled.API.Enums;
using Exiled.Events.EventArgs.Player;
using Utils.NonAllocLINQ;
using UnityEngine;
using Exiled.API.Features.Doors;

namespace CampingOpenGates
{
    public class EventHandlers
    {
        private static Plugin plugin;
        private static List<CoroutineHandle> Coroutines = new List<CoroutineHandle> { };
        private static Dictionary<string, int> PlayerScans = new Dictionary<string, int>();
        private static HashSet<DoorType> FrozenDoors = new HashSet<DoorType>();
        public EventHandlers(Plugin P) => plugin = P;


        private static List<Player> GetAlivePlayers()
        {
            List<Player> alivePlayers = new List<Player> { };
            foreach (Player curPlayer in Player.List)
            {
                if (curPlayer.IsAlive && curPlayer.Role.Team != Team.OtherAlive && curPlayer.Role.Team != Team.SCPs)
                {
                    alivePlayers.Add(curPlayer);
                }
            }
            return alivePlayers;
        }

        private static List<Player> GetAliveScps()
        {
            List<Player> aliveScps = new List<Player> { };
            foreach (Player curPlayer in Player.List)
            {
                if (curPlayer.IsAlive && curPlayer.Role.Team == Team.SCPs)
                {
                    aliveScps.Add(curPlayer);
                }
            }
            return aliveScps;
        }

        private static bool CheckPlayersInRoom(List<Player> players, RoomType curRoom)
        {
            foreach(Player curPlayer in players)
            {
                if (curPlayer.CurrentRoom.Type == curRoom && CheckBoundaries(curPlayer.CurrentRoom.Type, curPlayer.Position))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool CheckDoorsOpen(RoomType curRoom)
        {
            if (curRoom == RoomType.Hcz079)
            {
                return plugin.Config.CampRooms[curRoom].Any(DoorType => Door.Get(DoorType).IsOpen == false && !FrozenDoors.Contains(DoorType));
            }
            else
            {
                return plugin.Config.CampRooms[curRoom].All(DoorType => Door.Get(DoorType).IsOpen == false && !FrozenDoors.Contains(DoorType));
            }
        }


        private static void SendCassieMessage(DoorType curDoor, RoomType curRoom)
        {
            CassieMessage DoorPron = plugin.Translation.DoorPronounciation[curDoor];
            CassieMessage RoomPron = plugin.Translation.RoomPronounciation[curRoom];

            if (curDoor == DoorType.GateA || curDoor == DoorType.GateB)
            {
                Cassie.MessageTranslated(
                plugin.Translation.GateMalfunctionMessage.CassieText.Replace("{ROOM}", RoomPron.CassieText).
                Replace("{SECONDS}", plugin.Config.DoorFrozenTime.ToString()),
                plugin.Translation.GateMalfunctionMessage.CaptionText.Replace("{ROOM}", RoomPron.CaptionText).
                Replace("{SECONDS}", plugin.Config.DoorFrozenTime.ToString())
                );
            }
            else
            {
                Cassie.MessageTranslated(
                  plugin.Translation.DoorMalfunctionMessage.CassieText.Replace("{DOORTYPE}", DoorPron.CassieText).
                  Replace("{ROOM}", RoomPron.CassieText).Replace("{SECONDS}", plugin.Config.DoorFrozenTime.ToString()),
                  plugin.Translation.DoorMalfunctionMessage.CaptionText.Replace("{DOORTYPE}", DoorPron.CaptionText).
                  Replace("{ROOM}", RoomPron.CaptionText).Replace("{SECONDS}", plugin.Config.DoorFrozenTime.ToString())
                );
            }
          
        }

        private static void OpenDoors(RoomType room)
        {
            bool sentMessage = false;

            foreach (DoorType curDoor in plugin.Config.CampRooms[room])
            {
                Door CampDoor = Door.Get(curDoor);

                if (CampDoor.IsOpen == true || FrozenDoors.Contains(CampDoor.Type))
                {
                    continue;
                }

                
                CampDoor.IsOpen = true;
                CampDoor.Lock(plugin.Config.DoorFrozenTime, DoorLockType.NoPower);
                FrozenDoors.Add(CampDoor.Type);

                if (!sentMessage)
                {
                    sentMessage = true;
                    SendCassieMessage(curDoor, room);
                }

                if (plugin.Config.CloseDoor)
                {
                    Timing.CallDelayed(plugin.Config.DoorFrozenTime, () =>
                    {
                        CampDoor.IsOpen = false;
                        FrozenDoors.Remove(CampDoor.Type);
                    });
                }
            }
        }
        

        public static void Scan()
        {
            List<Player> alivePlayers = GetAlivePlayers();
            List<Player> aliveScps = GetAliveScps();

            if (alivePlayers.Count <= plugin.Config.NumberOfPlayers)
            {
                foreach (Player curPlayer in alivePlayers)
                {
                    if (CheckBoundaries(curPlayer.CurrentRoom.Type, curPlayer.Position) &&
                        CheckDoorsOpen(curPlayer.CurrentRoom.Type) &&
                        !CheckPlayersInRoom(aliveScps, curPlayer.CurrentRoom.Type))
                    {
                        if (PlayerScans[curPlayer.UserId]++ >= plugin.Config.CampingLimit)
                        {
                            
                            PlayerScans[curPlayer.UserId] = 0;
                            OpenDoors(curPlayer.CurrentRoom.Type);
                        }

                        Log.Debug(curPlayer.CurrentRoom.Type.ToString() + " " + PlayerScans[curPlayer.UserId].ToString());
                    }
                    else
                    {
                        PlayerScans[curPlayer.UserId] = 0;
                    }
                }
            }

            if (plugin.Config.SurfaceScpCheck)
            {
                foreach (Player curScp in aliveScps)
                {
                    if (curScp.CurrentRoom.Type == RoomType.Surface &&
                        CheckDoorsOpen(curScp.CurrentRoom.Type) &&
                        !CheckPlayersInRoom(alivePlayers, curScp.CurrentRoom.Type))
                    {
                        if (PlayerScans[curScp.UserId]++ >= plugin.Config.CampingLimit)
                        {
                            PlayerScans[curScp.UserId] = 0;
                            OpenDoors(curScp.CurrentRoom.Type);
                        }

                        Log.Debug(curScp.CurrentRoom.Type.ToString() + " " + PlayerScans[curScp.UserId].ToString());
                    }
                    else
                    {
                        PlayerScans[curScp.UserId] = 0;
                    }
                }
            }
           
        }

        public IEnumerator<float> ScanLoop()
        {
            for (; ; )
            {
                yield return Timing.WaitForSeconds(1f);
                Scan();
            }
        }

        public void OnRoundStarted()
        {
            Coroutines.Add(Timing.RunCoroutine(ScanLoop()));
            
        }

        public void OnRoundEnded(RoundEndedEventArgs ev)
        {
            foreach (CoroutineHandle CHandle in Coroutines)
            {
                Timing.KillCoroutines(CHandle);
            }
            PlayerScans = new Dictionary<string, int>();
        }

        public void OnSpawned(SpawnedEventArgs ev)
        {
            PlayerScans[ev.Player.UserId] = 0;
        }

        public void OnToggling(TogglingFlashlightEventArgs ev)
        {
            Log.Debug("Room: " + ev.Player.CurrentRoom.Type + " Coords: " + ev.Player.Position.ToString());
            Log.Debug(CheckBoundaries(ev.Player.CurrentRoom.Type, ev.Player.Position));
        }

        public void OnInteractingDoor(InteractingDoorEventArgs ev)
        {
            Log.Debug("Door: " + ev.Door.Type + " Coords: " + ev.Door.Position.ToString());
        }

        public void OnDetonated()
        {
            foreach (CoroutineHandle CHandle in Coroutines)
            {
                Timing.KillCoroutines(CHandle);
            }
        }

        public static bool CheckBoundaries(RoomType roomType, Vector3 player)
        {
            switch(roomType)
            {
                case RoomType.Surface:
                    return true;
                case RoomType.EzGateA:  
                    Vector3 gateA = Door.Get(DoorType.GateA).Position;
                    Vector3 gateAEntranceDoor = Door.List.Where((Door door) => door.Type == DoorType.EntranceDoor).OrderBy((Door door) =>
                                              Vector3.Distance(gateA, door.Position)).FirstOrDefault().Position;
                    return VectorCheck.IsPastInternal(gateAEntranceDoor, gateA, player);
                case RoomType.EzGateB:
                    Vector3 gateB = Door.Get(DoorType.GateB).Position;
                    Vector3 gateBElevator = Door.Get(DoorType.ElevatorGateB).Position;
                    return VectorCheck.IsWithinRange(gateB, gateBElevator, player);
                case RoomType.HczArmory:
                    Vector3 armoryDoor = Door.Get(DoorType.HczArmory).Position;
                    List<Vector3> outerArmoryDoors = Door.List.Where((Door door) => door.Type == DoorType.HeavyContainmentDoor).OrderBy((Door door) => 
                                                     Vector3.Distance(armoryDoor, door.Position)).Take(3).Select(d => d.Position).ToList();
                    Vector3 outerArmoryDoor = VectorCheck.SelectMostDissimilarVector(outerArmoryDoors);
                    return VectorCheck.IsPastInternalWithBoundaries(outerArmoryDoor, armoryDoor, player, -1.85f, 1.85f);
                case RoomType.HczHid:
                    Vector3 hidDoor = Door.Get(DoorType.HID).Position;
                    Vector3 hidLeftDoor = Door.Get(DoorType.HIDLeft).Position;
                    return VectorCheck.IsPastInternalWithBoundaries(hidLeftDoor, hidDoor, player, -2.74f, 2.74f, true);
                case RoomType.Hcz096:
                    Vector3 scp096Door = Door.Get(DoorType.Scp096).Position;
                    Vector3 outerScp096Door = Door.List.Where((Door door) => door.Type == DoorType.HeavyContainmentDoor).OrderBy((Door door) => 
                                              Vector3.Distance(scp096Door, door.Position)).FirstOrDefault().Position;
                    return VectorCheck.IsPastInternal(outerScp096Door, scp096Door, player);
                case RoomType.Hcz106:
                    Vector3 scp106Door = Door.Get(DoorType.Scp106Primary).Position;
                    Vector3 outerScp106Door = Door.List.Where((Door door) => door.Type == DoorType.HeavyContainmentDoor).OrderBy((Door door) => 
                                              Vector3.Distance(scp106Door, door.Position)).FirstOrDefault().Position;
                    return VectorCheck.IsPastInternal(outerScp106Door, scp106Door, player);
                case RoomType.Hcz049:
                    Vector3 scp049ArmoryDoor = Door.Get(DoorType.Scp049Armory).Position;
                    Vector3 scp049ElevatorDoor = Door.Get(DoorType.ElevatorScp049).Position;
                    return VectorCheck.IsPastInternalWithBoundaries(scp049ElevatorDoor, scp049ArmoryDoor, player, -2.1f, 2.1f);
                case RoomType.EzIntercom:
                    Vector3 intercomDoor = Door.Get(DoorType.Intercom).Position;
                    List<Vector3> outerIntercomDoors = Door.List.Where((Door door) => door.Type == DoorType.EntranceDoor).OrderBy((Door door) => 
                                                       Vector3.Distance(intercomDoor, door.Position)).Take(2).Select(d => d.Position).ToList();
                    Vector3 outerIntercomDoor = VectorCheck.SelectMostSimilarVector(intercomDoor, outerIntercomDoors);
                    return VectorCheck.IsPastInternal(outerIntercomDoor, intercomDoor, player) || player.y < -1000;
                case RoomType.Hcz079:
                    Vector3 scp079FirstDoor = Door.Get(DoorType.Scp079First).Position;
                    Vector3 outerScp079Door = Door.List.Where((Door door) => door.Type == DoorType.HeavyContainmentDoor).OrderBy((Door door) =>
                                              Vector3.Distance(scp079FirstDoor, door.Position)).FirstOrDefault().Position;
                    return VectorCheck.IsPastInternalWithRoom(outerScp079Door, scp079FirstDoor, player, 3.0f);
                default:
                    return false;
            }

        }

    }
}