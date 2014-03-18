using System;

namespace ServiceStack.OrmLite
{
    /// <summary>
    /// Thrown when an update to a table with optimistic concurrency column(s) fails.
    /// </summary>
    public class RowModifiedException : Exception
    {
        public RowModifiedException()
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
