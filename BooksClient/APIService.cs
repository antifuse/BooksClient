using MARC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using Zoom.Net;
using Zoom.Net.YazSharp;

namespace BooksClient
{
    internal class ApiService
    {
        static readonly HttpClient Client = new HttpClient();

        Connection _blConnection;

        public ApiService()
        {
           
            _blConnection = new Connection("z3950cat.bl.uk", 9909)
            {
                DatabaseName = "BNB03U",
                Username = Properties.Settings.Default.blUsername,
                Password = Properties.Settings.Default.blPassword
            };

            _blConnection.Connect();
        }

        public Dictionary<string, string> SearchForIsbn(string isbn, string api)
        {
            switch (api)
            {
                case "DNB":
                    {
                        var searchdnb = SearchDnbAsync(isbn);
                        searchdnb.Wait();
                        if (searchdnb.Result.Count != 0) return searchdnb.Result;
                        goto case "BL";
                    }

                case "BL":
                    {
                        var searchbl = SearchBl(isbn);
                        return searchbl;
                    }
                
                
            }
            return null;
        }

        public async Task<Dictionary<string, string>> SearchDnbAsync(string isbn)
        {
            Console.WriteLine("es ist ich, deine schlechte debug-praxis");
            if (!Regex.IsMatch(isbn, @"^\d+$")) return new Dictionary<string, string>();
            var response = await Client.GetAsync("https://services.dnb.de/sru/dnb?operation=searchRetrieve&version=1.1&recordSchema=MARC21-xml&query=NUM%3D\"" + isbn + "\"").ConfigureAwait(false);

            var xdoc = XDocument.Parse(await response.Content.ReadAsStringAsync());
            Console.WriteLine(xdoc);
            Console.WriteLine(xdoc.Root.Element("{http://www.loc.gov/zing/srw/}numberOfRecords").Value);
            if (xdoc.Root.Element("{http://www.loc.gov/zing/srw/}numberOfRecords").Value == "0") return new Dictionary<string, string>();
            List<XElement> recs = new List<XElement>();
            foreach (XElement element in xdoc.Root.Element("{http://www.loc.gov/zing/srw/}records")?.Elements("{http://www.loc.gov/zing/srw/}record"))
            {
                recs.Add(element.Element("{http://www.loc.gov/zing/srw/}recordData")?.Element("{http://www.loc.gov/MARC21/slim}record"));
            }
            XDocument newdoc = new XDocument(new XElement("{http://www.loc.gov/MARC21/slim}collection"));
            newdoc.Root?.Add(recs);
            newdoc.Save("result.xml");
            FileMARCXml marc = new FileMARCXml(newdoc);

            
            MARC.Record rec = marc[0];
            Dictionary<string, string> data = new Dictionary<string, string>();
            if (rec["020"] == null) return data;
            Console.WriteLine(rec.ToString());
            data["author"] = ((DataField)rec["100"])?['a']?.Data ?? ((DataField)rec["700"])?['a']?.Data ?? ((DataField)rec["245"])?['c']?.Data;
            data["title"] = ((DataField)rec["245"])?['a']?.Data;
            data["subtitle"] = ((DataField)rec["245"])?['b']?.Data ?? ((DataField)rec["245"])?['n']?.Data;
            data["publisher"] = ((DataField)rec["264"])?['b']?.Data;
            data["year"] = ((DataField)rec["264"])?['c']?.Data;
            data["isbn"] = Regex.Match(((DataField)rec["020"])?['a']?.Data, @"\d+").Value;

            return data;

        }

        public Dictionary<string, string> SearchBl(string isbn)
        {
            if (!Regex.IsMatch(isbn, @"^\d+$")) return new Dictionary<string, string>();
            try
            {
                IResultSet results = (ResultSet)_blConnection.Search(new PrefixQuery("@attr 1=7 @attr 2=3 @attr 3=3 @attr 4=1 @attr 5=100 @attr 6=1 \"" + isbn + "\""));
                var firstResult = Encoding.UTF8.GetString(results[0].Content);
                File.WriteAllBytes("record.mrc", results[0].Content);
                FileMARC marc = new FileMARC();
                marc.ImportMARC("record.mrc");
                MARC.Record rec = marc[0];
                Dictionary<string, string> data = new Dictionary<string, string>();

                var author = ((DataField)rec["100"])?['a']?.Data ?? ((DataField)rec["700"])?['a']?.Data ?? ((DataField)rec["245"])?['c']?.Data;
                if (author != null) data["author"] = Regex.Replace(author, ",$", "");

                var title = ((DataField)rec["245"])?['a']?.Data;
                if (title != null) data["title"] = Regex.Replace(title, "[ ,:/]+$", "");
                var subtitle = ((DataField)rec["245"])?['b']?.Data ?? ((DataField)rec["245"])?['n']?.Data;
                if (subtitle != null) data["subtitle"] = Regex.Replace(subtitle, "[ ,:/]", "");
                var publisher = ((DataField)rec["264"])?['b']?.Data;
                if (publisher != null) data["publisher"] = Regex.Replace(publisher, ",$", "");
                data["year"] = (((DataField)rec["264"])?['c']?.Data)?.Replace(".", "");
                data["isbn"] = Regex.Match(((DataField)rec["020"])?['a']?.Data, @"\d+")?.Value;

                return data;
            } catch
            {
                MessageBox.Show("Melde dich an um die British Library Services zu nutzen.", "Authentifizierungsfehler", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return new Dictionary<string, string>();
            }
            
            
        }


    }
}
