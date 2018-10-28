using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudyTime.Core
{
    /// <summary>
    /// Parser for bsuir API 
    /// </summary>
    public class BsuirParser
    {
        private static byte WeeksTotal => 4;

        /// <summary>
        /// Main parse method
        /// </summary>
        /// <param name="value">Json object to parse</param>
        /// <param name="weekNumber">Subjects of this week will be shown if IsWholeMonth is false</param>
        /// <param name="IsWholeMonth">Name speaks for itself</param>
        /// <returns></returns>
        public Dictionary<string,int> Parse(JObject value, byte weekNumber, bool IsWholeMonth)
        {
            if(weekNumber > WeeksTotal)
            {
                return null;
            }

            var returnList = new Dictionary<string, int>();

            var timeMultiplier = 1;

            var tempSubjectName = string.Empty;

            int tempLessonTime = 0;
             
            foreach (JProperty prop in value.Children<JProperty>())
            {
                if (prop.Name == "schedules")
                {
                    foreach (JArray schedulesArr in prop.Children<JArray>())
                    {
                        //object inside of "schedules" tag

                        foreach(JObject daySchedule in schedulesArr.Children<JObject>())
                        {
                            foreach (JProperty anotherProp in daySchedule.Children<JProperty>())
                            {
                                if (anotherProp.Name == "schedule")
                                {

                                    foreach (JArray scheduleArr in anotherProp.Children<JArray>())
                                    {
                                        //object inside of "schedule" tag

                                        foreach (JObject mainInfo in scheduleArr.Children<JObject>())
                                        {
                                            foreach (JProperty mainProperty in mainInfo.Children<JProperty>())
                                            {
                                                //Get all week numbers from weeknumber array
                                                //Needed to calculate hours for the whole month

                                                if ((mainProperty.Name == "weekNumber") && (IsWholeMonth))
                                                {
                                                    foreach (JArray weekNumbersArr in mainProperty.Children<JArray>())
                                                    {
                                                        foreach (JValue week in weekNumbersArr.Children<JValue>())
                                                        {
                                                            if ((int)week == 0)
                                                            {
                                                                timeMultiplier = 4;
                                                                break;
                                                            }
                                                            else
                                                            {
                                                                timeMultiplier += 1;
                                                            }
                                                        }
                                                    }
                                                }

                                                //Add all week numbers to a list

                                                var weeksArray = new List<byte>();

                                                if (mainProperty.Name == "weekNumber")
                                                {
                                                    foreach (JArray weekNumbersArr in mainProperty.Children<JArray>())
                                                    {
                                                        foreach (JValue week in weekNumbersArr.Children<JValue>())
                                                        {
                                                            weeksArray.Add((byte)week);
                                                        }
                                                    }

                                                    //we need info only about the week user provided

                                                    if ((!weeksArray.Contains(weekNumber)) && (!IsWholeMonth))
                                                    {
                                                        break;
                                                    }
                                                }

                                                var tempToken = mainProperty.Value;
                                                
                                                if (mainProperty.Name == "startLessonTime")
                                                {

                                                    //write minus value
                                                    //cuz then we can simply add the value of endlesson
                                                    tempLessonTime = -(TimeSpan.Parse(tempToken.Value<String>()).Hours * 60 
                                                        + TimeSpan.Parse(tempToken.Value<String>()).Minutes);
                                                }
                                                else if(mainProperty.Name == "endLessonTime")
                                                {
                                                    tempLessonTime += TimeSpan.Parse(tempToken.Value<String>()).Hours * 60 
                                                        + TimeSpan.Parse(tempToken.Value<String>()).Minutes;

                                                }

                                                if (mainProperty.Name == "subject")
                                                {
                                                    tempSubjectName = tempToken.Value<string>();

                                                    if (returnList.ContainsKey(tempSubjectName))
                                                    {
                                                        returnList[tempSubjectName] += tempLessonTime * timeMultiplier;
                                                    }
                                                    else
                                                    {
                                                        returnList.Add(tempSubjectName, tempLessonTime * timeMultiplier);
                                                    }
                                                }

                                            }
                                        }
                                    }
                                }
                            }
                        }

                    }
                    

                    break;
                }
            }


            return returnList;
            
        }

    }
}
