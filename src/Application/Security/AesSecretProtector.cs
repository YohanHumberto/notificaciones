using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Domain.Interfaces;

namespace NotificationService.Infrastructure.Security;

public class AesSecretProtector : ISecretProtector
{
    private readonly byte[] _key;

    public AesSecretProtector(IConfiguration configuration)
    {
        var key = configuration["NotificationSecurity:EncryptionKey"] ?? configuration["AppSettings:EncryptionKey"];
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("No se ha configurado NotificationSecurity:EncryptionKey.");
        }

        if (key.Length != 32)
        {
            throw new InvalidOperationException("La clave de cifrado debe tener exactamente 32 caracteres (AES-256). ");
        }

        _key = Encoding.UTF8.GetBytes(key.PadRight(32, '0').Substring(0, 32));
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            return string.Empty;
        }

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var memoryStream = new MemoryStream();
        memoryStream.Write(aes.IV, 0, aes.IV.Length);

        using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
        using (var writer = new StreamWriter(cryptoStream, Encoding.UTF8))
        {
            writer.Write(plainText);
        }

        return Convert.ToBase64String(memoryStream.ToArray());
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrWhiteSpace(cipherText))
        {
            return string.Empty;
        }

        var bytes = Convert.FromBase64String(cipherText);
        using var aes = Aes.Create();
        aes.Key = _key;

        var iv = new byte[aes.BlockSize / 8];
        Array.Copy(bytes, 0, iv, 0, iv.Length);
        aes.IV = iv;

        var encrypted = new byte[bytes.Length - iv.Length];
        Array.Copy(bytes, iv.Length, encrypted, 0, encrypted.Length);

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var memoryStream = new MemoryStream(encrypted);
        using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
        using var reader = new StreamReader(cryptoStream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
}
