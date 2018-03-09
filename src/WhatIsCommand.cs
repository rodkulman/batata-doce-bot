using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Rodkulman.Telegram
{
    public static class WhatIsCommand
    {
        public static IEnumerable<string> ReplyMessage(string message)
        {
            var tokens = message.Split(' ');            

            if (tokens.Length == 1)
            {
                yield return "Pergunta alguma coisa né fio";
                yield break;
            }

            if (tokens.Length == 2)
            {
                switch (tokens[1].ToLower())
                {
                    case "love":
                        yield return "baby don't hurt me";
                        yield return "don't hurt me";
                        yield return "no more";
                        yield break;
                    case "life":
                        yield return "42";
                        yield break;
                    case "man":
                        yield return "a miserable pile of secrets";
                        yield break;
                    case "it":
                    case "ti":
                        yield return File.ReadAllLines(@"text-replies\whatis-it.txt").GetRandomElement();
                        yield break;
                    default:
                        yield return "te acalma fdp falta mais ifs nesse código";
                        yield break;
                }
            }

            yield return "não vale perguntar mais que duas coisas.";
        }
    }
}