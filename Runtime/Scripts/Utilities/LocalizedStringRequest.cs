namespace NPTP.InputSystemWrapper.Utilities
{
    public class LocalizedStringRequest
    {
        public string LocalizationKey { get; }
        
        public string localizedString;

        public LocalizedStringRequest(string localizationKey)
        {
            LocalizationKey = localizationKey;
        }
    }
}