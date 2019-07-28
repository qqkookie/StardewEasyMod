using System;
using System.IO;

using Microsoft.Xna.Framework.Input;

using StardewValley;
using StardewValley.Menus;
using StardewValley.BellsAndWhistles;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace EasySpeedTime
{
    using ModMain = EasySpeedTime;

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
                    // set title screen resolution
                    var xy = ModMain.Config.TitleResolution;
                    if (xy[0] > 1024 && xy[1] > 768)
                    {
                        Game1.graphics.PreferredBackBufferWidth = xy[0];
                        Game1.graphics.PreferredBackBufferHeight = xy[1];
                        Game1.graphics.ApplyChanges();
                        Game1.updateViewportForScreenSizeChange(false, xy[0], xy[1]);
                    }

                    title.receiveKeyPress(Keys.Escape);  // skip intro
                    Current = Step.Skipping;
                }
                return;
            }
            else if (Current == Step.Skipping)
            {
                TitleMenu title = (TitleMenu)Game1.activeClickableMenu;

                Game1.options.setWindowedOption("Windowed");
                // title.receiveKeyPress(Keys.Escape);


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
                        if (SkipTo == Screen.AutoHost)
                            Game1.multiplayerMode = 2;  // server mode
                        try
                        {
                            SaveGame.Load(lastLoaded);  // load last save

                            // SetLastFile("");
                            title.exitThisMenu(false);
                            Current = Step.AutoLoading;
                            return;
                        }
                        catch { }
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
            else if (Current == Step.AutoLoading)
            {
                // currently not working code.
                /*
                string str2 = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3688") + "... ";
                int widthOfString = SpriteText.getWidthOfString(str2);
                int height = 64;
                Game1.spriteBatch.Begin();
                SpriteText.drawString(Game1.spriteBatch, str2, 100, 100,
                    999999, widthOfString, height, 1f, 0.88f, false, 0, str2, -1);
                Game1.spriteBatch.End();
                return;
                */
            }

            Current = Step.Done;
        }

        // Code for loading saved game
        private static void SetLastFile(string last)
        {
            ModMain.Config.LastLoadedSave = last;
            ModMain.ModHelper.WriteConfig(ModMain.Config);
        }

        internal static void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            SetLastFile(Constants.SaveFolderName);
        }

        internal static void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            if (ModMain.Config.ForgetLastOnTitle)
                SetLastFile("");
        }
    }
}