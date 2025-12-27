namespace LeafBidAPI.Exceptions;

public class UserCreationFailedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the UserCreationFailedException class.
    /// </summary>
    public UserCreationFailedException()
    {
    }
    
    /// <summary>
    /// Initializes a new instance of the UserCreationFailedException class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public UserCreationFailedException(string message) : base(message)
    {
    }
    
    /// <summary>
    /// Initializes a new instance of the UserCreationFailedException class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public UserCreationFailedException(string message, Exception inner) : base(message, inner)
    {
    }
}