namespace Games.Views;
using Games.Models;

public partial class ComputerHintPage : ContentPage
{
    private StackLayout _inventoryLayout;
    private BoxView _currentSelectedBackground;
    private Image _currentSelectedItem;

    public ComputerHintPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        this.Opacity = 0;

        try
        {
            // 清除可能存在的二进制标签
            var existingLabel = this.Content.FindByName<Label>("BinaryLabel");
            if (existingLabel != null)
            {
                (this.Content as Grid)?.Children.Remove(existingLabel);
            }

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

            // 显示物品信息（不包括手电筒）
            if (!itemName.Contains("torch"))
            {
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

    private async void OnBackgroundTapped(object sender, EventArgs e)
    {
        try
        {
            // 获取台灯状态
            var lightState = await App.GameStateRepo.GetStateAsync("light_state");
            bool isLightOn = true;  // 默认灯是开着的
            if (lightState != null)
            {
                isLightOn = Convert.ToBoolean(lightState);
            }

            // 检查是否点击的是白色区域、选中了手电筒，且台灯处于关闭状态
            if (sender is Frame && 
                _currentSelectedItem?.Source.ToString().Contains("torch") == true && 
                !isLightOn)  // 只有在灯关闭的时候才显示二进制码
            {
                // 直接在屏幕中央显示二进制代码
                var binaryLabel = new Label
                {
                    Text = "01101100    01101111    01101101    01100101",
                    TextColor = Colors.Black,
                    FontSize = 20,
                    FontAttributes = FontAttributes.Bold,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    IsVisible = true
                };

                // 移除之前的标签（如果存在）
                var existingLabel = this.Content.FindByName<Label>("BinaryLabel");
                if (existingLabel != null)
                {
                    (this.Content as Grid)?.Children.Remove(existingLabel);
                }

                // 添加新标签并设置名称
                binaryLabel.StyleId = "BinaryLabel";  // 使用 StyleId 来代替 x:Name
                (this.Content as Grid)?.Children.Add(binaryLabel);
                Grid.SetColumn(binaryLabel, 0);

                // 取消手电筒的选中效果
                if (_currentSelectedBackground != null && _currentSelectedItem != null)
                {
                    _currentSelectedBackground.IsVisible = false;
                    _currentSelectedItem.Scale = 1.0;
                    _currentSelectedBackground = null;
                    _currentSelectedItem = null;
                }
            }
            else
            {
                // 点击背景时取消物品选中状态
                if (_currentSelectedBackground != null && _currentSelectedItem != null)
                {
                    _currentSelectedBackground.IsVisible = false;
                    _currentSelectedItem.Scale = 1.0;
                    _currentSelectedBackground = null;
                    _currentSelectedItem = null;
                }

                // 移除二进制代码标签（如果存在）
                var binaryLabel = this.Content.FindByName<Label>("BinaryLabel");
                if (binaryLabel != null)
                {
                    (this.Content as Grid)?.Children.Remove(binaryLabel);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in OnBackgroundTapped: {ex.Message}");
        }
    }

    public async void OnBackClicked(object sender, EventArgs e)
    {
        await this.FadeTo(0, 500);
        await Shell.Current.GoToAsync("//Room1Wall2");
    }

    public async void OnSettingClicked(object sender, EventArgs e)
    {
        await App.SettingRepo.UpdateBackPage(1, "ComputerHintPage");
        await Shell.Current.GoToAsync("//SettingPage");
    }
} 