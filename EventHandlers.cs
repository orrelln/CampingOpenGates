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

namespace CampingOpenGates
{
    public class EventHandlers
    {
        private static Plugin plugin;
        private static List<CoroutineHandle> Coroutines = new List<CoroutineHandle> { };
        private static Dictionary<string, int> PlayerScans = new Dictionary<string, int>();
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
            List<Player> AliveScps = new List<Player> { };
            foreach (Player CurPlayer in Player.List)
            {
                if (CurPlayer.IsAlive && CurPlayer.Role.Team == Team.SCPs)
                {
                    AliveScps.Add(CurPlayer);
                }
            }
            return AliveScps;
        }

        private static bool CheckScpInRoom(List<Player> scps, RoomType curRoom)
        {
            foreach(Player curScp in scps)
            {
                if (curScp.CurrentRoom.Type == curRoom && CheckBoundaries(curScp.CurrentRoom.Type, curScp.Position))
                {
                    return true;
                }
            }
            return false;
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

        public static void Scan()
        {
            List<Player> alivePlayers = GetAlivePlayers();
            List<Player> aliveScps = GetAliveScps();

            if (alivePlayers.Count > plugin.Config.NumberOfPlayers)
            {
                return;
            }

            foreach (Player curPlayer in alivePlayers)
            {
                if (CheckBoundaries(curPlayer.CurrentRoom.Type, curPlayer.Position) && 
                    plugin.Config.CampRooms[curPlayer.CurrentRoom.Type].All(DoorType => Door.Get(DoorType).IsOpen == false) &&
                    !CheckScpInRoom(aliveScps, curPlayer.CurrentRoom.Type))
                {
                    if (PlayerScans[curPlayer.UserId]++ >= plugin.Config.CampingLimit)
                    {
                        PlayerScans[curPlayer.UserId] = 0;
                        bool sentMessage = false;

                        foreach (DoorType curDoor in plugin.Config.CampRooms[curPlayer.CurrentRoom.Type])
                        {
                            Door CampDoor = Door.Get(curDoor);

                            if (CampDoor.IsOpen == true)
                            {
                                continue;
                            }

                            CampDoor.IsOpen = true;
                            CampDoor.Lock(plugin.Config.DoorFrozenTime, DoorLockType.NoPower);

                            if (!sentMessage)
                            {
                                SendCassieMessage(curDoor, curPlayer.CurrentRoom.Type);
                                sentMessage = true;
                            }

                            if (plugin.Config.CloseDoor)
                            {
                                Timing.CallDelayed(plugin.Config.DoorFrozenTime, () =>
                                {
                                    CampDoor.IsOpen = false;
                                });
                            }
                        }
                    }

                    Log.Debug(curPlayer.CurrentRoom.Type.ToString() + " " + PlayerScans[curPlayer.UserId].ToString());
                }
                else
                {
                    PlayerScans[curPlayer.UserId] = 0;
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
                    Vector3 GateA = Door.Get(DoorType.GateA).Position;
                    Vector3 GateAElevator = Door.Get(DoorType.ElevatorGateA).Position;
                    return VectorCheck.IsWithinRange(GateA, GateAElevator, player);
                case RoomType.EzGateB:
                    Vector3 GateB = Door.Get(DoorType.GateB).Position;
                    Vector3 GateBElevator = Door.Get(DoorType.ElevatorGateB).Position;
                    return VectorCheck.IsWithinRange(GateB, GateBElevator, player);
                case RoomType.HczArmory:
                    Vector3 ArmoryDoor = Door.Get(DoorType.HczArmory).Position;
                    List<Vector3> OuterArmoryDoors = Door.List.Where((Door door) => door.Type == DoorType.HeavyContainmentDoor).OrderBy((Door door) => 
                                                     Vector3.Distance(ArmoryDoor, door.Position)).Take(3).Select(d => d.Position).ToList();
                    Vector3 OuterArmoryDoor = VectorCheck.SelectMostDissimilarVector(OuterArmoryDoors);
                    return VectorCheck.IsPastInternalWithBoundaries(OuterArmoryDoor, ArmoryDoor, player, -1.85f, 1.85f);
                case RoomType.HczHid:
                    Vector3 HidDoor = Door.Get(DoorType.HID).Position;
                    Vector3 HidLeftDoor = Door.Get(DoorType.HIDLeft).Position;
                    return VectorCheck.IsPastInternalWithBoundaries(HidLeftDoor, HidDoor, player, -2.74f, 2.74f, true);
                case RoomType.Hcz096:
                    Vector3 Scp096Door = Door.Get(DoorType.Scp096).Position;
                    Vector3 OuterScp096Door = Door.List.Where((Door door) => door.Type == DoorType.HeavyContainmentDoor).OrderBy((Door door) => 
                                              Vector3.Distance(Scp096Door, door.Position)).FirstOrDefault().Position;
                    return VectorCheck.IsPastInternal(OuterScp096Door, Scp096Door, player);
                case RoomType.Hcz106:
                    Vector3 Scp106Door = Door.Get(DoorType.Scp106Primary).Position;
                    Vector3 OuterScp106Door = Door.List.Where((Door door) => door.Type == DoorType.HeavyContainmentDoor).OrderBy((Door door) => 
                                              Vector3.Distance(Scp106Door, door.Position)).FirstOrDefault().Position;
                    return VectorCheck.IsPastInternal(OuterScp106Door, Scp106Door, player);
                case RoomType.Hcz049:
                    Vector3 Scp049ArmoryDoor = Door.Get(DoorType.Scp049Armory).Position;
                    Vector3 Scp049ElevatorDoor = Door.Get(DoorType.ElevatorScp049).Position;
                    return VectorCheck.IsPastInternalWithBoundaries(Scp049ElevatorDoor, Scp049ArmoryDoor, player, -2.1f, 2.1f);
                case RoomType.EzIntercom:
                    Vector3 IntercomDoor = Door.Get(DoorType.Intercom).Position;
                    List<Vector3> OuterIntercomDoors = Door.List.Where((Door door) => door.Type == DoorType.EntranceDoor).OrderBy((Door door) => 
                                                       Vector3.Distance(IntercomDoor, door.Position)).Take(2).Select(d => d.Position).ToList();
                    Vector3 OuterIntercomDoor = VectorCheck.SelectMostSimilarVector(IntercomDoor, OuterIntercomDoors);
                    return VectorCheck.IsPastInternal(OuterIntercomDoor, IntercomDoor, player) || player.y < -1000;
                default:
                    return false;
            }

        }

    }
}