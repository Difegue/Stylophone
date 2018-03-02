using MpcNET.Message;

namespace MpcNET.Test
{
    public static class MpdMessageExtension
    {
        public static bool HasSuccessResponse<T>(this IMpdMessage<T> message)
        {
            return message.Response.State.Connected &&
                   message.Response.State.Status == "OK" &&
                   !message.Response.State.Error &&
                   message.Response.State.ErrorMessage == string.Empty &&
                   message.Response.State.MpdError == string.Empty;
        }
    }
}