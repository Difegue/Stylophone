namespace MpcNET.Test
{
    using MpcNET.Message;

    public static class MpdMessageExtension
    {
        public static bool HasSuccessResponse<T>(this IMpdMessage<T> message)
        {
            return message.Response.Result.Connected &&
                   message.Response.Result.Status == "OK" &&
                   !message.Response.Result.Error &&
                   message.Response.Result.ErrorMessage == string.Empty &&
                   message.Response.Result.MpdError == string.Empty;
        }
    }
}