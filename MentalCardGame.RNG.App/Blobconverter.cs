using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MentalCardGame.RNG
{
    internal static class BlobConverter
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="modulo">Modulo in big-endian</param>
        /// <param name="exp">Exponent in big-endian</param>
        /// <returns></returns>
        public static PublicKeyBlob ToPublicKeyBlobData(byte[] modulo, byte[] exp)
        {
            var publicKeyStruct = new PUBLICKEYSTRUC();
            publicKeyStruct.aiKeyAlg = ALG_ID.CALG_RSA_KEYX;
            publicKeyStruct.bType = BlobType.PUBLICKEYBLOB;
            publicKeyStruct.bVersion = 2;
            var rsa = new RSAPUBKEY();
            rsa.magic = RSAPUBKRY_Magic.RSA1;
            rsa.bitlen = (uint)modulo.Length * 8;
            rsa.pubexp = BitConverter.ToUInt32(exp.Concat((byte)0).ToArray(), 0);

            var erg = new PublicKeyBlob();
            erg.modulus = modulo.Reverse().ToArray();
            erg.publickeystruc = publicKeyStruct;
            erg.rsapubkey = rsa;
            return erg;
        }

        public static void GetParameters(PublicKeyBlob blob, out byte[] modulo, out byte[] exp)
        {
            // ToDo laut internet ist der Exponent immer 3 byte, Windows läßt immer die letzen 0 bytes weg, wodurch es auch vier oder 2 bytes sein können. Gegencheckn mit tobies implementierung.
            exp = BitConverter.GetBytes(blob.rsapubkey.pubexp).Take(3).ToArray();
            modulo = blob.modulus.Reverse().ToArray();
        }

        public static byte[] ToPublicKeyBlobByte(PublicKeyBlob blop)
        {
            var list = new List<byte>();

            //Publickeystruct
            list.AddRange(GetBytes(blop.publickeystruc));

            //RSAPubKey
            list.AddRange(GetBytes(blop.rsapubkey));

            list.AddRange(blop.modulus);

            return list.ToArray();
        }

        private static byte[] GetBytes(RSAPUBKEY rsapubkey)
        {
            List<byte> list = new List<byte>();
            list.AddRange(BitConverter.GetBytes((UInt32)rsapubkey.magic));
            list.AddRange(BitConverter.GetBytes((UInt32)rsapubkey.bitlen));
            list.AddRange(BitConverter.GetBytes((UInt32)rsapubkey.pubexp));
            return list.ToArray();
        }

        private static byte[] GetBytes(PUBLICKEYSTRUC publickeystruc)
        {
            List<byte> list = new List<byte>();

            list.Add((byte)publickeystruc.bType);
            list.Add((byte)publickeystruc.bVersion);
            list.AddRange(BitConverter.GetBytes(publickeystruc.reserved));
            list.AddRange(BitConverter.GetBytes((UInt32)publickeystruc.aiKeyAlg));
            return list.ToArray();
        }

        public static PublicKeyBlob ToPublicKeyBlobData(byte[] blob)
        {
            var erg = new PublicKeyBlob();
            PUBLICKEYSTRUC s1 = ReadPublicKeyStruct(blob.Take(8).ToArray());
            RSAPUBKEY s2 = ReadRSAPubKey(blob.Skip(8).Take(12).ToArray());
            byte[] modulo = blob.Skip(8).Skip(12).Reverse().ToArray();

            erg.publickeystruc = s1;
            erg.rsapubkey = s2;
            erg.modulus = modulo.Reverse().ToArray();
            return erg;
        }

        public static byte[] ToPrivateKeyBlobByte(PrivateKeyBlob blop)
        {
            var list = new List<byte>();

            //Publickeystruct
            list.AddRange(GetBytes(blop.publickeystruc));

            //RSAPubKey
            list.AddRange(GetBytes(blop.rsapubkey));

            list.AddRange(blop.modulus);
            list.AddRange(blop.prime1);
            list.AddRange(blop.prime2);
            list.AddRange(blop.exponent1);
            list.AddRange(blop.exponent2);
            list.AddRange(blop.coefficient);
            list.AddRange(blop.privateExponent);

            return list.ToArray();
        }

        public static void GetParameters(PrivateKeyBlob blob,
            out byte[] modulo,
            out byte[] exp,
            out byte[] p,
            out byte[] q,
            out byte[] dp,
            out byte[] dq,
            out byte[] inverseQ,
            out byte[] d)
        {
            // ToDo laut internet ist der Exponent immer 3 byte, Windows läßt immer die letzen 0 bytes weg, wodurch es auch vier oder 2 bytes sein können. Gegencheckn mit tobies implementierung.
            exp = BitConverter.GetBytes(blob.rsapubkey.pubexp).Take(3).ToArray();
            modulo = blob.modulus.Reverse().ToArray();
            p = blob.prime1.Reverse().ToArray();
            q = blob.prime2.Reverse().ToArray();
            d = blob.privateExponent.Reverse().ToArray();
            dp = blob.exponent1.Reverse().ToArray();
            dq = blob.exponent2.Reverse().ToArray();
            inverseQ = blob.coefficient.Reverse().ToArray();
        }

        public static PrivateKeyBlob ToPrivateKeyBlobData(byte[] modulus, byte[] exponent, byte[] p, byte[] q, byte[] dp, byte[] dq, byte[] inverseQ, byte[] d)
        {
            var publicKeyStruct = new PUBLICKEYSTRUC();
            publicKeyStruct.aiKeyAlg = ALG_ID.CALG_RSA_KEYX;
            publicKeyStruct.bType = BlobType.PRIVATEKEYBLOB;
            publicKeyStruct.bVersion = 2;
            var rsa = new RSAPUBKEY();
            rsa.magic = RSAPUBKRY_Magic.RSA2;
            rsa.bitlen = (uint)modulus.Length * 8;
            rsa.pubexp = BitConverter.ToUInt32((exponent).Concat((byte)0).ToArray(), 0);

            var erg = new PrivateKeyBlob();
            erg.modulus = modulus.Reverse().ToArray();
            erg.prime1 = p.Reverse().ToArray();
            erg.prime2 = q.Reverse().ToArray();
            erg.exponent1 = dp.Reverse().ToArray();
            erg.exponent2 = dq.Reverse().ToArray();
            erg.coefficient = inverseQ.Reverse().ToArray();
            erg.privateExponent = d.Reverse().ToArray();
            erg.publickeystruc = publicKeyStruct;
            erg.rsapubkey = rsa;
            return erg;

            //<RSAKeyValue>
            //   <Modulus>…</Modulus>
            //   <Exponent>…</Exponent>
            //   <P>…</P>
            //   <Q>…</Q>
            //   <DP>…</DP>
            //   <DQ>…</DQ>
            //   <InverseQ>…</InverseQ>
            //   <D>…</D>
            //</RSAKeyValue>
        }

        public static PrivateKeyBlob ToPrivateKeyBlobData(byte[] blob)
        {
            var erg = new PrivateKeyBlob();
            int index = 0;

            PUBLICKEYSTRUC s1 = ReadPublicKeyStruct(DataGrepper(blob, ref index, 8));
            RSAPUBKEY s2 = ReadRSAPubKey(DataGrepper(blob, ref index, 12));
            erg.modulus = DataGrepper(blob, ref index, (int)s2.bitlen / 8);
            erg.prime1 = DataGrepper(blob, ref index, (int)s2.bitlen / 16);
            erg.prime2 = DataGrepper(blob, ref index, (int)s2.bitlen / 16);
            erg.exponent1 = DataGrepper(blob, ref index, (int)s2.bitlen / 16);
            erg.exponent2 = DataGrepper(blob, ref index, (int)s2.bitlen / 16);
            erg.coefficient = DataGrepper(blob, ref index, (int)s2.bitlen / 16);
            erg.privateExponent = DataGrepper(blob, ref index, (int)s2.bitlen / 8);

            System.Diagnostics.Debug.Assert(index == blob.Length);

            erg.publickeystruc = s1;
            erg.rsapubkey = s2;
            return erg;
        }

        /// <summary>
        /// Ließt count viele Bytes und erhöt danach index um count
        /// </summary>
        /// <param name="data"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns>Die gelesen bytes</returns>
        private static byte[] DataGrepper(byte[] data, ref int index, int count)
        {
            var erg = data.Skip(index).Take(count).ToArray();
            index += count;
            return erg;
        }

        private static RSAPUBKEY ReadRSAPubKey(byte[] p)
        {
            var erg = new RSAPUBKEY();
            erg.magic = (RSAPUBKRY_Magic)BitConverter.ToUInt32(p, 0);
            erg.bitlen = BitConverter.ToUInt32(p, 4);
            erg.pubexp = BitConverter.ToUInt32(p, 8);
            return erg;
        }

        private static PUBLICKEYSTRUC ReadPublicKeyStruct(byte[] p)
        {
            var erg = new PUBLICKEYSTRUC();

            erg.bType = (BlobType)p[0];
            erg.bVersion = p[1];
            erg.reserved = BitConverter.ToUInt16(p, 2);
            erg.aiKeyAlg = (ALG_ID)BitConverter.ToUInt32(p, 4);

            return erg;
        }

        public struct PrivateKeyBlob
        {
            public PUBLICKEYSTRUC publickeystruc { get; set; }

            public RSAPUBKEY rsapubkey { get; set; }

            /// <summary>
            /// In Litle-Endian
            /// </summary>
            public Byte[] modulus { get; set; }

            /// <summary>
            /// In Litle-Endian
            /// </summary>
            public Byte[] prime1 { get; set; }

            /// <summary>
            /// In Litle-Endian
            /// </summary>
            public Byte[] prime2 { get; set; }

            /// <summary>
            /// In Litle-Endian
            /// </summary>
            public Byte[] exponent1 { get; set; }

            /// <summary>
            /// In Litle-Endian
            /// </summary>
            public Byte[] exponent2 { get; set; }

            /// <summary>
            /// In Litle-Endian
            /// </summary>
            public Byte[] coefficient { get; set; }

            /// <summary>
            /// In Litle-Endian
            /// </summary>
            public Byte[] privateExponent { get; set; }
        }

        public struct PUBLICKEYSTRUC
        {
            public BlobType bType { get; set; }

            public byte bVersion { get; set; }

            public ushort reserved { get; set; }

            public ALG_ID aiKeyAlg { get; set; }
        }

        public struct RSAPUBKEY
        {
            public RSAPUBKRY_Magic magic { get; set; }

            public UInt32 bitlen { get; set; }

            public UInt32 pubexp { get; set; }
        }

        public struct PublicKeyBlob
        {
            public PUBLICKEYSTRUC publickeystruc { get; set; }

            public RSAPUBKEY rsapubkey { get; set; }

            /// <summary>
            /// In Litle-Endian
            /// </summary>
            public Byte[] modulus { get; set; }
        }

        public enum RSAPUBKRY_Magic : uint
        {
            RSA1 = 0x31415352,
            RSA2 = 0x32415352
        }

        public enum BlobType : byte
        {
            KEYSTATEBLOB = 0xC,//	The BLOB is a key state BLOB.
            OPAQUEKEYBLOB = 0x9,//	The key is a session key.
            PLAINTEXTKEYBLOB = 0x8,//	The key is a session key.
            PRIVATEKEYBLOB = 0x7,//	The key is a public/private key pair.
            PUBLICKEYBLOB = 0x6,//	The key is a public key.
            PUBLICKEYBLOBEX = 0xA,//	The key is a public key.
            SIMPLEBLOB = 0x1,//	The key is a session key.
            SYMMETRICWRAPKEYBLOB = 0xB,//	The key is a symmetric key.
        }

        public enum ALG_ID : uint
        {
            CALG_3DES = 0x00006603,//	Triple DES encryption algorithm.
            CALG_3DES_112 = 0x00006609,//	Two-key triple DES encryption with effective key length equal to 112 bits.
            CALG_AES = 0x00006611,//	Advanced Encryption Standard (AES). This algorithm is supported by the Microsoft AES Cryptographic Provider.
            CALG_AES_128 = 0x0000660e,//	128 bit AES. This algorithm is supported by the Microsoft AES Cryptographic Provider.
            CALG_AES_192 = 0x0000660f,//	192 bit AES. This algorithm is supported by the Microsoft AES Cryptographic Provider.
            CALG_AES_256 = 0x00006610,//	256 bit AES. This algorithm is supported by the Microsoft AES Cryptographic Provider.
            CALG_AGREEDKEY_ANY = 0x0000aa03,//	Temporary algorithm identifier for handles of Diffie-Hellman–agreed keys.
            CALG_CYLINK_MEK = 0x0000660c,//	An algorithm to create a 40-bit DES key that has parity bits and zeroed key bits to make its key length 64 bits. This algorithm is supported by the Microsoft Base Cryptographic Provider.
            CALG_DES = 0x00006601,//	DES encryption algorithm.
            CALG_DESX = 0x00006604,//	DESX encryption algorithm.
            CALG_DH_EPHEM = 0x0000aa02,//	Diffie-Hellman ephemeral key exchange algorithm.
            CALG_DH_SF = 0x0000aa01,//	Diffie-Hellman store and forward key exchange algorithm.
            CALG_DSS_SIGN = 0x00002200,//	DSA public key signature algorithm.
            CALG_ECDH = 0x0000aa05,//	Elliptic curve Diffie-Hellman key exchange algorithm.	Note  This algorithm is supported only through Cryptography API: Next Generation.	Windows Server 2003 and Windows XP:  This algorithm is not supported.
            CALG_ECDSA = 0x00002203,//	Elliptic curve digital signature algorithm.	Note  This algorithm is supported only through Cryptography API: Next Generation.	Windows Server 2003 and Windows XP:  This algorithm is not supported.
            CALG_ECMQV = 0x0000a001,//	Elliptic curve Menezes, Qu, and Vanstone (MQV) key exchange algorithm. This algorithm is not supported.
            CALG_HASH_REPLACE_OWF = 0x0000800b,//	One way function hashing algorithm.
            CALG_HUGHES_MD5 = 0x0000a003,//	Hughes MD5 hashing algorithm.
            CALG_HMAC = 0x00008009,//	HMAC keyed hash algorithm. This algorithm is supported by the Microsoft Base Cryptographic Provider.
            CALG_KEA_KEYX = 0x0000aa04,//	KEA key exchange algorithm (FORTEZZA). This algorithm is not supported.
            CALG_MAC = 0x00008005,//	MAC keyed hash algorithm. This algorithm is supported by the Microsoft Base Cryptographic Provider.
            CALG_MD2 = 0x00008001,//	MD2 hashing algorithm. This algorithm is supported by the Microsoft Base Cryptographic Provider.
            CALG_MD4 = 0x00008002,//	MD4 hashing algorithm.
            CALG_MD5 = 0x00008003,//	MD5 hashing algorithm. This algorithm is supported by the Microsoft Base Cryptographic Provider.
            CALG_NO_SIGN = 0x00002000,//	No signature algorithm.
            CALG_OID_INFO_CNG_ONLY = 0xffffffff,//	The algorithm is only implemented in CNG. The macro, IS_SPECIAL_OID_INFO_ALGID, can be used to determine whether a cryptography algorithm is only supported by using the CNG functions.
            CALG_OID_INFO_PARAMETERS = 0xfffffffe,//	The algorithm is defined in the encoded parameters. The algorithm is only supported by using CNG. The macro, IS_SPECIAL_OID_INFO_ALGID, can be used to determine whether a cryptography algorithm is only supported by using the CNG functions.
            CALG_PCT1_MASTER = 0x00004c04,//	Used by the Schannel.dll operations system. This ALG_ID should not be used by applications.
            CALG_RC2 = 0x00006602,//	RC2 block encryption algorithm. This algorithm is supported by the Microsoft Base Cryptographic Provider.
            CALG_RC4 = 0x00006801,//	RC4 stream encryption algorithm. This algorithm is supported by the Microsoft Base Cryptographic Provider.
            CALG_RC5 = 0x0000660d,//	RC5 block encryption algorithm.
            CALG_RSA_KEYX = 0x0000a400,//	RSA public key exchange algorithm. This algorithm is supported by the Microsoft Base Cryptographic Provider.
            CALG_RSA_SIGN = 0x00002400,//	RSA public key signature algorithm. This algorithm is supported by the Microsoft Base Cryptographic Provider.
            CALG_SCHANNEL_ENC_KEY = 0x00004c07,//	Used by the Schannel.dll operations system. This ALG_ID should not be used by applications.
            CALG_SCHANNEL_MAC_KEY = 0x00004c03,//	Used by the Schannel.dll operations system. This ALG_ID should not be used by applications.
            CALG_SCHANNEL_MASTER_HASH = 0x00004c02,//	Used by the Schannel.dll operations system. This ALG_ID should not be used by applications.
            CALG_SEAL = 0x00006802,//	SEAL encryption algorithm. This algorithm is not supported.
            CALG_SHA = 0x00008004,//	SHA hashing algorithm. This algorithm is supported by the Microsoft Base Cryptographic Provider.
            CALG_SHA1 = 0x00008004,//	Same as CALG_SHA. This algorithm is supported by the Microsoft Base Cryptographic Provider.
            CALG_SHA_256 = 0x0000800c,//	256 bit SHA hashing algorithm. This algorithm is supported by Microsoft Enhanced RSA and AES Cryptographic Provider..	Windows XP with SP3:  This algorithm is supported by the Microsoft Enhanced RSA and AES Cryptographic Provider (Prototype).	Windows XP with SP2, Windows XP with SP1, and Windows XP:  This algorithm is not supported.
            CALG_SHA_384 = 0x0000800d,//	384 bit SHA hashing algorithm. This algorithm is supported by Microsoft Enhanced RSA and AES Cryptographic Provider.	Windows XP with SP3:  This algorithm is supported by the Microsoft Enhanced RSA and AES Cryptographic Provider (Prototype).	Windows XP with SP2, Windows XP with SP1, and Windows XP:  This algorithm is not supported.
        }
    }
}