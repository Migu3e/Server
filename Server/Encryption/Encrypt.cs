using System.Security.Cryptography;
using System.Text;

namespace Chat.Encryption;

public class Encrypt
{
    public string encrypt(string password)
    {
        string finalResult;
        byte[] data = UTF8Encoding.UTF8.GetBytes(password);
        string hash = "f0xlern";
        using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
        {
            byte[] keys = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(hash));
            using (TripleDESCryptoServiceProvider tripDes = new TripleDESCryptoServiceProvider() {Key = keys, Mode =CipherMode.ECB,Padding = PaddingMode.PKCS7})
            {
                ICryptoTransform transform = tripDes.CreateEncryptor();
                byte[] result = transform.TransformFinalBlock(data, 0, data.Length);
                finalResult = Convert.ToBase64String(result, 0, result.Length);
            }
        }

        return finalResult;
    }
}