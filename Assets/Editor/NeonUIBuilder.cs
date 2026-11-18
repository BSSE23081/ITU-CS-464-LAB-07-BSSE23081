using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Tools > Build Neon UI  : builds the whole 10-screen menu system on one Canvas.
public static class NeonUIBuilder
{
    static Color BG, Cyan, Pink, Gold, Purple, Green, Red, White, Grey, Dim;
    static readonly Vector2 C = new Vector2(.5f, .5f), TL = new Vector2(0, 1), TR = new Vector2(1, 1);
    static Sprite box;
    static ScreenManager sm;
    static Transform canvasT;
    static TMP_FontAsset font;

    [MenuItem("Tools/Build Neon UI")]
    public static void Build()
    {
        InitColors();
        box = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        // optional: create a TMP font asset from Orbitron and save it here to use it automatically
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Orbitron SDF.asset");

        var old = GameObject.Find("NeonCanvas");
        if (old != null) Undo.DestroyObjectImmediate(old);

        var go = new GameObject("NeonCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(go, "Build Neon UI");
        canvasT = go.transform;
        go.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        sm = go.AddComponent<ScreenManager>();

        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            es.AddComponent<StandaloneInputModule>();
#endif
            Undo.RegisterCreatedObjectUndo(es, "Build Neon UI");
        }

        var bg = NewRect("Background", canvasT); Stretch(bg);
        var bgi = bg.gameObject.AddComponent<Image>(); bgi.color = BG; bgi.raycastTarget = false;

        BuildTitle(Panel("Title"));
        BuildMainMenu(Panel("MainMenu"));
        BuildModeSelect(Panel("ModeSelect"));
        BuildLoadout(Panel("Loadout"));
        BuildShop(Panel("Shop"));
        BuildLeaderboard(Panel("Leaderboard"));
        BuildAchievements(Panel("Achievements"));
        BuildSettings(Panel("Settings"));
        BuildPause(Panel("Pause"));
        BuildResults(Panel("Results"));
        BuildRun(Panel("Run"));

        // editor view: only Title visible (set a panel's CanvasGroup alpha to 1 to edit it)
        foreach (var s in sm.screens) { s.group.alpha = s.id == sm.startScreen ? 1 : 0; }

        Selection.activeGameObject = go;
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("Neon UI built. Set Game view to 1080x1920 portrait and press Play.");
    }

    // ---------------------------------------------------------------- screens

    static void BuildTitle(RectTransform p)
    {
        var logo = Text(p, "NEON DASH", 130, Cyan, C, new Vector2(0, 260), new Vector2(1000, 200));
        logo.gameObject.AddComponent<CanvasGroup>();
        var pl = logo.gameObject.AddComponent<Pulse>(); pl.minAlpha = .75f; pl.scaleAmount = .03f; pl.speed = 1.6f;

        Text(p, "SWIPE UP TO JUMP  -  SWIPE DOWN TO SLAM", 34, Grey, C, new Vector2(0, 100), new Vector2(900, 120));

        var tap = Text(p, "TAP TO START", 56, White, C, new Vector2(0, -420), new Vector2(800, 120));
        tap.gameObject.AddComponent<CanvasGroup>();
        var pt = tap.gameObject.AddComponent<Pulse>(); pt.minAlpha = .3f; pt.speed = 3f;

        var area = NewRect("TapArea", p); Stretch(area);
        var ai = area.gameObject.AddComponent<Image>(); ai.color = new Color(0, 0, 0, 0);
        var ab = area.gameObject.AddComponent<Button>(); ab.transition = Selectable.Transition.None;
        Root(ab, "MainMenu");
    }

    static void BuildMainMenu(RectTransform p)
    {
        Text(p, "NEON DASH", 110, Cyan, C, new Vector2(0, 700), new Vector2(1000, 160));
        Text(p, "PLAYER 01", 36, Cyan, C, new Vector2(0, 570), new Vector2(600, 60));
        Go(Btn(p, "PLAY", Cyan, C, new Vector2(0, 420), new Vector2(640, 170), 64), "ModeSelect");
        Text(p, "HIGH 449M   |   COINS 17", 36, Gold, C, new Vector2(0, 270), new Vector2(900, 60));
        Go(Btn(p, "SHOP", Pink, C, new Vector2(0, 100), new Vector2(640, 150)), "Shop");
        Go(Btn(p, "LEADERBOARD", Gold, C, new Vector2(0, -80), new Vector2(640, 150), 40), "Leaderboard");
        Go(Btn(p, "ACHIEVEMENTS", Cyan, C, new Vector2(0, -260), new Vector2(640, 150), 38), "Achievements");
        Go(Btn(p, "SETTINGS", Purple, C, new Vector2(0, -440), new Vector2(640, 150)), "Settings");
    }

    static void BuildModeSelect(RectTransform p)
    {
        Header(p, "SELECT MODE");
        Go(Card(p, "ENDLESS", "Run until you crash", Cyan, new Vector2(0, 380), new Vector2(900, 300)), "Loadout");
        Go(Card(p, "TIME TRIAL", "Beat the clock: 60 seconds", Pink, new Vector2(0, 40), new Vector2(900, 300)), "Loadout");
        Card(p, "ZEN", "LOCKED - reach 500m in one run", Grey, new Vector2(0, -300), new Vector2(900, 300), true);
    }

    static void BuildLoadout(RectTransform p)
    {
        Header(p, "LOADOUT");
        var prev = Box(p, "SkinPreview", Color.Lerp(BG, Cyan, .4f), Cyan, C, new Vector2(0, 520), new Vector2(260, 260), 6);
        Cycle(p, "SKIN", new[] { "NEON CYAN", "GOLD RUSH", "EMERALD" }, Cyan, 230, prev, new[] { Cyan, Gold, Green });
        Cycle(p, "TRAIL", new[] { "NO TRAIL", "CYAN TRAIL", "MAGENTA TRAIL" }, Pink, 30);
        Cycle(p, "ITEM", new[] { "NONE", "REVIVE", "COIN MAGNET" }, Purple, -170);
        Go(Btn(p, "SHOP", Pink, C, new Vector2(0, -380), new Vector2(420, 110), 40), "Shop");
        Go(Btn(p, "START RUN", Cyan, C, new Vector2(0, -560), new Vector2(760, 180), 64), "Run");
    }

    static void BuildShop(RectTransform p)
    {
        Header(p, "SHOP");
        var coin = Box(p, "Coins", Color.Lerp(BG, Gold, .12f), Gold, TR, new Vector2(-60, -105), new Vector2(300, 110), 3);
        var ct = Text(coin.transform, "COINS 17", 38, Gold, C, Vector2.zero, new Vector2(280, 100)); Stretch(ct.rectTransform);

        var tg = p.gameObject.AddComponent<TabGroup>();
        string[] names = { "SKINS", "TRAILS", "ITEMS" };
        tg.tabs = new Button[3]; tg.borders = new Image[3]; tg.labels = new TMP_Text[3];
        tg.indicators = new GameObject[3]; tg.contents = new GameObject[3];
        tg.onColor = Cyan; tg.offColor = Dim; tg.offTextColor = Grey;
        for (int i = 0; i < 3; i++)
        {
            float x = (i - 1) * 310f;
            var b = Btn(p, names[i], Cyan, C, new Vector2(x, 650), new Vector2(290, 110), 38);
            tg.tabs[i] = b; tg.borders[i] = b.GetComponent<Image>(); tg.labels[i] = b.GetComponentInChildren<TMP_Text>();
            var ind = NewRect("Indicator", p); Set(ind, C, new Vector2(x, 585), new Vector2(200, 8));
            var ii = ind.gameObject.AddComponent<Image>(); ii.color = Cyan; ii.raycastTarget = false;
            tg.indicators[i] = ind.gameObject;
            var content = NewRect(names[i] + "Content", p); Stretch(content);
            tg.contents[i] = content.gameObject;
        }
        var skins = tg.contents[0].transform; var trails = tg.contents[1].transform; var items = tg.contents[2].transform;
        ShopRow(skins, "Neon Cyan", "EQUIPPED", "IN USE", Cyan, 380, false);
        ShopRow(skins, "Gold Rush", "200 COINS", "BUY", Gold, 100);
        ShopRow(skins, "Emerald", "300 COINS", "BUY", Green, -180);
        ShopRow(trails, "No Trail", "EQUIPPED", "IN USE", Cyan, 380, false);
        ShopRow(trails, "Cyan Trail", "150 COINS", "BUY", Cyan, 100);
        ShopRow(trails, "Magenta Trail", "250 COINS", "BUY", Pink, -180);
        ShopRow(items, "Free Coins", "Watch an ad to earn 50", "WATCH", Gold, 380);
        ShopRow(items, "Revive (x0)", "100 COINS", "BUY", Pink, 100);
        ShopRow(items, "Coin Magnet (x0)", "120 COINS", "BUY", Purple, -180);
    }

    static void BuildLeaderboard(RectTransform p)
    {
        Header(p, "LEADERBOARD");
        var me = Box(p, "PlayerRow", Color.Lerp(BG, Cyan, .1f), Cyan, C, new Vector2(0, 620), new Vector2(960, 110), 3);
        var mt = Text(me.transform, "PLAYER 01", 40, Cyan, C, new Vector2(-220, 0), new Vector2(500, 90), TextAlignmentOptions.MidlineLeft);
        Text(me.transform, "EDIT", 36, Cyan, C, new Vector2(380, 0), new Vector2(160, 90), TextAlignmentOptions.MidlineRight);
        LbRow(p, "#1", "NOVA", "9999M", "14784", Gold, 470);
        LbRow(p, "#2", "BYTE.EXE", "8120M", "1860", White, 340);
        LbRow(p, "#3", "ZERO_G", "7544M", "22417", White, 210);
        LbRow(p, "#4", "PIXEL", "6310M", "10550", White, 80);
        LbRow(p, "#5", "ECHO_9", "5987M", "1355", White, -50);
        LbRow(p, "#77", "YOU", "449M", "17", Cyan, -230, true);
    }

    static void BuildAchievements(RectTransform p)
    {
        Header(p, "ACHIEVEMENTS");
        Text(p, "COMPLETED", 44, Gold, TL, new Vector2(60, -270), new Vector2(600, 80), TextAlignmentOptions.MidlineLeft);
        AchDone(p, "ROOKIE RUNNER", "10 runs done", "10", -240);
        AchDone(p, "FIRST RUN", "You took your first leap", ">", 240);
        Text(p, "LOCKED", 44, Grey, TL, new Vector2(60, -840), new Vector2(600, 80), TextAlignmentOptions.MidlineLeft);
        AchLocked(p, "HARDCORE RUNNER", "Complete 50 runs", -352);
        AchLocked(p, "LEGENDARY RUNNER", "Complete 100 runs", -117);
        AchLocked(p, "500 METERS", "Reach 500m in one run", 117);
        AchLocked(p, "1KM LIMIT", "Reach 1000m in one run", 352);
    }

    static void BuildSettings(RectTransform p)
    {
        Header(p, "SETTINGS");
        Toggle(p, "SOUND", 500);
        Toggle(p, "MUSIC", 300);
        Toggle(p, "VIBRATION", 100);
        var open = Btn(p, "CREDITS", Gold, C, new Vector2(0, -150), new Vector2(640, 150));

        var pop = NewRect("CreditsPopup", p); Stretch(pop);
        var pi = pop.gameObject.AddComponent<Image>(); pi.color = new Color(0, 0, 0, .9f);
        var card = Box(pop, "Card", BG, Gold, C, Vector2.zero, new Vector2(900, 900), 4);
        Text(card.transform, "CREDITS", 64, Gold, C, new Vector2(0, 330), new Vector2(800, 100));
        Text(card.transform, "NEON DASH\nUI inspired by Neon Runner\nFont: Orbitron (SIL OFL 1.1)\nBuilt in Unity for ITU CS-464 Lab 07", 38, White, C, new Vector2(0, 40), new Vector2(800, 500));
        var close = Btn(card.transform, "CLOSE", Cyan, C, new Vector2(0, -340), new Vector2(400, 130));
        UnityEventTools.AddBoolPersistentListener(open.onClick, pop.gameObject.SetActive, true);
        UnityEventTools.AddBoolPersistentListener(close.onClick, pop.gameObject.SetActive, false);
        pop.gameObject.SetActive(false);
    }

    static void BuildPause(RectTransform p)
    {
        Text(p, "PAUSED", 110, Cyan, C, new Vector2(0, 500), new Vector2(900, 160));
        Back(Btn(p, "RESUME", Cyan, C, new Vector2(0, 200), new Vector2(640, 150), 52));
        Root(Btn(p, "RESTART", Gold, C, new Vector2(0, 0), new Vector2(640, 150), 52), "Run");
        Go(Btn(p, "SETTINGS", Purple, C, new Vector2(0, -200), new Vector2(640, 150), 52), "Settings");
        Root(Btn(p, "MAIN MENU", Pink, C, new Vector2(0, -400), new Vector2(640, 150), 48), "MainMenu");
    }

    static void BuildResults(RectTransform p)
    {
        Text(p, "RUN OVER", 120, Red, C, new Vector2(0, 600), new Vector2(1000, 180));
        Stat(p, "DISTANCE", "449 M", Cyan, 380);
        Stat(p, "COINS", "16", Gold, 280);
        Stat(p, "BEST", "449 M", Pink, 180);
        Root(Btn(p, "RETRY", Cyan, C, new Vector2(0, -20), new Vector2(700, 160), 60), "Run");
        Root(Btn(p, "WATCH AD: CONTINUE", Gold, C, new Vector2(0, -210), new Vector2(700, 150), 36), "Run");
        Root(Btn(p, "MAIN MENU", Pink, C, new Vector2(0, -400), new Vector2(700, 150), 48), "MainMenu");
    }

    // placeholder so you can reach Pause and Results (not one of the 10 screens)
    static void BuildRun(RectTransform p)
    {
        Text(p, "RUN IN PROGRESS", 70, White, C, new Vector2(0, 300), new Vector2(900, 120));
        Text(p, "(gameplay placeholder)", 34, Grey, C, new Vector2(0, 200), new Vector2(900, 80));
        Go(Btn(p, "II", Cyan, TR, new Vector2(-60, -90), new Vector2(140, 140), 56), "Pause");
        Go(Btn(p, "DIE (TEST)", Red, C, new Vector2(0, -100), new Vector2(640, 150), 48), "Results");
    }

    // ---------------------------------------------------------------- widgets

    static void Header(Transform p, string title)
    {
        Back(Btn(p, "<", Cyan, TL, new Vector2(60, -90), new Vector2(140, 140), 64));
        Text(p, title, 60, Cyan, TL, new Vector2(240, -90), new Vector2(620, 140), TextAlignmentOptions.MidlineLeft);
    }

    static Button Btn(Transform p, string label, Color accent, Vector2 anchor, Vector2 pos, Vector2 size, int fs = 44)
    {
        var img = Box(p, "Btn_" + label.Replace(" ", ""), Color.Lerp(BG, accent, .18f), accent, anchor, pos, size);
        var b = img.gameObject.AddComponent<Button>();
        b.targetGraphic = img; b.transition = Selectable.Transition.None;
        img.gameObject.AddComponent<ButtonFeedback>();
        var t = Text(img.transform, label, fs, accent, C, Vector2.zero, size);
        Stretch(t.rectTransform);
        return b;
    }

    static Button Card(Transform p, string title, string desc, Color accent, Vector2 pos, Vector2 size, bool locked = false)
    {
        var img = Box(p, "Card_" + title.Replace(" ", ""), Color.Lerp(BG, accent, locked ? .05f : .15f), accent, C, pos, size);
        var b = img.gameObject.AddComponent<Button>();
        b.targetGraphic = img; b.transition = Selectable.Transition.None;
        if (!locked) img.gameObject.AddComponent<ButtonFeedback>();
        Text(img.transform, title, 56, accent, C, new Vector2(0, 45), new Vector2(size.x - 40, 80));
        Text(img.transform, desc, 30, locked ? Grey : White, C, new Vector2(0, -45), new Vector2(size.x - 40, 80));
        b.interactable = !locked;
        return b;
    }

    static void Cycle(Transform p, string title, string[] opts, Color accent, float y, Image tint = null, Color[] tints = null)
    {
        var img = Box(p, "Cycle_" + title, Color.Lerp(BG, accent, .12f), accent, C, new Vector2(0, y), new Vector2(900, 170));
        var b = img.gameObject.AddComponent<Button>();
        b.targetGraphic = img; b.transition = Selectable.Transition.None;
        img.gameObject.AddComponent<ButtonFeedback>();
        Text(img.transform, title + "  (TAP TO CHANGE)", 28, Grey, C, new Vector2(0, 48), new Vector2(800, 50));
        var v = Text(img.transform, opts[0], 54, accent, C, new Vector2(0, -20), new Vector2(800, 90));
        var c = img.gameObject.AddComponent<CycleOption>();
        c.button = b; c.value = v; c.options = opts; c.tint = tint; c.tints = tints;
    }

    static Button ShopRow(Transform p, string title, string sub, string btn, Color accent, float y, bool active = true)
    {
        var row = Box(p, "Row_" + title.Replace(" ", ""), Color.Lerp(BG, accent, .1f), accent, C, new Vector2(0, y), new Vector2(960, 230), 3);
        var icon = Box(row.transform, "Icon", Color.Lerp(BG, accent, .5f), accent, C, new Vector2(-380, 0), new Vector2(140, 140));
        var it = Text(icon.transform, title.Substring(0, 1), 64, accent, C, Vector2.zero, new Vector2(140, 140)); Stretch(it.rectTransform);
        Text(row.transform, title, 44, White, C, new Vector2(-60, 35), new Vector2(520, 70), TextAlignmentOptions.MidlineLeft);
        Text(row.transform, sub, 30, Grey, C, new Vector2(-60, -35), new Vector2(520, 60), TextAlignmentOptions.MidlineLeft);
        var b = Btn(row.transform, btn, active ? accent : Grey, C, new Vector2(340, 0), new Vector2(220, 100), 36);
        b.interactable = active;
        return b;
    }

    static void LbRow(Transform p, string rank, string name, string dist, string coins, Color accent, float y, bool you = false)
    {
        var row = Box(p, "LbRow_" + rank, Color.Lerp(BG, accent, you ? .15f : .06f), you ? Cyan : Dim, C, new Vector2(0, y), new Vector2(960, 110), 3);
        var t = row.transform;
        Text(t, rank, 40, accent, C, new Vector2(-390, 0), new Vector2(140, 90), TextAlignmentOptions.MidlineLeft);
        Text(t, name, 40, White, C, new Vector2(-150, 0), new Vector2(380, 90), TextAlignmentOptions.MidlineLeft);
        Text(t, dist, 40, Cyan, C, new Vector2(180, 0), new Vector2(260, 90), TextAlignmentOptions.MidlineRight);
        Text(t, coins, 36, Gold, C, new Vector2(395, 0), new Vector2(170, 90), TextAlignmentOptions.MidlineRight);
    }

    static void AchDone(Transform p, string title, string desc, string icon, float x)
    {
        var card = Box(p, "Ach_" + title.Replace(" ", ""), Color.Lerp(BG, Cyan, .12f), Cyan, C, new Vector2(x, 330), new Vector2(460, 420), 4);
        var ic = Box(card.transform, "Icon", Color.Lerp(BG, Cyan, .45f), Cyan, C, new Vector2(0, 90), new Vector2(170, 170));
        var it = Text(ic.transform, icon, 72, White, C, Vector2.zero, new Vector2(170, 170)); Stretch(it.rectTransform);
        Text(card.transform, title, 38, White, C, new Vector2(0, -60), new Vector2(420, 60));
        Text(card.transform, desc, 28, Grey, C, new Vector2(0, -125), new Vector2(420, 70));
    }

    static void AchLocked(Transform p, string title, string desc, float x)
    {
        var card = Box(p, "Locked_" + title.Replace(" ", ""), Color.Lerp(BG, Grey, .06f), Dim, C, new Vector2(x, -150), new Vector2(220, 380), 3);
        var ic = Box(card.transform, "Icon", BG, Dim, C, new Vector2(0, 85), new Vector2(110, 110));
        var it = Text(ic.transform, "?", 56, Grey, C, Vector2.zero, new Vector2(110, 110)); Stretch(it.rectTransform);
        Text(card.transform, title, 26, White, C, new Vector2(0, -25), new Vector2(200, 90));
        Text(card.transform, desc, 22, Grey, C, new Vector2(0, -120), new Vector2(200, 80));
    }

    static void Toggle(Transform p, string label, float y)
    {
        Text(p, label, 48, White, C, new Vector2(-250, y), new Vector2(450, 100), TextAlignmentOptions.MidlineLeft);
        var st = Text(p, "ON", 40, Cyan, C, new Vector2(100, y), new Vector2(120, 100), TextAlignmentOptions.MidlineRight);
        var track = Box(p, "Toggle_" + label, Color.Lerp(BG, Cyan, .15f), Cyan, C, new Vector2(300, y), new Vector2(200, 100));
        var b = track.gameObject.AddComponent<Button>();
        b.targetGraphic = track; b.transition = Selectable.Transition.None;
        var knob = NewRect("Knob", track.transform); Set(knob, C, new Vector2(50, 0), new Vector2(70, 70));
        var ki = knob.gameObject.AddComponent<Image>();
        ki.sprite = box; ki.type = Image.Type.Sliced; ki.color = White; ki.raycastTarget = false;
        var ts = track.gameObject.AddComponent<ToggleSwitch>();
        ts.button = b; ts.knob = knob; ts.track = track; ts.label = st;
        ts.travel = 50; ts.onColor = Cyan; ts.offColor = Grey;
    }

    static void Stat(Transform p, string label, string val, Color c, float y)
    {
        Text(p, label, 36, Grey, C, new Vector2(-210, y), new Vector2(380, 80), TextAlignmentOptions.MidlineRight);
        Text(p, val, 56, c, C, new Vector2(240, y), new Vector2(420, 80), TextAlignmentOptions.MidlineLeft);
    }

    // ---------------------------------------------------------------- low-level helpers

    static RectTransform Panel(string id)
    {
        var r = NewRect(id, canvasT); Stretch(r);
        var cg = r.gameObject.AddComponent<CanvasGroup>();
        sm.screens.Add(new ScreenManager.ScreenEntry { id = id, group = cg });
        return r;
    }

    static void Go(Button b, string id) { UnityEventTools.AddStringPersistentListener(b.onClick, sm.Go, id); }
    static void Root(Button b, string id) { UnityEventTools.AddStringPersistentListener(b.onClick, sm.GoRoot, id); }
    static void Back(Button b) { UnityEventTools.AddVoidPersistentListener(b.onClick, sm.Back); }

    static RectTransform NewRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    static void Set(RectTransform r, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        r.anchorMin = anchor; r.anchorMax = anchor; r.pivot = anchor;
        r.anchoredPosition = pos; r.sizeDelta = size;
    }

    static void Stretch(RectTransform r)
    {
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
    }

    static TextMeshProUGUI Text(Transform p, string s, int size, Color col, Vector2 anchor, Vector2 pos, Vector2 sz,
                                TextAlignmentOptions al = TextAlignmentOptions.Center)
    {
        var r = NewRect("Text", p); Set(r, anchor, pos, sz);
        var t = r.gameObject.AddComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.text = s; t.fontSize = size; t.color = col; t.alignment = al; t.raycastTarget = false;
        return t;
    }

    // bordered rounded box: outer image = border colour, inner image = fill
    static Image Box(Transform p, string name, Color fill, Color border, Vector2 anchor, Vector2 pos, Vector2 size, float bw = 4f)
    {
        var outer = NewRect(name, p); Set(outer, anchor, pos, size);
        var oi = outer.gameObject.AddComponent<Image>();
        oi.sprite = box; oi.type = Image.Type.Sliced; oi.color = border;
        var inner = NewRect("Fill", outer); Stretch(inner);
        inner.offsetMin = new Vector2(bw, bw); inner.offsetMax = new Vector2(-bw, -bw);
        var ii = inner.gameObject.AddComponent<Image>();
        ii.sprite = box; ii.type = Image.Type.Sliced; ii.color = fill; ii.raycastTarget = false;
        return oi;
    }

    static void InitColors()
    {
        BG = Hex("05050F"); Cyan = Hex("19E6FF"); Pink = Hex("FF2D8A"); Gold = Hex("FFC933");
        Purple = Hex("9B5CFF"); Green = Hex("2EE89A"); Red = Hex("FF3358"); White = Hex("EDEDF7");
        Grey = Hex("9A9AB0"); Dim = Hex("3A3A52");
    }

    static Color Hex(string h) { Color c; ColorUtility.TryParseHtmlString("#" + h, out c); return c; }
}
