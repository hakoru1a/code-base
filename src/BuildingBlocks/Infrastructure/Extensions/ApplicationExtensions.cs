using FluentValidation;
using Infrastructure.Common.Behavior;
using Infrastructure.Policies;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Mapster;
using MapsterMapper;

namespace Infrastructure.Extensions
{
    /// <summary>
    /// Extension methods cho Application Layer services - CQRS, Validation, Mapping
    /// 
    /// MỤC ĐÍCH:
    /// - Đăng ký các services cốt lõi của Application Layer theo Clean Architecture
    /// - Hỗ trợ CQRS pattern với MediatR (Commands & Queries)
    /// - Validation tự động với FluentValidation
    /// - Object mapping hiệu năng cao với Mapster
    /// - Pipeline behaviors: logging, validation, performance monitoring
    /// 
    /// SỬ DỤNG:
    /// 1. Basic usage:
    ///    services.AddGenericApplicationServices(typeof(CreateOrderCommand).Assembly);
    /// 
    /// 2. Với custom Mapster configuration:
    ///    services.AddApplicationServicesWithMapsterConfig<OrderMappingConfig>(typeof(CreateOrderCommand).Assembly);
    /// 
    /// IMPACT:
    /// + CQRS: Tách biệt rõ ràng Command (write) và Query (read), dễ scale và maintain
    /// + Validation: Tất cả requests được validate tự động trước khi xử lý
    /// + Performance: Mapster nhanh hơn AutoMapper 2-3 lần, reduce CPU usage
    /// + Behaviors: UnhandledExceptionBehaviour bắt lỗi, PerformanceBehaviour track slow queries
    /// - Memory: MediatR handler instances được cache, tốn memory nếu có nhiều handlers
    /// </summary>
    public static class ApplicationExtensions
    {
        /// <summary>
        /// Đăng ký tất cả Application services: MediatR, FluentValidation, Mapster, Pipeline Behaviors
        /// 
        /// CÁCH DÙNG:
        /// services.AddGenericApplicationServices(typeof(CreateOrderHandler).Assembly);
        /// 
        /// ĐĂNG KÝ:
        /// - MediatR: Scan assembly tìm tất cả Command/Query handlers
        /// - FluentValidation: Scan assembly tìm tất cả Validators
        /// - Mapster: High-performance object mapping
        /// - Pipeline Behaviors:
        ///   + UnhandledExceptionBehaviour: Bắt và log exception
        ///   + PerformanceBehaviour: Log warning nếu request chậm (>500ms)
        ///   + ValidationBehaviour: Auto validate request trước khi handle
        /// </summary>
        public static IServiceCollection AddGenericApplicationServices(this IServiceCollection services, Assembly? assembly = null)
        {
            var targetAssembly = assembly ?? Assembly.GetCallingAssembly();

            // Register Mapster - High performance mapping
            services.AddSingleton(TypeAdapterConfig.GlobalSettings);
            services.AddScoped<IMapper, ServiceMapper>();

            // Register MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(targetAssembly));

            // Register FluentValidation
            services.AddValidatorsFromAssembly(targetAssembly);

            // Register Pipeline Behaviors
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

            return services;
        }

        /// <summary>
        /// Đăng ký Application services với custom Mapster configuration
        /// 
        /// CÁCH DÙNG:
        /// services.AddApplicationServicesWithMapsterConfig<OrderMappingConfig>(typeof(CreateOrderHandler).Assembly);
        /// 
        /// VÍ DỤ MAPSTER CONFIG:
        /// public class OrderMappingConfig : IRegister
        /// {
        ///     public void Register(TypeAdapterConfig config)
        ///     {
        ///         config.NewConfig<Order, OrderDto>()
        ///             .Map(dest => dest.CustomerName, src => src.Customer.FullName)
        ///             .Map(dest => dest.TotalAmount, src => src.Items.Sum(i => i.Price * i.Quantity));
        ///     }
        /// }
        /// 
        /// PHÙ HỢP: Khi cần custom mapping phức tạp (nested objects, calculated fields)
        /// </summary>
        public static IServiceCollection AddApplicationServicesWithMapsterConfig<TMapsterConfig>(this IServiceCollection services, Assembly? assembly = null)
            where TMapsterConfig : class, new()
        {
            var targetAssembly = assembly ?? Assembly.GetCallingAssembly();

            // Configure Mapster with specific config class
            var mapsterConfig = new TMapsterConfig();
            if (mapsterConfig is IRegister register)
            {
                register.Register(TypeAdapterConfig.GlobalSettings);
            }

            return services.AddGenericApplicationServices(targetAssembly);
        }
    }
}