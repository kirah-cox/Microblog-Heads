namespace microblog_heads.Services;

public class MediaService
{
    private static readonly HashSet<string> ImageExt = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    private static readonly HashSet<string> VideoExt = new(StringComparer.OrdinalIgnoreCase)
        { ".mp4", ".webm" };

    public const long MaxImageBytes = 5 * 1024 * 1024;
    public const long MaxVideoBytes = 25 * 1024 * 1024;
    public const int MaxFilesPerPost = 4;

    private readonly string _folder;

    public MediaService(IWebHostEnvironment env)
    {
        _folder = Path.Combine(env.ContentRootPath, "Media");
        Directory.CreateDirectory(_folder);
    }

    public static bool IsVideo(string fileName) => VideoExt.Contains(Path.GetExtension(fileName));

    public async Task<string> SaveFileAsync(Stream source, string originalName, long length)
    {
        var ext = Path.GetExtension(originalName).ToLowerInvariant();
        var isImage = ImageExt.Contains(ext);
        var isVideo = VideoExt.Contains(ext);

        if (!isImage && !isVideo)
            throw new ArgumentException("Only jpg, png, gif, webp, mp4 and webm files are allowed.");
        
         if (length > (isVideo ? MaxVideoBytes : MaxImageBytes))   
            throw new ArgumentException(isVideo ? $"Videos can't exceed {MaxVideoBytes}MB." : $"Images can't exceed {MaxImageBytes}MB.");
        
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