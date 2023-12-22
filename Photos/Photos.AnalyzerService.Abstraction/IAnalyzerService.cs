namespace Photos.AnalyzerService.Abstraction
{
    public interface IAnalyzerService
    {
        Task<dynamic> AnalyzeAsync(byte[] data);
    }
}
