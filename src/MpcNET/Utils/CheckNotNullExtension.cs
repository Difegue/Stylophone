using System;

namespace MpcNET.Utils
{
    public static class CheckNotNullExtension
    {
        public static void CheckNotNull(this object toBeChecked)
        {
            if (toBeChecked == null)
            {
                throw new ArgumentNullException(nameof(toBeChecked));
            }
        }
    }
}