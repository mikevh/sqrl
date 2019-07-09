namespace mikevh.sqrl.Repos
{
    public interface IUserRepo
    {
        User Get(string idk);
        void Update(User user);
        void Add(User user);
        bool Remove(User user);
    }
}