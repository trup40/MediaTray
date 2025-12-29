using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace MediaTrayMaster
{
    // ---------------------------------------------------------
    // 1. GİRİŞ NOKTASI
    // ---------------------------------------------------------
    static class Program
    {
        private static readonly string _appGuid = "Global\\MediaTrayMaster_Unique_Mutex_ID";

        [STAThread]
        static void Main()
        {
            SettingsManager manager = new SettingsManager();
            manager.Load();
            Loc.CurrentLang = manager.CurrentSettings.Language;

            using (Mutex mutex = new Mutex(false, _appGuid))
            {
                if (!mutex.WaitOne(0, false))
                {
                    MessageBox.Show(Loc.Tr("RunningMsg"), Loc.Tr("RunningTitle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MediaTrayApp());
            }
        }
    }

    // ---------------------------------------------------------
    // 2. VERİ MODELLERİ VE LOKALİZASYON
    // ---------------------------------------------------------
    public class AppSettings
    {
        public string Language { get; set; } = GetSystemLanguage();
        public string ThemeMode { get; set; } = "auto";
        public bool IsSingleTrayMode { get; set; } = false;
        public string DoubleClickAction { get; set; } = "next";
        public bool EnableCustomIcons { get; set; } = false;
        public Dictionary<string, string> CustomIcons { get; set; } = new Dictionary<string, string>();

        private static string GetSystemLanguage()
        {
            var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLower();
            return culture == "tr" ? "tr" : "en";
        }
    }

    public static class Loc
    {
        public static string CurrentLang = "tr";

        public static string Tr(string key)
        {
            return _texts.ContainsKey(CurrentLang) && _texts[CurrentLang].ContainsKey(key)
                ? _texts[CurrentLang][key]
                : key;
        }

        private static readonly Dictionary<string, Dictionary<string, string>> _texts = new Dictionary<string, Dictionary<string, string>>
        {
            ["tr"] = new Dictionary<string, string> {
                {"Settings", "Ayarlar"},
                {"About", "Hakkında"},
                {"Exit", "Çıkış"},
                {"Prev", "Önceki"},
                {"Next", "Sonraki"},
                {"PlayPause", "Oynat/Duraklat"},
                {"Stop", "Durdur"},
                {"Lang", "Dil"},
                {"Appearance", "Görünüm Modu"},
                {"TrayMode", "Tepsi Modu"},
                {"MultiTray", "Çoklu Simgeler (4 Adet)"},
                {"SingleTray", "Tek Simge"},
                {"Auto", "Otomatik"},
                {"Dark", "Koyu Mod"},
                {"Light", "Açık Mod"},
                {"Current", "Şu an"},
                {"CustomIcons", "Özel Simgeler"},
                {"Reset", "Sıfırla"},
                {"Save", "KAYDET"},
                {"Close", "KAPAT"},
                {"LightShort", "Açık"},
                {"DarkShort", "Koyu"},
                {"AboutTitle", "Hakkında"},
                {"AppTitle", "Media Tray Master"},
                {"AppSub", "Profesyonel Oynatma Kontrol Aracı v1.0.0"}, // GÜNCELLENDİ
                {"AboutDesc", "Bu yazılım, Windows iş akışınızı bölmeden\narka planda medya kontrolü sağlamak için\ngeliştirilmiştir."},
                {"Features", "Hızlı Erişim • Özelleştirilebilir • Hibrit Kontrol"},
                {"DevLabel", "Geliştirici: Eagle"}, // GÜNCELLENDİ
                {"ContactLabel", "İletişim: trup40@protonmail.com"}, // GÜNCELLENDİ
                {"ExitConfirm", "Uygulamayı kapatmak istiyor musunuz?"},
                {"SelectIcon", "Simge Seç"},
                {"MasterTooltip", "Medya Kontrolcüsü"},
                {"DblClickAction", "Çift Tık"},
                {"ActionNext", "Sonraki"},
                {"ActionPrev", "Önceki"},
                {"ActionStop", "Durdur"},
                {"ActionPlay", "Oynat/Duraklat"},
                {"Copyright", "Her Hakkı Mahfuzdur\n© 2025"}, // GÜNCELLENDİ
                {"RunningTitle", "Bilgi"},
                {"RunningMsg", "Media Tray Master zaten çalışıyor! Sistem tepsisini kontrol edin."}
            },
            ["en"] = new Dictionary<string, string> {
                {"Settings", "Settings"},
                {"About", "About"},
                {"Exit", "Exit"},
                {"Prev", "Previous"},
                {"Next", "Next"},
                {"PlayPause", "Play/Pause"},
                {"Stop", "Stop"},
                {"Lang", "Language"},
                {"Appearance", "Appearance Mode"},
                {"TrayMode", "Tray Mode"},
                {"MultiTray", "Multi Icons (4 items)"},
                {"SingleTray", "Single Icon"},
                {"Auto", "Automatic"},
                {"Dark", "Dark Mode"},
                {"Light", "Light Mode"},
                {"Current", "Current"},
                {"CustomIcons", "Custom Icons"},
                {"Reset", "Reset"},
                {"Save", "SAVE"},
                {"Close", "CLOSE"},
                {"LightShort", "Light"},
                {"DarkShort", "Dark"},
                {"AboutTitle", "About"},
                {"AppTitle", "Media Tray Master"},
                {"AppSub", "Professional Play Controlling Tool v1.0.0"},
                {"AboutDesc", "This software is developed to provide\nseamless background media control without\ninterrupting your workflow."},
                {"Features", "Fast Access • Customizable • Hybrid Control"},
                {"DevLabel", "Developer: Eagle"},
                {"ContactLabel", "Contact: trup40@protonmail.com"},
                {"ExitConfirm", "Do you want to exit the application?"},
                {"SelectIcon", "Select Icon"},
                {"MasterTooltip", "Media Controller"},
                {"DblClickAction", "Double Click"},
                {"ActionNext", "Next Track"},
                {"ActionPrev", "Previous Track"},
                {"ActionStop", "Stop"},
                {"ActionPlay", "Play/Pause"},
                {"Copyright", "All Rights Reserved\n© 2025"},
                {"RunningTitle", "Information"},
                {"RunningMsg", "Media Tray Master is already running! Check the system tray."}
            }
        };
    }

    // ---------------------------------------------------------
    // 3. TEMA MOTORU
    // ---------------------------------------------------------
    public static class ThemeHelper
    {
        public static Color DarkBack = Color.FromArgb(32, 32, 32);
        public static Color DarkPanel = Color.FromArgb(45, 45, 48);
        public static Color DarkText = Color.FromArgb(240, 240, 240);
        public static Color DarkControl = Color.FromArgb(60, 60, 60);

        public static Color LightBack = Color.WhiteSmoke;
        public static Color LightPanel = Color.White;
        public static Color LightText = Color.Black;
        public static Color LightControl = Color.FromArgb(225, 225, 225);

        public static void ApplyTheme(Form form, bool isDark)
        {
            form.BackColor = isDark ? DarkBack : LightBack;
            form.ForeColor = isDark ? DarkText : LightText;
            ApplyRecursive(form, isDark);
            form.Invalidate();
        }

        private static void ApplyRecursive(Control parent, bool isDark)
        {
            Color back = isDark ? DarkBack : LightBack;
            Color fore = isDark ? DarkText : LightText;
            Color ctrlColor = isDark ? DarkControl : LightControl;

            foreach (Control c in parent.Controls)
            {
                if (c is Panel) { c.BackColor = back; c.ForeColor = fore; }
                else if (c is Button btn)
                {
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 1;
                    btn.FlatAppearance.BorderColor = isDark ? Color.Gray : Color.DarkGray;
                    btn.BackColor = ctrlColor;
                    btn.ForeColor = fore;
                }
                else if (c is ComboBox cb) { cb.FlatStyle = FlatStyle.Flat; cb.BackColor = ctrlColor; cb.ForeColor = fore; }
                else if (c is RadioButton || c is CheckBox || c is Label) { c.ForeColor = fore; c.BackColor = Color.Transparent; }
                else if (c is PictureBox pb) { pb.BackColor = Color.Transparent; }

                if (c.HasChildren) ApplyRecursive(c, isDark);
            }
        }
    }

    // ---------------------------------------------------------
    // 4. UYGULAMA MANTIĞI
    // ---------------------------------------------------------
    public class MediaTrayApp : ApplicationContext
    {
        private readonly List<NotifyIcon> _trayIcons = new List<NotifyIcon>();
        private readonly SettingsManager _settingsManager;
        private readonly UserPreferenceChangedEventHandler _preferenceChangedHandler;
        private System.Windows.Forms.Timer _clickTimer;

        public MediaTrayApp()
        {
            _settingsManager = new SettingsManager();
            _settingsManager.Load();
            Loc.CurrentLang = _settingsManager.CurrentSettings.Language;

            _clickTimer = new System.Windows.Forms.Timer();
            _clickTimer.Interval = SystemInformation.DoubleClickTime;
            _clickTimer.Tick += OnClickTimerTick;

            _preferenceChangedHandler = new UserPreferenceChangedEventHandler(OnThemeChanged);
            SystemEvents.UserPreferenceChanged += _preferenceChangedHandler;

            InitializeIcons();
        }

        private void InitializeIcons()
        {
            foreach (var icon in _trayIcons) { icon.Visible = false; icon.Dispose(); }
            _trayIcons.Clear();

            if (_settingsManager.CurrentSettings.IsSingleTrayMode)
                CreateMasterIcon();
            else
            {
                CreateTrayIcon("previous", "Prev", NativeMethods.VK_MEDIA_PREV_TRACK, "Prev");
                CreateTrayIcon("next", "Next", NativeMethods.VK_MEDIA_NEXT_TRACK, "Next");
                CreateTrayIcon("play", "PlayPause", NativeMethods.VK_MEDIA_PLAY_PAUSE, "PlayPause");
                CreateTrayIcon("stop", "Stop", NativeMethods.VK_MEDIA_STOP, "Stop");
            }
        }

        private void CreateTrayIcon(string key, string tooltipKey, byte mediaKey, string menuTitleKey)
        {
            NotifyIcon ni = new NotifyIcon();
            ni.Text = Loc.Tr(tooltipKey);
            ni.Visible = true;
            ni.Tag = key;
            UpdateSingleIcon(ni);
            ni.ContextMenuStrip = CreateMenu(key, mediaKey, menuTitleKey);
            ni.MouseClick += (s, e) => { if (e.Button == MouseButtons.Left) SendKey(mediaKey); };
            _trayIcons.Add(ni);
        }

        private void CreateMasterIcon()
        {
            NotifyIcon ni = new NotifyIcon();
            ni.Text = Loc.Tr("MasterTooltip");
            ni.Visible = true;
            ni.Tag = "play";
            UpdateSingleIcon(ni);

            ContextMenuStrip menu = new ContextMenuStrip();
            bool enableCustom = _settingsManager.CurrentSettings.EnableCustomIcons;

            var itemPlay = new ToolStripMenuItem(Loc.Tr("PlayPause"), null, (s, e) => SendKey(NativeMethods.VK_MEDIA_PLAY_PAUSE));
            itemPlay.Font = new Font(itemPlay.Font, FontStyle.Bold);
            itemPlay.Image = IconRenderer.CreateBitmap("play", false, _settingsManager.CurrentSettings.CustomIcons, enableCustom, 16);
            menu.Items.Add(itemPlay);

            var itemNext = new ToolStripMenuItem(Loc.Tr("Next"), null, (s, e) => SendKey(NativeMethods.VK_MEDIA_NEXT_TRACK));
            itemNext.Image = IconRenderer.CreateBitmap("next", false, _settingsManager.CurrentSettings.CustomIcons, enableCustom, 16);
            menu.Items.Add(itemNext);

            var itemPrev = new ToolStripMenuItem(Loc.Tr("Prev"), null, (s, e) => SendKey(NativeMethods.VK_MEDIA_PREV_TRACK));
            itemPrev.Image = IconRenderer.CreateBitmap("previous", false, _settingsManager.CurrentSettings.CustomIcons, enableCustom, 16);
            menu.Items.Add(itemPrev);

            var itemStop = new ToolStripMenuItem(Loc.Tr("Stop"), null, (s, e) => SendKey(NativeMethods.VK_MEDIA_STOP));
            itemStop.Image = IconRenderer.CreateBitmap("stop", false, _settingsManager.CurrentSettings.CustomIcons, enableCustom, 16);
            menu.Items.Add(itemStop);

            menu.Items.Add(new ToolStripSeparator());
            AddStandardMenuBottom(menu);
            ni.ContextMenuStrip = menu;

            ni.MouseClick += (s, e) => { if (e.Button == MouseButtons.Left) _clickTimer.Start(); };
            ni.DoubleClick += (s, e) => { _clickTimer.Stop(); PerformDoubleClickAction(); };

            _trayIcons.Add(ni);
        }

        private void OnClickTimerTick(object sender, EventArgs e)
        {
            _clickTimer.Stop();
            SendKey(NativeMethods.VK_MEDIA_PLAY_PAUSE);
        }

        private void PerformDoubleClickAction()
        {
            string action = _settingsManager.CurrentSettings.DoubleClickAction;
            byte key = NativeMethods.VK_MEDIA_NEXT_TRACK;
            switch (action)
            {
                case "prev": key = NativeMethods.VK_MEDIA_PREV_TRACK; break;
                case "stop": key = NativeMethods.VK_MEDIA_STOP; break;
                case "play": key = NativeMethods.VK_MEDIA_PLAY_PAUSE; break;
                default: key = NativeMethods.VK_MEDIA_NEXT_TRACK; break;
            }
            SendKey(key);
        }

        private ContextMenuStrip CreateMenu(string iconKey, byte mediaKey, string titleKey)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            var actionItem = new ToolStripMenuItem(Loc.Tr(titleKey), null, (s, e) => SendKey(mediaKey));
            actionItem.Font = new Font(actionItem.Font, FontStyle.Bold);
            actionItem.Image = IconRenderer.CreateBitmap(iconKey, false, _settingsManager.CurrentSettings.CustomIcons, _settingsManager.CurrentSettings.EnableCustomIcons, 16);
            menu.Items.Add(actionItem);
            menu.Items.Add(new ToolStripSeparator());
            AddStandardMenuBottom(menu);
            return menu;
        }

        private void AddStandardMenuBottom(ContextMenuStrip menu)
        {
            var settingsItem = new ToolStripMenuItem(Loc.Tr("Settings"), null, (s, e) => ShowSettings());
            settingsItem.Image = MenuIconRenderer.DrawGear();
            menu.Items.Add(settingsItem);

            menu.Items.Add(new ToolStripSeparator());

            var aboutItem = new ToolStripMenuItem(Loc.Tr("About"), null, (s, e) => ShowAbout());
            aboutItem.Image = MenuIconRenderer.DrawInfo();
            menu.Items.Add(aboutItem);

            menu.Items.Add(new ToolStripSeparator());

            var exitItem = new ToolStripMenuItem(Loc.Tr("Exit"), null, (s, e) => ExitApp());
            exitItem.Image = MenuIconRenderer.DrawExit();
            menu.Items.Add(exitItem);
        }

        private void UpdateSingleIcon(NotifyIcon ni)
        {
            string key = ni.Tag as string;
            if (string.IsNullOrEmpty(key)) return;
            if (ni.Icon != null) { var old = ni.Icon; ni.Icon = null; old.Dispose(); }

            bool isDark = _settingsManager.IsDarkMode();
            ni.Icon = IconRenderer.CreateIcon(key, isDark, _settingsManager.CurrentSettings.CustomIcons, _settingsManager.CurrentSettings.EnableCustomIcons);
        }

        private void UpdateAllIcons()
        {
            foreach (var icon in _trayIcons) UpdateSingleIcon(icon);
        }

        private void OnThemeChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General && _settingsManager.CurrentSettings.ThemeMode == "auto")
                UpdateAllIcons();
        }

        private void SendKey(byte key)
        {
            NativeMethods.keybd_event(key, 0, NativeMethods.KEYEVENTF_EXTENDEDKEY, 0);
            NativeMethods.keybd_event(key, 0, NativeMethods.KEYEVENTF_EXTENDEDKEY | NativeMethods.KEYEVENTF_KEYUP, 0);
        }

        private void ShowSettings()
        {
            using (var settingsForm = new SettingsForm(_settingsManager))
            {
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    _settingsManager.Save();
                    Loc.CurrentLang = _settingsManager.CurrentSettings.Language;
                    InitializeIcons();
                }
            }
        }

        private void ShowAbout()
        {
            using (var aboutForm = new AboutForm(_settingsManager))
            {
                aboutForm.ShowDialog();
            }
        }

        private void ExitApp()
        {
            if (MessageBox.Show(Loc.Tr("ExitConfirm"), Loc.Tr("Exit"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                ExitThread();
        }

        protected override void ExitThreadCore()
        {
            SystemEvents.UserPreferenceChanged -= _preferenceChangedHandler;
            foreach (var icon in _trayIcons) { icon.Visible = false; icon.Dispose(); }
            _trayIcons.Clear();
            base.ExitThreadCore();
        }
    }

    // ---------------------------------------------------------
    // 5. AYARLAR FORMU
    // ---------------------------------------------------------
    public class SettingsForm : Form
    {
        private SettingsManager _manager;
        private RadioButton _rbAuto, _rbDark, _rbLight;
        private RadioButton _rbMulti, _rbSingle;
        private Panel _scrollPanel;
        private ComboBox _cbLanguage;
        private ComboBox _cbDoubleClickAction;
        private Label _lblDblAction;
        private CheckBox _chkEnableIcons;
        private List<Control> _iconControls = new List<Control>();

        public SettingsForm(SettingsManager manager)
        {
            _manager = manager;
            InitializeComponent();
            ApplyCurrentTheme();
        }

        private void ApplyCurrentTheme()
        {
            bool isDark = false;
            if (_rbDark.Checked) isDark = true;
            else if (_rbLight.Checked) isDark = false;
            else isDark = _manager.IsWindowsDarkMode();

            ThemeHelper.ApplyTheme(this, isDark);
        }

        private void InitializeComponent()
        {
            this.Size = new Size(500, 620);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            _scrollPanel = new Panel();
            _scrollPanel.Dock = DockStyle.Top;
            _scrollPanel.Height = 510;
            _scrollPanel.AutoScroll = true;
            this.Controls.Add(_scrollPanel);

            int yPos = 20;

            Label langLabel = CreateLabel(Loc.Tr("Lang") + ":", 20, yPos + 4);
            langLabel.Tag = "Lang";
            _scrollPanel.Controls.Add(langLabel);

            _cbLanguage = new ComboBox();
            _cbLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
            _cbLanguage.Items.Add(new KeyValuePair<string, string>("tr", "Türkçe"));
            _cbLanguage.Items.Add(new KeyValuePair<string, string>("en", "English"));
            _cbLanguage.DisplayMember = "Value";
            _cbLanguage.ValueMember = "Key";
            _cbLanguage.Location = new Point(140, yPos);
            _cbLanguage.Width = 110;
            foreach (KeyValuePair<string, string> item in _cbLanguage.Items)
            {
                if (item.Key == _manager.CurrentSettings.Language) { _cbLanguage.SelectedItem = item; break; }
            }
            _cbLanguage.SelectedIndexChanged += (s, e) => {
                var selected = (KeyValuePair<string, string>)_cbLanguage.SelectedItem;
                _manager.CurrentSettings.Language = selected.Key;
                Loc.CurrentLang = selected.Key;
                RefreshTexts();
            };
            _scrollPanel.Controls.Add(_cbLanguage);
            yPos += 40;
            AddSeparator(ref yPos);

            Label modeLabel = CreateLabel(Loc.Tr("TrayMode") + ":", 20, yPos);
            modeLabel.Tag = "TrayMode";
            _scrollPanel.Controls.Add(modeLabel);
            yPos += 30;

            Panel pnlTrayMode = new Panel();
            pnlTrayMode.Location = new Point(30, yPos);
            pnlTrayMode.Size = new Size(440, 60);
            _scrollPanel.Controls.Add(pnlTrayMode);

            _rbMulti = new RadioButton { Text = Loc.Tr("MultiTray"), Location = new Point(0, 0), AutoSize = true, Checked = !_manager.CurrentSettings.IsSingleTrayMode, Tag = "MultiTray" };
            pnlTrayMode.Controls.Add(_rbMulti);

            _rbSingle = new RadioButton { Text = Loc.Tr("SingleTray"), Location = new Point(0, 25), AutoSize = true, Checked = _manager.CurrentSettings.IsSingleTrayMode, Tag = "SingleTray" };
            pnlTrayMode.Controls.Add(_rbSingle);

            _lblDblAction = new Label { Text = Loc.Tr("DblClickAction") + ":", Location = new Point(170, 27), AutoSize = true, Tag = "DblClickAction", Font = new Font("Segoe UI", 9) };
            pnlTrayMode.Controls.Add(_lblDblAction);

            _cbDoubleClickAction = new ComboBox();
            _cbDoubleClickAction.DropDownStyle = ComboBoxStyle.DropDownList;
            _cbDoubleClickAction.Location = new Point(250, 22);
            _cbDoubleClickAction.Width = 120;
            _cbDoubleClickAction.Items.Add(new KeyValuePair<string, string>("next", Loc.Tr("ActionNext")));
            _cbDoubleClickAction.Items.Add(new KeyValuePair<string, string>("prev", Loc.Tr("ActionPrev")));
            _cbDoubleClickAction.Items.Add(new KeyValuePair<string, string>("play", Loc.Tr("ActionPlay")));
            _cbDoubleClickAction.Items.Add(new KeyValuePair<string, string>("stop", Loc.Tr("ActionStop")));
            _cbDoubleClickAction.DisplayMember = "Value";
            _cbDoubleClickAction.ValueMember = "Key";
            foreach (KeyValuePair<string, string> item in _cbDoubleClickAction.Items)
            {
                if (item.Key == _manager.CurrentSettings.DoubleClickAction) { _cbDoubleClickAction.SelectedItem = item; break; }
            }
            if (_cbDoubleClickAction.SelectedItem == null) _cbDoubleClickAction.SelectedIndex = 0;
            pnlTrayMode.Controls.Add(_cbDoubleClickAction);

            Action toggleDblClick = () => {
                _lblDblAction.Visible = _rbSingle.Checked;
                _cbDoubleClickAction.Visible = _rbSingle.Checked;
                if (_rbSingle.Checked) _cbDoubleClickAction.Left = _lblDblAction.Right + 5;
            };
            _rbSingle.CheckedChanged += (s, e) => toggleDblClick();
            _rbMulti.CheckedChanged += (s, e) => toggleDblClick();
            toggleDblClick();
            yPos += 60;
            AddSeparator(ref yPos);

            Label themeLabel = CreateLabel(Loc.Tr("Appearance") + ":", 20, yPos);
            themeLabel.Tag = "Appearance";
            _scrollPanel.Controls.Add(themeLabel);
            yPos += 30;

            Panel pnlThemeMode = new Panel();
            pnlThemeMode.Location = new Point(30, yPos);
            pnlThemeMode.Size = new Size(400, 90);
            _scrollPanel.Controls.Add(pnlThemeMode);

            _rbAuto = new RadioButton { Location = new Point(0, 0), AutoSize = true, Checked = _manager.CurrentSettings.ThemeMode == "auto" };
            _rbDark = new RadioButton { Location = new Point(0, 25), AutoSize = true, Checked = _manager.CurrentSettings.ThemeMode == "dark" };
            _rbLight = new RadioButton { Location = new Point(0, 50), AutoSize = true, Checked = _manager.CurrentSettings.ThemeMode == "light" };

            EventHandler themeChanged = (s, e) => ApplyCurrentTheme();
            _rbAuto.CheckedChanged += themeChanged;
            _rbDark.CheckedChanged += themeChanged;
            _rbLight.CheckedChanged += themeChanged;

            pnlThemeMode.Controls.AddRange(new Control[] { _rbAuto, _rbDark, _rbLight });
            yPos += 90;
            AddSeparator(ref yPos);

            _chkEnableIcons = new CheckBox();
            _chkEnableIcons.Text = Loc.Tr("CustomIcons") + ":";
            _chkEnableIcons.Location = new Point(20, yPos);
            _chkEnableIcons.AutoSize = true;
            _chkEnableIcons.Tag = "CustomIcons";
            _chkEnableIcons.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            _chkEnableIcons.Checked = _manager.CurrentSettings.EnableCustomIcons;
            _chkEnableIcons.CheckedChanged += (s, e) => ToggleIconControls();
            _scrollPanel.Controls.Add(_chkEnableIcons);
            yPos += 35;

            AddIconRow("previous", "Prev", ref yPos);
            AddIconRow("next", "Next", ref yPos);
            AddIconRow("play", "PlayPause", ref yPos);
            AddIconRow("stop", "Stop", ref yPos);

            ToggleIconControls();

            yPos += 15;
            Button btnSave = new Button();
            btnSave.Size = new Size(120, 35);
            btnSave.Location = new Point(350, 530);
            btnSave.DialogResult = DialogResult.OK;
            btnSave.Click += (s, e) => SaveChanges();
            btnSave.Tag = "Save";
            this.Controls.Add(btnSave);
            this.AcceptButton = btnSave;

            RefreshTexts();
        }

        private void ToggleIconControls()
        {
            bool enabled = _chkEnableIcons.Checked;
            foreach (Control c in _iconControls) c.Enabled = enabled;
        }

        private void RefreshTexts()
        {
            this.Text = Loc.Tr("Settings");
            bool isWinDark = _manager.IsWindowsDarkMode();
            string winStatus = isWinDark ? Loc.Tr("DarkShort") : Loc.Tr("LightShort");
            _rbAuto.Text = $"{Loc.Tr("Auto")} ({Loc.Tr("Current")}: {winStatus})";
            _rbDark.Text = Loc.Tr("Dark");
            _rbLight.Text = Loc.Tr("Light");

            var currentSel = ((KeyValuePair<string, string>)_cbDoubleClickAction.SelectedItem).Key;
            _cbDoubleClickAction.Items.Clear();
            _cbDoubleClickAction.Items.Add(new KeyValuePair<string, string>("next", Loc.Tr("ActionNext")));
            _cbDoubleClickAction.Items.Add(new KeyValuePair<string, string>("prev", Loc.Tr("ActionPrev")));
            _cbDoubleClickAction.Items.Add(new KeyValuePair<string, string>("play", Loc.Tr("ActionPlay")));
            _cbDoubleClickAction.Items.Add(new KeyValuePair<string, string>("stop", Loc.Tr("ActionStop")));
            foreach (KeyValuePair<string, string> item in _cbDoubleClickAction.Items)
            {
                if (item.Key == currentSel) { _cbDoubleClickAction.SelectedItem = item; break; }
            }
            UpdateRecursive(this);
            if (_rbSingle.Checked) _cbDoubleClickAction.Left = _lblDblAction.Right + 5;
        }

        private void UpdateRecursive(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                UpdateControlText(c);
                if (c.HasChildren) UpdateRecursive(c);
            }
        }

        private void UpdateControlText(Control c)
        {
            if (c.Tag is string key)
            {
                string text = Loc.Tr(key);
                if ((c is Label || c is CheckBox) && key != "Save" && !text.EndsWith(":")) text += ":";
                c.Text = text;
            }
        }

        private void AddIconRow(string iconType, string locKey, ref int y)
        {
            Label label = CreateLabel(Loc.Tr(locKey) + ":", 20, y + 6);
            label.Tag = locKey;
            label.Font = new Font("Segoe UI", 9);
            label.Size = new Size(115, 20);
            _scrollPanel.Controls.Add(label); _iconControls.Add(label);

            PictureBox darkPreview = CreatePreviewBox(140, y);
            _scrollPanel.Controls.Add(darkPreview); _iconControls.Add(darkPreview);

            Button btnDark = new Button { Tag = "DarkShort", Font = new Font("Segoe UI", 8), Size = new Size(60, 24), Location = new Point(175, y) };
            _scrollPanel.Controls.Add(btnDark); _iconControls.Add(btnDark);

            PictureBox lightPreview = CreatePreviewBox(275, y);
            lightPreview.BackColor = Color.FromArgb(240, 240, 240);
            _scrollPanel.Controls.Add(lightPreview); _iconControls.Add(lightPreview);

            Button btnLight = new Button { Tag = "LightShort", Font = new Font("Segoe UI", 8), Size = new Size(60, 24), Location = new Point(310, y) };
            _scrollPanel.Controls.Add(btnLight); _iconControls.Add(btnLight);

            Button btnClear = new Button { Tag = "Reset", Font = new Font("Segoe UI", 8), Size = new Size(55, 24), Location = new Point(410, y) };
            _scrollPanel.Controls.Add(btnClear); _iconControls.Add(btnClear);

            string darkKey = iconType + "_dark";
            string lightKey = iconType + "_light";

            void UpdatePreviews()
            {
                SetPreviewImage(darkPreview, darkKey, iconType, true);
                SetPreviewImage(lightPreview, lightKey, iconType, false);
            }
            UpdatePreviews();

            Action<string> select = (k) => {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Title = Loc.Tr("SelectIcon");
                    ofd.Filter = "Image Files|*.png;*.jpg;*.ico;*.bmp";
                    if (ofd.ShowDialog() == DialogResult.OK) { _manager.CurrentSettings.CustomIcons[k] = ofd.FileName; UpdatePreviews(); }
                }
            };
            btnDark.Click += (s, e) => select(darkKey);
            btnLight.Click += (s, e) => select(lightKey);
            btnClear.Click += (s, e) => { _manager.CurrentSettings.CustomIcons.Remove(darkKey); _manager.CurrentSettings.CustomIcons.Remove(lightKey); UpdatePreviews(); };

            y += 40;
        }

        private void SetPreviewImage(PictureBox pb, string fileKey, string type, bool dark)
        {
            if (pb.Image != null) pb.Image.Dispose();
            pb.Image = IconRenderer.CreateBitmap(type, dark, _manager.CurrentSettings.CustomIcons, true, 32);
        }

        private Label CreateLabel(string text, int x, int y)
        {
            return new Label { Text = text, Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(x, y), AutoSize = true };
        }

        private void AddSeparator(ref int y)
        {
            Label sep = new Label { BorderStyle = BorderStyle.Fixed3D, Height = 2, Width = 440, Location = new Point(20, y) };
            _scrollPanel.Controls.Add(sep);
            y += 20;
        }

        private PictureBox CreatePreviewBox(int x, int y)
        {
            return new PictureBox { Size = new Size(24, 24), Location = new Point(x, y), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.FromArgb(50, 50, 50) };
        }

        private void SaveChanges()
        {
            if (_rbAuto.Checked) _manager.CurrentSettings.ThemeMode = "auto";
            else if (_rbDark.Checked) _manager.CurrentSettings.ThemeMode = "dark";
            else if (_rbLight.Checked) _manager.CurrentSettings.ThemeMode = "light";

            _manager.CurrentSettings.IsSingleTrayMode = _rbSingle.Checked;
            _manager.CurrentSettings.EnableCustomIcons = _chkEnableIcons.Checked;

            if (_cbDoubleClickAction.SelectedItem != null)
                _manager.CurrentSettings.DoubleClickAction = ((KeyValuePair<string, string>)_cbDoubleClickAction.SelectedItem).Key;
        }
    }

    // ---------------------------------------------------------
    // 6. ÖZEL HAKKINDA FORMU
    // ---------------------------------------------------------
    public class AboutForm : Form
    {
        private SettingsManager _manager;
        public AboutForm(SettingsManager manager) { _manager = manager; InitializeComponent(); ApplyTheme(); }
        private void ApplyTheme()
        {
            bool isDark = _manager.IsDarkMode();
            ThemeHelper.ApplyTheme(this, isDark);
            foreach (Control c in this.Controls)
            {
                if (c.Tag?.ToString() == "Title") c.ForeColor = Color.FromArgb(52, 152, 219);
                if (c.Tag?.ToString() == "Dimmed") c.ForeColor = isDark ? Color.Gray : Color.DimGray;
                if (c.Tag?.ToString() == "Separator") c.BackColor = isDark ? Color.Gray : Color.LightGray;
            }
        }
        private void InitializeComponent()
        {
            this.Size = new Size(450, 410); this.StartPosition = FormStartPosition.CenterScreen; this.FormBorderStyle = FormBorderStyle.FixedDialog; this.MaximizeBox = false; this.MinimizeBox = false; this.Text = Loc.Tr("AboutTitle");
            int yPos = 15; int formWidth = this.ClientSize.Width;

            PictureBox pbLogo = new PictureBox(); pbLogo.Size = new Size(64, 64); pbLogo.Location = new Point((formWidth - 64) / 2, yPos);

            try
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MediaTrayMaster.app.png"))
                {
                    if (stream != null)
                    {
                        using (var temp = Image.FromStream(stream))
                        {
                            pbLogo.Image = IconRenderer.ResizeImage(temp, 64, 64);
                        }
                    }
                    else pbLogo.Image = IconRenderer.CreateIcon("play", false, _manager.CurrentSettings.CustomIcons, false).ToBitmap();
                }
            }
            catch
            {
                pbLogo.Image = IconRenderer.CreateIcon("play", false, _manager.CurrentSettings.CustomIcons, false).ToBitmap();
            }
            pbLogo.SizeMode = PictureBoxSizeMode.CenterImage; pbLogo.BackColor = Color.Transparent; this.Controls.Add(pbLogo); yPos += 70;

            Label lblTitle = new Label(); lblTitle.Text = Loc.Tr("AppTitle"); lblTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold); lblTitle.ForeColor = Color.FromArgb(52, 152, 219); lblTitle.Tag = "Title"; lblTitle.AutoSize = false; lblTitle.Size = new Size(formWidth, 30); lblTitle.Location = new Point(0, yPos); lblTitle.TextAlign = ContentAlignment.MiddleCenter; this.Controls.Add(lblTitle); yPos += 28;
            Label lblSub = new Label(); lblSub.Text = Loc.Tr("AppSub"); lblSub.Font = new Font("Segoe UI", 10); lblSub.Tag = "Dimmed"; lblSub.AutoSize = false; lblSub.Size = new Size(formWidth, 20); lblSub.Location = new Point(0, yPos); lblSub.TextAlign = ContentAlignment.MiddleCenter; this.Controls.Add(lblSub); yPos += 25;
            AddSeparator(ref yPos, formWidth);
            Label lblDesc = new Label(); lblDesc.Text = Loc.Tr("AboutDesc"); lblDesc.Font = new Font("Segoe UI", 10); lblDesc.AutoSize = false; lblDesc.Size = new Size(formWidth, 65); lblDesc.Location = new Point(0, yPos); lblDesc.TextAlign = ContentAlignment.MiddleCenter; this.Controls.Add(lblDesc); yPos += 65;
            Label lblFeat = new Label(); lblFeat.Text = Loc.Tr("Features"); lblFeat.Font = new Font("Segoe UI", 9, FontStyle.Bold); lblFeat.AutoSize = false; lblFeat.Size = new Size(formWidth, 20); lblFeat.Location = new Point(0, yPos); lblFeat.TextAlign = ContentAlignment.MiddleCenter; this.Controls.Add(lblFeat); yPos += 25;
            AddSeparator(ref yPos, formWidth);
            Label lblDev = new Label(); lblDev.Text = $"{Loc.Tr("DevLabel")}\n{Loc.Tr("ContactLabel")}"; lblDev.Font = new Font("Consolas", 9); lblDev.Tag = "Dimmed"; lblDev.AutoSize = false; lblDev.Size = new Size(formWidth, 35); lblDev.Location = new Point(0, yPos); lblDev.TextAlign = ContentAlignment.MiddleCenter; this.Controls.Add(lblDev); yPos += 40;
            Panel sep3 = new Panel(); sep3.Size = new Size(200, 1); sep3.Location = new Point((formWidth - 200) / 2, yPos); sep3.BackColor = Color.Gray; sep3.Tag = "Separator"; this.Controls.Add(sep3); yPos += 12;
            Label lblCopy = new Label(); lblCopy.Text = Loc.Tr("Copyright"); lblCopy.Font = new Font("Segoe UI", 8); lblCopy.Tag = "Dimmed"; lblCopy.AutoSize = false; lblCopy.Size = new Size(formWidth, 35); lblCopy.Location = new Point(0, yPos); lblCopy.TextAlign = ContentAlignment.MiddleCenter; this.Controls.Add(lblCopy);
        }
        private void AddSeparator(ref int y, int width) { Panel sep = new Panel(); sep.Size = new Size(300, 1); sep.Location = new Point((width - 300) / 2, y); sep.BackColor = Color.Gray; sep.Tag = "Separator"; this.Controls.Add(sep); y += 15; }
    }

    // ---------------------------------------------------------
    // 7. YARDIMCI SINIFLAR
    // ---------------------------------------------------------
    public class SettingsManager
    {
        private readonly string _filePath;
        public AppSettings CurrentSettings { get; private set; }
        public SettingsManager() { _filePath = Path.Combine(Application.StartupPath, "settings.json"); CurrentSettings = new AppSettings(); }
        public void Load() { try { if (File.Exists(_filePath)) { string json = File.ReadAllText(_filePath); CurrentSettings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings(); } } catch { CurrentSettings = new AppSettings(); } }
        public void Save() { try { var options = new JsonSerializerOptions { WriteIndented = true }; string json = JsonSerializer.Serialize(CurrentSettings, options); File.WriteAllText(_filePath, json); } catch { } }
        public bool IsDarkMode() { if (CurrentSettings.ThemeMode == "dark") return true; if (CurrentSettings.ThemeMode == "light") return false; return IsWindowsDarkMode(); }
        public bool IsWindowsDarkMode() { try { using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize")) { if (key?.GetValue("SystemUsesLightTheme") is int val) return val == 0; } } catch { } return false; }
    }

    public static class IconRenderer
    {
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);
            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                using (var wrapMode = new System.Drawing.Imaging.ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
        }

        public static Icon CreateIcon(string iconType, bool isDarkMode, Dictionary<string, string> customIcons, bool enableCustom)
        {
            using (Bitmap bmp = CreateBitmap(iconType, isDarkMode, customIcons, enableCustom, 32)) return ConvertBitmapToIcon(bmp);
        }

        public static Bitmap CreateBitmap(string iconType, bool isDarkMode, Dictionary<string, string> customIcons, bool enableCustom, int size = 16)
        {
            string iconKey = iconType + (isDarkMode ? "_dark" : "_light");
            if (enableCustom && customIcons != null && customIcons.ContainsKey(iconKey) && File.Exists(customIcons[iconKey]))
            {
                try { using (Bitmap original = new Bitmap(customIcons[iconKey])) return new Bitmap(original, new Size(size, size)); } catch { }
            }

            Bitmap bmp = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Color color = isDarkMode ? Color.White : Color.FromArgb(60, 60, 60);
                if (size <= 16 && !isDarkMode) color = Color.FromArgb(40, 40, 40);
                using (SolidBrush brush = new SolidBrush(color)) DrawShape(g, brush, iconType, size);
            }
            return bmp;
        }

        private static Icon ConvertBitmapToIcon(Bitmap bitmap) { IntPtr hIcon = bitmap.GetHicon(); try { using (Icon tempIcon = Icon.FromHandle(hIcon)) return (Icon)tempIcon.Clone(); } finally { NativeMethods.DestroyIcon(hIcon); } }
        private static void DrawShape(Graphics g, Brush brush, string type, int size)
        {
            float s = size / 32f;
            switch (type)
            {
                case "previous": g.FillRectangle(brush, 3 * s, 4 * s, 4 * s, 24 * s); g.FillPolygon(brush, new[] { new PointF(9 * s, 16 * s), new PointF(28 * s, 4 * s), new PointF(28 * s, 28 * s) }); break;
                case "next": g.FillPolygon(brush, new[] { new PointF(4 * s, 4 * s), new PointF(23 * s, 16 * s), new PointF(4 * s, 28 * s) }); g.FillRectangle(brush, 25 * s, 4 * s, 4 * s, 24 * s); break;
                case "play": g.FillPolygon(brush, new[] { new PointF(4 * s, 4 * s), new PointF(18 * s, 16 * s), new PointF(4 * s, 28 * s) }); g.FillRectangle(brush, 21 * s, 4 * s, 3 * s, 24 * s); g.FillRectangle(brush, 26 * s, 4 * s, 3 * s, 24 * s); break;
                case "stop": g.FillRectangle(brush, 4 * s, 4 * s, 24 * s, 24 * s); break;
            }
        }
    }

    public static class MenuIconRenderer
    {
        public static Bitmap DrawGear() { Bitmap bmp = new Bitmap(16, 16); using (Graphics g = Graphics.FromImage(bmp)) { g.SmoothingMode = SmoothingMode.AntiAlias; g.Clear(Color.Transparent); using (Pen pen = new Pen(Color.FromArgb(80, 80, 80), 2)) { g.DrawEllipse(pen, 3, 3, 10, 10); g.DrawLine(pen, 8, 1, 8, 15); g.DrawLine(pen, 1, 8, 15, 8); g.DrawLine(pen, 3, 3, 13, 13); g.DrawLine(pen, 13, 3, 3, 13); } g.FillEllipse(Brushes.White, 6, 6, 4, 4); } return bmp; }
        public static Bitmap DrawInfo() { Bitmap bmp = new Bitmap(16, 16); using (Graphics g = Graphics.FromImage(bmp)) { g.SmoothingMode = SmoothingMode.AntiAlias; g.Clear(Color.Transparent); using (Brush b = new SolidBrush(Color.FromArgb(52, 152, 219))) g.FillEllipse(b, 1, 1, 14, 14); g.FillRectangle(Brushes.White, 7, 4, 2, 2); g.FillRectangle(Brushes.White, 7, 7, 2, 5); } return bmp; }
        public static Bitmap DrawExit() { Bitmap bmp = new Bitmap(16, 16); using (Graphics g = Graphics.FromImage(bmp)) { g.SmoothingMode = SmoothingMode.AntiAlias; g.Clear(Color.Transparent); using (Pen p = new Pen(Color.FromArgb(231, 76, 60), 2)) { g.DrawArc(p, 3, 3, 10, 10, -60, 300); g.DrawLine(p, 8, 1, 8, 8); } } return bmp; }
    }

    public static class NativeMethods
    {
        [DllImport("user32.dll", SetLastError = true)] public static extern bool DestroyIcon(IntPtr hIcon);
        [DllImport("user32.dll")] public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
        public const int KEYEVENTF_EXTENDEDKEY = 0x1; public const int KEYEVENTF_KEYUP = 0x2;
        public const byte VK_MEDIA_NEXT_TRACK = 0xB0; public const byte VK_MEDIA_PREV_TRACK = 0xB1; public const byte VK_MEDIA_STOP = 0xB2; public const byte VK_MEDIA_PLAY_PAUSE = 0xB3;
    }
}