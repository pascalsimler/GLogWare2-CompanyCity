using System.Security.Cryptography;

namespace Gudel.GLogWare.Entities;

public class User : BaseTracking
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public bool IsLdap { get; set; } = false;
    public string? ActiveDirectoryDistinguishedName { get; set; }
    public bool IsActive { get; set; } = true;
    public int FailedAccessCount { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = [];


    // Password hashing and verification methods
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 600_000;

    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    public static bool Verify(string password, string? hash)
    {
        var parts = hash?.Split('.');
        if (parts is not { Length: 3 } || !int.TryParse(parts[0], out var iterations))
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[1]);
            var expected = Convert.FromBase64String(parts[2]);
            var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public static IReadOnlyList<string> Validate(string password)
    {
        var errors = new List<string>();
        if (password.Length < 12) errors.Add("Passwords must be at least 12 characters.");
        if (!password.Any(char.IsUpper)) errors.Add("Passwords must have at least one uppercase letter.");
        if (!password.Any(char.IsLower)) errors.Add("Passwords must have at least one lowercase letter.");
        if (!password.Any(char.IsDigit)) errors.Add("Passwords must have at least one digit.");
        if (password.All(char.IsLetterOrDigit)) errors.Add("Passwords must have at least one non-alphanumeric character.");
        return errors;
    }
}