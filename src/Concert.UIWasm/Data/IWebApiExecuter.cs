namespace Concert.UIWasm.Data
{
    public interface IWebApiExecuter
    {
        Task<T?> InvokeGet<T>(string relativeUrl);
    }
}