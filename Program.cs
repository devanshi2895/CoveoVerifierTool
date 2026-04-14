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

            var hotelResults     = new List<VerifyResult>();
            var excursionResults = new List<VerifyResult>();

            foreach (var id in hotelIds)
            {
                var result = service.VerifyAsync(service.BuildHotelRequest(sriggLeId: id), "HOTEL", id).GetAwaiter().GetResult();
                hotelResults.Add(result);
            }

            foreach (var id in excursionIds)
            {
                var result = service.VerifyAsync(service.BuildExcursionRequest(sriggLeId: id), "EXCURSION", id).GetAwaiter().GetResult();
                excursionResults.Add(result);
            }

            if (hotelResults.Count > 0)
                PrintTable("HOTEL RESULTS", hotelResults);

            if (excursionResults.Count > 0)
                PrintTable("EXCURSION RESULTS", excursionResults);

            PrintSummary(hotelResults, excursionResults);

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

        // ── Banner ──────────────────────────────────────────────────────────────

        static void PrintBanner()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║          COVEO VERIFIER TOOL — Sitecore Data Check       ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }

        // ── Results table ────────────────────────────────────────────────────────

        static void PrintTable(string heading, List<VerifyResult> results)
        {
            const int wNum    = 3;
            const int wId     = 12;
            const int wName   = 40;
            const int wItemId = 36;

            int wMissing = "Missing Keys".Length;
            foreach (var r in results)
            {
                List<string> m = GetMissing(r);
                string t = m.Count == 0 ? "None" : string.Join(", ", m);
                if (t.Length > wMissing) wMissing = t.Length;
            }

            string sep = string.Format("+-{0}-+-{1}-+-{2}-+-{3}-+-{4}-+",
                new string('-', wNum), new string('-', wId), new string('-', wName),
                new string('-', wItemId), new string('-', wMissing));

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  " + heading);
            Console.ResetColor();

            Console.WriteLine(sep);
            Console.WriteLine(string.Format("| {0} | {1} | {2} | {3} | {4} |",
                "#".PadRight(wNum),
                "SriggleId".PadRight(wId),
                "Name".PadRight(wName),
                "Item ID".PadRight(wItemId),
                "Missing Keys".PadRight(wMissing)));
            Console.WriteLine(sep);

            int idx = 0;
            int withMissing = 0;

            foreach (var r in results)
            {
                idx++;
                List<string> missing = GetMissing(r);
                if (missing.Count > 0) withMissing++;
                string missingText = missing.Count == 0 ? "None" : string.Join(", ", missing);

                Console.Write(string.Format("| {0} | {1} | {2} | {3} | ",
                    idx.ToString().PadRight(wNum),
                    Safe(r.SriggLeId).PadRight(wId),
                    Trunc(GetName(r), wName).PadRight(wName),
                    Trunc(Safe(r.ItemId), wItemId).PadRight(wItemId)));

                Console.ForegroundColor = missing.Count == 0 ? ConsoleColor.Green : ConsoleColor.Red;
                Console.Write(missingText.PadRight(wMissing));
                Console.ResetColor();
                Console.WriteLine(" |");
            }

            Console.WriteLine(sep);
            Console.WriteLine(string.Format("{0} of {1} records have missing GA fields",
                withMissing, results.Count));
        }

        // ── Summary ──────────────────────────────────────────────────────────────

        static void PrintSummary(List<VerifyResult> hotelResults, List<VerifyResult> excursionResults)
        {
            int hotelExists = 0, hotelMissing = 0;
            foreach (var r in hotelResults)
            {
                if (r.Exists) hotelExists++;
                if (r.MissingHotelKeys != null && r.MissingHotelKeys.Count > 0) hotelMissing++;
            }

            int excursionExists = 0, excursionMissing = 0;
            foreach (var r in excursionResults)
            {
                if (r.Exists) excursionExists++;
                if (r.MissingExcursionKeys != null && r.MissingExcursionKeys.Count > 0) excursionMissing++;
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  SUMMARY");
            Console.ResetColor();

            Console.WriteLine(string.Format(
                "Hotel     | Exists: {0,-3} | Total: {1,3} | Records with missing fields: {2}",
                hotelResults.Count == 0 ? "N/A" : (hotelExists > 0 ? "YES" : "NO"),
                hotelResults.Count, hotelMissing));
            Console.WriteLine(string.Format(
                "Excursion | Exists: {0,-3} | Total: {1,3} | Records with missing fields: {2}",
                excursionResults.Count == 0 ? "N/A" : (excursionExists > 0 ? "YES" : "NO"),
                excursionResults.Count, excursionMissing));
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        static string GetName(VerifyResult r)
        {
            if (r.Label == "HOTEL")
                return r.HotelInfo != null ? Safe(r.HotelInfo.Name) : string.Empty;
            return r.ExcursionInfo != null ? Safe(r.ExcursionInfo.Name) : string.Empty;
        }

        static List<string> GetMissing(VerifyResult r)
        {
            return r.Label == "HOTEL"
                ? (r.MissingHotelKeys ?? new List<string>())
                : (r.MissingExcursionKeys ?? new List<string>());
        }

        static string Safe(string s) => s ?? string.Empty;

        static string Trunc(string s, int max)
        {
            if (s == null || s.Length <= max) return s ?? string.Empty;
            return s.Substring(0, max - 3) + "...";
        }
    }
}
