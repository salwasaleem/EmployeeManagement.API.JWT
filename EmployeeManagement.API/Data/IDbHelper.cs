

using Microsoft.Data.SqlClient;

namespace EmployeeManagement.API.Data
{
    public interface IDbHelper
    {
        SqlConnection GetConnection();
    }
}
