namespace Games.Views;
using Games.Models;

public partial class LightSwitchPage : ContentPage
{
    private StackLayout _inventoryLayout;
    private BoxView _currentSelectedBackground;
    private Image _currentSelectedItem;
    private bool _isLightOn = false;
    private Image _lightImage;
    private Label _fragmentHintLabel;

    public LightSwitchPage()
    {
        InitializeComponent();
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

            if (_inventoryLayout == null)
            {
                Console.WriteLine("Error: InventoryLayout not found");
                return;
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

            // 初始化灯的图片控件
            _lightImage = this.FindByName<Image>("LightImage");
            
            // 从数据库加载灯的状态，如果没有状态则默认为开启
            var lightState = await App.GameStateRepo.GetStateAsync("light_state");
            if (lightState == null)
            {
                // 如果数据库中没有状态，设置为开启状态
                _isLightOn = true;
                await App.GameStateRepo.SaveStateAsync("light_state", true);
            }
            else
            {
                _isLightOn = Convert.ToBoolean(lightState);
            }

            if (_lightImage != null)
            {
                _lightImage.Source = _isLightOn ? "light_on.png" : "light_off.png";
            }

            await this.FadeTo(1, 500);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in OnAppearing: {ex.Message}");
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

        var inventoryItem = new Image
        {
            Source = itemName + ".png",
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
        try
        {
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
                int fragmentCount = items.Count(i => i.ItemName.Contains("picture_fragment") && i.IsCollected);
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
        catch (Exception ex)
        {
            Console.WriteLine($"Error in OnInventoryItemClicked: {ex.Message}");
        }
    }

    private string GetItemInfo(string itemName)
    {
        return itemName switch
        {
            "torch" => "一个紫外线手电筒",
            "knife" => "一把小刀",
            "dishcloth" => "衣服的一角",
            "battery" => "一节电池",
            "blue_and_yellow_ball" => "一个蓝色的球和一个黄色的球",
            _ => "未知物品"
        };
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

    public async void OnBackClicked(object sender, EventArgs e)
    {
        await this.FadeTo(0, 500);
        await Shell.Current.GoToAsync("//Room1Wall2");
    }

    public async void OnSettingClicked(object sender, EventArgs e)
    {
        await App.SettingRepo.UpdateBackPage(1, "LightSwitchPage");
        await Shell.Current.GoToAsync("//SettingPage");
    }

    public async void OnLightImageClicked(object sender, EventArgs e)
    {
        try
        {
            if (_currentSelectedBackground == null) // 没有选中任何物品
            {
                // 不论灯的状态如何，都显示需要电池的提示
                FragmentHintLabel.Text = "需要电池";  // 使用 XAML 中定义的标签
                FragmentHintLabel.Opacity = 0;
                FragmentHintLabel.IsVisible = true;
                await FragmentHintLabel.FadeTo(1, 500);
                await Task.Delay(1800);
                await FragmentHintLabel.FadeTo(0, 500);
                FragmentHintLabel.IsVisible = false;
                return;
            }

            // 检查是否选中了电池
            if (_currentSelectedItem?.Source.ToString().Contains("battery") == true)
            {
                // 先取消选中效果
                if (_currentSelectedBackground != null && _currentSelectedItem != null)
                {
                    _currentSelectedBackground.IsVisible = false;
                    _currentSelectedItem.Scale = 1.0;
                    _currentSelectedBackground = null;
                    _currentSelectedItem = null;
                }

                // 使用电池
                await UseBattery();
                // 根据当前状态切换灯
                await ToggleLight();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in OnLightImageClicked: {ex.Message}");
        }
    }

    private async Task ToggleLight()
    {
        _isLightOn = !_isLightOn;
        if (_lightImage != null)
        {
            _lightImage.Source = _isLightOn ? "light_on.png" : "light_off.png";
        }
        
        // 保存灯的状态到数据库
        bool stateToSave = _isLightOn;  // 先保存到布尔变量
        await App.GameStateRepo.SaveStateAsync("light_state", stateToSave);  // 直接传递布尔值

        // 显示开关灯的提示文字
        FragmentHintLabel.Text = _isLightOn ? "开灯" : "关灯";
        FragmentHintLabel.Opacity = 0;
        FragmentHintLabel.IsVisible = true;
        await FragmentHintLabel.FadeTo(1, 500);
        await Task.Delay(1800);
        await FragmentHintLabel.FadeTo(0, 500);
        FragmentHintLabel.IsVisible = false;
    }

    private async Task UseBattery()
    {
        try
        {
            // 从数据库中删除电池
            var items = await App.InventoryRepo.GetInventoryItemsAsync();
            var battery = items.FirstOrDefault(i => i.ItemName == "battery");
            if (battery != null)
            {
                await App.InventoryRepo.DeleteInventoryItemAsync(battery);
            }

            // 清空选中状态
            if (_currentSelectedBackground != null && _currentSelectedItem != null)
            {
                _currentSelectedBackground.IsVisible = false;
                _currentSelectedItem.Scale = 1.0;
                _currentSelectedBackground = null;
                _currentSelectedItem = null;
            }

            // 刷新物品栏显示
            OnAppearing();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in UseBattery: {ex.Message}");
        }
    }
} 