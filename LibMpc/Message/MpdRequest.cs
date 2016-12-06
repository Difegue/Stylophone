namespace LibMpc
{
    public interface IMpdRequest
    {
        IMpcCommand Command { get; }
    }

    public class MpdRequest : IMpdRequest
    {
        public MpdRequest(IMpcCommand command)
        {
            Command = command;
        }

        public IMpcCommand Command { get; }
    }
}