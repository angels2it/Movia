using System.Threading.Tasks;
using Movia.Mobile.Models;
using Movia.Mobile.ViewModels;

namespace Movia.Mobile.Services
{
    public interface IAccountService
    {
        Task<LoginModelResult> Login(LoginModel model);
        UserData GetUserById(string id);
    }
}
