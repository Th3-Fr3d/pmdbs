﻿using pmdbs.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.IO;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using LunaForms;

namespace pmdbs
{
    /// <summary>
    /// The main UI form of the application.
    /// </summary>
    public partial class MainForm : MetroFramework.Forms.MetroForm
    {
        #region DECLARATIONS
        private List<ListEntry> entryList = new List<ListEntry>();
        
        private readonly char[] passwordSpecialCharacters = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '_', '-', '$', '%', '&', '/', '(', ')', '=', '?', '{', '[', ']', '}', '\\', '+', '*', '#', ',', '.', '<', '>', '|', '@', '!', '~', ';', ':', '"' };
        private readonly char[] passwordCharacters = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' };
        private string MasterPassword;
        private bool GuiLoaded = false;
        private bool IsDefaultIcon = true;
        private bool EditFieldShown = false;
        private Size MaxSize;
        private Size MinSize;
        private string DataDetailsID;
        private IconData AddIcon = new IconData();
        private int DataPerPage = 25;
        private int PreviousColorPage = 0;
        private int PreviousColorContentCount = 0;
        int MaxPages = 1;
        public static void InvokeReload()
        {
            GlobalVarPool.Form1.ApplyFilter(0);
        }

        public static void InvokeSyncAnimationStop()
        {
            GlobalVarPool.Form1.SyncAnimationStop();
        }

        public static void InvokeRefreshSettings()
        {
            GlobalVarPool.Form1.RefreshSettings();
        }
        #endregion

        #region FUNCTIONALITY_METHODS_AND_OTHER_UGLY_CODE
        private void DataFlowLayoutPanelList_MouseEnter(object sender, EventArgs e)
        {
            DataFlowLayoutPanelList.Focus();
        }

        private void AddFlowLayoutPanelCenter_MouseEnter(object sender, EventArgs e)
        {
            AddFlowLayoutPanelCenter.Focus();
        }

        private void DataFlowLayoutPanelEdit_MouseEnter(object sender, EventArgs e)
        {
            DataFlowLayoutPanelEdit.Focus();
        }

        private void WindowButtonClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void windowButtonMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void WindowHeaderLabelLogo_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                WinAPI.ReleaseCapture();
                WinAPI.SendMessage(Handle, WinAPI.WM_NCLBUTTONDOWN, WinAPI.HT_CAPTION, 0);
            }
        }

        private void FlowLayoutPanel1_Resize(object sender, EventArgs e)
        {
            foreach (ListEntry entry in DataFlowLayoutPanelList.Controls)
            {
                entry.Width = DataFlowLayoutPanelList.Width - 25;
            }
            WinAPI.ShowScrollBar(DataFlowLayoutPanelList.Handle, (int)WinAPI.ScrollBarDirection.SB_HORZ, false);
        }

        private void DashboardPanel_Paint(object sender, PaintEventArgs e)
        {
            Bitmap B = new Bitmap(Width, Height);
            Graphics G = Graphics.FromImage(B);
            G.Clear(Color.Transparent);
            G.DrawLine(new Pen(new SolidBrush(Color.FromArgb(17, 17, 17))), new Point(0, 0), new Point(Width, 0));
            e.Graphics.DrawImage((Image)(B.Clone()), 0, 0);
            G.Dispose();
            B.Dispose();
        }

        #endregion
        /// <summary>
        /// The main UI form of the application.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            #region LOAD
            // PREVENT FLICKERING
            foreach (Control c in this.Controls)
            {
                int style = WinAPI.GetWindowLong(c.Handle, WinAPI.GWL_EXSTYLE);
                style |= WinAPI.WS_EX_COMPOSITED;
                WinAPI.SetWindowLong(c.Handle, WinAPI.GWL_EXSTYLE, style);
            }
            GlobalVarPool.Form1 = this;
            InitializeTransparency();
            #region INIT
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            MaxSize = this.MaximumSize;
            MinSize = this.MinimumSize;
            this.MinimumSize = this.Size;
            this.MaximumSize = this.Size;
            GlobalVarPool.loadingSpinner = SettingsLunaProgressSpinnerFading;
            GlobalVarPool.loadingLabel = SettingsLabelLoadingStatus;
            GlobalVarPool.loadingLogo = SettingsPictureBoxLoadingLogo;
            GlobalVarPool.loadingPanel = SettingsPanelLoadingMain;
            GlobalVarPool.settingsPanel = SettingsTableLayoutPanelMain;
            GlobalVarPool.promptAction = SettingsLabelPromptAction;
            GlobalVarPool.promptEMail = SettingsLabelPromptMailInfo;
            GlobalVarPool.promptMain = SettingsLabelPromptMain;
            GlobalVarPool.promptPanel = SettingsPanelPromptMain;
            GlobalVarPool.SyncButton = DataSyncAdvancedImageButton;
            #endregion
            #region ADD_EVENTHANDLERS
            DataAddAdvancedImageButton.OnClickEvent += DataAddAdvancedImageButton_Click;
            DataDetailsRemoveAdvancedImageButton.OnClickEvent += DataRemoveAdvancedImageButton_Click;
            DataDetailsEditAdvancedImageButton.OnClickEvent += DataEditAdvancedImageButton_Click;
            DataLeftAdvancedImageButton.OnClickEvent += DataLeftAdvancedImageButton_Click;
            DataRightAdvancedImageButton.OnClickEvent += DataRightAdvancedImageButton_Click;
            DataSyncAdvancedImageButton.OnClickEvent += DataSyncAdvancedImageButton_Click;
            DataDetailsEntryEmail.OnClickEvent += DataDetailsEntryEmail_Click;
            DataDetailsEntryUsername.OnClickEvent += DataDetailsEntryUsername_Click;
            DataDetailsEntryPassword.OnClickEvent += DataDetailsEntryPassword_Click;
            DataDetailsEntryWebsite.OnClickEvent += DataDetailsEntryWebsite_Click;
            DataEditSaveAdvancedImageButton.OnClickEvent += DataEditSave_Click;
            DataEditCancelAdvancedImageButton.OnClickEvent += DataEditCancel_Click;
            MenuMenuEntryHome.OnClickEvent += MenuMenuEntryHome_Click;
            MenuMenuEntrySettings.OnClickEvent += MenuMenuEntrySettings_Click;
            MenuMenuEntryPasswords.OnClickEvent += MenuMenuEntryPasswords_Click;
            AddPanelAdvancedImageButtonSave.OnClickEvent += AddPanelAdvancedImageButtonSave_Click;
            AddPanelAdvancedImageButtonAbort.OnClickEvent += AddPanelAdvancedImageButtonAbort_Click;
            windowButtonClose.OnClickEvent += WindowButtonClose_Click;
            FilterEditFieldSearch.TextBoxTextChanged += FilterEditFieldSearch_TextChanged;
            LoginEditFieldOfflinePassword.EnterKeyPressed += LoginEditFieldOfflinePassword_EnterKeyPressed;
            SyncAnimationTimer.Tick += SyncAnimationTimer_Tick;
            windowButtonMinimize.OnClickEvent += windowButtonMinimize_Click;
            #endregion

            #endregion
            lunaItemList1.LunaItemClicked += card_click;
            DashboardLunaItemListBreaches.LunaItemClicked += Breach_Clicked;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            if (GuiLoaded)
            {
                return;
            }
            this.Hide();

            ThreadPool.QueueUserWorkItem((x) =>
            {
                using (var splashForm = new SplashForm())
                {
                    splashForm.Show();
                    while (!GuiLoaded)
                        Application.DoEvents();
                    splashForm.Close();
                    this.Invoke((MethodInvoker)delegate {
                        // this code runs on the UI thread!
                        this.Show();
                        this.Activate();
                    });
                }
            });
            await HelperMethods.LoadSettings();
            //TODO: Check for Password Changes
            if (GlobalVarPool.scryptHash.Equals(string.Empty))
            {
                LoginPictureBoxOnlineMain.SuspendLayout();
                LoginPictureBoxOfflineMain.SuspendLayout();
                LoginPictureBoxRegisterMain.BringToFront();
                LoginPictureBoxLoadingMain.SuspendLayout();
            }
            else
            {
                LoginPictureBoxOnlineMain.SuspendLayout();
                LoginPictureBoxOfflineMain.BringToFront();
                LoginPictureBoxRegisterMain.SuspendLayout();
                LoginPictureBoxLoadingMain.SuspendLayout();
                if (!GlobalVarPool.name.Equals("User"))
                {
                    LoginLabelOfflineUsername.Text = GlobalVarPool.name;
                }
            }
            Thread.Sleep(1500); // SHOW SPLASHSCREEN
            GuiLoaded = true;
        }

        #region Menu
        Bitmap bmp;
        float angle = 0f;
        bool showSyncAnimation = false;
        private System.Windows.Forms.Timer SyncAnimationTimer = new System.Windows.Forms.Timer();
        private void MenuMenuEntryHome_Click(object sender, EventArgs e)
        {
            MenuPanelHomeIndicator.BackColor = Colors.Orange;
            MenuPanelSettingsIndicator.BackColor = Color.White;
            MenuPanelPasswordsIndicator.BackColor = Color.White;
            WindowHeaderLabelTitle.Text = "Dashboard";
            FilterAdvancedComboBoxSort.Visible = false;
            FilterEditFieldSearch.Visible = false;
            DashboardTableLayoutPanel.BringToFront();
        }

        private void MenuMenuEntrySettings_Click(object sender, EventArgs e)
        {
            MenuPanelHomeIndicator.BackColor = Color.White;
            MenuPanelSettingsIndicator.BackColor = Colors.Orange;
            MenuPanelPasswordsIndicator.BackColor = Color.White;
            SettingsTableLayoutPanelMain.BringToFront();
            FilterAdvancedComboBoxSort.Visible = false;
            FilterEditFieldSearch.Visible = false;
            if (GlobalVarPool.wasOnline)
            {
                SettingsFlowLayoutPanelOnline.BringToFront();
            }
            else
            {
                SettingsFlowLayoutPanelOffline.BringToFront();
            }
            WindowHeaderLabelTitle.Text = "Settings";
        }

        private void MenuMenuEntryPasswords_Click(object sender, EventArgs e)
        {
            MenuPanelHomeIndicator.BackColor = Color.White;
            MenuPanelSettingsIndicator.BackColor = Color.White;
            MenuPanelPasswordsIndicator.BackColor = Colors.Orange;
            FilterAdvancedComboBoxSort.Visible = true;
            FilterEditFieldSearch.Visible = true;
            DataTableLayoutPanelMain.BringToFront();
            WindowHeaderLabelTitle.Text = "Passwords";
        }

        private void SyncAnimationStart()
        {
            showSyncAnimation = true;
            GlobalVarPool.outputLabel = MenuSyncLabelStatus;
            GlobalVarPool.outputLabelIsValid = true;
            bmp = new Bitmap(Resources.icon_syncing);
            MenuSyncLabelHeader.Text = "Syncing ...";
            MenuSyncLabelHeader.ForeColor = Colors.Orange;
            MenuSyncLabelStatus.ForeColor = Colors.Orange;
            MenuSyncPictureBox.Image = Resources.icon_empty;
            SyncAnimationTimer.Interval = 50;
            SyncAnimationTimer.Start();
        }

        private void SyncAnimationStop()
        {
            showSyncAnimation = false;
        }

        private void SyncAnimationTimer_Tick(object sender, EventArgs e)
        {
            angle += 18;
            angle = angle % 360;
            MenuSyncPictureBox.Invalidate();
            if (!showSyncAnimation && angle == 0)
            {
                SyncAnimationTimer.Stop();
                bmp = null;
                MenuSyncPictureBox.Invalidate();
                MenuSyncPictureBox.Image = Resources.Icon_sync;
                GlobalVarPool.outputLabelIsValid = false;
                MenuSyncLabelHeader.ForeColor = Color.FromArgb(100, 100, 100);
                MenuSyncLabelStatus.ForeColor = Color.FromArgb(100, 100, 100);
                MenuSyncLabelHeader.Text = "Sync";
                MenuSyncLabelStatus.Text = "Last Update: " + TimeConverter.UnixTimeStampToDateTime(Convert.ToDouble(TimeConverter.TimeStamp())).ToString("u");
            }
        }

        private void MenuSyncPictureBox_Paint(object sender, PaintEventArgs e)
        {
            if (bmp != null)
            {
                float bw2 = bmp.Width / 2f;
                float bh2 = bmp.Height / 2f;
                e.Graphics.TranslateTransform(bw2, bh2);
                e.Graphics.RotateTransform(angle);
                e.Graphics.TranslateTransform(-bw2, -bh2);
                e.Graphics.DrawImage(bmp, 0, 0);
                e.Graphics.ResetTransform();
            }
        }
        #endregion

        #region DataPanel
        private void AddSingleEntry(DataRow newRow)
        {
            if (PreviousColorContentCount > 24)
            {
                return;
            }
            int ID = Convert.ToInt32(newRow["0"]);
            string strTimeStamp = TimeConverter.UnixTimeStampToDateTime(Convert.ToDouble(newRow["2"].ToString())).ToString("u");
            strTimeStamp = strTimeStamp.Substring(0, strTimeStamp.Length - 1);
            ListEntry listEntry = new ListEntry
            {
                BackColor = Color.White,
                HostName = newRow["3"].ToString().Equals("\x01") ? "-" : newRow["3"].ToString(),
                HostNameFont = new Font("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point, 0),
                HostNameForeColor = SystemColors.ControlText,
                Name = "listEntry",
                Size = new Size(1041, 75),
                TabIndex = 14,
                TimeStamp = strTimeStamp,
                TimeStampFont = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0),
                TimeStampForeColor = SystemColors.ControlText,
                UserName = newRow["4"].ToString(),
                UserNameFont = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, 0),
                UserNameForeColor = SystemColors.ControlText,
                ColorNormal = Color.White,
                ColorHover = Colors.LightGray,
                BackgroundColor = Color.White,
                id = ID
            };
            byte[] iconBytes = Convert.FromBase64String(newRow["9"].ToString());
            using (MemoryStream ms = new MemoryStream(iconBytes, 0, iconBytes.Length))
            {
                Image icon = Image.FromStream(ms, true);
                listEntry.FavIcon = icon.GetThumbnailImage(350, 350, null, new IntPtr());
                icon.Dispose();
            }
            DataFlowLayoutPanelList.Controls.Add(listEntry);
            entryList.Add(listEntry);
            listEntry.OnClickEvent += ListEntry_Click;
            DataFlowLayoutPanelList.SetFlowBreak(listEntry, true);
            FlowLayoutPanel1_Resize(this, null);
        }

        private void ReloadSingleEntry(DataRow invalidatedRow)
        {
            // TODO: IS THIS CORRECT?
            // invalidatedEntry SEEMS TO BE NEVER USED
            int ID = Convert.ToInt32(invalidatedRow["0"]);
            ListEntry invalidatedEntry = entryList.Where(element => element.id == ID).First();
            ApplyFilter(PreviousColorPage);
        }

        private void RefreshUserData(int page)
        {
            if (page < 0)
            {
                page = 0;
            }
            DataTable userData = GlobalVarPool.FilteredUserData;
            // GET MAXIMUM PAGES (FLOOR)
            MaxPages = Convert.ToInt32(Math.Floor((double)userData.Rows.Count / DataPerPage));
            if (userData.Rows.Count % DataPerPage != 0)
            {
                // ADD ONE PAGE
                MaxPages++;
            }
            // CHECK IF PAGE IS WITHIN BOUNDARIES
            if (page < MaxPages)
            {
                PreviousColorPage = page;
            }
            else
            {
                if (PreviousColorPage == MaxPages - 1)
                {
                    return;
                }
                else
                {
                    PreviousColorPage = MaxPages - 1 < 0 ? 0 : MaxPages - 1;
                }
            }
            PreviousColorContentCount = 0;
            //D_id, D_hid, D_datetime, D_host, D_uname, D_password, D_url, D_email, D_notes
            DataFlowLayoutPanelList.SuspendLayout();
            for (int i = 0; i < entryList.Count; i++)
            {
                entryList[i].Hide();
            }
            for (int i = PreviousColorPage * DataPerPage; ((PreviousColorPage * DataPerPage) + DataPerPage >= userData.Rows.Count) ? i < userData.Rows.Count : i < (PreviousColorPage * DataPerPage) + DataPerPage; i++)
            {
                int ID = Convert.ToInt32(userData.Rows[i]["0"]);
                string strTimeStamp = TimeConverter.UnixTimeStampToDateTime(Convert.ToDouble(userData.Rows[i]["2"].ToString())).ToString("u");
                strTimeStamp = strTimeStamp.Substring(0, strTimeStamp.Length - 1);
                ListEntry entry = entryList[i % DataPerPage];
                byte[] iconBytes = Convert.FromBase64String(userData.Rows[i]["9"].ToString());
                using (MemoryStream ms = new MemoryStream(iconBytes, 0, iconBytes.Length))
                {
                    Image icon = Image.FromStream(ms, true);
                    entry.FavIcon = icon.GetThumbnailImage(350, 350, null, new IntPtr());
                    icon.Dispose();
                }
                entry.HostName = userData.Rows[i]["3"].ToString().Equals("\x01") ? "-" : userData.Rows[i]["3"].ToString();
                entry.TimeStamp = strTimeStamp;
                entry.UserName = userData.Rows[i]["4"].ToString();
                entry.id = ID;
                entry.Show();
                PreviousColorContentCount++;
            }
            DataFlowLayoutPanelList.ResumeLayout();
            FlowLayoutPanel1_Resize(this, null);
        }

        private void ListEntry_Click(object sender, EventArgs e)
        {
            if (EditFieldShown)
            {
                // USE LINQ TO CHECK IF THERE ARE UNSAVED CHANGES
                if (!GlobalVarPool.UserData.AsEnumerable().Where(row => row["0"].ToString().Equals(DataDetailsID)).Where(row => (row["3"].ToString().Equals(DataEditEditFieldHostname.TextTextBox) || (row["3"].ToString().Equals("\x01") && string.IsNullOrEmpty(DataEditEditFieldHostname.TextTextBox))) && (row["4"].ToString().Equals(DataEditEditFieldUsername.TextTextBox) || (row["4"].ToString().Equals("\x01") && string.IsNullOrEmpty(DataEditEditFieldUsername.TextTextBox))) && (row["5"].ToString().Equals(DataEditEditFieldPassword.TextTextBox) || (row["5"].ToString().Equals("\x01") && string.IsNullOrEmpty(DataEditEditFieldPassword.TextTextBox))) && (row["6"].ToString().Equals(DataEditEditFieldWebsite.TextTextBox) || (row["6"].ToString().Equals("\x01") && string.IsNullOrEmpty(DataEditEditFieldWebsite.TextTextBox))) && (row["7"].ToString().Equals(DataEditEditFieldEmail.TextTextBox) || row["7"].ToString().Equals("\x01") && (string.IsNullOrEmpty(DataEditEditFieldEmail.TextTextBox))) && (row["8"].ToString().Equals(DataEditAdvancedRichTextBoxNotes.TextValue) || (row["8"].ToString().Equals("\x01") && string.IsNullOrEmpty(DataEditAdvancedRichTextBoxNotes.TextValue)))).Any())
                {
                    bool actionIsConfirmed = false;
                    using (MetroFramework.Forms.MetroForm prompt = new ConfirmationForm("There are unsaved changes. Do you really want to discard them?", MessageBoxButtons.YesNo))
                    {
                        actionIsConfirmed = prompt.ShowDialog().Equals(DialogResult.OK);
                    }
                    if (!actionIsConfirmed) { return; }
                }
                EditFieldShown = false;
            }
            //D_id, D_hid, D_datetime, D_host, D_uname, D_password, D_url, D_email, D_notes
            DataPanelDetails.BringToFront();
            ListEntry SenderObject = (ListEntry)sender;
            int index = SenderObject.id;
            DataRow LinkedRow = GlobalVarPool.UserData.AsEnumerable().SingleOrDefault(r => r.Field<String>("0").Equals(index.ToString()));
            UpdateDetailsWindow(LinkedRow);
        }

        private void UpdateDetailsWindow(DataRow LinkedRow)
        {
            byte[] iconBytes = Convert.FromBase64String(LinkedRow["9"].ToString());
            using (MemoryStream ms = new MemoryStream(iconBytes, 0, iconBytes.Length))
            {
                Image icon = Image.FromStream(ms);
                DataPictureBoxDetailsLogo.Image = icon.GetThumbnailImage(350, 350, null, new IntPtr());
                icon.Dispose();
            }
            DataLabelDetailsHostname.Text = LinkedRow["3"].ToString();
            DataDetailsEntryUsername.Content = LinkedRow["4"].ToString().Equals("\x01") ? "-" : LinkedRow["4"].ToString();
            DataDetailsEntryPassword.Content = LinkedRow["5"].ToString().Equals("\x01") ? "-" : LinkedRow["5"].ToString();
            DataDetailsEntryWebsite.Content = LinkedRow["6"].ToString().Equals("\x01") ? "-" : LinkedRow["6"].ToString();
            DataDetailsEntryWebsite.RawText = LinkedRow["6"].ToString().Equals("\x01") ? "-" : LinkedRow["6"].ToString();
            DataDetailsEntryEmail.Content = LinkedRow["7"].ToString().Equals("\x01") ? "-" : LinkedRow["7"].ToString();
            DataDetailsCustomLabelNotes.Content = LinkedRow["8"].ToString().Equals("\x01") ? "-" : LinkedRow["8"].ToString();
            DataDetailsID = LinkedRow["0"].ToString();
        }

        private void DataAddAdvancedImageButton_Click(object sender, EventArgs e)
        {
            DataTableLayoutPanelMain.SuspendLayout();
            AddPanelMain.BringToFront();
            AddPanelMain.ResumeLayout();
        }

        private async void DataEditSave_Click(object sender, EventArgs e)
        {
            DataEditSaveAdvancedImageButton.Enabled = false;
            //D_id, D_hid, D_datetime, D_host, D_uname, D_password, D_url, D_email, D_notes
            string Hostname = DataBaseHelper.Security.SQLInjectionCheck(DataEditEditFieldHostname.TextTextBox);
            string Username = DataBaseHelper.Security.SQLInjectionCheck(DataEditEditFieldUsername.TextTextBox);
            string Password = DataBaseHelper.Security.SQLInjectionCheck(DataEditEditFieldPassword.TextTextBox);
            string Email = DataBaseHelper.Security.SQLInjectionCheck(DataEditEditFieldEmail.TextTextBox);
            string Website = DataBaseHelper.Security.SQLInjectionCheck(DataEditEditFieldWebsite.TextTextBox);
            string Notes = DataBaseHelper.Security.SQLInjectionCheck(DataEditAdvancedRichTextBoxNotes.TextValue);
            string DateTime = TimeConverter.TimeStamp();
            string[] Values = new string[]
            {
                Hostname,
                Username,
                Password,
                Website,
                Email,
                Notes
            };
            string[] RawValues = new string[Values.Length];
            Array.Copy(Values, RawValues, Values.Length);
            string[] Columns = new string[]
            {
                "D_host",
                "D_uname",
                "D_password",
                "D_url",
                "D_email",
                "D_notes"
            };
            if (Password.Equals("") || Hostname.Equals(""))
            {
                return;
            }
            for (int i = 0; i < Values.Length; i++)
            {
                if (Values[i].Equals(""))
                {
                    Values[i] = "\x01";
                }
                else
                {
                    Values[i] = CryptoHelper.AESEncrypt(Values[i], GlobalVarPool.localAESkey);
                }
            }
            string Query = "UPDATE Tbl_data SET D_datetime = \"" + DateTime + "\"";
            for (int i = 0; i < Columns.Length; i++)
            {
                Query += ", " + Columns[i] + " = \"" + Values[i] + "\"";
            }
            Query += " WHERE D_id = " + DataDetailsID + ";";
            await DataBaseHelper.ModifyData(Query);
            DataRow LinkedRow = GlobalVarPool.UserData.AsEnumerable().SingleOrDefault(r => r.Field<string>("0").Equals(DataDetailsID));
            string oldUrl = LinkedRow["6"].ToString();
            string oldHostname = LinkedRow["3"].ToString();
            if (!Website.Equals(oldUrl))
            {
                new Thread(async delegate () {
                    string favIcon = "";
                    try
                    {
                        if (string.IsNullOrWhiteSpace(Website))
                        {
                            if (!Hostname[0].Equals(oldHostname[0]) || !oldUrl.Equals("\x01"))
                            {
                                favIcon = pmdbs.Icon.Generate(Hostname);
                            }
                            else
                            {
                                return;
                            }
                        }
                        else
                        {
                            favIcon = pmdbs.Icon.Get(Website, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message.ToUpper() + "\n" + ex.ToString());
                        favIcon = pmdbs.Icon.Generate(Hostname);
                    }
                    LinkedRow["9"] = favIcon;
                    string encryptedFavIcon = CryptoHelper.AESEncrypt(favIcon, GlobalVarPool.localAESkey);
                    Query = DataBaseHelper.Security.SQLInjectionCheckQuery(new string[] { "UPDATE Tbl_data SET D_icon = \"", encryptedFavIcon, "\" WHERE D_id = ", DataDetailsID + ";" });
                    await DataBaseHelper.ModifyData(Query);
                    Invoke((MethodInvoker)delegate
                    {
                        UpdateDetailsWindow(LinkedRow);
                        ReloadSingleEntry(LinkedRow);
                    });
                }).Start();
            }
            for (int i = 3; i < (int)ColumnCount.Tbl_data - 1; i++)
            {
                LinkedRow[i.ToString()] = RawValues[i - 3].Equals("") ? "\x01" : RawValues[i - 3];
            }
            UpdateDetailsWindow(LinkedRow);
            LinkedRow["2"] = DateTime;
            ReloadSingleEntry(LinkedRow);
            DataFlowLayoutPanelEdit.SuspendLayout();
            DataPanelDetails.BringToFront();
            DataPanelDetails.ResumeLayout();
            DataEditSaveAdvancedImageButton.Enabled = true;
            EditFieldShown = false;
        }

        private void DataEditCancel_Click(object sender, EventArgs e)
        {
            // USE LINQ TO CHECK IF THERE ARE UNSAVED CHANGES
            if (!GlobalVarPool.UserData.AsEnumerable().Where(row => row["0"].ToString().Equals(DataDetailsID)).Where(row => (row["3"].ToString().Equals(DataEditEditFieldHostname.TextTextBox) || (row["3"].ToString().Equals("\x01") && string.IsNullOrEmpty(DataEditEditFieldHostname.TextTextBox))) && (row["4"].ToString().Equals(DataEditEditFieldUsername.TextTextBox) || (row["4"].ToString().Equals("\x01") && string.IsNullOrEmpty(DataEditEditFieldUsername.TextTextBox))) && (row["5"].ToString().Equals(DataEditEditFieldPassword.TextTextBox) || (row["5"].ToString().Equals("\x01") && string.IsNullOrEmpty(DataEditEditFieldPassword.TextTextBox))) && (row["6"].ToString().Equals(DataEditEditFieldWebsite.TextTextBox) || (row["6"].ToString().Equals("\x01") && string.IsNullOrEmpty(DataEditEditFieldWebsite.TextTextBox))) && (row["7"].ToString().Equals(DataEditEditFieldEmail.TextTextBox) || row["7"].ToString().Equals("\x01") && (string.IsNullOrEmpty(DataEditEditFieldEmail.TextTextBox))) && (row["8"].ToString().Equals(DataEditAdvancedRichTextBoxNotes.TextValue) || (row["8"].ToString().Equals("\x01") && string.IsNullOrEmpty(DataEditAdvancedRichTextBoxNotes.TextValue)))).Any())
            {
                bool actionIsConfirmed = false;
                using (MetroFramework.Forms.MetroForm prompt = new ConfirmationForm("There are unsaved changes. Do you really want to discard them?", MessageBoxButtons.YesNo))
                {
                    actionIsConfirmed = prompt.ShowDialog().Equals(DialogResult.OK);
                }
                if (!actionIsConfirmed) { return; }
            }
            DataFlowLayoutPanelEdit.SuspendLayout();
            DataPanelDetails.BringToFront();
            DataPanelDetails.ResumeLayout();
            EditFieldShown = false;
        }

        private void DataEditAdvancedImageButton_Click(object sender, EventArgs e)
        {
            DataRow LinkedRow = GlobalVarPool.UserData.AsEnumerable().SingleOrDefault(r => r.Field<String>("0").Equals(DataDetailsID));
            DataEditEditFieldHostname.TextTextBox = LinkedRow["3"].ToString();
            DataEditEditFieldUsername.TextTextBox = LinkedRow["4"].ToString().Equals("\x01") ? "" : LinkedRow["4"].ToString();
            DataEditEditFieldPassword.TextTextBox = LinkedRow["5"].ToString().Equals("\x01") ? "" : LinkedRow["5"].ToString();
            DataEditEditFieldWebsite.TextTextBox = LinkedRow["6"].ToString().Equals("\x01") ? "" : LinkedRow["6"].ToString();
            
            DataEditEditFieldEmail.TextTextBox = LinkedRow["7"].ToString().Equals("\x01") ? "" : LinkedRow["7"].ToString();
            DataEditAdvancedRichTextBoxNotes.TextValue = LinkedRow["8"].ToString().Equals("\x01") ? "" : LinkedRow["8"].ToString();
            byte[] iconBytes = Convert.FromBase64String(LinkedRow["9"].ToString());
            using (MemoryStream ms = new MemoryStream(iconBytes, 0, iconBytes.Length))
            {
                Image icon = Image.FromStream(ms);
                DataEditPictureBoxLogo.Image = icon.GetThumbnailImage(350, 350, null, new IntPtr());
                icon.Dispose();
            }
            DataPanelDetails.SuspendLayout();
            DataFlowLayoutPanelEdit.BringToFront();
            DataFlowLayoutPanelEdit.ResumeLayout();
            EditFieldShown = true;
        }

        private void DataEditAnimatedButtonGeneratePassword_Click(object sender, EventArgs e)
        {
            char[] characterSet;
            int passwordLength = Convert.ToInt32(DataEditAdvancedNumericUpDown.TextValue);
            bool specialCharactersEnabled = DataEditAdvancedCheckBox.Checked;
            string randomizedPassword = string.Empty;
            if (specialCharactersEnabled)
            {
                characterSet = passwordSpecialCharacters;
            }
            else
            {
                characterSet = passwordCharacters;
            }
            using (System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                // INITIALIZE 4 BYTE BUFFER
                byte[] buffer = new byte[4];

                for (int i = 0; i < passwordLength; i++)
                {
                    // FILL BUFFER.
                    rng.GetBytes(buffer);

                    // CONVERT TO Int32 AND USE ABSOLUTE VALUE.
                    int value = Math.Abs(BitConverter.ToInt32(buffer, 0));
                    // USE MODULO TO MAP 32 BIT INTEGER TO CHARACTER RANGE
                    randomizedPassword += characterSet[value % characterSet.Length];
                }
            }
            DataEditEditFieldPassword.TextTextBox = randomizedPassword;
        }

        private async void DataRemoveAdvancedImageButton_Click(object sender, EventArgs e)
        {
            bool actionIsConfirmed = false;
            using (MetroFramework.Forms.MetroForm prompt = new ConfirmationForm("Do you really want to delete the selected account?",MessageBoxButtons.YesNo))
            {
                actionIsConfirmed = prompt.ShowDialog().Equals(DialogResult.OK);
            }
            if (!actionIsConfirmed) { return; }
            DataRow LinkedRow = GlobalVarPool.UserData.AsEnumerable().SingleOrDefault(r => r.Field<string>("0").Equals(DataDetailsID));
            string hid = LinkedRow["1"].ToString();
            if (!hid.Equals("EMPTY"))
            {
                await DataBaseHelper.ModifyData(DataBaseHelper.Security.SQLInjectionCheckQuery(new string[] { "INSERT INTO Tbl_delete (DEL_hid) VALUES (\"", hid, "\");" }));
            }
            await DataBaseHelper.ModifyData(DataBaseHelper.Security.SQLInjectionCheckQuery(new string[] { "DELETE FROM Tbl_data WHERE D_id = ", DataDetailsID, ";" }));
            GlobalVarPool.UserData.Rows.Remove(LinkedRow);
            ApplyFilter(PreviousColorPage);
            DataPanelDetails.SuspendLayout();
            DataPanelNoSel.BringToFront();
            DataPanelNoSel.ResumeLayout();
        }

        private void DataLeftAdvancedImageButton_Click(object sender, EventArgs e)
        {
            if (PreviousColorPage != 0)
            {
                RefreshUserData(PreviousColorPage - 1);
            }
        }

        private void DataRightAdvancedImageButton_Click(object sender, EventArgs e)
        {
            RefreshUserData(PreviousColorPage + 1);
        }

        private void DataSyncAdvancedImageButton_Click(object sender, EventArgs e)
        {
            if (!GlobalVarPool.wasOnline)
            {
                DataTableLayoutPanelMain.SuspendLayout();
                SettingsTableLayoutPanelMain.BringToFront();
                SettingsFlowLayoutPanelRegister.BringToFront();
                return;
            }
            SyncAnimationStart();
            AutomatedTaskFramework.Tasks.Clear();
            if (!GlobalVarPool.connected)
            {
                AutomatedTaskFramework.Task.Create(SearchCondition.Contains, "DEVICE_AUTHORIZED", NetworkAdapter.MethodProvider.Connect);
            }
            if (!GlobalVarPool.isUser)
            {
                AutomatedTaskFramework.Task.Create(SearchCondition.In, "ALREADY_LOGGED_IN|LOGIN_SUCCESSFUL", NetworkAdapter.MethodProvider.Login);
            }
            AutomatedTaskFramework.Task.Create(SearchCondition.Contains, "FETCH_SYNC", NetworkAdapter.MethodProvider.Sync);
            AutomatedTaskFramework.Tasks.Execute();
            DataSyncAdvancedImageButton.Enabled = false;
        }

        private void DataDetailsEntryUsername_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(DataDetailsEntryUsername.Content);
        }

        private void DataDetailsEntryPassword_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(DataDetailsEntryPassword.Content);
        }

        private void DataDetailsEntryEmail_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(DataDetailsEntryEmail.Content);
        }

        private void DataDetailsEntryWebsite_Click(object sender, EventArgs e)
        {
            if (!DataDetailsEntryWebsite.RawText.Equals("-"))
            {
                System.Diagnostics.Process.Start(DataDetailsEntryWebsite.RawText);
            }
        }

        private void DataFilterResultsAnimatedButtonReset_Click(object sender, EventArgs e)
        {
            FilterEditFieldSearch.TextTextBox = string.Empty;
            FilterEditFieldSearch.ShowDefaultValue();
            DataPanelNoSel.BringToFront();
        }
        #endregion

        #region AddPanel
        private void AddPanelGeneratePasswordAnimatedButtonGenerate_Click(object sender, EventArgs e)
        {
            char[] characterSet;
            int passwordLength = Convert.ToInt32(AddPanelGeneratePasswordeAdvancedNumericUpDown.TextValue);
            bool specialCharactersEnabled = AddPanelGeneratePasswordAdvancedCheckBox.Checked;
            string randomizedPassword = string.Empty;
            if (specialCharactersEnabled)
            {
                characterSet = passwordSpecialCharacters;
            }
            else
            {
                characterSet = passwordCharacters;
            }
            using (System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                // INITIALIZE 4 BYTE BUFFER
                byte[] buffer = new byte[4];

                for (int i = 0; i < passwordLength; i++)
                {
                    // FILL BUFFER.
                    rng.GetBytes(buffer);

                    // CONVERT TO Int32 AND USE ABSOLUTE VALUE.
                    int value = Math.Abs(BitConverter.ToInt32(buffer, 0));
                    // USE MODULO TO MAP 32 BIT INTEGER TO CHARACTER RANGE
                    randomizedPassword += characterSet[value % characterSet.Length];
                }
            }
            AddEditFieldPassword.TextTextBox = randomizedPassword;
        }

        private void ResetFields()
        {
            AddPanelNotesAdvancedRichTextBox.TextValue = "";
            AddEditFieldEmail.TextTextBox = "";
            AddEditFieldHostname.TextTextBox = "";
            AddEditFieldPassword.TextTextBox = "";
            AddEditFieldUsername.TextTextBox = "";
            AddEditFieldWebsite.TextTextBox = "";
            AddPictureBoxCheckIconIcon.Image = Resources._default;
            IsDefaultIcon = true;
        }

        private void AddPanelAdvancedImageButtonSave_Click(object sender, EventArgs e)
        {
            AddPanelAdvancedImageButtonSave.Enabled = false;
            string Hostname = DataBaseHelper.Security.SQLInjectionCheck(AddEditFieldHostname.TextTextBox);
            string Username = DataBaseHelper.Security.SQLInjectionCheck(AddEditFieldUsername.TextTextBox);
            string Password = DataBaseHelper.Security.SQLInjectionCheck(AddEditFieldPassword.TextTextBox);
            string Email = DataBaseHelper.Security.SQLInjectionCheck(AddEditFieldEmail.TextTextBox);
            string Website = DataBaseHelper.Security.SQLInjectionCheck(AddEditFieldWebsite.TextTextBox);
            string Notes = DataBaseHelper.Security.SQLInjectionCheck(AddPanelNotesAdvancedRichTextBox.TextValue);
            string DateTime = TimeConverter.TimeStamp();
            
            if (Password.Equals("") || Hostname.Equals(""))
            {
                CustomException.ThrowNew.GenericException("Please enter at least a hostname and a password to save this account!");
                AddPanelAdvancedImageButtonSave.Enabled = true;
                return;
            }
            new Thread(async delegate() {
                string favIcon = string.Empty;
                if (!IsDefaultIcon)
                {
                    using (Bitmap iconBmp = new Bitmap(AddPictureBoxCheckIconIcon.Image))
                    using (MemoryStream ms = new MemoryStream())
                    {
                        iconBmp.Save(ms, ImageFormat.Png);
                        favIcon = Convert.ToBase64String(ms.ToArray());
                    }
                }
                else
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(Website))
                        {
                            // EXECUTE CODE IN CATCH
                            throw new Exception();
                        }
                        else
                        {
                            favIcon = pmdbs.Icon.Get(Website, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message.ToUpper() + "\n" + ex.ToString());
                        favIcon = pmdbs.Icon.Generate(Hostname);
                    }
                }
                string[] Values = new string[]
                {
                    Hostname,
                    Username,
                    Password,
                    Email,
                    Website,
                    Notes,
                    favIcon
                };
                string[] Columns = new string[]
                {
                    "D_host",
                    "D_uname",
                    "D_password",
                    "D_email",
                    "D_url",
                    "D_notes",
                    "D_icon"
                };
                for (int i = 0; i < Values.Length; i++)
                {
                    if (Values[i].Equals(""))
                    {
                        Values[i] = "\x01";
                    }
                    else
                    {
                        Values[i] = CryptoHelper.AESEncrypt(Values[i], GlobalVarPool.localAESkey);
                    }
                }
                string Query = "INSERT INTO Tbl_data (D_datetime";
                for (int i = 0; i < Columns.Count(); i++)
                {
                    Query += ", " + Columns[i];
                }
                Query += ") VALUES (\"" + DateTime + "\"";
                for (int i = 0; i < Values.Count(); i++)
                {
                    Query += ", \"" + Values[i] + "\"";
                }
                Query += ");";
                await DataBaseHelper.ModifyData(Query);
                if (GlobalVarPool.UserData == null)
                {
                    GlobalVarPool.UserData = new DataTable();
                    for (int i = 0; i < (int)ColumnCount.Tbl_data; i++)
                    {
                        GlobalVarPool.UserData.Columns.Add(i.ToString(), typeof(string));
                    }
                }
                //D_id, D_hid, D_datetime, D_host, D_uname, D_password, D_url, D_email, D_notes
                Task<List<string>> GetId = DataBaseHelper.GetDataAsList("SELECT D_id FROM Tbl_data ORDER BY D_id DESC LIMIT 1;", (int)ColumnCount.SingleColumn);
                List<string> IdList = await GetId;
                Invoke((MethodInvoker)delegate
                {
                    DataRow NewRow = GlobalVarPool.UserData.Rows.Add();
                    NewRow["0"] = IdList[0];
                    NewRow["1"] = "EMPTY";
                    NewRow["2"] = DateTime;
                    NewRow["3"] = Hostname.Equals("") ? "\x01" : Hostname;
                    NewRow["4"] = Username.Equals("") ? "\x01" : Username;
                    NewRow["5"] = Password.Equals("") ? "\x01" : Password;
                    NewRow["6"] = Website.Equals("") ? "\x01" : Website;
                    NewRow["7"] = Email.Equals("") ? "\x01" : Email;
                    NewRow["8"] = Notes.Equals("") ? "\x01" : Notes;
                    NewRow["9"] = favIcon;
                    ApplyFilter(PreviousColorPage);
                    AddPanelAdvancedImageButtonSave.Enabled = true;
                    AddPanelMain.SuspendLayout();
                    DataTableLayoutPanelMain.BringToFront();
                    DataTableLayoutPanelMain.ResumeLayout();
                    ResetFields();
                });
            }).Start();
        }

        private void AddPanelAdvancedImageButtonAbort_Click(object sender, EventArgs e)
        {
            AddPanelMain.SuspendLayout();
            DataTableLayoutPanelMain.BringToFront();
            DataTableLayoutPanelMain.ResumeLayout();
            ResetFields();
        }

        private void AddPanelAnimatedButtonCheckIcon_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(AddEditFieldHostname.TextTextBox))
            {
                CustomException.ThrowNew.GenericException("Please enter a valid Hostname first!");
                return;
            }
            AddPanelAnimatedButtonCheckIcon.Enabled = false;
            new Thread(delegate ()
            {
                string base64Img;
                try
                {
                    base64Img = pmdbs.Icon.Get(AddEditFieldWebsite.TextTextBox, true);
                    // base64Img = pmdbs.Icon.Temp();
                }
                catch
                {
                    base64Img = pmdbs.Icon.Generate(AddEditFieldHostname.TextTextBox);
                }
                byte[] iconBytes = Convert.FromBase64String(base64Img);
                Invoke((MethodInvoker)delegate
                {
                    using (MemoryStream ms = new MemoryStream(iconBytes, 0, iconBytes.Length))
                    {
                        Image icon = Image.FromStream(ms, true);
                        AddPictureBoxCheckIconIcon.Image = icon.GetThumbnailImage(350, 350, null, new IntPtr());
                        icon.Dispose();
                    }
                    IsDefaultIcon = false;
                    AddPanelAnimatedButtonCheckIcon.Enabled = true;
                });
            }).Start();
        }

        private void AddEditFieldPassword_TextBoxTextChanged(object sender, EventArgs e)
        {
            string password = AddEditFieldPassword.TextTextBox;
            if (string.IsNullOrEmpty(password))
            {
                AddLabelPasswordStrengthIndicator.Text = "Too short";
                AddPasswordStrengthIndicator.SetIndex(0);
                return;
            }
            Password.Result result = Password.Security.SimpleCheck(password);
            int strength = Array.IndexOf(new string[] { "F", "D-", "D", "D+", "C-", "C", "C+", "B-", "B", "B+", "A-", "A" }, result.Grade);
            AddPasswordStrengthIndicator.SetIndex(strength);
            AddLabelPasswordStrengthIndicator.Text = result.Complexity;
        }

        #endregion

        #region LoginPanel

        private bool LoginButtonDisabled = false;

        private void InitializeTransparency()
        {
            Bitmap bmp = new Bitmap(LoginPictureBoxOnlineMain.Width, LoginPictureBoxOnlineMain.Height);
            using (Graphics graph = Graphics.FromImage(bmp))
            {
                Rectangle ImageSize = new Rectangle(0, 0, LoginPictureBoxOnlineMain.Width, LoginPictureBoxOnlineMain.Height);
                graph.FillRectangle(new SolidBrush(Color.FromArgb(220, 17, 17, 17)), ImageSize);
            }
            this.LoginPictureBoxOnlineMain.Image = bmp;
            this.LoginPictureBoxRegisterMain.Image = bmp;
            this.LoginPictureBoxOfflineMain.Image = bmp;
            this.LoginPictureBoxLoadingMain.Image = bmp;
        }

        private void LoginLabelOnlineRegister_Click(object sender, EventArgs e)
        {
            LoginPictureBoxOnlineMain.SuspendLayout();
            LoginPictureBoxRegisterMain.BringToFront();
            LoginPictureBoxRegisterMain.ResumeLayout();
        }

        private void LoginLabelRegisterSignIn_Click(object sender, EventArgs e)
        {
            LoginPictureBoxRegisterMain.SuspendLayout();
            LoginPictureBoxOnlineMain.BringToFront();
            LoginPictureBoxOnlineMain.ResumeLayout();
        }

        private void LoginAnimatedButtonOnlineLogin_Click(object sender, EventArgs e)
        {
            CustomException.ThrowNew.GenericException();
        }

        private void LoginEditFieldOfflinePassword_EnterKeyPressed(object sender, EventArgs e)
        {
            LoginAnimatedButtonOfflineLogin_Click(sender, e);
        }

        private async void LoginAnimatedButtonOfflineLogin_Click(object sender, EventArgs e)
        {
            if (LoginButtonDisabled)
            {
                return;
            }
            LoginButtonDisabled = true;
            LoginLabelOfflineError.ForeColor = Color.FromArgb(17, 17, 17);
            string Password = LoginEditFieldOfflinePassword.TextTextBox;
            if (Password.Equals(""))
            {
                LoginLabelOfflineError.ForeColor = Color.Firebrick;
                LoginLabelOfflineError.Text = "Please enter a password!";
                LoginButtonDisabled = false;
                return;
            }
            LoginLunaProgressSpinnerFading.Start();
            LoginPictureBoxLoadingMain.ResumeLayout();
            LoginPictureBoxLoadingMain.BringToFront();
            LoginPictureBoxOfflineMain.SuspendLayout();
            LoginLoadingLabelDetails.Text = "Loading Saved Hash...";
            LoginLoadingLabelDetails.Text = "Hashing Password...";
            string Stage1PasswordHash = CryptoHelper.SHA256Hash(Password);
            Task<string> ScryptTask = Task.Run(() => CryptoHelper.ScryptHash(Stage1PasswordHash, GlobalVarPool.firstUsage));
            string Stage2PasswordHash = await ScryptTask;
            LoginLoadingLabelDetails.Text = "Checking Password...";
            if (!Stage2PasswordHash.Equals(GlobalVarPool.scryptHash))
            {
                LoginLabelOfflineError.ForeColor = Color.Firebrick;
                LoginLabelOfflineError.Text = "Wrong Password!";
                LoginButtonDisabled = false;
                LoginPictureBoxOfflineMain.ResumeLayout();
                LoginPictureBoxOfflineMain.BringToFront();
                LoginPictureBoxLoadingMain.SuspendLayout();
                LoginLunaProgressSpinnerFading.Stop();
                return;
            }
            GlobalVarPool.localAESkey = CryptoHelper.SHA256Hash(Stage1PasswordHash.Substring(32, 32));
            GlobalVarPool.onlinePassword = CryptoHelper.SHA256Hash(Stage1PasswordHash.Substring(0, 32));
            LoginLoadingLabelDetails.Text = "Decrypting Your Data... 0%";
            Task<DataTable> GetData = DataBaseHelper.GetDataAsDataTable("SELECT D_id, D_hid, D_datetime, D_host, D_uname, D_password, D_url, D_email, D_notes, D_icon FROM Tbl_data;", (int)ColumnCount.Tbl_data);
            GlobalVarPool.UserData = await GetData;
            int Columns = GlobalVarPool.UserData.Columns.Count;
            int RowCounter = 0;
            int Fields = (Columns - 3) * GlobalVarPool.UserData.Rows.Count;
            foreach (DataRow Row in GlobalVarPool.UserData.Rows)
            {
                for (int i = 3; i < Columns; i++)
                {
                    string FieldValue = Row[i].ToString();
                    if (!FieldValue.Equals("\x01"))
                    {
                        string decryptedData = CryptoHelper.AESDecrypt(FieldValue, GlobalVarPool.localAESkey);
                        Row.BeginEdit();
                        Row.SetField(i, decryptedData);
                        Row.EndEdit();
                    }
                    double Percentage = ((((double)RowCounter * ((double)Columns - (double)3)) + (double)i - 3) / (double)Fields) * (double)100;
                    double FinalPercentage = Math.Round(Percentage, 0, MidpointRounding.ToEven);
                    LoginLoadingLabelDetails.Text = "Decrypting Your Data... " + FinalPercentage.ToString() + "%";
                }
                RowCounter++;
            }
            LoginLoadingLabelDetails.Text = "Loading User Interface...";
            DataFlowLayoutPanelList.SuspendLayout();
            new Thread(delegate () {
                for (int i = 0; i < DataPerPage; i++)
                {
                    ListEntry listEntry = new ListEntry
                    {
                        BackColor = Color.White,
                        HostNameFont = new Font("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point, 0),
                        HostNameForeColor = SystemColors.ControlText,
                        Name = "listEntry",
                        Size = new Size(1041, 75),
                        TabIndex = 14,
                        TimeStampFont = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0),
                        TimeStampForeColor = SystemColors.ControlText,
                        UserNameFont = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, 0),
                        UserNameForeColor = SystemColors.ControlText,
                        ColorNormal = Color.White,
                        ColorHover = Colors.LightGray,
                        BackgroundColor = Color.White
                    };
                    listEntry.Hide();
                    Invoke((MethodInvoker)delegate
                    {
                        DataFlowLayoutPanelList.Controls.Add(listEntry);
                    });
                    entryList.Add(listEntry);
                    listEntry.OnClickEvent += ListEntry_Click;
                    DataFlowLayoutPanelList.SetFlowBreak(listEntry, true);
                }
                Invoke((MethodInvoker)delegate
                {
                    DataFlowLayoutPanelList.ResumeLayout();
                    FlowLayoutPanel1_Resize(this, null);
                    LoginLunaProgressSpinnerFading.Stop();
                    LoginPictureBoxOnlineMain.Dispose();
                    LoginPictureBoxOfflineMain.Dispose();
                    LoginPictureBoxRegisterMain.Dispose();
                    PanelMain.BringToFront();
                    PanelLogin.Dispose();
                    this.MinimumSize = MinSize;
                    this.MaximumSize = MaxSize;
                    this.MaximizeBox = true;
                    this.MinimizeBox = true;
                    InitFilterPanel();
                    ApplyFilter(0);
                });
            }).Start();
        }

        private async void LoginAnimatedButtonRegister_Click(object sender, EventArgs e)
        {
            if (LoginButtonDisabled)
            {
                return;
            }
            LoginButtonDisabled = true;
            string Password1 = LoginEditFieldRegisterPassword.TextTextBox;
            string Password2 = LoginEditFieldRegisterPassword2.TextTextBox;
            if (!Password1.Equals(Password2))
            {
                LoginLabelRegisterError.Visible = true;
                LoginLabelRegisterError.Text = "These passwords don't match!";
                LoginButtonDisabled = false;
                return;
            }
            Password.Result offlineResult = Password.Security.SimpleCheck(Password1);
            if (offlineResult.Score < 110)
                {
                using (ErrorForm errorForm = new ErrorForm("It should be at least \"Okay\".\n\nTry adding more numbers, special characters or even unicode characters to increase the password strength.", "Security Exception", "Your password is too weak!", true))
                {
                    errorForm.ShowDialog();
                }
                LoginLabelRegisterError.Visible = true;
                LoginLabelRegisterError.Text = "Password too weak.";
                LoginButtonDisabled = false;
                return;
            }
            LoginLunaProgressSpinnerFading.Start();
            LoginPictureBoxLoadingMain.ResumeLayout();
            LoginPictureBoxLoadingMain.BringToFront();
            LoginPictureBoxRegisterMain.SuspendLayout();
            LoginLoadingLabelDetails.Text = "Checking Password Strength ...";
            Task<Password.Result> GetResult = Password.Security.OnlineCheckAsync(Password1);
            Password.Result result = await GetResult;
            LoginLabelRegisterError.ForeColor = Color.Firebrick;
            switch (result.IsCompromised)
            {
                case 0:
                    {
                        break;
                    }
                case 1:
                    {
                        using (ErrorForm errorForm = new ErrorForm("This password is COMMONLY USED and has been leaked " + result.TimesSeen.ToString() + " times previously. ", "Security Warning", "Common password detected!", true, Resources.facepalm))
                        {
                            errorForm.ShowDialog();
                        }
                        LoginLunaProgressSpinnerFading.Stop();
                        LoginPictureBoxLoadingMain.SuspendLayout();
                        LoginPictureBoxRegisterMain.ResumeLayout();
                        LoginPictureBoxRegisterMain.BringToFront();
                        LoginButtonDisabled = false;
                        return;
                    }
                default:
                    {
                        // CONNECTION ERROR
                        bool actionIsConfirmed = true;
                        using (MetroFramework.Forms.MetroForm prompt = new ConfirmationForm("Failed to establish a connection to the following online service:\nPassword Leak Checker.\nIt could not be validated that your password is strong and has not been leaked previously.\n\nDo you want to continue anyway?","Security Warning", MessageBoxButtons.YesNo, true))
                        {
                            actionIsConfirmed = prompt.ShowDialog().Equals(DialogResult.OK);
                        }
                        if (!actionIsConfirmed)
                        {
                            LoginLunaProgressSpinnerFading.Stop();
                            LoginPictureBoxLoadingMain.SuspendLayout();
                            LoginPictureBoxRegisterMain.ResumeLayout();
                            LoginPictureBoxRegisterMain.BringToFront();
                            LoginButtonDisabled = false;
                            return;
                        }
                        break;
                    }
            }
            LoginLoadingLabelDetails.Text = "Hashing Password...";
            string Stage1PasswordHash = CryptoHelper.SHA256Hash(Password1);
            string FirstUsage = TimeConverter.TimeStamp();
            Task<String> ScryptTask = Task.Run(() => CryptoHelper.ScryptHash(Stage1PasswordHash, FirstUsage));
            string Stage2PasswordHash = await ScryptTask;
            LoginLoadingLabelDetails.Text = "Initializing Database...";
            await DataBaseHelper.ModifyData(DataBaseHelper.Security.SQLInjectionCheckQuery(new string[] { "INSERT INTO Tbl_user (U_password, U_wasOnline, U_firstUsage) VALUES (\"", Stage2PasswordHash, "\", 0, \"", FirstUsage, "\");" }));
            MasterPassword = Stage1PasswordHash;
            GlobalVarPool.localAESkey = CryptoHelper.SHA256Hash(Stage1PasswordHash.Substring(32, 32));
            GlobalVarPool.onlinePassword = CryptoHelper.SHA256Hash(Stage1PasswordHash.Substring(0, 32));
            LoginLunaProgressSpinnerFading.Stop();
            LoginPictureBoxOnlineMain.Dispose();
            LoginPictureBoxOfflineMain.Dispose();
            LoginPictureBoxRegisterMain.Dispose();
            PanelMain.BringToFront();
            PanelLogin.Dispose();
            InitFilterPanel();
            this.MinimumSize = MinSize;
            this.MaximumSize = MaxSize;
        }
        private void LoginEditFieldRegisterPassword2_TextBoxTextChanged(object sender, EventArgs e)
        {
            if (LoginLabelRegisterError.Visible)
            {
                LoginLabelRegisterError.Visible = false;
            }
        }
        private void LoginEditFieldRegisterPassword_TextBoxTextChanged(object sender, EventArgs e)
        {
            if (LoginLabelRegisterError.Visible)
            {
                LoginLabelRegisterError.Visible = false;
            }
            string password = LoginEditFieldRegisterPassword.TextTextBox;
            if (string.IsNullOrEmpty(password))
            {
                LoginLabelRegisterPasswordStrengthIndicator.Text = "...";
                LoginLabelRegisterPasswordStrengthIndicator.ForeColor = Color.White;
                LoginPasswordStrengthIndicatorRegister.SetIndex(0);
                return;
            }
            Password.Result result = Password.Security.SimpleCheck(password);
            int strength = Array.IndexOf(new string[] { "F", "D-", "D", "D+", "C-", "C", "C+", "B-", "B", "B+", "A-", "A" }, result.Grade);
            LoginPasswordStrengthIndicatorRegister.SetIndex(strength);
            LoginLabelRegisterPasswordStrengthIndicator.ForeColor = LoginPasswordStrengthIndicatorRegister.Colors[strength];
            LoginLabelRegisterPasswordStrengthIndicator.Text = result.Complexity;
        }

        #endregion

        #region SettingsPanel
        #region SettingsPrompt
        private void SettingsAnimatedButtonPromptSubmit_Click(object sender, EventArgs e)
        {
            string code = SettingsEditFieldPromptCode.TextTextBox;
            if (code.Equals(string.Empty))
            {
                SettingsLabelPromptCode.ForeColor = Color.Firebrick;
                SettingsLabelPromptCode.Text = "*Enter code (this field is required)";
                return;
            }
            if (!Regex.IsMatch(code, "^[0-9]{6}$"))
            {
                SettingsLabelPromptCode.ForeColor = Color.Firebrick;
                SettingsLabelPromptCode.Text = "*Enter code (6 digits)";
                return;
            }
            if (!GlobalVarPool.connected)
            {
                CustomException.ThrowNew.NetworkException("Not connected!");
                return;
            }
            if (string.IsNullOrEmpty(GlobalVarPool.promptCommand))
            {
                CustomException.ThrowNew.GenericException("User entered code but command has not been set!");
                return;
            }
            // DEEP COPY SCHEDULED TASKS
            List<AutomatedTaskFramework.Task> scheduledTasks = scheduledTasks = AutomatedTaskFramework.Tasks.GetAll().ConvertAll(task => new AutomatedTaskFramework.Task(task.SearchCondition, task.FinishedCondition, task.TaskAction));
            AutomatedTaskFramework.Tasks.Clear();
            switch (GlobalVarPool.promptCommand)
            {
                case "ACTIVATE_ACCOUNT":
                    {
                        AutomatedTaskFramework.Task.Create(SearchCondition.Contains, "ACCOUNT_VERIFIED", () => NetworkAdapter.MethodProvider.ActivateAccount(code));
                        AutomatedTaskFramework.Task.Create(SearchCondition.In, "ALREADY_LOGGED_IN|LOGIN_SUCCESSFUL", NetworkAdapter.MethodProvider.Login);
                        break;
                    }
                case "CONFIRM_NEW_DEVICE":
                    {
                        AutomatedTaskFramework.Task.Create(SearchCondition.Contains, "LOGIN_SUCCESSFUL", () => NetworkAdapter.MethodProvider.ConfirmNewDevice(code));
                        for (int i = 1; i < scheduledTasks.Count; i++)
                        {
                            AutomatedTaskFramework.Tasks.Add(scheduledTasks[i]);
                        }
                        break;
                    }
                case "VERIFY_PASSWORD_CHANGE":
                    {
                        AutomatedTaskFramework.Task.Create(SearchCondition.Contains, "PASSWORD_CHANGED", () => NetworkAdapter.MethodProvider.CommitPasswordChange(GlobalVarPool.plainMasterPassword, code));
                        break;
                    }
            }
            AutomatedTaskFramework.Tasks.Execute();
            SettingsPanelPromptMain.SendToBack();
            SettingsEditFieldPromptCode.TextTextBox = string.Empty;
            if (GlobalVarPool.promptFromBackgroundThread)
            {
                SettingsTableLayoutPanelMain.SendToBack();
                GlobalVarPool.promptFromBackgroundThread = false;
            }
        }

        private void SettingsLinkLabelPromptResendCode_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!GlobalVarPool.connected)
            {
                CustomException.ThrowNew.NetworkException("Not connected.");
                return;
            }
            NetworkAdapter.MethodProvider.ResendCode();
        }
        #endregion
        #region SettingsLogin
        private async void SettingsAnimatedButtonLoginSubmit_Click(object sender, EventArgs e)
        {
            string ip = SettingsEditFieldLoginIP.TextTextBox;
            string strPort = SettingsEditFieldLoginPort.TextTextBox;
            string username = SettingsEditFieldLoginUsername.TextTextBox;
            string password = SettingsEditFieldLoginPassword.TextTextBox;
            bool isIP = false;

            if (string.IsNullOrEmpty(ip))
            {
                CustomException.ThrowNew.FormatException("Please enter the IPv4 address or DNS of the server you'd like to connect to.");
                return;
            }
            if (string.IsNullOrEmpty(strPort))
            {
                CustomException.ThrowNew.FormatException("Please enter the port of the server you'd like to connect to.");
                return;
            }
            if (string.IsNullOrEmpty(username))
            {
                CustomException.ThrowNew.FormatException("Please enter your username.");
                return;
            }
            if (string.IsNullOrEmpty(password))
            {
                CustomException.ThrowNew.FormatException("Please enter your password.");
                return;
            }
            int port = Convert.ToInt32(strPort);
            if (port < 1 || port > 65536)
            {
                CustomException.ThrowNew.FormatException("Please enter a valid port number.");
                return;
            }
            if (Regex.IsMatch(ip, @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5]).){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$"))
            {
                isIP = true;
                GlobalVarPool.REMOTE_ADDRESS = ip;
            }
            string stage1PasswordHash = CryptoHelper.SHA256Hash(password);
            GlobalVarPool.onlinePassword = CryptoHelper.SHA256Hash(stage1PasswordHash.Substring(0, 32));
            GlobalVarPool.plainMasterPassword = password;
            GlobalVarPool.username = username;
            try
            {
                if (!isIP)
                {
                    Task<IPHostEntry> ipTask = Dns.GetHostEntryAsync(ip);
                    IPHostEntry ipAddress = await ipTask;
                    string ipv4String = ipAddress.AddressList.First().MapToIPv4().ToString();
                    GlobalVarPool.REMOTE_ADDRESS = ipv4String;
                }
                GlobalVarPool.REMOTE_PORT = port;
                GlobalVarPool.previousPanel = SettingsFlowLayoutPanelLogin;
                GlobalVarPool.loadingType = LoadingHelper.LoadingType.LOGIN;
                Func<bool> finishCondition = () => { return GlobalVarPool.isUser; };
                Thread t = new Thread(new ParameterizedThreadStart(LoadingHelper.Load))
                {
                    IsBackground = true
                };
                t.Start(new List<object> { SettingsFlowLayoutPanelOnline, SettingsLabelLoadingStatus, true, finishCondition
            });
                AutomatedTaskFramework.Tasks.Clear();
                if (GlobalVarPool.connected)
                {
                    AutomatedTaskFramework.Task.Create(SearchCondition.In, "ALREADY_LOGGED_IN|LOGIN_SUCCESSFUL", NetworkAdapter.MethodProvider.Login);
                }
                else
                {
                    AutomatedTaskFramework.Task.Create(SearchCondition.Contains, "DEVICE_AUTHORIZED", NetworkAdapter.MethodProvider.Connect);
                    AutomatedTaskFramework.Task.Create(SearchCondition.In, "ALREADY_LOGGED_IN|LOGIN_SUCCESSFUL", NetworkAdapter.MethodProvider.Login);
                }
                AutomatedTaskFramework.Tasks.Execute();
                await DataBaseHelper.ModifyData(DataBaseHelper.Security.SQLInjectionCheckQuery(new string[] { "UPDATE Tbl_settings SET S_server_ip = \"", GlobalVarPool.REMOTE_ADDRESS, "\", S_server_port = \"", GlobalVarPool.REMOTE_PORT.ToString(), "\";" }));
            }
            catch (Exception ex)
            {
                CustomException.ThrowNew.GenericException(ex.ToString());
            }
        }

        #endregion
        #region SettingsRegister
        private async void SettingsAnimatedButtonSubmit_Click(object sender, EventArgs e)
        {
            string ip = SettingsEditFieldRegisterIP.TextTextBox;
            string strPort = SettingsEditFieldRegisterPort.TextTextBox;
            string username = SettingsEditFieldRegisterUsername.TextTextBox;
            string email = SettingsEditFieldRegisterEmail.TextTextBox; 
            string nickname = SettingsEditFieldRegisterName.TextTextBox;
            bool isIP = false;
            if (string.IsNullOrEmpty(nickname))
            {
                nickname = "User";
            }
            if (string.IsNullOrEmpty(ip))
            {
                CustomException.ThrowNew.FormatException("Please enter the IPv4 address or DNS of the server you'd like to connect to.");
                return;
            }
            if (string.IsNullOrEmpty(strPort))
            {
                CustomException.ThrowNew.FormatException("Please enter the port of the server you'd like to connect to.");
                return;
            }
            if (string.IsNullOrEmpty(username))
            {
                CustomException.ThrowNew.FormatException("Please enter your username.");
                return;
            }
            if (new string[] { username, email, nickname}.Where(element => new string[] { " ", "\"", "'" }.Any(element.Contains)).Any())
            {
                CustomException.ThrowNew.FormatException("The username, nickname and email address may not contain spaces, single or double quotes.");
                return;
            }
            if (username.Contains("__"))
            {
                CustomException.ThrowNew.FormatException("The username may not contain double underscores.");
            }
            if (string.IsNullOrEmpty(email))
            {
                CustomException.ThrowNew.FormatException("Please enter your email address.");
                return;
            }
            // DAMN REGEX SYNTAX SUCKS ... TODO: REPLACE THIS WITH SOME ACTUALLY READABLE LINQ QUERY
            if (!Regex.IsMatch(email, @"^[^.][0-9a-zA-z\.!#$%&'*+-/=?^_`{|}~]+@[a-zA-Z0-9-\.]+\.[a-z]+$"))
            {
                CustomException.ThrowNew.FormatException("Please enter a valid email address.");
                return;
            }
            GlobalVarPool.name = nickname;
            GlobalVarPool.email = email;
            GlobalVarPool.username = username;
            int port = Convert.ToInt32(strPort);
            if (port < 1 || port > 65536)
            {
                CustomException.ThrowNew.FormatException("Please enter a valid port number.");
                return;
            }
            // MORE DISGUSTING REGEXES. THIS ONE DOESN'T EVEN WORK PROPERLY AS IT ALLOWS STUFF LIKE 1.1.1 AS IPv4 ADDRESSES --> TODO: LINQ <3
            if (Regex.IsMatch(ip, @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5]).){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$"))
            {
                isIP = true;
                GlobalVarPool.REMOTE_ADDRESS = ip;
            }
            try
            {
                if (!isIP)
                {
                    Task<IPHostEntry> ipTask = Dns.GetHostEntryAsync(ip);
                    IPHostEntry ipAddress = await ipTask;
                    string ipv4String = ipAddress.AddressList.First().ToString();
                    GlobalVarPool.REMOTE_ADDRESS = ipv4String;
                }
                GlobalVarPool.REMOTE_PORT = port;
                GlobalVarPool.previousPanel = SettingsFlowLayoutPanelRegister;
                GlobalVarPool.loadingType = LoadingHelper.LoadingType.REGISTER;
                Func<bool> finishCondition = () => { return GlobalVarPool.isUser; };
                Thread t = new Thread(new ParameterizedThreadStart(LoadingHelper.Load))
                {
                    IsBackground = true
                };
                t.Start(new List<object> { SettingsFlowLayoutPanelOnline, SettingsLabelLoadingStatus, true, finishCondition });
                AutomatedTaskFramework.Tasks.Clear();
                if (GlobalVarPool.connected)
                {
                    AutomatedTaskFramework.Task.Create(SearchCondition.Contains, "SEND_VERIFICATION_ACTIVATE_ACCOUNT", NetworkAdapter.MethodProvider.Register);
                }
                else
                {
                    AutomatedTaskFramework.Task.Create(SearchCondition.Contains, "DEVICE_AUTHORIZED", NetworkAdapter.MethodProvider.Connect);
                    AutomatedTaskFramework.Task.Create(SearchCondition.Contains, "SEND_VERIFICATION_ACTIVATE_ACCOUNT", NetworkAdapter.MethodProvider.Register);
                }
                AutomatedTaskFramework.Tasks.Execute();
                await DataBaseHelper.ModifyData(DataBaseHelper.Security.SQLInjectionCheckQuery(new string[] { "UPDATE Tbl_settings SET S_server_ip = \"", GlobalVarPool.REMOTE_ADDRESS, "\", S_server_port = \"", GlobalVarPool.REMOTE_PORT.ToString(), "\";" }));
            }
            catch
            {
                CustomException.ThrowNew.GenericException("Something went wrong.");
            }
        }
        #endregion
        #region SettingsOffline
        private void SettingsAnimatedButtonLogin_Click(object sender, EventArgs e)
        {
            SettingsFlowLayoutPanelLogin.BringToFront();
        }
        private void SettingsAnimatedButtonRegister_Click(object sender, EventArgs e)
        {
            SettingsFlowLayoutPanelRegister.BringToFront();
        }
        private async void SettingsAnimatedButtonChangePasswordSubmit_Click(object sender, EventArgs e)
        {
            // TODO: CHECK PASSWORD STRENGTH
            string password = SettingsEditFieldOfflineNewPassword.TextTextBox;
            string password2 = SettingsEditFieldOfflineNewPasswordConfirm.TextTextBox;
            if (!password.Equals(password2))
            {
                CustomException.ThrowNew.GenericException("These passwords don't match.");
                return;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                CustomException.ThrowNew.GenericException("Please enter a new master password.");
                return;
            }
            GlobalVarPool.loadingType = LoadingHelper.LoadingType.DEFAULT;
            Func<bool> finishCondition = () => { return GlobalVarPool.commandErrorCode == 0; };
            Thread t = new Thread(new ParameterizedThreadStart(LoadingHelper.Load))
            {
                IsBackground = true
            };
            t.Start(new List<object> { SettingsFlowLayoutPanelOffline, SettingsLabelLoadingStatus, true, finishCondition });
            await HelperMethods.ChangeMasterPassword(password, true);
            GlobalVarPool.commandErrorCode = 0;
        }

        private void SettingsAnimatedButtonOfflineChangeNameSubmit_Click(object sender, EventArgs e)
        {
            string name = GetNameOrDefault(SettingsEditFieldOfflineName.TextTextBox);
            ChangeLocalName(name);
        }

        #endregion
        #region SettingsOnline
        private void SettingsAnimatedButtonOnlinePasswordChangeSubmit_Click(object sender, EventArgs e)
        {
            // TODO: CHECK PASSWORD STRENGTH
            string password = SettingsEditFieldOnlinePasswordChangeNew.TextTextBox;
            string password2 = SettingsEditFieldOnlinePasswordChangeConfirm.TextTextBox;
            if (!password.Equals(password2))
            {
                CustomException.ThrowNew.GenericException("These passwords don't match.");
                return;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                CustomException.ThrowNew.GenericException("Please enter a new master password.");
                return;
            }
            GlobalVarPool.loadingType = LoadingHelper.LoadingType.PASSWORD_CHANGE;
            GlobalVarPool.plainMasterPassword = password;
            SettingsEditFieldOnlinePasswordChangeNew.TextTextBox = string.Empty;
            SettingsEditFieldOnlinePasswordChangeConfirm.TextTextBox = string.Empty;
            GlobalVarPool.previousPanel = SettingsFlowLayoutPanelOnline;
            Func<bool> finishCondition = () => { return GlobalVarPool.commandErrorCode == 0; };
            Thread t = new Thread(new ParameterizedThreadStart(LoadingHelper.Load))
            {
                IsBackground = true
            };
            t.Start(new List<object> { SettingsFlowLayoutPanelOnline, SettingsLabelLoadingStatus, true, finishCondition });
            AutomatedTaskFramework.Tasks.Clear();
            if (!GlobalVarPool.connected)
            {
                AutomatedTaskFramework.Task.Create(SearchCondition.Contains, "DEVICE_AUTHORIZED", NetworkAdapter.MethodProvider.Connect);
            }
            if (!GlobalVarPool.isUser)
            {
                AutomatedTaskFramework.Task.Create(SearchCondition.In, "ALREADY_LOGGED_IN|LOGIN_SUCCESSFUL", NetworkAdapter.MethodProvider.Login);
            }
            AutomatedTaskFramework.Task.Create(SearchCondition.Contains, "SEND_VERIFICATION_CHANGE_PASSWORD", NetworkAdapter.MethodProvider.InitPasswordChange);
            AutomatedTaskFramework.Tasks.Execute();
        }
        private void SettingsAnimatedButtonOnlineChangeName_Click(object sender, EventArgs e)
        {
            string name = GetNameOrDefault(SettingsEditFieldOnlineChangeName.TextTextBox);
            ChangeLocalName(name);
            GlobalVarPool.loadingType = LoadingHelper.LoadingType.DEFAULT;
            Func<bool> finishCondition = () => { return GlobalVarPool.commandErrorCode == 0; };
            Thread t = new Thread(new ParameterizedThreadStart(LoadingHelper.Load))
            {
                IsBackground = true
            };
            t.Start(new List<object> { SettingsFlowLayoutPanelOnline, SettingsLabelLoadingStatus, true, finishCondition });
            AutomatedTaskFramework.Tasks.Clear();
            if (!GlobalVarPool.connected)
            {
                AutomatedTaskFramework.Task.Create(SearchCondition.Contains, "DEVICE_AUTHORIZED", NetworkAdapter.MethodProvider.Connect);
            }
            if (!GlobalVarPool.isUser)
            {
                AutomatedTaskFramework.Task.Create(SearchCondition.In, "ALREADY_LOGGED_IN|LOGIN_SUCCESSFUL", NetworkAdapter.MethodProvider.Login);
            }
            AutomatedTaskFramework.Task.Create(SearchCondition.Contains, "NAME_CHANGED", () => NetworkAdapter.MethodProvider.ChangeName(name));
            AutomatedTaskFramework.Tasks.Execute();
        }
        #endregion
        private async void ChangeLocalName(string name)
        {
            await DataBaseHelper.ModifyData(DataBaseHelper.Security.SQLInjectionCheckQuery(new string[] { "UPDATE Tbl_user SET U_name = \"", name, "\";" }));
            GlobalVarPool.name = name;
            RefreshSettings();
        }
        private string GetNameOrDefault(string name)
        {
            return string.IsNullOrWhiteSpace(name)? "User" : name;
        }

        private void RefreshSettings()
        {
            if (GlobalVarPool.wasOnline)
            {
                SettingsEditFieldOnlineChangeName.TextTextBox = GlobalVarPool.name;
                SettingsEditFieldRegisterIP.TextTextBox = GlobalVarPool.REMOTE_ADDRESS;
                SettingsEditFieldRegisterPort.TextTextBox = GlobalVarPool.REMOTE_PORT.ToString();
                SettingsEditFieldLoginIP.TextTextBox = GlobalVarPool.REMOTE_ADDRESS;
                SettingsEditFieldLoginPort.TextTextBox = GlobalVarPool.REMOTE_PORT.ToString();
            }
            else
            {
                SettingsEditFieldOfflineName.TextTextBox = GlobalVarPool.name;
            }
        }
        #endregion

        #region FilterPanel
        private string previousTextBoxContent = string.Empty;
        private int previousIndex = 2;
        private bool indexForceChange = false;
        private bool textForceChange = false;

        private void InitFilterPanel()
        {
            FilterAdvancedComboBoxSort.SelectedIndex = 2;
        }
        private void FilterEditFieldSearch_TextChanged(object sender, EventArgs e)
        {
            if (textForceChange)
            {
                textForceChange = false;
                return;
            }
            if (EditFieldShown)
            {
                // USE LINQ TO CHECK IF THERE ARE UNSAVED CHANGES
                if (!GlobalVarPool.UserData.AsEnumerable().Where(row => row["0"].ToString().Equals(DataDetailsID)).Where(row => (row["3"].ToString().Equals(DataEditEditFieldHostname.TextTextBox) || (row["3"].ToString().Equals("\x01") && string.IsNullOrEmpty(DataEditEditFieldHostname.TextTextBox))) && (row["4"].ToString().Equals(DataEditEditFieldUsername.TextTextBox) || (row["4"].ToString().Equals("\x01") && string.IsNullOrEmpty(DataEditEditFieldUsername.TextTextBox))) && (row["5"].ToString().Equals(DataEditEditFieldPassword.TextTextBox) || (row["5"].ToString().Equals("\x01") && string.IsNullOrEmpty(DataEditEditFieldPassword.TextTextBox))) && (row["6"].ToString().Equals(DataEditEditFieldWebsite.TextTextBox) || (row["6"].ToString().Equals("\x01") && string.IsNullOrEmpty(DataEditEditFieldWebsite.TextTextBox))) && (row["7"].ToString().Equals(DataEditEditFieldEmail.TextTextBox) || row["7"].ToString().Equals("\x01") && (string.IsNullOrEmpty(DataEditEditFieldEmail.TextTextBox))) && (row["8"].ToString().Equals(DataEditAdvancedRichTextBoxNotes.TextValue) || (row["8"].ToString().Equals("\x01") && string.IsNullOrEmpty(DataEditAdvancedRichTextBoxNotes.TextValue)))).Any())
                {
                    bool actionIsConfirmed = false;
                    using (MetroFramework.Forms.MetroForm prompt = new ConfirmationForm("There are unsaved changes. Do you really want to discard them?", MessageBoxButtons.YesNo))
                    {
                        actionIsConfirmed = prompt.ShowDialog().Equals(DialogResult.OK);
                    }
                    if (!actionIsConfirmed)
                    {
                        textForceChange = true;
                        FilterEditFieldSearch.TextTextBox = previousTextBoxContent;
                        return;
                    }
                }
                EditFieldShown = false;
            }
            string textBoxContent = FilterEditFieldSearch.TextTextBox;
            if (!textBoxContent.Equals(previousTextBoxContent))
            {
                previousTextBoxContent = textBoxContent;
                // Do things
                ApplyFilter(0);
            }
        }
        private void FilterAdvancedComboBoxSort_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (indexForceChange)
            {
                indexForceChange = false;
                return;
            }
            if (EditFieldShown)
            {
                // USE LINQ TO CHECK IF THERE ARE UNSAVED CHANGES
                if (!GlobalVarPool.UserData.AsEnumerable().Where(row => row["0"].ToString().Equals(DataDetailsID)).Where(row => (row["3"].ToString().Equals(DataEditEditFieldHostname.TextTextBox) || (row["3"].ToString().Equals("\x01") && string.IsNullOrEmpty(DataEditEditFieldHostname.TextTextBox))) && (row["4"].ToString().Equals(DataEditEditFieldUsername.TextTextBox) || (row["4"].ToString().Equals("\x01") && string.IsNullOrEmpty(DataEditEditFieldUsername.TextTextBox))) && (row["5"].ToString().Equals(DataEditEditFieldPassword.TextTextBox) || (row["5"].ToString().Equals("\x01") && string.IsNullOrEmpty(DataEditEditFieldPassword.TextTextBox))) && (row["6"].ToString().Equals(DataEditEditFieldWebsite.TextTextBox) || (row["6"].ToString().Equals("\x01") && string.IsNullOrEmpty(DataEditEditFieldWebsite.TextTextBox))) && (row["7"].ToString().Equals(DataEditEditFieldEmail.TextTextBox) || row["7"].ToString().Equals("\x01") && (string.IsNullOrEmpty(DataEditEditFieldEmail.TextTextBox))) && (row["8"].ToString().Equals(DataEditAdvancedRichTextBoxNotes.TextValue) || (row["8"].ToString().Equals("\x01") && string.IsNullOrEmpty(DataEditAdvancedRichTextBoxNotes.TextValue)))).Any())
                {
                    bool actionIsConfirmed = false;
                    using (MetroFramework.Forms.MetroForm prompt = new ConfirmationForm("There are unsaved changes. Do you really want to discard them?", MessageBoxButtons.YesNo))
                    {
                        actionIsConfirmed = prompt.ShowDialog().Equals(DialogResult.OK);
                    }
                    if (!actionIsConfirmed)
                    {
                        indexForceChange = true;
                        FilterAdvancedComboBoxSort.SelectedIndex = previousIndex;
                        return;
                    }
                }
                EditFieldShown = false;
            }
            int selectedIndex = FilterAdvancedComboBoxSort.SelectedIndex;
            if (GlobalVarPool.UserData.Rows.Count != 0 && selectedIndex != previousIndex)
            {
                previousIndex = selectedIndex;
                ApplyFilter(0);
            }
        }
        private void ApplyFilter(int page)
        {
            GlobalVarPool.FilteredUserData = GlobalVarPool.UserData.Copy();
            string searchTerm = FilterEditFieldSearch.TextTextBox;
            if (!string.IsNullOrWhiteSpace(searchTerm) && !searchTerm.Equals(FilterEditFieldSearch.DefaultValue))
            {
                EnumerableRowCollection<DataRow> searchResults = GlobalVarPool.FilteredUserData.AsEnumerable().Where(row => row["3"].ToString().StartsWith(searchTerm));
                if (searchResults.Any())
                {
                    DataFilterResultsLabelSearchTerm.Text = searchTerm;
                    DataFilterResultsLabelSortTerm.Text = FilterAdvancedComboBoxSort.Items[FilterAdvancedComboBoxSort.SelectedIndex].ToString();
                    DataFilterResultsPanel.BringToFront();
                    GlobalVarPool.FilteredUserData = searchResults.CopyToDataTable();
                }
                else
                {
                    DataFilterLabelSearchTerm.Text = searchTerm;
                    DataFilterPanelNotFound.BringToFront();
                    GlobalVarPool.FilteredUserData = new DataTable();
                }
            }
            else
            {
                DataPanelNoSel.BringToFront();
            }
            if (GlobalVarPool.FilteredUserData.Rows.Count > 0)
            {
                switch (FilterAdvancedComboBoxSort.SelectedIndex)
                {
                    case 0:
                        {
                            GlobalVarPool.FilteredUserData = GlobalVarPool.FilteredUserData.AsEnumerable().OrderBy(row => row["3"]).CopyToDataTable();
                            break;
                        }
                    case 1:
                        {
                            GlobalVarPool.FilteredUserData = GlobalVarPool.FilteredUserData.AsEnumerable().OrderByDescending(row => row["3"]).CopyToDataTable();
                            break;
                        }
                    case 2:
                        {
                            GlobalVarPool.FilteredUserData = GlobalVarPool.FilteredUserData.AsEnumerable().OrderBy(row => row["2"]).CopyToDataTable();
                            break;
                        }
                    case 3:
                        {
                            GlobalVarPool.FilteredUserData = GlobalVarPool.FilteredUserData.AsEnumerable().OrderByDescending(row => row["2"]).CopyToDataTable();
                            break;
                        }
                    default:
                        {
                            GlobalVarPool.FilteredUserData = GlobalVarPool.FilteredUserData.AsEnumerable().OrderBy(row => row["2"]).CopyToDataTable();
                            break;
                        }
                }
            }
            RefreshUserData(page);
        }


        #endregion

        #region Dashboard
        private int t = 0;
        private void animatedButton1_Click(object sender, EventArgs e)
        {
            lunaItemList1.Add("Windows 7 SP1", Resources.devices_colored_windows, t.ToString(), t);
            t++;
        }

        private void card_click(object sender, EventArgs e)
        {
            LunaItem item = (LunaItem)sender;
            CustomException.ThrowNew.NotImplementedException("item[" + item.Id.ToString() + "]");
        }
        #endregion
        private List<Breaches.Breach> breaches;
        private void label16_Click(object sender, EventArgs e)
        {
            UpdateBreaches();
        }

        private async void UpdateBreaches()
        {
            DashboardLunaItemListBreaches.RemoveAll();
            List<string> domains = new List<string>();
            int rowCount = GlobalVarPool.UserData.Rows.Count;
            for (int i = 0; i < rowCount; i++)
            {
                string domain = HttpHelper.GetDomain(GlobalVarPool.UserData.Rows[i]["6"].ToString());
                if (domain != null)
                {
                    domains.Add(domain);
                }
            }
            try
            {
                Task<List<Breaches.Breach>> GetBreaches = Breaches.FetchAllAsync();
                breaches = await GetBreaches;
            }
            catch (Exception ex)
            {
                CustomException.ThrowNew.NetworkException(ex.ToString());
                return;
            }
            if (breaches == null)
            {
                // TODO: EMBED ERROR MESSAGE INTO UI
                CustomException.ThrowNew.NetworkException("Connection failure.");
                return;
            }
            Task<List<string>> GetIgnoredBreaches = DataBaseHelper.GetDataAsList("SELECT B_hash FROM Tbl_breaches;", (int)ColumnCount.SingleColumn);
            List<string> ignoredBreaches = await GetIgnoredBreaches;
            int index = 0;
            for (int i = 0; i < breaches.Count; i++)
            {
                Breaches.Breach breach = breaches[i];
                if (domains.Contains(breach.Domain))
                {
                    string hash = CryptoHelper.SHA256HashBase64(breach.Name + breach.BreachDate);
                    if (!ignoredBreaches.Contains(hash))
                    {
                        DashboardLunaItemListBreaches.Add(breach.Title, Resources.exclamation_mark, breach.BreachDate, hash, i, index);
                        index++;
                    }
                }
            }
        }

        private async void Breach_Clicked(object sender, EventArgs e)
        {
            LunaItem item = (LunaItem)sender;
            Breaches.Breach breach = breaches[item.Index];
            Task<Image> GetIcon = pmdbs.Icon.GetFromUrlAsync(breach.LogoPath);
            Image icon = await GetIcon;
            DialogResult result = new BreachForm(breach.BreachDate, breach.Title, icon ?? Resources.breach, breach.DataClasses, breach.Description, breach.PwnCount, breach.IsVerified, breach.Domain).ShowDialog();
            if (result == DialogResult.Ignore)
            {
                await DataBaseHelper.ModifyData("INSERT INTO Tbl_breaches (B_hash) VALUES (\"" + item.Id + "\");");
                DashboardLunaItemListBreaches.RemoveAt(item.Index2);
            }
        }

        
    }
}