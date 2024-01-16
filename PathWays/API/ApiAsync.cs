using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using PathWays.Const;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathWaysVMS.API
{
    public static class ApiAsync
    {
        public static async Task<Tuple<string, bool>> CallApi(string apiUrl, HttpMethod method, string? data = null)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpRequestMessage request = new HttpRequestMessage(method, ConstantVariables.APIURL + apiUrl);
                    if (data != null)
                    {
                        if (method == HttpMethod.Post || method == HttpMethod.Put)
                        {
                            request.Content = new StringContent(data, Encoding.UTF8, "application/json");
                        }
                    }
                    HttpResponseMessage response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        var responsecon = await response.Content.ReadAsStringAsync();
                        return new Tuple<string, bool>(responsecon, response.IsSuccessStatusCode);
                    }
                    else
                    {
                        return new Tuple<string, bool>($"Error: {response.StatusCode} - {response.ReasonPhrase}", response.IsSuccessStatusCode);
                    }
                }
            }
            catch (Exception ex)
            {
                return new Tuple<string, bool>($"Exception: {ex.Message}", false);
            }
        }

    }
}
