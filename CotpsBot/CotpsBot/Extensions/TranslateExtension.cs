using System;
using System.Reflection;
using System.Resources;
using System.Threading;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace CotpsBot.Extensions
{
    [ContentProperty("Text")]
    public class TranslateExtension: IMarkupExtension
    {
        #region Fields

        const string ResourceId = "CotpsBot.Resources.Lang.LangRes";
        static readonly Lazy<ResourceManager> resmgr = new Lazy<ResourceManager>(() =>
            new ResourceManager(ResourceId, typeof(TranslateExtension).GetTypeInfo().Assembly));

        #endregion

        #region Properties

        public string Text { get; set; }

        #endregion

        #region Methods

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Text == null)
                return "";

            var ci = Thread.CurrentThread.CurrentUICulture;
            var translation = resmgr.Value.GetString(Text, ci);

            if (translation == null)
            {

#if DEBUG
                throw new ArgumentException(
                    String.Format("Key '{0}' was not found in resources '{1}' for culture '{2}'.", Text, ResourceId, ci.Name),
                    "Text");
#else
				translation = Text;
#endif
            }
            return translation;
        }

        #endregion

    }
}