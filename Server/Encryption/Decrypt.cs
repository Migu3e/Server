using System.Security.Cryptography;
using System.Text;

namespace Chat.Encryption;

public class Decrypt
{
    public string decrypt(string password)
    {
        string finalResult;
        byte[] data = Convert.FromBase64String(password);
        string hash = "f0xlern";
        using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
        {
            byte[] keys = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(hash));
            using (TripleDESCryptoServiceProvider tripDes = new TripleDESCryptoServiceProvider() {Key = keys, Mode =CipherMode.ECB,Padding = PaddingMode.PKCS7})
            {
                ICryptoTransform transform = tripDes.CreateDecryptor();
                byte[] result = transform.TransformFinalBlock(data, 0, data.Length);
                finalResult = UTF8Encoding.UTF8.GetString(result);
            }
        }

        return finalResult;
    }
}