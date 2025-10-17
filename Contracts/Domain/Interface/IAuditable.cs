using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Domain.Interface
{
    public interface IAuditable<T> : IDateTracking, IUserTracking<T>
    {
    }
}
