namespace Games.Views;
using Games.Models;

public partial class MoveBooksPuzzle : ContentPage
{
    private bool isNavigating = false;  // 添加导航状态标志
    private Image draggedBook;
    private double startX, startY;
    private readonly Dictionary<Image, (double X, double Y)> originalPositions;
    private readonly Dictionary<Image, Image> bookStack;
    private readonly double[] axisPositions = new double[] { 210.0, 360.0, 499.0 }; // 更新为新的轴线位置

    // 添加物品栏相关字段
    private StackLayout _inventoryLayout;
    private BoxView _currentSelectedBackground;
    private Image _currentSelectedItem;
    private bool _hasPictureFragment = false;

    public MoveBooksPuzzle()
    {
        InitializeComponent();
        originalPositions = new Dictionary<Image, (double X, double Y)>();
        bookStack = new Dictionary<Image, Image>();
        this.Opacity = 0;  // 确保初始透明度为0

        // 初始化书本位置
        InitializeBooks();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        this.Opacity = 0;
        isNavigating = false;

        try
        {
            // 重置所有书本到初始位置
            Book1.Margin = new Thickness(130, 0, 68, 92);
            Book2.Margin = new Thickness(150, 0, 138, 135);
            Book3.Margin = new Thickness(449, 0, 138, 92);
            Book4.Margin = new Thickness(310, 0, 13, 92);
            Book5.Margin = new Thickness(315, 0, 138, 117);

            // 重置所有书本的效果
            foreach (var book in new[] { Book1, Book2, Book3, Book4, Book5 })
            {
                book.Scale = 1.0;
                book.Opacity = 1.0;
                book.ZIndex = 0;
                book.TranslationX = 0;
            }

            // 清空堆叠关系
            bookStack.Clear();
            draggedBook = null;

            // 重置状态
            _hasPictureFragment = false;

            // 初始化物品栏
            if (_inventoryLayout == null)
            {
                _inventoryLayout = this.FindByName<StackLayout>("InventoryLayout");
            }

            if (_inventoryLayout == null)
            {
                Console.WriteLine("Error: InventoryLayout not found");
                return;  // 如果找不到物品栏，直接返回而不设置页面可见
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

            // 检查当前房间的画碎片是否已被收集
            var booksPictureFragment = await App.InventoryRepo.GetInventoryItemAsync("books_picture_fragment");
            if (booksPictureFragment != null && booksPictureFragment.IsCollected)  // 修改这里的条件判断
            {
                _hasPictureFragment = true;
            }

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

            // 最后再设置页面可见
            await this.FadeTo(1, 500);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in OnAppearing: {ex.Message}");
            // 确保页面在出错时也是可见的
            this.Opacity = 1;
        }
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        // 重置所有状态
        draggedBook = null;
        bookStack.Clear();
        foreach (var book in new[] { Book1, Book2, Book3, Book4, Book5 })
        {
            if (originalPositions.ContainsKey(book))
            {
                var (originalX, originalY) = originalPositions[book];
                book.Margin = new Thickness(originalX, 0, book.Margin.Right, originalY);
            }
            book.ZIndex = 0;
        }
    }

    private void InitializeBooks()
    {
        // 设置初始位置
        originalPositions[Book1] = (130, 92);
        originalPositions[Book2] = (150, 135);
        originalPositions[Book3] = (449, 92);
        originalPositions[Book4] = (310, 92);
        originalPositions[Book5] = (315, 117);

        // 为每本书添加拖拽手势
        foreach (var book in new[] { Book1, Book2, Book3, Book4, Book5 })
        {
            var panGesture = new PanGestureRecognizer();
            panGesture.TouchPoints = 1;  // 设置单点触控
            panGesture.PanUpdated += OnBookPanUpdated;
            book.GestureRecognizers.Add(panGesture);
        }
    }

    private bool isDragging = false;
    private double startDragX, startDragY;

    private void OnBookPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        if (sender is Image book)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    if (CanMoveBook(book))
                    {
                        isDragging = true;
                        draggedBook = book;
                        startDragX = book.Margin.Left;
                        startDragY = book.Margin.Bottom;
                        book.ZIndex = 100;
                    }
                    break;

                case GestureStatus.Running:
                    if (isDragging && draggedBook != null)
                    {
                        // 计算新位置
                        double newX = startDragX + e.TotalX;
                        double newY = startDragY - e.TotalY;

                        // 更新书本位置
                        draggedBook.Margin = new Thickness(
                            newX,
                            0,
                            draggedBook.Margin.Right,
                            newY
                        );
                    }
                    break;

                case GestureStatus.Completed:
                    if (isDragging && draggedBook != null)
                    {
                        try
                        {
                            // 找到最近的轴线
                            double bookCenterX = draggedBook.Margin.Left + draggedBook.Width / 2;
                            double nearestAxis = axisPositions
                                .OrderBy(x => Math.Abs(x - bookCenterX))
                                .First();

                            // 获取该轴线上的所有书本
                            var booksOnAxis = new[] { Book1, Book2, Book3, Book4, Book5 }
                                .Where(b => b != draggedBook)
                                .Where(b => Math.Abs((b.Margin.Left + b.Width / 2) - nearestAxis) < 30)
                                .OrderByDescending(b => b.Margin.Bottom)
                                .ToList();

                            // 设置最终位置
                            double targetBottom;
                            if (booksOnAxis.Any())
                            {
                                var topBook = booksOnAxis.First();
                                targetBottom = topBook.Margin.Bottom + topBook.Height;
                            }
                            else
                            {
                                targetBottom = 92;
                            }

                            // 设置到目标位置
                            draggedBook.Margin = new Thickness(
                                nearestAxis - draggedBook.Width / 2,
                                0,
                                draggedBook.Margin.Right,
                                targetBottom
                            );

                            // 更新原始位置
                            originalPositions[draggedBook] = (nearestAxis - draggedBook.Width / 2, targetBottom);

                            // 检查是否完成谜题
                            CheckPuzzleComplete();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error in drag completion: {ex.Message}");
                            ReturnBookToOriginalPosition(draggedBook);
                        }
                        finally
                        {
                            isDragging = false;
                            draggedBook.ZIndex = 0;
                            draggedBook = null;
                        }
                    }
                    break;
            }
        }
    }

    private Image FindTopBookOnAxis(double axis)
    {
        var booksOnAxis = new[] { Book1, Book2, Book3, Book4, Book5 }
            .Where(b => Math.Abs((b.Margin.Left + b.Width / 2) - axis) < 30)
            .Where(b => !bookStack.ContainsKey(b)) // 只考虑最顶层的书
            .OrderByDescending(b => b.Margin.Bottom + b.Height) // 考虑书本高度
            .FirstOrDefault();
        return booksOnAxis;
    }

    private void PlaceBookOnAxis(Image book, double axis)
    {
        // 计算使书本中心对齐到轴线的左边距
        double newLeft = axis - book.Width / 2;

        // 找到该轴上所有的书，按底部位置从低到高排序
        var booksOnAxis = new[] { Book1, Book2, Book3, Book4, Book5 }
            .Where(b => b != book)
            .Where(b => Math.Abs((b.Margin.Left + b.Width / 2) - axis) < 30)
            .OrderByDescending(b => b.Margin.Bottom + b.Height);  // 修改为从高到低排序

        // 清除当前书的堆叠关系
        bookStack.Remove(book);

        double newBottom;

        // 找到轴上最高的书
        var highestBook = booksOnAxis.FirstOrDefault();  // 由于是从高到低排序，直接取第一个
        if (highestBook != null)
        {
            // 如果轴上有书，放在最高的书上面
            newBottom = highestBook.Margin.Bottom + highestBook.Height;
            bookStack[book] = highestBook;
        }
        else
        {
            // 如果轴上没有书，使用基准高度
            newBottom = 92;
        }

        // 使用动画移动到新位置
        var animation = new Animation();

        animation.Add(0, 1, new Animation((value) =>
        {
            book.Margin = new Thickness(value, 0, book.Margin.Right, book.Margin.Bottom);
        }, book.Margin.Left, newLeft));

        animation.Add(0, 1, new Animation((value) =>
        {
            book.Margin = new Thickness(book.Margin.Left, 0, book.Margin.Right, value);
        }, book.Margin.Bottom, newBottom));

        animation.Commit(book, "AlignAnimation", 16, 250, Easing.CubicInOut);
        originalPositions[book] = (newLeft, newBottom);

        // 检查是否完成谜题
        CheckPuzzleComplete();
    }

    private void PlaceBookOnTarget(Image draggedBook, Image targetBook)
    {
        // 清除旧的堆叠关系
        bookStack.Remove(draggedBook);

        // 找到目标书所在轴
        double targetAxis = axisPositions
            .OrderBy(x => Math.Abs((targetBook.Margin.Left + targetBook.Width / 2) - x))
            .First();

        // 计算新位置（中心对齐到轴线，直接贴合目标书）
        double newLeft = targetAxis - draggedBook.Width / 2;
        double newBottom = targetBook.Margin.Bottom + targetBook.Height; // 直接贴合没有间隙

        // 检查是否会与其他书重叠
        var otherBooksOnAxis = new[] { Book1, Book2, Book3, Book4, Book5 }
            .Where(b => b != draggedBook && b != targetBook)
            .Where(b => Math.Abs((b.Margin.Left + b.Width / 2) - targetAxis) < 30)
            .Where(b => Math.Abs(b.Margin.Bottom - newBottom) < 1);

        if (otherBooksOnAxis.Any())
        {
            ReturnBookToOriginalPosition(draggedBook);
            return;
        }

        // 更新堆叠关系

        bookStack[draggedBook] = targetBook;

        // 用动画移动到新位置
        var animation = new Animation();

        animation.Add(0, 1, new Animation((value) =>
        {
            draggedBook.Margin = new Thickness(
                value,
                0,
                draggedBook.Margin.Right,
                draggedBook.Margin.Bottom
            );
        }, draggedBook.Margin.Left, newLeft));

        animation.Add(0, 1, new Animation((value) =>
        {
            draggedBook.Margin = new Thickness(
                draggedBook.Margin.Left,
                0,
                draggedBook.Margin.Right,
                value
            );
        }, draggedBook.Margin.Bottom, newBottom));

        animation.Commit(draggedBook, "StackAnimation", 16, 250, Easing.CubicInOut);

        // 检查是否完成谜题
        CheckPuzzleComplete();
    }

    private void ReturnBookToOriginalPosition(Image book)
    {
        if (!originalPositions.ContainsKey(book)) return;

        var (originalX, originalY) = originalPositions[book];

        var animation = new Animation();

        animation.Add(0, 1, new Animation((value) =>
        {
            book.Margin = new Thickness(
                value,
                0,
                book.Margin.Right,
                book.Margin.Bottom
            );
        }, book.Margin.Left, originalX));

        animation.Add(0, 1, new Animation((value) =>
        {
            book.Margin = new Thickness(
                book.Margin.Left,
                0,
                book.Margin.Right,
                value
            );
        }, book.Margin.Bottom, originalY));

        animation.Commit(book, "ReturnAnimation", 16, 250, Easing.SpringOut);
    }

    private async void CheckPuzzleComplete()
    {
        try
        {
            var booksOnThirdAxis = new[] { Book1, Book2, Book3, Book4, Book5 }
                .Where(b => Math.Abs((b.Margin.Left + b.Width / 2) - axisPositions[2]) < 30)
                .OrderBy(b => b.Margin.Bottom)
                .ToList();

            if (booksOnThirdAxis.Count != 5)
            {
                return;
            }

            if (booksOnThirdAxis[0] != Book1 ||
                booksOnThirdAxis[1] != Book2 ||
                booksOnThirdAxis[2] != Book3 ||
                booksOnThirdAxis[3] != Book4 ||
                booksOnThirdAxis[4] != Book5)
            {
                return;
            }

            if (!_hasPictureFragment)
            {
                // 找到一个空的物品栏位置或已有画碎片的位置
                var items = await App.InventoryRepo.GetInventoryItemsAsync();
                var anyPictureFragment = items.FirstOrDefault(i => i.ItemName.Contains("picture_fragment"));
                int slotIndex;

                if (anyPictureFragment != null)
                {
                    slotIndex = anyPictureFragment.SlotIndex;
                }
                else
                {
                    slotIndex = -1;
                    for (int i = 0; i < _inventoryLayout.Children.Count; i++)
                    {
                        if (_inventoryLayout.Children[i] is Image slot &&
                            slot.Source.ToString().EndsWith("inventory.png"))
                        {
                            slotIndex = i;
                            break;
                        }
                    }
                }

                if (slotIndex != -1)
                {
                    // 先保存到数据库
                    var newItem = new InventoryItem
                    {
                        ItemName = "books_picture_fragment",
                        SlotIndex = slotIndex,
                        IsCollected = true
                    };
                    await App.InventoryRepo.SaveInventoryItemAsync(newItem);

                    // 更新UI
                    UpdateInventorySlot(slotIndex, "sofa_picture_fragment");
                    _hasPictureFragment = true;

                    // ���新获取最新的画碎片数量
                    items = await App.InventoryRepo.GetInventoryItemsAsync();
                    int fragmentCount = items.Count(i => i.ItemName.Contains("picture_fragment"));

                    // 显示提示文本和动画效果
                    FragmentHintLabel.Text = $"画碎片 {fragmentCount}/5";
                    FragmentHintLabel.Opacity = 0;
                    FragmentHintLabel.IsVisible = true;

                    // 同时执行文本和物品栏动画
                    if (_inventoryLayout.Children[slotIndex] is Grid grid &&
                        grid.Children.LastOrDefault() is Image inventoryItem)
                    {
                        // 启动字幕淡入动画但不等待它完成
                        FragmentHintLabel.FadeTo(1, 500);

                        // 执行画碎片的放大缩小动画
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
            Console.WriteLine($"Error in CheckPuzzleComplete: {ex.Message}");
        }
    }

    private bool CheckColorsMatch(Image upperBook, Image lowerBook)
    {
        // 获取上下两本书的颜色
        var upperColor = GetBookColor(upperBook);
        var lowerColor = GetBookColor(lowerBook);

        // 检查相邻的颜色是否匹配
        return upperColor == lowerColor;
    }

    private string GetBookColor(Image book)
    {
        // 根据图片中显示的颜色来判断
        if (book == Book1) return "green";  // 第一本书顶部是绿色
        if (book == Book2) return "green";  // 第二本书底部是绿色
        if (book == Book3) return "brown";  // 第三本书是棕色
        if (book == Book4) return "brown";  // 第四本书顶部是棕色
        if (book == Book5) return "brown";  // 第五本书底部是棕色
        return "";
    }

    private bool IsOnThirdAxis(Image book)
    {
        // 检查book1是否在第三个轴上
        var bookBounds = book.Bounds;
        return Math.Abs(bookBounds.Center.X - 419) < 50; // 使用Book3的X坐标作为参考
    }

    public async void OnBackClicked(object sender, EventArgs e)
    {
        if (!isNavigating && this.Opacity > 0)  // 检查导航状态和页面是否可见
        {
            isNavigating = true;
            await this.FadeTo(0, 500);
            await Shell.Current.GoToAsync("//Room1Wall3");
        }
    }

    public async void OnSettingClicked(object sender, EventArgs e)
    {
        if (!isNavigating && this.Opacity > 0)
        {
            isNavigating = true;
            await App.SettingRepo.UpdateBackPage(1, "MoveBooksPuzzle");
            await Shell.Current.GoToAsync("//SettingPage");
        }
    }

    private bool CanMoveBook(Image book)
    {
        try
        {
            // 检查是否有书直接放在本书上面（过堆叠关系）
            if (bookStack.Values.Contains(book))
            {
                return false;  // 如果是其他书的底座，不能移动
            }

            // 获取书本当前位置
            double bookCenterX = book.Margin.Left + book.Width / 2;
            double bookBottom = book.Margin.Bottom;

            // 找到同一轴上的所有书
            var booksOnSameAxis = new[] { Book1, Book2, Book3, Book4, Book5 }
                .Where(b => b != book)
                .Where(b => Math.Abs((b.Margin.Left + b.Width / 2) - bookCenterX) < 30);

            // 检查是否有书在正上方
            foreach (var otherBook in booksOnSameAxis)
            {
                if (Math.Abs(otherBook.Margin.Bottom - (bookBottom + book.Height)) < 1)
                {
                    return false;  // 如果有书在正上方，不能移动
                }
            }

            return true;  // 他情况都可以移动
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CanMoveBook: {ex.Message}");
            return false;
        }
    }

    private Image FindTargetBook(Image draggedBook)
    {
        // 获取拖动书本的中心点和底部位置
        double draggedCenterX = draggedBook.Margin.Left + draggedBook.Width / 2;
        double draggedBottom = draggedBook.Margin.Bottom;

        // 找到最近的轴
        double nearestAxis = axisPositions
            .OrderBy(x => Math.Abs(draggedCenterX - x))
            .First();

        // 在最近的轴上找所有书，按底部位置从低到高排序（这样可以找到最底层的书）
        var booksOnAxis = new[] { Book1, Book2, Book3, Book4, Book5 }
            .Where(b => b != draggedBook)
            .Where(b => Math.Abs((b.Margin.Left + b.Width / 2) - nearestAxis) < 30)
            .OrderBy(b => b.Margin.Bottom);  // 从到高排序

        // 找到第一本比拖动书底部更低的书
        var targetBook = booksOnAxis
            .Where(b => draggedBottom > b.Margin.Bottom + b.Height)
            .LastOrDefault();  // 取最高的那本

        return targetBook;
    }

    private bool IsOverBook(Image draggedBook, Image targetBook)
    {
        // 获取两本书的中心点和位置
        double draggedCenterX = draggedBook.Margin.Left + draggedBook.Width / 2;
        double targetCenterX = targetBook.Margin.Left + targetBook.Width / 2;
        double draggedBottom = draggedBook.Margin.Bottom;
        double targetBottom = targetBook.Margin.Bottom;

        // 检查是否在同一轴线上
        bool onSameAxis = Math.Abs(draggedCenterX - targetCenterX) < 50; // 放宽判定范围

        // 垂直位置（确保拖动的书在目标书上方）
        bool verticallyAligned = draggedBottom < targetBottom + targetBook.Height;

        return onSameAxis && verticallyAligned;
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
}