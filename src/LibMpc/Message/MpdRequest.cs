namespace LibMpc
{
    public interface IMpdRequest<T>
    {
        IMpcCommand<T> Command { get; }
    }

    public class MpdRequest<T> : IMpdRequest<T>
    {
        public MpdRequest(IMpcCommand<T> command)
        {
            Command = command;
        }

        public IMpcCommand<T> Command { get; }
    }
}