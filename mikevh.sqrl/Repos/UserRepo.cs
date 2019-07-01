using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private readonly static Dictionary<string, User> _store = new Dictionary<string, User>();

        public void Add(User user)
        {
            if(!_store.ContainsKey(user.idk))
            {
                _store.Add(user.idk, user);
            }
        }

        public void Update(User user)
        {
            if(_store.TryGetValue(user.idk, out var storedUser))
            {
                // todo: update props
            }
        }

        public User Get(string idk)
        {
            _store.TryGetValue(idk, out var user);
            return user;
        }

        public bool Remove(User user)
        {
            if(_store.ContainsKey(user.idk))
            {
                _store.Remove(user.idk);
                return true;
            }
            return false;
        }
    }
}
