using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography.Core;

namespace MentalCardGame.RNG
{
    internal class PrivateKey
    {
        private bool validParameter = false;

        private byte[] modulusCach;
        private byte[] exponentCach;
        private byte[] pCach;
        private byte[] qCach;
        private byte[] dpCach;
        private byte[] dqCach;
        private byte[] inverseQCach;
        private byte[] dCach;

        public byte[] Modulus
        {
            get
            {
                if (modulusCach == null)
                    SetCach();
                return modulusCach;
            }
            set
            {
                Log(value);
                if (modulusCach == null)
                    SetCach();
                if (value.SequenceEqual(modulusCach))
                    return;
                SetKey(value, Exponent, P, Q, DP, DQ, InverseQ, D);
            }
        }

        public byte[] Exponent
        {
            get
            {
                if (exponentCach == null)
                    SetCach();
                return exponentCach;
            }
            set
            {
                Log(value);
                if (exponentCach == null)
                    SetCach();
                if (value.SequenceEqual(exponentCach))
                    return;
                SetKey(Modulus, value, P, Q, DP, DQ, InverseQ, D);
            }
        }

        public byte[] P
        {
            get
            {
                if (pCach == null)
                    SetCach();
                return pCach;
            }
            set
            {
                Log(value);
                if (pCach == null)
                    SetCach();
                if (value.SequenceEqual(pCach))
                    return;
                SetKey(Modulus, Exponent, value, Q, DP, DQ, InverseQ, D);
            }
        }

        public byte[] Q
        {
            get
            {
                if (qCach == null)
                    SetCach();
                return qCach;
            }
            set
            {
                Log(value);
                if (qCach == null)
                    SetCach();
                if (value.SequenceEqual(qCach))
                    return;
                SetKey(Modulus, Exponent, P, value, DP, DQ, InverseQ, D);
            }
        }

        public byte[] DP
        {
            get
            {
                if (dpCach == null)
                    SetCach();
                return dpCach;
            }
            set
            {
                Log(value);
                if (dpCach == null)
                    SetCach();
                if (value.SequenceEqual(dpCach))
                    return;
                SetKey(Modulus, Exponent, P, Q, value, DQ, InverseQ, D);
            }
        }

        public byte[] DQ
        {
            get
            {
                if (dqCach == null)
                    SetCach();
                return dqCach;
            }
            set
            {
                Log(value);
                if (dqCach == null)
                    SetCach();
                if (value.SequenceEqual(dqCach))
                    return;
                SetKey(Modulus, Exponent, P, Q, DP, value, InverseQ, D);
            }
        }

        public byte[] InverseQ
        {
            get
            {
                if (inverseQCach == null)
                    SetCach();
                return inverseQCach;
            }
            set
            {
                Log(value);
                if (inverseQCach == null)
                    SetCach();
                if (value.SequenceEqual(inverseQCach))
                    return;
                SetKey(Modulus, Exponent, P, Q, DP, DQ, value, D);
            }
        }

        public byte[] D
        {
            get
            {
                if (dCach == null)
                    SetCach();
                return dCach;
            }
            set
            {
                Log(value);
                if (dCach == null)
                    SetCach();
                if (value.SequenceEqual(dCach))
                    return;
                SetKey(Modulus, Exponent, P, Q, DP, DQ, InverseQ, value);
            }
        }

        public Windows.Security.Cryptography.Core.CryptographicKey KeyPair { get; set; }

        public Windows.Security.Cryptography.Core.AsymmetricKeyAlgorithmProvider provider;

        public PrivateKey()
        {
            provider = Windows.Security.Cryptography.Core.AsymmetricKeyAlgorithmProvider.OpenAlgorithm(Windows.Security.Cryptography.Core.AsymmetricAlgorithmNames.RsaSignPkcs1Sha256);
            KeyPair = provider.CreateKeyPair(512);
        }

        public async Task<byte[]> Sign(byte[] toSign)
        {
            return (await CryptographicEngine.SignAsync(KeyPair, toSign.AsBuffer())).ToArray();
        }

        public bool Veryfiy(byte[] toVeryfy, byte[] signiture)
        {
            return CryptographicEngine.VerifySignature(KeyPair, toVeryfy.AsBuffer(), signiture.AsBuffer());
        }

        private void SetCach()
        {
            var byteblob = KeyPair.Export(CryptographicPrivateKeyBlobType.Capi1PrivateKey).ToArray();
            var blobstruct = BlobConverter.ToPrivateKeyBlobData(byteblob);
            BlobConverter.GetParameters(blobstruct, out modulusCach, out exponentCach, out pCach, out qCach, out dpCach, out dqCach, out inverseQCach, out dCach);
        }

        private void Log(byte[] value, [CallerMemberName]string caller = "")
        {
            System.Diagnostics.Debug.WriteLine(caller + ": " + Convert.ToBase64String(value));
        }

        private void SetKey(byte[] modulus, byte[] exponent, byte[] p, byte[] q, byte[] dp, byte[] dq, byte[] inverseQ, byte[] d)
        {
            var binary = BlobConverter.ToPrivateKeyBlobByte(BlobConverter.ToPrivateKeyBlobData(modulus, exponent, p, q, dp, dq, inverseQ, d));

            try
            {
                KeyPair = provider.ImportKeyPair(binary.AsBuffer(), CryptographicPrivateKeyBlobType.Capi1PrivateKey);
                validParameter = true;
            }
            catch (Exception)
            {
                validParameter = false;
            }
            modulusCach = modulus;
            exponentCach = exponent;
            pCach = p;
            qCach = q;
            dpCach = dp;
            dqCach = dq;
            inverseQCach = inverseQ;
            dCach = d;
        }

        public bool ValidParameter
        {
            get { return validParameter; }
        }
    }
}