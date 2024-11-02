using System.Security.Cryptography;

namespace TheDiscDb.Core.DiscHash;

public static class HashingExtensions
{
    public static string CalculateHash(this IEnumerable<FileHashInfo> files)
    {
        HashAlgorithm hash = MD5.Create();

        foreach (var file in files)
        {
            byte[] fileSizeBytes = BitConverter.GetBytes(file.Size);
            hash.TransformBlock(fileSizeBytes, 0, fileSizeBytes.Length, new byte[fileSizeBytes.Length], 0);
        }

        hash.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

        if (hash?.Hash == null)
        {
            throw new Exception("Unable to create disc hash");
        }

        return BitConverter.ToString(hash.Hash).Replace("-", "");
    }
}