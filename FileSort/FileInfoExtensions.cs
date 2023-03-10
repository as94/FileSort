namespace FileSort;

public static class FileInfoExtensions
{
    public static long ConvertToMegabytes(this FileInfo fileInfo)
    {
        if (fileInfo == null)
        {
            throw new ArgumentNullException(nameof(fileInfo));
        }

        return fileInfo.Length / 1024 / 1024;
    }
}