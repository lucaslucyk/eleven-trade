using System;
using System.Reflection;
using System.Resources;
using System.Threading;

namespace CotpsBot.Services
{
    public class CodeTranslator : ICodeTranslator
    {
        #region Fields

        private const string ResourceId = "CotpsBot.Resources.Lang.LangRes";

        private static readonly Lazy<ResourceManager> Resmgr = new Lazy<ResourceManager>(() =>
            new ResourceManager(ResourceId, typeof(CodeTranslator).GetTypeInfo().Assembly));

        #endregion

        #region Methods

        public string Translate(string text)
        {
            var translation = Resmgr.Value.GetString(text, Thread.CurrentThread.CurrentUICulture);
            return translation ?? "";
        }

        #endregion
    }
}