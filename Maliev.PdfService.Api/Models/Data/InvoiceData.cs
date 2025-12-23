namespace Maliev.PdfService.Api.Models.Data;

public class InvoiceData
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public List<InvoiceItemData> Items { get; set; } = new();
}

public class InvoiceItemData
{
    public int Index { get; set; }
    public string Description { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public double TotalPrice { get; set; }
}
