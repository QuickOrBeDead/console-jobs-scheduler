using System.Transactions;

namespace ConsoleJobScheduler.Core.Infra.Data;

public static class TransactionScopeUtility
{
    public static TransactionScope CreateNewReadUnCommitted()
    {
        return new TransactionScope(TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted },
            TransactionScopeAsyncFlowOption.Enabled);
    }

    public static TransactionScope CreateNewReadCommitted()
    {
        return new TransactionScope(TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);
    }
}