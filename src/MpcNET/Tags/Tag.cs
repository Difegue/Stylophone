namespace MpcNET.Tags
{
    public interface ITag
    {
        string Value { get; }
    }


    internal class Tag : ITag
    {
        internal Tag(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }
}