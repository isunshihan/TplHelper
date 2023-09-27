using AngleSharp.Dom;
using AngleSharp.Html;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TplHelper
{
    public class Helper
    {
        IConfigurationRoot configuration;
        HtmlParser parser = new HtmlParser();

        public Helper()
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(Path.Combine(AppContext.BaseDirectory))
               .AddJsonFile("Config/config.json", optional: true, reloadOnChange: true);
            configuration = builder.Build();
        }

        public void Go()
        {
            var paths = configuration.GetSection("tplroot").GetChildren();
            IEnumerable<string> files;
            var ext = configuration.GetSection("ext").Value;
            IHtmlDocument doc;
            var tags = configuration.GetSection("tags").GetChildren().ToList();
            var props = configuration.GetSection("props").GetChildren().ToList();
            List<string> lines;
            IElement el;
            StringBuilder sb;
            string str;
            string oldStr;
            string newStr;
            Regex ex = new Regex("class=\"[\\s\\S]+?\"");
            foreach (var path in paths)
            {
                Console.WriteLine("读取模板目录----" + path);
                files = CommonHelper.GetFiles(path.Value);
                foreach (var file in files) //读取模板文件
                {
                    Console.WriteLine("读取文件" + file);
                    doc = parser.ParseDocument(file);
                    sb = new StringBuilder(); //每个文件重新构造字符串
                    if (file.EndsWith(ext))
                    {
                        lines = File.ReadAllLines(file).ToList();
                        foreach (var line in lines)
                        {
                            str = line.Trim();
                            if (ex.IsMatch(line))
                            {
                                oldStr = ex.Match(line).Value.Replace("class=\"", string.Empty);
                                newStr = CommonHelper.GetRandomString(10) + " " + oldStr;
                                str = str.Replace(oldStr, newStr);
                                sb.AppendLine(str);
                            }
                            else
                            {  
                                sb.AppendLine(line);
                            }
                            
                            if (str.StartsWith("<div") && str.EndsWith(">")
                                || str.StartsWith("<ul") && str.EndsWith(">")
                                || str.StartsWith("<p") && str.EndsWith(">")) //在后面随机插入元素
                            {
                                tags.Shuffle();
                                props.Shuffle();
                                el = doc.CreateElement(tags[0].Value);
                                el.SetAttribute(props[0].Value,
                                    CommonHelper.GetRandomString(10));
                                sb.AppendLine(el.OuterHtml);
                            }
                        }


                        //uls = doc.QuerySelectorAll("ul").ToList();
                        //if (uls != null)
                        //{
                        //    Console.WriteLine("在所有ul标签中增加随机标签");
                        //    foreach (var ul in uls)
                        //    {
                        //        srchtml = ul.OuterHtml;
                        //        ul.ClassName = ul.ClassName + " " + CommonHelper.GetRandomString(10);
                        //        for (int i = 0; i < 5; i++) //创建10个随机标签
                        //        {
                        //            tags.Shuffle();
                        //            el = doc.CreateElement(tags[0].Value);
                        //            el.SetAttribute(props[0].Value,
                        //                CommonHelper.GetRandomString(10));
                        //            ul.AppendChild(el);
                        //        }
                        //    }

                        //}

                        File.WriteAllText(file, sb.ToString());
                        Console.WriteLine("写入文件");
                    }
                    else
                    {
                        Console.WriteLine("该文件非模板文件，跳过");
                    }
                }
            }

            Console.WriteLine("修改模板完成");
        }
    }
}
