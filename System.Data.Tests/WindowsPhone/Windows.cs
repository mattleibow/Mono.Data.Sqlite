namespace Windows.Globalization
{
    using global::System.Globalization;
    using global::System.Threading;

    public static class ApplicationLanguages
    {
        public static string PrimaryLanguageOverride
        {
            get { return Thread.CurrentThread.CurrentCulture.Name; }

            set { Thread.CurrentThread.CurrentCulture = new CultureInfo(value); }
        }
    }
}