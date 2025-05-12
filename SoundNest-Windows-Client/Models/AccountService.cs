using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundNest_Windows_Client.Models
{
    public interface IAccountService
    {
        Account CurrentUser { get; }
        bool IsAuthenticated { get; }
        void SaveUser(string name, string email, int role, int id, string aditionalInformation, string profileImagePath);
        void CloseSesion();
    }

    public class AccountService : IAccountService
    {
        private Account currentUser;

        public Account CurrentUser => currentUser;

        public bool IsAuthenticated => currentUser != null;

        public void SaveUser(string name, string email, int role, int id, string aditionalInformation, string profileImagePath)
        {
            currentUser = new Account(name, email, role, id, aditionalInformation, profileImagePath);
        }

        public void CloseSesion()
        {
            currentUser = null;
        }
    }
}
