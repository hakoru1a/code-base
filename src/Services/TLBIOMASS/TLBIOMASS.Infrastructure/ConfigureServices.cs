using Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TLBIOMASS.Domain.Agencies.Interfaces;
using TLBIOMASS.Domain.Companies.Interfaces;
using TLBIOMASS.Domain.Customers.Interfaces;
using TLBIOMASS.Domain.Landowners.Interfaces;
using TLBIOMASS.Domain.MaterialRegions.Interfaces;
using TLBIOMASS.Domain.Materials.Interfaces;
using TLBIOMASS.Domain.Payments.Interfaces;
using TLBIOMASS.Domain.Receivers.Interfaces;
using TLBIOMASS.Domain.WeighingTicketCancels.Interfaces;
using TLBIOMASS.Domain.WeighingTickets.Interfaces;
using TLBIOMASS.Infrastructure.Persistences;
using TLBIOMASS.Infrastructure.Repositories;

namespace TLBIOMASS.Infrastructure
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCommonInfrastructure<TLBIOMASSContext>(configuration);

            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<IReceiverRepository, ReceiverRepository>();
            services.AddScoped<IAgencyRepository, AgencyRepository>();
            services.AddScoped<IMaterialRepository, MaterialRepository>();
            services.AddScoped<ILandownerRepository, LandownerRepository>();
            services.AddScoped<IMaterialRegionRepository, MaterialRegionRepository>();
            services.AddScoped<IWeighingTicketRepository, WeighingTicketRepository>();
            services.AddScoped<IWeighingTicketPaymentRepository, WeighingTicketPaymentRepository>();
            services.AddScoped<IPaymentDetailRepository, PaymentDetailRepository>();
            services.AddScoped<IWeighingTicketCancelRepository, WeighingTicketCancelRepository>();

            return services;
        }
    }
}
