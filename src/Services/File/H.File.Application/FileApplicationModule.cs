using H.File.Application.Services;
using H.File.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using Volo.Abp.Modularity;

namespace H.File.Application;

[DependsOn(typeof(FileEntityFrameworkCoreModule))]
public class FileApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        // 绑定 MinIO 配置
        context.Services.Configure<MinioOptions>(configuration.GetSection("Minio"));

        var minioOptions = configuration.GetSection("Minio").Get<MinioOptions>() ?? new MinioOptions();

        // 注册 MinIO Client
        context.Services.AddSingleton<IMinioClient>(_ =>
        {
            var client = new MinioClient()
                .WithEndpoint(minioOptions.Endpoint)
                .WithCredentials(minioOptions.AccessKey, minioOptions.SecretKey);

            if (minioOptions.UseSsl)
                client = client.WithSSL();

            return client.Build();
        });

        // 注册存储服务
        context.Services.AddSingleton<MinioStorageService>();
    }
}
