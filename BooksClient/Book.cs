using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooksClient
{
    public class Book
    {
        public int id { get; set; }
        public string author { get; set; }
        public string title { get; set; }
        public string publisher { get; set; }
        public string isbn { get; set; }
        public string year { get; set; }
        public string subtitle { get; set; }

        public Book(string title)
        {
            this.title = title;
        }

        public Book() { }
    }
}
