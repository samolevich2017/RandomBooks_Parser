using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RandomBook_Parser.Model {
    [DataContract]
    class Book {
        [IgnoreDataMember]
        public int ID { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Author { get; set; }
        [DataMember]
        public string Gener { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string PublishYear { get; set; }
        [DataMember]
        public string CoverUrl { get; set; }

        public Book() {

        }

        public Book(int id, string title, string author, string gener, string descr, string year, string url) {
            ID = id;
            Title = title;
            Author = author;
            Gener = gener;
            Description = descr;
            PublishYear = year;
            CoverUrl = url;
        }
    }
}
