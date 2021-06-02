using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Enou
{
    class HttpClientWrapper
    {
        public static long SaveWordToEnouServerGetId(string word)
        {
            String jsonWord = "{\"word\":\"" + word + "\"}";
            String token = Common.appSettings.EnouAccountToken;

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(Common.appSettings.EnouServerWordApi),
                Method = HttpMethod.Post,
                Content = new StringContent(jsonWord, Encoding.UTF8, "application/json"),
            };
            request.Headers.Add("token", Common.appSettings.EnouAccountToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            long retWordId = 0;
            try
            {
                HttpResponseMessage response = client.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonString = response.Content.ReadAsStringAsync().Result;

                    JObject jObject = JObject.Parse(jsonString);

                    String wordStr = jObject["data"].ToString();
                    JObject wordJObject = JObject.Parse(word);

                    retWordId = long.Parse(jObject["id"].ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }


            return retWordId;
        }

        public static bool LoginByPwd(String account, String password)
        {

            String loginInfo = "{\"account\":\"" + account + "\", \"password\":\"" + password + "\"}";
            Console.WriteLine(loginInfo);


            HttpClient client = new HttpClient();
            Uri uri = new Uri(Common.appSettings.EnouServerLoginApi);
            client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));

            StringContent content = new StringContent(loginInfo, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = client.PostAsync(uri, content).Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonString = response.Content.ReadAsStringAsync().Result;
                    Console.WriteLine("token is " + jsonString);

                    JObject jObject = JObject.Parse(jsonString);
                   
                    Common.appSettings.EnouAccountToken = jObject["data"].ToString();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }


        public static bool LoginByToken()
        {
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(Common.appSettings.EnouServerTokenLoginCheckApi),
                Method = HttpMethod.Get,
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("token", Common.appSettings.EnouAccountToken);
            try
            {
                HttpResponseMessage response = client.SendAsync(request).Result;
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public static bool GetKnownWords()
        {
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(Common.appSettings.EnouServerGetKnownWordApi),
                Method = HttpMethod.Get,
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("token", Common.appSettings.EnouAccountToken);
            try
            {
                HttpResponseMessage response = client.SendAsync(request).Result;
                if(response.IsSuccessStatusCode)
                {
                    string jsonString = response.Content.ReadAsStringAsync().Result;

                    JObject jObject = JObject.Parse(jsonString);

                    List<String> wordList = jObject["data"].ToObject<List<String>>();
                    Common.SetKnownWords(wordList);
                    return true;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            return false;
        }

        public static bool LearnWord(String word)
        {
            HttpClient client = new HttpClient();

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(Common.appSettings.EnouServerLearnWordApi),
                Method = HttpMethod.Post,
                Content = new FormUrlEncodedContent(new Dictionary<string, string>

                {
                    {"spell", word }
                }),
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("token", Common.appSettings.EnouAccountToken);

            try
            {
                HttpResponseMessage response = client.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }



        public static void ModifyWord(long wordId, String word)
        {
            String jsonWord = "{ \"id\":" + wordId + " , \"word\":\"" + word + "\"}";
            HttpClient client = new HttpClient();

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(Common.appSettings.EnouServerWordApi),
                Method = HttpMethod.Put,
                Content = new StringContent(jsonWord, Encoding.UTF8, "application/json"),
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("token", Common.appSettings.EnouAccountToken);

            try
            {
                HttpResponseMessage response = client.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                }
            }
            catch
            {

            }
        }


    }
}
