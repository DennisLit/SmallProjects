using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace StudyTime.Core
{
    public static class RequestsHelper
    {
        public static string GroupsScheduleByGroupName(string groupName)
        {
            var httpRequest = WebRequest.CreateHttp(ApiConstants.GroupsScheduleByGroupNameCall + groupName);
            httpRequest.ContentType = ApiConstants.DefaultContentType;
            httpRequest.Method = ApiConstants.HttpMethod;

            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();

            var result = string.Empty;

            using (var sr = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = sr.ReadToEnd();
            }

            return result;
        }
    }
    
}
