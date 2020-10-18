using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;

namespace YOGIYOCrawler
{
    class Product
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]   // 데이터베이스에서 자동으로 값 부여된다는 아노테이션
        public int id { get; set; }

        public StoreType store { get; set; }
        public string category { get; set; }
        public string name { get; set; }
        public decimal price { get; set; }
        public string comment { get; set; }
        public string image { get; set; }
        //public byte[] image { get; set; }
    }
}
