namespace Maliev.PdfService.Api.Models.Data;

/// <summary>
/// Data contract for Financial Report documents.
/// </summary>
public class FinancialReportData
{
    /// <summary>The report title.</summary>
    public string ReportTitle { get; set; } = string.Empty;
    /// <summary>The report number.</summary>
    public string ReportNumber { get; set; } = string.Empty;
    /// <summary>The date the report was generated.</summary>
    public DateTime ReportDate { get; set; }
    /// <summary>The start of the reporting period.</summary>
    public DateTime PeriodStart { get; set; }
    /// <summary>The end of the reporting period.</summary>
    public DateTime PeriodEnd { get; set; }
    /// <summary>The company name.</summary>
    public string CompanyName { get; set; } = string.Empty;
    /// <summary>The company address.</summary>
    public string? CompanyAddress { get; set; }
    /// <summary>The sections in the report.</summary>
    public List<ReportSection> Sections { get; set; } = new();
    /// <summary>Total revenue amount.</summary>
    public double TotalRevenue { get; set; }
    /// <summary>Total expenses amount.</summary>
    public double TotalExpenses { get; set; }
    /// <summary>Net profit (Revenue - Expenses).</summary>
    public double NetProfit { get; set; }
    /// <summary>The currency code (e.g., THB).</summary>
    public string Currency { get; set; } = "THB";
}

/// <summary>
/// Data contract for a section in a Financial Report.
/// </summary>
public class ReportSection
{
    /// <summary>The section title.</summary>
    public string SectionTitle { get; set; } = string.Empty;
    /// <summary>The line items in this section.</summary>
    public List<ReportLineItem> LineItems { get; set; } = new();
    /// <summary>The total amount for this section.</summary>
    public double SectionTotal { get; set; }
}

/// <summary>
/// Data contract for a line item in a Financial Report section.
/// </summary>
public class ReportLineItem
{
    /// <summary>The line item description.</summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>The amount.</summary>
    public double Amount { get; set; }
    /// <summary>Whether this item should be highlighted.</summary>
    public bool IsHighlight { get; set; }
}
