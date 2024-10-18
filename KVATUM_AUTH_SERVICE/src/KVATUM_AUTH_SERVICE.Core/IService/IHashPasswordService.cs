namespace KVATUM_AUTH_SERVICE.Core.IService
{
    public interface IHashPasswordService
    {
        string Compute(string password);
    }
}