using HtmlAgilityPack;
using System.Linq;
using System.Text;

namespace Rodkulman.Telegram
{
    public static class DicionarioInformal
    {
        public static string GetDefinition(string term)
        {
            var web = new HtmlWeb() { OverrideEncoding = Encoding.GetEncoding("iso-8859-1") };
            var html = web.Load($"https://www.dicionarioinformal.com.br/{term}");

            var definitions = html.DocumentNode.Descendants("div").Where(x => x.Attributes["itemprop"]?.Value == "description");

            if (definitions.Any())
            {
                var definition = definitions.GetRandomElement().Element("p").InnerText.Trim();

                return $"<a href=\"https://www.dicionarioinformal.com.br/{term}\">{definition}</a>";
            }
            else
            {
                return $"{term} não encontrado, provavelmente porque tu inventou essa palavra, {term}, agora pra me ludibriar";
            }
        }
    }
}