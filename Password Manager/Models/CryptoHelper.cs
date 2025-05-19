using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

public static class CryptoHelper
{
    // Generates a cryptographic key and IV based on HWID
    private static (byte[] Key, byte[] IV) GetKeyAndIV()
    {
        string machineGuid = GetMachineGuid();
        byte[] hash;

        // Use SHA-512 to hash the machine GUID (512 bits = 64 bytes)
        using (SHA512 sha = SHA512.Create())
        {
            hash = sha.ComputeHash(Encoding.UTF8.GetBytes(machineGuid));
        }

        // Split the 64-byte hash
        // First 32 for the AES encryption key
        byte[] key = hash.Take(32).ToArray();

        // Next 16 bytes for AES IV
        byte[] iv = hash.Skip(32).Take(16).ToArray();
        
        // Returns the key and IV as two variables
        return (key, iv);
    }

    // Encrypts plaintext using AES with a key and IV
    public static string Encrypt(string plainText)
    {
        // Get encryption key and IV
        var (key, iv) = GetKeyAndIV();

        // Create a new AES instance
        using Aes aes = Aes.Create();
        aes.Key = key; // Assign key
        aes.IV = iv; // Assign IV
        aes.Padding = PaddingMode.PKCS7; // Fill the remainder of blocks with bytes of the same value

        // Memory stream to hold encrypted data
        using MemoryStream ms = new();

        // Encryption stream to write encrypted data into the memory stream
        using (CryptoStream cs = new(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
        using (StreamWriter writer = new(cs)) // Wraps the stream in a writer to write in plaintext
        {
            writer.Write(plainText); // Writes text, which will be encrypted
        }

        // Returns the encrypted data as a Base64 string
        return Convert.ToBase64String(ms.ToArray());
    }

    // Decrypts AES-encrypted string with the same method
    public static string Decrypt(string encryptedText)
    {
        var (key, iv) = GetKeyAndIV();

        // Decodes the Base64 string back to encrypted bytes
        byte[] buffer = Convert.FromBase64String(encryptedText);

        using Aes aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Padding = PaddingMode.PKCS7;

        using MemoryStream ms = new(buffer);
        using CryptoStream cs = new(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using StreamReader reader = new(cs); // SR wrapped in CS to read decrypted text

        // Read and return the decrytped data
        return reader.ReadToEnd();
    }

    public static string GetMachineGuid()
    {
        string location = @"SOFTWARE\Microsoft\Cryptography"; // Path
        string name = "MachineGuid"; // Property

        using (RegistryKey localMachineX64View = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)) // Registry to be opened
        {
            using (RegistryKey rk = localMachineX64View.OpenSubKey(location)) // Opens the folder of the chosen registry
            {
                if (rk == null)
                {
                    throw new KeyNotFoundException($"Key Not Found: 0 {location}");
                }

                object machineGuid = rk.GetValue(name);
                if (machineGuid == null)
                {
                    throw new IndexOutOfRangeException($"Index Not Found: 0 {name}");
                }

                return machineGuid.ToString();
            }
        }
    } // User's Local Machine HWID
}
