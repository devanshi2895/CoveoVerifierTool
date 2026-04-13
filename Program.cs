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

            // Banner
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║          COVEO VERIFIER TOOL — Sitecore Data Check       ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

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

            // Hotel verification
            var hotelRequest = service.BuildHotelRequest();
            var hotelResult = service.VerifyAsync(hotelRequest, "HOTEL").GetAwaiter().GetResult();
            results.Add(hotelResult);

            // Excursion verification
            var excursionRequest = service.BuildExcursionRequest();
            var excursionResult = service.VerifyAsync(excursionRequest, "EXCURSION").GetAwaiter().GetResult();
            results.Add(excursionResult);

            // Summary table
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("══════════════════════════════════════════════════════════");
            Console.WriteLine("  SUMMARY");
            Console.WriteLine("══════════════════════════════════════════════════════════");
            Console.ResetColor();
            Console.WriteLine(string.Format("  {0,-20} {1,-8} {2}", "Label", "Exists", "Count"));
            Console.WriteLine("  " + new string('-', 40));

            foreach (var result in results)
            {
                if (result.Exists)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(string.Format("  {0,-20} {1,-8} {2}", result.Label, "YES", result.Count));
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(string.Format("  {0,-20} {1,-8} {2}", result.Label, "NO", result.Count));
                }
                Console.ResetColor();
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
