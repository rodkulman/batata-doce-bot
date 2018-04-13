using HtmlAgilityPack;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodkulman.Telegram
{
    public static class DicionarioInformal
    {
        public static string GetDefinition(string token)
        {
            var web = new HtmlWeb() { OverrideEncoding = Encoding.GetEncoding("iso-8859-1") };
            var html = web.Load($"https://www.dicionarioinformal.com.br/{token}");

            var definitions = html.DocumentNode.Descendants("div").Where(x => x.Attributes["itemprop"]?.Value == "description");

            if (definitions.Any())
            {
                var definition = definitions.GetRandomElement().Element("p").InnerText.Trim();

                return $"<a href=\"https://www.dicionarioinformal.com.br/{token}\">{definition}</a>";
            }
            else
            {
                return "Nem eu sei 😂";
            }
        }
    }
}