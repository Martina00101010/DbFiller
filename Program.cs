﻿using System;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DbFiller
{
    class Program
    {
        static void Main(string[] args)
        {
            bool del;

            del = (args.Length == 1 && args[0] == "del") ? true : false;
            ReadDataFromCnb("https://www.cnb.cz/en/financial_markets/foreign_exchange_market/exchange_rate_fixing/year.txt?year=2017", del);
            ReadDataFromCnb("https://www.cnb.cz/en/financial_markets/foreign_exchange_market/exchange_rate_fixing/year.txt?year=2018", del);
        }
        public static void ReadDataFromCnb(string url, bool del)
        {
            WebClient w = new WebClient();
            string data = w.DownloadString(url);
            List<string> allRows = data.Split('\n').ToList();

            List<string> code = new List<string>();
            List<int> codeAm = new List<int>();
            List<string> allCodes = allRows[0].Split('|').ToList();
            ParseCodes(code, codeAm, allCodes);
            FillDb(code, codeAm, allRows, del);
        }

        public static void ParseCodes(List<string> code, List<int> codeAm, List<string> allCodes)
        {
            int k;
            int len;

            k = 0;
            len = allCodes.Count;
            while (++k < len)
            {
                code.Add(allCodes[k].Substring(allCodes[k].Length - 3));
                codeAm.Add(int.Parse(Regex.Match(allCodes[k], @"\d+").Value));
            }
        }

        public static void FillDb(List<string> code, List<int> codeAm, List<string> allRows, bool del)
        {
            using (var db = new CrownContext())
            {
                int row;
                int col;
                int i;
                int j;
                string[] ac;
                ExchangeRate er;

                CrownContext context = new CrownContext();
                CultureInfo ci = new CultureInfo("cs-CZ");
                i = 0;
                row = allRows.Count - 1;
                col = allRows[0].Split('|').Length;
                while (++i < row)
                {
                    j = 0;
                    ac = allRows[i].Split('|');
                    while (++j < col)
                    {
                        er = new ExchangeRate
                        {
                            Rate = Convert.ToDecimal(ac[j]) / codeAm[j - 1],
                            Date = DateTime.ParseExact(ac[0], "dd.MM.yyyy", ci),
                            Currency = code[j - 1]
                        };
                        bool q = db.ExchangeRates
                                .Any(i => i.Date == er.Date && i.Currency == er.Currency);
                        if (!q && !del)
                        {
                            db.ExchangeRates.Add(er);
                            Console.WriteLine("Adding {0:dd_MM_yyyy} {1:N6}\t{2}", er.Date, er.Rate, er.Currency);
                        }
                        else if (q && !del)
                            Console.WriteLine("Exists {0:dd_MM_yyyy} {1:N6}\t{2}", er.Date, er.Rate, er.Currency);
                        else if (q)
                        {
                            var r = db.ExchangeRates
                                 .Where(e => e.Date == er.Date && e.Currency == er.Currency)
                                 .First();
                            db.ExchangeRates.Remove(r);
                            Console.WriteLine("Deleting {0:dd_MM_yyyy} {1:N6}\t{2}", er.Date, er.Rate, er.Currency);
                        }
                    }
                }
                db.SaveChanges();
            }
        }
    }
}
