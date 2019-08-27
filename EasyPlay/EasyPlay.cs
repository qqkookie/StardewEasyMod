using System;

using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace EasyPlay
{
    public class EasyPlay : Mod
    {
        internal static IModHelper ModHelper;
        internal static IModEvents Events;
        internal static ModConfig Config;
        internal static IReflectionHelper Reflection;

        //internal static ITranslationHelper Translation;
        // internal static IMonitor Logger;

        public override void Entry(IModHelper helper)
        {
            ModHelper = helper;
            Events = helper.Events;
            Reflection = helper.Reflection;
            Config = helper.ReadConfig<ModConfig>();

            SkullElevator.Setup();

            LadderFinder.Setup();

            EasyHorse.Setup();

            DontAskToEat.Setup();

        }
    }
}

/*
{
    Translation = helper.Translation;
    string startingMessage = Translation.Get("template.start", new { mod = helper.ModRegistry.ModID, folder = helper.DirectoryPath });
    Monitor.Log(startingMessage, LogLevel.Trace);

    config = helper.ReadConfig<Config>();

    helper.Events.Input.ButtonPressed += Input_ButtonPressed;
}

private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
{
    e.Button.TryGetKeyboard(out Keys keyPressed);


    if (keyPressed.Equals(config.debugKey))
        Monitor.Log(Translation.Get("template.key"), LogLevel.Info);
}
*/

