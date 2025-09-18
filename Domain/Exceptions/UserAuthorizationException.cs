namespace PrintingTools.Domain.Exceptions;

public class UserAuthorizationException : DomainException
{
    public Guid UserId { get; }
    public string Operation { get; }
    
    public UserAuthorizationException(Guid userId, string operation) 
        : base($"Пользователь {userId} не имеет прав для выполнения операции '{operation}'")
    {
        UserId = userId;
        Operation = operation;
    }
}