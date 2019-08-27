using System;
using System.IO;
using Microsoft.Xna.Framework;

using StardewValley;
using StardewValley.Menus;
using StardewModdingAPI;

namespace EasySave
{
    using ModMain = EasySave;

    //  Skip Intro part of EasySpeedTime class
    internal static class SkipIntro
    {
        /// <summary>A game screen.</summary>
        public enum Screen
        {
            Intro,
            Title,          /// <summary>Main title screen.</summary>
            Load,           /// <summary>Load game screen.</summary>
            Host,           /// <summary>Co-op screen on the host tab.</summary>
            Join,           /// <summary>Co-op screen on the join tab.</summary>
            AutoLoad,       /// <summary>Auto-load last loaded single game</summary>
            AutoHost,       /// <summary>Auto-load last loaded multi-player game</summary>
        }
        /// <summary>Where to skip to?</summary>
        internal static Screen SkipTo;

        /// <summary>A step in the mod logic.</summary>
        internal enum Step
        {
            Done,           /// <summary>Done. No action needed.</summary>
            Launching,      /// <summary>Before title screen.</summary>
            Skipping,       /// <summary>Skip to wanted screen</summary>
            Waiting,        /// <summary>The co-op menu is waiting for a connection.</summary>
            AutoLoading,
        }
        /// <summary>Current step</summary>
        internal static Step Current = Step.Launching;

        internal static void SkipIntroUpdateTicked()
        {
            // start intro skip on game launch
            if (Current == Step.Launching)
            {
                // wait until the game is ready
                if (Game1.activeClickableMenu is TitleMenu title && Game1.currentGameTime != null)
                {
                    int[] win = ModMain.Config.TitleWindow;
                    // set title screen resolution
                    if (win.Length >= 2 && win[0] > 1024 && win[1] > 768)
                    {
                        Game1.graphics.PreferredBackBufferWidth = win[0];
                        Game1.graphics.PreferredBackBufferHeight = win[1];
                        Game1.graphics.ApplyChanges();
                        Game1.updateViewportForScreenSizeChange(false, win[0], win[1]);
                    }
                    // set title window position
                    if (win.Length >= 4 && win[2] > 0 && win[3] > 0)
                    {
                        var form = System.Windows.Forms.Control.FromHandle(Program.gamePtr.Window.Handle).FindForm();
                        form.Location = new System.Drawing.Point(win[2], win[3]);
                    }

                    // Don't pause on focus loss until game is fully loaded.
                    Game1.options.pauseWhenOutOfFocus = false;

                    title.skipToTitleButtons();  // skip intro
                    Current = Step.Skipping;
                }
                return;
            }
            else if (Current == Step.Skipping)
            {
                TitleMenu title = (TitleMenu)Game1.activeClickableMenu;

                // skip to other screen
                if (SkipTo == Screen.Title)
                {
                    // skip button transition
                    while (ModMain.Reflection.GetField<int>(title, "buttonsToShow").GetValue() < TitleMenu.numberOfButtons)
                        title.update(Game1.currentGameTime);
                }
                else if (SkipTo == Screen.Load)
                {
                    // skip to load screen
                    title.performButtonAction("Load");
                    while (TitleMenu.subMenu == null)
                        title.update(Game1.currentGameTime);
                }
                else if (SkipTo == Screen.Join || SkipTo == Screen.Host)
                {
                    // skip to co-op screen
                    title.performButtonAction("Co-op");
                    while (TitleMenu.subMenu == null)
                        title.update(Game1.currentGameTime);

                    if (SkipTo == Screen.Host)
                    {
                        Current = Step.Waiting;
                        return;
                    }
                }
                else if (SkipTo == Screen.AutoLoad || SkipTo == Screen.AutoHost)
                {
                    string lastLoaded = ModMain.Config.LastLoadedSave;      // recall last saved name

                    if (!String.IsNullOrEmpty(lastLoaded) && Directory.Exists(Path.Combine(Constants.SavesPath, lastLoaded)))
                    {
                        title.update(Game1.currentGameTime);

                        if (SkipTo == Screen.AutoHost)
                            Game1.multiplayerMode = 2;  // server mode

                        Game1.activeClickableMenu = new AutoLoader(lastLoaded);
                    }
                }
            }
            else if (Current == Step.Waiting)
            {
                // do nothing if a confirmation box is on-screen (e.g. multiplayer disconnect error)
                if (!(TitleMenu.subMenu is ConfirmationDialog)
                    && (SkipTo == Screen.Host && TitleMenu.subMenu is CoopMenu submenu))
                {
                    if (submenu.hostTab == null)   // select host tab
                        return;     // not connected yet

                    submenu.receiveLeftClick(submenu.hostTab.bounds.X, submenu.hostTab.bounds.Y, playSound: false);
                }
            }

            Current = Step.Done;
        }

        // Code for loading saved game
        internal static void SetLastFile(string last)
        {
            ModMain.Config.LastLoadedSave = last;
            ModMain.ModHelper.WriteConfig(ModMain.Config);
        }

        private class AutoLoader : IClickableMenu
        {
            private int WaitCnt = 30;   // Wait until main loop displays "Loading..." message.
            private string SaveName;

            internal AutoLoader(string savefile)
            {
                Game1.gameMode = Game1.loadingMode;     // Trick main loop to diaplay "Loading..." message"
                SaveName = savefile;
            }

            public override void update(GameTime time)
            {
                base.update(time);

                if (WaitCnt == 0)
                {
                    SaveGame.Load(SaveName);  // load last save
                    exitThisMenu(false);
                }
                WaitCnt--;
            }
        }
    }
}
