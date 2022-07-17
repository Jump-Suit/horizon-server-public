namespace RT.Cryptography
{
    public interface ICipherFactory
    {
        ICipher CreateNew(CipherContext context);
        ICipher CreateNew(CipherContext context, byte[] publicKey);
        ICipher CreateNew(RsaKeyPair rsaKeyPair);
    }
}
