namespace Windows.Globalization
{
    public static class ApplicationLanguages
    {
        public static string PrimaryLanguageOverride
        {
            get { return global::System.Threading.Thread.CurrentThread.CurrentCulture.Name; }

            set { global::System.Threading.Thread.CurrentThread.CurrentCulture = new global::System.Globalization.CultureInfo(value); }
        }
    }
}