using Maliev.PdfService.Api.Models.Data;

namespace Maliev.PdfService.Tests.TestData;

/// <summary>
/// Static test data fixtures for all PDF document types.
/// Covers English, Thai, and bilingual (Mixed) variants.
/// </summary>
internal static class DocumentTestData
{
    // ==================== INVOICE ====================

    // ==================== STANDARD INVOICE (ใบแจ้งหนี้) ====================

    public static InvoiceData InvoiceStandard_English => new()
    {
        InvoiceNumber = "INV-STD-EN-001",
        DocumentType = "Invoice",
        IssueDate = new DateTime(2026, 3, 15),
        DueDate = new DateTime(2026, 4, 14),
        PaymentTermsDays = 30,
        PoNumber = "PO-2026-0100",
        SellerName = "Maliev Co., Ltd.",
        SellerTaxId = "0125561001573",
        SellerAddress = "36/1 Moo 3, Khlong Khoi, Pak Kret, Nonthaburi 11120",
        SellerPhone = "+66 2 234 5678",
        CustomerName = "Tech Solutions Co., Ltd.",
        CustomerType = "Corporate",
        CustomerBranch = "สำนักงานใหญ่",
        CustomerTaxId = "0105548890002",
        BillingAddress = "100 Innovation Drive, Bangkok 10110",
        Currency = "THB",
        Subtotal = 85000m,
        TaxAmount = 5950m,
        GrandTotal = 90950m,
        LateFeePercentage = 1.5m,
        BankName = "Kasikorn Bank",
        BankAccountName = "Maliev Co., Ltd.",
        BankAccountNumber = "012-3-45678-9",
        BankBranch = "Silom Branch",
        Items = new List<InvoiceItemData>
        {
            new() { Index = 1, ItemCode = "FDM-SVC-005", Description = "FDM 3D Printing Service — ABS Material", Quantity = 50, Unit = "pcs", UnitPrice = 1500m, LineSubtotal = 75000m, LineTaxAmount = 5250m, LineTotal = 80250m },
            new() { Index = 2, ItemCode = "POST-001", Description = "Post-processing & Finishing", Quantity = 10, Unit = "pcs", UnitPrice = 1000m, LineSubtotal = 10000m, LineTaxAmount = 700m, LineTotal = 10700m }
        }
    };

    public static InvoiceData InvoiceStandard_Thai => new()
    {
        InvoiceNumber = "INV-STD-TH-001",
        DocumentType = "Invoice",
        IssueDate = new DateTime(2026, 3, 18),
        DueDate = new DateTime(2026, 4, 17),
        PaymentTermsDays = 45,
        SellerName = "บริษัท มาลีฟ จำกัด",
        SellerTaxId = "0125561001573",
        SellerAddress = "36/1 หมู่ 3 ตำบลคลองข่อย อำเภอปากเกร็ด นนทบุรี 11120",
        CustomerName = "บริษัท เทคโนโลยี พรีซิชั่น จำกัด",
        CustomerType = "Corporate",
        CustomerBranch = "สำนักงานใหญ่",
        CustomerTaxId = "0105557890124",
        BillingAddress = "88 ถนนพหลโยธิน กรุงเทพฯ 10400",
        Currency = "THB",
        Subtotal = 120000m,
        TaxAmount = 8400m,
        GrandTotal = 128400m,
        LateFeePercentage = 1.5m,
        BankName = "ธนาคารกสิกรไทย",
        BankAccountName = "บริษัท มาลีฟ จำกัด",
        BankAccountNumber = "012-3-45678-9",
        BankBranch = "สาขาสีลม",
        Items = new List<InvoiceItemData>
        {
            new() { Index = 1, ItemCode = "SLA-SVC-003", Description = "บริการพิมพ์ 3 มิติ SLA — วัสดุเรซิน", Quantity = 40m, Unit = "ชิ้น", UnitPrice = 3000m, LineSubtotal = 120000m, LineTaxAmount = 8400m, LineTotal = 128400m }
        }
    };

    // ==================== TAX INVOICE (ใบกำกับภาษี) ====================

    public static InvoiceData InvoiceEnglish_SingleItem => new()
    {
        InvoiceNumber = "INV-EN-001",
        DocumentType = "TaxInvoice",
        IssueDate = new DateTime(2026, 3, 1),
        DueDate = new DateTime(2026, 3, 31),
        PaymentTermsDays = 30,
        PoNumber = "PO-2026-0045",
        SellerName = "Maliev Co., Ltd.",
        SellerTaxId = "0125561001573",
        SellerAddress = "36/1 Moo 3, Khlong Khoi, Pak Kret, Nonthaburi 11120",
        SellerPhone = "+66 2 234 5678",
        CustomerName = "Precision Engineering Co., Ltd.",
        CustomerType = "Corporate",
        CustomerBranch = "สำนักงานใหญ่",
        CustomerTaxId = "0105548890001",
        BillingAddress = "55 Industrial Estate Rd, Lat Krabang, Bangkok 10520",
        Currency = "THB",
        Subtotal = 50000m,
        TaxAmount = 3500m,
        GrandTotal = 53500m,
        LateFeePercentage = 1.5m,
        BankName = "Kasikorn Bank",
        BankAccountName = "Maliev Co., Ltd.",
        BankAccountNumber = "012-3-45678-9",
        BankBranch = "Silom Branch",
        Items = new List<InvoiceItemData>
        {
            new() { Index = 1, ItemCode = "FDM-SVC-001", Description = "FDM 3D Printing Service — ABS Material", Quantity = 50, Unit = "pcs", UnitPrice = 1000m, LineSubtotal = 50000m, LineTaxAmount = 3500m, LineTotal = 53500m }
        }
    };

    public static InvoiceData InvoiceEnglish_ManyItems => new()
    {
        InvoiceNumber = "INV-EN-002",
        DocumentType = "TaxInvoice",
        IssueDate = new DateTime(2026, 2, 28),
        DueDate = new DateTime(2026, 3, 29),
        PaymentTermsDays = 30,
        QuotationReference = "Q-2026-0102",
        SellerName = "Maliev Co., Ltd.",
        SellerTaxId = "0125561001573",
        SellerAddress = "36/1 Moo 3, Khlong Khoi, Pak Kret, Nonthaburi 11120",
        CustomerName = "Auto Parts Manufacturer Ltd.",
        CustomerType = "Corporate",
        CustomerBranch = "สำนักงานใหญ่",
        CustomerTaxId = "0105541230099",
        BillingAddress = "88 Factory Road, Amata City, Chonburi 20000",
        Currency = "THB",
        Subtotal = 185000m,
        TotalDiscountAmount = 10500m,
        TaxAmount = 12950m,
        WithholdingTaxAmount = 5550m,
        WithholdingTaxPercentage = 3m,
        GrandTotal = 192400m,
        BankName = "Bangkok Bank",
        BankAccountName = "Maliev Co., Ltd.",
        BankAccountNumber = "091-0-12345-6",
        BankBranch = "Asoke Branch",
        LateFeePercentage = 1.5m,
        Items = Enumerable.Range(1, 12).Select(i => new InvoiceItemData
        {
            Index = i,
            ItemCode = $"CNC-{i:D3}",
            Description = $"CNC Machined Component — Part #{i:D3}",
            Quantity = i * 5m,
            Unit = "pcs",
            UnitPrice = 500m + i * 100m,
            DiscountPercentage = i % 3 == 0 ? 5m : 0m,
            TaxRate = 7m,
            LineSubtotal = (500m + i * 100m) * (i * 5m) * (i % 3 == 0 ? 0.95m : 1m),
            LineTaxAmount = (500m + i * 100m) * (i * 5m) * (i % 3 == 0 ? 0.95m : 1m) * 0.07m,
            LineTotal = (500m + i * 100m) * (i * 5m) * (i % 3 == 0 ? 0.95m : 1m) * 1.07m
        }).ToList()
    };

    public static InvoiceData InvoiceThai_SingleItem => new()
    {
        InvoiceNumber = "INV-TH-001",
        DocumentType = "TaxInvoice",
        IssueDate = new DateTime(2026, 3, 5),
        DueDate = new DateTime(2026, 4, 4),
        PaymentTermsDays = 30,
        SellerName = "Maliev Co., Ltd.",
        SellerTaxId = "0125561001573",
        SellerAddress = "36/1 หมู่ 3 ตำบลคลองข่อย อำเภอปากเกร็ด นนทบุรี 11120",
        CustomerName = "บริษัท ก้าวหน้าเทคโนโลยี จำกัด",
        CustomerType = "Corporate",
        CustomerBranch = "สำนักงานใหญ่",
        CustomerTaxId = "0105557890123",
        BillingAddress = "200 ถนนวิภาวดีรังสิต เขตดอนเมือง กรุงเทพฯ 10210",
        Currency = "THB",
        Subtotal = 75000m,
        TaxAmount = 5250m,
        GrandTotal = 80250m,
        LateFeePercentage = 1.5m,
        BankName = "ธนาคารกสิกรไทย",
        BankAccountName = "บริษัท เมเลียฟ จำกัด",
        BankAccountNumber = "012-3-45678-9",
        BankBranch = "สาขาสีลม",
        Items = new List<InvoiceItemData>
        {
            new() { Index = 1, ItemCode = "SLA-SVC-001", Description = "บริการพิมพ์ 3 มิติ SLA — วัสดุเรซิน", Quantity = 30m, Unit = "ชิ้น", UnitPrice = 2500m, LineSubtotal = 75000m, LineTaxAmount = 5250m, LineTotal = 80250m }
        }
    };

    public static InvoiceData InvoiceThai_ManyItems => new()
    {
        InvoiceNumber = "INV-TH-002",
        DocumentType = "TaxInvoice",
        IssueDate = new DateTime(2026, 3, 8),
        DueDate = new DateTime(2026, 4, 7),
        PaymentTermsDays = 30,
        SellerName = "Maliev Co., Ltd.",
        SellerTaxId = "0125561001573",
        SellerAddress = "36/1 หมู่ 3 ตำบลคลองข่อย อำเภอปากเกร็ด นนทบุรี 11120",
        CustomerName = "ห้างหุ้นส่วน สยามพาร์ท",
        CustomerType = "Corporate",
        CustomerTaxId = "0105545678901",
        BillingAddress = "456 ถนนพระราม 9 เขตสวนหลวง กรุงเทพฯ 10250",
        Currency = "THB",
        Subtotal = 124000m,
        TaxAmount = 8680m,
        GrandTotal = 132680m,
        LateFeePercentage = 1.5m,
        Notes = "ชำระเงินภายในกำหนด รับส่วนลด 2%",
        Items = Enumerable.Range(1, 15).Select(i => new InvoiceItemData
        {
            Index = i,
            ItemCode = $"FDM-{i:D3}",
            Description = $"ชิ้นส่วนพิมพ์ 3 มิติ FDM — ชุดที่ {i}",
            Quantity = i * 2m,
            Unit = "ชิ้น",
            UnitPrice = 300m + i * 50m,
            LineSubtotal = (300m + i * 50m) * (i * 2m),
            LineTaxAmount = (300m + i * 50m) * (i * 2m) * 0.07m,
            LineTotal = (300m + i * 50m) * (i * 2m) * 1.07m
        }).ToList()
    };

    public static InvoiceData InvoiceMixed_SingleItem => new()
    {
        InvoiceNumber = "INV-MX-001",
        DocumentType = "TaxInvoice",
        IssueDate = new DateTime(2026, 3, 1),
        DueDate = new DateTime(2026, 3, 31),
        PaymentTermsDays = 30,
        PoNumber = "PO-2026-0099",
        SellerName = "Maliev Co., Ltd.",
        SellerTaxId = "0125561001573",
        SellerAddress = "36/1 Moo 3, Khlong Khoi, Pak Kret, Nonthaburi 11120 / ถนนสีลม บางรัก กรุงเทพฯ",
        CustomerName = "International Manufacturing Co., Ltd. (บริษัท แมนูแฟคเจอร์ริ่ง จำกัด)",
        CustomerType = "Corporate",
        CustomerBranch = "สำนักงานใหญ่",
        CustomerTaxId = "0105567890123",
        BillingAddress = "789 Eastern Seaboard Industrial Estate, Rayong 21140",
        Currency = "THB",
        Subtotal = 120000m,
        TaxAmount = 8400m,
        WithholdingTaxAmount = 3600m,
        WithholdingTaxPercentage = 3m,
        GrandTotal = 124800m,
        LateFeePercentage = 1.5m,
        BankName = "Kasikorn Bank / ธนาคารกสิกรไทย",
        BankAccountName = "Maliev Co., Ltd.",
        BankAccountNumber = "012-3-45678-9",
        Items = new List<InvoiceItemData>
        {
            new() { Index = 1, ItemCode = "CNC-PROTO-001", Description = "CNC Prototype Machining (งานกลึง CNC ต้นแบบ)", Quantity = 3m, Unit = "pcs/ชิ้น", UnitPrice = 40000m, LineSubtotal = 120000m, LineTaxAmount = 8400m, LineTotal = 128400m }
        }
    };

    public static InvoiceData InvoiceMixed_ManyItems => new()
    {
        InvoiceNumber = "INV-MX-002",
        DocumentType = "TaxInvoice",
        IssueDate = new DateTime(2026, 2, 15),
        DueDate = new DateTime(2026, 3, 16),
        PaymentTermsDays = 30,
        QuotationReference = "Q-2026-0088",
        SellerName = "Maliev Co., Ltd.",
        SellerTaxId = "0125561001573",
        CustomerName = "Automotive Parts Co. / บริษัท ชิ้นส่วนยานยนต์ จำกัด",
        CustomerType = "Corporate",
        CustomerBranch = "สำนักงานใหญ่",
        CustomerTaxId = "0105578901234",
        BillingAddress = "100 Hemaraj Industrial Estate, Chonburi 20000",
        Currency = "THB",
        Subtotal = 246000m,
        TaxAmount = 17220m,
        WithholdingTaxAmount = 7380m,
        WithholdingTaxPercentage = 3m,
        GrandTotal = 255840m,
        LateFeePercentage = 1.5m,
        BankName = "Kasikorn Bank",
        BankAccountName = "Maliev Co., Ltd.",
        BankAccountNumber = "012-3-45678-9",
        Items = new List<InvoiceItemData>
        {
            new() { Index = 1, ItemCode = "FDM-001", Description = "FDM Printed Housing (เคสพิมพ์ FDM)", Quantity = 100m, Unit = "pcs", UnitPrice = 800m, LineSubtotal = 80000m, LineTaxAmount = 5600m, LineTotal = 85600m },
            new() { Index = 2, ItemCode = "SLA-001", Description = "SLA Resin Prototype (ต้นแบบเรซิน SLA)", Quantity = 20m, Unit = "pcs", UnitPrice = 2500m, LineSubtotal = 50000m, LineTaxAmount = 3500m, LineTotal = 53500m },
            new() { Index = 3, ItemCode = "CNC-001", Description = "CNC Aluminium Bracket (แบร็กเก็ตอลูมิเนียม)", Quantity = 30m, Unit = "pcs", UnitPrice = 2000m, LineSubtotal = 60000m, LineTaxAmount = 4200m, LineTotal = 64200m },
            new() { Index = 4, ItemCode = "POST-001", Description = "Post-processing & Finishing (งานหลังการผลิต)", Quantity = 1m, Unit = "lot", UnitPrice = 56000m, LineSubtotal = 56000m, LineTaxAmount = 3920m, LineTotal = 59920m }
        }
    };

    public static InvoiceData InvoiceThai_Individual => new()
    {
        InvoiceNumber = "INV-TH-003",
        DocumentType = "TaxInvoice",
        IssueDate = new DateTime(2026, 3, 10),
        DueDate = new DateTime(2026, 4, 9),
        PaymentTermsDays = 30,
        SellerName = "Maliev Co., Ltd.",
        SellerTaxId = "0125561001573",
        SellerAddress = "36/1 หมู่ 3 ตำบลคลองข่อย อำเภอปากเกร็ด นนทบุรี 11120",
        CustomerName = "สมชาย มีสุข",
        CustomerType = "Individual",
        CustomerTaxId = "1234567890123",
        BillingAddress = "200 ถนนสุขุมวิท แขวงคลองเตย เขตคลองเตย กรุงเทพฯ 10110",
        Currency = "THB",
        Subtotal = 15000m,
        TaxAmount = 1050m,
        GrandTotal = 16050m,
        LateFeePercentage = 1.5m,
        Items = new List<InvoiceItemData>
        {
            new() { Index = 1, ItemCode = "FDM-SVC-003", Description = "บริการพิมพ์ 3 มิติ FDM — ชิ้นส่วนต้นแบบ", Quantity = 10m, Unit = "ชิ้น", UnitPrice = 1500m, LineSubtotal = 15000m, LineTaxAmount = 1050m, LineTotal = 16050m }
        }
    };

    public static InvoiceData InvoiceThai_IndividualNoId => new()
    {
        InvoiceNumber = "INV-TH-004",
        DocumentType = "TaxInvoice",
        IssueDate = new DateTime(2026, 3, 12),
        DueDate = new DateTime(2026, 4, 11),
        PaymentTermsDays = 30,
        SellerName = "Maliev Co., Ltd.",
        SellerTaxId = "0125561001573",
        SellerAddress = "36/1 หมู่ 3 ตำบลคลองข่อย อำเภอปากเกร็ด นนทบุรี 11120",
        CustomerName = "วิชัย รักเงิน",
        CustomerType = "Individual",
        BillingAddress = "55 หมู่บ้านสวนธน ซอย ฉลองกรุง เขตบางคอแพล กรุงเทพฯ 10120",
        Currency = "THB",
        Subtotal = 8000m,
        TaxAmount = 560m,
        GrandTotal = 8560m,
        LateFeePercentage = 1.5m,
        Items = new List<InvoiceItemData>
        {
            new() { Index = 1, ItemCode = "SLA-SVC-002", Description = "บริการพิมพ์ 3 มิติ SLA — ต้นแบบเรซิน", Quantity = 4m, Unit = "ชิ้น", UnitPrice = 2000m, LineSubtotal = 8000m, LineTaxAmount = 560m, LineTotal = 8560m }
        }
    };

    // ==================== QUOTATION ====================

    public static QuotationData QuotationEnglish_SingleItem => new()
    {
        QuotationNumber = "Q-EN-001",
        VersionNumber = 1,
        QuotationDate = new DateTime(2026, 3, 1),
        ValidityStart = new DateTime(2026, 3, 1),
        ValidityEnd = new DateTime(2026, 3, 31),
        SellerName = "Maliev Co., Ltd.",
        SellerTaxId = "0125561001573",
        SellerAddress = "36/1 Moo 3, Khlong Khoi, Pak Kret, Nonthaburi 11120",
        SellerPhone = "+66 2 234 5678",
        SellerEmail = "info@maliev.com",
        CustomerName = "Precision Engineering Co., Ltd.",
        CustomerType = "Corporate",
        CustomerBranch = "สำนักงานใหญ่",
        CustomerTaxId = "0105548890001",
        CustomerAddress = "55 Industrial Estate Rd, Lat Krabang, Bangkok 10520",
        ContactPerson = "Mr. James Anderson",
        Currency = "THB",
        Subtotal = 45000m,
        TaxAmount = 3150m,
        TotalAmount = 48150m,
        DeliveryExpectations = "10–14 business days after order confirmation",
        Items = new List<QuotationItemData>
        {
            new() { Index = 1, MaterialName = "ABS Filament — Black", ManufacturingProcess = "FDM 3D Printing", Quantity = 50, QuantityUnit = "pcs", UnitPrice = 900m, LineTotal = 45000m, Notes = "Layer height 0.2mm, Infill 30%" }
        }
    };

    public static QuotationData QuotationEnglish_ManyItems => new()
    {
        QuotationNumber = "Q-EN-002",
        VersionNumber = 2,
        QuotationDate = new DateTime(2026, 2, 20),
        ValidityStart = new DateTime(2026, 2, 20),
        ValidityEnd = new DateTime(2026, 3, 20),
        SellerName = "Maliev Co., Ltd.",
        SellerTaxId = "0125561001573",
        SellerAddress = "36/1 Moo 3, Khlong Khoi, Pak Kret, Nonthaburi 11120",
        CustomerName = "Auto Parts Manufacturer Ltd.",
        CustomerType = "Corporate",
        CustomerBranch = "สำนักงานใหญ่",
        CustomerTaxId = "0105541230099",
        CustomerAddress = "88 Hemaraj Industrial Estate, Chonburi 20000",
        ContactPerson = "Ms. Sarah Chen",
        Currency = "THB",
        SubtotalBeforeDiscount = 320000m,
        TotalDiscount = 16000m,
        Subtotal = 304000m,
        TaxAmount = 21280m,
        TotalAmount = 325280m,
        DeliveryExpectations = "14–21 business days after PO receipt",
        SpecialTerms = "50% deposit upon order, 50% upon delivery. Prices valid for 30 days.",
        ChangeSummary = "Rev 2: Added post-processing line items, adjusted quantities per customer request.",
        Discounts = new List<QuotationDiscountData>
        {
            new() { DiscountType = "Percentage", DiscountValue = 5m, Conditions = "Volume discount for orders over 200 pcs" }
        },
        Items = Enumerable.Range(1, 10).Select(i => new QuotationItemData
        {
            Index = i,
            MaterialName = i <= 5 ? $"Aluminium 6061 — Component {i:D2}" : $"Post-Processing Service {i - 5}",
            ManufacturingProcess = i <= 5 ? "CNC Milling" : "Anodizing / Finishing",
            Quantity = i * 10m,
            QuantityUnit = "pcs",
            UnitPrice = 800m + i * 200m,
            LineTotal = (800m + i * 200m) * (i * 10m),
            Notes = i <= 3 ? $"Tolerance ±0.05mm, Ra 1.6" : null
        }).ToList()
    };

    public static QuotationData QuotationThai_SingleItem => new()
    {
        QuotationNumber = "Q-TH-001",
        VersionNumber = 1,
        QuotationDate = new DateTime(2026, 3, 5),
        ValidityStart = new DateTime(2026, 3, 5),
        ValidityEnd = new DateTime(2026, 4, 4),
        SellerName = "Maliev Co., Ltd.",
        SellerTaxId = "0125561001573",
        SellerAddress = "36/1 หมู่ 3 ตำบลคลองข่อย อำเภอปากเกร็ด นนทบุรี 11120",
        SellerPhone = "+66 2 234 5678",
        SellerEmail = "info@maliev.com",
        CustomerName = "บริษัท ก้าวหน้าเทคโนโลยี จำกัด",
        CustomerType = "Corporate",
        CustomerBranch = "สำนักงานใหญ่",
        CustomerTaxId = "0105557890123",
        CustomerAddress = "200 ถนนวิภาวดีรังสิต เขตดอนเมือง กรุงเทพฯ 10210",
        ContactPerson = "คุณสมชาย ใจดี",
        Currency = "THB",
        Subtotal = 60000m,
        TaxAmount = 4200m,
        TotalAmount = 64200m,
        DeliveryExpectations = "7–10 วันทำการหลังยืนยันคำสั่งซื้อ",
        Items = new List<QuotationItemData>
        {
            new() { Index = 1, MaterialName = "เรซิน — สีขาว", ManufacturingProcess = "SLA พิมพ์ 3 มิติ", Quantity = 30m, QuantityUnit = "ชิ้น", UnitPrice = 2000m, LineTotal = 60000m, Notes = "ความละเอียดสูง, เคลือบผิวเรียบ" }
        }
    };

    public static QuotationData QuotationThai_ManyItems => new()
    {
        QuotationNumber = "Q-TH-002",
        VersionNumber = 1,
        QuotationDate = new DateTime(2026, 3, 8),
        ValidityStart = new DateTime(2026, 3, 8),
        ValidityEnd = new DateTime(2026, 4, 7),
        SellerName = "Maliev Co., Ltd.",
        SellerTaxId = "0125561001573",
        SellerAddress = "36/1 หมู่ 3 ตำบลคลองข่อย อำเภอปากเกร็ด นนทบุรี 11120",
        CustomerName = "ห้างหุ้นส่วน สยามพาร์ท",
        CustomerType = "Corporate",
        CustomerTaxId = "0105545678901",
        CustomerAddress = "456 ถนนพระราม 9 เขตสวนหลวง กรุงเทพฯ 10250",
        Currency = "THB",
        Subtotal = 195000m,
        TaxAmount = 13650m,
        TotalAmount = 208650m,
        DeliveryExpectations = "14–21 วันทำการ",
        SpecialTerms = "ราคานี้ไม่รวมค่าขนส่ง กรุณาชำระล่วงหน้า 50% ก่อนเริ่มงาน",
        Items = Enumerable.Range(1, 12).Select(i => new QuotationItemData
        {
            Index = i,
            MaterialName = $"อลูมิเนียม — ชิ้นงาน #{i:D2}",
            ManufacturingProcess = i % 2 == 0 ? "กลึง CNC" : "กัด CNC",
            Quantity = i * 5m,
            QuantityUnit = "ชิ้น",
            UnitPrice = 500m + i * 100m,
            LineTotal = (500m + i * 100m) * (i * 5m)
        }).ToList()
    };

    public static QuotationData QuotationMixed_SingleItem => new()
    {
        QuotationNumber = "Q-MX-001",
        VersionNumber = 1,
        QuotationDate = new DateTime(2026, 3, 1),
        ValidityStart = new DateTime(2026, 3, 1),
        ValidityEnd = new DateTime(2026, 3, 30),
        SellerName = "Maliev Co., Ltd.",
        SellerTaxId = "0125561001573",
        SellerAddress = "36/1 Moo 3, Khlong Khoi, Pak Kret, Nonthaburi 11120 / 36/1 หมู่ 3 ตำบลคลองข่อย อำเภอปากเกร็ด นนทบุรี 11120",
        CustomerName = "International Manufacturing Co., Ltd. (บริษัท แมนูแฟคเจอร์ริ่ง จำกัด)",
        CustomerAddress = "789 Eastern Seaboard Industrial Estate, Rayong 21140",
        Currency = "THB",
        Subtotal = 180000m,
        TaxAmount = 12600m,
        TotalAmount = 192600m,
        DeliveryExpectations = "14 business days / 14 วันทำการ",
        Items = new List<QuotationItemData>
        {
            new() { Index = 1, MaterialName = "Titanium Grade 5 / ไทเทเนียม เกรด 5", ManufacturingProcess = "CNC 5-Axis Milling", Quantity = 5m, QuantityUnit = "pcs/ชิ้น", UnitPrice = 36000m, LineTotal = 180000m, Notes = "Tolerance ±0.02mm, Surface Ra 0.8" }
        }
    };

    public static QuotationData QuotationMixed_ManyItems => new()
    {
        QuotationNumber = "Q-MX-002",
        VersionNumber = 3,
        QuotationDate = new DateTime(2026, 2, 10),
        ValidityStart = new DateTime(2026, 2, 10),
        ValidityEnd = new DateTime(2026, 3, 11),
        SellerName = "Maliev Co., Ltd.",
        SellerTaxId = "0125561001573",
        CustomerName = "Automotive OEM Co. / บริษัท ยานยนต์ จำกัด",
        CustomerAddress = "100 Eastern Economic Corridor, Rayong 21140",
        Currency = "THB",
        SubtotalBeforeDiscount = 520000m,
        TotalDiscount = 26000m,
        Subtotal = 494000m,
        TaxAmount = 34580m,
        TotalAmount = 528580m,
        DeliveryExpectations = "21–28 business days / 21–28 วันทำการ after PO",
        SpecialTerms = "Volume pricing applied. Payment: 30% advance, 70% on delivery. Warranty: 12 months.",
        ChangeSummary = "Rev 3: Added scanning service, revised quantities for CNC items.",
        Discounts = new List<QuotationDiscountData>
        {
            new() { DiscountType = "Percentage", DiscountValue = 5m, Conditions = "Volume discount (order > 500 pcs)" }
        },
        Items = new List<QuotationItemData>
        {
            new() { Index = 1, MaterialName = "ABS Housing — Black (เคส ABS)", ManufacturingProcess = "FDM 3D Printing", Quantity = 200m, QuantityUnit = "pcs", UnitPrice = 800m, LineTotal = 160000m },
            new() { Index = 2, MaterialName = "SLA Prototype — Clear Resin (ต้นแบบใส)", ManufacturingProcess = "SLA 3D Printing", Quantity = 50m, QuantityUnit = "pcs", UnitPrice = 2800m, LineTotal = 140000m, Notes = "High-resolution, 50µm layer" },
            new() { Index = 3, MaterialName = "Aluminium 6061 Bracket (แบร็กเก็ต Al)", ManufacturingProcess = "CNC Milling", Quantity = 100m, QuantityUnit = "pcs", UnitPrice = 1500m, LineTotal = 150000m, Notes = "Tolerance ±0.05mm" },
            new() { Index = 4, MaterialName = "3D Scanning Service (งานสแกน 3D)", ManufacturingProcess = "3D Scanning", Quantity = 5m, QuantityUnit = "lot", UnitPrice = 14000m, LineTotal = 70000m }
        }
    };

    // ==================== RECEIPT ====================

    public static ReceiptData ReceiptEnglish_Cash => new()
    {
        ReceiptNumber = "RCP-EN-001",
        ReceiptDate = DateTime.UtcNow,
        CustomerName = "John Smith",
        CustomerType = "Individual",
        CustomerTaxId = "1234567890123",
        CustomerAddress = "123 Main Street, Bangkok 10110",
        PaymentMethod = "Cash",
        Items = new List<ReceiptItemData>
        {
            new() { Index = 1, Description = "Consulting Hours", Quantity = 5, UnitPrice = 200, TotalPrice = 1000 }
        },
        Subtotal = 1000,
        TaxAmount = 70,
        TotalAmount = 1070,
        Currency = "USD",
        CompanyName = "Maliev Co., Ltd."
    };

    public static ReceiptData ReceiptEnglish_Transfer => new()
    {
        ReceiptNumber = "RCP-EN-002",
        ReceiptDate = DateTime.UtcNow,
        CustomerName = "Precision Parts Co., Ltd.",
        CustomerType = "Corporate",
        CustomerBranch = "สำนักงานใหญ่",
        CustomerTaxId = "0105548890001",
        CustomerAddress = "55 Industrial Estate, Lat Krabang, Bangkok 10520",
        PaymentMethod = "Bank Transfer",
        ReferenceNumber = "TRF-2026-001234",
        Items = new List<ReceiptItemData>
        {
            new() { Index = 1, Description = "FDM 3D Printing Service - 50 pcs", Quantity = 50, UnitPrice = 250, TotalPrice = 12500 },
            new() { Index = 2, Description = "Post-processing & Finishing", Quantity = 1, UnitPrice = 2000, TotalPrice = 2000 }
        },
        Subtotal = 14500,
        TaxAmount = 1015,
        DiscountAmount = 500,
        TotalAmount = 15015,
        Currency = "THB",
        CompanyName = "Maliev Co., Ltd.",
        CompanyTaxId = "0125561001573",
        Notes = "Thank you for your business!"
    };

    public static ReceiptData ReceiptEnglish_CreditCard => new()
    {
        ReceiptNumber = "RCP-EN-003",
        ReceiptDate = DateTime.UtcNow,
        CustomerName = "Auto Parts Manufacturer",
        CustomerType = "Corporate",
        CustomerBranch = "สำนักงานใหญ่",
        CustomerTaxId = "0105541230099",
        PaymentMethod = "Credit Card",
        ReferenceNumber = "CC-****-****-****-1234",
        Items = new List<ReceiptItemData>
        {
            new() { Index = 1, Description = "CNC Machined Bracket - Aluminium", Quantity = 1, UnitPrice = 4500, TotalPrice = 4500 }
        },
        Subtotal = 4500,
        TaxAmount = 315,
        TotalAmount = 4815,
        Currency = "THB",
        CompanyName = "Maliev Co., Ltd."
    };

    public static ReceiptData ReceiptThai_Cash => new()
    {
        ReceiptNumber = "RCP-TH-001",
        ReceiptDate = DateTime.UtcNow,
        CustomerName = "สมชาย วงศ์สกุล",
        CustomerType = "Individual",
        CustomerTaxId = "1234567890123",
        CustomerAddress = "99 ถนนลาดพร้าว กรุงเทพฯ 10230",
        PaymentMethod = "เงินสด",
        Items = new List<ReceiptItemData>
        {
            new() { Index = 1, Description = "ค่าบริการให้คำปรึกษา", Quantity = 3, UnitPrice = 2000, TotalPrice = 6000 }
        },
        Subtotal = 6000,
        TaxAmount = 420,
        TotalAmount = 6420,
        Currency = "THB",
        CompanyName = "บริษัท ที่ปรึกษาอาชีพ จำกัด"
    };

    public static ReceiptData ReceiptThai_Transfer => new()
    {
        ReceiptNumber = "RCP-TH-002",
        ReceiptDate = DateTime.UtcNow,
        CustomerName = "บริษัท ไทย ซอฟต์แวร์ จำกัด",
        CustomerType = "Corporate",
        CustomerBranch = "สำนักงานใหญ่",
        CustomerTaxId = "0105559123456",
        CustomerAddress = "123 ถนนพหลโยธิน เขตดุสิต กรุงเทพฯ 10300",
        PaymentMethod = "โอนเงินผ่านธนาคาร",
        ReferenceNumber = "TRF-7700-12345-67890",
        Items = new List<ReceiptItemData>
        {
            new() { Index = 1, Description = "ใบอนุญาตซอฟต์แวร์ - รายปี", Quantity = 10, UnitPrice = 15000, TotalPrice = 150000 },
            new() { Index = 2, Description = "แพ็คเกจสนับสนุน - พรีเมียม", Quantity = 1, UnitPrice = 60000, TotalPrice = 60000 }
        },
        Subtotal = 210000,
        TaxAmount = 14700,
        DiscountAmount = 15000,
        TotalAmount = 209700,
        Currency = "THB",
        CompanyName = "บริษัท ที่ปรึกษาอาชีพ จำกัด",
        CompanyTaxId = "0105559123456",
        CompanyPhone = "02-123-4567",
        Notes = "ขอบคุณที่ใช้บริการครับ"
    };

    public static ReceiptData ReceiptMixed_Cash => new()
    {
        ReceiptNumber = "RCP-MX-001",
        ReceiptDate = DateTime.UtcNow,
        CustomerName = "Mr. Somsak Ltd. (บริษัท สมศักดิ์ จำกัด)",
        CustomerType = "Corporate",
        CustomerBranch = "สำนักงานใหญ่",
        CustomerTaxId = "0105567890123",
        CustomerAddress = "456 Sukhumvit Road, Bangkok 10110",
        PaymentMethod = "เงินสด / Cash",
        Items = new List<ReceiptItemData>
        {
            new() { Index = 1, Description = "Professional Consulting (บริการให้คำปรึกษา)", Quantity = 8, UnitPrice = 2500, TotalPrice = 20000 }
        },
        Subtotal = 20000,
        TaxAmount = 1400,
        TotalAmount = 21400,
        Currency = "THB",
        CompanyName = "Maliev Co., Ltd."
    };

    // ==================== DELIVERY NOTE ====================

    public static DeliveryNoteData DeliveryNoteEnglish_SingleItem => new()
    {
        DeliveryNoteNumber = "DN-EN-001",
        OrderNumber = "ORD-2026-001",
        CustomerName = "ABC Corporation",
        CustomerAddress = "100 Business Park, Suite 500, New York, NY 10001",
        DeliveryDate = DateTime.UtcNow.AddDays(3),
        TrackingNumber = "1Z999AA10123456784",
        CarrierName = "FedEx Express",
        DeliveryContact = "John Doe",
        DeliveryPhone = "+1-555-123-4567",
        Items = new List<DeliveryNoteItemData>
        {
            new()
            {
                ProductCode = "SRV-001",
                ProductName = "Server Rack Model X",
                QuantityOrdered = 1,
                QuantityManufactured = 1,
                QuantityDelivered = 1,
                UnitOfMeasure = "unit"
            }
        }
    };

    public static DeliveryNoteData DeliveryNoteEnglish_ManyItems => new()
    {
        DeliveryNoteNumber = "DN-EN-002",
        OrderNumber = "ORD-2026-002",
        CustomerName = "Tech Solutions Inc.",
        CustomerAddress = "500 Tech Drive, Building C, San Jose, CA 95110",
        DeliveryDate = DateTime.UtcNow.AddDays(5),
        TrackingNumber = "1Z999AA10123456785",
        CarrierName = "UPS Ground",
        Items = Enumerable.Range(1, 10).Select(i => new DeliveryNoteItemData
        {
            ProductCode = $"HW-{i:D3}",
            ProductName = $"Hardware Component {i}",
            QuantityOrdered = i * 10,
            QuantityManufactured = i * 10,
            QuantityDelivered = i * 10,
            UnitOfMeasure = "pcs"
        }).ToList()
    };

    public static DeliveryNoteData DeliveryNoteThai_SingleItem => new()
    {
        DeliveryNoteNumber = "DN-TH-001",
        OrderNumber = "ORD-2026-001",
        CustomerName = "บริษัท ก้าวหน้า จำกัด",
        CustomerAddress = "123 ถนนสุขุมวิท เขตวัฒนา กรุงเทพฯ 10260",
        DeliveryDate = DateTime.UtcNow.AddDays(2),
        TrackingNumber = "EMS-TH-1234567890",
        CarrierName = "ไปรษณีย์ไทย",
        DeliveryContact = "นายสมชาย ใจดี",
        DeliveryPhone = "02-123-4567",
        Items = new List<DeliveryNoteItemData>
        {
            new()
            {
                ProductCode = "SRV-001",
                ProductName = "เครื่องแม่ข่าย รุ่น X",
                QuantityOrdered = 2,
                QuantityManufactured = 2,
                QuantityDelivered = 2,
                UnitOfMeasure = "เครื่อง"
            }
        },
        Notes = "กรุณาติดต่อผู้รับก่อนจัดส่ง"
    };

    public static DeliveryNoteData DeliveryNoteThai_ManyItems => new()
    {
        DeliveryNoteNumber = "DN-TH-002",
        OrderNumber = "ORD-2026-002",
        CustomerName = "ห้างหุ้นส่วน สยามเทค",
        CustomerAddress = "88 ถนนพหลโยธิน เขตจตุจักร กรุงเทพฯ 10900",
        DeliveryDate = DateTime.UtcNow.AddDays(3),
        Items = Enumerable.Range(1, 8).Select(i => new DeliveryNoteItemData
        {
            ProductCode = $"HW-{i:D3}",
            ProductName = $"อุปกรณ์ฮาร์ดแวร์ {i}",
            QuantityOrdered = i * 5,
            QuantityManufactured = i * 5,
            QuantityDelivered = i * 5,
            UnitOfMeasure = "ชิ้น"
        }).ToList()
    };

    public static DeliveryNoteData DeliveryNoteMixed_SingleItem => new()
    {
        DeliveryNoteNumber = "DN-MX-001",
        OrderNumber = "ORD-2026-001",
        CustomerName = "International Trading Co., Ltd.",
        CustomerAddress = "789 Industrial Zone, Bangkok 10520",
        DeliveryDate = DateTime.UtcNow.AddDays(4),
        TrackingNumber = "DHL-789456123",
        CarrierName = "DHL Express",
        DeliveryContact = "Mr. Somsak (นายสมศักดิ์)",
        DeliveryPhone = "+66-81-234-5678",
        Items = new List<DeliveryNoteItemData>
        {
            new()
            {
                ProductCode = "SERVER-001",
                ProductName = "Enterprise Server (เครื่องแม่ข่ายระดับองค์กร)",
                QuantityOrdered = 3,
                QuantityManufactured = 3,
                QuantityDelivered = 3,
                UnitOfMeasure = "unit"
            },
            new()
            {
                ProductCode = "UPS-001",
                ProductName = "Uninterruptible Power Supply",
                QuantityOrdered = 3,
                QuantityManufactured = 3,
                QuantityDelivered = 3,
                UnitOfMeasure = "unit"
            }
        },
        Notes = "Please contact before delivery / กรุณาติดต่อก่อนจัดส่ง"
    };

    public static DeliveryNoteData DeliveryNoteEnglish_MultiPage => new()
    {
        DeliveryNoteNumber = "DN-EN-003",
        OrderNumber = "ORD-2026-003",
        CustomerName = "Tech Solutions Inc.",
        CustomerAddress = "500 Tech Drive, Building C, San Jose, CA 95110",
        DeliveryDate = DateTime.UtcNow.AddDays(5),
        TrackingNumber = "1Z999AA10123456785",
        CarrierName = "UPS Ground",
        DeliveryContact = "John Smith",
        DeliveryPhone = "+1-555-0123",
        Items = Enumerable.Range(1, 60).Select(i => new DeliveryNoteItemData
        {
            ProductCode = $"HW-{i:D3}",
            ProductName = $"Hardware Component Number {i} - This is a long product name to test wrapping",
            QuantityOrdered = i * 10,
            QuantityManufactured = i * 10,
            QuantityDelivered = i * 10,
            UnitOfMeasure = i % 2 == 0 ? "pcs" : "units"
        }).ToList(),
        Notes = "This is a test delivery note with many items to verify multi-page rendering. Please contact the delivery contact before arriving."
    };

    // ==================== JOB TICKET ====================

    public static JobTicketData JobTicketFdm_English => new()
    {
        JobTicketNumber = "JT-2026-00042",
        IssuedDate = new DateTime(2026, 3, 8),
        Priority = 2,
        JobId = "550e8400-e29b-41d4-a716-446655440000",
        OrderId = "550e8400-e29b-41d4-a716-446655440001",
        OrderReference = "ORD-2026-1234",
        DeliveryDeadline = new DateTime(2026, 3, 18),
        CustomerName = "Precision Engineering Co., Ltd.",
        PartName = "Mounting Bracket — Side Panel",
        MaterialName = "ABS Filament — Black",
        ColorName = "Matte Black",
        Quantity = 50,
        SurfaceFinishing = "As-printed, deburring",
        Technology = "FDM",
        AssignedMachine = "Bambu Lab X1C #3",
        VolumeCm3 = 42.5m,
        EstimatedMinutes = 185,
        ThreadTapRequired = true,
        InsertRequired = false,
        PartMarking = false,
        Requirements = "Layer height 0.2mm, 30% gyroid infill, 4 perimeters. All M4 threads to be tapped.",
        PreviewImages = null
    };

    public static JobTicketData JobTicketCnc_Thai => new()
    {
        JobTicketNumber = "JT-2026-00043",
        IssuedDate = new DateTime(2026, 3, 8),
        Priority = 1,
        JobId = "550e8400-e29b-41d4-a716-446655440002",
        OrderId = "550e8400-e29b-41d4-a716-446655440003",
        OrderReference = "ORD-2026-1235",
        DeliveryDeadline = new DateTime(2026, 3, 12),
        CustomerName = "บริษัท ยานยนต์ไทย จำกัด",
        PartName = "แผ่นยึดเครื่องยนต์ — อลูมิเนียม",
        MaterialName = "Aluminium 6061-T6",
        ColorName = "Natural / อโนไดซ์ใส",
        Quantity = 10,
        SurfaceFinishing = "Anodized Clear",
        Technology = "CNC",
        AssignedMachine = "Haas VF-2 #1",
        Tolerance = "±0.05mm",
        SurfaceRoughness = "Ra 1.6",
        InspectionType = "CMM Full Inspection",
        Requirements = "ต้องการ CMM inspection ทุกชิ้น พร้อม report. ห้ามมี burr ที่รูทุกรู",
        Notes = "งานเร่ง — ลูกค้าต้องการส่งภายใน 4 วัน",
        PreviewImages = null
    };

    public static JobTicketData JobTicketSla_Mixed => new()
    {
        JobTicketNumber = "JT-2026-00044",
        IssuedDate = new DateTime(2026, 3, 9),
        Priority = 4,
        JobId = "550e8400-e29b-41d4-a716-446655440004",
        OrderId = "550e8400-e29b-41d4-a716-446655440005",
        OrderReference = "ORD-2026-1240",
        DeliveryDeadline = new DateTime(2026, 3, 22),
        CustomerName = "International Design Studio / สตูดิโอดีไซน์",
        PartName = "Prototype Housing — Clear Resin (เคสต้นแบบ)",
        MaterialName = "SLA Resin — Clear ABS-Like",
        ColorName = "Clear / โปร่งใส",
        Quantity = 5,
        SurfaceFinishing = "Sanded to Ra 0.8 + clear coat",
        Technology = "SLA",
        AssignedMachine = "Formlabs Form 3+ #2",
        VolumeCm3 = 128.3m,
        EstimatedMinutes = 420,
        ThreadTapRequired = false,
        InsertRequired = true,
        PartMarking = true,
        Requirements = "Insert M3 brass heat-set inserts x4 per part. Mark part number on bottom surface. Crystal-clear finish required.",
        Notes = "Handle with care — clear resin scratches easily",
        PreviewImages = null
    };

    // ==================== FINANCIAL REPORT ====================

    public static FinancialReportData ReportEnglish_Simple => new()
    {
        ReportTitle = "Monthly Financial Summary",
        ReportNumber = "RPT-2026-001",
        ReportDate = DateTime.UtcNow,
        PeriodStart = DateTime.UtcNow.AddMonths(-1),
        PeriodEnd = DateTime.UtcNow,
        CompanyName = "Maliev Co., Ltd.",
        CompanyAddress = "36/1 Moo 3, Khlong Khoi, Pak Kret, Nonthaburi 11120",
        Sections = new List<ReportSection>
        {
            new()
            {
                SectionTitle = "Revenue",
                LineItems = new List<ReportLineItem>
                {
                    new() { Description = "Product Sales", Amount = 150000 },
                    new() { Description = "Service Revenue", Amount = 75000 }
                },
                SectionTotal = 225000
            },
            new()
            {
                SectionTitle = "Operating Expenses",
                LineItems = new List<ReportLineItem>
                {
                    new() { Description = "Salaries & Wages", Amount = 80000 },
                    new() { Description = "Rent & Utilities", Amount = 15000 },
                    new() { Description = "Marketing", Amount = 10000 }
                },
                SectionTotal = 105000
            }
        },
        TotalRevenue = 225000,
        TotalExpenses = 105000,
        NetProfit = 120000,
        Currency = "THB"
    };

    public static FinancialReportData ReportEnglish_Complex => new()
    {
        ReportTitle = "Annual Financial Report",
        ReportNumber = "RPT-2025-ANNUAL",
        ReportDate = DateTime.UtcNow,
        PeriodStart = new DateTime(2025, 1, 1),
        PeriodEnd = new DateTime(2025, 12, 31),
        CompanyName = "Maliev Co., Ltd.",
        CompanyAddress = "36/1 Moo 3, Khlong Khoi, Pak Kret, Nonthaburi 11120",
        Sections = new List<ReportSection>
        {
            new()
            {
                SectionTitle = "Revenue Streams",
                LineItems = new List<ReportLineItem>
                {
                    new() { Description = "Software Licenses", Amount = 2500000 },
                    new() { Description = "Cloud Services", Amount = 1800000 },
                    new() { Description = "Consulting", Amount = 950000 },
                    new() { Description = "Hardware Sales", Amount = 750000 }
                },
                SectionTotal = 6000000
            },
            new()
            {
                SectionTitle = "Cost of Goods Sold",
                LineItems = new List<ReportLineItem>
                {
                    new() { Description = "Infrastructure Costs", Amount = 800000 },
                    new() { Description = "Third-party Licenses", Amount = 400000 },
                    new() { Description = "Support Staff", Amount = 600000 }
                },
                SectionTotal = 1800000
            },
            new()
            {
                SectionTitle = "Operating Expenses",
                LineItems = new List<ReportLineItem>
                {
                    new() { Description = "Employee Salaries", Amount = 1500000 },
                    new() { Description = "R&D Investment", Amount = 500000 },
                    new() { Description = "Marketing & Sales", Amount = 300000 },
                    new() { Description = "Administrative", Amount = 200000 }
                },
                SectionTotal = 2500000
            }
        },
        TotalRevenue = 6000000,
        TotalExpenses = 4300000,
        NetProfit = 1700000,
        Currency = "THB"
    };

    public static FinancialReportData ReportThai_Simple => new()
    {
        ReportTitle = "รายงานการเงินรายเดือน",
        ReportNumber = "RPT-2026-001",
        ReportDate = DateTime.UtcNow,
        PeriodStart = DateTime.UtcNow.AddMonths(-1),
        PeriodEnd = DateTime.UtcNow,
        CompanyName = "บริษัท ก้าวหน้า จำกัด",
        CompanyAddress = "123 ถนนพหลโยธิน กรุงเทพฯ 10300",
        Sections = new List<ReportSection>
        {
            new()
            {
                SectionTitle = "รายได้",
                LineItems = new List<ReportLineItem>
                {
                    new() { Description = "การขายสินค้า", Amount = 1500000 },
                    new() { Description = "ค่าบริการ", Amount = 750000 }
                },
                SectionTotal = 2250000
            },
            new()
            {
                SectionTitle = "ค่าใช้จ่ายในการดำเนินงาน",
                LineItems = new List<ReportLineItem>
                {
                    new() { Description = "เงินเดือนพนักงาน", Amount = 800000 },
                    new() { Description = "ค่าเช่าและสาธารณูปโภค", Amount = 150000 },
                    new() { Description = "ค่าการตลาด", Amount = 100000 }
                },
                SectionTotal = 1050000
            }
        },
        TotalRevenue = 2250000,
        TotalExpenses = 1050000,
        NetProfit = 1200000,
        Currency = "THB"
    };

    public static FinancialReportData ReportThai_Complex => new()
    {
        ReportTitle = "รายงานการเงินประจำปี",
        ReportNumber = "RPT-2025-ANNUAL",
        ReportDate = DateTime.UtcNow,
        PeriodStart = new DateTime(2025, 1, 1),
        PeriodEnd = new DateTime(2025, 12, 31),
        CompanyName = "บริษัท ไทยเทคโนโลยี จำกัด",
        CompanyAddress = "88 ถนนวิภาวดีรังสิต กรุงเทพฯ 10900",
        Sections = new List<ReportSection>
        {
            new()
            {
                SectionTitle = "รายได้จากการดำเนินงาน",
                LineItems = new List<ReportLineItem>
                {
                    new() { Description = "การขายซอฟต์แวร์", Amount = 25000000 },
                    new() { Description = "บริการคลาวด์", Amount = 18000000 },
                    new() { Description = "บริการให้คำปรึกษา", Amount = 9500000 },
                    new() { Description = "การขายอุปกรณ์", Amount = 7500000 }
                },
                SectionTotal = 60000000
            },
            new()
            {
                SectionTitle = "ต้นทุนขาย",
                LineItems = new List<ReportLineItem>
                {
                    new() { Description = "ค่าโครงสร้างพื้นฐาน", Amount = 8000000 },
                    new() { Description = "ค่าลิขสิทธิ์บุคคลที่สาม", Amount = 4000000 },
                    new() { Description = "ค่าสนับสนุน", Amount = 6000000 }
                },
                SectionTotal = 18000000
            },
            new()
            {
                SectionTitle = "ค่าใช้จ่ายในการดำเนินงาน",
                LineItems = new List<ReportLineItem>
                {
                    new() { Description = "เงินเดือนพนักงาน", Amount = 15000000 },
                    new() { Description = "การวิจัยและพัฒนา", Amount = 5000000 },
                    new() { Description = "การตลาดและการขาย", Amount = 3000000 },
                    new() { Description = "การบริหาร", Amount = 2000000 }
                },
                SectionTotal = 25000000
            }
        },
        TotalRevenue = 60000000,
        TotalExpenses = 43000000,
        NetProfit = 17000000,
        Currency = "THB"
    };

    public static FinancialReportData ReportMixed_Simple => new()
    {
        ReportTitle = "Quarterly Report / รายงานรายไตรมาส",
        ReportNumber = "RPT-2026-Q1",
        ReportDate = DateTime.UtcNow,
        PeriodStart = new DateTime(2026, 1, 1),
        PeriodEnd = new DateTime(2026, 3, 31),
        CompanyName = "Asia Pacific Trading Co., Ltd.",
        CompanyAddress = "789 International Tower, Bangkok 10500",
        Sections = new List<ReportSection>
        {
            new()
            {
                SectionTitle = "Revenue / รายได้",
                LineItems = new List<ReportLineItem>
                {
                    new() { Description = "Domestic Sales (ขายในประเทศ)", Amount = 5000000 },
                    new() { Description = "Export Sales", Amount = 3000000 }
                },
                SectionTotal = 8000000
            },
            new()
            {
                SectionTitle = "Expenses / ค่าใช้จ่าย",
                LineItems = new List<ReportLineItem>
                {
                    new() { Description = "Operations / การดำเนินงาน", Amount = 3500000 },
                    new() { Description = "Administrative", Amount = 1500000 }
                },
                SectionTotal = 5000000
            }
        },
        TotalRevenue = 8000000,
        TotalExpenses = 5000000,
        NetProfit = 3000000,
        Currency = "THB"
    };
}
