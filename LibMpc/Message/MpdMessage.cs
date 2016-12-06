namespace LibMpc
{
    public interface IMpdMessage
    {
        IMpdRequest Request { get; }
        IMpdResponse Response { get; }
    }

    public class MpdMessage : IMpdMessage
    {
        public MpdMessage(IMpcCommand command)
        {
            Request = new MpdRequest(command);
        }

        public void AddResponse(IMpdResponse response)
        {
            Response = response;
        }

        public IMpdRequest Request { get; }
        public IMpdResponse Response { get; private set; }
    }
}