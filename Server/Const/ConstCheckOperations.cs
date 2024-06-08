namespace Server.Const
{
    public static class ConstCheckOperations
    {
        public static bool IsCommand(string message)
        {
            return message.StartsWith("/");
        }
        public static bool IsList(string message)
        {
            return message == "/list";
        }

        public static bool IsListRooms(string message)
        {
            return message == "/list rooms";
        }

        public static bool IsHelp(string message)
        {
            return message == "/help";
        }

        public static bool IsCreateRoom(string message)
        {
            return message.StartsWith("/croom");
        }

        public static bool IsJoinRoom(string message)
        {
            return message.StartsWith("/jroom");
        }

        public static bool IsInviteRoom(string message)
        {
            return message.StartsWith("/iroom");
        }

        public static bool IsPrivate(string message)
        {
            return message.StartsWith("/private");
        }

        public static bool IsLeave(string message)
        {
            return message == "/leave";
        }

        public static bool IsLogout(string message)
        {
            return message == "logout";
        }
    }
}