namespace AspNetCoreApp.Models;

public class FileNameValidator
{
    public string GetValidFileName(string suggestedFileName)
    {
        if (string.IsNullOrWhiteSpace(suggestedFileName))
        {
            return "file";
        }

        //validate fileName
        foreach (char invalidChar in Path.GetInvalidFileNameChars())
        {
            suggestedFileName = suggestedFileName.Replace(invalidChar, '_');
        }

        string? fileNameWithoutExtension = Path.GetFileNameWithoutExtension(suggestedFileName);
        if (string.IsNullOrWhiteSpace(fileNameWithoutExtension))
        {
            fileNameWithoutExtension = "file";
        }
        if (fileNameWithoutExtension.Length > 32)
        {
            fileNameWithoutExtension = fileNameWithoutExtension[..32];//fileNameWithoutExtension.Substring(0, 32);
        }

        string? extension = Path.GetExtension(suggestedFileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = string.Empty;
        }
        if (extension.Length > 16)
        {
            extension = extension[..16];//extension.Substring(0, 16);
        }

        string validFileName = fileNameWithoutExtension + extension;
        if (string.IsNullOrWhiteSpace(validFileName))
        {
            validFileName = "file";
        }

        return validFileName;
    }
}