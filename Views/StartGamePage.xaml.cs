namespace Games.Views;

public partial class StartGamePage : ContentPage
{
    public StartGamePage()
    {
        InitializeComponent();
    }

    private async Task InitializeBtnAsync()
    {
        try
        {
            // 获取主Grid
            if (Content is Grid mainGrid)
            {
                // 移除所有StackLayout（按钮容器）
                var stackLayouts = mainGrid.Children.OfType<StackLayout>().ToList();
                foreach (var stackLayout in stackLayouts)
                {
                    mainGrid.Children.Remove(stackLayout);
                }
            }

            if (!await App.SettingRepo.IsSettingExists(1))
            {
                await App.SettingRepo.AddNewSetting(1, 0, "StartGamePage");
            }

            var archive = await App.SettingRepo.GetArchiveById(1);
            if (archive == 0)
            {
                StartGameBtn1();
            }
            else if (archive == 1)
            {
                StartGameBtn2();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"初始化按钮出错: {ex.Message}");
            StartGameBtn1(); // 出错时显示默认按钮
        }
    }

    public async void OnSettingClicked(object sender, EventArgs e)
    {
        await App.SettingRepo.UpdateBackPage(1, "StartGamePage");
        await Shell.Current.GoToAsync("//SettingPage");
    }

    public async void OnButtonClicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            if (button.Text == "新游戏")
            {
                try
                {
                    // 重置所有游戏数据
                    await App.SettingRepo.ResetGame(1);
                    await App.InventoryRepo.ResetInventoryAsync();
                    // 重置游戏状态数据
                    await App.GameStateRepo.ResetGameStatesAsync();
                    
                    // 设置存档状态为1（表示有存档）
                    await App.SettingRepo.UpdateArchive(1, 1);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"重置游戏数据时出错: {ex.Message}");
                }
            }
            // 继续游戏时不做任何数据清除，直接进入游戏
        }

        await this.FadeTo(0, 1000);
        await Shell.Current.GoToAsync("//Room1Wall1");
    }

    public void StartGameBtn1()
    {
        var startgameBtn = new Button
        {
            Text = "新游戏",
            HorizontalOptions = LayoutOptions.Center,
            BackgroundColor = Colors.Purple,
            TextColor = Colors.White,
            Margin = new Thickness(0, 5)
        };
        startgameBtn.Clicked += OnButtonClicked;

        var stackLayout = new StackLayout
        {
            Orientation = StackOrientation.Vertical,
            VerticalOptions = LayoutOptions.End,
            HorizontalOptions = LayoutOptions.Center,
            Spacing = 10,
            Margin = new Thickness(0, 0, 0, 30)
        };
        stackLayout.Children.Add(startgameBtn);

        if (Content is Grid grid)
        {
            grid.Children.Add(stackLayout);
        }
    }

    public void StartGameBtn2()
    {
        var continueGameBtn = new Button
        {
            Text = "继续游戏",
            HorizontalOptions = LayoutOptions.Center,
            BackgroundColor = Colors.Purple,
            TextColor = Colors.White,
            Margin = new Thickness(0, 5)
        };
        continueGameBtn.Clicked += OnButtonClicked;

        var newGameBtn = new Button
        {
            Text = "新游戏",
            HorizontalOptions = LayoutOptions.Center,
            BackgroundColor = Colors.Purple,
            TextColor = Colors.White,
            Margin = new Thickness(0, 5)
        };
        newGameBtn.Clicked += OnButtonClicked;

        var stackLayout = new StackLayout
        {
            Orientation = StackOrientation.Vertical,
            VerticalOptions = LayoutOptions.End,
            HorizontalOptions = LayoutOptions.Center,
            Spacing = 10,
            Margin = new Thickness(0, 0, 0, 30)
        };
        stackLayout.Children.Add(continueGameBtn);
        stackLayout.Children.Add(newGameBtn);

        if (Content is Grid grid)
        {
            grid.Children.Add(stackLayout);
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        this.Opacity = 0;
        await InitializeBtnAsync();
        await this.FadeTo(1, 500);
    }
}