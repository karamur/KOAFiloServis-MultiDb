using MKFiloServis.Web.Helpers;
using MKFiloServis.Web.Services.Security;

namespace MKFiloServis.Web.Services;

public sealed class ArchiveBrowserService
{
    private readonly string _storageRoot;
    private readonly string _repositoryRoot;

    public ArchiveBrowserService(IWebHostEnvironment environment)
    {
        _storageRoot = Path.GetFullPath(AppStoragePaths.GetStorageRoot(environment.ContentRootPath));
        _repositoryRoot = Path.GetFullPath(AppStoragePaths.GetArchiveRepositoryRoot(environment.ContentRootPath));
        Directory.CreateDirectory(_repositoryRoot);
    }

    public string RepositoryRoot => _repositoryRoot;

    public IReadOnlyList<ArchiveDirectoryItem> GetDirectories(string? relativeDirectory = null)
    {
        var directory = ResolveRepositoryPath(relativeDirectory);
        return Directory.EnumerateDirectories(directory)
            .Select(path => new DirectoryInfo(path))
            .OrderBy(info => info.Name)
            .Select(info => new ArchiveDirectoryItem(
                info.Name,
                Path.GetRelativePath(_repositoryRoot, info.FullName).Replace('\\', '/')))
            .ToList();
    }

    public IReadOnlyList<ArchiveFileItem> GetFiles(string? relativeDirectory = null, bool recursive = true)
    {
        var directory = ResolveRepositoryPath(relativeDirectory);
        var option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        return Directory.EnumerateFiles(directory, "*", option)
            .Select(path => new FileInfo(path))
            .OrderByDescending(info => info.LastWriteTimeUtc)
            .Select(info =>
            {
                var storageRelativePath = Path.GetRelativePath(_storageRoot, info.FullName).Replace('\\', '/');
                var repositoryRelativePath = Path.GetRelativePath(_repositoryRoot, info.FullName).Replace('\\', '/');
                var format = DetectFormat(info.FullName);
                var displayName = info.Name.EndsWith(".enc", StringComparison.OrdinalIgnoreCase)
                    ? info.Name[..^4]
                    : info.Name;

                return new ArchiveFileItem(
                    storageRelativePath,
                    repositoryRelativePath,
                    displayName,
                    Path.GetDirectoryName(repositoryRelativePath)?.Replace('\\', '/') ?? string.Empty,
                    Path.GetExtension(displayName).ToLowerInvariant(),
                    info.Length,
                    info.LastWriteTime,
                    format);
            })
            .ToList();
    }

    public string ValidateStorageRelativePath(string storageRelativePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storageRelativePath);
        var fullPath = Path.GetFullPath(Path.Combine(_storageRoot, storageRelativePath.Replace('/', Path.DirectorySeparatorChar)));
        EnsureUnderRoot(fullPath, _repositoryRoot);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("Depo dosyası bulunamadı.", storageRelativePath);
        return fullPath;
    }

    private string ResolveRepositoryPath(string? relativeDirectory)
    {
        var relative = string.IsNullOrWhiteSpace(relativeDirectory) ? "." : relativeDirectory;
        var fullPath = Path.GetFullPath(Path.Combine(_repositoryRoot, relative.Replace('/', Path.DirectorySeparatorChar)));
        EnsureUnderRoot(fullPath, _repositoryRoot);
        if (!Directory.Exists(fullPath))
            throw new DirectoryNotFoundException("Seçilen Depo dizini bulunamadı.");
        return fullPath;
    }

    private static void EnsureUnderRoot(string fullPath, string root)
    {
        var normalizedRoot = root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(fullPath, root, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("Depo kökü dışındaki dizinlere erişilemez.");
    }

    private static ArchiveFileFormat DetectFormat(string fullPath)
    {
        using var stream = File.OpenRead(fullPath);
        Span<byte> header = stackalloc byte[4];
        var read = stream.Read(header);
        if (read == 4 && DataProtectionFileProtector.IsProtectedFormat(header))
            return ArchiveFileFormat.DataProtection;
        if (read == 4 && header.SequenceEqual("KOA1"u8))
            return ArchiveFileFormat.LegacyAes;
        if (fullPath.EndsWith(".enc", StringComparison.OrdinalIgnoreCase))
            return ArchiveFileFormat.LegacyEncrypted;
        return ArchiveFileFormat.Plain;
    }
}

public sealed record ArchiveDirectoryItem(string Name, string RelativePath);

public sealed record ArchiveFileItem(
    string StorageRelativePath,
    string RepositoryRelativePath,
    string DisplayName,
    string Directory,
    string Extension,
    long Size,
    DateTime ModifiedAt,
    ArchiveFileFormat Format);

public enum ArchiveFileFormat
{
    Plain,
    DataProtection,
    LegacyAes,
    LegacyEncrypted
}
