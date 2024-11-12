namespace NPTP.InputSystemWrapper.Utilities
{
    public class LocalizedStringRequest
    {
        public readonly string localizationKey;
        public string localizedString;

        public LocalizedStringRequest(string localizationKey)
        {
            this.localizationKey = localizationKey;
        }
    }
}