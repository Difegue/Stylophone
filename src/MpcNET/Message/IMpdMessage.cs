namespace MpcNET.Message
{
    public interface IMpdMessage<T>
    {
        IMpdRequest<T> Request { get; }
        IMpdResponse<T> Response { get; }

        bool IsResponseValid { get; }
    }
}