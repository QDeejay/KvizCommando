using KvizCommando.Shared.Constants;
using KvizCommando.Shared.Models.Enums;
using System.Net;

namespace KvizCommando.Server.Extensions
{
    public static class HttpResponseExtensions
    {
        /// <summary>
        /// Új értesítést helyez a megjelenítési sorba.
        /// </summary>
        /// <param name="response">A kiegészítendő HTTP-válasz.</param>
        /// <param name="text">A megjelenítendő vagy elküldendő szöveg.</param>
        /// <param name="type">Az üzenet vagy megjelenítés típusa.</param>
        public static void AddToast(this HttpResponse response, string text, ToastType type)
        {
            response.Headers[HttpHeaderNames.TOAST_TEXT] = WebUtility.UrlEncode(text); ;
            response.Headers[HttpHeaderNames.TOAST_TYPE] = type.ToString();
        }
    }
}
