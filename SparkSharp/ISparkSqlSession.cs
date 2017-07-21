using System.Collections.Generic;
using System.Threading.Tasks;

namespace SparkSharp
{
    public interface ISparkSqlSession
    {
        Task<IEnumerable<T>> QuerySparkSqlAsync<T>(string sql);
    }
}