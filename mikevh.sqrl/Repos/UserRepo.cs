using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mikevh.sqrl.Repos
{
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
            if(!_store.TryGetValue(user.idk, out var existing))
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
