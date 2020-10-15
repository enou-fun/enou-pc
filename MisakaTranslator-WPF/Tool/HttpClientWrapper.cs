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

namespace MisakaTranslator_WPF
{
    class HttpClientWrapper
    {
        public static long SendWordToEnouServerGetId(string word)
        {
            String jsonWord = "{\"word\":\"" + word + "\"}";
            Console.WriteLine(jsonWord);
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonWord);

            String token = Common.appSettings.EnouAccountToken;
            Console.WriteLine(" SendWordToEnouServerAsync token is " + token);

            String api = Common.appSettings.EnouServerWordApi;
            WebRequest request = WebRequest.Create(api);
            request.Method = "POST";
            request.Headers.Add("token", token);
            request.ContentType = "application/json";
            request.ContentLength = byteArray.Length;

            using (Stream st = request.GetRequestStream())
                st.Write(byteArray, 0, byteArray.Length);

            WebResponse webResponse = null;
            long retWordId = 0;
            try
            {
                webResponse = request.GetResponse();
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    var response = ex.Response as HttpWebResponse;
                    if (response != null && response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        if (Common.appSettings.EnouAccountToken == null)
                        {
                            // todo open the login window
                        }
                        else
                        {
                            return SendWordToEnouServerGetId(word);
                        }
                    }
                }
            }
            finally
            {
                if (webResponse != null)
                {
                    Stream myResponseStream = webResponse.GetResponseStream();
                    StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);
                    string retString = myStreamReader.ReadToEnd();
                    webResponse.Close();

                    JObject jObject = JObject.Parse(retString);
                    File.WriteAllText("log.txt", "jObject is " + jObject.ToString());
                    retWordId = long.Parse(jObject["id"].ToString());
                }
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
                    Common.appSettings.EnouAccountToken = jsonString;
                }
            }
            catch
            {
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
