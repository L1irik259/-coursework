using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QRCoder;
using FranchiseAgregator.Models;

namespace FranchiseAgregator.Services
{
    public class PdfOrderService
    {
        public byte[] GenerateOrderPdf(Order order)
        {
            byte[] qrCodeBytes = GenerateQrCode(order);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(inner =>
                            {
                                inner.Item()
                                    .Text($"Заявка #{order.OrderId}")
                                    .FontSize(22).Bold().FontColor(Color.FromHex("#e85d2a"));
                                inner.Item().PaddingTop(4)
                                    .Text($"Номер: {order.OrderNumber}")
                                    .FontSize(12).FontColor(Color.FromHex("#636e72"));
                                inner.Item().PaddingTop(2)
                                    .Text($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}")
                                    .FontSize(10).FontColor(Color.FromHex("#636e72"));
                            });
                            row.ConstantItem(110).Image(qrCodeBytes);
                        });
                        col.Item().PaddingTop(8).LineHorizontal(2).LineColor(Color.FromHex("#e85d2a"));
                    });

                    page.Content().PaddingTop(16).Column(col =>
                    {
                        col.Spacing(16);

                        col.Item()
                            .Background(Color.FromHex("#e8f5e9"))
                            .Padding(10)
                            .Row(row =>
                            {
                                row.ConstantItem(6).Background(Color.FromHex("#2e7d32"));
                                row.RelativeItem().PaddingLeft(12)
                                    .Text($"Статус: {order.OrderStatus?.Name ?? "Выполнен"}")
                                    .FontSize(13).Bold().FontColor(Color.FromHex("#2e7d32"));
                            });

                        col.Item().Text("Информация о заявке").FontSize(14).Bold();

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(170);
                                cols.RelativeColumn();
                            });

                            AddRow(table, "Дата создания", order.CreatedAt.ToString("dd.MM.yyyy HH:mm"));
                            AddRow(table, "Клиент", order.Client?.FullName ?? "—");
                            AddRow(table, "Email клиента", order.Client?.Email ?? "—");
                            AddRow(table, "Телефон клиента", order.Client?.Phone ?? "—");
                            AddRow(table, "Регион", order.Region?.Name ?? "—");
                            AddRow(table, "Способ связи", order.ContactMethod?.Name ?? "—");
                            AddRow(table, "Комментарий",
                                string.IsNullOrWhiteSpace(order.Comments) ? "—" : order.Comments);
                        });

                        col.Item().PaddingTop(8).Text("Информация о франшизе").FontSize(14).Bold();

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(170);
                                cols.RelativeColumn();
                            });

                            AddRow(table, "Франшиза", order.Franchise?.Name ?? "—");
                            AddRow(table, "Категория", order.Franchise?.Category?.Name ?? "—");
                            AddRow(table, "Франчайзер", order.Franchise?.Franchiser?.Name ?? "—");
                            AddRow(table, "Паушальный взнос",
                                order.Franchise != null ? $"{order.Franchise.PledgeAmount:N0} руб." : "—");
                            AddRow(table, "Инвестиции",
                                order.Franchise != null ? $"{order.Franchise.InvestmentAmount:N0} руб." : "—");
                            AddRow(table, "Итоговая сумма", $"{order.TotalAmount:N0} руб.");
                        });
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("FranchiseHub — Автоматически сформированный документ")
                                .FontSize(9).FontColor(Color.FromHex("#b2bec3"));
                        });
                });
            }).GeneratePdf();
        }

        private static void AddRow(TableDescriptor table, string label, string value)
        {
            table.Cell().Padding(7).Background(Color.FromHex("#f5f6fa"))
                .Text(label).Bold().FontColor(Color.FromHex("#636e72")).FontSize(10);
            table.Cell().Padding(7).Text(value).FontSize(11);
        }

        private static byte[] GenerateQrCode(Order order)
        {
            string qrText =
                $"Заявка #{order.OrderId}\n" +
                $"Номер: {order.OrderNumber}\n" +
                $"Франшиза: {order.Franchise?.Name}\n" +
                $"Клиент: {order.Client?.FullName}\n" +
                $"Дата: {order.CreatedAt:dd.MM.yyyy}\n" +
                $"Статус: {order.OrderStatus?.Name ?? "Выполнен"}\n" +
                $"Сумма: {order.TotalAmount:N0} руб.";

            using var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrData);
            return qrCode.GetGraphic(10);
        }
    }
}
