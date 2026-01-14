using Maliev.PdfService.Api.Services;
using Maliev.PdfService.Data.Data;
using Maliev.PdfService.Tests.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PdfProgram = Maliev.PdfService.Api.Program;

namespace Maliev.PdfService.Tests.Fixtures;

/// <summary>
/// Custom test factory for PdfService integration tests with UploadServiceClient mock.
/// </summary>
public class PdfServiceTestFactory : BaseIntegrationTestFactory<PdfProgram, PdfDbContext>
{
    public Mock<IUploadServiceClient> UploadServiceMock { get; } = new();

    protected override void ConfigureAdditionalServices(IServiceCollection services)
    {
        base.ConfigureAdditionalServices(services);

        // Remove existing IUploadServiceClient registration
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IUploadServiceClient));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }

        // Add mocked IUploadServiceClient
        services.AddScoped(_ => UploadServiceMock.Object);
    }
}
