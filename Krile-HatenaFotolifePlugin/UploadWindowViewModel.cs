
namespace HatenaFotolifePlugin
{
    public class UploadWindowViewModel
    {
        public UploadWindowViewModel()
        {
            this.Title = Setting.Instance.DefaultTitle;
            this.Folder = Setting.Instance.DefaultFolder;
        }

        public string Title { get; set; }
        public string Folder { get; set; }
    }
}
