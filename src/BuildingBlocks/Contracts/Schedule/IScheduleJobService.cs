using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Schedule
{
    public interface IScheduleJobService
    {
        #region FireAndForget
        string Enqueue(Expression<Action> func);

        string Enqueue<T>(Expression<Action<T>> func);
        #endregion

        #region DelayJobs
        string Schedule(Expression<Action> func, TimeSpan delay);

        string Schedule<T>(Expression<Action<T>> func, TimeSpan delay);

        string Schedule(Expression<Action> func, DateTimeOffset enqueueAt);

        //string Schedule<T>(Expression<Action<T>> func, DateTimeOffset enqueueAt);

        #endregion

        #region ContinuosJobs
        string ContinueQueueWith(string parentJobId, Expression<Action> func);

        #endregion
        bool Delete(string jobId);
        bool Requeue(string jobId);
    }
}
