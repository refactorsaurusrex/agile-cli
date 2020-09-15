namespace AgileCli.Services
{
    internal interface ITokenManager
    {
        void Store(string token);
        string Retrieve();
        string AppDataDirectory { get; }
    }
}