using System.Text;

namespace StudyHelperMVC.Services;

public class Chunker
{
    public List<string> Split(string input, int chunkSize)
    {
        var parts = new List<string>();
        if (string.IsNullOrEmpty(input)) return parts;

        for (int i = 0; i < input.Length; i += chunkSize)
        {
            int len = Math.Min(chunkSize, input.Length - i);
            parts.Add(input.Substring(i, len));
        }
        return parts;
    }
}
