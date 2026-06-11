using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QRCoder;
using FranchiseAgregator.Models;

namespace FranchiseAgregator.Services
{
    public class PdfOrderService
    {
        public byte[] GenerateOrderPdf(Order order, string qrUrl)
        {
            byte[] qrCodeBytes = GenerateQrCode(qrUrl);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2.5f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Content().Column(col =>
                    {
                        col.Spacing(0);

                        // ── Шапка ──────────────────────────────────────────────
                        col.Item().AlignCenter().Column(header =>
                        {
                            header.Item()
                                .Text("FranchiseHub")
                                .FontSize(26).Bold().FontColor(Color.FromHex("#e85d2a"));
                            header.Item().PaddingTop(2)
                                .Text("Подтверждение заявки")
                                .FontSize(12).FontColor(Color.FromHex("#636e72"));
                        });

                        col.Item().PaddingTop(12).PaddingBottom(12)
                            .LineHorizontal(1.5f).LineColor(Color.FromHex("#e85d2a"));

                        // ── Номер и дата ────────────────────────────────────────
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("ЧЕК").FontSize(10)
                                    .FontColor(Color.FromHex("#b2bec3")).Bold();
                                c.Item().PaddingTop(2)
                                    .Text($"№ {order.OrderNumber}")
                                    .FontSize(16).Bold();
                            });
                            row.RelativeItem().AlignRight().Column(c =>
                            {
                                c.Item().Text("Дата формирования").FontSize(10)
                                    .FontColor(Color.FromHex("#b2bec3")).Bold();
                                c.Item().PaddingTop(2)
                                    .Text(DateTime.Now.ToString("dd.MM.yyyy HH:mm"))
                                    .FontSize(13);
                            });
                        });

                        col.Item().PaddingTop(12).PaddingBottom(12)
                            .LineHorizontal(1).LineColor(Color.FromHex("#dfe6e9"));

                        // ── Статус ──────────────────────────────────────────────
                        col.Item()
                            .Background(Color.FromHex("#e8f5e9"))
                            .Padding(10)
                            .Row(row =>
                            {
                                row.ConstantItem(5).Background(Color.FromHex("#2e7d32"));
                                row.RelativeItem().PaddingLeft(10)
                                    .Text($"✓  Статус: {order.OrderStatus?.Name ?? "Выполнена"}")
                                    .FontSize(13).Bold().FontColor(Color.FromHex("#2e7d32"));
                            });

                        col.Item().PaddingTop(16).PaddingBottom(6)
                            .Text("ЗАЯВКА").FontSize(10).Bold()
                            .FontColor(Color.FromHex("#b2bec3"));

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(160);
                                cols.RelativeColumn();
                            });

                            AddRow(table, "Клиент", order.Client?.FullName ?? "—");
                            AddRow(table, "Email", order.Client?.Email ?? "—");
                            AddRow(table, "Телефон", order.Client?.Phone ?? "—");
                            AddRow(table, "Регион", order.Region?.Name ?? "—");
                            AddRow(table, "Способ связи", order.ContactMethod?.Name ?? "—");
                            AddRow(table, "Дата создания", order.CreatedAt.ToString("dd.MM.yyyy HH:mm"));
                            if (!string.IsNullOrWhiteSpace(order.Comments))
                                AddRow(table, "Комментарий", order.Comments);
                        });

                        col.Item().PaddingTop(16).PaddingBottom(6)
                            .Text("ФРАНШИЗА").FontSize(10).Bold()
                            .FontColor(Color.FromHex("#b2bec3"));

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(160);
                                cols.RelativeColumn();
                            });

                            AddRow(table, "Название", order.Franchise?.Name ?? "—");
                            AddRow(table, "Категория", order.Franchise?.Category?.Name ?? "—");
                            AddRow(table, "Франчайзер", order.Franchise?.Franchiser?.Name ?? "—");
                            AddRow(table, "Паушальный взнос",
                                order.Franchise != null
                                    ? $"{order.Franchise.PledgeAmount:N0} руб."
                                    : "—");
                            AddRow(table, "Инвестиции",
                                order.Franchise != null
                                    ? $"{order.Franchise.InvestmentAmount:N0} руб."
                                    : "—");
                        });

                        col.Item().PaddingTop(16)
                            .LineHorizontal(1.5f).LineColor(Color.FromHex("#dfe6e9"));

                        // ── Итого ───────────────────────────────────────────────
                        col.Item().PaddingTop(12).PaddingBottom(12).Row(row =>
                        {
                            row.RelativeItem()
                                .Text("ИТОГО")
                                .FontSize(14).Bold().FontColor(Color.FromHex("#636e72"));
                            row.AutoItem()
                                .Text($"{order.TotalAmount:N0} руб.")
                                .FontSize(18).Bold().FontColor(Color.FromHex("#e85d2a"));
                        });

                        col.Item().LineHorizontal(1.5f).LineColor(Color.FromHex("#dfe6e9"));

                        // ── QR-код ──────────────────────────────────────────────
                        col.Item().PaddingTop(24).AlignCenter().Column(qrCol =>
                        {
                            qrCol.Item().AlignCenter().Width(130).Image(qrCodeBytes);
                            qrCol.Item().PaddingTop(8).AlignCenter()
                                .Text("Отсканируйте для подтверждения")
                                .FontSize(9).FontColor(Color.FromHex("#636e72"));
                            qrCol.Item().PaddingTop(2).AlignCenter()
                                .Text(qrUrl.Length > 60 ? qrUrl[..60] + "…" : qrUrl)
                                .FontSize(8).FontColor(Color.FromHex("#b2bec3"));
                        });
                    });
                });
            }).GeneratePdf();
        }

        private static void AddRow(TableDescriptor table, string label, string value)
        {
            table.Cell().Padding(6).Background(Color.FromHex("#f5f6fa"))
                .Text(label).Bold().FontColor(Color.FromHex("#636e72")).FontSize(10);
            table.Cell().Padding(6).Text(value).FontSize(11);
        }

        private static byte[] GenerateQrCode(string content)
        {
            using var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrData);
            return qrCode.GetGraphic(10);
        }
    }
}
