using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Text.RegularExpressions;
//using SautinSoft.PdfFocus;

namespace firstTry
{
    public class ExtractMetadata
    {
        public int pageCount = 0;
        public Dictionary<string,string> ExtractAllMetadata(string fileName)
        {
            var dict = new Dictionary<string, string>();

            SautinSoft.PdfFocus f = new SautinSoft.PdfFocus();
            f.OpenPdf(fileName);
            pageCount = f.PageCount;
            string[] pages = new string[f.PageCount];
            // MessageBox.Show(pages.Length.ToString());

            for (int i = 0; i < f.PageCount; i++)
            {
               
                pages[i] = f.ToText(i+1, 1);
              //  MessageBox.Show(pages[i]);

            }

            var blocks = MakeBlocks(pages);

            dict["File"] = fileName;
            dict["Format"] = ExtractFormat(fileName);

            Dictionary<string, string> firstPageMetadataDict = ExtractFirstPageMetadata(pages);
            dict["Name"] = firstPageMetadataDict["Name"];
            dict["Jornal"] = firstPageMetadataDict["Jornal"];
            dict["Author"] = firstPageMetadataDict["Author"];
            dict["Year"] = firstPageMetadataDict["Year"];

            dict["Annotation"] = ExtractAnnotation(blocks);
            dict["KeyWords"] = ExtractKeyWords(blocks);
            dict["References"] = ExtractReferences(blocks);
         //   dict["Maths"] = ExtractMaths(fileName);

            MakeXml(dict);

            return dict;
        }

        public int FindFirstPagesCount(string[] pages)
        {
            int res = 0;
          

            return res;
        }

        public Dictionary<string,string> ExtractFirstPageMetadata(string[] pages)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict["Name"] = "";
            dict["Author"] = "";
            dict["Year"] = "";
            dict["Jornal"] = "";
            //MessageBox.Show(pages.Length);

            if (pages[0].IndexOf("Math-Net.Ru", StringComparison.CurrentCultureIgnoreCase) != -1)
            {
                var parts = pages[0].Split(Environment.NewLine);

                var myParts = new List<string>();
                var str = "";
                for (int j = 0; j < parts.Length - 1; j++)
                {
                    if (parts[j] != "")
                    {
                        str = str + parts[j].Replace("Created by unlicensed version of PDF Focus .Net 7.8.1.29!", "").
                   Replace("The unlicensed version inserts \"trial\" into random places.", "").
                   Replace("This text will disappear after purchasing the license.", "");
                    }
                    else
                    {
                        myParts.Add(str);
                        str = "";
                    }

                }
                var pageBlocks = myParts.Where(x => x != "").ToArray();
                var metaString = pageBlocks[1];
             //   MessageBox.Show(metaString);

                var metaArray = metaString.Split(",");

                string pattern = @"^\s*(\w\.\s?){1,2}\w{2,}\s*$";
//^\s*\w{2,}\s(\w\.\s?){1,2}\s*$";

                var name = "";
                for (int i = 0; i < metaArray.Length; i++)
                {
                  
                    if (Regex.IsMatch(metaArray[i], pattern))
                    {
                        name = name + metaArray[i] + ", ";
                      //  MessageBox.Show(name);
                    }
                    else
                    {
                        dict["Author"] = name;
                            //.Substring(0, name.Length - 2);
                        dict["Name"] = metaArray[i];
                        dict["Jornal"] = metaArray[i + 1];
                        dict["Year"] = metaArray[i + 2];
                        break;
                    }
                }
            }
            else
            {
                var bl = MakeBlocks(new [] { pages[0] });

                string pattern = @"(\p{Lu}\.\s?){1,2}\p{Lu}[^\s\d,]{2,}";

                for (int i=0; i<bl[0].Length; i++)
                {
                    if (Regex.IsMatch(bl[0][i], pattern))
                    {
                        dict["Author"] = String.Join(", ", Regex.Matches(bl[0][i], pattern).Cast<Match>().Select(m => m.Value)); 
                        if (i>0)
                        {
                            if(bl[0][i].IndexOf(Regex.Match(bl[0][i], pattern).ToString(), 0) > 15)
                            {
                                dict["Name"] = bl[0][i].Substring(0, bl[0][i].IndexOf(Regex.Match(bl[0][i], pattern).ToString(), 0));
                                
                            }
                            else
                            {
                                dict["Name"] = bl[0][i - 1];
                                if (char.IsLower(dict["Name"][0]))
                                {
                                    dict["Name"] = bl[0][i - 2] + " " + bl[0][i - 1];
                                }
                            }
                            
                        }
                        else
                        {
                            continue;
                           
                        }
                        break;
                    }
                }
              
                
                if (Regex.IsMatch(bl[0][bl[0].Length-1], @"[12]\d\d\d"))
                {
                    dict["Jornal"] = bl[0][bl[0].Length-1];
                    dict["Year"] = Regex.Match(bl[0][bl[0].Length - 1], @"[12]\d\d\d").Value;
                }
                if (Regex.IsMatch(bl[0][0], @"[12]\d\d\d"))
                {
                    dict["Jornal"] = bl[0][0];
                    dict["Year"] = Regex.Match(bl[0][0], @"[12]\d\d\d").Value;
                }

            }
            

            return dict;
        }

        public string[][] MakeBlocks(string[] pages)
        {
            string[][] blocks = new string[pages.Length][];
            for (int i = 0; i < pages.Length; i++)
            {
                var parts = pages[i].Split(Environment.NewLine);
              

         //       parts = parts.ToList().Take(parts.Length - 3).ToArray();
                var myParts = new List<string>();
                var str = "";
                for (int j = 0; j < parts.Length - 1; j++)
                {
                    str = parts[j].Replace("Created by unlicensed version of PDF Focus .Net 7.8.1.29!", "").
                    Replace("The unlicensed version inserts \"trial\" into random places.", "").
                    Replace("This text will disappear after purchasing the license.", "");
                   
                    myParts.Add(str);
                }
                var pageBlocks = myParts.Where(x => x != "").ToArray();
                
                blocks[i] = pageBlocks;
            }

       
            return blocks;
        }
        public string ExtractFormat(string fileName)
        {
            var res = fileName.Split('.').Last();
            return res;
        }

        public string ExtractName(string[] pages)
        {
            var res = "";

            


            return res;
        }

        public string ExtractJornal(string[] pages)
        {
            var res = "";

            return res;
        }
        public string ExtractAuthor(string[] pages)
        {
            var res = "";

            return res;
        }

        public string ExtractYear(string[] pages)
        {
            var res = "";

            return res;
        }

        public string ExtractAnnotation(string[][] blocks)
        {
            var res = "";

            int maxPage = Math.Min(pageCount, 3);
           for (int i=0; i<maxPage; i++)
            {
                for (int j=0; j<blocks[i].Length; j++) 
                {
                   // MessageBox.Show(blocks[i][j]);
                    if (blocks[i][j].IndexOf("Аннотация", StringComparison.CurrentCultureIgnoreCase) != -1 || blocks[i][j].IndexOf("Annotation", StringComparison.CurrentCultureIgnoreCase) != -1 || blocks[i][j].IndexOf("Abstract", StringComparison.CurrentCultureIgnoreCase) != -1)
                    {
                        if (blocks[i][j].Length <= 15) res = blocks[i][j + 1];
                        else res = blocks[i][j];
                     //   MessageBox.Show(res);
                        break;
                    }
                }
                if (res != "") break;
            }

            return res;
        }

        public string ExtractKeyWords(string[][] blocks)
        {
            var res = "";
            int maxPage = Math.Min(pageCount, 3);

            for (int i = 0; i < maxPage; i++)
            {
                for (int j = 0; j < blocks[i].Length; j++)
                {
                    if (blocks[i][j].IndexOf("Ключевые", StringComparison.CurrentCultureIgnoreCase) != -1 || blocks[i][j].IndexOf("Keywords", StringComparison.CurrentCultureIgnoreCase) != -1 || blocks[i][j].IndexOf("Key words", StringComparison.CurrentCultureIgnoreCase) != -1)
                    {
                        if (blocks[i][j].Length <= 17) res = blocks[i][j + 1].Split(":").Last();
                        else res = blocks[i][j].Split(":").Last();
                     
                        break;
                    }
                }
                if (res != "") break;
            }
            return res;
        }

        public string ExtractReferences(string[][] blocks)
        {
            var res = "";

            for (int i = 0; i < pageCount; i++)
            {
                for (int j = 0; j < blocks[i].Length; j++)
                {
                    // MessageBox.Show(blocks[i][j]);
                    if (blocks[i][j].Contains("Литератур") || blocks[i][j].Contains("Список использованной литературы") || blocks[i][j].Contains("References"))
                    {
                        if (blocks[i][j].Length <= 40) res = blocks[i][j + 1];
                        else res = blocks[i][j];
                        //  MessageBox.Show(res);
                        int k = 2;
                        while (k>=0)
                        {
                            if (j + k < blocks[i].Length)
                            {
                                if (blocks[i][j + k].Contains("References")) break;
                                if (blocks[i][j + k][0] == '1' && Char.GetUnicodeCategory(blocks[i][j + k][1]).ToString() != "DecimalDigitNumber") break;
                                if (Char.GetUnicodeCategory(res[0]) == Char.GetUnicodeCategory(blocks[i][j + k][0]))
                                {
                                    res = res + Environment.NewLine + blocks[i][j + k];
                                }
                                k++;
                            }
                            else
                            {
                                if (i < pageCount - 1)
                                {
                                    i++;
                                    j = 0;
                                    k = 0;
                                }
                                else break;
                            }  
                        }
                        break;
                    }
                }
                if (res != "") break;
            }

            return res;
        }

        public void MakeXml(Dictionary<string,string> dict)
        {
           
            XmlDocument myXmlDocument = new XmlDocument();
            myXmlDocument.Load("wwwroot/metadata_test.xml"); 
            XmlNode node;
            node = myXmlDocument.DocumentElement;
            foreach (XmlNode node1 in node.ChildNodes) 
            {
                node1.InnerText = dict[node1.Name]; 
            }
            myXmlDocument.Save("wwwroot/metadata_txt.txt");
            myXmlDocument.Save("wwwroot/metadata_test.xml");
        }

        public string ExtractMaths(string f)
        {
            var res = "";

            return res;
        }
    }
}
