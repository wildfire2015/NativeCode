using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PSupport
{
    /// <summary>
    /// 加密解密类
    /// </summary>
    public class Encryption
    {
        #region "定义加密字串变量"
        private SymmetricAlgorithm mCSP = new DESCryptoServiceProvider();  //声明对称算法变量
        private  string CIV  = Convert.ToBase64String(new byte[] { 32, 57, 6, 43,31, 29,88,79 });  //初始化向量
        private  string CKEY = Convert.ToBase64String(new byte[] { 77, 23, 18,90,100,51,82,45 });  //密钥（常量）
        #endregion
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="inputbyts"></param>
        /// <returns></returns>
        public byte[] Encrypt(byte[] inputbyts)
        {
            
            ICryptoTransform ct = mCSP.CreateEncryptor(Convert.FromBase64String(CKEY), Convert.FromBase64String(CIV));
            byte[] byts = new byte[inputbyts.Length];
            inputbyts.CopyTo(byts, 0);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, ct, CryptoStreamMode.Write);
            cs.Write(byts, 0, byts.Length);
            cs.FlushFinalBlock();
            cs.Close();
            byts = ms.ToArray();
            ms.Close();
            return byts;
        }
        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="inputbyts"></param>
        /// <returns></returns>
        public byte[] Decrypt(byte[] inputbyts)
        {
            ICryptoTransform ct = mCSP.CreateDecryptor(Convert.FromBase64String(CKEY), Convert.FromBase64String(CIV));
            byte[] byts = new byte[inputbyts.Length];
            inputbyts.CopyTo(byts, 0);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, ct, CryptoStreamMode.Write);
            cs.Write(byts, 0, byts.Length);
            cs.FlushFinalBlock();
            cs.Close();
            byts = ms.ToArray();
            ms.Close();
            return byts;
        }
    }
}
