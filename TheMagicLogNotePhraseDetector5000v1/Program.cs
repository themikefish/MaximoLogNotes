using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TheMagicLogNotePhraseDetector5000v1 // for organization purposes
{
    class Program 
    {
        private Dictionary<string, int> _nGramCounts { get; set; } = new Dictionary<string, int>();
        static void Main(string[] args) // main method
        {
            Console.WriteLine("Starting...");
            new Program();
            Console.WriteLine("Done.");
            Console.Read();
        }

        private Program()  // the constructor. like a special kind of method. instanciates a program object
        {
            //get string from config file
            var minWordsString = ConfigurationManager.AppSettings["minNGramWords"];
            var maxWordsString = ConfigurationManager.AppSettings["maxNGramWords"];
            //creating a correct datatype var, and trying to parse the string
            int minNGramWords;
            var minWordSuccess = int.TryParse(minWordsString, out minNGramWords);
            if (!minWordSuccess)
            {
                Console.WriteLine($"{minWordsString} is not an int");
                minNGramWords = 3; //default
            }

            int maxNGramWords;
            var maxWordSuccess = int.TryParse(maxWordsString, out maxNGramWords);
            if (!maxWordSuccess)
            {
                Console.WriteLine($"{maxWordsString} is not an int");
                maxNGramWords = 8; //default
            }
   
            var minOccurences = 20;

            var baseDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var inputDirectory = Path.Combine(baseDirectory, "WorkFiles");
            var outputDirectory = Path.Combine(inputDirectory, "Output");
            
            Directory.CreateDirectory(outputDirectory);
            //Directory.EnumerateFiles(outputDirectory).ToList().ForEach(f => File.Delete(f));


            var outputFileName = Path.Combine(outputDirectory, $@"{DateTime.Now:yyyyMMddHHmmss}_testOutput.csv");
            var pathToTextFile = Path.Combine(inputDirectory, @"DUMP.csv");

            if (!File.Exists(pathToTextFile))
            {
                Console.WriteLine($"Please place a data file to process at {pathToTextFile}");
                return;
            }
            var TextFromFile = File.ReadAllLines(pathToTextFile); // reads all lines into atring array

            Console.WriteLine("Processing Comments.");
            //processing
            foreach(var Comment in TextFromFile) // loops through each line
            {
                //Console.WriteLine(Comment);
                var newString = CleanUpHtml(Comment.ToLower());
                newString = CleanUpText(newString);
                
                for (int i = minNGramWords; i <=maxNGramWords; i++)
                {
                    //Console.WriteLine($"Processing {i} word N-grams...");
                    RowProcessor(i, newString);
                }
            }

            Console.WriteLine("Starting to write to file...");
            //dealing with results
            foreach (KeyValuePair<string, int> entry in _nGramCounts.OrderBy(o => o.Key))
            {
                if (entry.Value <= minOccurences) continue;
                File.AppendAllText(outputFileName, $"{entry.Key},{entry.Value}" + "\n");
            }
            Console.WriteLine("File writing complete.");

            Process.Start(outputFileName);

        }
            string CleanUpHtml(string stringToClean)
        {

            stringToClean = Regex.Replace(stringToClean, @"\s+", " "); // any whitespace character (equal to [\r\n\t\f\v ])
            stringToClean = stringToClean.Replace((char)0xA0, ' '); // non-breaking white space
            stringToClean = Regex.Replace(stringToClean, @"<[^>]*>", " "); // non-escaped html tags to space
            stringToClean = Regex.Replace(stringToClean, @"\&amp\;lt\;", "<"); // &< to <
            stringToClean = Regex.Replace(stringToClean, @"\&amp\;gt\;", ">"); // &> to >
            stringToClean = Regex.Replace(stringToClean, @"\<.*\/\>", " "); // stuff between tags to space    
            stringToClean = Regex.Replace(stringToClean, @"\[.*\/\]", " "); // stuff between brackets to space
            stringToClean = Regex.Replace(stringToClean, @"\{.*\/\}", " "); // stuff between curly braces to space
            stringToClean = Regex.Replace(stringToClean, @"\&[a-z]+\;?", " "); // any remaining escaped html
                                  
            File.AppendAllText(@"C:\Users\fishm\Desktop\stringToClean.txt", stringToClean + "\n"); // writes to file
            //Console.WriteLine(stringToClean);
            return stringToClean;
        }

        string CleanUpText(string stringToClean)
        {
            stringToClean = Regex.Replace(stringToClean, @"\d", " "); // periods not followed by number to space
            stringToClean = Regex.Replace(stringToClean, @"[ ]+", " "); // multiple spaces to single space

            var updates = new List<FindReplace>()
            {
                // new FindReplace("\t", " "),  // redundant me thinks
                new FindReplace("(", " "),
                new FindReplace(")", " "),

                new FindReplace(" district ", " sasd "),

                new FindReplace(" res ", " caller "),
                new FindReplace(" resident "," caller "),
                new FindReplace(" cp "," caller "),
                new FindReplace(" c p "," caller "),
                             
                new FindReplace(" low lat "," lowlat "),
                new FindReplace(" lower lat "," lowlat "),
                new FindReplace(" lower lateral ", " lowlat "),
                new FindReplace(" ll ", " lowlat "),
                new FindReplace(" low lateral ", " lowlat "),
                new FindReplace(" lat launch ", "lat_launch "),
                new FindReplace(" lateral launch ", "lat_launch "),

                new FindReplace(" see snake camera ", " camera "),
                new FindReplace(" see snake ", " camera "),

                new FindReplace("method ", " method_"),
                new FindReplace("methods ", " method_"),
                new FindReplace("method-", " method_"),
                new FindReplace(" and a ", " method_a "),
                new FindReplace(" and b ", " method_b "),
                new FindReplace(" and k ", " method_k "),
                new FindReplace(" b)", " method_b "),
                new FindReplace(" k)", " method_k "),
                new FindReplace(",b )", " method_b "),
                new FindReplace(",k )", " method_k "),

                new FindReplace(" week ", " week(s) "),
                new FindReplace(" weeks "," week(s) "),

                new FindReplace(" tvi ", " tv "),
                new FindReplace(" tved ", " tvd "),

                new FindReplace(" c/o ", " cleanout "),
                new FindReplace(" co ", " cleanout "),
                new FindReplace(" clean out ", " cleanout "),
                new FindReplace(" private cleanout "," private_cleanout "),
                new FindReplace(" sasd cleanout ", " sasd_cleanout "),
                new FindReplace(" coi ", " cleanout_install "),
                new FindReplace(" cor ", " cleanout_replace "),


                new FindReplace(" ml ", " mainline "),
                new FindReplace(" m/l ", " mainline "),
                new FindReplace(" main line ", " mainline "),

                new FindReplace(" di ", " drainage_inlet "),
                new FindReplace(" drainage inlet ", " drainage_inlet "),

                new FindReplace(" inch ", " inch(es) "),
                new FindReplace(" inchs ", " inch(es) "),
                new FindReplace(" inches ", " inch(es) "),

                new FindReplace( " finding ", " finding(s) "),
                new FindReplace(" findings ", " finding(s) "),

                new FindReplace(" dig up", " dig_up "),
                new FindReplace(" dug up ", " dug_up "),
       
   
                new FindReplace(" usage "," use "),

                new FindReplace(" functioning ", " working "),
                new FindReplace(" working properly ", " working "),

                new FindReplace(" carson box ", " carson_box "),
                new FindReplace(" valve box ", " valve_box "),

                new FindReplace(" non recoverable ", " nonrecoverable "),

                new FindReplace(" upon my arrival ", " on arrival "),

                new FindReplace(" did not ", " didnt "),
                new FindReplace(" can not ", "cannot"),

                new FindReplace(" not holding ", " not_holding "),
                new FindReplace(" not ongoing ", " not_ongoing "),

                new FindReplace(" m h ", " manhole "),
                new FindReplace(" mh ", " manhole "),
                new FindReplace(" m/h ", " manhole "),
                new FindReplace(" m-h ", " manhole "),
                new FindReplace(" man hole ", " manhole "),

                new FindReplace(" pic ", " photo(s) "),
                new FindReplace(" pics ", " photo(s) "),
                new FindReplace(" photo ", " photo(s) "),
                new FindReplace(" photos ", " photo(s) "),
                new FindReplace(" picture ", " photo(s) "),
                new FindReplace(" pictures ", " photo(s) "),
                new FindReplace(" pix ", " photo(s) "),

                new FindReplace(" work order ", " workorder(s) "),
                new FindReplace(" work orders ", " workorder(s) "),
                new FindReplace(" wos ", " workorder(s) "),
                new FindReplace(" wo ", " workorder(s) "),

                new FindReplace(" sr ", " servicerequest(s) "),
                new FindReplace(" srs ", "servicerequest(s) "),
                new FindReplace(" s/r ", " servicerequest(s) "),
                new FindReplace(" service request ", " servicerequest(s) "),
                new FindReplace(" service requests ", " servicerequest(s) "),

                new FindReplace(" asset ", " asset(s) "),
                new FindReplace(" assets ", " asset(s) "),

                new FindReplace( " were not ", " were_not "),
                new FindReplace(" w ", " "),
                new FindReplace(" x ", " "),
                new FindReplace(" a ", " "),
                new FindReplace(" an ", " "),
                new FindReplace(" and ", " "),

                new FindReplace(" gal ", " gallon(s) "),
                new FindReplace(" gallon ", " gallon(s) "),
                new FindReplace(" gallons ", " gallon(s) "),
                new FindReplace(" the ", " "),
                new FindReplace(" private plumber "," plumber "),
                new FindReplace(" ft ", " "),
                new FindReplace(" to contact "," to call "),
                new FindReplace(" stand by ", " standby "),
                new FindReplace(" standby crew ", " crew "),
                new FindReplace(" sasd crew "," crew "),

                new FindReplace("FALSE", " "),
                new FindReplace("TRUE", " "),
                new FindReplace("gt;", " "),
                new FindReplace("lt;", " "),
                new FindReplace("unhidewhenused", " "),
                new FindReplace("qformat=", " "),
                new FindReplace("quot;true", " "),
                new FindReplace("quot;false", " "),
                new FindReplace("quot;heading", " "),
                new FindReplace("quot;name", " "),
                new FindReplace("quot;", " "),
                new FindReplace("amp;", " "),
                new FindReplace("amp; w", " "),
                new FindReplace("amp; name", " "),
                new FindReplace("w lsdexception locked", " "),
                new FindReplace("w lsdexception", " "),
                new FindReplace("<! [if ]", " "),
                new FindReplace(" [if ", " "),
                new FindReplace(" gte ", " "),
                new FindReplace(" mso ", " "),
                new FindReplace(" < ! ", " "),
                new FindReplace(" defpriority ", " "),
                new FindReplace(" def false ", " "),
                new FindReplace(" latentstylecount ", " "),

                new FindReplace("/", " "),
                new FindReplace(":", " "),
                new FindReplace("'", ""),
                new FindReplace("=", " "),
                new FindReplace("*", " "),
                new FindReplace("#", " "),
                new FindReplace("@", " "),
                new FindReplace(",", " "),
                new FindReplace(".", " "),
                new FindReplace("-", " ")                
            };
            
            foreach(var update in updates)
            {
                stringToClean = stringToClean.Replace(update.Find, update.Replace);
            }

            return stringToClean;
        }

        private void RowProcessor(int wordsInNGram, string toProcess)
        {
            // braek down into an array
            var wordarray = toProcess.Split(new []{ ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (wordarray.Length < wordsInNGram) //if there aren't enough words, bail
                return;

            // operate on the array
            for (int wordIdx = 0; wordIdx <= wordarray.Length-wordsInNGram; wordIdx++)
            {
                var nGram = string.Empty;
                for(int nGramIdx = wordIdx; nGramIdx < wordIdx+wordsInNGram; nGramIdx++)
                {
                    nGram += wordarray[nGramIdx] + (nGramIdx != wordIdx+wordsInNGram-1 ? " " : string.Empty);
                    // create phrase (n-gram)
                }

                //Console.WriteLine(nGram);
                // check if it's in dictionary, if so add to count
                int value;
                _nGramCounts.TryGetValue(nGram, out value);
                _nGramCounts[nGram] = value+1;
            }



        }
    }
}
