namespace Windows.Globalization
{
    using System.Globalization;

    public static class ApplicationLanguages
    {
        public static string PrimaryLanguageOverride
        {
            get { return System.Threading.Thread.CurrentThread.CurrentCulture.Name; }

            set { System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(value); }
        }
    }
}