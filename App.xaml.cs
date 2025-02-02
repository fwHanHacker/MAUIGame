using Games.Services;
using System.IO;

namespace Games
{
    public partial class App : Application
    {
        public static SettingDatabase SettingRepo { get; private set; }
        public static InventoryDatabase InventoryRepo { get; private set; }
        public static GameStateDatabase GameStateRepo { get; private set; }

        public App(SettingDatabase settingRepo, InventoryDatabase inventoryRepo)
        {
            InitializeComponent();
            MainPage = new AppShell();
            SettingRepo = settingRepo;
            InventoryRepo = inventoryRepo;
            GameStateRepo = new GameStateDatabase();
        }
    }
}
