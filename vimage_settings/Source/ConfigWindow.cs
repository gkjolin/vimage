﻿using System;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using vimage;

namespace vimage_settings
{
    public partial class ConfigWindow : Form
    {
        private Config vimageConfig;

        public List<ControlItem> ControlItems = new List<ControlItem>();
        public List<ContextMenuItem> ContextMenuItems = new List<ContextMenuItem>();
        public List<ContextMenuItem> ContextMenuItems_Animation = new List<ContextMenuItem>();
        public List<CustomActionItem> CustomActionItems = new List<CustomActionItem>();
        public List<ControlItem> CustomActionBindings = new List<ControlItem>();

        public ContextMenuItem ContextMenuItemFocused;

        public ConfigWindow()
        {
            InitializeComponent();

            linkLabel1.TabStop = false; // won't set to false in the Designer.cs for weird reason

            // Load Config File
            vimageConfig = new Config();
            vimageConfig.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.txt"));

            // Set Settings
            checkBox_OpenAtMousePosition.Checked = vimageConfig.Setting_OpenAtMousePosition;
            checkBox_SmoothingDefault.Checked = vimageConfig.Setting_SmoothingDefault;
            checkBox_BackgroundForImagesWithTransparencyDefault.Checked = vimageConfig.Setting_BackgroundForImagesWithTransparencyDefault;
            checkBox_PositionLargeWideImagesInCorner.Checked = vimageConfig.Setting_PositionLargeWideImagesInCorner;
            checkBox_PreloadNextImage.Checked = vimageConfig.Setting_PreloadNextImage;
            checkBox_OpenSettingsEXE.Checked = vimageConfig.Setting_OpenSettingsEXE;
            checkBox_ListenForConfigChanges.Checked = vimageConfig.Setting_ListenForConfigChanges;
            comboBox_LimitImagesToMonitor.SelectedIndex = vimageConfig.Setting_LimitImagesToMonitor;
            numericUpDown_MinImageSize.Value = vimageConfig.Setting_MinImageSize;
            numericUpDown_SmoothingMinImageSize.Value = vimageConfig.Setting_SmoothingMinImageSize;
            numericUpDown_ZoomSpeed.Value = vimageConfig.Setting_ZoomSpeed;
            numericUpDown_ZoomSpeedFast.Value = vimageConfig.Setting_ZoomSpeedFast;
            comboBox_DefaultSortBy.SelectedIndex = (int)vimageConfig.Setting_DefaultSortBy;
            comboBox_DefaultSortDir.SelectedIndex = (int)vimageConfig.Setting_DefaultSortDir;

            // Setup Control Bindings
            AddControlItem("Drag", vimageConfig.Control_Drag);
            AddControlItem("Close", vimageConfig.Control_Close);
            AddControlItem("Open Context Menu", vimageConfig.Control_OpenContextMenu);
            AddControlItem("Prev Image", vimageConfig.Control_PrevImage);
            AddControlItem("Next Image", vimageConfig.Control_NextImage);
            AddControlItem("Rotate Clockwise", vimageConfig.Control_RotateClockwise);
            AddControlItem("Rotate Anti-Clockwise", vimageConfig.Control_RotateAntiClockwise);
            AddControlItem("Flip", vimageConfig.Control_Flip);
            AddControlItem("Fit To Monitor Auto", vimageConfig.Control_FitToMonitorAuto);
            AddControlItem("Fit To Monitor Width", vimageConfig.Control_FitToMonitorWidth);
            AddControlItem("Fit To Monitor Height", vimageConfig.Control_FitToMonitorHeight);
            AddControlItem("Fit To Monitor Alt", vimageConfig.Control_FitToMonitorAlt);
            AddControlItem("Zoom Faster", vimageConfig.Control_ZoomFaster);
            AddControlItem("Zoom Alt", vimageConfig.Control_ZoomAlt);
            AddControlItem("Toggle Smoothing", vimageConfig.Control_ToggleSmoothing);
            AddControlItem("Toggle Background For Transparency", vimageConfig.Control_ToggleBackgroundForTransparency);
            AddControlItem("Toggle Always On Top", vimageConfig.Control_ToggleAlwaysOnTop);
            AddControlItem("Pause Animation", vimageConfig.Control_PauseAnimation);
            AddControlItem("Prev Frame", vimageConfig.Control_PrevFrame);
            AddControlItem("Next Frame", vimageConfig.Control_NextFrame);
            AddControlItem("Open Config", vimageConfig.Control_OpenConfig);
            AddControlItem("Reload Config", vimageConfig.Control_ReloadConfig);
            AddControlItem("Reset Image", vimageConfig.Control_ResetImage);
            AddControlItem("Open At Location", vimageConfig.Control_OpenAtLocation);
            AddControlItem("Delete", vimageConfig.Control_Delete);
            AddControlItem("Copy", vimageConfig.Control_Copy);
            AddControlItem("Copy as Image", vimageConfig.Control_CopyAsImage);
            AddControlItem("Open Duplicate Image", vimageConfig.Control_OpenDuplicateImage);
            AddControlItem("Random Image", vimageConfig.Control_RandomImage);

            // Setup Context Menu Editor
            checkBox_ContextMenuShowMargin.Checked = vimageConfig.ContextMenuShowMargin;
            numericUpDown_ContextMenu_Animation_InsertAtIndex.Value = vimageConfig.ContextMenu_Animation_InsertAtIndex;

            AddContextMenuItems(vimageConfig.ContextMenu);
            tabControl_ContextMenus.SelectedIndex = 1;
            AddContextMenuItems(vimageConfig.ContextMenu_Animation);
            tabControl_ContextMenus.SelectedIndex = 0;

            if (ContextMenuItems.Count > 0)
                ContextMenuItems[0].GiveItemFocus();

            AddCustomActionItems(vimageConfig.CustomActions);
            AddCustomActionBindings(vimageConfig.CustomActionBindings);
        }

        private ControlItem AddControlItem(string name, List<int> control)
        {
            ControlItem item = new ControlItem(name, control);
            panel_Controls.Controls.Add(item);
            panel_Controls.Controls.SetChildIndex(item, 0);
            ControlItems.Add(item);

            return item;
        }
        private void RemoveControlitem(ControlItem item)
        {
            panel_Controls.Controls.Remove(item);
            ControlItems.Remove(item);
        }

        private void AddContextMenuItems(List<object> contextItems, int depth = 0)
        {
            for (int i = 0; i < contextItems.Count; i++)
            {
                if (contextItems[i] is List<object>)
                    AddContextMenuItems(contextItems[i] as List<object>, depth + 1); // submenu items
                else if (contextItems[i] is string)
                    AddContextMenuItem(contextItems[i] as string, "", depth, true); // submenu
                else
                    AddContextMenuItem((contextItems[i] as dynamic).name, (contextItems[i] as dynamic).func, depth); // item
            }
        }
        private ContextMenuItem AddContextMenuItem(string name = "", string func = "", int subitem = 0, bool submenu = false, int position = -1)
        {
            List<ContextMenuItem> CurrentList = GetContextMenuList();

            ContextMenuItem item = new ContextMenuItem(name, func, vimageConfig);
            item.Location = new System.Drawing.Point(0, (item.Height - 2) * CurrentList.Count);

            GetCurrentContextMenuPanel().Controls.Add(item);

            if (position == -1)
            {
                // add to bottom
                CurrentList.Add(item);
            }
            else
            {
                // add at position
                CurrentList.Insert(position, item);
                RefreshContextMenuItems();
            }

            item.AddConfigWindowReference(this);
            item.SetSubitem(subitem);
            item.SetSubmenu(submenu);

            return item;
        }
        public void RefreshContextMenuItems(int scrollValue = -1)
        {
            List<ContextMenuItem> CurrentList = GetContextMenuList();

            // scroll back to top temporarily
            if (scrollValue == -1)
                scrollValue = GetCurrentContextMenuPanel().VerticalScroll.Value;
            GetCurrentContextMenuPanel().VerticalScroll.Value = 0;

            // refresh positions of each item
            for (int i = 0; i < CurrentList.Count; i++)
                CurrentList[i].Location = new System.Drawing.Point(0, (CurrentList[i].Height - 2) * i);
            
            // scroll back
            GetCurrentContextMenuPanel().VerticalScroll.Value = Math.Min(scrollValue, GetCurrentContextMenuPanel().VerticalScroll.Maximum);
            GetCurrentContextMenuPanel().PerformLayout();
        }
        
        public List<ContextMenuItem> GetContextMenuList() { return tabControl_ContextMenus.SelectedIndex == 1 ? ContextMenuItems_Animation : ContextMenuItems; }
        public TabPage GetCurrentContextMenuPanel() { return tabControl_ContextMenus.SelectedTab; }
        public int GetContextMenuPanelScrollValue() { return (int)GetCurrentContextMenuPanel().VerticalScroll.Value; }
        public int GetCustomActionPanelScrollValue() { return (int)panel_CustomActions.VerticalScroll.Value; }

        private void AddCustomActionItems(List<object> customActions)
        {
            for (int i = 0; i < customActions.Count; i++)
                AddCustomActionItem((customActions[i] as dynamic).name, (customActions[i] as dynamic).func);
        }
        private CustomActionItem AddCustomActionItem(string name = "", string func = "", int position = -1)
        {
            CustomActionItem item = new CustomActionItem(name, func);

            panel_CustomActions.Controls.Add(item);
            panel_CustomActions.Controls.SetChildIndex(item, 0);

            CustomActionItems.Add(item);
            item.AddConfigWindowReference(this);

            return item;
        }
        public void RefreshCustomAcionItems(int scrollValue = -1)
        {
            // scroll back to top temporarily
            if (scrollValue == -1)
                scrollValue = panel_CustomActions.VerticalScroll.Value;
            panel_CustomActions.VerticalScroll.Value = 0;

            // refresh positions of each item
            for (int i = 0; i < CustomActionItems.Count; i++)
                CustomActionItems[i].Location = new System.Drawing.Point(0, (CustomActionItems[i].Height - 2) * i);

            // scroll back
            panel_CustomActions.VerticalScroll.Value = Math.Min(scrollValue, panel_CustomActions.VerticalScroll.Maximum);
            panel_CustomActions.PerformLayout();
        }
        
        private void AddCustomActionBindings(List<object> customActionBindings)
        {
            for (int i = 0; i < customActionBindings.Count; i++)
                CustomActionBindings.Add(AddControlItem((customActionBindings[i] as dynamic).name, (customActionBindings[i] as dynamic).bindings));
        }

        private void button_Save_Click(object sender, EventArgs e)
        {
            // Update Values
            vimageConfig.Setting_OpenAtMousePosition = checkBox_OpenAtMousePosition.Checked;
            vimageConfig.Setting_SmoothingDefault = checkBox_SmoothingDefault.Checked;
            vimageConfig.Setting_BackgroundForImagesWithTransparencyDefault = checkBox_BackgroundForImagesWithTransparencyDefault.Checked;
            vimageConfig.Setting_PositionLargeWideImagesInCorner = checkBox_PositionLargeWideImagesInCorner.Checked;
            vimageConfig.Setting_PreloadNextImage = checkBox_PreloadNextImage.Checked;
            vimageConfig.Setting_OpenSettingsEXE = checkBox_OpenSettingsEXE.Checked;
            vimageConfig.Setting_ListenForConfigChanges = checkBox_ListenForConfigChanges.Checked;
            vimageConfig.Setting_LimitImagesToMonitor = comboBox_LimitImagesToMonitor.SelectedIndex;
            vimageConfig.Setting_MinImageSize = (int)numericUpDown_MinImageSize.Value;
            vimageConfig.Setting_SmoothingMinImageSize = (int)numericUpDown_SmoothingMinImageSize.Value;
            vimageConfig.Setting_ZoomSpeed = (int)numericUpDown_ZoomSpeed.Value;
            vimageConfig.Setting_ZoomSpeedFast = (int)numericUpDown_ZoomSpeedFast.Value;
            vimageConfig.Setting_DefaultSortBy = (SortBy)comboBox_DefaultSortBy.SelectedIndex;
            vimageConfig.Setting_DefaultSortDir = (SortDirection)comboBox_DefaultSortDir.SelectedIndex;

            vimageConfig.ContextMenuShowMargin = checkBox_ContextMenuShowMargin.Checked;
            vimageConfig.ContextMenu_Animation_InsertAtIndex = (int)numericUpDown_ContextMenu_Animation_InsertAtIndex.Value;

            // Update Context Menu
            vimageConfig.ContextMenu.Clear();
            SaveContextMenu(vimageConfig.ContextMenu, ContextMenuItems);
            vimageConfig.ContextMenu_Animation.Clear();
            SaveContextMenu(vimageConfig.ContextMenu_Animation, ContextMenuItems_Animation);

            // Update Custom Actions
            SaveCustomActions(vimageConfig.CustomActions, CustomActionItems, vimageConfig.CustomActionBindings);

            // Save Config File
            vimageConfig.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.txt"));
        }
        private void SaveContextMenu(List<object> contextMenu, List<ContextMenuItem> contextMenuItems)
        {
            int currentSubLevel = 0;
            List<object> currentMenu = contextMenu;
            for (int i = 0; i < contextMenuItems.Count; i++)
            {
                if (contextMenuItems[i].Submenu)
                {
                    // Submenu
                    currentMenu.Add(contextMenuItems[i].GetName());
                }
                else if (contextMenuItems[i].Subitem != 0)
                {
                    // Subitem
                    if (contextMenuItems[i].Subitem != currentSubLevel)
                    {
                        // First subitem
                        currentSubLevel = contextMenuItems[i].Subitem;
                        currentMenu.Add(new List<object>());
                        currentMenu = currentMenu[currentMenu.Count - 1] as List<object>;
                    }

                    currentMenu.Add(new { name = contextMenuItems[i].GetName(), func = contextMenuItems[i].GetFunc() });
                }
                else
                {
                    // Item
                    if (currentSubLevel != 0)
                    {
                        currentSubLevel = 0;
                        currentMenu = contextMenu;
                    }

                    currentMenu.Add(new { name = contextMenuItems[i].GetName(), func = contextMenuItems[i].GetFunc() });
                }
            }
        }
        private void SaveCustomActions(List<object> customActions, List<CustomActionItem> customActionitems, List<object> customActionBindings)
        {
            for (int i = 0; i < CustomActionBindings.Count; i++)
                RemoveControlitem(CustomActionBindings[i]);
            CustomActionBindings.Clear();

            List<object> oldCustomActionBindings = new List<object>(customActionBindings);
            customActions.Clear();
            customActionBindings.Clear();

            for (int i = 0; i < customActionitems.Count; i++)
            {
                customActions.Add(new { name = customActionitems[i].GetName(), func = customActionitems[i].GetFunc() });
                customActionBindings.Add(new { name = customActionitems[i].GetName(), bindings = i < oldCustomActionBindings.Count ? (oldCustomActionBindings[i] as dynamic).bindings : new List<int>() });
            }

            // Update Context Menu functions list
            for (int c = 0; c < ContextMenuItems.Count; c++)
                ContextMenuItems[c].RefreshFunctions(vimageConfig);
            for (int c = 0; c < ContextMenuItems_Animation.Count; c++)
                ContextMenuItems_Animation[c].RefreshFunctions(vimageConfig);

            // Update Controls list
            AddCustomActionBindings(customActionBindings);
        }

        private void button_ControlsDefault_Click(object sender, EventArgs e)
        {
            // Reset Controls to Default
            vimageConfig.SetDefaultControls();

            foreach (ControlItem item in ControlItems)
                item.UpdateBindings();

            panel_Controls.Focus();
        }

        private void button_ContextMenuDefault_Click(object sender, EventArgs e)
        {
            // Reset Context Menu Setup to Default
            vimageConfig.SetDefaultContextMenu();

            // Clear items
            if (ContextMenuItems.Count != 0)
                ContextMenuItems[0].Parent.Controls.Clear();
            if (ContextMenuItems_Animation.Count != 0)
                ContextMenuItems_Animation[0].Parent.Controls.Clear();
            ContextMenuItems.Clear();
            ContextMenuItems_Animation.Clear();

            // Scroll back to top
            for (int i = 0; i < tabControl_ContextMenus.TabCount; i++)
                tabControl_ContextMenus.TabPages[i].VerticalScroll.Value = 0;

            int currentTab = tabControl_ContextMenus.SelectedIndex;

            // Add items
            tabControl_ContextMenus.SelectedIndex = 0;
            AddContextMenuItems(vimageConfig.ContextMenu);
            tabControl_ContextMenus.SelectedIndex = 1;
            AddContextMenuItems(vimageConfig.ContextMenu_Animation);

            tabControl_ContextMenus.SelectedIndex = currentTab;

            if (ContextMenuItems.Count > 0)
                ContextMenuItems[0].GiveItemFocus();
        }

        private void button_ContextMenuAddNew_Click(object sender, EventArgs e)
        {
            ContextMenuItem item;
            List<ContextMenuItem> CurrentList = GetContextMenuList();

            int focusedIndex = ContextMenuItemFocused == null ? -1 : CurrentList.IndexOf(ContextMenuItemFocused);
            if (focusedIndex == -1 || focusedIndex == CurrentList.Count - 1)
            {
                // scroll back to top temporarily
                GetCurrentContextMenuPanel().VerticalScroll.Visible = false;
                GetCurrentContextMenuPanel().VerticalScroll.Value = 0;

                // add item to bottom
                item = AddContextMenuItem("-", "-");

                // scroll to bottom
                GetCurrentContextMenuPanel().VerticalScroll.Visible = true;
                GetCurrentContextMenuPanel().VerticalScroll.Value = GetCurrentContextMenuPanel().VerticalScroll.Maximum;
                GetCurrentContextMenuPanel().PerformLayout();
            }
            else
                item = AddContextMenuItem("-", "-", ContextMenuItemFocused.Subitem, false, focusedIndex + 1);

            item.GiveItemFocus();
        }
        private void button_CustomActionsAddNew_Click(object sender, EventArgs e)
        {
            AddCustomActionItem();

            // scroll to bottom
            GetCurrentContextMenuPanel().VerticalScroll.Visible = true;
            GetCurrentContextMenuPanel().VerticalScroll.Value = GetCurrentContextMenuPanel().VerticalScroll.Maximum;
            GetCurrentContextMenuPanel().PerformLayout();
        }
        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            // focus on tab change to allow for scrolling with mouse wheel
            TabControl.SelectedTab.Focus();
            if (TabControl.SelectedTab == tabControls)
                panel_Controls.Focus();
            else if (TabControl.SelectedTab == tabContextMenu)
                tabControl_ContextMenus.SelectedTab.Focus();
        }

        private void tabControl_ContextMenus_SelectedIndexChanged(object sender, EventArgs e)
        {
            // focus on tab change to allow for scrolling with mouse wheel
            tabControl_ContextMenus.SelectedTab.Focus();

            List<ContextMenuItem> CurrentList = GetContextMenuList();
            if (CurrentList.Count > 0)
                CurrentList[0].GiveItemFocus();
            else if (ContextMenuItemFocused != null)
                ContextMenuItemFocused.RemoveItemFocus();
        }


        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://torrunt.net/vimage");
        }
        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("mailto:me@torrunt.net");
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://openil.sourceforge.net");
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.famfamfam.com/lab/icons/silk/");
        }

        private void linkLabel6_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://github.com/Torrunt/vimage");
        }

        private void linkLabel7_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://www.codeproject.com/articles/36342/exiflib-a-fast-exif-data-extractor-for-net-2-0");
        }

        private void linkLabel8_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://crashreporterdotnet.codeplex.com/");
        }
    }
}
