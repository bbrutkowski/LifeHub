namespace LifeHub.Application.Interfaces.Utils
{
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string hashedPassword, string password);
    }
}
