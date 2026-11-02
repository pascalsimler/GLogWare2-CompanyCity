namespace Gudel.GLogWare.Entities;

public class User : BaseTracking
{
    /// <summary>
    /// The unique identifier for the user
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The username of the user to be used for login and identification
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// The email address of the user. Can be used for login and identification
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// The display name of the user
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Password hash. Null for Active Directory users, who are validated against LDAP
    /// </summary>
    public string? PasswordHash { get; set; }
    
    /// <summary>
    /// Indicates whether the user is an LDAP user
    /// </summary>
    public bool IsLdap { get; set; } = false;
    
    /// <summary>
    /// The distinguished name of the user in Active Directory
    /// </summary>
    public string? ActiveDirectoryDistinguishedName { get; set; }
    
    /// <summary>
    /// The phone number of the user
    /// </summary>
    public string? Phone { get; set; }
    
    /// <summary>
    /// Additional information 1
    /// </summary>
    public string? Info1 { get; set; }
    
    /// <summary>
    /// Additional information 2
    /// </summary>
    public string? Info2 { get; set; }
    
    /// <summary>
    /// Additional information 3
    /// </summary>
    public string? Info3 { get; set; }

    /// <summary>
    /// The thumbnail photo of the user stored as byte array. Null if no photo is available
    /// </summary>
    public byte[]? ThumbnailPhoto { get; set; }
    
    /// <summary>
    /// The content type of the thumbnail photo
    /// </summary>
    public string? ThumbnailPhotoContentType { get; set; }
    
    /// <summary>
    /// The data URL of the thumbnail photo 
    /// </summary>
    public string? PhotoDataUrl => ThumbnailPhoto is { Length: > 0 }
        ? $"data:{ThumbnailPhotoContentType ?? "image/jpeg"};base64,{Convert.ToBase64String(ThumbnailPhoto)}"
        : null;
    
    /// <summary>
    /// 
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// 
    /// </summary>
    public int FailedAccessCount { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    public DateTimeOffset? LockoutEnd { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public ICollection<UserRole> UserRoles { get; set; } = [];
}