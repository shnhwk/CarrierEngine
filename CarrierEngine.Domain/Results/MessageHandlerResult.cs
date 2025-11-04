namespace CarrierEngine.Domain.Results;

public record MessageHandlerResult<T> : BaseResult<T>
{

    /// <summary>
    /// Whether the message should be acknowledged (ACK) or negatively acknowledged (NACK).
    /// </summary>
    public MessageAcknowledgeAction AcknowledgeAction { get; init; } = MessageAcknowledgeAction.Ack;

    /// <summary>
    /// When NACKed, determines if the message should be requeued (true) or dead-lettered/discarded (false).
    /// Ignored when <see cref="AcknowledgeAction"/> is Ack.
    /// </summary>
    public bool Requeue { get; init; } = false;

    public static MessageHandlerResult<T> Success(T data) => new() { IsSuccess = true, Data = data };
    public static MessageHandlerResult<T> Failure(string message, MessageAcknowledgeAction acknowledgeAction = MessageAcknowledgeAction.Nack)
    {
        return new MessageHandlerResult<T> { IsSuccess = false, ErrorMessage = message, AcknowledgeAction = acknowledgeAction };
    }

    public enum MessageAcknowledgeAction
    {
        Ack,
        Nack
    }
}