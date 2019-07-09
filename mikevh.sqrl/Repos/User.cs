using System;
using System.Collections.Generic;

namespace mikevh.sqrl.Repos
{
    public class User
    {
        public string idk { get; set; }
        public string suk { get; set; }
        public string vuk { get; set; }

        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public DateTime LastLoggedIn { get; set; }
        public int LoginCount { get; set; }
        public int UpdateCount { get; set; }
    }

    public interface IUserRepo
    {
        User Get(string idk);
        void Update(User user);
        void Add(User user);
        bool Remove(User user);
    }

    public class UserRepo : IUserRepo
    {
        private static readonly Dictionary<string, User> _store = new Dictionary<string, User>();

        public void Add(User user)
        {
            user.LoginCount = 1;
            _store.Add(user.idk, user);
        }

        public void Update(User user)
        {
            if (!_store.TryGetValue(user.idk, out var existing))
            {
                Add(user);
            }
            else
            {
                existing.LastLoggedIn = user.LastLoggedIn;
                existing.LoginCount = user.LoginCount;
                existing.Name = user.Name;
                existing.UpdatedOn = DateTime.Now;
                existing.suk = user.suk;
                existing.vuk = user.vuk;
            }
        }

        public User Get(string idk)
        {
            _store.TryGetValue(idk, out var user);
            return user;
        }

        public bool Remove(User user)
        {
            _store.Remove(user.idk);
            return true;
        }
    }
}