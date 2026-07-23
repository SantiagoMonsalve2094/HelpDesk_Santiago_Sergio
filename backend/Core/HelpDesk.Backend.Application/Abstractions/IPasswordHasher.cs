namespace HelpDesk.Backend.Application.Abstractions;

public interface IPasswordHasher
{
    string Hash(string password);
}
