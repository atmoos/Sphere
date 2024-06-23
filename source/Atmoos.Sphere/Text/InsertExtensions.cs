using static System.Threading.Tasks.ConfigureAwaitOptions;

namespace Atmoos.Sphere.Text;

public static class InsertExtensions
{
    /// <summary>
    /// Inserts a <paramref name="section"/> of text into the <paramref name="file"/> in-between the lines marked by the <paramref name="tag"/>.
    /// </summary>
    /// <param name="file">The file in which <paramref name="section"/> is to be replaced.</param>
    /// <param name="tag">The line tag indicating where to insert the text.</param>
    /// <param name="section">The text that is to be inserted.</param>
    public static void InsertSection(this FileInfo file, in LineTag tag, IEnumerable<String> section)
    {
        String temporaryCopy = CreateTemporaryCopy(file, in tag, section);
        File.Move(temporaryCopy, file.FullName, overwrite: true);

        static String CreateTemporaryCopy(FileInfo file, in LineTag tag, IEnumerable<String> lines)
        {
            var temporaryFile = $"{file.FullName}.tmp";
            try {
                using var source = file.OpenText();
                using var temporary = File.CreateText(temporaryFile);
                source.InsertSection(temporary, in tag, lines);
            }
            catch {
                File.Delete(temporaryFile);
                throw;
            }
            return temporaryFile;
        }
    }

    /// <summary>
    /// Inserts a <paramref name="section"/> of text into the <paramref name="file"/> in-between the lines marked by the <paramref name="tag"/>.
    /// </summary>
    /// <param name="file">The file in which <paramref name="section"/> is to be replaced.</param>
    /// <param name="tag">The line tag indicating where to insert the text.</param>
    /// <param name="section">The text that is to be inserted.</param>
    /// <param name="token">The cancellation token.</param>
    public static async Task InsertSectionAsync(this FileInfo file, LineTag tag, IEnumerable<String> section, CancellationToken token = default)
    {
        String temporaryCopy = await CreateTemporaryCopy(file, tag, section, token).ConfigureAwait(None);
        File.Move(temporaryCopy, file.FullName, overwrite: true);

        static async Task<String> CreateTemporaryCopy(FileInfo file, LineTag tag, IEnumerable<String> lines, CancellationToken token)
        {
            var temporaryFile = $"{file.FullName}.tmp";
            try {
                using var source = file.OpenText();
                using var temporary = File.CreateText(temporaryFile);
                await source.InsertSectionAsync(temporary, tag, lines, token).ConfigureAwait(None);
            }
            catch {
                File.Delete(temporaryFile);
                throw;
            }
            return temporaryFile;
        }
    }

    /// <summary>
    /// Inserts a <paramref name="section"/> of text into the <paramref name="destination"/> stream in-between the lines marked by the <paramref name="tag"/>.
    /// </summary>
    /// <param name="source">The current lines into which <paramref name="section"/> is to be inserted.</param>
    /// <param name="destination">The destination stream.</param>
    /// <param name="tag">The line tag indicating where to insert the text.</param>
    /// <param name="section">The text that is to be inserted.</param>
    public static void InsertSection(this TextReader source, TextWriter destination, in LineTag tag, IEnumerable<String> section)
        => destination.WriteLines(Insert.Section(source.ReadLines(), tag, section));

    /// <summary>
    /// Inserts a <paramref name="section"/> of text into the <paramref name="destination"/> stream in-between the lines marked by the <paramref name="tag"/>.
    /// </summary>
    /// <param name="source">The current lines into which <paramref name="section"/> is to be inserted.</param>
    /// <param name="destination">The destination stream.</param>
    /// <param name="tag">The line tag indicating where to insert the text.</param>
    /// <param name="section">The text that is to be inserted.</param>
    /// <param name="token">The cancellation token.</param>
    public static async Task InsertSectionAsync(this TextReader source, TextWriter destination, LineTag tag, IEnumerable<String> section, CancellationToken token = default)
        => await destination.WriteLinesAsync(Insert.Section(source.ReadLinesAsync(token), tag, section, token), token).ConfigureAwait(false);
}
