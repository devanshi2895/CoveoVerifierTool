using System;
using System.Collections.Generic;
using CoveoVerifierTool.Services;

namespace CoveoVerifierTool
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            PrintBanner();

            // Parse comma-separated IDs from --hotels and --excursions flags
            var hotelIds     = ParseArg(args, "--hotels");
            var excursionIds = ParseArg(args, "--excursions");

            if (hotelIds.Count == 0 && excursionIds.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("Usage  : CoveoVerifierTool.exe --hotels <id1,id2,...> --excursions <id1,id2,...>");
                Console.WriteLine("Example: CoveoVerifierTool.exe --hotels 29,30 --excursions 11650,11651");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("No IDs supplied — nothing to verify.");
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            CoveoService service;
            try
            {
                service = new CoveoService();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[FATAL] Could not initialise CoveoService: " + ex.Message);
                Console.ResetColor();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            var results = new List<VerifyResult>();

            foreach (var id in hotelIds)
            {
                var req    = service.BuildHotelRequest(sriggLeId: id);
                var result = service.VerifyAsync(req, "HOTEL", id).GetAwaiter().GetResult();
                results.Add(result);
            }

            foreach (var id in excursionIds)
            {
                var req    = service.BuildExcursionRequest(sriggLeId: id);
                var result = service.VerifyAsync(req, "EXCURSION", id).GetAwaiter().GetResult();
                results.Add(result);
            }

            PrintTable(results);

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        // ── CLI helpers ─────────────────────────────────────────────────────────

        static List<string> ParseArg(string[] args, string flag)
        {
            var list = new List<string>();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], flag, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var part in args[i + 1].Split(','))
                    {
                        var trimmed = part.Trim();
                        if (!string.IsNullOrEmpty(trimmed))
                            list.Add(trimmed);
                    }
                    break;
                }
            }
            return list;
        }

        // ── Output ──────────────────────────────────────────────────────────────

        static void PrintBanner()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║          COVEO VERIFIER TOOL — Sitecore Data Check       ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }

        static void PrintTable(List<VerifyResult> results)
        {
            // Column headers
            const string H_ID    = "SriggLeId";
            const string H_LABEL = "Label";
            const string H_COUNT = "Count";
            const string H_ITEM  = "Item ID";
            const string H_TITLE = "Title";

            // Compute natural widths from data
            int wId    = H_ID.Length;
            int wLabel = H_LABEL.Length;
            int wCount = H_COUNT.Length;
            int wItem  = H_ITEM.Length;
            int wTitle = H_TITLE.Length;

            foreach (var r in results)
            {
                wId    = Math.Max(wId,    Safe(r.SriggLeId).Length);
                wLabel = Math.Max(wLabel, Safe(r.Label).Length);
                wCount = Math.Max(wCount, r.Count.ToString().Length);
                wItem  = Math.Max(wItem,  Safe(r.ItemId).Length);
                wTitle = Math.Max(wTitle, Safe(r.Title).Length);
            }

            // Cap title column to prevent excessive line width
            const int MaxTitle = 60;
            wTitle = Math.Min(wTitle, MaxTitle);

            // Build separator line
            string sep = string.Format("+-{0}-+-{1}-+-{2}-+-{3}-+-{4}-+",
                new string('-', wId),
                new string('-', wLabel),
                new string('-', wCount),
                new string('-', wItem),
                new string('-', wTitle));

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  RESULTS");
            Console.ResetColor();
            Console.WriteLine(sep);

            // Header row
            Console.WriteLine(string.Format("| {0} | {1} | {2} | {3} | {4} |",
                H_ID.PadRight(wId),
                H_LABEL.PadRight(wLabel),
                H_COUNT.PadRight(wCount),
                H_ITEM.PadRight(wItem),
                H_TITLE.PadRight(wTitle)));

            Console.WriteLine(sep);

            // Data rows
            foreach (var r in results)
            {
                string title = Safe(r.Title);
                if (title.Length > wTitle)
                    title = title.Substring(0, wTitle - 3) + "...";

                Console.ForegroundColor = r.Exists ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine(string.Format("| {0} | {1} | {2} | {3} | {4} |",
                    Safe(r.SriggLeId).PadRight(wId),
                    Safe(r.Label).PadRight(wLabel),
                    r.Count.ToString().PadRight(wCount),
                    Safe(r.ItemId).PadRight(wItem),
                    title.PadRight(wTitle)));
                Console.ResetColor();
            }

            Console.WriteLine(sep);
        }

        static string Safe(string s) => s ?? string.Empty;
    }
}
