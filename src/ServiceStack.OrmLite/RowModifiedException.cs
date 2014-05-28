using System;

namespace ServiceStack.OrmLite
{
    /// <summary>
    /// Thrown when an update to a table with optimistic concurrency column(s) fails.
    /// </summary>
    public class RowModifiedException : Exception
    {
        private const string DefaultMessage = "The row was modified or deleted since the last read";

        public RowModifiedException()
            : base(DefaultMessage)
        {
        }

        public RowModifiedException(string message)
            : base(message)
        {
        }

        public RowModifiedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
