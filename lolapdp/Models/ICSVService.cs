namespace lolapdp.Models
{
    public interface ICSVService
    {
        List<T> ReadCSV<T>(string filePath) where T : class, new();
        void WriteCSV<T>(string filePath, List<T> data) where T : class;
    }
}
//dbl