using KvizCommando.Client.Services.Visual.UiService;
using KvizCommando.Shared.Constants;
using KvizCommando.Shared.Models.Enums;
using System.Net;

namespace KvizCommando.Client.Http
{
    public sealed class ToastHandler : DelegatingHandler
    {
        private readonly ToastService _toast;

        public ToastHandler(ToastService toast)
        {
            _toast = toast;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.Headers.TryGetValues(HttpHeaderNames.TOAST_TEXT, out var texts) &&
                response.Headers.TryGetValues(HttpHeaderNames.TOAST_TYPE, out var types))
            {
                if (Enum.TryParse<ToastType>(types.First(), out var type))
                {
                    Console.WriteLine(_toast.GetHashCode());
                    _toast.Show(WebUtility.UrlDecode(texts.First()), type);
                }
            }

            return response;
        }
    }
}
