using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace Maliev.PdfService.Api.Services;

/// <summary>
/// Implementation of <see cref="IInvoiceServiceClient"/>.
/// </summary>
public class InvoiceServiceClient : IInvoiceServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<InvoiceServiceClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="InvoiceServiceClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    /// <param name="logger">The logger.</param>
    public InvoiceServiceClient(HttpClient httpClient, ILogger<InvoiceServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<InvoiceDto?> GetInvoiceByIdAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/invoice/v1/invoices/{invoiceId}", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<InvoiceDto>(cancellationToken: cancellationToken);
            }

            _logger.LogWarning("Failed to fetch invoice {InvoiceId}. Status code: {StatusCode}", invoiceId, response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching invoice {InvoiceId}", invoiceId);
        }

        return null;
    }
}
