using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Support
{
    /// <summary>
    /// 加密解密类
    /// </summary>
    public class Encryption
    {
        #region "定义加密字串变量"
        private SymmetricAlgorithm mCSP = new DESCryptoServiceProvider();  //声明对称算法变量
        private const string CIV = "jisdf@45+-03434+adf&^&**112323";  //初始化向量
        private const string CKEY = "&*767dh=-+24!!@"; //密钥（常量）
        #endregion
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="sinput"></param>
        /// <returns></returns>
        public string EncryptString(string sinput)
        {
            ICryptoTransform ct = mCSP.CreateEncryptor(Convert.FromBase64String(CKEY), Convert.FromBase64String(CIV));
            byte[] byts = Encoding.UTF8.GetBytes(sinput);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, ct, CryptoStreamMode.Write);
            cs.Write(byts, 0, byts.Length);
            cs.FlushFinalBlock();
            cs.Close();
            string sfinastr = Convert.ToBase64String(ms.ToArray());
            ms.Close();
            return sfinastr;

        }
        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="sinput"></param>
        /// <returns></returns>
        public string DecryptString(string sinput)
        {
            ICryptoTransform ct = mCSP.CreateEncryptor(Convert.FromBase64String(CKEY), Convert.FromBase64String(CIV));
            byte[] byts = Convert.FromBase64String(sinput);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, ct, CryptoStreamMode.Write);
            cs.Write(byts, 0, byts.Length);
            cs.FlushFinalBlock();
            cs.Close();
            string sfinastr = Encoding.UTF8.GetString(ms.ToArray());
            ms.Close();
            return sfinastr;
        }
    }
}
