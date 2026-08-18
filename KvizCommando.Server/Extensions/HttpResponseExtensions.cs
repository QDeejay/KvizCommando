using KvizCommando.Shared.Constants;
using KvizCommando.Shared.Models.Enums;
using System.Net;

namespace KvizCommando.Server.Extensions
{
    public static class HttpResponseExtensions
    {
        public static void AddToast(this HttpResponse response, string text, ToastType type)
        {
            response.Headers[HttpHeaderNames.TOAST_TEXT] = WebUtility.UrlEncode(text); ;
            response.Headers[HttpHeaderNames.TOAST_TYPE] = type.ToString();
        }
    }
}
