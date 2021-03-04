﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using CurrencyConverter.Enums;
using CurrencyConverter.Models;
using Newtonsoft.Json.Linq;

namespace CurrencyConverter.Helpers
{
    public static class RequestHelper
    {
        public const string EmptyBaseUrl = "https://api.currconv.com/api/v7/";
        public const string FreeBaseUrl = "https://free.currconv.com/api/v7/";

        public static List<Currency> GetAllCurrencies(string apiKey = null)
        {
            string url;
            if (string.IsNullOrEmpty(apiKey))
                url = EmptyBaseUrl + "currencies";
            else
                url = FreeBaseUrl + "currencies" + "?apiKey=" + apiKey;

            var jsonString = GetResponse(url);

            var data = JObject.Parse(jsonString)["results"].ToArray();
            return data.Select(item => item.First.ToObject<Currency>()).ToList();
        }

        public static List<Country> GetAllCountries(string apiKey = null)
        {
            string url;
            if (string.IsNullOrEmpty(apiKey))
                url = EmptyBaseUrl + "countries";
            else
                url = FreeBaseUrl + "countries" + "?apiKey=" + apiKey;

            var jsonString = GetResponse(url);

            var data = JObject.Parse(jsonString)["results"].ToArray();

            return data.Select(item => item.First.ToObject<Country>()).ToList();
        }

        public static List<CurrencyHistory> GetHistoryRange(CurrencyType from, CurrencyType to, string startDate, string endDate, string apiKey = null)
        {
            string url;
            if (string.IsNullOrEmpty(apiKey))
                url = EmptyBaseUrl + "convert?q=" + from + "_" + to + "&compact=ultra&date=" + startDate + "&endDate=" + endDate;
            else
                url = FreeBaseUrl + "convert?q=" + from + "_" + to + "&compact=ultra&date=" + startDate + "&endDate=" + endDate + "&apiKey=" + apiKey;

            var jsonString = GetResponse(url);
            var data = JObject.Parse(jsonString).First.ToArray();
            return (from item in data
                    let obj = (JObject)item
                    from prop in obj.Properties()
                    select new CurrencyHistory
                    {
                        Date = prop.Name,
                        ExchangeRate = item[prop.Name].ToObject<double>()
                    }).ToList();
        }

        public static CurrencyHistory GetHistory(CurrencyType from, CurrencyType to, string date, string apiKey = null)
        {
            string url;
            if (string.IsNullOrEmpty(apiKey))
                url = EmptyBaseUrl + "convert?q=" + from + "_" + to + "&compact=ultra&date=" + date;
            else
                url = FreeBaseUrl + "convert?q=" + from + "_" + to + "&compact=ultra&date=" + date + "&apiKey=" + apiKey;

            var jsonString = GetResponse(url);
            var data = JObject.Parse(jsonString);
            return data.Properties().Select(prop => new CurrencyHistory
            {
                Date = prop.Name,
                ExchangeRate = data[prop.Name][date].ToObject<double>()
            }).FirstOrDefault();
        }

        public static double ExchangeRate(CurrencyType from, CurrencyType to, string apiKey = null)
        {
            string url;
            if (string.IsNullOrEmpty(apiKey))
                url = EmptyBaseUrl + "convert?q=" + from + "_" + to + "&compact=y";
            else
                url = FreeBaseUrl + "convert?q=" + from + "_" + to + "&compact=y&apiKey=" + apiKey;

            var jsonString = GetResponse(url);
            return JObject.Parse(jsonString).First.First["val"].ToObject<double>();
        }

        private static string GetResponse(string url)
        {
            string jsonString;

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                jsonString = reader.ReadToEnd();
            }

            return jsonString;
        }
    }
}
