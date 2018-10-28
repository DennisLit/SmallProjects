using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudyTime.Core
{
    public static class StringExtensions
    {
        public static JObject ToJObject(this string value)
        {
            try { return JObject.Parse(value); } catch (JsonReaderException) { return null; }
        }
    }
}
