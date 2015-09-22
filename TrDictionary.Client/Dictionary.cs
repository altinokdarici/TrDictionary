using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace TrDictionary.Client
{
    public class Dictionary
    {
        public static List<Word> Words = new List<Word>();
        public async static Task Load()
        {
            if (Words.Count == 0)
            {
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/sozluk.json"));
                string json = await FileIO.ReadTextAsync(file);
                Words = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Word>>(json);
            }
        }
        public static List<Word> Search(string key, bool exactMatch)
        {
            List<Word> result = null;
            if (exactMatch)
                result = Words.Where(x => x.Key.ToLower() == key.ToLower()).ToList();
            else
                result = Words.Where(x => x.Key.ToLower().StartsWith(key.ToLower())).ToList();

            result.Reverse();
            return result;
        }
    }


    public class Word
    {
        public string Prefix { get; set; }
        public string Key { get; set; }
        public string Meaning { get; set; }
        public override string ToString()
        {
            return string.Format("{0}\n\t{1}-{2}", Key, Prefix, Meaning);
        }
    }

}
