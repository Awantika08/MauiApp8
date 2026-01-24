using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using MauiApp8.Entities;
using QuestColors = QuestPDF.Helpers.Colors;

namespace MauiApp8.Services;

public class PdfExportService
{
    public byte[] GenerateJournalPdf(List<JournalEntry> entries, DateTime start, DateTime end)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);

                page.Header().Column(col =>
                {
                    col.Item().Text("Journal Export").FontSize(20).SemiBold();
                    col.Item().Text($"Date Range: {start:yyyy-MM-dd} -> {end:yyyy-MM-dd}");
                    col.Item().LineHorizontal(1);
                });

                page.Content().Column(col =>
                {
                    if (entries.Count == 0)
                    {
                        col.Item().Text("No entries found in selected range.");
                        return;
                    }

                    foreach (var e in entries.OrderBy(x => x.EntryDate))
                    {
                        col.Item().PaddingVertical(8).Column(card =>
                        {
                            card.Item()
                                .Text($"{e.EntryDate:yyyy-MM-dd}  |  {e.Title}")
                                .SemiBold()
                                .FontSize(14);

                            card.Item().Text($"Primary Mood: {e.PrimaryMood?.Name ?? "-"}");
                            card.Item().Text($"Word Count: {e.WordCount}");

                            var tags = e.EntryTags?
                                .Select(t => t.Tag!.Name)
                                .ToList() ?? new List<string>();

                            card.Item().Text($"Tags: {(tags.Count == 0 ? "-" : string.Join(", ", tags))}");

                            card.Item()
                                .PaddingTop(4)
                                .Background(QuestColors.Grey.Lighten4)
                                .Padding(8)
                                .Text(e.Content ?? "");
                        });

                        col.Item().LineHorizontal(0.5f);
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Generated on ");
                  
                });
            });
        }).GeneratePdf();
    }
}
