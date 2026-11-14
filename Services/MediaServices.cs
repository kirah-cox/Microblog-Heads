namespace microblog_heads.Services;

/// <summary>
/// Saves uploaded pictures to the repo's Media folder (committed to git so everyone has the
/// same files). The database stores only the file name, which the app serves at /media/{name}.
/// </summary>
public class MediaService
{
    private static readonly HashSet<string> ImageExt = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    public const long MaxImageBytes = 5 * 1024 * 1024;
    public const int MaxFilesPerPost = 4;

    private readonly string _folder;

    public MediaService(IWebHostEnvironment env)
    {
        _folder = Path.Combine(env.ContentRootPath, "Media");
        Directory.CreateDirectory(_folder);
    }

    public async Task<string> SaveFileAsync(Stream source, string originalName, long length)
    {
        var ext = Path.GetExtension(originalName).ToLowerInvariant();

        if (!ImageExt.Contains(ext))
            throw new ArgumentException("Only jpg, png, gif and webp pictures are allowed.");

        if (length > MaxImageBytes)
            throw new ArgumentException($"Pictures can't exceed {MaxImageBytes / (1024 * 1024)}MB.");

        var fileName = Guid.NewGuid().ToString("N") + ext;
        await using var target = File.Create(Path.Combine(_folder, fileName));
        await source.CopyToAsync(target);
        return fileName;
    }

    public void DeleteFile (string fileName)
    {
        var path = Path.Combine(_folder, Path.GetFileName(fileName));
        if(File.Exists(path)) File.Delete(path);
    }
}
