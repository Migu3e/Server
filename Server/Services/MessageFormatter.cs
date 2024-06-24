    using System.Text;
    using Server.Const;
    using Server.Interfaces;

    public class MessageFormatter : IMessageFormatter
    {
        public string Response(string username, string message)
        {
            return $"<{DateTime.Now} - {username}> {message}\n";
        }

        public string RoomWasDeleted(string roomName)
        {
            return $"Room '{roomName}' has been deleted\n";
        }

        public string RoomWasCreated(string username, string roomName)
        {
            return $"{username} has created room '{roomName}'\n";
        }

        public string ClientJoinedRoom(string roomName, string clientName)
        {
            return $"{clientName} has joined the room {roomName}\n";
        }

        public string ClientLeftRoom(string roomName, string clientName)
        {
            return $"{clientName} has left the room {roomName}\n";
        }

        public string InviteToRoom(string roomName, string clientName)
        {
            return $"You have invited {clientName} to join room {roomName}.";
        }

        public string InviteToRoomMessege(string roomName, string clientName)
        {
            return $"{clientName} has invited you to join room {roomName}.";
        }

        public string AllClientListMessage(List<string> onlineClients, List<string> offlineClients, string currentUser)
        {
            string clientList = ConstMasseges.ListOfOnline;

            foreach (var client in onlineClients)
            {
                clientList += $"\n<--> {client}" + (client == currentUser ? " (you)" : "");
            }

            clientList += ConstMasseges.ListOfOffline;

            foreach (var client in offlineClients)
            {
                clientList += $"\n<--> {client} (offline)";
            }

            return clientList;
        }

        public string UpdatedClientListMessage(List<string> onlineClients, string newClient)
        {
            string clientList = ConstMasseges.ListOfOnlineClientsChanges;

            foreach (var client in onlineClients)
            {
                clientList += $"<--> {client}" + (client == newClient ? " (Just Joined)\n" : "\n");
            }

            return clientList;
        }

        public string ClientListMessage(List<string> onlineClients, string currentUser)
        {
            string clientList = ConstMasseges.ListOfOnlineClientsAre;

            foreach (var client in onlineClients)
            {
                clientList += $"\n<--> {client}" + (client == currentUser ? " (you)" : "");
            }

            return clientList;
        }

        public string GenerateRoomListMessage(List<IRoom> rooms, string currentUsername)
        {
            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.AppendLine(ConstMasseges.RoomListHeader);

            foreach (var room in rooms.Where(r => !ConstCheckCommands.IsPrivateRoom(r.Name)))
            {
                messageBuilder.AppendLine($"Room {room.Name}");
                foreach (var member in room.Members)
                {
                    string memberLine = $"| <{member.Username}>";
                    if (member.Username == currentUsername)
                    {
                        memberLine += ConstMasseges.YouIndicator;
                    }
                    messageBuilder.AppendLine(memberLine);
                }
                messageBuilder.AppendLine();
            }

            return messageBuilder.ToString();
        }

        public string UserHasEnteredPrivateChat(string username)
        {
            return $"{username} has entered the private chat with you.";
        }

        public string UserJoinedPrivateRoom(string username, string roomName)
        {
            return $"{username} has joined the private room {roomName}.";
        }

        public string RoomDoesNotExist(string targetUsername)
        {
            return $"Room with {targetUsername} doesn't exist.";
        }
    }