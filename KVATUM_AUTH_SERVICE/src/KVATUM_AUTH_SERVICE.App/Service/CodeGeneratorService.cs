namespace KVATUM_AUTH_SERVICE.App.Service
{
    public static class CodeGeneratorService
    {
        public static string GenerateCode()
        {
            var rnd = new Random();
            return rnd.Next(100000, 999999).ToString();
        }
    }
}