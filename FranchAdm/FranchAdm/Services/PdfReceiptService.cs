using System;
using System.Drawing.Imaging;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using QRCoder;
using Font  = iTextSharp.text.Font;
using Image = iTextSharp.text.Image;

namespace FranchAdm.Services
{
    public static class PdfReceiptService
    {
        // Ссылка на Google-форму опроса (заменить на реальную при получении)
        private const string SurveyUrl = "https://forms.gle/CX6YFSp8o3X6dA5X6";

        public static byte[] Generate(ReceiptDataDto data)
        {
            using (var ms = new MemoryStream())
            {
                var doc = new Document(PageSize.A4, 50, 50, 60, 50);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                BaseFont bf      = LoadFont();
                var fTitle       = new Font(bf, 20, Font.BOLD);
                var fSection     = new Font(bf, 11, Font.BOLD);
                var fLabel       = new Font(bf, 10, Font.BOLD);
                var fValue       = new Font(bf, 10);
                var fCaption     = new Font(bf, 8, Font.ITALIC, BaseColor.GRAY);

                // ── Заголовок ──────────────────────────────────────────────────────────
                doc.Add(new Paragraph("ЧЕК ЗАЯВКИ НА ФРАНШИЗУ", fTitle)
                    { Alignment = Element.ALIGN_CENTER, SpacingAfter = 4 });
                doc.Add(HLine());

                // ── Номер и дата ───────────────────────────────────────────────────────
                var headerTbl = Table(2, new float[] { 55, 45 });
                headerTbl.SpacingBefore = 6;
                headerTbl.SpacingAfter  = 2;
                AddCell(headerTbl, $"№ заявки:  {data.OrderNumber}", fSection, Element.ALIGN_LEFT);
                AddCell(headerTbl, $"Дата:  {data.CreatedAt:dd.MM.yyyy}", fSection, Element.ALIGN_RIGHT);
                doc.Add(headerTbl);

                // ── Клиент ─────────────────────────────────────────────────────────────
                doc.Add(HLine());
                doc.Add(Section("КЛИЕНТ", fSection));
                doc.Add(Row("Имя:", data.ClientName, fLabel, fValue));
                doc.Add(Row("Email:", data.ClientEmail, fLabel, fValue));

                // ── Франшиза ───────────────────────────────────────────────────────────
                doc.Add(HLine());
                doc.Add(Section("ФРАНШИЗА", fSection));
                doc.Add(Row("Название:", data.FranchiseName, fLabel, fValue));
                doc.Add(Row("Франчайзер:", data.FranchiserName, fLabel, fValue));
                doc.Add(Row("Категория:", data.CategoryName, fLabel, fValue));

                // ── Финансовые условия ─────────────────────────────────────────────────
                doc.Add(HLine());
                doc.Add(Section("ФИНАНСОВЫЕ УСЛОВИЯ", fSection));
                doc.Add(Row("Итоговая сумма:", $"{data.FinalPrice:N0} руб.", fLabel, fValue));
                doc.Add(Row("Залог:", $"{data.PledgeAmount:N0} руб.", fLabel, fValue));
                doc.Add(Row("Роялти:", $"{data.RoyaltyPercent:N2}%", fLabel, fValue));
                if (data.DiscountPercent.HasValue && data.DiscountPercent.Value > 0)
                    doc.Add(Row("Скидка:", $"{data.DiscountPercent.Value:N2}%", fLabel, fValue));

                // ── Детали заявки ──────────────────────────────────────────────────────
                doc.Add(HLine());
                doc.Add(Section("ДЕТАЛИ ЗАЯВКИ", fSection));
                doc.Add(Row("Регион:", data.RegionName, fLabel, fValue));
                doc.Add(Row("Способ связи:", data.ContactMethodName, fLabel, fValue));

                // ── QR-код ─────────────────────────────────────────────────────────────
                doc.Add(HLine());
                doc.Add(new Paragraph("Отсканируйте QR-код для прохождения опроса:", fCaption)
                    { Alignment = Element.ALIGN_CENTER, SpacingBefore = 8, SpacingAfter = 6 });

                var qrImage = Image.GetInstance(GenerateQr(SurveyUrl));
                qrImage.ScaleAbsolute(90, 90);
                qrImage.Alignment = Element.ALIGN_CENTER;
                doc.Add(qrImage);

                doc.Add(new Paragraph(SurveyUrl, fCaption)
                    { Alignment = Element.ALIGN_CENTER, SpacingBefore = 4 });

                doc.Close();
                return ms.ToArray();
            }
        }

        // ── Вспомогательные методы ─────────────────────────────────────────────────────

        private static BaseFont LoadFont()
        {
            var fontsDir = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
            foreach (var name in new[] { "arial.ttf", "Arial.ttf", "ARIAL.TTF" })
            {
                var path = Path.Combine(fontsDir, name);
                if (File.Exists(path))
                    return BaseFont.CreateFont(path, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            }
            throw new InvalidOperationException(
                "Шрифт Arial не найден в системе. Требуется для отображения кириллицы в PDF.");
        }

        private static byte[] GenerateQr(string url)
        {
            var gen  = new QRCodeGenerator();
            var qrData = gen.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            var qr   = new QRCode(qrData);
            using (var bmp = qr.GetGraphic(5))
            using (var buf = new MemoryStream())
            {
                bmp.Save(buf, ImageFormat.Png);
                return buf.ToArray();
            }
        }

        private static PdfPTable HLine()
        {
            var t = new PdfPTable(1) { WidthPercentage = 100, SpacingBefore = 6, SpacingAfter = 6 };
            t.AddCell(new PdfPCell
            {
                Border          = Rectangle.BOTTOM_BORDER,
                BorderColor     = BaseColor.LIGHT_GRAY,
                BorderWidthBottom = 0.5f,
                Padding         = 0,
                MinimumHeight   = 1
            });
            return t;
        }

        private static Paragraph Section(string text, Font font)
            => new Paragraph(text, font) { SpacingAfter = 4 };

        private static Paragraph Row(string label, string value, Font fLabel, Font fValue)
        {
            var p = new Paragraph { SpacingAfter = 3 };
            p.Add(new Chunk(label + "  ", fLabel));
            p.Add(new Chunk(value ?? "—", fValue));
            return p;
        }

        private static PdfPTable Table(int cols, float[] widths)
        {
            var t = new PdfPTable(cols) { WidthPercentage = 100 };
            t.SetWidths(widths);
            return t;
        }

        private static void AddCell(PdfPTable table, string text, Font font, int align)
        {
            table.AddCell(new PdfPCell(new Phrase(text, font))
                { HorizontalAlignment = align, Border = PdfPCell.NO_BORDER });
        }
    }
}
