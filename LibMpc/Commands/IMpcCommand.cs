namespace LibMpc
{
    public interface IMpcCommand
    {
        string Value { get; }

        // TODO: Return IMpdResponse and create type-safe input.
        object ParseResponse(object response);
    }
}