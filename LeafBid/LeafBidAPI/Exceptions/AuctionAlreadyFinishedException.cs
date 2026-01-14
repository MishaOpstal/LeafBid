namespace LeafBidAPI.Exceptions;

public class AuctionAlreadyFinishedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the AuctionAlreadyFinishedException class.
    /// </summary>
    public AuctionAlreadyFinishedException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the AuctionAlreadyFinishedException class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public AuctionAlreadyFinishedException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the AuctionAlreadyFinishedException class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public AuctionAlreadyFinishedException(string message, Exception inner) : base(message, inner)
    {
    }
}