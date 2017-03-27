using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Movia.Mobile.Models;

namespace Movia.Mobile.Services
{
    public class AccountService : IAccountService
    {
        private readonly List<UserData> _users = new List<UserData>()
        {
            new UserData()
            {
                Id = "8367c658-40c5-317a-04b4-de0d05050505",
                Username = "U1",
                Password = "Pass1",
                Icon = "icon_u1"
            },
            new UserData()
            {
                Id = "d267c658-40c5-317a-04b4-de0f05050505",
                Username = "U2",
                Password = "Pass2",
                Icon = "icon_u2"
            },
            new UserData()
            {
                Id = "d267c658-40c5-317a-04b4-de0f05050506",
                Username = "U3",
                Password = "Pass3",
                Icon = "icon_u3"
            },
            new UserData()
            {
                Id = "d267c658-40c5-317a-04b4-de0f05050507",
                Username = "U4",
                Password = "Pass4",
                Icon = "icon_u4"
            },
            new UserData()
            {
                Id = "d267c658-40c5-317a-04b4-de0f05050508",
                Username = "U5",
                Password = "Pass5",
                Icon = "icon_u5"
            }
        };
        public Task<LoginModelResult> Login(LoginModel model)
        {
            var result = new LoginModelResult { Ok = false };
            if (_users.Any(e => e.Username == model.Username && e.Password == model.Password))
            {
                result.Ok = true;
                result.Id = _users.First(e => e.Username == model.Username && e.Password == model.Password).Id;
                return Task.FromResult(result);
            }
            return Task.FromResult(result);
        }

        public UserData GetUserById(string id)
        {
            return _users.FirstOrDefault(e => e.Id == id);
        }
    }
}