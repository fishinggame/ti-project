using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koncowka
{
    public class Picture
    {
        private string[] arrayOfSrcs;
        public Picture(string json)
        {
            JObject jObject = JObject.Parse(json);
            object[] temp = (object [])(jObject["images"].ToArray());
            arrayOfSrcs = Array.ConvertAll<object, string>(temp, Convert.ToString);
        }
        public string[] ArrayOfSrcs
        {
            get
            {
                return arrayOfSrcs;
            }
        }
    }
}
