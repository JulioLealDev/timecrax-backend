namespace Timecrax.Api.Domain.Exceptions;

public sealed class DomainException : Exception
{
    public string? Field { get; }

    public DomainException(string message)
        : base(message)
    {
    }

    public DomainException(string message, string field)
        : base(message)
    {
        Field = field;
    }
}
