using System.Data;
using System.Data.Common;
using Quartz;
using Quartz.Impl.AdoJobStore;
using Quartz.Util;

namespace ConsoleJobScheduler.Core.Infra.Data
{
    public static class ConnectionUtility
    {
        public static ConnectionAndTransactionHolder GetConnection(IsolationLevel isolationLevel, string dataSource)
        {
            DbConnection conn;
            DbTransaction tx;
            try
            {
                conn = DBConnectionManager.Instance.GetConnection(dataSource);
                conn.Open();
            }
            catch (Exception e)
            {
                throw new JobPersistenceException($"Failed to obtain DB connection from data source '{dataSource}': {e}", e);
            }

            try
            {
                tx = conn.BeginTransaction(isolationLevel);
            }
            catch (Exception e)
            {
                conn.Close();
                throw new JobPersistenceException("Failure setting up connection.", e);
            }

            return new ConnectionAndTransactionHolder(conn, tx);
        }
    }
}