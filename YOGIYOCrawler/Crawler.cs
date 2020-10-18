using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Net;
using System.Drawing;

//Selenium Library
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;
using System.Drawing.Imaging;

namespace YOGIYOCrawler
{
    public class Crawler
    {
        protected ChromeDriverService driverService = null;
        protected ChromeOptions options = null;
        protected ChromeDriver driver = null;

        protected DirectoryInfo _rootDir = null;

        public Crawler()
        {
            driverService = ChromeDriverService.CreateDefaultService();
            options = new ChromeOptions();

            Init();
        }

        private void Init()
        {
            driver = new ChromeDriver(driverService, options);
            driver.Navigate().GoToUrl("https://www.yogiyo.co.kr/mobile/#/");

            var wait = new WebDriverWait(driver,new TimeSpan(0,0,10));
            wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(@"/html/body/div[7]/div/div[14]/a"))).Click();
            //wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(@"//*[@class=""restaurant-list""]/div[1]"))).Click();
        }

        IReadOnlyCollection<IWebElement> elemsCache = null;
        public void GetData(string xpath)
        {
            //*[@id="menu"]/div/div[position()>1]
            elemsCache = driver.FindElementsByXPath(xpath);
            Console.WriteLine(elemsCache.Count);
            foreach(var e in elemsCache)
            {
                Console.WriteLine(e.Text);
                //e.Click();
            }
        }

        public void GetChild(string idx, string xpath)
        {
            int index = int.Parse(idx);
            elemsCache = elemsCache.ToArray()[index].FindElements(By.XPath(xpath));
            foreach(var child in elemsCache)
            {
                Console.WriteLine(child.Text);
            }
        }

        public void Click()
        {
            elemsCache.ToArray()[0].Click();
        }

        public void MakeList()
        {
            Console.Write(@"편의점 구분코드 입력 : ");
            var storeType = (StoreType)int.Parse(Console.ReadLine());
            Console.WriteLine($"{storeType} 선택!");

            // 편의점 화면 내 제품 카테고리들 elements
            var categories = driver.FindElements(By.XPath(@"//*[@id=""menu""]/div/div[position()>1]"));
            int catNum = categories.Count;
            float catIdx = 1;
            foreach (var category in categories)
            {

                // 카테고리명이 표시되는 헤더 부분 element
                var categoryHeading = category.FindElement(By.XPath(@"div[@class=""panel-heading""]"));
                Console.WriteLine($@"분류 ""{categoryHeading.Text}"" 크롤링 실시 - {(int)(catIdx++/catNum*100)} %");
                // 크롤링 제외 여부 묻기
                /*
                Console.Write("크롤링을 건너뛸까요? : ");
                string input = Console.ReadLine();
                if (input.Trim() == "y")
                    continue;
                */

                // 카테고리 내부 제품들 목록 elements
                var products = category.FindElements(By.XPath(@"descendant::li"));
                if(!products.ToArray()[0].Displayed)
                    categoryHeading.Click();

                List<Product> prodList = new List<Product>();
                foreach (var prod in products)
                {
                    var textArea = prod.FindElement(By.XPath("descendant::td[@class=\"menu-text\"]"));
                    var imgArea = prod.FindElement(By.XPath(@"descendant::*[@class=""photo""]"));


                    string name = textArea.FindElement(By.XPath(@"descendant::div[2]")).Text;
                    string comment = textArea.FindElement(By.XPath(@"descendant::div[3]")).Text;
                    var priceArea = textArea.FindElement(By.XPath(@"descendant::div[4]")).FindElements(By.XPath(@"descendant::span"));
                    decimal price = decimal.Parse(priceArea.ToArray()[0].Text[0..^1]);  // 0 - 정가, 1 - 할인가. 정가를 가져오도록 하드코딩
                    Console.WriteLine($"\t{name}({price}원):{comment}");

                    // 이미지 획득 descendant::*[@class="photo"]
                    //Byte[] res = null;
                    string imgURL = null;
                    if ( imgArea.Displayed )
                    {
                        string queryStr = imgArea.GetCssValue("background-image");
                        Regex rgx = new Regex(@"[-a-zA-Z0-9@:%_\+.~#?&//=]{2,256}\.[a-z]{2,4}\b(\/[-a-zA-Z0-9@:%_\+.~#?&//=]*)?.jpg");
                        imgURL = (rgx.Matches(queryStr).ToArray()[0]).ToString();

                        //string imgURL = (rgx.Matches(queryStr).ToArray()[0]).ToString() + "?width=384&height=273&quality=100";
                        //WebClient wc = new WebClient();
                        //wc.Proxy = null;
                        //Stream stream = wc.OpenRead(imgURL);
                        //Image img = Bitmap.FromStream(stream);
                        //// 이미지를 바이트 배열화
                        //using ( var memStream = new MemoryStream())
                        //{
                        //    img.Save(memStream, ImageFormat.Png);
                        //    res = memStream.ToArray();
                        //}
                    }
                    
                    prodList.Add(new Product { name = name, price = price, comment = comment, category = categoryHeading.Text, store = storeType, image = imgURL });
                }

                using (var db = new DatabaseBroker())
                {
                    foreach (var prod in prodList)
                    {
                        db.Add(prod);
                    }
                    db.SaveChanges();
                }

                categoryHeading.Click();
            }
        }

        public void GetAttr(string str)
        {
            Console.WriteLine(elemsCache.ToArray()[0].GetAttribute(str));
        }

        public void GetCss(string str)
        {
            string queryStr = elemsCache.ToArray()[0].FindElement(By.XPath($@"descendant::*[@class=""photo""]")).GetCssValue(str);

            Console.WriteLine(queryStr);
            Regex rgx = new Regex(@"[-a-zA-Z0-9@:%_\+.~#?&//=]{2,256}\.[a-z]{2,4}\b(\/[-a-zA-Z0-9@:%_\+.~#?&//=]*)?.jpg");
            string imgURL = (rgx.Matches(queryStr).ToArray()[0]).ToString() + "?width=384&height=273&quality=100";
            Console.WriteLine(imgURL);
        }

    }

    // 편의점 종류
    public enum StoreType
    {
        None = 0,
        GS,
        CU,
        Ministop,
        Seven11,
        Emart24,
        HomeplusExpress,
        Other
    }

}
