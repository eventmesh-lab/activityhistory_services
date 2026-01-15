using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace activityhistory_application.Interfaces
{
    public interface IUserServices
    {
        Task<Guid> GetIdUserByEmailServices(string email);
    }
}
