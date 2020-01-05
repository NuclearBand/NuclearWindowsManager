### Introduction

In any game, even the simplest Unity game, there is UI. It includes the main menu, various auxiliary windows with settings, game interface and additional windows of the game interface (inventory, character window, shop and so on).

I think many developers have their own window manager that allows to conveniently open and close windows. My window manager possesses additional functionality - it is able to reconstruct windows in case of change of orientation of the screen (in the presence of alternative windows for different orientations), processes pressing of Back button (for Android), is able to block input at inactive windows and and a couple more features.

In this article I'd like to share the UI development workflow with my library by an example of simple interface development.

### Window creation
We will create an interface with two windows.

![Interface mockup](https://habrastorage.org/webt/ko/3q/pv/ko3qpvkiynygnistugenttehfp8.gif)

Each window is a separate prefab. Since version 2018.3, windows are easier to edit, because you can set the UI Environment - canvas for windows in the settings, so you no longer need to switch to a separate scene to prototype the UI.

![UIEnvironmen settingst](https://habrastorage.org/webt/ep/xg/ni/epxgnipwsewhdt92dypqf47nv6m.png)
![TestWindow1](https://habrastorage.org/webt/2k/md/c_/2kmdc_m4npym1a--vgwvommeukw.png)
![TestWindow2](https://habrastorage.org/webt/gk/ia/4h/gkia4hd9hvp196h5p4d-q1htjwq.png)

I store all window prefabs in the Resources folder so that they can be instantiated along a relative path.
Windows have a base class "Window", which is MonoBehaviour.

In each game I create a "base child", and then I inherit specific windows from it. This is useful because in each game all windows often have something in common - like opening/closing animations, or some additional features used only in this game. It is convenient to store the address of a window in a static variable of the class itself, or to have a separate file with paths to all window prefabs.

```csharp
public class TestWindow : Window
{
}
```
```csharp
public class TestWindow1 : TestWindow 
{
    public static string Path = "TestWindow1";
}
```
```csharp
public class TestWindow2 : TestWindow 
{
    public static string Path = "TestWindow2";
}
```
I also keep the prefab of canvas as well as the prefab of InputBlocker in the resources. This is a UI element that is inserted between windows when you want to block input in one window while another is active. In this case, we need to make sure that the buttons of other windows do not react when Window2 window is active. Therefore we need to put a checkmark in the WithInputBlockForBackground checkbox in its settings. 
![InputBlocker](https://habrastorage.org/webt/bi/vt/ua/bivtuaskifpm6epeiebq_znu4fo.png)

The appearance of the windows is ready, so you can move on to writing logic.

### Opening/closing of windows

My manager is a WindowsManager static class. When loading a game, you need to initialize it - give it the address of root canvas. Additionally, you can specify the InputBlocker address. In this interface it is used, so it is necessary. To initialize it, we'll place it in the GameObject scene with the Starter script.
```csharp
WindowsManager.Init(new WindowsManagerSettings()
{
    RootPath = "Canvas",
    InputBlockPath = "InputBlocker"
});
```

After initialization, you can start creating windows.
```csharp
var windowReference = WindowsManager.CreateWindow(TestWindow1.Path);
```

An additional parameter of this method is the function through which the window receives data. We use it to pass a header to it when creating TestWindow2.

```csharp
WindowsManager.CreateWindow(
    TestWindow2.Path,
    window => (window as TestWindow2).Title = inputField.text
    );
```

We use the transferred data in the Init method, which is called each time a window is created.

```csharp
public string Title;
public Text text;
public override void Init()
{
    base.Init();
    text.text = Title;
}
```
Now we need to add animations. To do this, let's override StartShowAnimation and StartHideAnimation methods. For the manager to understand that the animation is over and the window opens/closes when the animation is finished, we need to call EndShowAnimationCallback and EndHideAnimationCallback.
```csharp
public class TestWindow : Window
{
    public CanvasGroup canvasGroup;
    
    protected override void StartShowAnimation()
    {
        canvasGroup.alpha = 0.0f;
        StartCoroutine(Appear());
    }
    
    protected override void StartHideAnimation()
    {
        StartCoroutine(Fade());
    }
    
    IEnumerator Fade()
    {
        for (var alpha = 1.0f; alpha >= -0.05f; alpha -= 0.1f)
        {
            canvasGroup.alpha = alpha;
            yield return new WaitForSeconds(0.15f);
        }
        EndHideAnimationCallback();
    }
    
    IEnumerator Appear()
    {
        for (var alpha = 0.0f; alpha <= 1.05f; alpha += 0.1f)
        {
            canvasGroup.alpha = alpha;
            yield return new WaitForSeconds(0.15f);
        }
        EndShowAnimationCallback();
    }
    
    protected override void DeInit()
    {
        base.DeInit();
        StopAllCoroutines();
        canvasGroup.alpha = 1.0f;
    }
}
```
The tricks given are enough to control the UI. Under the hood, the manager can prefetch windows and reuse closed ones. The DestoyOnClose checkmark is necessary for windows that change a lot in the course of life, create a lot of garbage inside them, so when closed it is easier to destroy them and create them again in the future than to write the logic in the DeInit method, which serves to bring the window to prefab so that you can reuse it without creating a new instance of the prefab.

### Reconfigure UI when screen orientation changes
For mobile platforms it is not always possible to create a universal UI for any resolution and aspect of the screen, so sometimes you have to create different prefabs for devices with aspect 21:10 and 4:3, for example.

In addition, sometimes the interface is different for different control methods. For example, for multiplatform games in one interface will be UI to control from the gamepad, and on the other by touchscreen. In general, the same windows may have different prefabs under different conditions, which may change in time (change of orientation, control or screen resolution, for example).

To create special prefabs for such cases, I specify a suffix in the prefab name. Let's say, by default, the game is landscape orientation, and if the player moves to portrait, then prefabs with the suffix "_p" will be loaded. Accordingly, the initialization of WindowsManager extends. 
```csharp
WindowsManager.Init(new WindowsManagerSettings()
{
    RootPath = "Canvas",
    InputBlockPath = "InputBlocker",
    SuffixesWithPredicates = new Dictionary<string, Func<bool>>
    {
        {"_p", WindowPredicates.IsPortrait}
    }
});
OrientationEventManager.Instance.OnOrientationChanged += WindowsManager.RefreshLayout;
```
```csharp
public static class WindowPredicates
{
    public static bool IsPortrait()
    {
        return Screen.width < Screen.height;
    }
}
```
For each suffix the condition is specified under which the suffix will be used.
OrientationEventManager - child of MonoBehaviour, which monitors the change of sides, is used to determine the moment at which it is possible to rebuild UI.

So, when you change orientation the UI should be rebuilt automatically. If UI is in changed state, and these changes must be saved after the rebuilding - the class for data transit is used. For example, we will store the number of rebuilds.
```csharp
public Text rebuildText;
int rebuildNum;

class TestWindow2_WindowTransientData : WindowTransientData
{
    readonly int rebuildNum;

    public TestWindow2_WindowTransientData (Window window) : base(window)
    {
        rebuildNum = (window as TestWindow2).rebuildNum;
    }

    public override void RestoreWindow(Window window)
    {
        base.RestoreWindow(window);
        (window as TestWindow2_WindowTransientData ).rebuildNum = rebuildNum;
    }
}

public override WindowTransientData GetTransientData()
{
    return new TestWindow2_WindowTransientData (this);
}

public override void Init()
{
    base.Init();
    rebuildNum = 0;
    DrawRebuildNum();
}

public override void InitAfterRebuild(WindowTransientData windowTransientData)
{
    base.InitAfterRebuild(windowTransientData);
    text.text = Title;
    ++rebuildNum;
    DrawRebuildNum();
}

void DrawRebuildNum()
{
    rebuildText.text = rebuildNum.ToString();
}
```

There is quite a lot of code for just one number - but almost all this code is a template, besides not all the windows have different variants. Those windows that are universal - are not created anew, so for TestWindow1 this code can be missed.

To track the change of orientation - a subscription to the auxiliary class OrientationManager is used, which should be on stage.

It is precisely because windows can be rebuilt instead of direct links to Windows functions WindowsManager.CreateWindow returns the WindowReference container.
![Reconfigured interface](https://habrastorage.org/webt/pl/xl/lq/plxllqwllmjksckubbfkougjx0a.gif)

### Chains of windows
A Window class contains several events that can be used to implement some commonly used UI patterns of behavior. For example, settings are often made through a chain of nested windows, and when the very last window is closed, the whole chain must close. 

A pattern to create a nested window chain uses a subscription to the window closing event (more precisely, to start the OnStartHide closing animation so that the whole chain starts closing at the same time):
```csharp
public class TestWindow2: Window
{
    public static string Path = "TestWindow2";

    public float Size;
    public Image background;

    public override void Init()
    {
        base.Init();
        background.color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
        background.rectTransform.localScale = Vector3.one * Size;
    }

    public static Action<Window> SetupWindow(float size)
    {
        return window => (window as TestWindow2).Size = size;
    }

    public void OpenOpenClickClick()
    {
        var w = WindowsManager.CreateWindow(Example4Window2.Path, Example4Window2.SetupWindow(Size * 0.9f)).Window;
        w.OnStartHide += window => Close();
    }
}
```
![](https://habrastorage.org/webt/mb/ve/rh/mbverhobvlr4dn1j4f2wlt5uebi.gif)


WindowsManager has a couple more small features, for example, can control the focus of the Back button in Android (analog of the ESC button on the PC). Usually a click means closing the last opened window. But some windows (e.g. minimap) should not react to it. The ProcessBackButton parameter of the Window class serves for this purpose. The event of pressing the button back will get the last window, where this variable is true. 
There are also auxiliary methods for completely blocking UI - they may be required during some synchronization on the network or while playing continuous animations.

I will be glad to receive any comments on the architecture and suggestions for additional WindowsManager features.