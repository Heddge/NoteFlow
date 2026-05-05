namespace NoteFlow.Helpers
{
    public static class ElectronRuntimeUrlHelper
    {
        public const string LoopbackHost = "127.0.0.1";
        public const string LoopbackIpv6Host = "[::1]";

        public static string BuildLoopbackUrl(string port)
        {
            return $"http://{LoopbackHost}:{port}";
        }

        public static string BuildBindingUrls(string port)
        {
            return $"{BuildLoopbackUrl(port)};http://{LoopbackIpv6Host}:{port}";
        }
    }
}
