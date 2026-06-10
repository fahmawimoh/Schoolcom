using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SchoolCom.Application.Common.Interfaces;
using SchoolCom.Application.Features.Auth.Commands.Login;
using SchoolCom.Infrastructure.Identity;
using SchoolCom.Infrastructure.Persistence;
using SchoolCom.Infrastructure.Persistence.Interceptors;
using SchoolCom.Application.Features.SchoolClasses;
using SchoolCom.Application.Features.Messaging;
using SchoolCom.Application.Features.Sections;
using SchoolCom.Application.Features.Students;
using SchoolCom.Application.Features.Teachers;
using SchoolCom.Application.Features.Subjects;
using SchoolCom.Application.Features.Rooms;
using SchoolCom.Application.Features.RoomAssignments;
using SchoolCom.Application.Features.ClassSections;
using SchoolCom.Application.Features.Timetables;
using SchoolCom.Infrastructure.Persistence.Repositories;
using SchoolCom.Infrastructure.Services;

namespace SchoolCom.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment? environment = null)
    {
        var connectionString = PostgresConnectionBuilder.Build(configuration);
        var isDevelopment = environment?.IsDevelopment() == true;

        services.AddSingleton<AuditableEntityInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>());
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            });

            options.ConfigureWarnings(w =>
                w.Ignore(RelationalEventId.PendingModelChangesWarning));

            if (isDevelopment)
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
                options.LogTo(
                    Console.WriteLine,
                    [DbLoggerCategory.Database.Command.Name],
                    LogLevel.Information);
            }
        });

        var redisConnection = configuration.GetConnectionString("Redis");
        var useInMemoryCache = isDevelopment
            || configuration.GetValue("Cache:UseInMemory", false);

        if (useInMemoryCache || string.IsNullOrWhiteSpace(redisConnection))
        {
            services.AddDistributedMemoryCache();
        }
        else
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
            });
        }

        services.AddHttpContextAccessor();

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IWhatsAppService, WhatsAppBusinessService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IJwtTokenBlacklistService, JwtTokenBlacklistService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<ISchoolClassRepository, SchoolClassRepository>();
        services.AddScoped<ISectionRepository, SectionRepository>();
        services.AddScoped<IClassSectionRepository, ClassSectionRepository>();
        services.AddScoped<IRoomAssignmentRepository, RoomAssignmentRepository>();
        services.AddScoped<ITeacherRepository, TeacherRepository>();
        services.AddScoped<ISubjectRepository, SubjectRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<ITimetableRepository, TimetableRepository>();
        services.AddScoped<IMessagingRepository, MessagingRepository>();
        services.AddScoped<IMessagingAccessService, MessagingAccessService>();
        services.AddSingleton<IFileStorage, LocalFileStorageService>();

        return services;
    }
}
