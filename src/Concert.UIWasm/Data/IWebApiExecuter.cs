namespace Concert.UIWasm.Data
{
    public interface IWebApiExecuter
    {
        Task InvokePost<T>(string relativeUrl, T obj);
        Task<T?> InvokeGet<T>(string relativeUrl);
        Task InvokeDelete(string relativeUrl);
    }
}