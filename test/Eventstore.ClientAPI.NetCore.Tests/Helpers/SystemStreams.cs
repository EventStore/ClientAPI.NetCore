namespace Eventstore.ClientAPI.Tests.Helpers
{
    public static class SystemStreams
    {
        public static bool IsSystemStream(string streamId)
        {
            return streamId.Length != 0 && streamId[0] == '$';
        }
    }
}