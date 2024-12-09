using Games.SQL;

namespace Games
{
    public partial class App : Application
    {
        public static SettingRepository SettingRepo { get; private set; }
        public App(SettingRepository repo)
        {
            InitializeComponent();

            MainPage = (new AppShell());

            SettingRepo = repo;
        }
    }
}
