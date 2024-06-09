using Server.Interfaces;
using Server.Models;
using System.Collections.Generic;
using System.Linq;

namespace Server.Const
{
    public static class ConstCheckCommands
    {
        public static string CanCreateRoom(string message, List<IRoom> rooms)
        {
            
            var parts = message.Split(' ');

            if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[1]))
            {
                return ConstMasseges.CannotCreateEmptyRoom;
            }

            if (rooms.Any(room => room.Name == parts[1]))
            {
                return ConstMasseges.CannotCreateRoomAlreadyExist;
            }

            return ConstMasseges.RoomWasCreated;
        }

        public static string CanJoinRoom(string message, IRoom? room)
        {
            var parts = message.Split(' ');

            if (parts.Length < 2)
            {
                return ConstMasseges.ErrorEmptyRoomMassage;
            }

            if (room == null)
            {
                return ConstMasseges.TheRoomDosentExist;
            }

            if (room.Name.Contains("|private|"))
            {
                return ConstMasseges.ErrorCannotEnterPrivateRoom;
            }

            return "true";
        }

        public static string CanInviteToRoom(string message, IClient? room)
        {
            var parts = message.Split(' ');

            if (parts.Length < 2 || string.IsNullOrWhiteSpace(parts[1]))
            {
                return ConstMasseges.ErrorEmptyInviteRoomMassage;
            }

            if (room == null)
            {
                return ConstMasseges.TheRoomDosentExist;
            }

            return "true";
        }
        public static string CanDeleteRoom(string message, IRoom? room)
        {
            var parts = message.Split(' ');

            if (parts.Length < 2)
            {
                return ConstMasseges.ErrorEmptyRoomMassage;
            }

            if (room == null)
            {
                return ConstMasseges.TheRoomDosentExist;
            }

            if (room.Name.Contains("|private|"))
            {
                return ConstMasseges.ErrorCannotEnterPrivateRoom;
            }

            return "true";
        }

    }
}