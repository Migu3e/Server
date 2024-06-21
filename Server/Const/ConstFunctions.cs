namespace Server.Const;

public class ConstFunctions
{
    public static string Response(string username,string message)
    {
        return $"<{DateTime.Now} - {username}> {message}\n";
    }
}