using SmartStudent.Application.Interfaces;
using SmartStudent.Core.Exceptions;

namespace SmartStudent.Infrastructure.Identity;

/// <summary>
/// Saves student profile photos locally under wwwroot/uploads/profiles with extension validation.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    private readonly string _uploadsRootPath;

    public LocalFileStorageService(string webRootPath)
    {
        _uploadsRootPath = Path.Combine(webRootPath, "uploads", "profiles");
        Directory.CreateDirectory(_uploadsRootPath);
    }

    public async Task<string> SaveProfilePhotoAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var ext = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(ext) || !AllowedExtensions.Contains(ext))
        {
            throw new BadRequestException("Invalid image format. Allowed formats: .jpg, .jpeg, .png, .webp.");
        }

        var safeFileName = $"{Guid.CreateVersion7()}{ext.ToLowerInvariant()}";
        var fullPath = Path.Combine(_uploadsRootPath, safeFileName);

        await using var output = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await fileStream.CopyToAsync(output, cancellationToken);

        return $"/uploads/profiles/{safeFileName}";
    }
}
