namespace LeafBidAPI.Exceptions;

public class BadRequestException : Exception
{
    /// <summary>
    /// Initializes a new instance of the NotFoundException class.
    /// </summary>
    public BadRequestException()
    {
    }
    
    /// <summary>
    /// Initializes a new instance of the NotFoundException class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public BadRequestException(string message) : base(message)
    {
    }
    
    /// <summary>
    /// Initializes a new instance of the NotFoundException class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public BadRequestException(string message, Exception inner) : base(message, inner)
    {
    }
}