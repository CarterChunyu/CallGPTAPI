using System.Security.Cryptography;
using System.Text;

namespace GptApi.Helpers
{
    public static class DesCrytoHelper
    {    
        public static string DesDecrypt(string hexString, string desKey, string desIV, string iv)
        {
            string decrypt = hexString;
            try
            {
                var des = new DESCryptoServiceProvider();
                des.Key = Encoding.ASCII.GetBytes(desKey);
                des.IV = Encoding.ASCII.GetBytes(desIV);
                byte[] dataByteArray = Convert.FromBase64String(hexString);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(dataByteArray, 0, dataByteArray.Length);
                        cs.FlushFinalBlock();
                        decrypt = Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                //todo...
            }

            return decrypt;
        }

        public static bool TryDesDecrypt(string hexString, string DesKey, string DesIV, out string original)
        {
            return hexString != (original = DesDecrypt(hexString, DesKey, DesIV, DesIV));
        }
    }
}
