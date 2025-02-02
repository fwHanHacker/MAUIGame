namespace Games.Views;
using Games.Models;

public partial class PaperDisplayPage : ContentPage
{
    private StackLayout _inventoryLayout;
    private BoxView _currentSelectedBackground;
    private Image _currentSelectedItem;

    public PaperDisplayPage()
    {
        InitializeComponent();
        
        // 添加页面背景的点击事件到左侧Grid
        var backgroundTapGesture = new TapGestureRecognizer();
        backgroundTapGesture.Tapped += OnBackgroundTapped;
        
        // 找到左侧的Grid并添加点击事件
        var leftGrid = this.FindByName<Grid>("LeftGrid");
        if (leftGrid != null)
        {
            leftGrid.GestureRecognizers.Add(backgroundTapGesture);
        }
    }

    private void OnBackgroundTapped(object sender, EventArgs e)
    {
        if (_currentSelectedBackground != null && _currentSelectedItem != null)
        {
            _currentSelectedBackground.IsVisible = false;
            _currentSelectedItem.Scale = 1.0;
            _currentSelectedBackground = null;
            _currentSelectedItem = null;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        this.Opacity = 0;

        try
        {
            if (_inventoryLayout == null)
            {
                _inventoryLayout = this.FindByName<StackLayout>("InventoryLayout");
            }

            // 清空物品栏显示
            for (int i = 0; i < _inventoryLayout.Children.Count; i++)
            {
                _inventoryLayout.Children[i] = new Image
                {
                    Source = "inventory.png",
                    Aspect = Aspect.AspectFill,
                    WidthRequest = 50,
                    HeightRequest = 50
                };
            }

            // 从数据库加载物品
            var items = await App.InventoryRepo.GetInventoryItemsAsync();

            // 如果有任何画碎片，在物品栏显示
            var anyPictureFragment = items.FirstOrDefault(i => i.ItemName.Contains("picture_fragment"));
            if (anyPictureFragment != null)
            {
                UpdateInventorySlot(anyPictureFragment.SlotIndex, "sofa_picture_fragment");
            }

            // 加载其他非画碎片物品
            foreach (var item in items.Where(i => !i.ItemName.Contains("picture_fragment")))
            {
                if (item.IsCollected)
                {
                    UpdateInventorySlot(item.SlotIndex, item.ItemName);
                }
            }

            await this.FadeTo(1, 500);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in OnAppearing: {ex.Message}");
            this.Opacity = 1;
        }
    }

    private void UpdateInventorySlot(int slotIndex, string itemName)
    {
        if (slotIndex < 0 || slotIndex >= _inventoryLayout.Children.Count)
            return;

        var grid = new Grid
        {
            WidthRequest = 50,
            HeightRequest = 50
        };

        var originalInventory = new Image
        {
            Source = "inventory.png",
            Aspect = Aspect.AspectFill,
            WidthRequest = 50,
            HeightRequest = 50
        };

        string imageSource = itemName;
        if (itemName.Contains("picture_fragment"))
        {
            imageSource = "sofa_picture_fragment";
        }

        var inventoryItem = new Image
        {
            Source = imageSource + ".png",
            WidthRequest = 40,
            HeightRequest = 40,
            Aspect = Aspect.AspectFill,
            Margin = new Thickness(5)
        };

        var whiteBackground = new BoxView
        {
            Color = Colors.White,
            IsVisible = false,
            WidthRequest = 50,
            HeightRequest = 50
        };

        grid.Children.Add(originalInventory);
        grid.Children.Add(whiteBackground);
        grid.Children.Add(inventoryItem);

        var tapGestureRecognizer = new TapGestureRecognizer();
        tapGestureRecognizer.Tapped += async (s, e) => 
        {
            await OnInventoryItemClicked(grid, itemName, whiteBackground, inventoryItem);
        };
        grid.GestureRecognizers.Add(tapGestureRecognizer);

        _inventoryLayout.Children[slotIndex] = grid;
    }

    private async Task OnInventoryItemClicked(Grid grid, string itemName, BoxView whiteBackground, Image inventoryItem)
    {
        if (string.IsNullOrEmpty(itemName) || itemName == "inventory.png")
            return;

        if (_currentSelectedBackground != null && _currentSelectedItem != null)
        {
            _currentSelectedBackground.IsVisible = false;
            _currentSelectedItem.Scale = 1.0;
        }

        whiteBackground.IsVisible = true;
        await inventoryItem.ScaleTo(1.2, 100);

        _currentSelectedBackground = whiteBackground;
        _currentSelectedItem = inventoryItem;

        string itemInfo;
        if (itemName.Contains("picture_fragment"))
        {
            var items = await App.InventoryRepo.GetInventoryItemsAsync();
            int fragmentCount = items.Count(i => i.ItemName.Contains("picture_fragment"));
            itemInfo = $"画碎片 {fragmentCount}/5";
        }
        else
        {
            itemInfo = GetItemInfo(itemName);
        }

        FragmentHintLabel.Text = itemInfo;
        FragmentHintLabel.Opacity = 0;
        FragmentHintLabel.IsVisible = true;

        await FragmentHintLabel.FadeTo(1, 500);
        await Task.Delay(1800);
        await FragmentHintLabel.FadeTo(0, 500);
        FragmentHintLabel.IsVisible = false;
    }

    private string GetItemInfo(string itemName)
    {
        return itemName switch
        {
            "torch" => "一个紫外线手电筒",
            "knife" => "一把小刀",
            "dishcloth" => "衣服的一角",
            "battery" => "一节电池",
            _ => "未知物品"
        };
    }

    public async void OnSettingClicked(object sender, EventArgs e)
    {
        await App.SettingRepo.UpdateBackPage(1, "PaperDisplayPage");
        await Shell.Current.GoToAsync("//SettingPage");
    }

    public async void OnDownButtonClicked(object sender, EventArgs e)
    {
        await this.FadeTo(0, 500);
        await Shell.Current.GoToAsync("//Room1Wall1");
    }
} 