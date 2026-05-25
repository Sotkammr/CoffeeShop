using CoffeeShopSales.Classes;
using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security;
using System.Text;

namespace CoffeeShop.Classes
{
    public static class ReceiptDocumentService
    {
        private static readonly CultureInfo RuCulture = CultureInfo.GetCultureInfo("ru-RU");
        private const string ReceiptFont = "Courier New";
        private const int ReceiptPageWidth = 4536;
        private const int ReceiptPageHeight = 15840;
        private const int ReceiptMargin = 180;
        private const int ReceiptLineWidth = 32;

        public static string CreateReceiptDocument(Sale sale)
        {
            if (sale == null)
                throw new ArgumentNullException(nameof(sale));

            string documentsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string receiptsDirectory = Path.Combine(documentsDirectory, "CoffeeShopReceipts");
            Directory.CreateDirectory(receiptsDirectory);

            string saleNumber = string.IsNullOrWhiteSpace(sale.SaleNumber) ? "Receipt" : sale.SaleNumber;
            string filePath = GetUniqueFilePath(
                receiptsDirectory,
                "Receipt_" + SanitizeFileName(saleNumber),
                ".docx");

            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
            {
                AddEntry(archive, "[Content_Types].xml", BuildContentTypesXml());
                AddEntry(archive, "_rels/.rels", BuildRootRelationshipsXml());
                AddEntry(archive, "docProps/core.xml", BuildCorePropertiesXml(sale));
                AddEntry(archive, "docProps/app.xml", BuildAppPropertiesXml());
                AddEntry(archive, "word/document.xml", BuildDocumentXml(sale));
            }

            return filePath;
        }

        private static string BuildContentTypesXml()
        {
            return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                   "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">" +
                   "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>" +
                   "<Default Extension=\"xml\" ContentType=\"application/xml\"/>" +
                   "<Override PartName=\"/word/document.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml\"/>" +
                   "<Override PartName=\"/docProps/core.xml\" ContentType=\"application/vnd.openxmlformats-package.core-properties+xml\"/>" +
                   "<Override PartName=\"/docProps/app.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.extended-properties+xml\"/>" +
                   "</Types>";
        }

        private static string BuildRootRelationshipsXml()
        {
            return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                   "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                   "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"word/document.xml\"/>" +
                   "<Relationship Id=\"rId2\" Type=\"http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties\" Target=\"docProps/core.xml\"/>" +
                   "<Relationship Id=\"rId3\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties\" Target=\"docProps/app.xml\"/>" +
                   "</Relationships>";
        }

        private static string BuildCorePropertiesXml(Sale sale)
        {
            string created = sale.SaleDate.ToUniversalTime()
                .ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
            string author = EscapeXml(sale.Cashier);
            string title = EscapeXml("\u0427\u0435\u043a " + sale.SaleNumber);

            return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                   "<cp:coreProperties xmlns:cp=\"http://schemas.openxmlformats.org/package/2006/metadata/core-properties\" " +
                   "xmlns:dc=\"http://purl.org/dc/elements/1.1/\" " +
                   "xmlns:dcterms=\"http://purl.org/dc/terms/\" " +
                   "xmlns:dcmitype=\"http://purl.org/dc/dcmitype/\" " +
                   "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
                   "<dc:title>" + title + "</dc:title>" +
                   "<dc:creator>" + author + "</dc:creator>" +
                   "<cp:lastModifiedBy>" + author + "</cp:lastModifiedBy>" +
                   "<dcterms:created xsi:type=\"dcterms:W3CDTF\">" + created + "</dcterms:created>" +
                   "<dcterms:modified xsi:type=\"dcterms:W3CDTF\">" + created + "</dcterms:modified>" +
                   "</cp:coreProperties>";
        }

        private static string BuildAppPropertiesXml()
        {
            return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                   "<Properties xmlns=\"http://schemas.openxmlformats.org/officeDocument/2006/extended-properties\" " +
                   "xmlns:vt=\"http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes\">" +
                   "<Application>CoffeeShop</Application>" +
                   "</Properties>";
        }

        private static string BuildDocumentXml(Sale sale)
        {
            var builder = new StringBuilder();
            builder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            builder.Append("<w:document xmlns:w=\"http://schemas.openxmlformats.org/wordprocessingml/2006/main\">");
            builder.Append("<w:body>");

            int totalItems = sale.Items == null ? 0 : sale.Items.Sum(item => item.Quantity);
            builder.Append(CreateParagraph("CoffeeShop", true, 24, "center", 40));
            builder.Append(CreateParagraph("\u041A\u0410\u0421\u0421\u041E\u0412\u042B\u0419 \u0427\u0415\u041A", true, 20, "center", 20));
            builder.Append(CreateParagraph("\u041F\u0420\u0418\u0425\u041E\u0414", false, 18, "center", 90));
            builder.Append(CreateSeparatorParagraph('='));
            builder.Append(CreateReceiptLineParagraph("\u0427\u0435\u043A", sale.SaleNumber, false));
            builder.Append(CreateReceiptLineParagraph("\u0414\u0430\u0442\u0430", sale.SaleDate.ToString("dd.MM.yyyy HH:mm"), false));
            builder.Append(CreateReceiptLineParagraph("\u041A\u0430\u0441\u0441\u0438\u0440", sale.Cashier, false));
            builder.Append(CreateReceiptLineParagraph("\u041E\u043F\u043B\u0430\u0442\u0430", sale.PaymentType, false));
            builder.Append(CreateSeparatorParagraph('-'));
            builder.Append(CreateParagraph(BuildReceiptLine("\u0422\u041E\u0412\u0410\u0420", "\u0421\u0423\u041C\u041C\u0410"), true, 17, "left", 0));
            builder.Append(CreateSeparatorParagraph('-'));
            builder.Append(CreateReceiptItems(sale));
            builder.Append(CreateSeparatorParagraph('-'));
            builder.Append(CreateReceiptLineParagraph("\u041F\u043E\u0437\u0438\u0446\u0438\u0439", totalItems.ToString(), false));
            builder.Append(CreateReceiptLineParagraph("\u0421\u043F\u043E\u0441\u043E\u0431 \u043E\u043F\u043B\u0430\u0442\u044B", sale.PaymentType, false));
            builder.Append(CreateReceiptLineParagraph("\u0418\u0422\u041E\u0413\u041E", FormatMoney(sale.TotalAmount), true));
            builder.Append(CreateSeparatorParagraph('='));
            builder.Append(CreateParagraph("\u0421\u041F\u0410\u0421\u0418\u0411\u041E \u0417\u0410 \u041F\u041E\u041A\u0423\u041F\u041A\u0423", true, 18, "center", 20));
            builder.Append(CreateParagraph("\u041D\u0435\u0444\u0438\u0441\u043A\u0430\u043B\u044C\u043D\u044B\u0439 \u044D\u043A\u0437\u0435\u043C\u043F\u043B\u044F\u0440", false, 14, "center", 0));

            builder.Append("<w:sectPr>");
            builder.AppendFormat(
                CultureInfo.InvariantCulture,
                "<w:pgSz w:w=\"{0}\" w:h=\"{1}\"/>",
                ReceiptPageWidth,
                ReceiptPageHeight);
            builder.AppendFormat(
                CultureInfo.InvariantCulture,
                "<w:pgMar w:top=\"{0}\" w:right=\"{1}\" w:bottom=\"{0}\" w:left=\"{1}\" w:header=\"0\" w:footer=\"0\" w:gutter=\"0\"/>",
                ReceiptMargin,
                ReceiptMargin);
            builder.Append("</w:sectPr>");

            builder.Append("</w:body>");
            builder.Append("</w:document>");
            return builder.ToString();
        }

        private static string CreateReceiptItems(Sale sale)
        {
            var items = sale.Items ?? Enumerable.Empty<SaleItem>();
            var builder = new StringBuilder();

            foreach (var item in items)
            {
                if (item == null)
                    continue;

                foreach (string line in WrapReceiptText(item.ProductName, ReceiptLineWidth))
                {
                    builder.Append(CreateParagraph(line, true, 17, "left", 0));
                }

                builder.Append(CreateParagraph(
                    BuildReceiptLine(
                        item.Quantity + " x " + FormatMoneyCompact(item.Price),
                        FormatMoneyCompact(item.Total)),
                    false,
                    17,
                    "left",
                    35));
            }

            return builder.ToString();
        }

        private static string CreateParagraph(
            string text,
            bool bold = false,
            int fontSize = 18,
            string alignment = "left",
            int spacingAfter = 100)
        {
            var builder = new StringBuilder();
            builder.Append("<w:p>");
            builder.Append("<w:pPr>");

            if (!string.IsNullOrWhiteSpace(alignment))
            {
                builder.AppendFormat(CultureInfo.InvariantCulture, "<w:jc w:val=\"{0}\"/>", alignment);
            }

            builder.AppendFormat(CultureInfo.InvariantCulture, "<w:spacing w:after=\"{0}\"/>", spacingAfter);
            builder.Append("</w:pPr>");
            builder.Append("<w:r>");
            builder.Append("<w:rPr>");

            if (bold)
            {
                builder.Append("<w:b/>");
                builder.Append("<w:bCs/>");
            }

            builder.Append("<w:rFonts w:ascii=\"" + ReceiptFont + "\" w:hAnsi=\"" + ReceiptFont + "\" w:cs=\"" + ReceiptFont + "\"/>");
            builder.AppendFormat(CultureInfo.InvariantCulture, "<w:sz w:val=\"{0}\"/><w:szCs w:val=\"{0}\"/>", fontSize);
            builder.Append("</w:rPr>");
            builder.AppendFormat("<w:t xml:space=\"preserve\">{0}</w:t>", EscapeXml(text));
            builder.Append("</w:r>");
            builder.Append("</w:p>");
            return builder.ToString();
        }

        private static string CreateSeparatorParagraph(char separator)
        {
            return CreateParagraph(new string(separator, ReceiptLineWidth), false, 16, "left", 30);
        }

        private static string CreateReceiptLineParagraph(string label, string value, bool bold)
        {
            string safeLabel = string.IsNullOrWhiteSpace(label) ? string.Empty : label.Trim();
            string safeValue = string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();
            return CreateParagraph(BuildReceiptLine(safeLabel + ":", safeValue), bold, bold ? 18 : 17, "left", 25);
        }

        private static string BuildReceiptLine(string left, string right)
        {
            string safeLeft = left ?? string.Empty;
            string safeRight = right ?? string.Empty;
            int spacesCount = ReceiptLineWidth - safeLeft.Length - safeRight.Length;

            if (spacesCount < 1)
                return safeLeft + " " + safeRight;

            return safeLeft + new string(' ', spacesCount) + safeRight;
        }

        private static string[] WrapReceiptText(string text, int maxLength)
        {
            string source = string.IsNullOrWhiteSpace(text) ? "-" : text.Trim();
            var words = source.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var lines = new System.Collections.Generic.List<string>();
            var currentLine = new StringBuilder();

            foreach (var word in words)
            {
                if (currentLine.Length == 0)
                {
                    currentLine.Append(word);
                    continue;
                }

                if (currentLine.Length + 1 + word.Length <= maxLength)
                {
                    currentLine.Append(' ').Append(word);
                }
                else
                {
                    lines.Add(currentLine.ToString());
                    currentLine.Clear();

                    if (word.Length <= maxLength)
                    {
                        currentLine.Append(word);
                    }
                    else
                    {
                        int index = 0;
                        while (index < word.Length)
                        {
                            int partLength = Math.Min(maxLength, word.Length - index);
                            string part = word.Substring(index, partLength);

                            if (partLength == maxLength)
                            {
                                lines.Add(part);
                            }
                            else
                            {
                                currentLine.Append(part);
                            }

                            index += partLength;
                        }
                    }
                }
            }

            if (currentLine.Length > 0)
                lines.Add(currentLine.ToString());

            return lines.Count == 0 ? new[] { "-" } : lines.ToArray();
        }

        private static void AddEntry(ZipArchive archive, string entryName, string content)
        {
            var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
            using (var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false)))
            {
                writer.Write(content);
            }
        }

        private static string EscapeXml(string value)
        {
            return SecurityElement.Escape(value ?? string.Empty) ?? string.Empty;
        }

        private static string FormatMoney(decimal value)
        {
            return string.Format(RuCulture, "{0:N2} \u0440\u0443\u0431.", value);
        }

        private static string FormatMoneyCompact(decimal value)
        {
            return string.Format(RuCulture, "{0:N2}", value);
        }

        private static string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var builder = new StringBuilder(fileName.Length);

            foreach (char character in fileName)
            {
                builder.Append(invalidChars.Contains(character) ? '_' : character);
            }

            return builder.ToString();
        }

        private static string GetUniqueFilePath(string directory, string baseFileName, string extension)
        {
            string filePath = Path.Combine(directory, baseFileName + extension);
            int index = 1;

            while (File.Exists(filePath))
            {
                filePath = Path.Combine(directory, baseFileName + "_" + index + extension);
                index++;
            }

            return filePath;
        }
    }
}
