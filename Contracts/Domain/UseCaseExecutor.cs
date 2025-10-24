using Contracts.Domain.Interface;
using DnsClient.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Domain
{
    public class UseCaseExecutor
    {
        public async Task<TResponse> Run<TRequest, TResponse>(
            IUseCase<TRequest, TResponse> useCase,
            TRequest request,
            CancellationToken ct)
        {
            Console.WriteLine("▶️ Start UseCase: " + useCase.GetType().Name);
            var result = await useCase.ExecuteAsync(request, ct);
            Console.WriteLine("✅ Success: " + useCase.GetType().Name);
            return result;
        }
    }
}
