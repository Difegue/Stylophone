using System;

namespace LibMpc
{
    /// <summary>
    /// The MpdOutput class contains all attributes of an output device of the MPD.
    /// </summary>
    public class MpdOutput
    {
        private readonly int id;
        private readonly string name;
        private readonly bool enabled;
        /// <summary>
        /// The id of the output.
        /// </summary>
        public int Id { get { return this.id; } }
        /// <summary>
        /// The name of the output.
        /// </summary>
        public string Name { get { return this.name; } }
        /// <summary>
        /// If the output is enabled.
        /// </summary>
        public bool IsEnabled { get { return this.enabled; } }
        /// <summary>
        /// Creates a new MpdOutput object.
        /// </summary>
        /// <param name="id">The id of the output.</param>
        /// <param name="name">The name of the output.</param>
        /// <param name="enabled">If the output is enabled.</param>
        public MpdOutput(int id, string name, bool enabled)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            this.id = id;
            this.name = name;
            this.enabled = enabled;
        }
        /// <summary>
        /// Returns a string representation of the object mainly for debuging purpose.
        /// </summary>
        /// <returns>A string representation of the object.</returns>
        public override string ToString()
        {
            return this.id + "::" + this.name + "::" + this.enabled;
        }
    }
}
