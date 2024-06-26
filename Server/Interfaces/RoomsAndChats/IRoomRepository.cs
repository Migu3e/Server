using Server.MongoDB;

namespace Server.Interfaces.RoomsAndChats;

public interface IRoomRepository
{
    Task<List<RoomDB>> GetAllRooms();
    Task InsertRoom(RoomDB room);
    Task UpdateRoomMessages(string roomName, string message);
    Task DeleteRoom(string roomName);
    Task<RoomDB> GetRoomByName(string roomName);
}