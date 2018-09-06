using System;
using System.Collections.Generic;
using System.Text;

namespace EventStore.ClientAPI.Exceptions
{
    /// <summary>
    /// Exception thrown if an operation expires before it can be scheduled. 
    /// </summary>
    public class OperationExpiredException : EventStoreConnectionException
    {
        /// <summary>
        /// Constructs a new <see cref="OperationExpiredException"/>. 
        /// </summary>
        public OperationExpiredException()
        {   
        }

        /// <summary>
        /// Constructs a new <see cref="OperationExpiredException"/>. 
        /// </summary>
        /// <param name="message"></param>
        public OperationExpiredException(string message) : base(message)
        {   
        }

        /// <summary>
        /// Constructs a new <see cref="OperationExpiredException"/>. 
        /// </summary>
        public OperationExpiredException(string message, Exception innerException):base(message, innerException)
        {
            
        }
    }
}
