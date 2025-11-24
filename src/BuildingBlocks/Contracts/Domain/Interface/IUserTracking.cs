using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Domain.Interface
{
    public interface IUserTracking<T>
    {
        T? CreatedBy { get; set; }
        T? LastModifiedBy { get; set; }
    }
}
