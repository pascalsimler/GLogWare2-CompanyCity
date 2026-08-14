using System.Security.Cryptography;
using System.Text;

namespace Gudel.GLogWare.Configuration;

public static class SecretProtector
{
    private const string KeyString =
        "IlEstBeauLeLavaboIlEstLaidLeBidet";

    private const int KeySize = 32;
    private const int NonceSize = 12;
    private const int TagSize = 16;

    private static readonly byte[] Key =
        CreateKey(KeyString);


    private static byte[] CreateKey(string key)
    {
        key ??= string.Empty;

        // Si > 32 caractères : prendre les 32 premiers
        if (key.Length > KeySize)
        {
            key = key[..KeySize];
        }
        // Si < 32 caractères : compléter avec 'G'
        else if (key.Length < KeySize)
        {
            key = key.PadRight(KeySize, 'G');
        }

        return Encoding.ASCII.GetBytes(key);
    }


    public static string Encrypt(string plainText)
    {
        ArgumentNullException.ThrowIfNull(plainText);

        byte[] plaintext =
            Encoding.UTF8.GetBytes(plainText);

        byte[] nonce =
            RandomNumberGenerator.GetBytes(NonceSize);

        byte[] ciphertext =
            new byte[plaintext.Length];

        byte[] tag =
            new byte[TagSize];

        using var aes =
            new AesGcm(Key, TagSize);

        aes.Encrypt(
            nonce,
            plaintext,
            ciphertext,
            tag);

        // [Nonce][Tag][Ciphertext]
        byte[] result =
            new byte[
                NonceSize +
                TagSize +
                ciphertext.Length];

        Buffer.BlockCopy(
            nonce,
            0,
            result,
            0,
            NonceSize);

        Buffer.BlockCopy(
            tag,
            0,
            result,
            NonceSize,
            TagSize);

        Buffer.BlockCopy(
            ciphertext,
            0,
            result,
            NonceSize + TagSize,
            ciphertext.Length);

        return Convert.ToBase64String(result);
    }


    public static string Decrypt(string encrypted)
    {
        ArgumentNullException.ThrowIfNull(encrypted);

        byte[] data;

        try
        {
            data = Convert.FromBase64String(encrypted);
        }
        catch (FormatException ex)
        {
            throw new CryptographicException(
                "Invalid encrypted value.",
                ex);
        }

        if (data.Length < NonceSize + TagSize)
        {
            throw new CryptographicException(
                "Invalid encrypted value.");
        }

        ReadOnlySpan<byte> nonce =
            data.AsSpan(0, NonceSize);

        ReadOnlySpan<byte> tag =
            data.AsSpan(NonceSize, TagSize);

        ReadOnlySpan<byte> ciphertext =
            data.AsSpan(NonceSize + TagSize);

        byte[] plaintext =
            new byte[ciphertext.Length];

        using var aes =
            new AesGcm(Key, TagSize);

        try
        {
            aes.Decrypt(
                nonce,
                ciphertext,
                tag,
                plaintext);
        }
        catch (CryptographicException ex)
        {
            throw new CryptographicException(
                "Unable to decrypt the value.",
                ex);
        }

        return Encoding.UTF8.GetString(plaintext);
    }
}