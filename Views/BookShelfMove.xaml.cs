namespace Games.Views;
using Games.Models;
using Microsoft.Maui.Controls.Shapes;

public partial class BookShelfMove : ContentPage
{
    private StackLayout _inventoryLayout;
    private BoxView _currentSelectedBackground;
    private Image _currentSelectedItem;
    
    private Image _selectedBook;
    private double _initialX;
    private Point _dragStart;
    private List<Image> books;
    private Dictionary<Image, int> originalColumns = new Dictionary<Image, int>();
    private double _originalTranslationX;

    public BookShelfMove()
    {
        InitializeComponent();
        
        // 初始化所有六本书
        books = new List<Image> { 
            BookFirst, BookSecond, BookThird, 
            BookFourth, BookFifth, BookSixth 
        };
        
        // 记录每本书的原始位置
        for (int i = 0; i < books.Count; i++)
        {
            originalColumns[books[i]] = i;
        }

        // 为每本书添加拖拽事件
        foreach (var book in books)
        {
            var panGesture = new PanGestureRecognizer();
            panGesture.PanUpdated += OnBookPanUpdated;
            book.GestureRecognizers.Add(panGesture);
        }

        // 添加背景点击事件
        var backgroundTapGesture = new TapGestureRecognizer();
        backgroundTapGesture.Tapped += OnBackgroundTapped;
        BackgroundImage.GestureRecognizers.Add(backgroundTapGesture);
    }

    private void OnBookPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        var book = (Image)sender;
        
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                // 如果已经有选中的书本，先重置它的效果
                if (_selectedBook != null)
                {
                    _selectedBook.Scale = 1.0;
                    _selectedBook.Opacity = 1.0;
                    _selectedBook.ZIndex = 0;
                }

                _selectedBook = book;
                _originalTranslationX = book.TranslationX;
                _dragStart = new Point(e.TotalX, e.TotalY);
                book.ZIndex = 1;
                // 添加拖动时的视觉反馈
                book.Scale = 1.1;
                book.Opacity = 0.8;
                break;

            case GestureStatus.Running:
                if (_selectedBook != null && _selectedBook == book) // 确保只移动当前选中的书本
                {
                    // 直接使用当前触摸点位置���偏移
                    double deltaX = e.TotalX - _dragStart.X;
                    _selectedBook.TranslationX = _originalTranslationX + deltaX;

                    // 实时计算最近的书本并显示视觉提示
                    var nearestBook = FindNearestBook(_selectedBook);
                    if (nearestBook != null)
                    {
                        // 可以添加视觉提示，比如高亮最近的书本
                        foreach (var b in books)
                        {
                            if (b != _selectedBook) // 不改变当前拖动书本的透明度
                            {
                                b.Opacity = (b == nearestBook) ? 1.0 : 0.7;
                            }
                        }
                    }
                }
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                if (_selectedBook != null && _selectedBook == book) // 确保只处理当前选中的书本
                {
                    // 恢复所有书本的视觉效果
                    foreach (var b in books)
                    {
                        b.Scale = 1.0;
                        b.Opacity = 1.0;
                        b.ZIndex = 0;
                    }
                    
                    SwapWithNearestBook(_selectedBook);
                    _selectedBook = null;
                }
                break;
        }
    }

    private Image FindNearestBook(Image draggedBook)
    {
        double draggedCenterX = draggedBook.X + draggedBook.TranslationX + draggedBook.Width / 2;
        
        return books
            .Where(b => b != draggedBook)
            .OrderBy(b => Math.Abs((b.X + b.TranslationX + b.Width / 2) - draggedCenterX))
            .FirstOrDefault();
    }

    private void SwapWithNearestBook(Image draggedBook)
    {
        try
        {
            // 获取拖动书本的中心点X坐标
            double draggedCenterX = draggedBook.X + draggedBook.TranslationX + draggedBook.Width / 2;
            
            // 找到最近的书本
            var nearestBook = books
                .Where(b => b != draggedBook)
                .OrderBy(b => Math.Abs((b.X + b.TranslationX + b.Width / 2) - draggedCenterX))
                .FirstOrDefault();

            if (nearestBook != null)
            {
                var container = (StackLayout)draggedBook.Parent;
                int draggedIndex = container.Children.IndexOf(draggedBook);
                int nearestIndex = container.Children.IndexOf(nearestBook);

                // 创建临时列表来存储当前顺序
                var tempList = container.Children.ToList();
                
                // 在列表中交换位置
                var temp = tempList[draggedIndex];
                tempList[draggedIndex] = tempList[nearestIndex];
                tempList[nearestIndex] = temp;

                // 清空容器
                container.Children.Clear();

                // 重新添加所有元素
                foreach (var item in tempList)
                {
                    container.Children.Add(item);
                }
            }

            // 重置所有书本的TranslationX
            foreach (var book in books)
            {
                book.TranslationX = 0;
            }

            // 检查顺序是否正确
            CheckBookOrder();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SwapWithNearestBook: {ex.Message}");
            draggedBook.TranslationX = 0;
        }
    }

    private async void CheckBookOrder()
    {
        try
        {
            var container = (StackLayout)books[0].Parent;
            
            // 获取当前书本顺序
            var currentOrder = container.Children.Cast<Image>().ToList();
            
            // 检查是否已经获得过电池
            bool hasBattery = await App.GameStateRepo.GetStateAsync("HasBattery");
            if (hasBattery)
            {
                return; // 如果已经获得过电池，直接返回
            }

            // 检查顺序是否正确
            bool isCorrect = true;

            // 检查前三本书是否是 BookSecond、BookFourth、BookFifth（顺序不限）
            var firstThreeBooks = currentOrder.Take(3).ToList();
            if (!firstThreeBooks.Contains(BookSecond) || 
                !firstThreeBooks.Contains(BookFourth) || 
                !firstThreeBooks.Contains(BookFifth))
            {
                isCorrect = false;
            }

            // 检查第四本书是否是 BookThird
            if (currentOrder[3] != BookThird)
            {
                isCorrect = false;
            }

            // 检查最后两本书是否是 BookFirst 和 BookSixth（顺序不限）
            var lastTwoBooks = currentOrder.Skip(4).Take(2).ToList();
            if (!lastTwoBooks.Contains(BookFirst) || 
                !lastTwoBooks.Contains(BookSixth))
            {
                isCorrect = false;
            }

            if (isCorrect)
            {
                // 找到第一个空的物品栏位置
                int slotIndex = -1;
                for (int i = 0; i < _inventoryLayout.Children.Count; i++)
                {
                    if (_inventoryLayout.Children[i] is Image slot && 
                        slot.Source.ToString().EndsWith("inventory.png"))
                    {
                        slotIndex = i;
                        break;
                    }
                }

                if (slotIndex != -1)
                {
                    // 保存到数据库
                    var newItem = new InventoryItem
                    {
                        ItemName = "battery",
                        SlotIndex = slotIndex,
                        IsCollected = true
                    };
                    await App.InventoryRepo.SaveInventoryItemAsync(newItem);

                    // 新物品栏显示
                    UpdateInventorySlot(slotIndex, "battery");

                    // 记录已获得电池的状态
                    await App.GameStateRepo.SaveStateAsync("HasBattery", true);

                    // 显示提示文本和动画效果
                    FragmentHintLabel.Text = "获得了一节电池";
                    FragmentHintLabel.Opacity = 0;
                    FragmentHintLabel.IsVisible = true;

                    // 同时执行文本和物品栏动画
                    if (_inventoryLayout.Children[slotIndex] is Grid grid && 
                        grid.Children.LastOrDefault() is Image inventoryItem)
                    {
                        // 启动字幕淡入动画但不等待它完成
                        FragmentHintLabel.FadeTo(1, 500);
                        
                        // 执行电池的放大缩小动画
                        await inventoryItem.ScaleTo(1.2, 100);
                        await inventoryItem.ScaleTo(1.0, 100);
                        
                        // 等待显示时间后淡出字幕
                        await Task.Delay(1800);
                        await FragmentHintLabel.FadeTo(0, 500);
                    }

                    FragmentHintLabel.IsVisible = false;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CheckBookOrder: {ex.Message}");
        }
    }

    private void OnBackgroundTapped(object sender, EventArgs e)
    {
        // 点击背景时取消物品栏的选中状态
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
            // 重置所有书本到初始位置
            var container = (StackLayout)books[0].Parent;
            container.Children.Clear();
            
            // 按原始顺序重新添加书本
            foreach (var book in new[] { BookFirst, BookSecond, BookThird, BookFourth, BookFifth, BookSixth })
            {
                // 重置书本的所有效果
                book.Scale = 1.0;
                book.Opacity = 1.0;
                book.ZIndex = 0;
                book.TranslationX = 0;
                container.Children.Add(book);
            }

            // 初始化物品栏
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

        // 添加物品栏背景图片
        var originalInventory = new Image
        {
            Source = "inventory.png",
            Aspect = Aspect.AspectFill,
            WidthRequest = 50,
            HeightRequest = 50
        };

        // 添加物品图片
        var inventoryItem = new Image
        {
            Source = itemName + ".png",
            WidthRequest = 40,
            HeightRequest = 40,
            Aspect = Aspect.AspectFill,
            Margin = new Thickness(5)
        };

        // 添加选中时的白色背景
        var whiteBackground = new BoxView
        {
            Color = Colors.White,
            IsVisible = false,
            WidthRequest = 50,
            HeightRequest = 50
        };

        // 按顺序添加各层
        grid.Children.Add(originalInventory);  // 底层：物品栏背景
        grid.Children.Add(whiteBackground);    // 中层：选中效果
        grid.Children.Add(inventoryItem);      // 顶层：物品图片

        // 添加点击事件
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
            // 添加其他物品的描述...
            _ => "未知物品"
        };
    }

    public async void OnBackClicked(object sender, EventArgs e)
    {
        await this.FadeTo(0, 500);
        await Shell.Current.GoToAsync("//Room1Wall3");
    }

    public async void OnSettingClicked(object sender, EventArgs e)
    {
        await App.SettingRepo.UpdateBackPage(1, "BookShelfMove");
        await Shell.Current.GoToAsync("//SettingPage");
    }
} 