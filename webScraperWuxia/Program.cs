using HtmlAgilityPack;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace webScraperWuxia
{
    internal class Program
    {
        static ConcurrentBag<string> bookNames = new ConcurrentBag<string>();
        static ConcurrentBag<string> chosenNames = new ConcurrentBag<string>();
        static List<string> words = new List<string>();
        static async Task Main(string[] args)
        {
            var stopwatch = Stopwatch.StartNew();
            RenewWords();

            var tasks = new List<Task>();

            Console.WriteLine("type popular for search in popular books");
            bool popularSearch = false;
            if(Console.ReadLine() == "popular")
            {
                popularSearch = true;
            }

            Console.WriteLine();
            Console.Write("From page: ");
            int fromPage = int.Parse(Console.ReadLine());
            Console.Write(", ToPage: ");
            int endPage = int.Parse(Console.ReadLine());

            for (int page = fromPage; page < endPage; page++)
            {
                tasks.Add(getPageLoadedName(page, popularSearch));
                await Task.Delay(200);
            }

            await Task.WhenAll(tasks);
            stopwatch.Stop();
            Console.WriteLine($"elapsed time {stopwatch.Elapsed.TotalSeconds}");
            Console.WriteLine();
            Console.WriteLine("Chosen books");
            foreach (string s in chosenNames) {
                Console.WriteLine(s);
            }

            SaveChosenBooks(fromPage, endPage, popularSearch);



        }

        private static void RenewWords()
        {
            using (var writer = new StreamReader("requiredwords.txt")) {
                string line = "";
                while(line != null){
                    line = writer.ReadLine();
                    if (line != null) {
                        line.ToLower();
                        words.Add(line);
                    }
                }
            }
        }

        static async Task getPageLoadedName(int page, bool popular = false)
        {
            try
            {
                string url = $"https://www.wuxiabox.com/list/all/all-newstime-{page}.html";
                if (popular)
                {
                    url = $"https://www.wuxiabox.com/list/all/all-onclick-{page}.html";
                }

                HttpClient client = new HttpClient();
                var html = await client.GetStringAsync(url);
                var htmlDocument = new HtmlDocument();

                htmlDocument.LoadHtml(html);

                var nameElements = htmlDocument.DocumentNode.SelectNodes("//h4[@class='novel-title text2row']");
                foreach (var s in nameElements)
                {
                    string bookname = s.InnerText.Trim();
                    bookNames.Add(bookname);
                    Console.WriteLine(bookname);
                    foreach(string word in words)
                    {
                        if (bookname.Contains(word))
                        {
                            chosenNames.Add(bookname);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"error on page {page}: {ex.Message}");
            }


            Console.Write(".");

        }

        static void SaveChosenBooks(int fromPage, int endPage, bool popular)
        {
            Console.WriteLine();
            Console.Write("Type the new txt document name: ");


            using(var writer = new StreamWriter($"{Console.ReadLine()}.txt"))
            {
                writer.WriteLine($"page {fromPage} to {endPage}. Date: {DateTime.Today}, Section popular: {popular}");
                foreach(string s in chosenNames)
                {
                    writer.WriteLine(s);
                }
            }
        }
    }
}
