using System;
using System.Text;
using D8_Demo.Models;

namespace D8_Demo.Tool;

public class MainSm4
    {
        /// <summary>
        /// 加密ECB模式
        /// </summary>
        /// <param name="secretKey">密钥</param>
        /// <param name="hexString">明文是否是十六进制</param>
        /// <param name="plainText">明文</param>
        /// <returns>返回密文</returns>
        public static String Encrypt_ECB(String plainText,String secretKey, bool hexString )
        {
            Sm4Context ctx = new Sm4Context();
            ctx.isPadding = true;
            ctx.mode = Sm4.SM4_ENCRYPT;
            byte[] keyBytes;
            if (hexString)
            {
                //keyBytes = Hex.Decode(secretKey);
                keyBytes = MathHelper.strToHexByte(secretKey);
                //byte[] keyBytes2 = Encoding.Default.GetBytes(secretKey);
                //byte[] keyBytes3 = Encoding.Default.GetBytes(secretKey);
            }
            else
            {
                keyBytes = Encoding.Default.GetBytes(secretKey);
            }
            Sm4 sm4 = new Sm4();
            sm4.sm4_setkey_enc(ctx, keyBytes);
            byte[] encrypted = sm4.sm4_crypt_ecb(ctx, MathHelper.strToHexByte(plainText));
            //String cipherText = Encoding.Default.GetString(Hex.Encode(encrypted));
            String cipherText = MathHelper.ToHexString(encrypted);
            cipherText = cipherText.Substring(0, plainText.Length);//20211014

            return cipherText;
        }
        /// <summary>
        /// 解密ECB模式
        /// </summary>
        /// <param name="secretKey">密钥</param>
        /// <param name="hexString">明文是否是十六进制</param>
        /// <param name="cipherText">密文</param>
        /// <returns>返回明文</returns>
        public static String Decrypt_ECB(String cipherText,String secretKey, bool hexString)
        {
            Sm4Context ctx = new Sm4Context();
            ctx.isPadding = true;
            ctx.mode = Sm4.SM4_DECRYPT;

            byte[] keyBytes;
            if (hexString)
            {
                keyBytes = MathHelper.strToHexByte(secretKey);
                //byte[] keyBytes2 = Encoding.Default.GetBytes(secretKey);
                //keyBytes =MathHelper.strToHexByte(secretKey);
            }
            else
            {
                keyBytes = Encoding.Default.GetBytes(secretKey);
            }
            Sm4 sm4 = new Sm4();
            sm4.sm4_setkey_dec(ctx, keyBytes);
            //byte[] decrypted = sm4.sm4_crypt_ecb(ctx, Hex.Decode(cipherText));
            byte[] decrypted = sm4.sm4_crypt_ecb(ctx, MathHelper.strToHexByte(cipherText));

            String mText = MathHelper.ToHexString(decrypted);
            //mText = mText.Substring(0, 32);//20211014取前32位
            mText = mText.Substring(0, cipherText.Length);
            return mText;

        }

        /// <summary>
        /// 加密CBC模式
        /// </summary>
        /// <param name="secretKey">密钥</param>
        /// <param name="hexString">明文是否是十六进制</param>
        /// <param name="iv"></param>
        /// <param name="plainText">明文</param>
        /// <returns>返回密文</returns>
        public static String Encrypt_CBC(String plainText,String secretKey, string iv , bool hexString )
        {
            Sm4Context ctx = new Sm4Context();
            ctx.isPadding = true;
            ctx.mode = Sm4.SM4_ENCRYPT;
            byte[] keyBytes;
            byte[] ivBytes;
            if (hexString)
            {
                //keyBytes = Hex.Decode(secretKey);
                //ivBytes = Hex.Decode(iv);
                keyBytes = MathHelper.strToHexByte(secretKey);
                ivBytes = MathHelper.strToHexByte(iv);
            }
            else
            {
                keyBytes = Encoding.Default.GetBytes(secretKey);
                ivBytes = Encoding.Default.GetBytes(iv);
            }
            byte[] plt = Encoding.Default.GetBytes(plainText);
            Sm4 sm4 = new Sm4();
            sm4.sm4_setkey_enc(ctx, keyBytes);
            byte[] encrypted = sm4.sm4_crypt_cbc(ctx, ivBytes, MathHelper.strToHexByte(plainText));
            //String cipherText = Encoding.Default.GetString(Hex.Encode(encrypted));
            String cipherText =MathHelper.ToHexString(encrypted);
            cipherText = cipherText.Substring(0, 32);//取前32位
            return cipherText;
        }
        /// <summary>
        /// 解密CBC模式
        /// </summary>
        /// <param name="secretKey">密钥</param>
        /// <param name="hexString">明文是否是十六进制</param>
        /// <param name="iv"></param>
        /// <param name="cipherText">密文</param>
        /// <returns>返回明文</returns>
        public static String Decrypt_CBC(String cipherText,String secretKey,string iv, bool hexString)
        {
            Sm4Context ctx = new Sm4Context();
            ctx.isPadding = true;
            ctx.mode = Sm4.SM4_DECRYPT;
            byte[] keyBytes;
            byte[] ivBytes;
            if (hexString)
            {
                //keyBytes = Hex.Decode(secretKey);
                //ivBytes = Hex.Decode(iv);
                keyBytes = MathHelper.strToHexByte(secretKey);
                ivBytes = MathHelper.strToHexByte(iv);
            }
            else
            {
                keyBytes = Encoding.Default.GetBytes(secretKey);
                ivBytes = Encoding.Default.GetBytes(iv);
            }
            Sm4 sm4 = new Sm4();
            sm4.sm4_setkey_dec(ctx, keyBytes);
            //byte[] decrypted = sm4.sm4_crypt_cbc(ctx, ivBytes, Hex.Decode(cipherText));
            byte[] decrypted = sm4.sm4_crypt_cbc(ctx, ivBytes, MathHelper.strToHexByte(cipherText));
            return MathHelper.ToHexString(decrypted);
        }
    }