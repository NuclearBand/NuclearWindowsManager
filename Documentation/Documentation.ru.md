### Введение

В любой, даже самой простой игре на юнити, присутствует UI. В него входят главное меню, различные вспомогательные окна с настройками, игровой интерфейс и дополнительные окна игрового интерфейса (инвентарь, окно персонажа, магазин и так далее).

Думаю, у многих разработчиков есть свой менеджер окон, который позволяет удобно открывать и закрывать их. Мой оконный менеджер обладает дополнительным функционалом - умеет перестраивать окна при смене ориентации экрана (при наличии альтернативных окон для разных ориентаций), обрабатывает нажатие кнопки Back (для андроида), умеет блокировать ввод у неактивных окон и ещё пара фич.

В этой статье я хотел бы поделиться рабочим процессом разработки UI с помощью своей библиотеки на примере разработки несложного интерфейса.
<cut />

### Создание окон
Будем создавать интерфейс из двух окон.

![Макет интерфейса](https://habrastorage.org/webt/ko/3q/pv/ko3qpvkiynygnistugenttehfp8.gif)

Каждое окно - это отдельный префаб. С версии 2018.3 окна стало удобнее редактировать, поскольку можно в настройках задать UI Environment - канвас для окон, поэтому больше не требуется переключаться на отдельную сцену для прототипирования UI.


![Настройки UIEnvironment](https://habrastorage.org/webt/ep/xg/ni/epxgnipwsewhdt92dypqf47nv6m.png)
![TestWindow1](https://habrastorage.org/webt/2k/md/c_/2kmdc_m4npym1a--vgwvommeukw.png)
![TestWindow2](https://habrastorage.org/webt/gk/ia/4h/gkia4hd9hvp196h5p4d-q1htjwq.png)

Все префабы окон я храню в папке Resources, чтобы их можно было инстанциировать по относительному пути.
Окна имеют базовый класс Window, который является MonoBehaviour.

В каждой игры я создаю “базового наследника”, и потом уже конкретные окна наследую от него. Это полезно, поскольку в каждой игре все окна часто имеют нечто общее - например анимации открытия/закрытия, или какие-то дополнительные фичи, используемые только в этой игре. Адрес окна удобно хранить в статической переменной самого класса, либо иметь отдельный файл с путями ко всем префабам окон.
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
Ещё в ресурсах я храню префаб канваса, а также префаб InputBlocker. Это ui элемент, который вставляется между окнами когда необходимо блокировать ввод в одно окно пока активно другое. В данном случае нам нужно, чтобы кнопки других окон не реагировали на нажатия когда активно окно Window2. Поэтому в его настройках нужно поставить галочку на WithInputBlockForBackground. 

![InputBlocker](https://habrastorage.org/webt/bi/vt/ua/bivtuaskifpm6epeiebq_znu4fo.png)

Внешний вид окон готов, поэтому можно перейти к написанию логики.

### Открытие/закрытие окон

Мой менеджер представляет собой статический класс WindowsManager. При загрузке игры его необходимо инициализировать - дать ему адрес рутового канваса. Дополнительно можно указать адрес InputBlocker. В этом интерфейсе он используется, поэтому он необходим. Для инициализации поместим в сцену GameObject со скриптом Starter.
```csharp
WindowsManager.Init(new WindowsManagerSettings()
{
    RootPath = "Canvas",
    InputBlockPath = "InputBlocker"
});
```

После инициализации можно приступать к созданию окон.
```csharp
var windowReference = WindowsManager.CreateWindow(TestWindow1.Path);
```

Дополнительный параметр этого метода является функцией, с помощью которой окно получает данные. Используем его, чтобы при создании TestWindow2 передать ему заголовок.

```csharp
WindowsManager.CreateWindow(
    TestWindow2.Path,
    window => (window as TestWindow2).Title = inputField.text
    );
```

Используем переданные данные в методе Init, который вызывается при каждом создании окна.

```csharp
public string Title;
public Text text;
public override void Init()
{
    base.Init();
    text.text = Title;
}
```
Теперь нам необходимо добавить анимации. Для этого переопределим методы StartShowAnimation и StartHideAnimation. Чтобы менеджер понял, что анимация завершилась и окно открылось/закрылось по окончанию анимаций необходимо вызвать EndShowAnimationCallback и EndHideAnimationCallback.
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

Приведённых приёмов вполне хватает, чтобы управлять UI. Под капотом менеджер умеет префетчить окна и переиспользовать закрытые. Галка DestoyOnClose необходима для окон, которые в процессе жизни сильно меняются, создают внутри себя много мусора, поэтому при закрытии их проще уничтожить и создать в будущем заново, чем прописывать логику в метод DeInit, служащий для приведения окна в префабное состояние, чтоб можно было его переиспользовать, не создавая новый экземпляр префаба.

### Перестраивание UI при смене ориентации экрана
Для мобильных платформ не всегда получается создать универсальный UI под любое разрешение и аспект экрана, поэтому иногда приходится создавать разные префабы под устройства с аспектом 21:10 и 4:3, например.

Кроме того, иногда интерфейс отличается для разных методов управления. Скажем, для мультиплатформенный игры в одном интерфейсе будет UI Для управления с геймпада, а на другом с помощью тачскрина. Вообщем, у одних и тех же окон могут быть разные префабы при различных условиях, которые могут меняться в рантайме (смена ориентации, управления или разрешения экрана, например).

Для создания специальных префабов под такие случаи я указываю в названии префаба суффикс. Скажем, по умолчанию, в игре ландшафтная ориентация, а если игрок перейдёт в портретную, то будут грузиться префабы с суффиксом “_p”. Соответственно инициализация WindowsManager расширяется. 
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
</spoiler>
<spoiler title="Код определения портретной ориентации">
public static class WindowPredicates
{
    public static bool IsPortrait()
    {
        return Screen.width < Screen.height;
    }
}
```
Для каждого суффикса указывается условие, при котором этот суффикс будет использоваться.
Для определения момента, в который возможно необходимо перестраивать UI, используется OrientationEventManager - наследник MonoBehaviour, который следит за изменением сторон.

Итак, при смене ориентации UI должен перестраиваться автоматически. Если UI находится в изменённом состоянии, и эти изменения необходимо сохранить после перестройки - используется класс для транзита данных. Для примера будем хранить число перестраиваний.
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

С виду довольно много кода для всего одного числа- но почти весь этот код является шаблоном, к тому же далеко не все окна имеют различные варианты. Те окна, которые универсальные - не создаются заново, поэтому для TestWindow1 этот код можно упустить.

Чтобы отследить смену ориентации - используется подписка на вспомогательный класс OrientationManager, который должен быть на сцене.

Именно из-за того, что окна могут перестраиваться вместо прямых ссылок на Window функции WindowsManager.CreateWindow возвращает контейнер WindowReference.
![Интерфейс с перестраиванием](https://habrastorage.org/webt/pl/xl/lq/plxllqwllmjksckubbfkougjx0a.gif)

### Цепочки окон
Класс Window содержит несколько ивентов, с помощью которых можно реализовывать некоторые часто используемые паттерны поведения UI. Например, часто настройки делают через цепочку вложенных окошек, и при закрытии самого последнего окна должна закрываться вся цепочка. 
Паттерн для создания связки таких окон использует подписку на событие закрытия окна (точнее начала анимации закрытия OnStartHide, чтобы вся цепочка начала закрываться одновременно):
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


WindowsManager обладает ещё парой маленьких фитч, например, умеет управлять фокусом кнопки Back в андроиде (аналог кнопки ESC на PC). Обычно нажатие подразумевает закрытие последнего открытого окна. Но некоторые окна (например мини-карта), не должны на неё реагировать. Для этого служит параметр ProcessBackButton у класса Window. Событие нажатия на кнопку назад получит последнее окно, у которого эта переменная равна true. 
Есть также вспомогательные методы полной блокировки UI - они могут потребоваться во время какой-нибудь синхронизации по сети, либо проигрывании непрерываемых анимаций.

Буду рад любым замечаниям по архитектуре и предложениям дополнительных фич WindowsManager'а.
