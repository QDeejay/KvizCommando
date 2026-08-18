using KvizCommando.Client.Services.Visual;

namespace KvizCommando.Client.Http
{
    public class LoaderHandler : DelegatingHandler
    {
        private readonly LoaderService _loader;
        public LoaderHandler(LoaderService loader) 
        {
            _loader = loader;
        }
        protected override async Task<HttpResponseMessage> SendAsync(
         HttpRequestMessage request,
         CancellationToken cancellationToken)
        {
           
            try 
            {
                _loader.Trigger();
                var response = await base.SendAsync(request, cancellationToken);
                return response;
            } 
            finally 
            {
                _loader.Hide(); 
            }
        }


    }
}
