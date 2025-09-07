using Microsoft.Win32;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Transmittal.Library.Helpers;
public static class ClientIdProvider
{
    private const string _defaultDirName = "Transmittal"; 
    private const string _defaultFileName = "client_id.txt"; 
    private const string _deterministicSalt = "transmittal"; // product-specific salt

    public static string GetOrCreateClientId(string? dirName = _defaultDirName, string? fileName = _defaultFileName)
    {
        try
        {
            var baseDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dir = Path.Combine(baseDir, dirName ?? _defaultDirName);
            Directory.CreateDirectory(dir);

            var path = Path.Combine(dir, fileName ?? _defaultFileName);
            if (File.Exists(path))
            {
                var existing = File.ReadAllText(path).Trim();
                if (!string.IsNullOrWhiteSpace(existing))
                    return existing;
            }

            var newId = Guid.NewGuid().ToString("N"); // 32 hex chars
            File.WriteAllText(path, newId);
            return newId;
        }
        catch
        {
            // If we cannot persist, fall back to a deterministic, hashed machine ID.
            return GetHashedMachineClientId(_deterministicSalt);
        }
    }

    private static string GetHashedMachineClientId(string salt)
    {
        var machineGuid = GetWindowsMachineGuid() ?? "unknown";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(salt));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(machineGuid));
        return ToHexUpper(hash); // uppercase hex, GA accepts arbitrary strings
    }

    private static string? GetWindowsMachineGuid()
    {
        try
        {
            using var key64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                .OpenSubKey(@"SOFTWARE\Microsoft\Cryptography");
            var v64 = key64?.GetValue("MachineGuid") as string;
            if (!string.IsNullOrWhiteSpace(v64))
            {
                return v64;
            }
        }
        catch { /* ignore */ }

        try
        {
            using var key32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
                .OpenSubKey(@"SOFTWARE\Microsoft\Cryptography");
            return key32?.GetValue("MachineGuid") as string;
        }
        catch 
        { 
            return null; 
        }
    }

    // Cross-target hex encoder (works on .NET Framework 4.8 and .NET 8)
    private static string ToHexUpper(byte[] bytes)
    {
#if NET8_0_OR_GREATER
        return Convert.ToHexString(bytes);
#else
        // Manual uppercase hex encoding
        var c = new char[bytes.Length * 2];
        const string hex = "0123456789ABCDEF";
        for (int i = 0, j = 0; i < bytes.Length; i++)
        {
            byte b = bytes[i];
            c[j++] = hex[b >> 4];
            c[j++] = hex[b & 0xF];
        }
        return new string(c);
#endif
    }
}
