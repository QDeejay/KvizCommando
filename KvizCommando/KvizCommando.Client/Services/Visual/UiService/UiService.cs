using KvizCommando.Client.Services.Visual.UiService.Language;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Services.Visual.UiService
{
    public sealed class UiServices
    {
        public ModalService Modal { get; }
        //public ToastService Toast { get; }
        //public LoaderService Loader { get; }
        public PageHeaderService Header { get; }
        public IDisplayMessageState HeadDisplay { get; }
        public NavigationManager Nav { get; }
        public ILanguageService Lang { get; }

        public UiServices(
            ModalService modal,
            //ToastService toast,
            //LoaderService loader,
            PageHeaderService header,
            IDisplayMessageState headDisplay,
            NavigationManager nav,
            ILanguageService lang)
        {
            Modal = modal;
            //Toast = toast;
            //Loader = loader;
            Header = header;
            HeadDisplay = headDisplay;
            Nav = nav;
            Lang = lang;
        }
    }
}
