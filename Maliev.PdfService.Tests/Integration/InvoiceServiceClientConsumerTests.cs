using Maliev.MessagingContracts;
using Maliev.MessagingContracts.Contracts.Invoices;
using Maliev.PdfService.Api.Consumers;
using Maliev.PdfService.Api.Services;
using Maliev.PdfService.Domain.Entities;
using Maliev.PdfService.Infrastructure.Data;
using Maliev.PdfService.Tests.Fixtures;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Maliev.PdfService.Tests.Integration;

/// <summary>
/// Integration tests for invoice-created PDF consumer behavior.
/// </summary>
public class InvoiceServiceClientConsumerTests : IClassFixture<PdfServiceTestFactory>
{
    private readonly PdfServiceTestFactory _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="InvoiceServiceClientConsumerTests"/> class.
    /// </summary>
    /// <param name="factory">The PDF service test factory.</param>
    public InvoiceServiceClientConsumerTests(PdfServiceTestFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Verifies that an invoice-created event generates and uploads a PDF.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Consume_InvoiceCreatedEvent_GeneratesPdfAndUploads()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PdfDbContext>();
        var pdfGenerator = scope.ServiceProvider.GetRequiredService<IPdfGenerator>();
        var uploadService = scope.ServiceProvider.GetRequiredService<IUploadServiceClient>();
        var invoiceServiceClientMock = new Mock<IInvoiceServiceClient>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<InvoiceFinalizedConsumer>>();

        var invoiceId = Guid.NewGuid();
        invoiceServiceClientMock.Setup(c => c.GetInvoiceByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InvoiceDto
            {
                InvoiceId = invoiceId,
                InvoiceNumber = "INV-2026-0001",
                CustomerName = "Test Customer",
                IssueDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(30),
                SubTotalAmount = 1000,
                TaxAmount = 70,
                TotalAmount = 1070,
                Items = new List<InvoiceItemDto>
                {
                    new() { Description = "Test Item", Quantity = 1, UnitPrice = 1000, TotalAmount = 1000 }
                }
            });

        _factory.UploadServiceMock.Setup(s => s.UploadFileAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("http://storage.com/invoice.pdf");

        var consumer = new InvoiceFinalizedConsumer(pdfGenerator, uploadService, context, logger, invoiceServiceClientMock.Object);

        var evt = new InvoiceCreatedEvent(
            MessageId: Guid.NewGuid(),
            MessageName: "InvoiceCreatedEvent",
            MessageType: MessageType.Event,
            MessageVersion: "1.0",
            PublishedBy: "Invoice",
            ConsumedBy: new[] { "Pdf" },
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: true,
            Payload: new InvoiceCreatedEventPayload(
                InvoiceId: invoiceId,
                InvoiceNumber: "INV-2026-0001",
                OrderId: null,
                OrderNumber: null,
                CustomerId: Guid.NewGuid(),
                TotalAmount: 1070.0,
                Currency: "THB",
                DueDate: DateTimeOffset.UtcNow.AddDays(30),
                CreatedAt: DateTimeOffset.UtcNow
            )
        );

        var mockContext = new Mock<ConsumeContext<InvoiceCreatedEvent>>();
        mockContext.Setup(m => m.Message).Returns(evt);
        mockContext.Setup(m => m.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(mockContext.Object);

        var log = await context.GenerationRequests.FirstOrDefaultAsync(r => r.ReferenceId == invoiceId.ToString());
        Assert.NotNull(log);
        Assert.Equal(GenerationStatus.Completed, log.Status);
    }
}
